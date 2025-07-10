using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR062C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR062CMessageInterpreter))]
	class TR062CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR062CMessageInterpreter, TR062CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR062C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr054Text = InterchangeProcessorTestHelper.GetStandardTR062CText("19MRNCC060C0123456");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr054Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Request Declaration Amendment (TR062) message has been received for Job B00000012.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19MRNCC060C0123456</td></tr>
				<tr><td>Case Id</td><td>ID123456</td></tr>
				<tr><td>Remarks</td><td>Remarks</td></tr>
			</table>";

		protected override TR062CProvider GetProvider(TextReader reader) => new TR062CProvider(new MailBoxItemProvider<Tr062C>(reader).Message);
	}
}
