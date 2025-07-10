using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC051CMessageProcessor))]
	sealed class CC051CMessageProcessorTest : NctsMessageProcessorTestCase<CC051CMessageProcessor, ICC051CDataProvider>
	{
		public void TestProcessCC051CMessageDeclaration()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var currentDateTime = DateTime.Now;
			mockProvider.Setup(x => x.PreparationDateAndTime).Returns(currentDateTime);
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Company.GC_RN_NKCountryCode = "BE";
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");

			var guaranteeNum1 = "GUA1";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "BE";
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			guaranteeHeader.CPH_Number = guaranteeNum1;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = 1000m;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_IsAggregated = true;
			transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

			var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionPND.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionPND.CPL_TranValue = -20m;
			transactionPND.CPL_IsAggregated = true;
			transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionPND.CPL_TransactionStatus = "PND";
			guaranteeHeader.CPH_Balance = 980;

			var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionCON.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionCON.CPL_TranValue = -500m;
			transactionCON.CPL_IsAggregated = true;
			transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionCON.CPL_TransactionStatus = "CON";

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_RN_NKCountryOfIssue = "BE";
			guarantee.PW_BondNumber = guaranteeNum1;
			guarantee.PW_BondAmount = 145;

			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE PRS" + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE 'NRL'", StatusCodes.NotReleasedForExport, movementHeader.BM_CustomsStatus);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());

				AssertEquals("Opening balance should never be deleted.", false, transaction.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("New transaction status deleted for Pending transaction.", true, transactionPND.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("New transaction status should never be deleted for Confirmed transaction.", false, transactionCON.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC051CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC051CDataProvider>());
		Mock<ICC051CDataProvider> mockProvider;

		protected override Mock<CC051CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC051CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC051CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC051C;

		protected override string ExpectedMessageFriendlyName => "No Release for Transit";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl };

		protected override Type ExpectedMessageInterpreterType => typeof(CC051CMessageInterpreter);
	}
}
