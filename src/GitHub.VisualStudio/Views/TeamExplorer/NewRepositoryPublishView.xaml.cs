﻿using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.ComponentModel.Composition;
using GitHub.Extensions.Reactive;
using GitHub.Services;
using GitHub.ViewModels.TeamExplorer;

namespace GitHub.VisualStudio.Views.TeamExplorer
{
    public class GenericNewRepositoryPublishView : NewViewBase<INewRepositoryPublishViewModel, NewRepositoryPublishView>
    { }
    
    [ExportViewFor(typeof(INewRepositoryPublishViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewRepositoryPublishView : GenericNewRepositoryPublishView
    {
        [ImportingConstructor]
        public NewRepositoryPublishView(ITeamExplorerServices teServices, INotificationDispatcher notifications)
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.PublishRepository, v => v.publishRepositoryButton));

                ViewModel.PublishRepository.Subscribe(state =>
                {
                    if (state == ProgressState.Success)
                    {
                        teServices.ShowMessage(UI.Resources.RepositoryPublishedMessage);
                    }
                });

                d(notifications.Listen()
                    .Where(n => n.Type == Notification.NotificationType.Error)
                    .Subscribe(n => teServices.ShowError(n.Message)));

                d(this.WhenAny(x => x.ViewModel.SafeRepositoryNameWarningValidator.ValidationResult, x => x.Value)
                    .WhereNotNull()
                    .Select(result => result?.Message)
                    .Subscribe(message =>
                    {
                        if (!String.IsNullOrEmpty(message))
                            teServices.ShowWarning(message);
                        else
                            teServices.ClearNotifications();
                    }));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
