using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE704MessageProcessor))]
	abstract class IE704MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE704MessageProcessor, IIE704>
	{
		protected override ZString MessageFriendlyName => "EMCS IE704 Message Processor";

		protected override ZString MessageType => EMCSIncomingMessageTypeList.Codes.IE704;

		protected override IE704MessageProcessor Processor => new IE704MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			var expectedInterpretation = @"A Generic Refusal (IE704) message has been received for Job E00000810.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Administrative Reference Code</td><td>MRN98761234</td></tr><tr><td>LRN</td><td>B000222547896254786321</td></tr></table><br />
<br />Functional Error 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Type</td><td>12</td></tr><tr><td>Error Reason</td><td>reason1</td></tr><tr><td>Error Location</td><td>location1</td></tr><tr><td>Original Attribute Value</td><td>original value 1</td></tr></table><br />
<br />Functional Error 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Type</td><td>15</td></tr><tr><td>Error Reason</td><td>reason2</td></tr><tr><td>Error Location</td><td>location2</td></tr><tr><td>Original Attribute Value</td><td>original value 2</td></tr></table>";
			AssertContains("Interpretation should be set", expectedInterpretation, incomingMessage.EM_MessageInterpretation);
			AssertEquals("Declaration Message Status", EDIMessage.Status.Rejected, declaration.JE_MessageStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Submission Rejected Response",
				new[] { "Your EMCS Declaration for Job E00000810 has been rejected. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}

		protected override void AssertEndToEndProcessing()
		{
			AssertEquals("Declaration Message Status", EDIMessage.Status.Rejected, declaration.JE_MessageStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail(
				"EMCS Submission Rejected Response",
				new[] { "Your EMCS Declaration for Job E00000810 has been rejected. For details please follow the link to the Job." },
				new string[] { "staff1@where.com" });
		}
	}
}
