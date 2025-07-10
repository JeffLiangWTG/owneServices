using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS351;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS351MessageInterpreter))]
	class TS351MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS351MessageInterpreter, TS351Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS351;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:TS351 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <Declaration>
    <LRN>LRN001</LRN>
    <MRN>12MRN345ABCDE678R9</MRN>
    <specificCircumstanceIndicator>SCI</specificCircumstanceIndicator>
    <PreviousDocument>
      <dateAndTimeOfPresentationOfTheGoods>2023-08-10</dateAndTimeOfPresentationOfTheGoods>
    </PreviousDocument>
    <PreviousDocument>
      <dateAndTimeOfPresentationOfTheGoods>2023-08-11</dateAndTimeOfPresentationOfTheGoods>
    </PreviousDocument>
    <ControlResult>
      <code>CR</code>
      <date>2023-08-15</date>
      <remarks>Control Result Remarks</remarks>
    </ControlResult>
    <Remarks>Remarks001</Remarks>
  </Declaration>
</q1:TS351>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A [G4 | G4+G3 | Manifest] Refusal (TS351) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Specific Circumstance Indicator</td><td>SCI</td></tr><tr><td>Date and Time of Presentation of the Goods</td><td>10-Aug-23</td></tr><tr><td>&nbsp;</td><td>11-Aug-23</td></tr><tr><td>Control Result Code</td><td>CR</td></tr><tr><td>Control Result Date</td><td>15-Aug-23</td></tr><tr><td>Control Result Remarks</td><td>Control Result Remarks</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override TS351Provider GetProvider(TextReader reader) => new TS351Provider(new MailBoxItemProvider<Ts351>(reader).Message);
	}
}
