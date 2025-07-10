using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM433MessageInterpreter))]
	class IM433MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM433MessageInterpreter, IM433Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM433;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(Factory, messageText: AISUCC5InterchangeProcessorTestHelper.GetStandardUCC5IM433Text());
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => ExpectedInterpretation;

		protected override IM433Provider GetProvider(TextReader reader) => new IM433Provider(new MailBoxItemProvider<Im433>(reader).Message);

		internal static string ExpectedInterpretation = $@"A Presentation Notification Rejection (IM433) message has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Rejection Date</td><td>29-Feb-24</td></tr>
				<tr><td>Rejection Reason</td><td>Rejection Reason</td></tr>
			</table><br />
			<br />
			{AISUCC5InterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation}";
	}
}
