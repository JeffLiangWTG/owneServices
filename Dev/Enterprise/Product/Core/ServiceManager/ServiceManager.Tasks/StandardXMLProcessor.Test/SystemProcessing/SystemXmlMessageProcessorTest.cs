using CargoWise.Application;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class SystemXmlMessageProcessorTest : ExtendedTestCaseWithFactory
	{
		public void TestGetEDIMessagesToProcess()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "BBB";
			var branch2 = company2.Branches.AddNew();
			branch1.GB_Code = "BR2";

			ZDateTime createTime = ZDateTime.UtcNow;
			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.CMR, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK, string.Empty, createTime);
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK, SystemMessageList.Codes.CustomerServiceResponse, createTime.AddMinutes(1));
			var message3 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, SystemMessageList.Codes.CustomerServiceResponse, createTime.AddMinutes(2));
			var message4 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Recognised, branch2.PK, string.Empty, createTime.AddMinutes(3));
			var message5 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK, SystemMessageList.Codes.CustomerServiceResponse, createTime.AddMinutes(4));
			var message6 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, SystemMessageList.Codes.CustomerServiceResponse, createTime.AddMinutes(5));
			var message7 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, branch1.PK, SystemMessageList.Codes.CustomerServiceResponse, createTime.AddMinutes(6));
			var message8 = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, SystemMessageList.Codes.CustomerServiceRequest, createTime.AddMinutes(7));
			var messageAAA = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, "AAA", createTime.AddMinutes(8));
			var messageBBB = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.SYS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, "BBB", createTime.AddMinutes(9));

			Factory.Save();

			var processor = new SystemXmlMessageProcessorForTest();

			using (branch1.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(4, messagesToProcess.Count);
				AssertEquals(message2.PK, messagesToProcess[0]);
				AssertEquals(message3.PK, messagesToProcess[1]);
				AssertEquals(message5.PK, messagesToProcess[2]);
				AssertEquals(message6.PK, messagesToProcess[3]);
			}

			using (branch2.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(4, messagesToProcess.Count);
				AssertEquals(message2.PK, messagesToProcess[0]);
				AssertEquals(message3.PK, messagesToProcess[1]);
				AssertEquals(message5.PK, messagesToProcess[2]);
				AssertEquals(message6.PK, messagesToProcess[3]);
			}

			processor.ClearSupportedMessages();
			processor.AddSupportedMessage_Exposed("AAA", (string)null);
			var messagePks = processor.GetEDIMessagePKs();
			AssertEquals(1, messagePks.Count);
			AssertEquals(messageAAA.PK, messagePks[0]);

			processor.ClearSupportedMessages();
			processor.AddSupportedMessage_Exposed("BBB", (string)null);
			messagePks = processor.GetEDIMessagePKs();
			AssertEquals(1, messagePks.Count);
			AssertEquals(messageBBB.PK, messagePks[0]);

			processor.AddSupportedMessage_Exposed("AAA", (string)null);
			messagePks = processor.GetEDIMessagePKs();
			AssertEquals(2, messagePks.Count);
			AssertEquals(messageAAA.PK, messagePks[0]);
			AssertEquals(messageBBB.PK, messagePks[1]);
		}

		public void TestGetMessageAction()
		{
			var processor = new SystemXmlMessageProcessorForTest();

			var action = processor.GetMessageAction(SystemMessage.MessageType, SystemMessageList.Codes.CustomerServiceResponse);
			AssertEquals("CustomerServiceResponseMessageAction", ObjectFactory.GetType("CustomerServiceResponseMessageAction"), action.GetType());

			action = processor.GetMessageAction(SystemMessage.MessageType, SystemMessageList.Codes.ReferenceDataUpdate);
			AssertEquals(typeof(ReferenceDataUpdateMessageAction), action.GetType());

			action = processor.GetMessageAction(SystemMessage.MessageType, SystemMessageList.Codes.LicenceUsageRequest);
			AssertType<LicenceUsageRequestMessageAction>(action);

			action = processor.GetMessageAction(SystemMessage.MessageType, SystemMessageList.Codes.StaffReportRequest);
			AssertType<StaffReportRequestMessageAction>(action);

			var testAction = new TestAction(null);
			using (ObjectFactory.Substitute("CustomerServiceResponseMessageAction", testAction))
			{
				action = processor.GetMessageAction(SystemMessage.MessageType, SystemMessageList.Codes.CustomerServiceResponse);
				AssertEquals("action type", typeof(TestAction), action.GetType());
			}

			// Add by Type
			processor.AddSupportedMessage_Exposed("AAA", testAction.GetType());
			action = processor.GetMessageAction(SystemMessage.MessageType, "AAA");
			AssertEquals("action type", testAction.GetType(), action.GetType());

			// Add by type id
			processor.ClearSupportedMessages();
			processor.AddSupportedMessage_Exposed("AAA", "CustomerServiceResponseMessageAction");
			action = processor.GetMessageAction(SystemMessage.MessageType, "AAA");
			AssertEquals("CustomerServiceResponseMessageAction", ObjectFactory.GetType("CustomerServiceResponseMessageAction"), action.GetType());

			AssertNull("unknown message type", processor.GetMessageAction("BAD", SystemMessageList.Codes.CustomerServiceResponse));
			AssertNull("unknown message sub type", processor.GetMessageAction(SystemMessage.MessageType, "BAD"));
		}

		public void TestAlwaysProcessOneByOne()
		{
			var processor = new SystemXmlMessageProcessor();
			AssertEquals(true, processor.AlwaysProcessOneByOne);
		}
	}
}
