using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ActivaPDCVinculacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationDVDResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationDVDResponseMessageProcessor, InboxNotificationDVDMessagePrettyFormatter, ActivaPdcVinculacionV1Sal>
	{
		public void TestProcessAcceptedMessage_CLR_GreenCircuit()
		{
			AddMessageProcessAndAssertResult_CLR_GreenCircuit();
		}

		public void TestProcessAcceptedMessage_CLP_GreenCircuit()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>02-10-2022, 13:12:40</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>" +
				"<br><H2>Guarantees</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGT9990000140</td><td>0.31</td></tr></table>" +
				"<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
				"<tr><td>00001</td><td>0.00</td><td>0.31</td><td>0.00</td><td>0.31</td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CLR_RedCircuit()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileRedCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.RED, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CLR_RedCircuit_ATC()
		{
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.Z;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileRedCircuitMessageATC(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, circuitCan: CircuitCodeList.Codes.RED, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CDA_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.ORANGE, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CDA_GreenCircuit_ATC()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageATC(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_CDA_OrangeCircuit_ATC()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOrangeCircuitMessageATC(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.ORANGE, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_PDA_Presentation_AndTriggerInboxRequest_EHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDAPresMessage(), InterchangeID);

				ProcessMessageForTest(message);
				var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
					"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
					"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(1) Pre-Declaration Accepted</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";

				AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 }, DeclarantId, DeclarantName, MRNCodeClearedNotPending);
			}
		}

		public void TestProcessAcceptedMessage_PDA_Amendment_AndTriggerInboxRequest_xT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFilePDAAmendMessage(), InterchangeID);

				ProcessMessageForTest(message);
				var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
					"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
					"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(2) Pre-Declaration Modification</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";

				AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 }, MRNCodeClearedNotPending);
			}
		}

		public void TestProcessRejectedMessage()
		{
			AddMessageProcessAndAssertResult_RejectedMessage();
		}

		public void TestGuaranteesAmount0WhenNoGuaranteesInResponse()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				AssertEquals("First Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
				AssertEquals("First Guarantee with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
				AssertEquals("Second Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
			});
		}

		public void TestGuaranteesAmountWhenGuaranteesInResponse_AEAT()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				AssertEquals("First Guarantee with entry instruction of entry has amount changed to the one in the response", 110m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
				AssertEquals("First Guarantee with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
				AssertEquals("Second Guarantee with entry instruction of entry has amount changed to the one in the response", 600m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
			});
		}

		public void TestGuaranteesAmountWhenGuaranteesInResponse_ATC()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);

			Factory.Save();
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, 110m, acceptanceDate);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, 600m, acceptanceDate);
			});
		}

		public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);

			Factory.Save();
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);

			Factory.Save();
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
				AssertEquals("Only OBL transaction for first guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).CusGuaranteeLineTransactions.Count);
				AssertEquals("Only OBL transaction for second guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithoutCONTransactions()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transaction + 1 new transaction for first guarantee", 2, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's new transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, -522.51m, acceptanceDate, false);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transaction + 1 new transaction for second guarantee", 2, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, -551.55m, acceptanceDate, false);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithoutCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: false, addOBLTransaction: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, -412.51m, acceptanceDate);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCodeClearedNotPending, entryHeader.CH_BGMReference, 48.45m, acceptanceDate);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

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
			AddGuarantees(entryInstruction.PK, MRNCodeClearedNotPending, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Only original transactions for first guarantee", 13, guarantee1Transactions.Count());
				AssertEquals("No new DVD transaction for first guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)));

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Only original transactions for second guarantee", 13, guarantee1Transactions.Count());
				AssertEquals("No new DVD transaction for second guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)));
			});
		}

		void AssertDVDAcceptedResponse(TestEdiMessage message, string entryStatusCode, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "")
		{
			AssertDVDResponse(message, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC");
		}

		void AssertDVDResponse(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: MRNCodeClearedNotPending, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, messageNum: MessageNum);
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_AllDocs()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCodeClearedNotPending + "_D_AEAT_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_NoDocs()
		{
			declaration.ZG_IsTrainingDeclaration = false;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_D_AEAT_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNames = new List<ZString>() { MRNCodeClearedNotPending + "_D_AEAT_CLR.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_CLR_GreenCircuit);
		}

		public void TestProcessRejectedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_RejectedMessage);
		}

		void AddMessageProcessAndAssertResult_CLR_GreenCircuit()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>02-10-2022, 13:12:40</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>" +
				"<br><H2>Guarantees</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>16ESAGT9990000140</td><td>0.31</td></tr></table>" +
				"<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
				"<tr><td>00001</td><td>0.00</td><td>0.31</td><td>0.00</td><td>0.31</td></tr></table>";

			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		void AddMessageProcessAndAssertResult_RejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>6322</td><td>El identificador del operador no es un EORI valido </td><td>CABECERA.ED_3_16_NumIdentifDepositante</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr>" +
				"</table>";

			AssertDVDResponse(responseMessage, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			declaration.Declarant.OA_OH = declarant.PK;

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Customs Declaration Cleared with pending complementary declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDA", "Customs PDA Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		CusEntryInstruction entryInstruction;

		string GetAcceptanceTestFileGreenCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileRedCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedRedCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileRedCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedRedCircuitMessageATC.txt");
		string GetAcceptanceTestFileOrangeCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedOrangeCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileGreenCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageATC.txt");
		string GetAcceptanceTestFileOrangeCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedOrangeCircuitMessageATC.txt");
		string GetAcceptanceTestFilePDAPresMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedPDCPresMessage.txt");
		string GetAcceptanceTestFilePDAAmendMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedPDCAmendMessage.txt");
		string GetAcceptanceTestFileWithoutGuarantees() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		string GetAcceptanceTestFileWithGuaranteesAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		string GetAcceptanceTestFileWithGuaranteesATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCodeClearedNotPending = "22ES009999D04136R3";
		const string MessageNum = "20220923121419948549";
		readonly ZDateTime acceptanceDate = new ZDateTime(2022, 09, 23, 12, 12, 54);
		readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 10, 02, 13, 12, 40);
		const string CsvClearance = "MYT5CUUEVP4QF7CJ";
		const string TransactionCommentPrefix = "DVD";

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFileGreenCircuitMessageAEAT();

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFileGreenCircuitMessageAEAT();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongXML()
		{
			AssertContains("logger", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
		}

		protected override void AssertLoggerMessagesWhenMessageProcessingError()
		{
			base.AssertLoggerMessagesWhenMessageProcessingError();

			AssertContains("logger exception", "Message Text is empty so can't continue with processing", GetAllConcatenatedUserLogStrings());
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification DVD (H2) Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 };

		protected override InboxNotificationDVDResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationDVDResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
