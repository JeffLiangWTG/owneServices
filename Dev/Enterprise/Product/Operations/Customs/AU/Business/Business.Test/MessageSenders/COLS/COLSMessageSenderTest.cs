using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendLodgementMessage() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "INVOICE";
			Factory.Save();

			var senderProvider = new COLSMessageSender(colsHeader, "XYZ");
			var dialogMessage = senderProvider.SendLodgementMessage(colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray());
			AssertEquals("Dialog Message is empty", string.Empty, dialogMessage);
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("Lodgement and attachment messages generated", 2, colsHeader.Messages.Count);
			var attachmentMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddAttachment);
			AssertEquals("Attachment message status", EDIMessage.Status.Pending, attachmentMessage.EM_Status);
			var lodgementMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddNewLodgement);
			AssertEquals("Lodgement message status", EDIMessage.Status.Queued, lodgementMessage.EM_Status);
			AssertContains("Additional Comment is included in the lodgement message", "\"additionalComment\":\"XYZ\"", lodgementMessage.EM_MessageText);
		});

		public void TestSendAttachmentMessages() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var docPivot1 = colsHeader.EDocPivotCollection.AddNew();
			docPivot1.CSD_DocType = "INVOICE";
			docPivot1.CSD_Description = "DocPivot #1";
			docPivot1.CSD_StorageDocReference = ZGuid.NewZGuid();
			var docPivot2 = colsHeader.EDocPivotCollection.AddNew();
			docPivot2.CSD_DocType = "INVOICE";
			docPivot2.CSD_Description = "DocPivot #2";
			docPivot2.CSD_StorageDocReference = ZGuid.NewZGuid();
			docPivot2.CSD_MessageStatus = COLSDocumentStatusList.Codes.SuccessfulDocumentSent;
			var docPivot3 = colsHeader.EDocPivotCollection.AddNew();
			docPivot3.CSD_DocType = "INVOICE";
			docPivot3.CSD_Description = "DocPivot #3";
			docPivot3.CSD_StorageDocReference = ZGuid.NewZGuid();
			Factory.Save();

			var senderProvider = new COLSMessageSender(colsHeader, "");
			var dialogMessage = senderProvider.SendAttachmentMessages(colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray());
			AssertEquals("Dialog Message", "3 attachment message(s) generated", dialogMessage);
			AssertEquals("QCH_MessageStatus", COLSHeaderStatusList.Codes.AwaitingAddAttachmentResponse, colsHeader.QCH_MessageStatus);
			var generatedMessages = colsHeader.Messages.Cast<COLSMessage>().ToArray();
			AssertContainsExactElementsInAnyOrder("3 messages generated with status QUE and PND", new[] { EDIMessage.Status.Queued, EDIMessage.Status.Pending, EDIMessage.Status.Pending }, generatedMessages.Select(x => x.EM_Status));
			AssertContainsExactElementsInAnyOrder("3 messages generated from docPivot1 and docPivot3", new[] { docPivot1.PK, docPivot2.PK, docPivot3.PK }, generatedMessages.Select(x => x.EM_LinkUniqueID));
		});

		public void TestSendAdditionalDocumentMessage() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "LRN123";
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var entryNumObject = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_EntryNum = "LRN234";
			entryNumObject.CE_Category = "CUS";
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "INVOICE";

			var senderProvider = new COLSMessageSender(colsHeader, "");
			senderProvider.SendAdditionalDocumentMessage(colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray());
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingAddAdditionalDocumentResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("AdditionalDocument and attachment messages generated", 2, colsHeader.Messages.Count);

			var additionalDocMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddAdditionalDocument);
			AssertEquals("AdditionalDocument message EM_Status", EDIMessage.Status.Queued, additionalDocMessage.EM_Status);
			AssertEquals("AdditionalDocument message EM_ApplicationReference", "LRN234", additionalDocMessage.EM_ApplicationReference);
			var attachmentMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddAttachment);
			AssertEquals("Attachment message status", EDIMessage.Status.Pending, attachmentMessage.EM_Status);
			AssertEquals("Attachment message EM_ApplicationReference", "LRN234", attachmentMessage.EM_ApplicationReference);
		});

		public void TestSendLogdementStatusMessage()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var entryNumObject = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumObject.CE_EntryNum = "CE1234";
			Factory.Save();

			var senderProvider = new COLSMessageSender(colsHeader, "");
			senderProvider.SendLodgementStatusMessage();
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingGetLodgementStatusResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("1 Lodgement message generated", 1, colsHeader.Messages.Count);
			AssertEquals("Message Status", "CE1234", colsHeader.Messages[0].EM_ApplicationReference);
			AssertType<COLSMessage>(colsHeader.Messages[0]);
		}

		public void TestSendPaymentStatusMessage()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;

			var senderProvider = new COLSMessageSender(colsHeader, "ACCNO123456");
			senderProvider.SendPaymentStatusMessage();
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingGetPaymentResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("1 payment status message generated", 1, colsHeader.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Queued, colsHeader.Messages[0].EM_Status);
			AssertEquals("Application Reference", "ACCNO123456", colsHeader.Messages[0].EM_ApplicationReference);
			AssertType<COLSMessage>(colsHeader.Messages[0]);
		}

		public void TestSwitchAepLogdementMessage()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			Factory.Save();

			var senderProvider = new COLSMessageSender(colsHeader, "");
			senderProvider.SendSwitchAEPLodgementMessage();
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingSwitchAepRequest, colsHeader.QCH_MessageStatus);
			AssertEquals("1 Lodgement message generated", 1, colsHeader.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Queued, colsHeader.Messages[0].EM_Status);
			AssertType<COLSMessage>(colsHeader.Messages[0]);
		}

		public void TestSendEnquiryMessageWithAttachment() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "INVOICE";
			Factory.Save();
			var additionalInformation = new COLSEnquiryAdditionalInformation(colsHeader);
			additionalInformation.DocumentRequired = true;

			var senderProvider = new COLSMessageSender(colsHeader, "XYZ");
			var dialogMessage = senderProvider.SendEnquiryMessage(additionalInformation, colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray());
			AssertEquals("Dialog Message is empty", string.Empty, dialogMessage);
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithDocsResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("Enquiry and attachment messages generated", 2, colsHeader.Messages.Count);
			var attachmentMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.AddAttachment);
			AssertEquals("Attachment message status", EDIMessage.Status.Pending, attachmentMessage.EM_Status);
			var enquiryMessage = colsHeader.Messages.Cast<COLSMessage>().Single(x => x.EM_MessageType == AUCOLSMessageTypeList.Codes.MakeAnEnquiry);
			AssertEquals("Enquiry message status", EDIMessage.Status.Queued, enquiryMessage.EM_Status);
			AssertContains("Additional comment is included in the enquiry message", "\"additionalComments\":\"XYZ\"", enquiryMessage.EM_MessageText);
		});

		public void TestSendEnquiryMessageWithoutAttachment() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_DocType = "INVOICE";
			Factory.Save();
			var additionalInformation = new COLSEnquiryAdditionalInformation(colsHeader);
			additionalInformation.DocumentRequired = false;

			var senderProvider = new COLSMessageSender(colsHeader, "XYZ");
			var dialogMessage = senderProvider.SendEnquiryMessage(additionalInformation, colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray());
			AssertEquals("Dialog Message is empty", string.Empty, dialogMessage);
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingMakeAnEnquiryWithoutDocsResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("One Enquiry message is generated", 1, colsHeader.Messages.Count);
			var enquiryMessage = colsHeader.Messages.Cast<COLSMessage>().Single();
			AssertEquals("Enquiry message status", EDIMessage.Status.Queued, enquiryMessage.EM_Status);
			AssertContains("Additional comment is included in the enquiry message", "\"additionalComments\":\"XYZ\"", enquiryMessage.EM_MessageText);
		});

		public void TestRollBack()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			colsHeader.QCH_MessageStatus = COLSHeaderStatusList.Codes.NotSent;
			Factory.Save();

			var senderProvider = new COLSMessageSender(colsHeader, "");
			senderProvider.SendLodgementMessage(Array.Empty<CusStorageDocPivot>());
			AssertEquals("ColsHeader MessageStatus has been changed", COLSHeaderStatusList.Codes.AwaitingAddLodgementResponse, colsHeader.QCH_MessageStatus);
			AssertEquals("1 Lodgement message generated", 1, colsHeader.Messages.Count);

			senderProvider.RollBack();
			AssertEquals("Roll back: ColsHeader MessageStatus", COLSHeaderStatusList.Codes.NotSent, colsHeader.QCH_MessageStatus);
			AssertEquals("Roll back: Generated Message", 0, colsHeader.Messages.Count);
		}

		public void TestLRNRequestedEventIsCreatedOnLodgement()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var senderProvider = new COLSMessageSender(colsHeader, "");
			senderProvider.SendLodgementMessage(Array.Empty<CusStorageDocPivot>());
			var lrnLog = colsHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.StatusChange).First();
			AssertEquals("Status change event", "STC", lrnLog.SL_SE_NKEvent);
			AssertEquals("|MST=COLS|NEW=SNT", lrnLog.SL_Reference);
		}
	}
}
