using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM464;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM464MessageInterpreter))]
	class IM464MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM464MessageInterpreter, IM464Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM464;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM464 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345CDEFG678R9</MRN>
    <CaseId>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</CaseId>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
</q1:IM464>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Request Declaration Invalidation (IM464) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IM464Provider GetProvider(TextReader reader) => new IM464Provider(new MailBoxItemProvider<Im464>(reader).Message);
	}
}
