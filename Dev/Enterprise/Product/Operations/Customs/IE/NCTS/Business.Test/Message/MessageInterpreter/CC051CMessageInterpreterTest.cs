using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC051C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC051CMessageInterpreter))]
	class CC051CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC051CMessageInterpreter, CC051CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE051;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A No Release for Transit (IE051) Message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19AA12345678901230</td></tr><tr><td>Declaration Submission Date And Time</td><td>17-Feb-23 00:00</td></tr><tr><td>No Release Motivation Code</td><td>CA</td></tr><tr><td>No Release Motivation Text</td><td>No Release</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc051Text = InterchangeProcessorTestHelper.GetStandardCC051CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc051Text, includeResponseWrap: false);

			return message;
		}

		protected override CC051CProvider GetProvider(TextReader reader) => new CC051CProvider(new MailBoxItemProvider<Cc051CType>(reader).Message);
	}
}
