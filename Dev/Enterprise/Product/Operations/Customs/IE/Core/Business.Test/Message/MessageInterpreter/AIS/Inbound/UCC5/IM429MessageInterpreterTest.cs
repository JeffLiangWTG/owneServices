using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM429Provider = Enterprise.Customs.IE.Messaging.UCC5.IM429Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM429MessageInterpreter))]
	class IM429MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM429MessageInterpreter, IM429Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM429;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM429Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Release Notification (IM429) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Declaration Type</td><td>AB</td></tr><tr><td>Additional Declaration Type</td><td>C</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acceptance Date</td><td>24-Feb-24</td></tr><tr><td>Response Date Limit</td><td>24-Feb-24</td></tr><tr><td>Preferred Payment Method</td><td>P</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IM429Provider GetProvider(TextReader reader) => new IM429Provider(new MailBoxItemProvider<Im429>(reader).Message);
	}
}
