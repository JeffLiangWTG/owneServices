using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.Presentation;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class OriginalMessageSenderTest : TestCaseWithFactory
	{
		public void TestResetToOriginal()
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11);
			var dynamicData = dataObject.MakeDynamic();

			var storage = consol.LoadOrCreateDocumentData("xxx");

			var notifications = new Moq.Mock<IUserNotificationService>();
			var document = new Moq.Mock<IDocument>();
			var messageInstructions = new Moq.Mock<IMessageInstructions>();

			var resetToOriginalEventFired = false;

			var broker = new EventBroker();
			broker
				.GetEvent<ResetToOriginalEvent>()
				.Subscribe(e => resetToOriginalEventFired = true);

			var services = new ServiceContainer();
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(broker);

			document.SetupGet(d => d.Data).Returns(dynamicData);
			notifications.Setup(s => s.ShowConfirmation(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(true);

			var sender = new OriginalMessageSender(messageInstructions.Object, document.Object, storage, services, "Shipping Instruction");

			var parameters = OriginalMessageSender.Parameters.New(new MacroMap(new Dictionary<string, object>
			{
				[OriginalMessageSender.Parameters.WarningName] = "warning",
				[OriginalMessageSender.Parameters.ConfirmationName] = "confirmation"
			}));

			sender.Send(parameters);

			var logs = storage.Logs
				.Find(l => l.SL_SE_NKEvent == Events.StatusUpdatedCode);

			AssertMultilineASCIIEquals("Reset To Original event added", string.Format("|DEP={0}|MST=Shipping Instruction|TYP=Reset To Original", Env.CurrentUser.FullName), string.Join("\r\n", logs.Select(l => l.SL_Reference)));
			Assert("Log has been saved in database.", logs.FirstOrDefault().IsInDatabase);
			AssertEquals("ResetToOriginalEvent fired", true, resetToOriginalEventFired);
		}

		public void TestResetToOriginal_ConfirmationMessageAndConfirmation_VariousRecipients()
		{
			VerifyConfirmationMessageAndConfirmation(null, @"WARNING: Using this option without checking with the recipient first might result in duplicate messages being processed by the recipient.
Resetting to Original should only be required when there is a serious messaging failure at the recipient's end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the recipient to make sure they have not already processed the message.", "I have confirmed with the recipient that they did not process the Original message already sent.");

			VerifyConfirmationMessageAndConfirmation("YANGMING", @"WARNING: Using this option without checking with the YANGMING first might result in duplicate messages being processed by the YANGMING.
Resetting to Original should only be required when there is a serious messaging failure at the YANGMING's end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the YANGMING to make sure they have not already processed the message.", "I have confirmed with the YANGMING that they did not process the Original message already sent.");

			VerifyConfirmationMessageAndConfirmation("ABC Logistics", @"WARNING: Using this option without checking with the ABC Logistics first might result in duplicate messages being processed by the ABC Logistics.
Resetting to Original should only be required when there is a serious messaging failure at the ABC Logistics' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the ABC Logistics to make sure they have not already processed the message.", "I have confirmed with the ABC Logistics that they did not process the Original message already sent.");

			VerifyConfirmationMessageAndConfirmation("ABC LOGISTICS", @"WARNING: Using this option without checking with the ABC LOGISTICS first might result in duplicate messages being processed by the ABC LOGISTICS.
Resetting to Original should only be required when there is a serious messaging failure at the ABC LOGISTICS' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the ABC LOGISTICS to make sure they have not already processed the message.", "I have confirmed with the ABC LOGISTICS that they did not process the Original message already sent.");

			VerifyConfirmationMessageAndConfirmation("Customs", @"WARNING: Using this option without checking with Customs first might result in duplicate messages being processed by Customs.
Resetting to Original should only be required when there is a serious messaging failure at Customs' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with Customs to make sure they have not already processed the message.", "I have confirmed with Customs that they did not process the Original message already sent.");

			VerifyConfirmationMessageAndConfirmation("CUSTOMS", @"WARNING: Using this option without checking with CUSTOMS first might result in duplicate messages being processed by CUSTOMS.
Resetting to Original should only be required when there is a serious messaging failure at CUSTOMS' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with CUSTOMS to make sure they have not already processed the message.", "I have confirmed with CUSTOMS that they did not process the Original message already sent.");
		}

		void VerifyConfirmationMessageAndConfirmation(string recipient, string message, string confirmation)
		{
			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "Test";
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;

			var document = new Moq.Mock<IDocument>();
			var descriptor = new Mock<IDocumentDescriptor>();
			descriptor.SetupGet(d => d.MessageInstructions.DocumentName).Returns("test");
			descriptor.SetupGet(d => d.MessageInstructions.Recipient).Returns(recipient);
			descriptor.SetupGet(d => d.EDocsInstructions.SaveCopyToEDocs).Returns(false);
			var documentInfo = new Mock<IDocumentInfo>();
			var broker = new EventBroker();
			var services = new ServiceContainer();
			documentInfo.SetupGet(di => di.Document).Returns(document.Object);
			documentInfo.SetupGet(di => di.DocumentData).Returns(documentData);
			documentInfo.SetupGet(di => di.Descriptor).Returns(descriptor.Object);
			documentInfo.SetupGet(di => di.Services).Returns(services);
			var dynamicData = new Mock<IDynamicData>();
			var securityService = new Mock<IDocumentSecurityService>();
			securityService.SetupGet(ss => ss.CanSendMessage).Returns(true);

			Factory.Save();

			var notifications = new Moq.Mock<IUserNotificationService>();
			var messageInstructions = new Moq.Mock<IMessageInstructions>();

			services.Register<IDocumentSecurityService>(securityService.Object);
			services.Register<IUserNotificationService>(notifications.Object);
			services.Register<IEventBroker>(broker);

			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			notifications.Setup(s => s.ShowConfirmation(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(true);

			var command = new ResetToOriginalCommand();
			command.Invoke(null, documentInfo.Object);

			notifications.Verify(n => n.ShowConfirmation(message, Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), confirmation), Times.Once);
			Assert(true);
		}
	}
}
