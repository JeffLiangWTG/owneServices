using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageAttacheeMessageHistoryHandlerTest : TestCaseWithFactory
	{
		public void TestGetEntryStatusAndMessageFromMessageHistory_SystemCreateTime()
		{
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0);
			var transactionId = ZGuid.NewZGuid().ToString();

			var previous509Message = Factory.New<AESInboundEDIMessage>();
			previous509Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE509;
			previous509Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(
				transactionID: transactionId, lrn: "LRN123456789", mrn: "21IEDUB11A782454R2", invalidationInitiatedByCustoms: "1", includeResponseWrap: false
			);
			previous509Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-3);
			previous509Message.EM_LinkedObject = entryHeader;
			previous509Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var previous564Message = Factory.New<AESInboundEDIMessage>();
			previous564Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX564;
			previous564Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX564Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-2);
			previous564Message.EM_LinkedObject = entryHeader;
			previous564Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var unknownMessage = Factory.New<AESInboundEDIMessage>();
			unknownMessage.EM_MessageType = "ABC";
			unknownMessage.EM_MessageText = "Something unexpected";
			unknownMessage.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-1);
			unknownMessage.EM_LinkedObject = entryHeader;
			unknownMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			var incoming864Message = Factory.New<AESInboundEDIMessage>();
			incoming864Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX864;
			incoming864Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming864Message.EM_SystemCreateTimeUtc = basicTime;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			var incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);

			var (status, message) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage);
			AssertEquals("Should ignore the 'EM_MessageType = 'ABC' and return the EX564 (CAR)", AESEntryStatusList.Codes.CancellationRequestedByCustoms, status);
			AssertEquals("Should ignore the 'EM_MessageType = 'ABC' and return the EX564", AESIncomingMessageTypeList.Codes.EX564, message.EM_MessageType);
		}

		public void TestGetEntryStatusAndMessageFromMessageHistory_AdditionalComparer()
		{
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0, 123);

			var transactionId = ZGuid.NewZGuid().ToString();
			var lrn = "LRN123456789";
			var mrn = "21IEDUB11A782454R2";

			var previous509Message = Factory.New<AESInboundEDIMessage>();
			previous509Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE509;
			previous509Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(
				transactionID: transactionId, lrn: "LRN123456789", mrn: "21IEDUB11A782454R2", invalidationInitiatedByCustoms: "1", includeResponseWrap: false
			);
			previous509Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-1);
			previous509Message.EM_LinkedObject = entryHeader;
			previous509Message.EM_Status = EDIMessage.Status.ProcessedOK;
			Factory.Save();

			var previous528Message = Factory.New<AESInboundEDIMessage>();
			previous528Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			previous528Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC528CText(lrn, mrn), includeResponseWrap: false);
			previous528Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-2);
			previous528Message.EM_LinkedObject = entryHeader;
			previous528Message.EM_Status = EDIMessage.Status.ProcessedOK;
			Factory.Save();

			var previous564Message = Factory.New<AESInboundEDIMessage>();
			previous564Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX564;
			previous564Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX564Text(lrn), includeResponseWrap: false);
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-2);
			previous564Message.EM_LinkedObject = entryHeader;
			previous564Message.EM_Status = EDIMessage.Status.ProcessedOK;
			Factory.Save();

			var incoming864Message = Factory.New<AESInboundEDIMessage>();
			incoming864Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX864;
			incoming864Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming864Message.EM_SystemCreateTimeUtc = basicTime;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			var incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);

			var status = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(
				entryHeader,
				incomingMessage,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			).status;
			AssertEquals("Retrieve the status from the most recent message (509).", AESEntryStatusList.Codes.Cancelled, status);

			previous509Message.Delete();
			Factory.Save();

			newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);

			status = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage).status;
			AssertEquals(
				"All messages have the same creation time and without an additional comparator, retrieve status from the first valid message in the default sequence(528).",
				AESEntryStatusList.Codes.MrnAllocated,
				status
			);

			status = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(
				entryHeader,
				incomingMessage,
				additionalComparer: new IEEDIMessageComparer(ListSortDirection.Descending)
			).status;
			AssertEquals(
				"When all creation times are the same, with IEEDIMessageComparer, 528 is the logically earliest message, should get status from the other(EX564) message.",
				AESEntryStatusList.Codes.CancellationRequestedByCustoms,
				status
			);
		}

		public void TestGetEntryStatusAndMessageFromMessageHistory_BeforeMessageWithType_SystemCreateTime()
		{
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0);
			var transactionId = ZGuid.NewZGuid().ToString();

			var previous509Message = Factory.New<AESInboundEDIMessage>();
			previous509Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE509;
			previous509Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(
				transactionID: transactionId, lrn: "LRN123456789", mrn: "21IEDUB11A782454R2", invalidationInitiatedByCustoms: "1", includeResponseWrap: false
			);
			previous509Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(30);
			previous509Message.EM_LinkedObject = entryHeader;
			previous509Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var previous564Message = Factory.New<AESInboundEDIMessage>();
			previous564Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX564;
			previous564Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX564Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(31);
			previous564Message.EM_LinkedObject = entryHeader;
			previous564Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var incoming864Message = Factory.New<AESInboundEDIMessage>();
			incoming864Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX864;
			incoming864Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming864Message.EM_SystemCreateTimeUtc = basicTime;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			var incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);

			var (status, message) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.EX564);
			AssertNull("Should not load from historical Messages that are later than the incoming message (status).", status);
			AssertNull("Should not load from historical Messages that are later than the incoming message (message).", message);

			previous509Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-30);
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-29);
			Factory.Save();

			newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest`2" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);
			(status, message) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.EX564);
			AssertEquals("Should return the value after processing the previous IE509 Message (status).", AESEntryStatusList.Codes.Cancelled, status);
			AssertEquals("Should return the value after processing the previous IE509 Message (message).", AESIncomingMessageTypeList.Codes.IE509, message.EM_MessageType);
		}

		public void TestGetEntryStatusAndMessageFromMessageHistory_BeforeMessageWithType_Direction()
		{
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0);
			var transactionId = ZGuid.NewZGuid().ToString();

			var outGoingMessage = Factory.New<AESOutboundEDIMessage>();
			outGoingMessage.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-30);

			var previous509Message = Factory.New<AESInboundEDIMessage>();
			previous509Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE509;
			previous509Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(
				transactionID: transactionId, lrn: "LRN123456789", mrn: "21IEDUB11A782454R2", invalidationInitiatedByCustoms: "1", includeResponseWrap: false
			);
			previous509Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-29);
			previous509Message.EM_LinkedObject = entryHeader;
			previous509Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var previous564Message = Factory.New<AESInboundEDIMessage>();
			previous564Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX564;
			previous564Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX564Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-28);
			previous564Message.EM_LinkedObject = entryHeader;
			previous564Message.EM_Status = EDIMessage.Status.ProcessedOK;

			var incoming864Message = Factory.New<AESInboundEDIMessage>();
			incoming864Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX864;
			incoming864Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming864Message.EM_SystemCreateTimeUtc = basicTime;

			Factory.Save();
			var newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			var incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming864Message.PK);
			var (status, message) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.EX564);
			AssertEquals("Should return the value after processing the previous IE509 Message (status).", AESEntryStatusList.Codes.Cancelled, status);
			AssertEquals("Should return the value after processing the previous IE509 Message (message).", AESIncomingMessageTypeList.Codes.IE509, message.EM_MessageType);
		}

		public void TestGetEntryStatusFromMessageHistory()
		{
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0);
			var transactionId = ZGuid.NewZGuid().ToString();

			var previous528Message = Factory.New<AESInboundEDIMessage>();
			previous528Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			previous528Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2"), includeResponseWrap: false);
			previous528Message.EM_LinkedObject = entryHeader;
			previous528Message.EM_Status = EDIMessage.Status.ProcessedOK;
			previous528Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-10);

			var incoming560Message = Factory.New<AESInboundEDIMessage>();
			incoming560Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE560;
			incoming560Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC560CText("LRN123456789", "21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming560Message.EM_LinkedObject = entryHeader;
			incoming560Message.EM_Status = EDIMessage.Status.ProcessedOK;
			incoming560Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(-5);

			var incoming529Message = Factory.New<AESInboundEDIMessage>();
			incoming529Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE529;
			incoming529Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, AESInterchangeProcessorTestHelper.GetStandardCC529CText("LRN123456789", "21IEDUB11A782454R2"), includeResponseWrap: false);
			incoming529Message.EM_LinkedObject = entryHeader;
			incoming529Message.EM_Status = EDIMessage.Status.ProcessedOK;
			incoming529Message.EM_SystemCreateTimeUtc = basicTime;

			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Enterprise.Customs.IE.Business.Testing.EntryHeaderMessageHistoryHandlerTest" };
			entryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			var incomingMessage = newFactory.Load<AESInboundEDIMessage>(incoming529Message.PK);

			var (status, _) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.IE528);
			var status2 = MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.IE528);
			AssertEquals(status, status2);

			(status, _) = MessageAttacheeMessageHistoryHandler.GetEntryStatusAndMessageFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.IE529);
			status2 = MessageAttacheeMessageHistoryHandler.GetEntryStatusFromMessageHistory(entryHeader, incomingMessage, AESIncomingMessageTypeList.Codes.IE529);
			AssertEquals(status, status2);
		}
	}
}
