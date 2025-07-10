using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using TS328Provider = Enterprise.Customs.IE.Messaging.UCC5.TS328Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS328MessageInterpreter))]
	sealed class TS328MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS328MessageInterpreter, TS328Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS328;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISUCC5InterchangeProcessorTestHelper.GetTS328Text("21IEDUB11A782454R2", "LRN001", new DateTime(2023, 9, 5, 12, 30, 30))
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A TSD Acceptance message (TS328) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>05-Sep-23</td></tr><tr><td>Response Date Limit</td><td>05-Oct-23</td></tr><tr><td>Remarks</td><td>remarks</td></tr></table>";

		protected override TS328Provider GetProvider(TextReader reader) => new TS328Provider(new MailBoxItemProvider<Ts328>(reader).Message);
	}
}
