using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM484;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM484MessageInterpreter))]
	class IM484MessageInterpreterTest : InboundMessageInterpreterAbstractTest<InboundEDIMessage, IM484MessageInterpreter, IIM484Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM484;

		protected override InboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM484 xmlns:q1=""http://www.ros.ie/schemas/customs"">
      <ImportOperation>
        <MRN>12MRN345CDEFG678R9</MRN>
        <LRN>LRN123456789</LRN>
        <RequestDate>2023-09-20</RequestDate>
        <DateLimit>2023-09-21</DateLimit>
      </ImportOperation>
      <GoodsShipment>
        <AdditionalInformation>
          <DocumentType>D001</DocumentType>
          <DocumentComplementaryInformation>DocInfo1</DocumentComplementaryInformation>
        </AdditionalInformation>
        <AdditionalInformation>
          <DocumentType>D002</DocumentType>
          <DocumentComplementaryInformation>DocInfo2</DocumentComplementaryInformation>
        </AdditionalInformation>
      </GoodsShipment>
    </q1:IM484>");
		}

		protected override ZString GetExpectedInterpretation(InboundEDIMessage message) => @"A Document Presentation Request (IM484) message has been received for Job B00000012.
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
<tr><td>LRN</td><td>LRN123456789</td></tr>
<tr><td>Request Date</td><td>20-Sep-23</td></tr>
<tr><td>Date Limit</td><td>21-Sep-23</td></tr></table>
<br />
<br />
Shipment Additional Information 1
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr><td>Document Type</td><td>D001</td></tr>
<tr><td>Document Complementary Information</td><td>DocInfo1</td></tr></table>
<br />
<br />
Shipment Additional Information 2
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr><td>Document Type</td><td>D002</td></tr>
<tr><td>Document Complementary Information</td><td>DocInfo2</td></tr></table>";

		protected override IIM484Provider GetProvider(TextReader reader) => new IM484Provider(new MailBoxItemProvider<Im484>(reader).Message);
	}
}
