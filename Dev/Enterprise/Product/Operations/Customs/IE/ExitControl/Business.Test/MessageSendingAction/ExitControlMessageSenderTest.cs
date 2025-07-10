using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class ExitControlMessageSenderTest : TestCaseWithFactory
	{
		public void TestMessageBuilder_ArrivalAtExit()
		{
			AssertMessageBuilder<IE507MessageBuilder>(AESOutgoingMessageTypeList.Codes.ArrivalAtExit);
		}

		public void TestMessageBuilder_ExitNotification()
		{
			AssertMessageBuilder<IE590MessageBuilder>(AESOutgoingMessageTypeList.Codes.ExitNotification);
		}

		void AssertMessageBuilder<T>(string messageType)
		{
			(_, _, _, var sender) = CreateSenderForTest(messageType);
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_MessageType = messageType;
			AssertType<T>("Should create correct MessageBuilder", sender.CreateMessageBuilder(outgoingMessage));
		}

		public void TestSend()
		{
			(_, var report, _, var sender) = CreateSenderForTest(AESOutgoingMessageTypeList.Codes.ArrivalAtExit);

			CombineAssertions("Created message should have correct values on all concerned fields. ", () =>
			{
				sender.Send();
				var createdMessage = (AESOutboundEDIMessage)report.Messages.Single();
				AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ArrivalAtExit, createdMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
				AssertStartsWith("EM_MessageText", @"<q1:CC507C xmlns:q1=""http://ecs.dgtaxud.ec"">", createdMessage.EM_MessageText);
				AssertEquals("CH_Status", LogicalStatusList.Codes.Sent, report.CER_MessageStatus);
			});
		}

		(CusExitHeader exitHeader, CusExitReport report, ExitControlMessageSendingObject sendingObject, ExitControlMessageSenderForTest sender) CreateSenderForTest(string messageType)
		{
			(_, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GB_Branch = branch.PK;
			var report = exitHeader.CusExitReports.AddNew();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			report.CER_CXC_Consignment = consignment.PK;

			var sendingObject = new ExitControlMessageSendingObject(report);
			sendingObject.MessageType = messageType;
			var sender = new ExitControlMessageSenderForTest(sendingObject);

			return (exitHeader, report, sendingObject, sender);
		}

		class ExitControlMessageSenderForTest : ExitControlMessageSender
		{
			public ExitControlMessageSenderForTest(ExitControlMessageSendingObject sendingObjectn) : base(sendingObjectn)
			{
			}

			public new IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage) => base.CreateMessageBuilder(outgoingMessage);
		}
	}
}
