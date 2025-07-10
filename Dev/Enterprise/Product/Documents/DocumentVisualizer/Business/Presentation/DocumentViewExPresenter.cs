using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class DocumentViewExPresenter : IDisposable
	{
		public DocumentViewExPresenter(IDocumentViewEx view, IDocumentInfo info)
		{
			Argument.NotNull(view, nameof(view));
			Argument.NotNull(info, nameof(info));

			this.view = view;
			this.document = info.Document;
			this.descriptor = info.Descriptor;
			this.services = info.Services;
			this.broker = services.Resolve<IEventBroker>();

			disposables = HookUpEvents().ToList();
		}

		readonly IDocumentViewEx view;
		readonly IDocument document;
		readonly IDocumentDescriptor descriptor;
		readonly IEventBroker broker;
		readonly IServiceContainer services;
		readonly List<IDisposable> disposables;

		IEnumerable<IDisposable> HookUpEvents()
		{
			yield return broker.GetEvent<DocumentBuildErrorEvent>().Subscribe(OnDocumentBuildError);
			yield return broker.GetEvent<DocumentViewCreatedEvent>().Subscribe(OnDocumentViewCreated, document);
			yield return broker.GetEvent<DataValidatedEvent>().Subscribe(_ => RefreshMenuAndFocusView(), document);
			yield return broker.GetEvent<MessageSentEvent>().Subscribe(_ => RefreshMenuAndFocusView(), document);
			yield return broker.GetEvent<MessageWithdrawalSentEvent>().Subscribe(_ => RefreshMenuAndFocusView(), document);
			yield return broker.GetEvent<ResetToOriginalEvent>().Subscribe(_ => RefreshMenuAndFocusView(), document);
			yield return broker.GetEvent<ExitEvent>().Subscribe(_ => view.Exit(), document);
			yield return broker.GetEvent<SavedEvent>().Subscribe(_ => view.MenuItems.Refresh(), document);
		}

		#region Public

		public void NotifyMouseDown()
		{
			broker.Publish(new EndEditEvent(document));
		}

		public void NotifyExiting()
		{
			if (document?.Data.HasChanges == null
				|| !document.Data.HasChanges)
			{
				return;
			}

			var saveChanges = services
				.Resolve<IUserNotificationService>()
				.ShowConfirmation(
					Res.GetString("8fbde9de-e3b2-40d3-8f57-6721abdece95", "{0} has changes. Would you like to save your changes?", document.Name),
					Res.GetString("624fb8d7-c7e1-44ec-9d9f-def12998e09e", "Exiting - {0}", document.Name));

			if (saveChanges)
			{
				var saver = new DocumentDataSaver(
					services,
					document,
					descriptor.DocumentData);

				saver.Save();
			}
		}

		#endregion

		#region Events

		void RefreshMenuAndFocusView()
		{
			view.MenuItems.Refresh();
			view.Focus();
		}

		void OnDocumentViewCreated(DocumentViewCreatedEvent args)
		{
			if (args.Document != document)
			{
				return;
			}

			view.Text = string.IsNullOrEmpty(descriptor.MessageInstructions?.TranslatedDocumentName) ?
				(document.Name ?? Res.GetString("c13bfdd7-6f34-4336-b110-daa058a82b91", "Document")) :
				descriptor.MessageInstructions.TranslatedDocumentName;

			foreach (var menuItem in descriptor.DisplayInstructions.MenuItems)
			{
				view.MenuItems.Add(menuItem);
			}

			if (descriptor.DocumentData is IStmALogParent logParent)
			{
				var messagingExtensions = descriptor.DocumentData.Parent?.GetSupporter()?.GetMessagingExtensions(document, descriptor.MessageInstructions);
				var displayInstructions = descriptor.DisplayInstructions;

				if (messagingExtensions?.ShowEvents() ?? displayInstructions.ShowEvents)
				{
					view.ShowLogsView(logParent);
				}

				if (messagingExtensions?.ShowLastEventDetails() ?? displayInstructions.ShowLastEventDetails)
				{
					SetupStatusPanel(logParent);
				}

				void LogCountChangedHandler(object s, CollectionCountChangedEventArgs e) => OnLogsCountChangedRefreshView(e);

				logParent.Logs.GetAllLogs().CountChanged += LogCountChangedHandler;
				disposables.Add(new DisposableAction(() => logParent.Logs.GetAllLogs().CountChanged -= LogCountChangedHandler));

				logParent.HasChangesChanged += OnLogsHasChangesChanged;
				disposables.Add(new DisposableAction(() => logParent.HasChangesChanged -= OnLogsHasChangesChanged));
			}
		}

		void SetupStatusPanel(IStmALogParent logParent)
		{
			view.ToggleStatusPanelVisibility(true);

			var messageStatus = logParent.GetCurrentMessageStatus(document, descriptor.MessageInstructions);
			view.SetStatusPanelCaption(messageStatus);

			void CountChangeHandler(object s, CollectionCountChangedEventArgs e) => OnLogsCountChangedResetCaption(e, logParent);

			logParent.Logs.GetAllLogs().CountChanged += CountChangeHandler;
			disposables.Add(new DisposableAction(() => logParent.Logs.GetAllLogs().CountChanged -= CountChangeHandler));
		}

		void OnLogsHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			view.MenuItems.Refresh();
		}

		void OnLogsCountChangedRefreshView(CollectionCountChangedEventArgs collectionCountChangedEventArgs)
		{
			var log = collectionCountChangedEventArgs.BizObject as StmALog;

			if (log != null
				&& collectionCountChangedEventArgs.ItemAdded
				&& MessageExtensions.IsValidMessageEvent(descriptor?.MessageInstructions?.DocumentName, log))
			{
				view.Refresh();
			}
		}

		void OnLogsCountChangedResetCaption(CollectionCountChangedEventArgs collectionCountChangedEventArgs, IStmALogParent logParent)
		{
			var log = collectionCountChangedEventArgs.BizObject as StmALog;

			if (log != null
				&& collectionCountChangedEventArgs.ItemAdded
				&& MessageExtensions.IsValidMessageEvent(descriptor?.MessageInstructions?.DocumentName, log))
			{
				view.SetStatusPanelCaption(logParent.GetCurrentMessageStatus(document, descriptor.MessageInstructions));
			}
		}

		void OnDocumentBuildError(DocumentBuildErrorEvent args)
		{
			view.MenuItems.Refresh();
			view.ShowWatermark(Res.GetString("26eca637-45f4-4fe4-99d5-41e3d68088f8", "There has been a problem creating the document. Please see notifications for details."));
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
			disposables.ForEach(subscription => subscription.Dispose());
		}

		#endregion
	}
}
