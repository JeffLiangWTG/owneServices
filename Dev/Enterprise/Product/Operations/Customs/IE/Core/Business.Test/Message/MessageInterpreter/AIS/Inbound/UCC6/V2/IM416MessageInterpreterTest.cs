using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM416MessageInterpreter))]
	sealed class IM416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM416MessageInterpreter, IIM416Provider>
	{
		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM416 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <additionalDeclarationType>A</additionalDeclarationType>
    <LRN>LRN001</LRN>
    <RejectionDate>2024-03-01</RejectionDate>
    <RejectionMotivationText>Rejection Motivation Text</RejectionMotivationText>
  </ImportOperation>
</q1:IM416>");
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM416;

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Customs Declaration Rejection (IM416) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Rejection Date</td><td>01-Mar-24</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table>";

		protected override IIM416Provider GetProvider(TextReader reader) => new IM416Provider(new MailBoxItemProvider<Im416>(reader).Message);
	}
}
