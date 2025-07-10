using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM482;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM482MessageInterpreter))]
	class IM482MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM482MessageInterpreter, IM482Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM482;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", AISInterchangeProcessorTestHelper.GetStandardIM482Text("12MRN345CDEFG678R9", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Documents Request (IM482) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Request Date</td><td>11-Aug-23 00:00</td></tr><tr><td>Date Limit</td><td>11-Aug-23 00:00</td></tr></table><br /><br />Document Additional Information: 1<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Type</td><td>Z123</td></tr><tr><td>Complementary Information</td><td>Test1</td></tr></table><br /><br />Document Additional Information: 2<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Type</td><td>Z234</td></tr><tr><td>Complementary Information</td><td>Test2</td></tr></table>";

		protected override IM482Provider GetProvider(TextReader reader) => new IM482Provider(new MailBoxItemProvider<Im482>(reader).Message);
	}
}
