using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM429MessageInterpreter))]
	class IM429MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM429MessageInterpreter, IIM429Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM429;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM429 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <MRN>12MRN345ABCDE678R9</MRN>
    <declarationType>IM</declarationType>
    <additionalDeclarationType>A</additionalDeclarationType>
    <declarationAcceptanceDate>2023-08-10</declarationAcceptanceDate>
    <releaseDate>2023-08-11</releaseDate>
    <ResponseDateLimit>2023-08-15</ResponseDateLimit>
    <PreferredPaymentMethod>J</PreferredPaymentMethod>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
</q1:IM429>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Release for Import (IM429) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Declaration Type</td><td>IM</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Declaration Acceptance Date</td><td>10-Aug-23</td></tr><tr><td>Release Date</td><td>11-Aug-23</td></tr><tr><td>Response Date Limit</td><td>15-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>J</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IIM429Provider GetProvider(TextReader reader) => new IM429Provider(new MailBoxItemProvider<Im429>(reader).Message);
	}
}
