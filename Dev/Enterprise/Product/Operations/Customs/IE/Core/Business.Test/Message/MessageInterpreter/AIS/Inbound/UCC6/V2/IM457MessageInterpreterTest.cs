using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM457;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM457MessageInterpreter))]
	sealed class IM457MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM457MessageInterpreter, IM457Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM457;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", AISInterchangeProcessorTestHelper.GetStandardIM457Text());

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Presentation Notification Registration (IM457) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Presentation Notification Registration Date and Time</td><td>15-Aug-23 14:10</td></tr></table>";

		protected override IM457Provider GetProvider(TextReader reader) => new IM457Provider(new MailBoxItemProvider<Im457>(reader).Message);
	}
}
