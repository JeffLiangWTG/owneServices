using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC521CProcessor))]
	class CC521CProcessorTest : ExitControlMessageProcessorTest<CC521CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC521CProvider>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Request for Proof of Exit not set Entry Status.", AESEntryStatusList.Codes.DiversionRequestRejected, messageAttachee.CER_Status);
			AssertEquals("Request for Proof of Exit not set Logical Status", LogicalStatusList.Codes.Accepted, messageAttachee.CER_MessageStatus);

			var jobNumber = messageAttachee.Header.CXH_JobReference;
			var messageInterpretation = $@"An Export Diversion Rejection message has been received from Customs on Job {messageAttachee.Header.CXH_JobReference} through the IE521 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REJ - Exit Released Rejected</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Diversion Rejection Reason Code</td><td>11 - Cancelled</td></tr><tr><td>Diversion Rejection Text</td><td>Because bad data</td></tr><tr><td>Customs office of Exit (Actual)</td><td>REFNO123</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override (CusExitHeader declaration, CusExitReport messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var factory = new BusinessObjectFactory();
			TestHelper.CreateNewOrGetExistingCusCodeList(factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.DiversionRejectionCode, "11", "Cancelled");
			factory.Save();
			return base.CreateSetupData();
		}

		protected override ZString MessageFriendlyName => "CC521C: DIVERSION REJECTION NOTIFCATION";

		protected override CC521CProcessor Processor => new CC521CProcessor(logger, typeof(Cc521C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE521;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC521CText("21IEDUB11A782454R2");
	}
}
