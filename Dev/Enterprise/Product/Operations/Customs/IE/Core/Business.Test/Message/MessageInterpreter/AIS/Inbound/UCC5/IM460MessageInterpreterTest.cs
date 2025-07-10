using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM460Provider = Enterprise.Customs.IE.Messaging.UCC5.IM460Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM460MessageInterpreter))]
	class IM460MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM460MessageInterpreter, IM460Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM460;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM460Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Control (IM460) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Control Notification Date &amp; Time</td><td>20-Feb-24 23:59</td></tr><tr><td>Time Limit for Control</td><td>07-Mar-24 14:37</td></tr><tr><td>Customs Office Lodgement</td><td>OF123456</td></tr><tr><td>Overall Control Type Coded</td><td>O</td></tr></table><br/><br/>Control Type<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Type Coded</td><td>O</td></tr><tr><td>Control Type Agency</td><td>Revenue</td></tr></table><br/><br/>Control Type<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Type Coded</td><td>1</td></tr><tr><td>Control Type Agency</td><td>Revenue1</td></tr></table>";

		protected override IM460Provider GetProvider(TextReader reader) => new IM460Provider(new MailBoxItemProvider<Im460>(reader).Message);
	}
}
