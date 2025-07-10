using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM462;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM462_IM962MessageInterpreter))]
	class IM462MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM462_IM962MessageInterpreter, IM462_IM962Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM462;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<IM462 xmlns=""http://www.ros.ie/schemas/customs/IM462H7"">
  <Declaration>
    <MRN>12MRN345CDEFG678R9</MRN>
    <CaseId>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</CaseId>
    <AmendReason>AmendReason001</AmendReason>
  </Declaration>
</IM462>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Amendment Request (IM462) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Amend Reason</td><td>AmendReason001</td></tr></table>";

		protected override IM462_IM962Provider GetProvider(TextReader reader) => new IM462_IM962Provider(new MailBoxItemProvider<Im462>(reader).Message);
	}
}
