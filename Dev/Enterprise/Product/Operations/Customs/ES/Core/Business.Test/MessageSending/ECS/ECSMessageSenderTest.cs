using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using static Enterprise.Customs.ES.Business.MessageSending.ECSMessageSender;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ECSMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ECSMessageSender(null));
		}

		public void TestGetMessageBuilders()
		{
			CombineAssertions(() =>
			{
				var messageSendingObject = new ECSMessageSendingObject(exitHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new ECSExitHeaderMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				var exitDetailSending = sendingObject.ExitDetail;
				AssertEquals("Exit Detail status is XXX", "XXX", exitDetailSending.CED_Status);

				var sender = new ECSMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				AssertEquals("The returned messagebuildersdata list has 1 element", 1, messageBuildersData.Count);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestSendMessage()
		{
			CombineAssertions(() =>
			{
				var messageSendingObject = new ECSMessageSendingObject(exitHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new ECSExitHeaderMessageSendingObjectParent(messageSendingObject);
				var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				var exitDetailSending = sendingObject.ExitDetail;
				AssertEquals("Exit Detail status is XXX", "XXX", exitDetailSending.CED_Status);

				var sender = new ECSMessageSender(messageSendingObjectParent);

				var messageBuildersData = sender.GetMessageBuildersData();
				messageBuildersData.Add(new MessageBuilderData { });
				sender.Send(messageBuildersData);
				Factory.Save();
				AssertEquals("The message was created and sent, messagesSent is 1", 1, sender.messagesSent);
				AssertEquals("There is a message with error (the empty builderdata added), messagesWithSendFailure is 1", 1, sender.messagesWithSendFailure);
				AssertEquals("LastKeyReported has exception", "ECSMessageSender.SendIndividualDeclaration", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				var messages = exitDetailSending.Messages;
				AssertEquals("There is a new message in the exit detail", 1, messages.Count);
				var sentMessage = messages[0];
				AssertEquals("IsInDatabase", true, sentMessage.IsInDatabase);
				AssertEquals("MessageType is EAL", "EAL", sentMessage.EM_MessageType);
				AssertEquals("Exit Detail status has changed to Awaiting Response", "AWR", exitDetailSending.CED_Status);
			});
		}

		public void TestSendMessage_MultipleDetails()
		{
			var exitDetail2 = Factory.NewWithValidTestData<CusExitDetail>();
			exitDetail2.CED_Status = "AAA";
			exitDetail2.CED_CEH = exitHeader.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var messageSendingObject = new ECSMessageSendingObject(exitHeader, staff);
				messageSendingObject.Factory.RefreshEnabled = false;
				var messageSendingObjectParent = new ECSExitHeaderMessageSendingObjectParent(messageSendingObject);
				var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
				sendingObject1.ShouldSend = true;
				var exitDetailSending1 = sendingObject1.ExitDetail;
				AssertEquals("Exit Detail status is XXX", "XXX", exitDetailSending1.CED_Status);
				var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
				sendingObject2.ShouldSend = true;
				var exitDetailSending2 = sendingObject2.ExitDetail;
				AssertEquals("Exit Detail status is AAA", "AAA", exitDetailSending2.CED_Status);

				var sender = new ECSMessageSender(messageSendingObjectParent);

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
				AssertEquals("First exit detail's message MessageType is EAL", "EAL", sentMessage1.EM_MessageType);
				AssertEquals("First Exit Detail status has changed to Awaiting Response", "AWR", exitDetailSending1.CED_Status);

				var messages2 = exitDetailSending2.Messages;
				AssertEquals("There is a new message in the second exit detail", 1, messages2.Count);
				var sentMessage2 = messages2[0];
				AssertEquals("Second exit detail's message IsInDatabase", true, sentMessage2.IsInDatabase);
				AssertEquals("Second exit detail's message MessageType is EAL", "EAL", sentMessage2.EM_MessageType);
				AssertEquals("SecondExit Detail status has changed to Awaiting Response", "AWR", exitDetailSending2.CED_Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.GetStaffAccount();
			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
			exitHeader.CEH_CustomsProfile = "TestCert1";
			var exitDetail = Factory.NewWithValidTestData<CusExitDetail>();
			exitDetail.CED_Status = "XXX";
			exitDetail.CED_CEH = exitHeader.PK;

			Factory.Save();
		}
		Enterprise.MasterFiles.Business.GlbStaff staff;
		CusExitControlHeader exitHeader;
	}
}
