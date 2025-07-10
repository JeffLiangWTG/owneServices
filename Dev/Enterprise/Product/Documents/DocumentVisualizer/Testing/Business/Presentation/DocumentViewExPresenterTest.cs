using System;
using System.Collections;
using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentViewExPresenterTest : TestCaseWithUXmlSupport
	{
		#region TestShowEventsTabAndStatusPanel

		public void TestShowEventsTabAndStatusPanel()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new Mock<IDocument>();
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = true,
					ShowLastEventDetails = true,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			documentInfo.Setup(i => i.Document).Returns(document.Object);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);

			if (documentData is IEnumerable)
			{
				var www = ((IEnumerable)documentData).GetEnumerator();
			}

			view.Setup(v => v.ShowLogsView(It.Is<IStmALogParent>(x => x.LogsParentPK == documentData.PK)));
			view.Setup(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)));
			view.Setup(v => v.SetStatusPanelCaption(It.IsAny<string>()));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document.Object));

			view.Verify(viewPara => viewPara.MenuItems, Times.Never);
			view.Verify(v => v.ShowLogsView(It.Is<IStmALogParent>(x => x.LogsParentPK == documentData.PK)), Times.Once);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.IsAny<string>()), Times.Once);

			Assert(true);
		}

		#endregion

		#region TestDoNotShowEventsTabAndStatusPanel

		public void TestDoNotShowEventsTabAndStatusPanel()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new Mock<IDocument>();
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = false,
					ShowLastEventDetails = false,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false,
					DocumentName = "test doc"
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			documentInfo.Setup(i => i.Document).Returns(document.Object);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);
			//view.Setup(v => v.ShowLogsView(It.IsAny<IStmALogParent>()));
			view.Setup(v => v.ToggleStatusPanelVisibility(It.IsAny<bool>()));
			view.Setup(v => v.SetStatusPanelCaption(It.IsAny<string>()));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document.Object));

			view.Verify(viewPara => viewPara.MenuItems, Times.Never);
			//view.Verify(v => v.ShowLogsView(It.IsAny<IStmALogParent>()), Times.Never);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.IsAny<bool>()), Times.Never);
			view.Verify(v => v.SetStatusPanelCaption(It.IsAny<string>()), Times.Never);

			Assert(true);
		}

		#endregion

		#region TestEventsTabAndStatusPanel_MessagingExtensions

		public void TestShowEventsTabAndStatusPanel_MessagingExtensions()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new Mock<IDocument>();
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();
			var supporter = new Mock<IVisualizableDocumentSupporter>();
			var messagingExtensions = new Mock<IMessagingExtensions>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = false,
					ShowLastEventDetails = false,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false,
					DocumentName = "test doc"
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			messagingExtensions
				.Setup(me => me.ShowEvents())
				.Returns(true);

			messagingExtensions
				.Setup(me => me.ShowLastEventDetails())
				.Returns(true);

			var dummyWithUXmlSupport = Factory.New<DummyWithUXmlSupport>();
			documentData.Parent = dummyWithUXmlSupport;
			dummyWithUXmlSupport.MessagingExtensions = messagingExtensions.Object;

			documentInfo.Setup(i => i.Document).Returns(document.Object);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);
			view.Setup(v => v.ToggleStatusPanelVisibility(It.IsAny<bool>()));
			view.Setup(v => v.SetStatusPanelCaption(It.IsAny<string>()));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document.Object));

			view.Verify(viewPara => viewPara.MenuItems, Times.Never);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.IsAny<bool>()), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.IsAny<string>()), Times.Once);

			Assert(true);
		}

		public void TestDoNotShowEventsTabAndStatusPanel_MessagingExtensions()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new Mock<IDocument>();
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();
			var supporter = new Mock<IVisualizableDocumentSupporter>();
			var messagingExtensions = new Mock<IMessagingExtensions>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = true,
					ShowLastEventDetails = true,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			messagingExtensions
				.Setup(me => me.ShowEvents())
				.Returns(false);

			messagingExtensions
				.Setup(me => me.ShowLastEventDetails())
				.Returns(false);

			var dummyWithUXmlSupport = Factory.New<DummyWithUXmlSupport>();
			documentData.Parent = dummyWithUXmlSupport;
			dummyWithUXmlSupport.MessagingExtensions = messagingExtensions.Object;

			documentInfo.Setup(i => i.Document).Returns(document.Object);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);

			if (documentData is IEnumerable)
			{
				var www = ((IEnumerable)documentData).GetEnumerator();
			}

			view.Setup(v => v.ShowLogsView(It.Is<IStmALogParent>(x => x.LogsParentPK == documentData.PK)));
			view.Setup(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)));
			view.Setup(v => v.SetStatusPanelCaption(It.IsAny<string>()));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document.Object));

			view.Verify(viewPara => viewPara.MenuItems, Times.Never);
			view.Verify(v => v.ShowLogsView(It.Is<IStmALogParent>(x => x.LogsParentPK == documentData.PK)), Times.Never);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)), Times.Never);
			view.Verify(v => v.SetStatusPanelCaption(It.IsAny<string>()), Times.Never);

			Assert(true);
		}

		#endregion

		#region TestRefreshStatusPanel

		public void TestRefreshStatusPanel()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new DummyDocument
			{
				Name = "test doc"
			};
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = true,
					ShowLastEventDetails = true,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false,
					DocumentName = "test doc"
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			documentInfo.Setup(i => i.Document).Returns(document);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);
			view.Setup(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)));

			view.Setup(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "No test doc Messages Have Been Sent.")));
			view.Setup(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "Status Updated from test doc")));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document));

			documentData.Logs.GetAllLogs().AddNew(AutoEvents.StatusUpdated, "|MST=test doc", DateTime.Now, false);

			view.Verify(viewPara => viewPara.MenuItems, Times.Once);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "No test doc Messages Have Been Sent.")), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "Status Updated from test doc")), Times.Once);

			Assert(true);
		}

		public void TestStatusPanel_TranslatedDocumentName()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new DummyDocument
			{
				Name = "test doc"
			};
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = true,
					ShowLastEventDetails = true,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(new DummyMessageInstructions
				{
					OrderLogsByLocalTime = false,
					DocumentName = "test doc",
					TranslatedDocumentName = "translated doc"
				});

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			documentInfo.Setup(i => i.Document).Returns(document);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);
			view.Setup(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document));

			documentData.Logs.GetAllLogs().AddNew(AutoEvents.StatusUpdated, "|MST=test doc", DateTime.Now, false);

			view.Verify(viewPara => viewPara.MenuItems, Times.Once);
			view.Verify(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "No translated doc Messages Have Been Sent.")), Times.Once);
			view.Verify(v => v.SetStatusPanelCaption(It.Is<string>(x => x == "Status Updated from translated doc")), Times.Once);

			Assert(true);
		}

		#endregion

		#region TestText

		public void TestText()
		{
			var view = new Mock<IDocumentViewEx>();
			var menuCollection = new Mock<IMenuItemCollection>();
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new DummyDocument
			{
				Name = "test doc"
			};
			var documentData = Factory.New<VisualizerDocumentData>();
			var documentInfo = new Mock<IDocumentInfo>();

			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(() => broker);

			descriptor
				.Setup(d => d.DisplayInstructions)
				.Returns(new DummyDisplayInstructions
				{
					ShowEvents = true,
					ShowLastEventDetails = true,
					MenuItems = Enumerable.Empty<IMenuItemDescriptor>()
				});

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false,
				DocumentName = "test doc",
				TranslatedDocumentName = null
			};

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(messageInstructions);

			descriptor
				.Setup(d => d.DocumentData)
				.Returns(documentData);

			documentInfo.Setup(i => i.Document).Returns(document);
			documentInfo.Setup(i => i.DocumentData).Returns(documentData);
			documentInfo.Setup(i => i.Descriptor).Returns(descriptor.Object);
			documentInfo.Setup(i => i.Services).Returns(services);

			var text = string.Empty;

			view.SetupSet(viewPara => viewPara.Text = It.IsAny<string>()).Callback<string>(x => text = x);
			view.Setup(viewPara => viewPara.MenuItems).Returns(menuCollection.Object);
			view.Setup(v => v.ToggleStatusPanelVisibility(It.Is<bool>(x => x)));

			var presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document));

			AssertEquals(text, "test doc");

			messageInstructions.TranslatedDocumentName = "translated doc";

			presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document));

			AssertEquals(text, "translated doc");

			messageInstructions.TranslatedDocumentName = string.Empty;

			presenter = new DocumentViewExPresenter(view.Object, documentInfo.Object);
			broker.Publish(new DocumentViewCreatedEvent(document));

			AssertEquals(text, "test doc");
		}

		#endregion

	}
}
