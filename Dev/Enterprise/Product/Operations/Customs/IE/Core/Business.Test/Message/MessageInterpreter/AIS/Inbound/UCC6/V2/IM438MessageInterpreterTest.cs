using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM438;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM438MessageInterpreter))]
	class IM438MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM438MessageInterpreter, IM438Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM438;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM438 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <MRN>12MRN345ABCDE678R9</MRN>
  </ImportOperation>
  <ReminderDetails>
    <requestDate>2023-08-10</requestDate>
    <expirationDate>2023-08-11</expirationDate>
  </ReminderDetails>
</q1:IM438>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Reminder for Providing Additional Documents (IM438) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Request Date</td><td>10-Aug-23</td></tr><tr><td>Expiration Date</td><td>11-Aug-23</td></tr></table>";

		protected override IM438Provider GetProvider(TextReader reader) => new IM438Provider(new MailBoxItemProvider<Im438>(reader).Message);
	}
}
