using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM482MessageInterpreter))]
	class IM482MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM482MessageInterpreter, IM482Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM484;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM482Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"
A Document Request (IM482) message has been received for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>MRN</td>
		<td>21IEDUB11A782454R2</td>
	</tr>
	<tr>
		<td>LRN</td>
		<td>LRN123456789</td>
	</tr>
	<tr>
		<td>Request Date</td>
		<td>01-Apr-24</td>
	</tr>
	<tr>
		<td>Date Limit</td>
		<td>01-Apr-24</td>
	</tr>
	<tr>
		<td>Document Type</td>
		<td>D001</td>
	</tr>
	<tr>
		<td>Document reference</td>
		<td>DocInfo1</td>
	</tr>
</table>";

		protected override IM482Provider GetProvider(TextReader reader) => new IM482Provider(new Messaging.MailBoxItemProvider<Im482>(reader).Message);
	}
}
