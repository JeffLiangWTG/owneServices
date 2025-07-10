using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class TCPResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestProcessAcceptedMessage()
		{
			Setup();

			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::2000+{0}'
DTM+9:201704271045:203'
GIS+1'
RFF+ZZZ:102385036'
UNT+6+1'
", sentMessage.EM_MessageNum);

			var receivedMessage = GetEDIMessage<TCPMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = master.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			processor.ProcessMessage(receivedMessage);
			AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals(CSAStatusList.Codes.Added, tcp.CA_CSAStatus);

			var wrapper = OrgHeaderTCPMessageWrapper.New(master);
			AssertNotNull(wrapper.Messages);
			AssertEquals(2, wrapper.Messages.Count);
			AssertEquals(true, wrapper.Messages.Contains(receivedMessage.PK));
		}

		public void TestProcessRejectedMessage()
		{
			Setup();

			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::1000+{0}'
DTM+9:201709131730:203'
GIS+14'
ERP+2:458791:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTNAD-BYTE OFFSET117'
FTX+AAO+++SEGMENTNADLINE7ELE POS8,0MAND ELEM MISSING'
UNT+9+1'
", sentMessage.EM_MessageNum);

			var receivedMessage = GetEDIMessage<TCPMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = master.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			processor.ProcessMessage(receivedMessage);
			AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals(CSAStatusList.Codes.ErrorAdded, tcp.CA_CSAStatus);

			var wrapper = OrgHeaderTCPMessageWrapper.New(master);
			AssertNotNull(wrapper.Messages);
			AssertEquals(2, wrapper.Messages.Count);
			AssertEquals(true, wrapper.Messages.Contains(receivedMessage.PK));
		}

		public void TestProcessInvalidMessage()
		{
			Setup();
			var acceptedContent = string.Format(@"
UNH+1+CUSRES:S:99B:UN'
BGM+:::1000+{0}'
DTM+9:201709131730:203'
ERP+2:458791:28'
ERC+ZZZ'
FTX+AAO+++SEGMENTNAD-BYTE OFFSET117'
FTX+AAO+++SEGMENTNADLINE7ELE POS8,0MAND ELEM MISSING'
UNT+9+1'
", sentMessage.BatchNumber);

			var receivedMessage = GetEDIMessage<TCPMessage>(acceptedContent);
			receivedMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			receivedMessage.EM_Status = EDIMessage.Status.Queued;
			receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receivedMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			receivedMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			receivedMessage.EM_MessageNum = sentMessage.EM_MessageNum;
			receivedMessage.EM_LinkUniqueID = master.PK;
			receivedMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			AssertExceptionThrown<UnableToInterpretMessageException>(() => processor.ProcessMessage(receivedMessage));
		}

		TCPResponseMessageProcessor processor;
		OrgHeader master, tcpOrg;
		OrgAddress tcpAddress;
		OrgImpAddInfo orgImp;
		TradeChainPartner tcp;
		string sentContent;
		TCPMessage sentMessage;
		string batchNumber;

		void Setup()
		{
			processor = new TCPResponseMessageProcessor(logger);
			master = Factory.New<OrgHeader>();
			master.OH_Code = "MASTERORG";

			tcpOrg = Factory.New<OrgHeader>();
			tcpOrg.OH_Code = "TCPORG";
			tcpAddress = tcpOrg.Addresses.AddNew();
			tcpAddress.Address1 = "TEST ADDRESS";

			orgImp = OrgImpAddInfo.Get(master);
			tcp = orgImp.TradeChainPartners.AddNew();
			tcp.CA_Org = tcpOrg.PK;
			tcp.CA_CSAID = "TESTCSAID";
			tcp.CA_CSAStatus = CSAStatusList.Codes.AwaitingAdd;

			batchNumber = "BATCHNUMBER";
			sentContent = string.Format(@"
UNH+{0}+CUSPED:S:99B:UN'
BGM+101+{1}+2'
CST++588:105+MASTERORG:58+TCPORG:58'
DTM+9:201910211425:203'
DMS++10'
RFF+ZZZ:TESTCSAID'
DOC+10'
NAD+UC+++MCCAY TOOL & ENGINEERING CO. INC:::::1+1449 WEST LARK INDUSTRIAL+FENTON+MO:163:5+63026+CA'
UNT+9+000588'
", EDIMessage.MessageNumberPlaceHolder, batchNumber);

			sentMessage = GetEDIMessage<TCPMessage>(sentContent);
			sentMessage.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			sentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
			sentMessage.EM_LinkTable = OrgHeader.Schema.TableName;
			sentMessage.EM_LinkUniqueID = master.PK;
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			sentMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			Factory.Save();
		}
	}
}
