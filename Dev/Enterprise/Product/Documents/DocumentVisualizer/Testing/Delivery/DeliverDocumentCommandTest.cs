using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Notification = Enterprise.DocumentVisualizer.Core.Notification;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DeliverDocumentCommandTest : TestCaseWithUXmlSupport
	{
		public void TestCommandInfo()
		{
			var dummy = Factory.New<DummyWithUXmlSupport>();
			var deliverCommand = new DeliverDocumentCommand(dummy);

			CombineAssertions(() =>
			{
				AssertEquals("Id", CommandIds.DeliverDocument, deliverCommand.Id);
				AssertEquals("Caption", "Deliver Document", deliverCommand.Caption);
				Assert("IsEnabled", !deliverCommand.IsEnabled);
				Assert("IsVisible", deliverCommand.IsVisible);
			});

			var documentInfo = new Mock<IDocumentInfo>();
			((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(documentInfo.Object);

			Assert("Enabled when there is at least one document info", deliverCommand.IsEnabled);
			Assert("Always visible", deliverCommand.IsVisible);
		}

		public void TestDeliver()
		{
			using (DataContextManagersSubstitution())
			{
				var (dummy, info, _, securityService, _, deliveryService) = CreateDocumentInfos();
				securityService.Setup(s => s.CanDeliver).Returns(true);

				var deliverCommand = new DeliverDocumentCommand(dummy);
				((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

				Assert("Deliver successfully", deliverCommand.Invoke());
				deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Once);
			}
		}

		public void TestDeliverWithSecurityError()
		{
			using (DataContextManagersSubstitution())
			{
				var (dummy, info, _, securityService, _, deliveryService) = CreateDocumentInfos();
				securityService.Setup(s => s.CanDeliver).Returns(false);

				var deliverCommand = new DeliverDocumentCommand(dummy);
				((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

				Assert("Deliver failed", !deliverCommand.Invoke());
				securityService.Verify(s => s.ShowDeliveryError(), Times.Once);
				deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Never);
			}
		}

		public void TestDeliverWithUnsavedChanges()
		{
			using (DataContextManagersSubstitution())
			{
				var (dummy, info, document, securityService, notificationService, deliveryService) = CreateDocumentInfos();
				securityService.Setup(s => s.CanDeliver).Returns(true);
				document.Object.Data.Properties.GetOrCreate("Name").SetValue("Wall");

				var deliverCommand = new DeliverDocumentCommand(dummy);
				((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

				Assert("Deliver failed", !deliverCommand.Invoke());
				notificationService.Verify(s => s.ShowMessage("Please save changes before delivering.", "Document Delivery"), Times.Once);
				deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Never);
			}
		}

		public void TestDeliverWithDocumentErrors()
		{
			using (DataContextManagersSubstitution())
			{
				var (dummy, info, document, securityService, notificationService, deliveryService) = CreateDocumentInfos();
				securityService.Setup(s => s.CanDeliver).Returns(true);
				document.Setup(d => d.Notifications).Returns(new[] { new Notification(new DummyNotificationSource(), NotificationType.Error, "Test Error") });

				var deliverCommand = new DeliverDocumentCommand(dummy);
				((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

				Assert("Deliver failed", !deliverCommand.Invoke());
				notificationService.Verify(s => s.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
				deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Never);
			}
		}

		public void TestDeliverWithDeliveryErrors()
		{
			using (DataContextManagersSubstitution())
			{
				var (dummy, info, document, securityService, notificationService, deliveryService) = CreateDocumentInfos();
				document.Setup(d => d.Notifications).Returns(new[] { new Notification(new DummyNotificationSource(), NotificationType.DeliveryError, "Test Delivery Error") });
				securityService.Setup(s => s.CanDeliver).Returns(true);

				var deliverCommand = new DeliverDocumentCommand(dummy);
				((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

				AssertNoExceptionThrown(() => deliverCommand.Invoke());
				notificationService.Verify(s => s.ShowMessage("This document contains delivery errors. Please fix all message errors before delivering.", "Document Delivery"), Times.Once);
				deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Never);
			}
		}

		public void TestDeliverAsDraft()
		{
			AssertDeliverAsDraft(DialogResult.Yes, true);
			AssertDeliverAsDraft(DialogResult.No, false);

			void AssertDeliverAsDraft(DialogResult answer, bool deliverSuccess)
			{
				using (DataContextManagersSubstitution())
				{
					var (dummy, info, _, securityService, _, deliveryService) = CreateDocumentInfos();
					securityService.Setup(s => s.CanDeliver).Returns(true);
					dummy.ShouldUseDraftWatermark = true;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(answer);
					var deliverCommand = new DeliverDocumentCommand(dummy);
					((INotifiableDocumentInfoCreated)deliverCommand).NotifyDocumentInfoCreated(info);

					AssertEquals(deliverSuccess, deliverCommand.Invoke());
					if (answer == DialogResult.Yes)
					{
						deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), true), Times.Once);
					}
					deliveryService.Verify(s => s.Deliver(dummy, It.IsAny<IReadOnlyCollection<IDocumentDelivery>>(), false), Times.Never);
				}
			}
		}

		(DummyWithUXmlSupport, DocumentInfo, Mock<IDocument>, Mock<IDocumentSecurityService>, Mock<IUserNotificationService>, Mock<IDocumentDeliveryService>) CreateDocumentInfos()
		{
			var dummy = Factory.New<DummyWithUXmlSupport>();
			var documentData = dummy.LoadOrCreateDocumentData("xxx");

			var dataObject = new DummyWithDocDataObjectValidation();
			dataObject.Name = "John";

			Factory.Save();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document"":DataContext=""Test"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier""
#Event:Type=""BeforeDocumentCreated""
	Name.DeliveryErrorIf({@data == ""John""}, ""Test Delivery Error"")
#End
#End");

			var worksheetTemplate = new StandardTemplate(worksheet);
			var data = dataObject.MakeDynamic();
			var document = new Mock<IDocument>();

			document.Setup(d => d.Data).Returns(data);

			var services = new ServiceContainer();
			var securityService = new Mock<IDocumentSecurityService>();
			services.Register<IDocumentSecurityService>(securityService.Object);
			securityService.Setup(s => s.CanDeliver).Returns(true);

			var notificationService = new Mock<IUserNotificationService>();
			services.Register<IUserNotificationService>(notificationService.Object);

			var deliveryService = new Mock<IDocumentDeliveryService>();
			services.Register<IDocumentDeliveryService>(deliveryService.Object);

			var descriptor = new Mock<IDocumentDescriptor>();
			var printInstructions = new DummyPrintInstructions
			{
				Title = "Test Document",
				DeliveryModes = new[]
				{
						nameof(PrintCopyType.ALL)
					}
			};
			descriptor
			.Setup(d => d.PrintInstructions)
				.Returns(printInstructions);

			var info = new DocumentInfo
			(
				new Lazy<ITemplate>(() => worksheetTemplate),
				new Lazy<IVisualizerDocumentData>(() => documentData),
				new Lazy<IServiceContainer>(() => services),
				new Lazy<IDocumentDescriptor>(() => descriptor.Object),
				new Lazy<IDocument>(() => document.Object)
			);

			return (dummy, info, document, securityService, notificationService, deliveryService);
		}
	}
}
