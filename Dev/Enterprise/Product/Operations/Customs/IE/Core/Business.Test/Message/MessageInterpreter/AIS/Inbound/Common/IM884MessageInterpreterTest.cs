using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM884;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM884MessageInterpreter))]
	class IM884MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM884MessageInterpreter, IM884Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM884;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM884 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345ABCDE678R9</MRN>
    <CaseId>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</CaseId>
    <DocumentsPresentRequestCancellationReason>Documents Presentation Request Cancellation Reason</DocumentsPresentRequestCancellationReason>
  </ImportOperation>
</q1:IM884>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Documents Presentation Request Cancellation (IM884) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Documents Presentation Request Cancellation Reason</td><td>Documents Presentation Request Cancellation Reason</td></tr></table>";

		protected override IM884Provider GetProvider(TextReader reader) => new IM884Provider(new MailBoxItemProvider<Im884>(reader).Message);
	}
}
