using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS315V;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(TS315VMessageInterpreter))]
	sealed class TS315VMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS315VMessageInterpreter, TS315VProvider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS315V;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", @"<q1:TS315V xmlns:q1=""http://www.ros.ie/schemas/customs"">
			  <Declaration>
				<LRN>LRN001</LRN>
			    <MRN>12MRN345ABCDE678R9</MRN>
			    <declarationAcknowledgementDate>2023-08-10</declarationAcknowledgementDate>
			  </Declaration>
			</q1:TS315V>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A [G4 | G4+G3 | Manifest] Declaration Registration (TS315V) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Declaration Acknowledgement Date</td><td>10-Aug-23</td></tr></table>";

		protected override TS315VProvider GetProvider(TextReader reader) => new TS315VProvider(new MailBoxItemProvider<Ts315V>(reader).Message);
	}
}
