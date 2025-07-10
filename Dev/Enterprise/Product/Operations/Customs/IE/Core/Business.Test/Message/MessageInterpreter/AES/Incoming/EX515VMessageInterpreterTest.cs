using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX515VMessageInterpreter))]
	class EX515VMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX515VMessageInterpreter, EX515VProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX515V;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Export Declaration Acknowledgement message has been received from Customs for Job B00000012 through the EX515V message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Acknowledged by Customs(MRN Allocated)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>22-Dec-20 00:00</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex515vText = AESInterchangeProcessorTestHelper.GetStandardEX515VText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex515vText, includeResponseWrap: false);

			return message;
		}

		protected override EX515VProvider GetProvider(TextReader reader) => new EX515VProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX515V.Ex515V>(reader).Message);
	}
}
