using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC009CMessageProcessor))]
	sealed class CC009CMessageProcessorTest : MessageProcessorTestCase<CC009CMessageProcessor, ICC009CDataProvider>
	{
		public void TestTransactionsDecisionNegative() => CombineAssertions(() =>
		{
			SetupC0009ForEuAndCtCountries(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			movementHeader.BM_PaperlessInbondNum = "LRN1234567";
			mockDataProvider.SetupGet(m => m.Decision).Returns(false);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var transactionOBL = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionOBL.CPL_Reference = "OPENING";
			transactionOBL.CPL_TranValue = 1000m;
			transactionOBL.CPL_Comment = "OPENING";
			transactionOBL.CPL_IsAggregated = true;
			transactionOBL.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

			var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionPND.CPL_Reference = movementHeader.BM_PaperlessInbondNum;
			transactionPND.CPL_TranValue = -20m;
			transactionPND.CPL_IsAggregated = true;
			transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionPND.CPL_TransactionStatus = "PND";
			guaranteeHeader.CPH_Balance = 980;

			var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionCON.CPL_Reference = movementHeader.BM_PaperlessInbondNum;
			transactionCON.CPL_TranValue = -500m;
			transactionCON.CPL_IsAggregated = true;
			transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionCON.CPL_TransactionStatus = "CON";

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 150;

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("amount of guarantees", 3, guaranteeHeader.GetTransactions().Count());
			AssertEquals("status con unchanged", PermitTransactionStatusList.Codes.Confirmed, transactionCON.CPL_TransactionStatus);
			AssertEquals("status pnd unchanged", PermitTransactionStatusList.Codes.Pending, transactionPND.CPL_TransactionStatus);
		});

		public void TestTransactionsDecisionPositive() => CombineAssertions(() =>
		{
			SetupC0009ForEuAndCtCountries(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			movementHeader.BM_PaperlessInbondNum = "LRN1234567";
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var transactionOBL = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionOBL.CPL_Reference = "OPENING";
			transactionOBL.CPL_TranValue = 1000m;
			transactionOBL.CPL_Comment = "OPENING";
			transactionOBL.CPL_IsAggregated = true;
			transactionOBL.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

			var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionPND.CPL_Reference = movementHeader.BM_PaperlessInbondNum;
			transactionPND.CPL_TranValue = -20m;
			transactionPND.CPL_IsAggregated = true;
			transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionPND.CPL_TransactionStatus = "PND";
			guaranteeHeader.CPH_Balance = 980;

			var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionCON.CPL_Reference = movementHeader.BM_PaperlessInbondNum;
			transactionCON.CPL_TranValue = -500m;
			transactionCON.CPL_IsAggregated = true;
			transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionCON.CPL_TransactionStatus = "CON";

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 150;

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			var newTransaction = guaranteeHeader.CusGuaranteeLineTransactions.FirstOrDefault(t => t.PK != transactionOBL.PK && t.PK != transactionPND.PK && t.PK != transactionCON.PK);
			AssertEquals("amount of transactions", 4, guaranteeHeader.GetTransactions().Count());
			AssertEquals("status con unchanged", PermitTransactionStatusList.Codes.Confirmed, transactionCON.CPL_TransactionStatus);
			AssertEquals("status pnd changed", PermitTransactionStatusList.Codes.Deleted, transactionPND.CPL_TransactionStatus);
			AssertNotNull("new transaction is created", newTransaction);
			AssertEquals("new transaction status", PermitTransactionStatusList.Codes.Confirmed, newTransaction?.CPL_TransactionStatus);
			AssertEquals("new transaction amount", 500M, newTransaction?.CPL_TranValue);
			AssertEquals("new transaction reference", movementHeader.BM_PaperlessInbondNum, newTransaction?.CPL_Reference);
		});

		public static void SetupC0009ForEuAndCtCountries(BusinessObjectFactory factory)
		{
			SetupC0009ForCountries(factory, factory.GetEuropeanUnionAndCtCountries().ToArray());
		}

		public static void SetupC0009ForCountries(BusinessObjectFactory factory, params string[] countries)
		{
			Universal.Testing.UniversalReferenceTestDataHelper universalReferenceTestDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			RefDataGrouping parent = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType("C0009", "C0009 Desc", "ZZ", 0);
			foreach (string text in countries)
			{
				universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(text, null, parent);
				universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeList("EUN", "C0009", text, text, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}

			factory.Save();
		}

		public void TestMessageTypesToInclude()
		{
			AssertEquals(1, Processor.MessageTypesToInclude.Count);
			AssertCollectionContains(BEIncomingMessageTypes.Codes.CC009C, Processor.MessageTypesToInclude);
		}

		public void TestPreProcessCC009C_NonMatchingLRN_NonMatchingMRN()
		{
			mockDataProvider.Setup(x => x.LRN).Returns(InvalidLRN);
			mockDataProvider.Setup(x => x.MRN).Returns(InvalidMRN);

			processor.PreProcessMessage(incomingMessage);

			AssertEquals(EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		public void TestPreProcessCC009C_MatchingLRN_InvalidBM_SubApplicationCode()
		{
			movementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();
			mockDataProvider.Setup(x => x.MRN).Returns(InvalidMRN);

			processor.PreProcessMessage(incomingMessage);

			AssertEquals(EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		public void TestPreProcessCC009C_MatchingLRN_ValidBM_SubApplicationCode()
		{
			processor.PreProcessMessage(incomingMessage);

			AssertEquals(EDIMessageStatusList.Codes.PreProcessedOK, incomingMessage.EM_Status);
		}

		public void TestPreProcessCC009C_NonMatchingLRN_MatchingMRN_InvalidBM_SubApplicationCode()
		{
			mockDataProvider.Setup(x => x.LRN).Returns(InvalidLRN);
			movementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();

			processor.PreProcessMessage(incomingMessage);

			AssertEquals(EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		public void TestPreProcessCC009C_NonMatchingLRN_MatchingMRN_ValidBM_SubApplicationCode()
		{
			mockDataProvider.Setup(x => x.LRN).Returns(InvalidLRN);

			processor.PreProcessMessage(incomingMessage);

			AssertEquals(EDIMessageStatusList.Codes.PreProcessedOK, incomingMessage.EM_Status);
		}

		public void TestProcessMessage_BM_CustomsStatusIsREL_DecisionTrue()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionTrue(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);
		}

		public void TestProcessMessage_BM_CustomsStatusIsENQ_DecisionTrue()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionTrue(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry);
		}

		public void TestProcessMessage_BM_CustomsStatusIsMRN_DecisionTrue()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionTrue(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);
		}

		public void TestProcessMessage_BM_CustomsStatusIsACK_DecisionTrue()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionTrue(NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
		}

		void AssertProcessMessage_BM_CustomsStatus_DecisionTrue(string customsStatus)
		{
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(customsStatus, () =>
			{
				AssertEquals(NCTS5DepartureCustomsStatusList.Codes.Cancelled, movementHeader.BM_CustomsStatus);
				var logs = movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && (x.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.Cancelled));
				AssertEquals("Event log is created successfully", 1, logs.Count());
			});
		}

		public void TestProcessMessage_BM_CustomsStatusIsREL_DecisionFalse()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionFalse(NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);
		}

		public void TestProcessMessage_BM_CustomsStatusIsENQ_DecisionFalse()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionFalse(NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry);
		}

		public void TestProcessMessage_BM_CustomsStatusIsMRN_DecisionFalse()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionFalse(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);
		}

		public void TestProcessMessage_BM_CustomsStatusIsACK_DecisionFalse()
		{
			AssertProcessMessage_BM_CustomsStatus_DecisionFalse(NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
		}

		void AssertProcessMessage_BM_CustomsStatus_DecisionFalse(string customsStatus)
		{
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(false);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals(customsStatus, movementHeader.BM_CustomsStatus);
				var logs = movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && (x.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.Cancelled));
				AssertEquals("Event log is created successfully", 0, logs.Count());
			});
		}

		public void TestProcessMessage_BM_CustomsStatusNotRELOrENQOrMRNOrACK()
		{
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Status didn't change", NCTS5DepartureCustomsStatusList.Codes.PreLodged, movementHeader.BM_CustomsStatus);
				var logs = movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && (x.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.Cancelled));
				AssertEquals("No Event log is created", 0, logs.Count());
			});
		}

		public void TestProcessMessage_EM_StatusWhenDecisionIs1AndBM_PhaseIsInvalidationSent()
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
		}

		public void TestProcessMessage_EM_StatusWhenDecisionIsNot1()
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(false);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
		}

		public void TestProcessMessage_EM_StatusWhenBM_PhaseIsNotInvalidationSent()
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
		}

		public void TestProcessMessage_EffectiveMessageStatusWhenDecisionIs1AndBM_PhaseIsInvalidationSent()
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
		}

		public void TestProcessMessage_EffectiveMessageStatusWhenDecisionIsNot1()
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(false);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
		}

		public void TestProcessMessage_EffectiveMessageStatusWhenBM_PhaseIsNotInvalidationSent_NotInitiatedByCustoms() => CombineAssertions(() =>
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);
			mockDataProvider.SetupGet(m => m.InitiatedByCustoms).Returns(false);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("message status", LogicalStatusList.Codes.Sent, nctsHeader.EffectiveMessageStatus);
			AssertEquals("phase", NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals("customs status", string.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
		});

		public void TestProcessMessage_EffectiveMessageStatusWhenBM_PhaseIsNotInvalidationSent_InitiatedByCustoms() => CombineAssertions(() =>
		{
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Amendment;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);
			mockDataProvider.SetupGet(m => m.InitiatedByCustoms).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("message status", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
			AssertEquals("phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals("customs status", NCTS5DepartureCustomsStatusList.Codes.Cancelled, nctsHeader.MovementHeader.BM_CustomsStatus);
		});

		public void TestProcessMessage_BM_PhaseWhenDecisionIs1()
		{
			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
		}

		public void TestProcessMessage_BM_PhaseWhenDecisionIsNot1()
		{
			nctsHeader.MovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(false);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals(NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
		}

		[TestDate(2023, 12, 01, 14, 03, 32)]
		public void TestProcessMessage_LogIsCreated()
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			mockDataProvider.SetupGet(m => m.Decision).Returns(true);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertCollectionContains(movementHeader.Logs.GetAllLogs().Cast<StmALog>(),
				l => l.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && l.SL_Reference == nctsHeader.MovementHeader.BM_CustomsStatus && l.SL_EventTime == ZDateTime.Now);
		}

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC009C;

		protected override Type ExpectedMessageInterpreterType => typeof(CC009CMessageInterpreter);

		protected override CC009CMessageProcessor Processor => processor;

		protected override void SetUp()
		{
			base.SetUp();
			mockDataProvider = new Mock<ICC009CDataProvider>();
			mockDataProvider.CallBase = true;

			var mockProcessor = new Mock<CC009CMessageProcessor>(new BatchProcessor.LoggingInformation());
			mockProcessor.CallBase = true;
			mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockDataProvider.Object);
			processor = mockProcessor.Object;

			incomingMessage = CreateIncomingMessage(Factory);

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = ValidLRN;
			movementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Extensions.CreateMovementReferenceNumber(movementHeader.Header, ValidMRN);
			Factory.Save();

			mockDataProvider.Setup(x => x.LRN).Returns(ValidLRN);
			mockDataProvider.Setup(x => x.MRN).Returns(ValidMRN);
		}

		Mock<ICC009CDataProvider> mockDataProvider;
		EDIMessage incomingMessage;
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
		CC009CMessageProcessor processor;

		const string ValidLRN = "12345";
		const string InvalidLRN = "54321";

		const string ValidMRN = "67890";
		const string InvalidMRN = "09876";
	}
}
