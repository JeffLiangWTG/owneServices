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
	[TestedType(typeof(CC525MessageInterpreter))]
	class CC525MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC525MessageInterpreter, CC525CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE525;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000525";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc525Text = AESInterchangeProcessorTestHelper.GetStandardAESCC525CText("21IEDUB11A782454R2", "0");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc525Text, includeResponseWrap: false);
			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Exit Release notification message has been received from Customs for Job B00000525 through the IE525 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Released for Exit</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Release Date</td><td>18-Sep-71</td></tr><tr><td>Storing Flag</td><td>N</td></tr></table>";

		protected override CC525CProvider GetProvider(TextReader reader) => new CC525CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC525C.Cc525C>(reader).Message);
	}
}
