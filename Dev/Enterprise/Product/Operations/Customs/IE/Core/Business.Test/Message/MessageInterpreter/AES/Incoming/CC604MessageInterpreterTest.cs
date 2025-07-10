using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC604C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC604MessageInterpreter))]
	sealed class CC604MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC604MessageInterpreter, CC604CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE604;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc604Text = AESInterchangeProcessorTestHelper.GetStandardAESCC604CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc604Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Exit Summary Declaration Amendment Acceptance message has been received from Customs for Job B00000012 through the IE604 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Amendment Accepted by Customs (ACC)</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>19-Sep-71 00:00</td></tr></table>";

		protected override CC604CProvider GetProvider(TextReader reader) => new CC604CProvider(new MailBoxItemProvider<Cc604C>(reader).Message);
	}
}
