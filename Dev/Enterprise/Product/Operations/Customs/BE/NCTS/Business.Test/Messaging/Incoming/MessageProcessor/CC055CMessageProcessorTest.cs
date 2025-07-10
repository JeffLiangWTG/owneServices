using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
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
	[TestedType(typeof(CC055CMessageProcessor))]
	sealed class CC055CMessageProcessorTest : NctsMessageProcessorTestCase<CC055CMessageProcessor, ICC055CDataProvider>
	{
		public void TestProcessCC055CMessageDeclaration()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetupAndProcessCC055CMessage(nctsHeader, "G11", "GR11", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);

			CombineAssertions(() =>
			{
				var movementHeader = nctsHeader.MovementHeader;
				AssertEquals("EDIMEssage Status Should be " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should be 'GIV'", NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should be '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should be 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondHeader Should have a CLR event logged", nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsCleared.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
				AssertContains("ST_NoteText", "Guarantee invalid.", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestGuaranteeTransactions_G01()
		{
			AssertGuaranteeTransaction("G01", true);
		}

		public void TestGuaranteeTransactions_G02()
		{
			AssertGuaranteeTransaction("G02", true);
		}

		public void TestGuaranteeTransactions_G03()
		{
			AssertGuaranteeTransaction("G03", false);
		}

		public void TestGuaranteeTransactions_G04()
		{
			AssertGuaranteeTransaction("G04", false);
		}

		public void TestGuaranteeTransactions_G05()
		{
			AssertGuaranteeTransaction("G05", true);
		}

		public void TestGuaranteeTransactions_G06()
		{
			AssertGuaranteeTransaction("G06", false);
		}

		public void TestGuaranteeTransactions_G07()
		{
			AssertGuaranteeTransaction("G07", false);
		}

		public void TestGuaranteeTransactions_G08()
		{
			AssertGuaranteeTransaction("G08", true);
		}

		public void TestGuaranteeTransactions_G09()
		{
			AssertGuaranteeTransaction("G09", true);
		}

		public void TestGuaranteeTransactions_G10()
		{
			AssertGuaranteeTransaction("G10", true);
		}

		public void TestGuaranteeTransactions_G11()
		{
			AssertGuaranteeTransaction("G11", false);
		}

		public void TestGuaranteeTransactions_G12()
		{
			AssertGuaranteeTransaction("G12", true);
		}

		public void TestDiscardedStatusAndNote()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			SetupAndProcessCC055CMessage(nctsHeader, "G11", "GR11", NCTS5DepartureCustomsStatusList.Codes.ReleaseRequestHasBeenRequested);

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMessage status should be {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
				var note = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single();
				AssertContains("Note text", "is discarded because its ‘Status at Customs’ is not ACK, PRE, MRN, AMR or GIV. In Case of GIV, the phase status should be 013 and message status SNT or ACK", note.ST_NoteText);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC055CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC055CDataProvider>());
		Mock<ICC055CDataProvider> mockProvider;

		protected override Mock<CC055CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC055CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC055CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC055C;

		protected override string ExpectedMessageFriendlyName => "CC055C Customs Message";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };

		protected override Type ExpectedMessageInterpreterType => typeof(CC055CMessageInterpreter);

		void SetupAndProcessCC055CMessage(NctsHeader nctsHeader, string errorCode, string grn, string customsStatus)
		{
			var guarenteeReference = GuaranteeReferenceXmlProvider.New(new GuaranteeReferenceType08
			{
				SequenceNumber = "1",
				Grn = grn,
				InvalidGuaranteeReason = new Collection<InvalidGuaranteeReasonType01>
				{
					new InvalidGuaranteeReasonType01
					{
						SequenceNumber = "1",
						Code = errorCode,
						Text = "Text01",
					}
				}
			});
			var guarenteeReferences = new Collection<GuaranteeReferenceXmlProvider>();
			guarenteeReferences.Add(guarenteeReference);
			mockProvider.Setup(x => x.GuaranteeReferences).Returns(guarenteeReferences);
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = customsStatus;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();
		}

		void AssertGuaranteeTransaction(string errorCode, bool shouldBeDeleted)
		{
			var guaranteeNum1 = "GUA1";
			var guaranteeNum2 = "GUA2";

			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeNum1;
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader.CPH_Balance = 1000m;

			var guaranteeHeader2 = Factory.NewWithValidTestData<BE.Business.CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = guaranteeNum2;
			guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader2.CPH_Type = "TRA";
			guaranteeHeader2.CPH_RN_NKCountryCode = "BE";
			guaranteeHeader2.CPH_Balance = 1000m;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";

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

			var transaction2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
			transaction2.CPL_Reference = "OPENING";
			transaction2.CPL_TranValue = 1000m;
			transaction2.CPL_Comment = "OPENING";
			transaction2.CPL_IsAggregated = true;
			transaction2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

			var transactionPND2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
			transactionPND2.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionPND2.CPL_TranValue = -20m;
			transactionPND2.CPL_IsAggregated = true;
			transactionPND2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionPND2.CPL_TransactionStatus = "PND";
			guaranteeHeader2.CPH_Balance = 980;

			var transactionCON2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
			transactionCON2.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
			transactionCON2.CPL_TranValue = -500m;
			transactionCON2.CPL_IsAggregated = true;
			transactionCON2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
			transactionCON2.CPL_TransactionStatus = "CON";

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = guaranteeNum1;
			guarantee.PW_BondAmount = 145;

			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondNumber = guaranteeNum2;
			guarantee.PW_BondAmount = 999;

			Factory.Save();

			SetupAndProcessCC055CMessage(nctsHeader, errorCode, guaranteeNum1, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);

			CombineAssertions(() =>
			{
				AssertEquals("Opening balance should never be deleted. Code =" + errorCode, false, transaction.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("New transaction status deleted for Pending transaction. Code =" + errorCode, shouldBeDeleted, transactionPND.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("New transaction status should never be deleted for Confirmed transaction. Code =" + errorCode, false, transactionCON.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));

				AssertEquals("Opening balance should never be deleted on guarantee that has no error. Code =" + errorCode, false, transaction2.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("Transaction PND should never be deleted on guarantee that has no error. Code =" + errorCode, false, transactionPND2.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
				AssertEquals("Transaction CON should never be deleted on guarantee that has no error. Code =" + errorCode, false, transactionCON2.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
			});
		}
	}
}
