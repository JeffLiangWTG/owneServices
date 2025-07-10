using System;
using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using Notification = Enterprise.DocumentVisualizer.Core.Notification;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DocumentDelivery))]
	sealed class DeliveryTest : CommandProviderTest
	{
		public void TestDeliverWithDeliveryError()
		{
			using (DataContextManagersSubstitution())
			{
				var dummy = Factory.New<DummyWithUXmlSupport>();
				var documentData = dummy.LoadOrCreateDocumentData("xxx");
				var documentName = "Test Document";

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
				document.Setup(d => d.Notifications).Returns(new[] { new Notification(new DummyNotificationSource(), NotificationType.DeliveryError, "Test Delivery Error") });

				var services = new ServiceContainer();
				var securityService = new Mock<IDocumentSecurityService>();
				services.Register<IDocumentSecurityService>(securityService.Object);
				securityService.Setup(s => s.CanDeliver).Returns(true);

				var notificationService = new Mock<IUserNotificationService>();
				services.Register<IUserNotificationService>(notificationService.Object);

				var descriptor = new Mock<IDocumentDescriptor>();
				var printInstructions = new DummyPrintInstructions
				{
					Title = documentName,
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

				var documentDelivery = new DocumentDelivery(dummy);
				documentDelivery.OnDocumentInfosCreated(info);

				AssertNoExceptionThrown(() => documentDelivery.Commands.First().Invoke());
				notificationService.Verify(s => s.ShowMessage("This document contains delivery errors. Please fix all message errors before delivering.", "Document Delivery"), Times.Once);
			}
		}

		protected override ICommandProvider CreateNewModule()
		{
			var supportable = new Mock<IDocumentSupportable>();
			return new DocumentDelivery(supportable.Object);
		}
	}
}
