using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM882;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM882MessageInterpreter))]
	class IM882MessageInterpreterTest : InboundMessageInterpreterAbstractTest<InboundEDIMessage, IM882MessageInterpreter, IM882Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM882;

		protected override InboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", @"<q1:IM882 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345CDEFG678R9</MRN>
    <CaseId>11111111-1111-1111-1111-111111111111</CaseId>
    <DocumentsUploadRequestCancellationReason>Documents Upload Request Cancellation Reason</DocumentsUploadRequestCancellationReason>
  </ImportOperation>
</q1:IM882>");
		}

		protected override ZString GetExpectedInterpretation(InboundEDIMessage message) => @"
A Documents Upload Request Cancellation (IM882) message has been received for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>MRN</td>
		<td>12MRN345CDEFG678R9</td>
	</tr>
	<tr>
		<td>Case Id</td>
		<td>11111111-1111-1111-1111-111111111111</td>
	</tr>
	<tr>
		<td>Documents Upload Request Cancellation Reason</td>
		<td>Documents Upload Request Cancellation Reason</td>
	</tr>
</table>";

		protected override IM882Provider GetProvider(TextReader reader) => new IM882Provider(new MailBoxItemProvider<Im882>(reader).Message);
	}
}
