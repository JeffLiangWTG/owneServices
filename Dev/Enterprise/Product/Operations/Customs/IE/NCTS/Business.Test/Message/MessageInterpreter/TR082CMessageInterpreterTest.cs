using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR082C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR082CMessageInterpreter))]
	class TR082CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR082CMessageInterpreter, TR082CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR082C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var messageText = InterchangeProcessorTestHelper.GetStandardTR082CText("23IE01ROS654321", "00234567");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"
			A Documents Request (TR082) message has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>23IE01ROS654321</td></tr>
				<tr><td>LRN</td><td>00234567</td></tr>
				<tr><td>Request Date</td><td>18-Sep-71 00:00</td></tr>
				<tr><td>Date Limit</td><td>18-Oct-71 00:00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Additional Information Document Type</td><td>Y022</td></tr>
				<tr><td>Additional Information Document Complementary Information</td><td>Info 1</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Additional Information Document Type</td><td>Y029</td></tr>
				<tr><td>Additional Information Document Complementary Information</td><td>Info 2</td></tr>
			</table>";

		protected override TR082CProvider GetProvider(TextReader reader) => new TR082CProvider(new MailBoxItemProvider<Tr082C>(reader).Message);
	}
}
