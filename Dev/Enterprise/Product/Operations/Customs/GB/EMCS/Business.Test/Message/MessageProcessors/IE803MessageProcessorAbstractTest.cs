using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE803MessageProcessor))]
	abstract class IE803MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE803MessageProcessor, IIE803>
	{
		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE803;

		protected override IE803MessageProcessor Processor => new IE803MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set DIV.", EntryStatusList.Codes.DIV, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of diverted e-AD", new[] { "Your EMCS Declaration for Job E00000810 received a notification of diverted e-AD. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
