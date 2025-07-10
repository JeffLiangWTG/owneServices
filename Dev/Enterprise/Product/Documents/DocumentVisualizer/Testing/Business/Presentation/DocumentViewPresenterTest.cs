using System;
using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing.Dummies;
using Enterprise.DocumentVisualizer.Testing.GUI;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentViewPresenterTest : TestCaseWithUXmlSupport
	{
		#region TestBuildUI

		public void TestBuildUI()
		{
			var document = new DummyDocument
			{
				Name = "test doc",
			};

			var page1 = new DummyPage();
			var page2 = new DummyPage();

			document.Pages.Add(page1);
			document.Pages.Add(page2);

			var documentInfo = new Mock<IDocumentInfo>();
			documentInfo.SetupGet(di => di.Document).Returns(document);

			var view = new Mock<IDocumentView>();
			var documentSecurityService = new Mock<IDocumentSecurityService>();
			var notificationViewBuildService = new Mock<INotificationViewBuildService>();

			var pageBuildService = new Mock<IPageViewBuildService>();
			var pageViews = new DummyPageViewCollection();
			var pageView1 = new DummyPageView();
			var pageView2 = new DummyPageView();

			var notificationViews = new DummyNotificationViews();
			var notificationView = new DummyNotificationView();

			documentSecurityService.Setup(s => s.CanModify).Returns(true);

			view.Setup(v => v.NotificationViews).Returns(notificationViews);

			notificationViewBuildService.Setup(s => s.CreateNotificationView(
				It.Is<string>(x => x == "document"),
				It.Is<string>(x => x == "This document has pending notifications. Click here to view them."),
				It.Is<Core.NotificationType>(x => x == Core.NotificationType.None),
				It.IsNotNull<Action>()))
				.Returns(notificationView);

			pageBuildService.Setup(v => v.CreatePageView(
				It.Is<IPage>(x => x == page1),
				It.IsNotNull<IPageViewPresenter>(),
				It.Is<bool>(x => !x)))
				.Returns(pageView1);

			pageBuildService.Setup(v => v.CreatePageView(
				It.Is<IPage>(x => x == page2),
				It.IsNotNull<IPageViewPresenter>(),
				It.Is<bool>(x => !x)))
				.Returns(pageView2);

			view.Setup(v => v.PageViews).Returns(pageViews);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);
			services.Register<IDocumentSecurityService>(() => documentSecurityService.Object);
			services.Register<IPageViewBuildService>(() => pageBuildService.Object);
			services.Register<INotificationViewBuildService>(() => notificationViewBuildService.Object);

			documentInfo.SetupGet(di => di.Services).Returns(services);

			var presenter = new DocumentViewPresenter(view.Object, documentInfo.Object);
			presenter.CreateView();

			AssertContainsExactElementsInAnyOrder("both page views have been added to the collection",
				new[] { pageView1, pageView2 },
				pageViews);

			AssertEquals("notificationView was added to the view collection",
				true, notificationViews.Contains(notificationView));

			documentSecurityService.Verify(s => s.CanModify, Times.Once);

			view.Verify(v => v.NotificationViews, Times.Exactly(2));

			notificationViewBuildService.Verify(s => s.CreateNotificationView(
				It.Is<string>(x => x == "document"),
				It.Is<string>(x => x == "This document has pending notifications. Click here to view them."),
				It.Is<Core.NotificationType>(x => x == Core.NotificationType.None),
				It.IsNotNull<Action>()), Times.Once);

			pageBuildService.Verify(v => v.CreatePageView(
				It.Is<IPage>(x => x == page1),
				It.IsNotNull<IPageViewPresenter>(),
				It.Is<bool>(x => !x)), Times.Once);

			pageBuildService.Verify(v => v.CreatePageView(
				It.Is<IPage>(x => x == page2),
				It.IsNotNull<IPageViewPresenter>(),
				It.Is<bool>(x => !x)), Times.Once);
		}

		#endregion

		#region TestBuildUI_NoPages

		public void TestBuildUI_NoPages()
		{
			var document = new DummyDocument
			{
				Name = "test doc"
			};

			var documentInfo = new Mock<IDocumentInfo>();
			documentInfo.SetupGet(di => di.Document).Returns(document);

			var view = new Mock<IDocumentView>();
			var documentSecurityService = new Mock<IDocumentSecurityService>();

			var notificationViewBuildService = new Mock<INotificationViewBuildService>();
			var notificationViews = new DummyNotificationViews();
			var notificationView = new DummyNotificationView();

			documentSecurityService.Setup(s => s.CanModify).Returns(true);

			view.Setup(v => v.NotificationViews).Returns(notificationViews);

			notificationViewBuildService.Setup(s => s.CreateNotificationView(
				It.Is<string>(x => x == "document"),
				It.Is<string>(x => x == "This document has pending notifications. Click here to view them."),
				It.Is<Core.NotificationType>(x => x == Core.NotificationType.None),
				It.IsNotNull<Action>()))
				.Returns(notificationView);

			view.Setup(v => v.ShowWatermark(It.Is<string>(x => x == "There's nothing to display.")));

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);
			services.Register<IDocumentSecurityService>(() => documentSecurityService.Object);
			services.Register<INotificationViewBuildService>(() => notificationViewBuildService.Object);

			documentInfo.SetupGet(di => di.Services).Returns(services);

			var presenter = new DocumentViewPresenter(view.Object, documentInfo.Object);
			presenter.CreateView();

			AssertEquals("notificationView was added to the view collection",
				true, notificationViews.Contains(notificationView));

			documentSecurityService.Verify(s => s.CanModify, Times.Once);
			view.Verify(v => v.NotificationViews, Times.Exactly(2));
			notificationViewBuildService.Verify(s => s.CreateNotificationView(
				It.Is<string>(x => x == "document"),
				It.Is<string>(x => x == "This document has pending notifications. Click here to view them."),
				It.Is<Core.NotificationType>(x => x == Core.NotificationType.None),
				It.IsNotNull<Action>()), Times.Once);
			view.Verify(v => v.ShowWatermark(It.Is<string>(x => x == "There's nothing to display.")), Times.Once);
		}

		#endregion

		#region TestValidateAfterEdit

		public void TestValidateAfterEdit()
		{
			var document = new DummyDocument
			{
				Name = "test doc",
				Pages = { new DummyPage() }
			};

			var documentInfo = new Mock<IDocumentInfo>();
			documentInfo.SetupGet(di => di.Document).Returns(document);

			var view = new Mock<IDocumentView>();
			var documentSecurityService = new Mock<IDocumentSecurityService>();
			var notificationViewBuildService = new Mock<INotificationViewBuildService>();

			var pageBuildService = new Mock<IPageViewBuildService>();
			var pageViews = new DummyPageViewCollection();
			var pageView = new DummyPageView();

			var notificationViews = new DummyNotificationViews();
			var notificationView = new DummyNotificationView();

			documentSecurityService.Setup(s => s.CanModify).Returns(true);

			view.Setup(v => v.NotificationViews).Returns(notificationViews);

			notificationViewBuildService.Setup(s => s.CreateNotificationView(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<Core.NotificationType>(),
				It.IsAny<Action>()))
				.Returns(notificationView);

			pageBuildService.Setup(v => v.CreatePageView(
				It.IsAny<IPage>(),
				It.IsAny<IPageViewPresenter>(),
				It.IsAny<bool>()))
				.Returns(pageView);

			view.Setup(v => v.PageViews).Returns(pageViews);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);
			services.Register<IDocumentSecurityService>(() => documentSecurityService.Object);
			services.Register<IPageViewBuildService>(() => pageBuildService.Object);
			services.Register<INotificationViewBuildService>(() => notificationViewBuildService.Object);

			documentInfo.SetupGet(di => di.Services).Returns(services);

			var validatedFired = false;
			broker.GetEvent<DataValidatedEvent>().Subscribe(data => { validatedFired = true; });

			var presenter = new DocumentViewPresenter(view.Object, documentInfo.Object);
			presenter.CreateView();

			broker.Publish(new DataValidatedEvent(document));

			AssertEquals("validated event fired", true, validatedFired);
		}

		#endregion

		#region TestRefreshViewAfterZoomChanged

		public void TestRefreshViewAfterZoomChanged()
		{
			var view = new Mock<IDocumentView>();
			var documentInfo = new Mock<IDocumentInfo>();

			view.Setup(v => v.Scale(It.Is<float>(x => x == 0.66f)));

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			documentInfo.SetupGet(di => di.Services).Returns(services);

			var presenter = new DocumentViewPresenter(view.Object, documentInfo.Object);
			presenter.NotifyZoomChanged(66);
			view.Verify(v => v.Scale(It.Is<float>(x => x == 0.66f)), Times.Once);

			var documentSettingsProvider = services.Resolve<IDocumentSettingsProviderService>();

			AssertEquals("Settings provider returs correct scale", 0.66f, documentSettingsProvider.Scale);
		}

		#endregion

		#region TestCallEndEditEventOnBeforeElement

		public void TestCallEndEditEventOnBeforeElement()
		{
			var document = new DummyDocument
			{
				Name = "Test Document",
				Pages = { new DummyPage() }
			};

			var documentInfo = new Mock<IDocumentInfo>();
			documentInfo.SetupGet(di => di.Document).Returns(document);

			var view = new Mock<IDocumentView>();
			var documentSecurityService = new Mock<IDocumentSecurityService>();
			var notificationViewBuildService = new Mock<INotificationViewBuildService>();

			var pageBuildService = new Mock<IPageViewBuildService>();
			var pageViews = new DummyPageViewCollection();
			var pageView = new DummyPageView();

			var notificationViews = new DummyNotificationViews();
			var notificationView = new DummyNotificationView();

			documentSecurityService.Setup(s => s.CanModify).Returns(true);

			view.Setup(v => v.NotificationViews).Returns(notificationViews);

			notificationViewBuildService.Setup(s => s.CreateNotificationView(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Core.NotificationType>(),
					It.IsAny<Action>()))
				.Returns(notificationView);

			pageBuildService.Setup(v => v.CreatePageView(
					It.IsAny<IPage>(),
					It.IsAny<IPageViewPresenter>(),
					It.IsAny<bool>()))
				.Returns(pageView);

			view.Setup(v => v.PageViews).Returns(pageViews);

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);
			services.Register<IDocumentSecurityService>(() => documentSecurityService.Object);
			services.Register<IPageViewBuildService>(() => pageBuildService.Object);
			services.Register<INotificationViewBuildService>(() => notificationViewBuildService.Object);

			documentInfo.SetupGet(di => di.Services).Returns(services);

			var presenter = new DocumentViewPresenter(view.Object, documentInfo.Object);
			presenter.CreateView();

			var eventPublished = false;

			var content = new Mock<IDynamicContentLayoutElement>();

			broker.Publish(new BeginEditEvent(document, content.Object));
			broker.GetEvent<EndEditEvent>().Subscribe(e => { eventPublished = (eventPublished || e.Content == content.Object); });

			var newContent = new Mock<IDynamicContentLayoutElement>();
			broker.Publish(new BeforeEditEvent(document, newContent.Object));

			AssertEquals(true, eventPublished);
		}

		#endregion
	}
}
