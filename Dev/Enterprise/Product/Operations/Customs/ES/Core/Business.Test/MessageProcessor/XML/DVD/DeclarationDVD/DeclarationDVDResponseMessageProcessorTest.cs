using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDResponseMessageProcessorTest : XMLResponseMessageProcessorTest<DeclarationDVDResponseMessageProcessor, DeclarationDVDMessagePrettyFormatter, Dvdh2V1Sal>
	{
		public void TestProcessAcceptedMessage_CLR_GreenCircuit()
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
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
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
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
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
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

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
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

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
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

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
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

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
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

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
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

				AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 }, DeclarantId, DeclarantName, MRNCode);
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
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<br><br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

				AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, expectedMessageInterpretation: expectedMessageInterpretationText);

				AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 }, MRNCode);
			}
		}

		public void TestProcessRejectedMessage()
		{
			AddMessageProcessAndAssertResult_RejectedMessage();
		}

		public void TestGuaranteesAmount0WhenNoGuaranteesInResponse()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithoutGuarantees(), InterchangeID);

			ProcessMessageForTest(message);

			Factory.Save();
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.Cleared, csvClearance: CsvClearance, entryReleaseDate: entryReleaseDate);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 110m, acceptanceDate);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 600m, acceptanceDate);
			});
		}

		public void TestGuaranteeTransactionsWhenNoGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transaction + 1 new transaction for first guarantee", 2, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's new transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, -522.51m, acceptanceDate, false);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transaction + 1 new transaction for second guarantee", 2, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, -551.55m, acceptanceDate, false);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithoutCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false, addOBLTransaction: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileWithGuaranteesATC(), InterchangeID);

			ProcessMessageForTest(message);
			AssertDVDAcceptedResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuitCan: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, -412.51m, acceptanceDate);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 48.45m, acceptanceDate);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenGuaranteesInResponse_WithCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, setPositiveTranAmount: true);
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
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
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

		public void TestProcessAcceptedMessageCreatesSupportingDocumentsDuplicates()
		{
			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc2 = declaration.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000002";

			var suppDoc21 = declaration.SupportingDocuments.AddNew();
			suppDoc21.CSI_Code = "X002";
			suppDoc21.CSI_ReferenceNumber = "es3600000002";

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;
			var suppDoc3 = entryInstruction.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X001";
			suppDoc3.CSI_ReferenceNumber = "ES3600000001";

			var suppDoc4 = declaration.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "X003";
			suppDoc4.CSI_ReferenceNumber = "ES3600000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("Before - There are 0 SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 0, chSupDocs.Length);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);
				ProcessMessageForTest(message);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;
				var suppDoc5 = entryInstruction1.SupportingDocuments.AddNew();
				suppDoc5.CSI_Code = "X002";
				suppDoc5.CSI_ReferenceNumber = "ES3600000002";

				var suppDoc51 = entryInstruction1.SupportingDocuments.AddNew();
				suppDoc51.CSI_Code = "X002";
				suppDoc51.CSI_ReferenceNumber = "es3600000002";

				var message1 = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);
				ProcessMessageForTest(message1);

				chSupDocs = GetCHSupportingDocuments();
				AssertEquals("There are 3 CH SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 3, chSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
			});
		}

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			var suppDoc1 = declaration.SupportingDocuments.AddNew();
			suppDoc1.CSI_Code = "X001";
			suppDoc1.CSI_ReferenceNumber = "ES3600000006";

			var suppDoc2 = declaration.SupportingDocuments.AddNew();
			suppDoc2.CSI_Code = "X002";
			suppDoc2.CSI_ReferenceNumber = "ES3600000007";

			var suppDoc3 = entryInstruction.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "X003";
			suppDoc3.CSI_ReferenceNumber = "ES3600000008";

			var suppDoc4 = invoice.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "X004";
			suppDoc4.CSI_ReferenceNumber = "ES3600000009";

			var suppDoc5 = invoiceLine.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "X005";
			suppDoc5.CSI_ReferenceNumber = "ES3600000010";

			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000001", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000002", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("BBB", "ES3600000003", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000004", subType: ZString.Empty, status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("AAAA", "ES3600000005", subType: "LIQ", status: "ACC");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("5018", "ES3600000021", subType: "LIQ", status: "ACC");
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("5018", "ES3600000022", subType: "LIQ", status: ZString.Empty);
			entryLine.Header.AddEntryHeaderDocument<SupportingDocument>("BBB", "ES3600000023", subType: "LIQ", status: "REJ");

			Factory.Save();

			CombineAssertions("Before", () =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 5 CL SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 5, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000001"), "5018", "ES3600000001", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000002"), "5018", "ES3600000002", "LIQ", ZString.Empty);
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000003"), "BBB", "ES3600000003", "LIQ", "REJ");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000004"), "5018", "ES3600000004", ZString.Empty, ZString.Empty);
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000005"), "AAAA", "ES3600000005", "LIQ", "ACC");

				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("There are 3 CH SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments", 3, chSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 3 CH SupportingDocuments before calling ProcessImportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000021", "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000021"), "5018", "ES3600000021", "LIQ", "ACC");
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000022"), "5018", "ES3600000022", "LIQ", ZString.Empty);
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000023"), "BBB", "ES3600000023", "LIQ", "REJ");
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);
			ProcessMessageForTest(message);

			CombineAssertions("For CL",() =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 4 CL SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 4, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000002", "ES3600000003", "ES3600000009", "ES3600000010" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000002"), "5018", "ES3600000002", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000003"), "BBB", "ES3600000003", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000009"), "X004", "ES3600000009", ZString.Empty, "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000010"), "X005", "ES3600000010", ZString.Empty, "ACC");
			});
			CombineAssertions("For CH", () =>
			{
				var chSupDocs = GetCHSupportingDocuments();
				AssertEquals("There are 5 CH SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments", 5, chSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 5 CH SupportingDocuments after calling ProcessImportEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000006", "ES3600000007", "ES3600000008", "ES3600000022", "ES3600000023" }, chSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000022"), "5018", "ES3600000022", "LIQ", "ACC");
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000023"), "BBB", "ES3600000023", "LIQ", "ACC");
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000006"), "X001", "ES3600000006", ZString.Empty, "ACC");
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000007"), "X002", "ES3600000007", ZString.Empty, "ACC");
				AssertDocument(chSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000008"), "X003", "ES3600000008", ZString.Empty, "ACC");
			});
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

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_D_AEAT_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_NoDocs()
		{
			declaration.ZG_IsTrainingDeclaration = false;

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_D_AEAT_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				var eDocNames = new List<ZString>() { MRNCode + "_D_AEAT_CLR.pdf" };
				AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

				entryHeader.Messages.Reload(true);
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				AddMessageProcessAndAssertResult_RejectedMessage(false);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestLoggerWriteOffTransactionError()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(entryHeader, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileGreenCircuitMessageAEAT(), InterchangeID);

				ProcessMessageForTest(message);
			}

			var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
			AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
		}

		void AddMessageProcessAndAssertResult_RejectedMessage(bool isEntryStatusEmpty = false)
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

			AssertDVDResponse(responseMessage, entryStatusCode: isEntryStatusEmpty ? ZString.Empty : OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		void AssertDVDAcceptedResponse(TestEdiMessage message, string entryStatusCode, string circuit = CircuitCodeList.Codes.GREEN, string circuitCan = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "")
		{
			AssertDVDResponse(message, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, mrn: MRNCode, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "ACC");
		}

		void AssertDVDResponse(TestEdiMessage message, string entryStatusCode, string messageSubType, string circuit = "", string circuitCan = "", ZDateTime? acceptanceDate = null, string mrn = "", string csvClearance = "", ZDateTime? entryReleaseDate = null, string expectedMessageInterpretation = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, circuitCan: circuitCan, acceptanceDate: acceptanceDate, movementReferenceNumber: mrn, csvClearance: csvClearance, entryReleaseDate: entryReleaseDate, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			declaration.Declarant.OA_OH = declarant.PK;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;

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
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		string GetAcceptanceTestFileGreenCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileRedCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedRedCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileRedCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedRedCircuitMessageATC.txt");
		string GetAcceptanceTestFileOrangeCircuitMessageAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedOrangeCircuitMessageAEAT.txt");
		string GetAcceptanceTestFileGreenCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageATC.txt");
		string GetAcceptanceTestFileOrangeCircuitMessageATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedOrangeCircuitMessageATC.txt");
		string GetAcceptanceTestFilePDAPresMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedPDCPresMessage.txt");
		string GetAcceptanceTestFilePDAAmendMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedPDCAmendMessage.txt");
		string GetAcceptanceTestFileWithoutGuarantees() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");
		string GetAcceptanceTestFileWithGuaranteesAEAT() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageGuaranteesAEAT.txt");
		string GetAcceptanceTestFileWithGuaranteesATC() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageGuaranteesATC.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MRNCode = "22ES009999D04136R3";
		const string MessageNum = "20220923121419948549";
		readonly ZDateTime acceptanceDate = new ZDateTime(2022, 09, 23, 12, 12, 54);
		readonly ZDateTime entryReleaseDate = new ZDateTime(2022, 10, 02, 13, 12, 40);
		const string CsvClearance = "MYT5CUUEVP4QF7CJ";
		const string TransactionCommentPrefix = "DVD";

		protected override ZString GetExpectedProcessorFriendlyName() => "DVD (H2) Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2 };

		protected override DeclarationDVDResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DeclarationDVDResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
