using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE802MessageProcessor))]
	abstract class IE802MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE802MessageProcessor, IIE802>
	{
		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE802;

		protected override IE802MessageProcessor Processor => new IE802MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set REM.", EntryStatusList.Codes.REM, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Reminder Message", new[] { "Your EMCS Declaration for Job E00000810 received a reminder message. For details please follow the Link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
