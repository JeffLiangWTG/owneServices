using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM462Provider = Enterprise.Customs.IE.Messaging.UCC5.IM462Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM462MessageInterpreter))]
	class IM462MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM462MessageInterpreter, IM462Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM462;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM462Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Request Declaration Amendment message (IM462) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>11111111-1111-1111-1111-111111111111</td></tr><tr><td>Amendment Reason</td><td>Amend Reason</td></tr></table>";

		protected override IM462Provider GetProvider(TextReader reader) => new IM462Provider(new MailBoxItemProvider<Im462>(reader).Message);
	}
}
