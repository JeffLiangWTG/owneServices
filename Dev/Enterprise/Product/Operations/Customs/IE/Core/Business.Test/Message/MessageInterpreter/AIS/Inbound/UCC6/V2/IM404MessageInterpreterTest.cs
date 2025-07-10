using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM404MessageInterpreter))]
	class IM404MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM404MessageInterpreter, IIM404Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM404 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <customsRegistrationNumber>12AB345CDEFGH678R9</customsRegistrationNumber>
    <MRN>12MRN345CDEFG678R9</MRN>
    <amendmentDateAndTime>2023-08-10T14:30:45</amendmentDateAndTime>
    <amendmentAcceptanceDateAndTime>2023-08-11T14:30:45</amendmentAcceptanceDateAndTime>
    <PreferredPaymentMethod>A</PreferredPaymentMethod>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
</q1:IM404>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Amendment Request Registration (IM404) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12AB345CDEFGH678R9</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Amendment Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IIM404Provider GetProvider(TextReader reader) => new IM404Provider(new MailBoxItemProvider<Im404>(reader).Message);
	}
}
