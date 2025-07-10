using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(SubsequentMessageDetails))]
	sealed class SubsequentMessageDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SubsequentMessageDetails(Factory.New<CusEntryHeader>());
		}

		public void TestFTASubsequentMessageDetails()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_FTARelationArticleCode = "1";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(0, entry.EntryNumbers.Count);
			var entryNumDHR = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DHR);
			entryNumDHR.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			entryNumDHR.CE_IssueDate = new ZDateTime("2024-01-01");

			var entryNum5SC = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);
			entryNum5SC.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			entryNum5SC.CE_IssueDate = new ZDateTime("2024-01-02");

			var subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals("법 제8조제1항", subsequentMessageDetails.LawCodeDescription);
			AssertEquals("AST", subsequentMessageDetails.FTAMessageStatus);
			AssertEquals(new ZDateTime("2024-01-01"), subsequentMessageDetails.FTAAcceptedDate);

			invoiceLine.JI_CountryOfOrigin = "UK";
			subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals("법 제8조제1항", subsequentMessageDetails.LawCodeDescription);
			AssertEquals("ARJ", subsequentMessageDetails.FTAMessageStatus);
			AssertEquals(new ZDateTime("2024-01-02"), subsequentMessageDetails.FTAAcceptedDate);
		}

		public void TestMessageStatus934()
		{
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.MessageStatus934);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.AcceptedDate934);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.EstimatedDateOfFinalPrice);
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.NoticeNumber934);

			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entryNumber.CE_IssueDate = new ZDateTime(2024, 1, 1);
			entryNumber.CE_ExpiryDate = new ZDateTime(2024, 2, 1);
			entryNumber.CE_EntryNum = "0301023000001";

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entry.SubsequentMessageDetails.MessageStatus934);
			AssertEquals(new ZDateTime(2024, 1, 1), entry.SubsequentMessageDetails.AcceptedDate934);
			AssertEquals(new ZDateTime(2024, 2, 1), entry.SubsequentMessageDetails.EstimatedDateOfFinalPrice);
			AssertEquals("030-10-23-000001", entry.SubsequentMessageDetails.NoticeNumber934);
		}

		public void TestMessageStatus5BD()
		{
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.MessageStatus5BD);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.AcceptedDate5BD);

			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BD);
			entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNumber.CE_IssueDate = new ZDateTime(2024, 1, 1);
			entryNumber.CE_EntryNum = "1234520000045M";
			Factory.Save();

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, entry.SubsequentMessageDetails.MessageStatus5BD);
			AssertEquals(new ZDateTime(2024, 1, 1), entry.SubsequentMessageDetails.AcceptedDate5BD);

			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.ReviewResult5BD);
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.RequestReason);
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.SecurityType);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.SecurityPeriodStartDate);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.SecurityPeriodEndDate);
			AssertEquals(0m, entry.SubsequentMessageDetails.SecurityAmount);

			var outgoingMessage1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5BD, "GOVCBR5BD_D1.xml");
			Factory.Save();

			entry.Messages.Add(outgoingMessage1);
			var r20Message = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R20, "2");
			r20Message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BD;
			r20Message.EM_ApplicationReference = outgoingMessage1.EM_MessageNum;
			entry.Messages.Add(r20Message);

			var outgoingMessage2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5BD, "GOVCBR5BD_D2.xml");
			entry.Messages.Add(outgoingMessage2);
			Factory.Save();

			entry.Reload();

			AssertEquals("반출신청", entry.SubsequentMessageDetails.RequestReason);
			AssertEquals("02", entry.SubsequentMessageDetails.SecurityType);
			AssertEquals(new ZDateTime(2024, 4, 29), entry.SubsequentMessageDetails.SecurityPeriodStartDate);
			AssertEquals(new ZDateTime(2024, 4, 30), entry.SubsequentMessageDetails.SecurityPeriodEndDate);
			AssertEquals(5000m, entry.SubsequentMessageDetails.SecurityAmount);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.ReviewDate5BD);

			var r99Message = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "4");
			r99Message.EM_ApplicationReference = outgoingMessage2.EM_MessageNum;
			r99Message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BD;
			entry.Messages.Add(r99Message);
			var incomingMessage = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._5BE, "5", "GOVCBR5BE_0.xml");
			incomingMessage.EM_MessageOwner = "ANT";
			incomingMessage.EM_ApplicationReference = outgoingMessage2.EM_MessageNum;
			incomingMessage.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BD;
			entry.Messages.Add(incomingMessage);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var entry1 = factory.Load<CusEntryHeader>(entry.PK);

			AssertEquals("ANT", entry1.SubsequentMessageDetails.ReviewResult5BD);
			AssertEquals("반출신청", entry1.SubsequentMessageDetails.RequestReason);
			AssertEquals("02", entry1.SubsequentMessageDetails.SecurityType);
			AssertEquals(new ZDateTime(2024, 4, 29), entry1.SubsequentMessageDetails.SecurityPeriodStartDate);
			AssertEquals(new ZDateTime(2024, 4, 30), entry1.SubsequentMessageDetails.SecurityPeriodEndDate);
			AssertEquals(5000m, entry1.SubsequentMessageDetails.SecurityAmount);
			AssertEquals(new ZDateTime(2014, 5, 6, 12, 30, 0), entry1.SubsequentMessageDetails.ReviewDate5BD);
		}

		public void TestMessageStatus5BF()
		{
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.MessageStatus5BF);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.ApprovalDate5BF);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(JobMessageTypeList.Codes.Import);
			entryNumber.CE_ExpiryDate = new ZDateTime(2024, 2, 1);
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BF);
			entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.MessageStatus5BF);
			AssertEquals(new ZDateTime(2024, 2, 1), entry.SubsequentMessageDetails.ApprovalDate5BF);

			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.AcceptedDate5BF);
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.ReviewResult5BF);
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.CancellationReason);

			var outgoingMessage1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5BF, "GOVCBR5BF_D1.xml");
			Factory.Save();

			entry.Messages.Add(outgoingMessage1);
			var r20Message = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R20, "2");
			entry.Messages.Add(r20Message);

			var outgoingMessage2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5BF, "GOVCBR5BF_0.xml");
			entry.Messages.Add(outgoingMessage2);
			Factory.Save();

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, entry.SubsequentMessageDetails.MessageStatus5BF);
			var utcTime = new ZDateTime(2024, 1, 1, 10, 0, 0, System.DateTimeKind.Utc);
			var r99Message = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "4");
			r99Message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BF;
			r99Message.EM_SystemCreateTimeUtc = utcTime;
			r99Message.EM_ApplicationReference = outgoingMessage2.EM_MessageNum;
			entry.Messages.Add(r99Message);
			var incomingMessage = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._5BG, "5");
			incomingMessage.EM_MessageOwner = "ANT";
			incomingMessage.EM_ApplicationReference = outgoingMessage2.EM_MessageNum;
			incomingMessage.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BF;
			entry.Messages.Add(incomingMessage);
			Factory.Save();

			entry.Reload();
			AssertEquals(utcTime.ToLocalBranchTime(), entry.SubsequentMessageDetails.AcceptedDate5BF);
			AssertEquals("ANT", entry.SubsequentMessageDetails.ReviewResult5BF);
			AssertEquals("신청", entry.SubsequentMessageDetails.CancellationReason);
		}

		public void TestMessageStatus5TM()
		{
			AssertEquals(ZString.Empty, entry.SubsequentMessageDetails.MessageStatus5TM);
			AssertEquals(ZDateTime.Empty, entry.SubsequentMessageDetails.AcceptedDate5TM);

			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TM);
			entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNumber.CE_IssueDate = new ZDateTime(2024, 1, 1);
			entryNumber.CE_EntryNum = "1234520000045M";
			Factory.Save();

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, entry.SubsequentMessageDetails.MessageStatus5TM);
			AssertEquals(new ZDateTime(2024, 1, 1), entry.SubsequentMessageDetails.AcceptedDate5TM);
		}

		public void TestMessageStatus5FN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum.CE_EntryLineReference = "1";
			entryNum.CE_IssueDate = new ZDateTime(2024, 02, 26);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNum2.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			entryNum2.CE_EntryLineReference = "2";
			entryNum2.CE_IssueDate = new ZDateTime(2024, 02, 27);

			var message1 = entry.Messages.AddNew();
			message1.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2024, 02, 26, 02, 00, 00);
			message1.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(SubsequentMessageDetailsTest));
			var messageText1 = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D1.xml");
			message1.EM_MessageText = messageText1;

			var message2 = entry.Messages.AddNew();
			message2.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2024, 02, 26, 01, 00, 00);
			message2.EM_ApplicationReference = "1";
			var messageText2 = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D2.xml");
			message2.EM_MessageText = messageText2;

			var message3 = entry.Messages.AddNew();
			message3.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2024, 02, 26, 01, 00, 00);
			message3.EM_ApplicationReference = "2";
			var messageText3 = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5FN_D3.xml");
			message3.EM_MessageText = messageText3;
			Factory.Save();

			var messageObject = entry.SubsequentMessageDetails;
			AssertEquals(2, messageObject.GOVCBR5FNMessages.Count);
			AssertEquals(1, messageObject.GOVCBR5FNMessages[0].EntryLineNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, messageObject.GOVCBR5FNMessages[0].MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, messageObject.GOVCBR5FNMessages[0].MessageStatusDesc);
			AssertEquals(new ZDateTime(2024, 02, 26), messageObject.GOVCBR5FNMessages[0].AcceptedDate);
			AssertEquals("4203400000", messageObject.GOVCBR5FNMessages[0].HSCode);
			AssertEquals("T", messageObject.GOVCBR5FNMessages[0].DutyReduction);
			AssertEquals("N", messageObject.GOVCBR5FNMessages[0].PostClearanceYN);

			AssertEquals(2, messageObject.GOVCBR5FNMessages[1].EntryLineNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, messageObject.GOVCBR5FNMessages[1].MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalRejected, messageObject.GOVCBR5FNMessages[1].MessageStatusDesc);
			AssertEquals(new ZDateTime(2024, 02, 27), messageObject.GOVCBR5FNMessages[1].AcceptedDate);
			AssertEquals("8463200000", messageObject.GOVCBR5FNMessages[1].HSCode);
			AssertEquals("D", messageObject.GOVCBR5FNMessages[1].DutyReduction);
			AssertEquals("Y", messageObject.GOVCBR5FNMessages[1].PostClearanceYN);
		}

		public void TestGOVCBRD72Messages()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(0, entry.Messages.Count);

			var subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals(0, subsequentMessageDetails.GOVCBRD72Messages.Count);

			CreateEDIMessage(ElectronicDocumentTypeList.Codes._929, "TRX", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R99, "RCV", "2", "1");

			var entryNumD72 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D72);
			entryNumD72.CE_EntryLineReference = "";
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._D72, "TRX", "3", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R20, "RCV", "4", "3");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._D72, "TRX", "5", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R99, "RCV", "6", "5");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R43, "RCV", "7", "5");

			entryNumD72.CE_EntryLineReference = "1";
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._D72, "TRX", "8", "2");
			AssertEquals(8, entry.Messages.Count);
			AssertEquals(3, entry.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._D72).Count());
			subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals(2, subsequentMessageDetails.GOVCBRD72Messages.Count);

			EDIMessage CreateEDIMessage(string messageType, string receiveTransmit, string messageNum, string applicationReference = "")
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_ReceiveTransmit = receiveTransmit;
				message.EM_MessageNum = messageNum;
				message.EM_ApplicationReference = applicationReference;
				return message;
			}
		}
		public void Test5BAAnd5BBSubsequentMessageDetails()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(0, entry.Messages.Count);

			var subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals(0, subsequentMessageDetails.GOVCBR5BBMessages.Count);

			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BA, "1", "");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R20, "2", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BA, "3", "");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R99, "4", "3");

			var entryNum5BA = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
			entryNum5BA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			entryNum5BA.CE_IssueDate = new ZDateTime("2024-01-01");
			entryNum5BA.CE_EntryLineReference = "";

			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BB, "5", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R20, "6", "5");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BB, "7", "1");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._R99, "8", "7");
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BC, "9", "7");

			entryNum5BA.CE_EntryLineReference = "1";
			CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BB, "10", "2");
			AssertEquals(10, entry.Messages.Count);
			AssertEquals(3, entry.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5BB).Count());
			subsequentMessageDetails = new SubsequentMessageDetails(entry);
			AssertEquals(2, subsequentMessageDetails.GOVCBR5BBMessages.Count);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, subsequentMessageDetails.MessageStatus5BA);
			AssertEquals(new ZDateTime("2024-01-01"), subsequentMessageDetails.AcceptedDate5BA);

			void CreateEDIMessage(ZString messageType, ZString messageNum, ZString applicationReference)
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_MessageNum = messageNum;
				message.EM_ApplicationReference = applicationReference;
			}
		}

		public void TestMessageStatus5UA()
		{
			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			entryNum1.CE_EntryLineReference = "1";
			entryNum1.CE_IssueDate = new ZDateTime(2024, 06, 02);

			var message5UA1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "GOVCBR5UA_D1.xml");
			message5UA1.EM_ApplicationReference = "1";
			message5UA1.EM_MessageNum = "1";
			message5UA1.EM_SystemCreateTimeUtc = new ZDateTime(2024, 05, 08);
			entry.Messages.Add(message5UA1);

			var message5UA2_1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "GOVCBR5UA_D2.xml");
			message5UA2_1.EM_ApplicationReference = "2";
			message5UA2_1.EM_MessageNum = "2";
			message5UA2_1.EM_SystemCreateTimeUtc = new ZDateTime(2024, 05, 10);
			entry.Messages.Add(message5UA2_1);

			Factory.Save();

			var subsequentMessageDetails1 = new SubsequentMessageDetails(entry);

			AssertEquals(1, subsequentMessageDetails1.GOVCBR5UAMessages.Count);
			AssertEquals("1", subsequentMessageDetails1.GOVCBR5UAMessages[0].ApplicationReference);

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			entryNum2.CE_EntryLineReference = "2";
			entryNum2.CE_IssueDate = new ZDateTime(2024, 06, 03);

			var message5UA2_2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "GOVCBR5UA_D1.xml");
			message5UA2_2.EM_ApplicationReference = "2";
			message5UA2_2.EM_MessageNum = "3";
			message5UA2_2.EM_SystemCreateTimeUtc = new ZDateTime(2024, 05, 09);
			entry.Messages.Add(message5UA2_2);

			var fileReader = new TestFileReader(typeof(SubsequentMessageDetailsTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Common.Incoming", "GOVCBRR20_5UA.xml");

			var messageR20 = Factory.New<EDIMessage>();
			messageR20.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			messageR20.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageR20.EM_MessageText = messageText;
			messageR20.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR20.EM_ApplicationReference = message5UA1.EM_MessageNum;
			messageR20.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5UA;
			entry.Messages.Add(messageR20);

			var messageR99 = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "1", "GOVCBRR99_5UA.xml");
			messageR99.EM_ApplicationReference = message5UA2_1.EM_MessageNum;
			messageR99.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5UA;
			entry.Messages.Add(messageR99);

			var message5UB = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._5UB, "1", "GOVCBR5UB_0.xml");
			message5UB.EM_ApplicationReference = message5UA2_1.EM_MessageNum;
			message5UB.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5UA;
			message5UB.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			entry.Messages.Add(message5UB);
			Factory.Save();

			var subsequentMessageDetails2 = new SubsequentMessageDetails(entry);

			AssertEquals(2, subsequentMessageDetails2.GOVCBR5UAMessages.Count);

			AssertEquals("1", subsequentMessageDetails2.GOVCBR5UAMessages[0].ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, subsequentMessageDetails2.GOVCBR5UAMessages[0].MessageStatus);

			var messageDetails = subsequentMessageDetails2.GOVCBR5UAMessages[1];
			AssertEquals("2", messageDetails.ApplicationReference);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, messageDetails.MessageStatus);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, messageDetails.MessageStatusDescription);
			AssertEquals(new ZDateTime(2024, 06, 03), messageDetails.MessageData5UA.AcceptedDate);
			AssertEquals(new ZDateTime(2020, 06, 01), messageDetails.MessageData5UA.ReviewDate);
			AssertEquals(AdditionalTexReductionResultTypeList.Descriptions.C, messageDetails.MessageData5UA.ReviewResultDescription);
			AssertEquals(new ZDateTime(2014, 01, 01), messageDetails.MessageData5UA.AmendmentDeclarationDate);
			AssertEquals(1, messageDetails.MessageData5UA.AmendmentVersionNo);
			AssertEquals(PenaltyExemptionCodeList.Descriptions.A, messageDetails.MessageData5UA.PenaltyTypeDescription);
			AssertEquals(PenaltyExemptionReasonCodeList.Descriptions.B5, messageDetails.MessageData5UA.PenaltyExemptionReasonsCodeDescription);
			AssertEquals(200m, messageDetails.MessageData5UA.PenaltyExemptionAmount);
		}

		public void TestMessageStatus5UL()
		{
			var message5UL1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UL);
			message5UL1.EM_MessageNum = "1";
			message5UL1.EM_MessageOwner = "123452000043U";
			message5UL1.EM_ApplicationReference = "1";
			message5UL1.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 20);
			entry.Messages.Add(message5UL1);

			var messageR991 = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "10");
			messageR991.EM_ApplicationReference = message5UL1.EM_MessageNum;
			entry.Messages.Add(messageR991);

			var message5UL2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UL);
			message5UL2.EM_MessageNum = "2";
			message5UL2.EM_MessageOwner = "987652000043U";
			message5UL2.EM_ApplicationReference = "2";
			message5UL2.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 21);
			entry.Messages.Add(message5UL2);

			var messageR20 = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R20, "11");
			messageR20.EM_ApplicationReference = message5UL2.EM_MessageNum;
			entry.Messages.Add(messageR20);

			var message5UL3 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UL);
			message5UL3.EM_MessageNum = "3";
			message5UL3.EM_MessageOwner = "987652000043U";
			message5UL3.EM_ApplicationReference = "3";
			message5UL3.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 22);
			entry.Messages.Add(message5UL3);

			var messageR992 = CreateIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "12");
			messageR992.EM_ApplicationReference = message5UL3.EM_MessageNum;
			entry.Messages.Add(messageR992);
			Factory.Save();

			var subsequentMessageDetails = new SubsequentMessageDetails(entry);

			var messages = subsequentMessageDetails.GOVCBR5ULMessages;
			AssertEquals(2, messages.Count);
			AssertEquals("1", messages[0].ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, messages[0].MessageStatus);
			AssertEquals("3", messages[1].ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, messages[1].MessageStatus);
		}

		EDIMessage CreateOutgoingMessage(string messageType, string filename = "")
		{
			IsIncoming = false;
			var fileReader = new TestFileReader(typeof(SubsequentMessageDetailsTest));
			var messageText = filename == ZString.Empty ? "" : fileReader.GetEmbeddedFileText(TestFilesPath, filename);

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = messageType;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = messageText;
			return outgoingMessage;
		}

		EDIMessage CreateIncomingMessage(string messageType, string messageNum, string filename = "")
		{
			IsIncoming = true;
			var fileReader = new TestFileReader(typeof(SubsequentMessageDetailsTest));
			var messageText = filename == ZString.Empty ? "" : fileReader.GetEmbeddedFileText(TestFilesPath, filename);

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = messageNum;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			return incomingMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1234520000045M";
		}

		JobDeclaration declaration;
		CusEntryHeader entry;

		bool IsIncoming { get; set; }
		public string TestFilesPath => IsIncoming ? "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming" : "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
