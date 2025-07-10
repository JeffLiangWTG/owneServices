using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC014C_v515.CC014CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class CancelNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<CancelNCTSResponseMessageProcessor, CancelNCTSMessagePrettyFormatter, Cc014Cv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			ProcessAndAssertAcceptedMessage();
		}

		public void TestMessageProcessingErrorHeaderNotDeparture()
		{
			AddGuarantees(addTransactions: true);

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			responseMessage.EM_LinkedObject = nctsHeader;

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretation = string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Header type is A so can't process Departure response message</H4>", responseMessage.EM_MessageNum, responseMessage.EM_MessageType, responseMessage.EM_MessageSubType, responseMessage.EM_ApplicationReference);

			AssertNCTSDeclaration(responseMessage, messageSubType: "AAA", emStatus: "FAL", expectedMessageInterpretation: expectedMessageInterpretation, commonCustomsStatus: OriginalEntryStatus, messageStatus: "FAL", mrnEntrynum: MRNCode);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for first guarantee", 13, guarantee1Transactions.Count());

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for second guarantee", 13, guarantee2Transactions.Count());
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingAcceptedMessage_WithCONTransactions()
		{
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			AddGuarantees(addTransactions: true);

			ProcessAndAssertAcceptedMessage(MRNCode);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", TransactionCommentPrefix, guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, nctsHeader.BH_JobReference, 110m, new ZDateTime(2022, 12, 12), false, true);

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", TransactionCommentPrefix, guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(TransactionCommentPrefix)), MRNCode, nctsHeader.BH_JobReference, 600m, new ZDateTime(2022, 12, 12), false, true);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingAcceptedMessage_WithCONTransactions_WithoutOpeningBalance()
		{
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			AddGuarantees(addTransactions: true, addOBLTransaction: false);

			ProcessAndAssertAcceptedMessage(MRNCode);

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
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			AddGuarantees(addTransactions: false);

			ProcessAndAssertAcceptedMessage(MRNCode);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
				AssertEquals("Only OBL transaction for first guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).CusGuaranteeLineTransactions.Count);
				AssertEquals("Only OBL transaction for second guarantee", 1, guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).CusGuaranteeLineTransactions.Count);
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingRejectedMessage_WithCONTransactions()
		{
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			AddGuarantees(addTransactions: true);

			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr></table>";

			AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, mrnEntrynum: MRNCode, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for first guarantee", 13, guarantee1Transactions.Count());

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for second guarantee", 13, guarantee2Transactions.Count());
			});
		}

		public void TestGuaranteeTransactionsAmountWhenProcessingErrorMessage_WithCONTransactions()
		{
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, MRNCode, ZString.Empty);
			AddGuarantees(addTransactions: true);

			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
				"</table>";

			AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, mrnEntrynum: MRNCode, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);

			CombineAssertions(() =>
			{
				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for first guarantee", 13, guarantee1Transactions.Count());

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions only for second guarantee", 13, guarantee2Transactions.Count());
			});
		}

		void ProcessAndAssertAcceptedMessage(string mrn = "")
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>12-12-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>SYLLV39DBRPW3DKV</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.Cancelled, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration, mrnEntrynum: mrn);
		}

		void AddGuarantees(bool addTransactions = false, bool addOBLTransaction = true, bool setPositiveTranAmount = false)
		{
			var balance = addTransactions ? 1000m + (-50m - 200) : 1000m;
			var transactionAmount = setPositiveTranAmount ? 150m : -50m;
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, MRNCode, ApplicationReference, transactionAmount, 1000m, balance, addTransactions, addOBLTransaction);
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondNumber = GuaranteeReference;

			balance = addTransactions ? 1000m + (-540m - 200) : 1200m;
			transactionAmount = setPositiveTranAmount ? 640m : -540m;
			SetUpGuarantee(OtherGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, MRNCode, ApplicationReference, transactionAmount, 1200m, balance, addTransactions, addOBLTransaction);
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondNumber = OtherGuaranteeReference;
		}

		const string MRNCode = "20ES00999930006184";
		const string GuaranteeReference = "16ESAGL9990000096";
		const string OtherGuaranteeReference = "17ESAGL9990000097";
		const string TransactionCommentPrefix = "NCTS Departure";

		protected override ZString PhaseStatusWhenErrorOrRejected => ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Cancel Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation };

		string GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelNCTSTestFilePath, "AcceptedMessage.txt");
		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelNCTSTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelNCTSTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override CancelNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CancelNCTSResponseMessageProcessor(logger);
	}
}
