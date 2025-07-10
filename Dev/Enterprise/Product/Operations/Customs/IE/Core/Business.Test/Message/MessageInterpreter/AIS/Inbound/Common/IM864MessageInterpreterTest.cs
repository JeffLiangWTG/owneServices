using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM864;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM864MessageInterpreter))]
	class IM864MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM864MessageInterpreter, IM864Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM864;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM864 xmlns:q1=""http://www.ros.ie/schemas/customs"">
<ImportOperation>
<MRN>12MRN345ABCDE678R9</MRN>
<CaseId>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</CaseId>
<InvalidationRequestCancellationReason>Some Reason for Declaration Invalidation Request Cancellation</InvalidationRequestCancellationReason>
</ImportOperation>
</q1:IM864>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Invalidation Request Cancellation (IM864) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>Some Reason for Declaration Invalidation Request Cancellation</td></tr></table>";

		protected override IM864Provider GetProvider(TextReader reader) => new IM864Provider(new MailBoxItemProvider<Im864>(reader).Message);
	}
}
