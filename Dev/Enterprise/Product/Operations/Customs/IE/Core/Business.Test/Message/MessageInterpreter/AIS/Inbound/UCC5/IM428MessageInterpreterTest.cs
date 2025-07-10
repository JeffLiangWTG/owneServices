using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM428Provider = Enterprise.Customs.IE.Messaging.UCC5.IM428Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM428MessageInterpreter))]
	class IM428MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM428MessageInterpreter, IM428Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM428;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISInterchangeProcessorTestHelper.GetStandardUCC5IM428Text()
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => DefaultInterpretation;

		public const string DefaultInterpretation = @"An Export MRN Allocation (IM428) message has been received from customs for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>Status</td>
		<td>Accepted by Customs (MRN Allocated)</td>
	</tr>
	<tr>
		<td>LRN</td>
		<td>LRN123456789</td>
	</tr>
	<tr>
		<td>MRN</td>
		<td>21IEDUB11A782454R2</td>
	</tr>
	<tr>
		<td>Acceptance Date</td>
		<td>20240222</td>
	</tr>
</table>";

		protected override IM428Provider GetProvider(TextReader reader) => new IM428Provider(new MailBoxItemProvider<Im428>(reader).Message);
	}
}
