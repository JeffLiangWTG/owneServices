using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX562;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX562MessageInterpreter))]
	class EX562MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX562MessageInterpreter, EX562Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX562;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Amendment Request message has been received from Customs for Job B00000012 through the EX562 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASE ID</td></tr><tr><td>Remarks</td><td>REMARKS</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex562Text = AESInterchangeProcessorTestHelper.GetStandardEX562Text("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex562Text, includeResponseWrap: false);

			return message;
		}

		protected override EX562Provider GetProvider(TextReader reader) => new EX562Provider(new MailBoxItemProvider<Ex562>(reader).Message);
	}
}
