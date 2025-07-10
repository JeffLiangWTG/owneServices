using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TCPMessage))]
	sealed class TCPMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOriginalMessage()
		{
			var batchNumber = "BATCH20200326";
			var sentMessage1 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAEXP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, new ZDateTime(2020, 03, 26), batchNumber);
			var sentMessage2 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.DataLoadingModule, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, new ZDateTime(2020, 03, 26), batchNumber);
			var sentMessage3 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Receive, EDIMessage.Status.Sent, new ZDateTime(2020, 03, 26), batchNumber);
			var sentMessage4 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Transmit, EDIMessage.Status.Queued, new ZDateTime(2020, 03, 26), batchNumber);
			var sentMessage5 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, new ZDateTime(2020, 01, 26), batchNumber);
			var sentMessage6 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, new ZDateTime(2020, 03, 26), "NOBATCH");
			var sentMessage7 = CreateSentMessageForTest(EDIMessage.ApplicationCodes.CAIMP, MessageTypeList.Codes.TradeChainPartner, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, new ZDateTime(2020, 03, 26), batchNumber);
			var receivedMessage = Factory.New<TCPMessage>();
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_SystemCreateTimeUtc = new ZDateTime(2020, 03, 27);
			var messageText = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::2000+{0}'
DTM+9:201704271045:203'
GIS+1'
RFF+ZZZ:102385036'
UNT+6+1'
", 6);
			receivedMessage.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			Factory.Save();

			AssertEquals(sentMessage7.PK, receivedMessage.OriginalMessage.PK);
		}

		EDIMessageWithBatchNumber CreateSentMessageForTest(string applicationCode, string messageType, string direction, string status, ZDateTime createTime, string batchNumber)
		{
			var sentMessage = Factory.New<EDIMessageWithBatchNumber>();
			sentMessage.EM_ApplicationCode = applicationCode;
			sentMessage.EM_MessageType = messageType;
			sentMessage.EM_ReceiveTransmit = direction;
			sentMessage.EM_Status = status;
			sentMessage.EM_SystemCreateTimeUtc = createTime;
			sentMessage.EM_MessageNum = batchNumber;
			var messageText = string.Format(@"
UNH+{0}+CUSPED:S:99B:UN'
BGM+101+{0}+2'
CST++588:105+MASTERORG:58+TCPORG:58'
DTM+9:201910211425:203'
DMS++10'
RFF+ZZZ:0118802EXP'
DOC+10'
NAD+UC+++MCCAY TOOL & ENGINEERING CO. INC:::::1+1449 WEST LARK INDUSTRIAL+FENTON+MO:163:5+63026+CA'
UNT+9+000588'
", batchNumber);
			sentMessage.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");

			return sentMessage;
		}
	}
}
