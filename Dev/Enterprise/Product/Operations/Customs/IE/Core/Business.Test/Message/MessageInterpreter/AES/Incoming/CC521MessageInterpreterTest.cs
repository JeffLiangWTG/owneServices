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
	[TestedType(typeof(CC521MessageInterpreter))]
	class CC521MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC521MessageInterpreter, CC521CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE521;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Export Diversion Rejection message has been received from Customs on Job B00000012 through the IE521 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REJ - Exit Released Rejected</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Diversion Rejection Reason Code</td><td>11 - Cancelled</td></tr><tr><td>Diversion Rejection Text</td><td>Because bad data</td></tr><tr><td>Customs office of Exit (Actual)</td><td>REFNO123</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc521Text = AESInterchangeProcessorTestHelper.GetStandardCC521CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc521Text, includeResponseWrap: false);
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.DiversionRejectionCode, "11", "Cancelled");
			return message;
		}

		protected override CC521CProvider GetProvider(TextReader reader) => new CC521CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C.Cc521C>(reader).Message);
	}
}
