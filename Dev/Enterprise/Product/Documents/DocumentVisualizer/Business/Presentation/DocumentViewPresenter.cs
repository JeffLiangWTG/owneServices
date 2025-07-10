using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class DocumentViewPresenter : IDisposable
	{
		public DocumentViewPresenter(IDocumentView view, IDocumentInfo documentInfo)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.documentInfo = documentInfo ?? throw new ArgumentNullException(nameof(documentInfo));
			#pragma warning disable CA2208 // Instantiate argument exceptions correctly
			// Suppress because 'services' will not match the name of the parameter in this method signature
			this.services = documentInfo.Services ?? throw new ArgumentNullException(nameof(services));
			#pragma warning restore CA2208 // Instantiate argument exceptions correctly
			this.broker = services.Resolve<IEventBroker>();

			var settingsProvider = new DocumentSettingsProviderService(() => scale);
			services.Register<IDocumentSettingsProviderService>(() => settingsProvider);
		}

		readonly IDocumentView view;
		readonly IDocumentInfo documentInfo;
		readonly IServiceContainer services;
		readonly IEventBroker broker;

		IDisposable[] eventSubscriptions;

		readonly EditableElementsIndexer indexer = new EditableElementsIndexer();

		float scale = 1f;

		#region Events

		void Refresh()
		{
			view.Refresh();
		}

		public void NotifyZoomChanged(int zoom)
		{
			var calcScale = Math.Max(20, Math.Min(zoom, 200)) / 100f;

			if (Math.Abs(scale - calcScale) > 0f)
			{
				scale = calcScale;
				view.Scale(scale);
			}
		}

		void OnBeforeEdit(BeforeEditEvent eventData)
		{
			if (elementBeingEdited != null)
			{
				broker.Publish(new EndEditEvent(documentInfo.Document, elementBeingEdited));
				elementBeingEdited = null;
			}
		}

		Action endEditSubscription;
		IDynamicContentLayoutElement elementBeingEdited;

		void OnBeginEdit(BeginEditEvent eventData)
		{
			if (elementBeingEdited != null)
			{
				ErrorReporter.ReportOnce("It is invalid to start editing an element before current edit has been finished.");
				return;
			}

			elementBeingEdited = eventData?.Content;
		}

		void OnReset(IDocument document)
		{
			OnValueChanged(document);
		}

		void OnValueChanged(IDocument document)
		{
			if (endEditSubscription != null)
			{
				endEditSubscription();
				endEditSubscription = null;
			}

			elementBeingEdited = null;

			document?.Data?.Validate();
			broker.Publish(new DataValidatedEvent(documentInfo.Document));

			Refresh();
		}

		void OnMoveToNextEditableElement(MoveToNextEditableElementEvent eventData)
		{
			var next = eventData.Direction == MoveToNextDirection.Forward
				? indexer.GetNext(eventData.Content)
				: indexer.GetPrevious(eventData.Content);

			if (next?.PageView != null)
			{
				var presenter = next.PageView.Presenter;

				var trigger = eventData.Direction == MoveToNextDirection.Forward
					? EditTrigger.TabForward
					: EditTrigger.TabBackwards;

				presenter.BeginEdit(next, trigger);
			}
		}

		void OnDataValidated(IDocument document)
		{
			CreateOrUpdateDocumentNotificationView(document);
		}

		#endregion

		#region CreateView

		public void CreateView()
		{
			if (!(documentInfo?.Document is IDocument document))
			{
				return;
			}

			BuildUI(document);

			eventSubscriptions?.ForEach(s => s.Dispose());

			eventSubscriptions = new[]
			{
				broker.GetEvent<BeforeEditEvent>().Subscribe(OnBeforeEdit, document),
				broker.GetEvent<BeginEditEvent>().Subscribe(OnBeginEdit, document),
				broker.GetEvent<MoveToNextEditableElementEvent>().Subscribe(OnMoveToNextEditableElement, document),
				broker.GetEvent<ResetEvent>().Subscribe(_ => OnReset(document), document),
				broker.GetEvent<ValueChangedEvent>().Subscribe(_ => OnValueChanged(document), document),
				broker.GetEvent<DataValidatedEvent>().Subscribe(_ => OnDataValidated(document), document)
			};
		}

		void BuildUI(IDocument document)
		{
			var isReadOnly = !services.Resolve<IDocumentSecurityService>().CanModify;

			using (PerformanceStatistics.StartMonitoring(PerformanceMonitorArea.CreatePagesUI))
			{
				if (document.Pages.Any())
				{
					CreatePages(isReadOnly, document);
				}
				else
				{
					view.ShowWatermark(Res.GetString("caca13c0-1b98-45f4-b149-d7883352cac9", "There's nothing to display."));
				}

				CreateOrUpdateDocumentNotificationView(document);
			}

			broker.Publish(new DocumentViewCreatedEvent(document));
		}

		void CreatePages(bool isDocumentReadOnly, IDocument document)
		{
			view.PageViews.Clear();

			var pageBuilder = services.Resolve<IPageViewBuildService>();

			foreach (var page in document.Pages)
			{
				var pageViewPresenter = new PageViewPresenter(document, services);

				var pageView = pageBuilder.CreatePageView(page, pageViewPresenter, isDocumentReadOnly);

				indexer.Index(pageView);

				view.PageViews.Add(pageView);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		void CreateOrUpdateDocumentNotificationView(IDocument document)
		{
			const string documentNotificationViewId = "document";

			var notificationView = view
				.NotificationViews
				.FirstOrDefault(nv => string.CompareOrdinal(nv.Id, documentNotificationViewId) == 0);

			var notificationType = document
				.GetMostSevereNotificationType()
				.GetValueOrDefault();

			if (notificationView == null)
			{
				var builder = services.Resolve<INotificationViewBuildService>();

				notificationView = builder.CreateNotificationView(
					documentNotificationViewId,
					Res.GetString("4f46dff9-9e10-4133-8012-24a57bdcae67", "This document has pending notifications. Click here to view them."),
					notificationType,
					() => services.Resolve<IUserNotificationService>().ShowNotifications(document.Notifications));

				view.NotificationViews.Add(notificationView);
			}
			else
			{
				notificationView.NotificationType = notificationType;
			}

			notificationView.Visible = document.Notifications.Any();
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
			indexer.Dispose();
			eventSubscriptions?.ForEach(s => s.Dispose());
		}

		#endregion
	}
}
