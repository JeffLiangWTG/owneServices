using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using CusGuaranteeHeader = Enterprise.Customs.BE.Business.CusGuaranteeHeader;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC029CMessageProcessor))]
	sealed class CC029CMessageProcessorTest : NctsMessageProcessorTestCase<CC029CMessageProcessor, ICC029CDataProvider>
	{
		public void TestNCTTADCreatedWhenProcessed()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetupAndProcessMessage(new List<CusGuaranteeHeader>(), Factory.NewWithValidTestData<OrgHeader>(), nctsHeader);
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageType, "NCT"));
			query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageSubType, "TAD"));

			AssertNotNull(Factory.LoadTop1<EDIMessage>(query));
		}

		protected override int ExpectedMessagesAfterProcessing => 2;

		public void TestPreProcessCC029C_MatchingLRN_WrongSubApplicationCode()
		{
			const string lrn = "2204528148060XXXXXX";
			MockProvider.Setup(x => x.LRN).Returns(lrn);
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = lrn;
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;
			movementHeader.BM_SubApplicationCode = NctsMoveHeaderType.Codes.Arrival;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);
			AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.Failed}", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		public void TestPreProcessMessage_CheckMessageSequenceIsValid_CustomStatusIsEmpty()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			MockProvider.Setup(x => x.ReleaseDate).Returns(new DateTime(1994, 2, 2));
			MockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 1));
			MockProvider.Setup(x => x.PreparationDateAndTime).Returns(new DateTime(1994, 2, 2, 10, 11, 12));
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_AdditionalDeclarationType = "D";

			movementHeader.BM_CustomsStatus = ZString.Empty;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			incomingMessage.EM_RetryCount = 4;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("CheckMessageSequenceIsValid", false, CheckMessageSequenceIsValid(incomingMessage, Logger));
		}

		public void TestPreProcessMessage_CheckMessageSequenceIsValid_CustomStatusIsACK()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			MockProvider.Setup(x => x.ReleaseDate).Returns(new DateTime(1994, 2, 2));
			MockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 1));
			MockProvider.Setup(x => x.PreparationDateAndTime).Returns(new DateTime(1994, 2, 2, 10, 11, 12));
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_AdditionalDeclarationType = "D";

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			incomingMessage.EM_RetryCount = 2;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("CheckMessageSequenceIsValid", false, CheckMessageSequenceIsValid(incomingMessage, Logger));
		}

		public void TestPreProcessMessage_CheckMessageSequenceIsValid_CustomStatusIsPRE()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			MockProvider.Setup(x => x.ReleaseDate).Returns(new DateTime(1994, 2, 2));
			MockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 1));
			MockProvider.Setup(x => x.PreparationDateAndTime).Returns(new DateTime(1994, 2, 2, 10, 11, 12));
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_AdditionalDeclarationType = "D";

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			incomingMessage.EM_RetryCount = 0;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("CheckMessageSequenceIsValid", false, CheckMessageSequenceIsValid(incomingMessage, Logger));
		}

		public void TestProcessCC029CMessageDeclaration()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetupAndProcessMessage(new List<CusGuaranteeHeader>(), Factory.NewWithValidTestData<OrgHeader>(), nctsHeader);
			Factory.Save();

			var movementHeader = nctsHeader.MovementHeader;
			AssertProcessSuccessfully(nctsHeader);
		}

		public void TestProcessCC029CMessageDeclaration_LRNIsEmpty()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			MockProvider.Setup(x => x.ReleaseDate).Returns(new DateTime(1994, 2, 2));
			MockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 1));
			MockProvider.Setup(x => x.PreparationDateAndTime).Returns(new DateTime(1994, 2, 2, 10, 11, 12));
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_AdditionalDeclarationType = "D";

			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertProcessSuccessfully(nctsHeader);
		}

		void AssertProcessSuccessfully(NctsHeader nctsHeader)
		{
			var movementHeader = nctsHeader.MovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("CusInBondMoveHeader Customs Status", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("EDIMEssage Status " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondHeader MRN", "22BE000000000012J1", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
				AssertEquals("CusInBondHeader Issue Date", new ZDateTime(1994, 2, 2), nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
				AssertEquals("CusInBondMoveHeader Declaration Type", NctsTypeOfAdditionalDeclarationList.Codes.A, movementHeader.BM_AdditionalDeclarationType);
				AssertEquals("CusInBondMoveHeader Entry Date", new ZDateTime(1994, 2, 1), movementHeader.BM_EntryDate);
				AssertContains("ST_NoteText", "New detailed status: Goods Released for Transit at Departure", incomingMessage.EM_MessageInterpretation);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
			});
		}

		public void TestLockEventCreated()
		{
			MockProvider.Setup(x => x.ReleaseDate).Returns(DateTime.Now);
			AssertLockEventCreated(NctsTransitStatusList.Codes.Unknown, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, "", "DEP", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, "The tabs are locked for editing because a Release for Transit was received.");
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => true;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC029CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC029CDataProvider>());
		Mock<ICC029CDataProvider> mockProvider;

		protected override Mock<CC029CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC029CMessageProcessor>(Logger));
		Mock<CC029CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC029C;

		protected override string ExpectedMessageFriendlyName => "Release for Transit";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid };

		protected override ZString[] PreProcessNOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Cancelled, NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };

		protected override Type ExpectedMessageInterpreterType => typeof(CC029CMessageInterpreter);

		protected override ZBool MessageConfirmsGuaranteeTransactions => true;

		protected override NctsHeader SetupAndProcessMessage(List<CusGuaranteeHeader> guaranteeHeaderList, OrgHeader org, NctsHeader nctsHeader)
		{
			var preparationDateTime = new DateTime(1994, 2, 2, 10, 11, 12);
			var releaseDate = new DateTime(1994, 2, 2);
			var declarationAcceptanceDate = new DateTime(1994, 2, 1);

			MockProvider.Setup(x => x.ReleaseDate).Returns(releaseDate);
			MockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(declarationAcceptanceDate);
			MockProvider.Setup(x => x.PreparationDateAndTime).Returns(preparationDateTime);

			return base.SetupAndProcessMessage(guaranteeHeaderList, org, nctsHeader);
		}

		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;
	}
}
