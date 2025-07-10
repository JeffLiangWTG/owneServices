using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM493;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM493MessageInterpreter))]
	class IM493MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM493MessageInterpreter, IM493Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM493;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM493 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345CDEFG678R9</MRN>
    <LRN>12LRN323666666</LRN>
  </ImportOperation>
</q1:IM493>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Amendment Notification for Partial or Deferred Quota Allocation (IM493) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>LRN</td><td>12LRN323666666</td></tr></table>";

		protected override IM493Provider GetProvider(TextReader reader) => new IM493Provider(new MailBoxItemProvider<Im493>(reader).Message);
	}
}
