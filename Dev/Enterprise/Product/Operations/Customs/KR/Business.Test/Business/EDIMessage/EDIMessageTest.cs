using System;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	sealed class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestApplicationCodeIsSet()
		{
			AssertEquals(EDIMessage.ApplicationCodes.KRCustoms, Factory.New<EDIMessage>().EM_ApplicationCode);
		}

		public void TestShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride()
		{
			var itemMessage5AS = new Declaration()
			{
				DeclarationOfficeId = new DeclarationDeclarationOfficeIdType { Value = "01020" },
				AdditionalInformation = new DeclarationAdditionalInformation
				{
					StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = "정정/취하/연장사유" }
				}
			};
			using (var stream = KRXmlObjectSerializer.Serialize(itemMessage5AS))
			{
				var message = Factory.New<EDIMessage>();
				message.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
				AssertXMLContains("<DeclarationOfficeID>01020</DeclarationOfficeID>", message.EM_MessageText);//getter does return data from em_messagedata. 
				AssertXMLContains("<StatementDescription>정정/취하/연장사유</StatementDescription>", message.EM_MessageText);//if set to EM_MessageText, Korean characters would have been lost
				Assert("Message Content has been set to EM_MessageData", !message.EM_MessageData.IsEmpty);
			}
		}
		public void TestPopulateMessageNumber()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "Message Number is " + EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();

			AssertNotNull("EM_MessageNum is assigned", message.EM_MessageNum);
			AssertEquals("MessageNumberPlaceHolder is replaced", "Message Number is " + message.EM_MessageNum, message.EM_MessageText);
		}
		public void TestMessageOrEntryStatus()
		{
			CreateMessageAndAssertMessageStatus(true);
		}

		public void TestMessageStatus()
		{
			CreateMessageAndAssertMessageStatus(false);
		}

		void CreateMessageAndAssertMessageStatus(bool isIncludingEntryStatus)
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var errMessage = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, ElectronicDocumentTypeList.Codes._830, ElectronicDocumentTypeList.Codes._R20, err: true);
			AssertNullOrEmpty(errMessage.MessageStatus);

			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5AS, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.AmendmentRejected, _5ASAmendmentType.Codes.Amendment, isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._DKJ, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.CancellationRejected, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.OriginalRejected, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5FE, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.AmendmentRejected, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.AmendmentRejected, MessageSubTypeLocalExport.Amendment, isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._R20, CustomsMessageStatusTypeList.Codes.CancellationRejected, MessageSubTypeLocalExport.Cancellation, isIncludingEntryStatus);

			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.Codes._R99, CustomsMessageStatusTypeList.Codes.OriginalAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5FE, ElectronicDocumentTypeList.Codes._R99, CustomsMessageStatusTypeList.Codes.AmendmentAccepted, "AX", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5UL, ElectronicDocumentTypeList.Codes._R99, CustomsMessageStatusTypeList.Codes.OriginalAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5BB, ElectronicDocumentTypeList.Codes._R99, CustomsMessageStatusTypeList.Codes.AmendmentAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5BF, ElectronicDocumentTypeList.Codes._R99, CustomsMessageStatusTypeList.Codes.CancellationAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._830, ElectronicDocumentTypeList.Codes._5AF, CustomsMessageStatusTypeList.Codes.OriginalAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5AS, ElectronicDocumentTypeList.Codes._5AF, CustomsMessageStatusTypeList.Codes.AmendmentAccepted, "", isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5DS, ElectronicDocumentTypeList.Codes._R38, CustomsMessageStatusTypeList.Codes.AmendmentAccepted, MessageSubTypeLocalExport.Amendment, isIncludingEntryStatus);
			CreateAndAssertMessage(entry, ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._R38, CustomsMessageStatusTypeList.Codes.CancellationAccepted, MessageSubTypeLocalExport.Cancellation, isIncludingEntryStatus);

			var expMessage = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, ElectronicDocumentTypeList.Codes._5AS, ElectronicDocumentTypeList.Codes._5AF, subType: _5ASAmendmentType.Codes.Amendment);
			CreateMessage(entry, ElectronicDocumentTypeList.Codes._5DT, ZDateTime.Now.AddMinutes(1),messageSubType: expMessage.EM_MessageType, messageOwner: CustomsEntryStatusTypeList.Codes.ANT, applicationReference: expMessage.EM_MessageNum );
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, expMessage.MessageOrEntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, expMessage.MessageStatus);

			var lexMessage1 = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, ElectronicDocumentTypeList.Codes._5DS, ElectronicDocumentTypeList.Codes._R38, subType: MessageSubTypeLocalExport.Amendment);
			CreateMessage(entry, ElectronicDocumentTypeList.Codes._RR3, ZDateTime.Now.AddMinutes(1),messageSubType: lexMessage1.EM_MessageType, messageOwner: CustomsEntryStatusTypeList.Codes.ANT, applicationReference: lexMessage1.EM_MessageNum );
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, lexMessage1.MessageOrEntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, lexMessage1.MessageStatus);

			var lexMessage2 = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, ElectronicDocumentTypeList.Codes._5DR, ElectronicDocumentTypeList.Codes._R38, subType: MessageSubTypeLocalExport.Cancellation);
			CreateMessage(entry, ElectronicDocumentTypeList.Codes._RR3, ZDateTime.Now.AddMinutes(1),messageSubType: lexMessage2.EM_MessageType, messageOwner: CustomsEntryStatusTypeList.Codes.CCL, applicationReference: lexMessage2.EM_MessageNum );
			AssertEquals(CustomsEntryStatusTypeList.Codes.CCL, lexMessage2.MessageOrEntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, lexMessage2.MessageStatus);

			var impMessage = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, ElectronicDocumentTypeList.Codes._5FE, ElectronicDocumentTypeList.Codes._R99, subType: DutyTaxCorrectionCodeList.Codes.X +  DeclarationCorrectionCodeList.Codes.X);
			CreateMessage(entry, ElectronicDocumentTypeList.Codes._5FK, ZDateTime.Now.AddMinutes(1), messageSubType: impMessage.EM_MessageType, messageOwner: CustomsEntryStatusTypeList.Codes.ANT, applicationReference: impMessage.EM_MessageNum );
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, impMessage.MessageOrEntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, impMessage.MessageStatus);
		}

		void CreateAndAssertMessage(CusEntryHeader entry, string outgoingCode, string incomingCode, string expectedStatus, string subType, bool isIncludingEntryStatus)
		{
			var message = CreateOutgoingMsgWithCorrespondingIncomingMsg(entry, outgoingCode, incomingCode, subType);
			var status = isIncludingEntryStatus ?  message.MessageOrEntryStatus : message.MessageStatus;
			AssertEquals(expectedStatus, status);
		}

		EDIMessage CreateOutgoingMsgWithCorrespondingIncomingMsg(CusEntryHeader entry, ZString outgoingMessageType, ZString incomingMessageType, string subType = "", string messageOwner = "", bool err = false)
		{
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_MessageType = outgoingMessageType;
			outgoingMessage.EM_MessageSubType = subType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			CreateMessage(entry, incomingMessageType, ZDateTime.Now, outgoingMessage.EM_MessageType, messageOwner, err ? ZString.Empty : outgoingMessage.EM_MessageNum);
			Factory.Save();

			return outgoingMessage;
		}
		EDIMessage CreateMessage(CusEntryHeader entry, string messageType, ZDateTime messageTime, string messageSubType = "", string messageOwner = "", string applicationReference = "")
		{
			var message = entry.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message.EM_MessageType = messageType;
			message.EM_SystemCreateUser = "ORG";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageOwner = messageOwner;
			message.EM_ApplicationReference = applicationReference;
			message.EM_SystemCreateTimeUtc = messageTime;
			Factory.Save();

			return message;
		}

		public void TestCusPollingTransactionCollection()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_MessageType = "DLT";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = "2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42,GOVCBR5AF" + "\n"
				+ "2020040909570620200409-ELI-e41c8a55-9a74-4a86-a3cd-96dca925e0c1,GOVCBR5AA";
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			AssertEquals(2, incomingMessage.CusPollingTransactions.Count);
			var orderedTransactions = incomingMessage.CusPollingTransactions.OrderBy(item => item.CPT_TransactionID).ToArray();
			AssertEquals("2020040909570320200409-ELI-edb54e29-d2aa-49ea-849c-87f29bbf1f42", orderedTransactions[0].CPT_TransactionID);
			AssertEquals("5AF", orderedTransactions[0].CPT_Reference);
			AssertEquals("2020040909570620200409-ELI-e41c8a55-9a74-4a86-a3cd-96dca925e0c1", orderedTransactions[1].CPT_TransactionID);
			AssertEquals("5AA", orderedTransactions[1].CPT_Reference);
		}

		[TestDate(2023, 4, 20)]
		public void TestGetEntryNumberLEX()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			AssertEquals("Pre-condition", "", entry.EntryNumber);
			Factory.Save();
			AssertEquals("Entry number is generated when saving CusEntryHeader.", "6N00223000001", entry.EntryNumber);
			AssertEquals(0u, entry.CH_VersionID);
			AssertEquals("CE_EntryLineReference of the CusEntryNum created is CH_VersionID + 1", "1", entry.CusEntryNumber.CE_EntryLineReference);

			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = entry;
			message1.EM_MessageText = "Entry Number is " + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			AssertEquals("EntryNumberPlaceHolder is replaced when saving EdiMessage. " +
				"Entry number is not generated as the CusEntryNum has CE_EntryLineReference greater than CH_VersionID already exists.",
				"Entry Number is 6N00223000001", message1.EM_MessageText);

			entry.CH_VersionID++;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = entry;
			message2.EM_MessageText = "Entry Number is " + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			AssertEquals("Entry Number is generated as the CusEntryNum has CE_EntryLineReference greater than CH_VersionID does not exist. " +
				"EntryNumberPlaceHolder is replaced with it.",
				"Entry Number is 6N00223000002", message2.EM_MessageText);
			AssertEquals(1u, entry.CH_VersionID);
			entry.EntryNumbers.Reload(true);
			AssertEquals("2", entry.EntryNumbers.Cast<CusEntryNumber>().OrderByDescending(x => x.CE_EntryLineReference).FirstOrDefault()?.CE_EntryLineReference);

			var message3 = Factory.New<EDIMessage>();
			message3.EM_LinkedObject = entry;
			message3.EM_MessageText = "Entry Number is " + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			AssertEquals("Entry Number is not generated as the CusEntryNum has CH_LineReference greater than CH_VersionID already exists.",
				"Entry Number is 6N00223000002", message2.EM_MessageText);
		}

		[TestDate(2023, 4, 20)]
		public void TestGetEntryNumberNonLEX()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Pre-condition", "", entry.EntryNumber);
			Factory.Save();
			AssertEquals("Entry number is assigned when saving CusEntryHeader.", "6N00223000001X", entry.EntryNumber);

			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = entry;
			message1.EM_MessageText = "Entry Number is " + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			AssertEquals("EntryNumberPlaceHolder is replaced when saving EdiMessage.", "Entry Number is 6N00223000001X", message1.EM_MessageText);

			entry.CH_VersionID++;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = entry;
			message2.EM_MessageText = "Entry Number is " + EDIMessage.EntryNumberPlaceHolder;
			Factory.Save();
			AssertEquals("Entry Number is created only when saving CusEntryHeader.", "Entry Number is 6N00223000001X", message2.EM_MessageText);
		}

		public void TestLinkedCusPollingTransactionType()
		{
			var messageSave = Factory.New<EDIMessage>();
			var transaction = Factory.New<CusPollingTransaction>();
			transaction.CPT_TransactionID = "1";
			transaction.CPT_ApplicationCode = CusPollingTransaction.ApplicationCodes.KRCustoms;
			transaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
			messageSave.EM_LinkedObject = transaction;
			Factory.Save();

			var messageLoad = new BusinessObjectFactory().Load<EDIMessage>(messageSave.PK);
			AssertType<CusPollingTransaction>(messageLoad.EM_LinkedObject);
		}

		[TestDate(2024, 04, 17)]
		public void Test5ULRefundNumber()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum5UL = entry.EntryNumbers.AddNew();
			entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL.CE_EntryLineReference = "11111";

			var impMessage5FE = Factory.New<EDIMessage>();
			impMessage5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			impMessage5FE.CustomsDisbursementBill = "11111";
			impMessage5FE.EM_LinkedObject = entry;
			impMessage5FE.EM_MessageText = "Refund Number is " + EDIMessage.RefundEntryNumberPlaceHolder;

			var impMessage5UL = Factory.New<EDIMessage>();
			impMessage5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			impMessage5UL.CustomsDisbursementBill = "11111";
			impMessage5UL.EM_ApplicationReference = "11111";
			impMessage5UL.EM_LinkedObject = entry;
			impMessage5UL.EM_MessageText = "Refund Number is " + EDIMessage.RefundEntryNumberPlaceHolder;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadEntry = factory.Load<CusEntryHeader>(entry.PK);
			entryNum5UL = loadEntry.EntryNumbers.Cast<CusEntryNumber>().LastOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);

			AssertEquals("CusEntryNum is Update when saving EdiMessage.", "6N0022400001U", entryNum5UL.CE_EntryNum);

			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "Refund Number is 6N0022400001U", impMessage5FE.EM_MessageText);
			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "", impMessage5FE.EM_MessageOwner);

			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "Refund Number is 6N0022400001U", impMessage5UL.EM_MessageText);
			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "6N0022400001U", impMessage5UL.EM_MessageOwner);
		}

		[TestDate(2025, 01, 01)]
		public void Test5ULRefundNumberHtml()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum5UL = entry.EntryNumbers.AddNew();
			entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL.CE_EntryLineReference = "11111";

			var impMessage5FE = Factory.New<EDIMessage>();
			impMessage5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			impMessage5FE.CustomsDisbursementBill = "11111";
			impMessage5FE.EM_LinkedObject = entry;
			impMessage5FE.EM_MessageText = "Refund Number is " + EDIMessage.RefundEntryNumberPlaceHolderHtml;

			var impMessage5UL = Factory.New<EDIMessage>();
			impMessage5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			impMessage5UL.CustomsDisbursementBill = "11111";
			impMessage5UL.EM_ApplicationReference = "11111";
			impMessage5UL.EM_LinkedObject = entry;
			impMessage5UL.EM_MessageText = "Refund Number is " + EDIMessage.RefundEntryNumberPlaceHolderHtml;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadEntry = factory.Load<CusEntryHeader>(entry.PK);
			entryNum5UL = loadEntry.EntryNumbers.Cast<CusEntryNumber>().LastOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);

			AssertEquals("CusEntryNum is Update when saving EdiMessage.", "6N0022500001U", entryNum5UL.CE_EntryNum);

			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "Refund Number is 6N0022500001U", impMessage5FE.EM_MessageText);
			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "", impMessage5FE.EM_MessageOwner);

			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "Refund Number is 6N0022500001U", impMessage5UL.EM_MessageText);
			AssertEquals("RefundEntryNumberPlaceHolder is replaced when saving EdiMessage.", "6N0022500001U", impMessage5UL.EM_MessageOwner);
		}
	}
}
