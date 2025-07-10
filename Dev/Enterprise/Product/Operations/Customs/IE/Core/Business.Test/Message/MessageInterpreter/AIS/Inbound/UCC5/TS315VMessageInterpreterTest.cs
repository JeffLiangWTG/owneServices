using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using TS315VProvider = Enterprise.Customs.IE.Messaging.UCC5.TS315VProvider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS315VMessageInterpreter))]
	sealed class TS315VMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS315VMessageInterpreter, TS315VProvider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS315V;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISUCC5InterchangeProcessorTestHelper.GetTS315VText("21IEDUB11A782454R2", "LRN001", new DateTime(2023, 9, 5, 12, 30, 30))
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Temporary Storage Declaration Registration Acceptance message (TS315V) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>05-Sep-23</td></tr></table>";

		protected override TS315VProvider GetProvider(TextReader reader) => new TS315VProvider(new MailBoxItemProvider<Ts315V>(reader).Message);
	}
}
