using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM099MessageInterpreter))]
	class IM099MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM099MessageInterpreter, IIM099Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM099 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN123456789</LRN>
    <DateLimitOfResponse>2023-08-11</DateLimitOfResponse>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
  <CustomsOfficeOfPresentation>
    <referenceNumber>PCO12345</referenceNumber>
  </CustomsOfficeOfPresentation>
  <SupervisingCustomsOffice>
    <referenceNumber>SCO12345</referenceNumber>
  </SupervisingCustomsOffice>
  <CustomsOfficeLodgement>
    <referenceNumber>LCO123456</referenceNumber>
  </CustomsOfficeLodgement>
</q1:IM099>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A General Notification and Request Information (IM099) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Date Limit of Response</td><td>2023-08-11</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr><tr><td>Customs Office of Presentation</td><td>PCO12345</td></tr><tr><td>Supervising Customs Office</td><td>SCO12345</td></tr><tr><td>Customs Office Lodgement</td><td>LCO123456</td></tr></table>";

		protected override IIM099Provider GetProvider(TextReader reader) => new IM099Provider(new MailBoxItemProvider<Im099>(reader).Message);
	}
}
