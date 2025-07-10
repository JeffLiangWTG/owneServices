using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM451Provider = Enterprise.Customs.IE.Messaging.UCC5.IM451Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM451MessageInterpreter))]
	class IM451MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM451MessageInterpreter, IM451Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM451;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM451Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Release Rejection (IM451) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN123</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason</td></tr></table>";

		protected override IM451Provider GetProvider(TextReader reader) => new IM451Provider(new MailBoxItemProvider<Im451>(reader).Message);
	}
}
