using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
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
	[TestedType(typeof(CC045CMessageProcessor))]
	sealed class CC045CMessageProcessorTest : NctsMessageProcessorTestCase<CC045CMessageProcessor, ICC045CDataProvider>
	{
		public void TestProcessCC045CMessageDeclaration()
		{
			var nctsHeaderArrival = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			Extensions.CreateMovementReferenceNumber(nctsHeaderArrival, mrn);
			Factory.Save();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetupAndProcessMessage(nctsHeader);

			CombineAssertions(() =>
			{
				var movementHeader = nctsHeader.MovementHeader;
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE 'WRO'", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondHeader Should have a CLR event logged", nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsCleared.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
				AssertContains("ST_NoteText", "Write-Off notification for NCTS departure received at", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestLockEventCreated()
		{
			AssertLockEventCreated(NctsTransitStatusList.Codes.Unknown, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, "", "DEP", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, "The tabs are locked for editing because an Write-Off Notification was received.");
		}

		public void TestConfirmedTransactionAddedForAllExistingTransactions()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";

			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "OPENING";
			transaction.CPL_TranValue = 1000m;
			transaction.CPL_Comment = "OPENING";
			transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

			var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionPND.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionPND.CPL_TranValue = -20m;
			transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionPND.CPL_TransactionStatus = "PND";
			guaranteeHeader.CPH_Balance = 980;

			var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transactionCON.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionCON.CPL_TranValue = -500m;
			transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionCON.CPL_TransactionStatus = "CON";

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial situation: 3 transaction on the guarantee header", 3, guaranteeHeader.GetTransactions().Count());
				SetupAndProcessMessage(nctsHeader);
				AssertEquals("There should be 4 transactions on the guarantee header", 4, guaranteeHeader.GetTransactions().Count());
				var newTransaction = guaranteeHeader.GetTransactions().ToArray()[3];
				AssertEquals("New transaction should be referenced to the LRN of the NCTS Header", "LRN1234567", newTransaction.CPL_Reference);
				AssertEquals("New transaction should be confirmed", "CON", newTransaction.CPL_TransactionStatus);
				AssertEquals("New transaction should have the opposite amount of the previous confirmed transaction", 500m, newTransaction.CPL_TranValue);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC045CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC045CDataProvider>());
		Mock<ICC045CDataProvider> mockProvider;

		protected override Mock<CC045CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC045CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC045CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC045C;

		protected override string ExpectedMessageFriendlyName => "Write-Off Notification";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged };

		protected override Type ExpectedMessageInterpreterType => typeof(CC045CMessageInterpreter);

		void SetupAndProcessMessage(NctsHeader nctsHeader)
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.WriteOffDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns(mrn);
			var movementHeader = nctsHeader.MovementHeader;
			Extensions.CreateMovementReferenceNumber(nctsHeader, mrn);
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();
		}
		readonly string mrn = "22BE000000000012J1";
	}
}
