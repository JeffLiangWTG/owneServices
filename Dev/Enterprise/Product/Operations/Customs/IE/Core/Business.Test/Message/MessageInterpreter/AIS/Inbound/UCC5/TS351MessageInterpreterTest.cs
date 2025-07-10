using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using TS351Provider = Enterprise.Customs.IE.Messaging.UCC5.TS351Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS351MessageInterpreter))]
	sealed class TS351MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS351MessageInterpreter, TS351Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS351;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISUCC5InterchangeProcessorTestHelper.GetTS351Text("21IEDUB11A782454R2", "LRN001", new DateTime(2023, 9, 5, 12, 30, 30), "remarks")
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Temporary Storage Refusal (TS351) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Control Result Date</td><td>05-Sep-23</td></tr><tr><td>Control Result Remarks</td><td>control remarks</td></tr><tr><td>Remarks</td><td>remarks</td></tr></table>";

		protected override TS351Provider GetProvider(TextReader reader) => new TS351Provider(new MailBoxItemProvider<Ts351>(reader).Message);
	}
}
