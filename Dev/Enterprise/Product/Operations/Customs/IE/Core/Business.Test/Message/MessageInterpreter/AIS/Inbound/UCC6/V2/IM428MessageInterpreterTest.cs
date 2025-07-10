using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM428;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM428MessageInterpreter))]
	class IM428MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM428MessageInterpreter, IIM428Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM428;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM428 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <customsRegistrationNumber>12CRN345ABCDE678R9</customsRegistrationNumber>
    <MRN>12MRN345CDEFG678R9</MRN>
    <declarationAcceptanceDateAndTime>2023-08-10T14:30:45</declarationAcceptanceDateAndTime>
    <declarationType>IM</declarationType>
    <additionalDeclarationType>A</additionalDeclarationType>
    <ResponseDateLimit>2023-08-11</ResponseDateLimit>
    <PreferredPaymentMethod>A</PreferredPaymentMethod>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
</q1:IM428>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Customs Declaration Acceptance (IM428) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Declaration Acceptance Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Declaration Type</td><td>IM</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Response Date Limit</td><td>11-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IIM428Provider GetProvider(TextReader reader) => new IM428Provider(new MailBoxItemProvider<Im428>(reader).Message);
	}
}
