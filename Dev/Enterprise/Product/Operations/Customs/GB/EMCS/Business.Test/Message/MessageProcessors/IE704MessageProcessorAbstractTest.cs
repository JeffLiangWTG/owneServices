using CargoWise.Types;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE704MessageProcessor))]
	abstract class IE704MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE704MessageProcessor, IIE704>
	{
		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE704;

		protected override IE704MessageProcessor Processor => new IE704MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("Declaration Message Status", EDIMessage.Status.Rejected, declaration.JE_MessageStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Submission Rejected Response", new[] { "Your EMCS Declaration for Job E00000810 has been rejected. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
