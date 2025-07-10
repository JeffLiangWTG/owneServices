using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.ES.Business.Testing;

public abstract class ImportGenericResponseMessageProcessorTest<TResponse, TResponseProcessor> : XMLResponseMessageProcessorTest<TResponse, IMessagePrettyFormatter, TResponseProcessor>
	where TResponse : ImportGenericResponseMessageProcessor<TResponseProcessor>
	where TResponseProcessor : class, ICommonServiceSegment, IImportCommonGeneric, IResponseCode
{
	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AEATDocs_InstructionA()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 4, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_I_AEAT_CLR.pdf", CsvClearance),
				(MRNCode + "_I_AEAT_CER.pdf", CsvImportCertificate),
				(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J031.pdf", MRNCode)]
			);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AEATDocs_InstructionC()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedEntryStatusForInstructionC, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", ExpcetedNumberDocMessagesAmountAEATInstructionC, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages, ExpectedDocumentsAEATInstructionC);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AEATDocs_Complementary_InstructionY()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Y;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 4, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_I_AEAT_CLR_C.pdf", CsvClearance),
				(MRNCode + "_I_AEAT_CER_C.pdf", CsvImportCertificate),
				(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J031.pdf", MRNCode)]
			);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_ATCDocs_InstructionA()
	{
		declaration.ZG_DestinationState = CanaryIslandCode;

		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceATC, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 6, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_I_AEAT_CLR.pdf", CsvClearance),
				(MRNCode + "_I_AEAT_CER.pdf", CsvImportCertificate),
				(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J032.pdf", MRNCode)]
			);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_ATCDocs_InstructionC()
	{
		declaration.ZG_DestinationState = CanaryIslandCode;

		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceATC, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedEntryStatusForInstructionC, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", ExpcetedNumberDocMessagesAmountATCInstructionC, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages, ExpectedDocumentsATCInstructionC);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_ATCDocs_Complementary_InstructionY()
	{
		declaration.ZG_DestinationState = CanaryIslandCode;

		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Y;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceATC, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 6, docMessages.Length);

			AssertDocumentRequestEDIMessages(docMessages,
				[(MRNCode + "_I_AEAT_CLR_C.pdf", CsvClearance),
				(MRNCode + "_I_AEAT_CER_C.pdf", CsvImportCertificate),
				(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
				(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
				(MRNCode + "_I_AEAT_J032.pdf", MRNCode)]
			);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs_InstructionA()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

		AddEntryDocuments(
			[(MRNCode + "_I_AEAT_J031.pdf", "CAU"),
			(MRNCode + "_I_AEAT_M031.pdf", "CAU"),
			(MRNCode + "_I_AEAT_CLR.pdf", "CLR"),
			(MRNCode + "_I_AEAT_CER.pdf", "CLR")]);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs_InstructionC()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.C;

		IEnumerable<(string FileName, string DocumentType)> entryDocuments =
			[(MRNCode + "_I_AEAT_J031.pdf", "CAU"),
			(MRNCode + "_I_AEAT_M031.pdf", "CAU")];
		if (ExpectedEntryStatusForInstructionC == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations)
		{
			entryDocuments = entryDocuments.Concat(
				[(MRNCode + "_I_AEAT_CLR.pdf", "CLR"),
				(MRNCode + "_I_AEAT_CER.pdf", "CLR")]);
		}
		else
		{
			entryDocuments = entryDocuments.Concat(
				[(MRNCode + "_I_AEAT_CLR_C.pdf", "CLR"),
				(MRNCode + "_I_AEAT_CER_C.pdf", "CLR")]);
		}
		AddEntryDocuments(entryDocuments);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedEntryStatusForInstructionC, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs_Complementary_InstructionY()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Y;

		AddEntryDocuments(
			[(MRNCode + "_I_AEAT_J031.pdf", "CAU"),
			(MRNCode + "_I_AEAT_M031.pdf", "CAU"),
			(MRNCode + "_I_AEAT_CLR_C.pdf", "CLR"),
			(MRNCode + "_I_AEAT_CER_C.pdf", "CLR")]);

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearanceAEAT, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, entryStatusCode: ExpectedentryStatusCode, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestProcessRejectedMessage()
	{
		AddMessageProcessAndAssertResult_RejectedMessage();
	}

	protected void AddMessageProcessAndAssertResult_RejectedMessage(bool isEntryStatusEmpty = false)
	{
		var entryStatus = isEntryStatusEmpty ? ZString.Empty : RejectedEntryStatus;

		AddMessageProcessAndAssertResult_RejectedMessageBase(entryStatus);
	}

	protected void AddMessageProcessAndAssertResult_RejectedMessage()
	{
		AddMessageProcessAndAssertResult_RejectedMessageBase(RejectedEntryStatus);
	}

	void AddMessageProcessAndAssertResult_RejectedMessageBase(ZString entryStatus)
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, RejectedTestFile, InterchangeID);

		ProcessMessageForTest(message);
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: RejectedMessageInterpretation, messageSubType: "REJ", messageNum: RejectedMessageNum, entryStatusCode: entryStatus, movementReferenceNumber: ExpectedMovementReferenceNumberReject, acceptanceDate: ExpectedMovementReferenceNumberIssueDateReject);
	}

	public void TestGuaranteesAmount0WhenNoGuaranteesInResponse()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutGuarantees, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true);

		CombineAssertions(() =>
		{
			AssertEquals("First Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
			AssertEquals("First Guarantee with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
			AssertEquals("Second Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
		});
	}

	public void TestGuaranteesAmountWhenGuaranteesInResponse_AEAT()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesAEAT, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			AssertEquals("First Guarantee with entry instruction of entry has amount changed to the one in the response", 110m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
			AssertEquals("First Guarantee with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
			AssertEquals("Second Guarantee with entry instruction of entry has amount changed to the one in the response", 600m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
		});
	}

	public void TestGuaranteesAmountWhenGuaranteesInResponse_ATC()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			AssertEquals("First Guarantee with entry instruction of entry has amount changed to the one in the response", 522.51m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
			AssertEquals("First Guarantee  with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
			AssertEquals("Second Guarantee with entry instruction of entry has amount changed to the one in the response", 551.55m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
		});
	}

	[TestDate(2021, 10, 05, 09, 36, 0)]
	public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithCONTransactions()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutGuarantees, InterchangeID);

		ProcessMessageForTest(message);

		Factory.Save();
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
			AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 110m, new ZDateTime(2021, 10, 05));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
			AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 600m, new ZDateTime(2021, 10, 05));
		});
	}

	public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutGuarantees, InterchangeID);

		ProcessMessageForTest(message);

		Factory.Save();
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for first guarantee", 12, guarantee1Transactions.Count());

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for second guarantee", 12, guarantee2Transactions.Count());
		});
	}

	public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithCONTransactions_WithPositiveAmount()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutGuarantees, InterchangeID);

		ProcessMessageForTest(message);

		Factory.Save();
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for first guarantee", 13, guarantee1Transactions.Count());

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for second guarantee", 13, guarantee2Transactions.Count());
		});
	}

	public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithoutCONTransactions()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithoutGuarantees, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
			AssertEquals("Only OBL transaction for first guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).CusGuaranteeLineTransactions.Count);
			AssertEquals("Only OBL transaction for second guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).CusGuaranteeLineTransactions.Count);
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithoutCONTransactions()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transaction + 3 new transactions for first guarantee", 4, guarantee1Transactions.Count());
			AssertNewTransaction("First guarantee's first transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix) && x.CPL_TranValue == -487.51m), MRNCode, entryHeader.CH_BGMReference, -487.51m, new ZDateTime(2020, 11, 04), false);
			AssertNewTransaction("First guarantee's second transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix) && x.CPL_TranValue == -30m), MRNCode, entryHeader.CH_BGMReference, -30m, new ZDateTime(2020, 11, 04), false);
			AssertNewTransaction("First guarantee's third transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix) && x.CPL_TranValue == -5m), MRNCode, entryHeader.CH_BGMReference, -5m, new ZDateTime(2020, 11, 04), false);

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transaction + 1 new transaction for second guarantee", 2, guarantee2Transactions.Count());
			AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, -551.55m, new ZDateTime(2020, 11, 04), false);
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithoutCONTransactions_WithoutOpeningBalance()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false, addOBLTransaction: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("No new transactions for first guarantee", 0, guarantee1Transactions.Count());

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("No new transaction for second guarantee", 0, guarantee2Transactions.Count());
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactions()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
			AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, -412.51m, new ZDateTime(2020, 11, 04));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
			AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 48.45m, new ZDateTime(2020, 11, 04));
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions first guarantee", 12, guarantee1Transactions.Count());

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for second guarantee", 12, guarantee2Transactions.Count());
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactions_WithPositiveAmount()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesATC, InterchangeID);

		ProcessMessageForTest(message);

		Factory.Save();
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "ATC", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for first guarantee", 13, guarantee1Transactions.Count());

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions for second guarantee", 13, guarantee2Transactions.Count());
		});
	}

	public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactionsWithCompleteAmount()
	{
		AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithGuaranteesAEAT, InterchangeID);

		ProcessMessageForTest(message);
		AssertAcceptedResponseBoth(message, entryHeader, typeMessage: "AEAT", expectedMessageInterpretation: ZString.Empty, parallel: true, acceptanceDate: AcceptanceDate);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Only original transactions for first guarantee", 13, guarantee1Transactions.Count());
			AssertEquals("No new IMP transaction for first guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Only original transactions for second guarantee", 13, guarantee1Transactions.Count());
			AssertEquals("No new IMP transaction for second guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)));
		});
	}

	protected void AssertProcessAcceptedMessageWithNoClearanceTriggers031DocumentCapture(
		string messageBody,
		string messageBodyATC = null,
		string expectedEntryStatus = EntryStatusCodes.CustomsDeclarationAccepted,
		string expectedCircuit = Declaration.MessageFunctionCodeList.Codes.GreenCircuit,
		string expectedCircuitCan = Declaration.MessageFunctionCodeList.Codes.GreenCircuit,
		ZDateTime? expectedAcceptanceDate = null,
		ZDateTime? expectedEntryReleaseDate = null,
		ZDateTime? expectedLimitPaymentDate = null,
		ZDateTime? expectedAtcLimitPaymentDate = null,
		string expectedPaymentProofNumber = null,
		string expectedAtcPaymentProofNumber = null,
		string expectedMRNCode = null,
		string expectedExportMRN = null)
	{
		declaration.ZG_DestinationState = MadridCode;
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, messageBody, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(
			message,
			entryHeader,
			typeMessage: "AEAT",
			entryStatusCode: expectedEntryStatus,
			parallel: true,
			acceptanceDate: expectedAcceptanceDate ?? ZDateTime.Empty,
			circuit: expectedCircuit,
			circuitCan: expectedCircuitCan,
			entryReleaseDate: expectedEntryReleaseDate ?? ZDateTime.Empty,
			limitPaymentDate: expectedLimitPaymentDate ?? ZDateTime.Empty,
			atcLimitPaymentDate: expectedAtcLimitPaymentDate ?? ZDateTime.Empty,
			csvClearance: ZString.Empty,
			csvImportCertificate: ZString.Empty,
			paymentProofNumber: expectedPaymentProofNumber,
			atcPaymentProofNumber: expectedAtcPaymentProofNumber,
			mrnCode: expectedMRNCode ?? MRNCode,
			exportMRN: expectedExportMRN ?? ExpectedExportMRN);

		CombineAssertions(
			"For destination not in Canary Islands, if a response is received with green circuit and CSV clearance is empty, trigger the document capture of M031",
			() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 2, docMessages.Length);
				AssertDocumentRequestEDIMessages(docMessages,
					[(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
					(MRNCode + "_I_AEAT_J031.pdf", MRNCode)]);
			});

		declaration.ZG_DestinationState = CanaryIslandCode;
		message = CreateNewEDIMessage(entryHeader.CH_BGMReference, messageBodyATC ?? messageBody, InterchangeID);

		ProcessMessageForTest(message, entryHeader.Messages);
		AssertAcceptedResponseBoth(
			message,
			entryHeader,
			typeMessage: "ATC",
			entryStatusCode: expectedEntryStatus,
			parallel: true,
			acceptanceDate: expectedAcceptanceDate ?? ZDateTime.Empty,
			circuit: expectedCircuit,
			circuitCan: expectedCircuitCan,
			entryReleaseDate: expectedEntryReleaseDate ?? ZDateTime.Empty,
			limitPaymentDate: expectedLimitPaymentDate ?? ZDateTime.Empty,
			atcLimitPaymentDate: expectedAtcLimitPaymentDate ?? ZDateTime.Empty,
			csvClearance: ZString.Empty,
			csvImportCertificate: ZString.Empty,
			paymentProofNumber: expectedPaymentProofNumber,
			atcPaymentProofNumber: expectedAtcPaymentProofNumber,
			mrnCode: expectedMRNCode ?? MRNCode,
			exportMRN: expectedExportMRN ?? ExpectedExportMRN);

		CombineAssertions(
			"For destination in Canary Islands, if a response is received with green circuit and CSV clearance is empty, trigger the document capture of M031 and M032",
			() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 4, docMessages.Length);
				AssertDocumentRequestEDIMessages(docMessages,
					[(MRNCode + "_I_AEAT_M031.pdf", MRNCode),
					(MRNCode + "_I_AEAT_M032.pdf", MRNCode),
					(MRNCode + "_I_AEAT_J031.pdf", MRNCode),
					(MRNCode + "_I_AEAT_J032.pdf", MRNCode)]);
			});
	}

	protected void AddEntryDocuments(IEnumerable<(string FileName, string DocumentType)> files)
	{
		var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
		foreach (var file in files)
		{
			docManagerInfo.AddFileOrDocument(new byte[1], file.FileName, file.DocumentType);
		}
		docManagerInfo.Save();
	}

	protected void AssertLoggerWriteOffTransactionError()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(entryHeader, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFileWithClearance, InterchangeID);

			ProcessMessageForTest(message);
		}

		var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDA", "PDS Response", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Customs Declaration Cleared with pending complementary declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration Accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDP", "Customs Declaration pending for documents (Box 44)", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDI", "Incomplete Pre-Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), EsCode);

		helper.CreateCusCodeListCanaryIsland(EsCode, CanaryIslandCode, $"Test {CanaryIslandCode}");

		entryStatusList = RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);

		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = DeclarantName;
		declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
		declaration.Declarant.OA_OH = declarant.PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

		invoice = declaration.Invoices.AddNew();
		invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "2203001011";
		invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "2203001012";
		invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "2203001013";

		AssertEquals("Merge done", true, declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

		entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_EntryStatus = OriginalEntryStatus;

		entryLine1 = entryHeader.MergedLines[0];
		entryLine1.CL_LineNumber = 1;
		entryLine2 = entryHeader.MergedLines[1];
		entryLine2.CL_LineNumber = 2;
		entryLine3 = entryHeader.MergedLines[2];
		entryLine3.CL_LineNumber = 3;
		feeVAT = entryLine3.Fees.AddNew();
		feeVAT.CF_MethodOfPayment = ZString.Empty;
		feeVAT.G4_Type = RefCusRateCodes.Vat;
		feeNotVAT = entryLine3.Fees.AddNew();
		feeNotVAT.CF_MethodOfPayment = ZString.Empty;
		feeNotVAT.G4_Type = RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;

		SetSentInterchange(entryHeader, InterchangeID);

		Factory.Save();
	}

	protected CusEntryLineFee feeNotVAT;
	protected CusEntryLineFee feeVAT;
	protected CusEntryLine entryLine3;
	protected CusEntryLine entryLine2;
	protected CusEntryLine entryLine1;
	protected JobComInvoiceLine invoiceLine3;
	protected JobComInvoiceLine invoiceLine2;
	protected JobComInvoiceLine invoiceLine1;
	protected JobComInvoiceHeader invoice;
	protected CodeDescriptionPairList entryStatusList;
	protected CusEntryInstruction entryInstruction;

	protected abstract string RejectedTestFile { get; }
	protected abstract string AcceptanceTestFileWithClearance { get; }
	protected virtual string AcceptanceTestFileWithClearanceAEAT => AcceptanceTestFileWithClearance;
	protected virtual string AcceptanceTestFileWithClearanceATC => AcceptanceTestFileWithClearance;
	protected virtual string AcceptanceTestFilePDSResponse => string.Empty;
	protected abstract string AcceptanceTestFileWithoutGuarantees { get; }
	protected abstract string AcceptanceTestFileWithGuaranteesAEAT { get; }
	protected abstract string AcceptanceTestFileWithGuaranteesATC { get; }
	protected abstract ZString RejectedMessageNum { get; }
	protected virtual ZString RejectedEntryStatus => OriginalEntryStatus;
	protected abstract ZString RejectedMessageInterpretation { get; }

	protected void AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFilePDSResponse, InterchangeID);

			ProcessMessageForTest(message);
			AssertPDSAcceptedResponse(message, entryHeader, expectedMessageInterpretation: ZString.Empty, parallel: true);

			AssertNewInboxMessages(entryHeader.Messages, [DeclarationMessageTypeList.Codes.InBoxNotificationForImport], DeclarantId, DeclarantName, MRNCode);
		}
	}

	protected void AssertProcessPDSAcceptedMessageAndTriggerInboxRequestWithPreviousDocument_xT()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFilePDSResponse, InterchangeID);

			ProcessMessageForTest(message);
			AssertPDSAcceptedResponse(message, entryHeader, expectedMessageInterpretation: ZString.Empty, parallel: true);

			AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport }, MRNCode);
		}
	}

	protected void AssertProcessPDSAcceptedMessageAndNotTriggerInboxRequestWhenNoPreviousDocument()
	{
		entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, AcceptanceTestFilePDSResponse, InterchangeID);

		ProcessMessageForTest(message);
		AssertPDSAcceptedResponse(message, entryHeader, expectedMessageInterpretation: ZString.Empty, parallel: true);
		AssertNotEquals("Last message is not Inbox request", "NPI", entryHeader.Messages.LastMessage.EM_MessageType);
	}

	protected abstract bool IsImportQuery { get; }
	protected abstract string ExpectedEntryStatusForInstructionC { get; }
	protected abstract string ExpectedCircuit { get; }
	protected abstract string ExpectedCircuitCan { get; }
	protected abstract ZDateTime ExpectedLimitPaymentDate { get; }
	protected abstract ZDateTime ExpectedATCLimitPaymentDate { get; }
	protected abstract string ExpectedAtcPaymentProofNumber { get; }
	protected abstract string ExpectedPaymentProofNumber { get; }
	protected abstract string ExpectedExportMRN { get; }
	protected abstract string ExpectedMovementReferenceNumberReject { get; }
	protected abstract ZDateTime ExpectedMovementReferenceNumberIssueDateReject { get; }
	protected abstract List<(ZString fileName, ZString urlParameter)> ExpectedDocumentsAEATInstructionC { get; }
	protected abstract int ExpcetedNumberDocMessagesAmountAEATInstructionC { get; }
	protected abstract List<(ZString fileName, ZString urlParameter)> ExpectedDocumentsATCInstructionC { get; }
	protected abstract int ExpcetedNumberDocMessagesAmountATCInstructionC { get; }
	protected abstract string ExpectedentryStatusCode { get; }

	void AssertAcceptedResponseBoth(
		TestEdiMessage message,
		CusEntryHeader entryHeader,
		string typeMessage = "BOTH",
		string expectedMessageInterpretation = "",
		string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
		ZDateTime? acceptanceDate = null,
		bool parallel = false,
		ZString? circuit = null,
		ZString? circuitCan = null,
		ZDateTime? limitPaymentDate = null,
		ZDateTime? atcLimitPaymentDate = null,
		ZDateTime? entryReleaseDate = null,
		ZString? csvClearance = null,
		ZString? csvImportCertificate = null,
		ZString? paymentProofNumber = null,
		ZString? atcPaymentProofNumber = null,
		ZString? mrnCode = null,
		ZString? exportMRN = null
		)
	{
		if (IsImportQuery)
		{
			switch (typeMessage)
			{
				case "AEAT":
					GenericCommonAssertProcessEntryData(
						message,
						entryHeader,
						expectedMessageInterpretation: expectedMessageInterpretation,
						messageNum: MessageNum,
						messageSubType: "ACC",
						entryStatusCode: entryStatusCode,
						circuit: circuit ?? ExpectedCircuit,
						acceptanceDate: acceptanceDate,
						entryReleaseDate: entryReleaseDate ?? EntryReleaseDate,
						limitPaymentDate: limitPaymentDate ?? ExpectedLimitPaymentDate,
						csvClearance: csvClearance ?? CsvClearance,
						csvImportCertificate: csvImportCertificate ?? CsvImportCertificate,
						parallel: parallel,
						movementReferenceNumber: mrnCode ?? MRNCode,
						paymentProofNumber: paymentProofNumber ?? ExpectedPaymentProofNumber,
						exportMRN: exportMRN ?? ExpectedExportMRN);
					break;
				case "ATC":
					GenericCommonAssertProcessEntryData(
						message,
						entryHeader,
						expectedMessageInterpretation: expectedMessageInterpretation,
						messageNum: MessageNum,
						messageSubType: "ACC",
						entryStatusCode: entryStatusCode,
						acceptanceDate: acceptanceDate,
						entryReleaseDate: entryReleaseDate ?? EntryReleaseDate,
						limitPaymentDate: limitPaymentDate,
						csvClearance: csvClearance ?? CsvClearance,
						csvImportCertificate: csvImportCertificate ?? CsvImportCertificate,
						circuit: circuit ?? ZString.Empty,
						circuitCan: circuitCan ?? ExpectedCircuitCan,
						atcLimitPaymentDate: atcLimitPaymentDate ?? ExpectedATCLimitPaymentDate,
						parallel: parallel,
						movementReferenceNumber: mrnCode ?? MRNCode,
						paymentProofNumber: paymentProofNumber ?? ZString.Empty,
						atcPaymentProofNumber: atcPaymentProofNumber ?? ExpectedAtcPaymentProofNumber,
						exportMRN: exportMRN ?? ExpectedExportMRN);
					break;
				case "BOTH":
					GenericCommonAssertProcessEntryData(
						message,
						entryHeader,
						expectedMessageInterpretation: expectedMessageInterpretation,
						messageNum: MessageNum,
						messageSubType: "ACC",
						entryStatusCode: entryStatusCode,
						acceptanceDate: acceptanceDate,
						entryReleaseDate: entryReleaseDate ?? EntryReleaseDate,
						circuit: circuit ?? ExpectedCircuit,
						paymentProofNumber: paymentProofNumber ??  ExpectedPaymentProofNumber,
						limitPaymentDate: ExpectedLimitPaymentDate,
						csvClearance: csvClearance ?? CsvClearance,
						csvImportCertificate: csvImportCertificate ?? CsvImportCertificate,
						circuitCan: circuitCan ?? ExpectedCircuitCan,
						atcLimitPaymentDate: atcLimitPaymentDate ?? ExpectedATCLimitPaymentDate,
						parallel: parallel,
						movementReferenceNumber: mrnCode ?? MRNCode,
						atcPaymentProofNumber: ExpectedAtcPaymentProofNumber,
						exportMRN: exportMRN ?? ExpectedExportMRN);
					break;
				default:
					break;
			}
		}
		else
		{
			GenericCommonAssertProcessEntryData(
				message,
				entryHeader,
				expectedMessageInterpretation:
				expectedMessageInterpretation,
				messageNum: MessageNum,
				messageSubType: "ACC",
				entryStatusCode: entryStatusCode,
				acceptanceDate: acceptanceDate,
				entryReleaseDate: entryReleaseDate ?? EntryReleaseDate,
				circuit: circuit ?? ExpectedCircuit,
				paymentProofNumber: paymentProofNumber ?? ExpectedPaymentProofNumber,
				limitPaymentDate: limitPaymentDate ?? ExpectedLimitPaymentDate,
				csvClearance: csvClearance ?? CsvClearance,
				csvImportCertificate: csvImportCertificate ?? CsvImportCertificate,
				circuitCan: circuitCan ?? ExpectedCircuitCan,
				atcLimitPaymentDate: atcLimitPaymentDate ?? ExpectedATCLimitPaymentDate,
				parallel: parallel,
				movementReferenceNumber: MRNCode,
				atcPaymentProofNumber: atcPaymentProofNumber ?? ExpectedAtcPaymentProofNumber,
				exportMRN: exportMRN ?? ExpectedExportMRN);
		}
	}
	void AssertPDSAcceptedResponse(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.PreDeclarationAccepted, bool parallel = false)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", entryStatusCode: entryStatusCode, acceptanceDate: AcceptanceDate, limitPaymentDate: ExpectedLimitPaymentDate, parallel: parallel, movementReferenceNumber: MRNCode);
	}

	protected const string MRNCode = "20ES00999930006184";
	protected const string MessageNum = "TARIC20201020141554350002";
	protected readonly ZDateTime AcceptanceDate = new (2020, 11, 04);
	protected readonly ZDateTime EntryReleaseDate = new (2020, 11, 04);
	protected readonly ZDateTime LimitPaymentDate = new (2021, 06, 07);
	protected readonly ZDateTime AtcLimitPaymentDate = new (2021, 06, 07);
	protected readonly ZDateTime MovementReferenceNumberIssueDate = new (2021, 01, 15, 05, 40, 55);
	protected const string CsvClearance = "TEST444444444444";
	protected const string CsvImportCertificate = "CRTF444444444444";
	protected const string AtcPaymentProofNumber = "JUSTPAGOATC";
	protected const string PaymentProofNumber = "1234";
	protected const string ExportMRN = "20EXP0999930006184";
	const string TransactionCommentPrefix = "IMP";

	const string MadridCode = "28";
	const string CanaryIslandCode = "61";

	protected void AddFeesToEntryLine()
	{
		var feeAEAT = entryLine1.Fees.AddNew();
		feeAEAT.CF_ChargeType = "A00";
		feeAEAT.CF_BaseValue = 200.650M;
		feeAEAT.CF_Rate = 3.700000M;
		feeAEAT.MaxMin = "MA";
		feeAEAT.G4_RateDuty = ZString.Empty;
		feeAEAT.CF_ChargeAmount = 3.72M;

		var feeATC = entryLine1.Fees.AddNew();
		feeATC.CF_ChargeType = "300";
		feeATC.CF_BaseValue = 200.650M;
		feeATC.CF_Rate = 3.700000M;
		feeATC.MaxMin = "MA";
		feeATC.G4_RateDuty = ZString.Empty;
		feeATC.CF_ChargeAmount = 3.72M;
	}
}
