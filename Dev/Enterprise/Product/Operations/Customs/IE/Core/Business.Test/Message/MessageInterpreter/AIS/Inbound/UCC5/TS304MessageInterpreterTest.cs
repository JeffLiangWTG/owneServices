using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using TS304Provider = Enterprise.Customs.IE.Messaging.UCC5.TS304Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS304MessageInterpreter))]
	sealed class TS304MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, TS304MessageInterpreter, TS304Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS304;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest() => AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(
			Factory,
			messageText: AISUCC5InterchangeProcessorTestHelper.GetTS304Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 30), "Test Remarks")
		);

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Temporary Storage Declaration Amendment Request Registration (TS304) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>05-Sep-23 00:00</td></tr><tr><td>Remarks</td><td>Test Remarks</td></tr></table>";

		protected override TS304Provider GetProvider(TextReader reader) => new TS304Provider(new MailBoxItemProvider<Ts304>(reader).Message);
	}
}
