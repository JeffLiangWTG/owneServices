using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR015V;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR015MessageInterpreter))]
	class TR015MessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR015MessageInterpreter, TR015VProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR015V;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr015Text = InterchangeProcessorTestHelper.GetStandardTR015VText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr015Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Transit Pre-lodged Declaration Acknowledgment message (TR015V) has been received. Customs has acknowledged receipt of a Pre-Lodged Transit Declaration for Job B00000012<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>LRN</td><td>LRNCC060C0123456789012</td></tr><tr><td>Declaration Acknowledgement Date</td><td>31-Jan-23 10:22</td></tr></table>";

		protected override TR015VProvider GetProvider(TextReader reader) => new TR015VProvider(new MailBoxItemProvider<Tr015V>(reader).Message);
	}
}
