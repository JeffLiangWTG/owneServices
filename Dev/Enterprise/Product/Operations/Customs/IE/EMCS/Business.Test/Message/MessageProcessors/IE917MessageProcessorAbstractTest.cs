using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE917MessageProcessor))]
	abstract class IE917MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE917MessageProcessor, IIE917>
	{
		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE917;

		protected override ZString MessageFriendlyName => "EMCS IE917 Message Processor";

		protected override IE917MessageProcessor Processor => new IE917MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEndToEndProcessing();
		}

		protected override void AssertEndToEndProcessing()
		{
			var expectedInterpretation = IE917MessageInterpreterAbstractTest.ExpectedInterpretation.AdjustEmptyXmlTags().RemoveLineBreakingsAndIndents();
			var actualInterpretation = incomingMessage.EM_MessageInterpretation.AdjustEmptyXmlTags().RemoveLineBreakingsAndIndents();
			AssertXMLEquals("Interpretation should be set", expectedInterpretation, actualInterpretation);
			AssertEquals("JE_MessageStatus should have been set ERR.", LogicalStatusList.Codes.Error, declaration.JE_MessageStatus);
			AssertEquals("EM_Status should have been set PRS.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS negative acknowledgement of XML message",
				expectingBodyTexts,
				expectingRecipients);
		}

		static readonly string[] expectingBodyTexts = new[] { "Your EMCS Declaration for Job E00000810 received a negative acknowledgement of XML message. For details please follow the Link to the Job." };
		static readonly string[] expectingRecipients = new string[] { "staff1@where.com" };
	}
}
