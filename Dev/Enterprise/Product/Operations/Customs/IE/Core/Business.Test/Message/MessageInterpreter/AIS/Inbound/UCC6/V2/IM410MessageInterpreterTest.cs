using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM410;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM410MessageInterpreter))]
	class IM410MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM410MessageInterpreter, IM410Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM410;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM410 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <customsRegistrationNumber>12CRN345ABCDE678R9</customsRegistrationNumber>
    <MRN>12MRN345ABCDE678R9</MRN>
    <invalidationDecisionDateAndTime>2023-08-10T14:30:45</invalidationDecisionDateAndTime>
    <invalidationRequestDateAndTime>2023-08-11T14:30:45</invalidationRequestDateAndTime>
    <invalidationInitiatedByCustoms>0</invalidationInitiatedByCustoms>
    <invalidationJustification>Invalidation Justification</invalidationJustification>
  </ImportOperation>
</q1:IM410>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Invalidation of Customs Declaration (IM410) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Invalidation Decision Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Invalidation Request Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Invalidation Initiated by Customs</td><td>0</td></tr><tr><td>Invalidation Justification</td><td>Invalidation Justification</td></tr></table>";

		protected override IM410Provider GetProvider(TextReader reader) => new IM410Provider(new MailBoxItemProvider<Im410>(reader).Message);
	}
}
