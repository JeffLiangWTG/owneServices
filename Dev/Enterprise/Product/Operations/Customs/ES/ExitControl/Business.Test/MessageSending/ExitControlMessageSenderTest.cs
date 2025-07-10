using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using static Enterprise.Customs.ES.ExitControl.Business.ExitControlMessageSender;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class ExitControlMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExitControlMessageSender(null));
		}

		public void TestGetMessageBuilders()
		{
			CombineAssertions(() =>
			{
				var messageSendingObject = new MessageSendingObject(exitHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new ExitControlMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				AssertEquals("Exit Detail status is XXX", "XXX", exitReport.CER_MessageStatus);

				var sender = new ExitControlMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("The returned messagebuildersdata list has 1 element", 1, messageBuildersData.Count);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSend()
		{
			var messageSendingObject = new MessageSendingObject(exitHeader, staff);
			messageSendingObject.Factory.RefreshEnabled = false;
			var messageSendingObjectParent = new ExitControlMessageSendingObjectParent(messageSendingObject);
			var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			CombineAssertions(() =>
			{
				AssertEquals("Exit Detail status is XXX", "XXX", exitReport.CER_MessageStatus);

				var sender = new ExitControlMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				messageBuildersData.Add(new MessageBuilderData { });
				sender.Send(messageBuildersData);
				Factory.Save();
				AssertEquals("The message was created and sent, messagesSent is 1", 1, sender.messagesSent);
				AssertEquals("There is a message with error (the empty builderdata added), messagesWithSendFailure is 1", 1, sender.messagesWithSendFailure);
				AssertEquals("LastKeyReported has exception", "ExitControlMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var messages = exitReport.Messages;
				AssertEquals("There is a new message in the exit report", 1, messages.Count);
				var sentMessage = messages[0];
				AssertEquals("IsInDatabase", true, sentMessage.IsInDatabase);
				AssertEquals("Exit report message status has changed to sent", "SNT", exitReport.CER_MessageStatus);
				AssertNotContains("Message Contains Message Number in Message Text", "&lt;&lt;MSGNO PLACEHOLDER&gt;&gt;", sentMessage.EM_MessageText);
			});
		}

		public void TestSendMessage_MultipleReports()
		{
			var exitReport2 = exitHeader.CusExitReports.AddNew();
			exitReport2.CER_MessageStatus = "AAA";
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_LocalReference = "Ref2";
			exitReport2.CER_CXC_Consignment = consignment2.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var messageSendingObject = new MessageSendingObject(exitHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new ExitControlMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.ShouldSend = true;
				var exitDetailSending1 = sendingObject1.MessagingObject;
				AssertEquals("Exit Detail status is XXX", "XXX", exitDetailSending1.CER_MessageStatus);
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.ShouldSend = true;
				var exitDetailSending2 = sendingObject2.MessagingObject;
				AssertEquals("Exit Detail status is AAA", "AAA", exitDetailSending2.CER_MessageStatus);

				var sender = new ExitControlMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				sender.Send(messageBuildersData);
				Factory.Save();
				AssertEquals("The messages were created and sent, messagesSent is 2", 2, sender.messagesSent);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();

				var messages1 = exitDetailSending1.Messages;
				AssertEquals("There is a new message in the first exit detail", 1, messages1.Count);
				var sentMessage1 = messages1[0];
				AssertEquals("First exit detail's message IsInDatabase", true, sentMessage1.IsInDatabase);
				AssertEquals("First Exit Detail message status has changed to Sent Response", "SNT", exitDetailSending1.CER_MessageStatus);

				var messages2 = exitDetailSending2.Messages;
				AssertEquals("There is a new message in the second exit detail", 1, messages2.Count);
				var sentMessage2 = messages2[0];
				AssertEquals("Second exit detail's message IsInDatabase", true, sentMessage2.IsInDatabase);
				AssertEquals("SecondExit Detail message status has changed to Sent Response", "SNT", exitDetailSending2.CER_MessageStatus);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.GetStaffAccount();
			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = staff.GS_Code;
			exitHeader.CXH_CustomsProfile = "TestCert1";

			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_MessageStatus = "XXX";
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_LocalReference = "Ref1";
			exitReport.CER_CXC_Consignment = consignment.PK;

			Factory.Save();
		}
		MasterFiles.Business.GlbStaff staff;
		CusExitHeader exitHeader;
		CusExitReport exitReport;
	}
}
