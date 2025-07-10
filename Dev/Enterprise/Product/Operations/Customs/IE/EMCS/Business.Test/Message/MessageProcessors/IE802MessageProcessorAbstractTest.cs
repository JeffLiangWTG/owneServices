using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE802MessageProcessor))]
	abstract class IE802MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE802MessageProcessor, IIE802>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE802;

		protected override ZString MessageFriendlyName => "EMCS IE802 Message Processor";

		protected override IE802MessageProcessor Processor => new IE802MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set REM.", EntryStatusList.Codes.REM, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Reminder Message",
				new[] { "Your EMCS Declaration for Job E00000810 received a reminder message. For details please follow the Link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void AssertEndToEndProcessing()
		{
			AssertEquals("JE_EntryStatus should have been set REM.", EntryStatusList.Codes.REM, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Reminder Message",
				new[] { "Your EMCS Declaration for Job E00000810 received a reminder message. For details please follow the Link to the Job." },
				new string[] { "staff1@where.com" });
		}
	}
}
