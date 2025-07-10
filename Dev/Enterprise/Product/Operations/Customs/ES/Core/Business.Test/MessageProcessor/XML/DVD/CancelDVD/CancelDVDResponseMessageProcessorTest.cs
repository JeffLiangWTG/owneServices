using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CancelDVDResponseMessageProcessorTest : XMLResponseMessageProcessorTest<CancelDVDResponseMessageProcessor, CancelDVDMessagePrettyFormatter, AnulaPdcVinculacionV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
				"<table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>";

			AssertCancelDVDResponse(message, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessRejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>261</td><td>MRN de DVD inexistente </td><td>MSJ.MRN_Operacion</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr>" +
				"</table>";

			AssertCancelDVDResponse(responseMessage, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestGuaranteesAmount0WhenProcessingAcceptedMessage()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelDVDResponse(message);

			CombineAssertions(() =>
			{
				AssertEquals("First Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
				AssertEquals("First Guarantee with entry instruction not of entry has amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
				AssertEquals("Second Guarantee with entry instruction of entry has amount 0", 0m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
			});
		}

		public void TestGuaranteesAmountNotChangedWhenProcessingRejectedMessage()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelDVDResponse(message, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ");

			CombineAssertions(() =>
			{
				AssertEquals("First Guarantee with entry instruction of entry amount is not changed", 1000m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID == entryInstruction.PK).PW_BondAmount);
				AssertEquals("First Guarantee with entry instruction not of entry amount is not changed", 1500m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == GuaranteeReference && x.EntryInstructionID != entryInstruction.PK).PW_BondAmount);
				AssertEquals("Second Guarantee with entry instruction of entry amount is not changed", 1200m, declaration.Guarantees.Cast<ESGuarantee>().First(x => x.PW_BondNumber == OtherGuaranteeReference).PW_BondAmount);
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestGuaranteeTransactionsAmountWhenProcessingAcceptedMessage_WithCONTransactions()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelDVDResponse(message);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 110m, new ZDateTime(2021, 10, 05), false, true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, entryHeader.CH_BGMReference, 600m, new ZDateTime(2021, 10, 05), false, true);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingAcceptedMessage_WithCONTransactions_WithoutOpeningBalance()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: true, addOBLTransaction: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelDVDResponse(message);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions for first guarantee", 12, guarantee1Transactions.Count());

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions for second guarantee", 12, guarantee2Transactions.Count());
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingAcceptedMessage_WithoutCONTransactions()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelDVDResponse(message);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
				AssertEquals("Only OBL transaction for first guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).CusGuaranteeLineTransactions.Count);
				AssertEquals("Only OBL transaction for second guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).CusGuaranteeLineTransactions.Count);
			});
		}

		void AssertCancelDVDResponse(TestEdiMessage message, string entryStatusCode = EntryStatusCodes.Cancelled, string messageSubType = "ACC", string expectedMessageInterpretation = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, movementReferenceNumber: MRNCode, messageNum: MessageNum);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.MovementReferenceNumber = MRNCode;

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		protected CusEntryInstruction entryInstruction;

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelDVDTestFilePath, "AcceptedMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelDVDTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelDVDTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MessageNum = "20220920161643601219";
		const string MRNCode = "20ES00999930006184";
		const string TransactionCommentPrefix = "DVD";

		protected override ZString GetExpectedProcessorFriendlyName() => "DVD (H2) Cancellation Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2Cancellation };

		protected override CancelDVDResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CancelDVDResponseMessageProcessor(logger);
	}
}
