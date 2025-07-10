using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class RSFResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestProcessAcceptedMessage()
		{
			Setup();

			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::2010+{0}'
DTM+9:201704271045:203'
GIS+1'
RFF+ZZZ:102385036'
UNT+6+1'
", rsf.B2_StatementNumber);

			var receivedMessage = GetEDIMessage<RSFMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.CSARevenueSummaryForm;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = rsf.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			processor.ProcessMessage(receivedMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message Status", EDIMessage.Status.Received, receivedMessage.EM_Status);
				AssertEquals("RSF Status", MessageStatusList.Codes.AcknowledgedOriginal, rsf.B2_Status);
				AssertEquals("RSF Process Date", "27-Apr-17", rsf.B2_ProcessDate.ToShortDateString());
				AssertEquals("Message Linked Object Table", receivedMessage.EM_LinkTable, CusStatementHeader.Schema.TableName);
				AssertEquals("Message Linked Object PK", receivedMessage.EM_LinkUniqueID, rsf.PK);
			});
		}

		public void TestProcessRejectedMessage()
		{
			Setup();

			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::1010+{0}'
DTM+9:201709131730:203'
GIS+14'
ERP+2:458791:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTNAD-BYTE OFFSET117'
FTX+AAO+++SEGMENTNADLINE7ELE POS8,0MAND ELEM MISSING'
UNT+9+1'
", rsf.B2_StatementNumber);

			var receivedMessage = GetEDIMessage<RSFMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.CSARevenueSummaryForm;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = rsf.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			processor.ProcessMessage(receivedMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message Status", EDIMessage.Status.Received, receivedMessage.EM_Status);
				AssertEquals("RSF Status", MessageStatusList.Codes.ErrorOriginal, rsf.B2_Status);
				AssertEquals("RSF Process Date", ZString.Empty, rsf.B2_ProcessDate.ToShortDateString());
				AssertEquals("Message Linked Object Table", receivedMessage.EM_LinkTable, CusStatementHeader.Schema.TableName);
				AssertEquals("Message Linked Object PK", receivedMessage.EM_LinkUniqueID, rsf.PK);
			});
		}

		public void TestProcessInvalidMessage()
		{
			Setup();
			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::1010+{0}'
DTM+9:201709131730:203'
ERP+2:458791:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTNAD-BYTE OFFSET117'
FTX+AAO+++SEGMENTNADLINE7ELE POS8,0MAND ELEM MISSING'
UNT+9+1'
", rsf.B2_StatementNumber);

			var receivedMessage = GetEDIMessage<RSFMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.CSARevenueSummaryForm;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = rsf.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			AssertExceptionThrown<UnableToInterpretMessageException>(() => processor.ProcessMessage(receivedMessage));
		}

		RSFResponseMessageProcessor processor;
		CusStatementHeader rsf;
		string sentContent;
		RSFMessage sentMessage;

		void Setup()
		{
			processor = new RSFResponseMessageProcessor(logger);
			rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_Status = MessageStatusList.Codes.AwaitingOriginal;
			rsf.B2_StatementNumber = "BATCHNUMBER";
			sentContent = string.Format(@"
UNH+{0}+CUSPED:S:99B:UN'
BGM+101+{1}+2'
CST++588:105+MASTERORG:58+RSFORG:58'
DTM+9:201910211425:203'
DMS++10'
RFF+ZZZ:0118802EXP'
DOC+10'
NAD+UC+++MCCAY TOOL & ENGINEERING CO. INC:::::1+1449 WEST LARK INDUSTRIAL+FENTON+MO:163:5+63026+CA'
UNT+9+000588'
", EDIMessage.MessageNumberPlaceHolder, rsf.B2_StatementNumber);

			sentMessage = GetEDIMessage<RSFMessage>(sentContent);
			sentMessage.EM_MessageType = MessageTypeList.Codes.CSARevenueSummaryForm;
			sentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			sentMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			sentMessage.EM_LinkUniqueID = rsf.PK;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			Factory.Save();
		}
	}
}
