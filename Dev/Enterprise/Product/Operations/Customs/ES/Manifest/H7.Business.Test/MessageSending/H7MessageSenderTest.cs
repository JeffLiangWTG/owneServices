using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class H7MessageSenderTest : TestCaseWithFactory
	{
		public void TestGetMessageBuildersData()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var billUnimplementedType = header.Bills.AddNew();
			var sendingObjectUnimplementedType = new H7MessageSendingObject(billUnimplementedType);
			sendingObjectUnimplementedType.Action = "ABC";

			var billValid = header.Bills.AddNew();
			var sendingObjectValid = new H7MessageSendingObject(billValid);
			sendingObjectValid.Action = DeclarationMessageTypeList.Codes.H7Query;

			var sendingObjects = new List<H7MessageSendingObject>() { sendingObjectValid };
			var sender = new H7MessageSender(sendingObjects);
			var messageBuilderData = sender.GetMessageBuildersData();

			CombineAssertions(() =>
			{
				AssertEquals("The returned messagebuildersdata list has 1 elements", 1, messageBuilderData.Count);
				AssertEquals("The messagebuildersdata element has FailureFlag false", false, messageBuilderData[0].FailureFlag);
				AssertNotNull("The messagebuildersdata element has MessageBuilder not null", messageBuilderData[0].MessageBuilder);
				AssertNotNull("The messagebuildersdata element has Bill not null", messageBuilderData[0].Bill);
				AssertNotNull("The messagebuildersdata element has sending object not null", messageBuilderData[0].SendingObject);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
			});

			sendingObjects = new List<H7MessageSendingObject>() { sendingObjectUnimplementedType };
			sender = new H7MessageSender(sendingObjects);
			messageBuilderData = sender.GetMessageBuildersData();

			CombineAssertions(() =>
			{
				AssertEquals("The returned messagebuildersdata list has 1 elements", 1, messageBuilderData.Count);
				AssertEquals("The messagebuildersdata element has FailureFlag false", true, messageBuilderData[0].FailureFlag);
				AssertEquals("LastKeyReported has exception", "H7MessageSender.GetIndividualMessageBuilder", ErrorReporter.LastKeyReported);
			});
			ErrorReporter.Clear();
		}

		public void TestQueryH7SendMessage()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill = header.Bills.AddNew();
			var sendingObject = new H7MessageSendingObject(bill);
			sendingObject.Action = DeclarationMessageTypeList.Codes.H7Query;

			var sendingObjects = new List<H7MessageSendingObject>() { sendingObject };
			var sender = new H7MessageSender(sendingObjects);
			var messageBuilderData = sender.GetMessageBuildersData();
			var messages = new List<ESEDIMessage>();

			var sendInfo = sender.Send(messageBuilderData, messages);

			CombineAssertions(() =>
			{
				AssertEquals("Bill message status should be sent", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				AssertEquals("1 message should be sent", 1, sendInfo.MessagesSent);
				AssertEquals("No message send failure", 0, sendInfo.MessagesWithSendFailure);
				AssertEquals("No message create failure", 0, sendInfo.MessagesWithCreateFailure);
				AssertSame("Message should reference bill", bill, messages[0].EM_LinkedObject);
				AssertEquals("Message type should be H7Q", messages[0].EM_MessageType, DeclarationMessageTypeList.Codes.H7Query);
			});
		}

		public void TestCommonAnnexH7SendMessage()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill = header.Bills.AddNew();
			var sendingObject = new UploadDocumentsSendingAction(bill);
			sendingObject.Action = DeclarationMessageTypeList.Codes.H7Annexes;

			var h7CusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			h7CusEntryNum.CE_ParentTable = "AsycudaBill";
			h7CusEntryNum.CE_ParentID = bill.PK;
			h7CusEntryNum.CE_EntryType = "MRN";
			h7CusEntryNum.CE_EntryLineReference = "H7";
			h7CusEntryNum.CE_RN_NKCountryCode = "ES";
			h7CusEntryNum.CE_EntryNum = "H7123";

			var sendingObjects = new List<IH7CommonMessageSendingObject>() { sendingObject };
			var sender = new H7MessageSender(sendingObjects);
			var messages = new List<ESEDIMessage>();
			var supportingDocument1 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc1.pdf", "CIV");
			var supportingDocument2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc2.pdf", "CIV");
			var requestedDocument1 = bill.RequestedDocuments.AddNew();
			var requestedDocument2 = bill.RequestedDocuments.AddNew();
			requestedDocument1.CSI_Status = "OPE";
			requestedDocument2.CSI_Status = "OPE";

			var docSendingObject1 = sendingObject.AddInfoCollection[0].EDocsCollection.AddNew();
			docSendingObject1.EDoc = supportingDocument1.UniqueKey;
			var docSendingObject2 = sendingObject.AddInfoCollection[0].EDocsCollection.AddNew();
			docSendingObject2.EDoc = supportingDocument2.UniqueKey;

			var messageBuilderData = sender.GetMessageBuildersData();
			var sendInfo = sender.Send(messageBuilderData, messages);

			CombineAssertions(() =>
			{
				AssertEquals("Bill message status should be sent", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				AssertEquals("1 message should be sent", 2, sendInfo.MessagesSent);
				AssertEquals("No message send failure", 0, sendInfo.MessagesWithSendFailure);
				AssertEquals("No message create failure", 0, sendInfo.MessagesWithCreateFailure);
				AssertSame("First message should reference bill", bill, messages[0].EM_LinkedObject);
				AssertSame("Second Message should reference bill", bill, messages[1].EM_LinkedObject);
				AssertEquals("Message type should be H7A", messages[0].EM_MessageType, DeclarationMessageTypeList.Codes.H7Annexes);
				AssertEquals("Message type should be H7A", messages[1].EM_MessageType, DeclarationMessageTypeList.Codes.H7Annexes);
			});
		}

		public void TestSendMessageBuilderDataWithoutMessageBuilder()
		{
			var sender = GetMessageSenderWithSendingObjectAction(DeclarationMessageTypeList.Codes.H7Query);
			var messageBuilderData = sender.GetMessageBuildersData();
			messageBuilderData[0].MessageBuilder = null;

			var sendInfo = sender.Send(messageBuilderData);
			CombineAssertions(() =>
			{
				AssertEquals("Message send failure", 1, sendInfo.MessagesWithSendFailure);
				AssertEquals("LastKeyReported has exception", "H7MessageSender.Send", ErrorReporter.LastKeyReported);
			});
			ErrorReporter.Clear();
		}

		public void TestSendMessageBuilderDataWithFailureFlag()
		{
			var sender = GetMessageSenderWithSendingObjectAction(DeclarationMessageTypeList.Codes.H7Query);
			var messageBuilderData = sender.GetMessageBuildersData();
			messageBuilderData[0].FailureFlag = true;

			var sendInfo = sender.Send(messageBuilderData);
			CombineAssertions(() =>
			{
				AssertEquals("Message send failure", 1, sendInfo.MessagesWithSendFailure);
				AssertEquals("LastKeyReported is empty", ZString.Empty, ErrorReporter.LastKeyReported);
			});
		}

		public void TestSend_ShowProgress()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObject1 = new H7MessageSendingObject(bill1);
			sendingObject1.Action = DeclarationMessageTypeList.Codes.H7Query;

			var sendingObject2 = new H7MessageSendingObject(bill2);
			sendingObject2.Action = DeclarationMessageTypeList.Codes.H7Query;

			var progressUpdateLogs = new List<(int ProcessedMessages, int MessagesToProcess)>();

			Action<int, int> progressUpdateCallback = (message, progress) =>
			{
				progressUpdateLogs.Add((message, progress));
			};

			var sender = new H7MessageSender(new H7MessageSendingObject[] { sendingObject1, sendingObject2 }, progressUpdateCallback);
			var messageBuilderData = sender.GetMessageBuildersData();

			var sendInfo = sender.Send(messageBuilderData);

			CombineAssertions(() =>
			{
				AssertEquals("2 messages should be sent", 2, sendInfo.MessagesSent);
				AssertEquals("Message should be sent for bill 1", 1, bill1.Messages.Count);
				AssertEquals("Message should be sent for bill 2", 1, bill2.Messages.Count);

				AssertEquals("Expected logs count", 2, progressUpdateLogs.Count);

				AssertEquals("First log's ProcessedMessages", 1, progressUpdateLogs[0].ProcessedMessages);
				AssertEquals("First log's MessagesToProcess", 2, progressUpdateLogs[0].MessagesToProcess);

				AssertEquals("Second log's ProcessedMessages", 2, progressUpdateLogs[1].ProcessedMessages);
				AssertEquals("Second log's MessagesToProcess", 2, progressUpdateLogs[1].MessagesToProcess);
			});
		}

		H7MessageSender GetMessageSenderWithSendingObjectAction(ZString sendingObjectAction)
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill = header.Bills.AddNew();
			var sendingObject = new H7MessageSendingObject(bill);
			sendingObject.Action = sendingObjectAction;

			var sendingObjects = new List<H7MessageSendingObject>() { sendingObject };
			var sender = new H7MessageSender(sendingObjects);
			return sender;
		}
	}
}
