using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CANPreDUAImportResponseMessageProcessorTest : XMLResponseMessageProcessorTest<CANPreDUAImportResponseMessageProcessor, IMessagePrettyFormatter, AnulaImportacionV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretation = "<H3>Accepted Cancellation</H3>";
			AssertCancelResponse(message, expectedMessageInterpretation: expectedMessageInterpretation);
		}

		public void TestProcessMessageRejected()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretation = "<H3>Rejected Cancellation</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>0301</td><td>El documento no es valido o no es Predeclaración Incompleta, Predeclaración Completa o Predeclaración Simplificada</td></tr>" +
						"</table>";
			AssertCancelResponse(message, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ", entryStatusCode: initialEntryStatus);
		}

		public void TestGuaranteesAmount0WhenProcessingAcceptedMessage()
		{
			AddGuarantees(entryInstruction.PK, MRNCode, entryHeader.CH_BGMReference, addTransactions: false);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertCancelResponse(message);

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
			AssertCancelResponse(message, entryStatusCode: initialEntryStatus, messageSubType: "REJ");

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
			AssertCancelResponse(message);

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
			AssertCancelResponse(message);

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
			AssertCancelResponse(message);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
				AssertEquals("Only OBL transaction for first guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).CusGuaranteeLineTransactions.Count);
				AssertEquals("Only OBL transaction for second guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).CusGuaranteeLineTransactions.Count);
			});
		}

		void AssertCancelResponse(TestEdiMessage message, string expectedMessageInterpretation = "", string messageSubType = "ACC", string entryStatusCode = EntryStatusCodes.Cancelled)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageNum: MessageNum, entryStatusCode: entryStatusCode, movementReferenceNumber: MRNCode);
		}
		const string MessageNum = "TARIC20201020141554350002";
		const string MRNCode = "20ES00999930006184";
		readonly ZString initialEntryStatus = "SNT";
		const string TransactionCommentPrefix = "IMP";

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;

			entryHeader.CH_EntryStatus = initialEntryStatus;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumber = MRNCode;

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Declaration Canceled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "SNT", "Sent", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			entryStatusList = RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}
		protected CodeDescriptionPairList entryStatusList;
		protected CusEntryInstruction entryInstruction;

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CANPreDUAImportTestFilePath, "AcceptedMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CANPreDUAImportTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CANPreDUAImportTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override CANPreDUAImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CANPreDUAImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Cancel Pre SAD Import Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation };
	}
}
