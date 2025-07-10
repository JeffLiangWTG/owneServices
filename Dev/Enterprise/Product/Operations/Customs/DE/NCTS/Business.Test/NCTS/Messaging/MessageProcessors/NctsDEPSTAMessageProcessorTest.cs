using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDEPSTAMessageProcessor))]
	sealed class NctsDEPSTAMessageProcessorTest : MessageProcessorAbstractTest<NctsDEPSTAMessageProcessor, AtlasInboundEDIMessage<IDEPSTA>>
	{
		public void TestDocumentsAttachedAsEDocs()
		{
			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestLinkedObjectNotFound()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, "NOTORIGINALMSG");

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals(nctsHeader.MovementHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Reset();
			messageMock.Setup(m => m.DataProvider).Returns((IDEPSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
			messageMock.Verify(m => m.DataProvider);
		}

		public void TestProcessMessage()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Customs Status", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
				AssertEquals("entry.MovementReferenceNumber", MovementReferenceNumber, nctsHeader.MovementReferenceNumber);
				AssertEquals("Message Status", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
			});
		}

		public void TestEventsCreated()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestEventReference_DepartureStatus510()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);

			dataProviderMock.Setup(m => m.DepartureStatus).Returns(A0115DepartureStatusCodeList.Codes._510);
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", DeNctsConstants.DepartureCustomsStatus.GuaranteeWrittenOff, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);

			var reference = nctsHeader.BH_JobReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var title = $"NCTS Departure Status Update Response for {reference}";
			var subject = $"{title} LRN: {LocalReferenceNumber}";
			var bodyMessageTitle = $"<title>{title}</title>";

			var bodyMessageHeader = $@"<strong>NCTS Departure Status Update Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=NctsMovementController&BusinessEntityPK={nctsHeader.PK}";
			var bodyMessageSummary = $@"Your NCTS Departure Declaration for Job {reference} has received a Status Update. For details please follow the Link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>LRN:</td><td>{LocalReferenceNumber}</td></tr>"
								   + $"<tr><td>MRN:</td><td>{MovementReferenceNumber}</td></tr>"
								   + "<tr><td>Status Update:</td><td>570 - Declaration completed</td></tr>"
								   + "</table>";

			AssertEmailForSingleRecipientWithTable("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestCustomsStatusMapper()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ACK", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("110"));
				AssertEquals("MRN", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("130"));
				AssertEquals("CAN", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("191"));
				AssertEquals(ZString.Empty, NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("510"));
				AssertEquals("CAN", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("520"));
				AssertEquals("WRO", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("570"));
				AssertEquals("WRO", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("571"));
				AssertEquals("WRO", NctsDEPSTAMessageProcessor.MapDepartureStatusToCustomsStatus("590"));
			});
		}

		public void TestPhaseStatusMapper()
		{
			CombineAssertions(() =>
			{
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("110"));
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("130"));
				AssertEquals("014", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("191"));
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("510"));
				AssertEquals("014", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("520"));
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("570"));
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("571"));
				AssertEquals("015", NctsDEPSTAMessageProcessor.MapDepartureStatusToPhaseStatus("590"));
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals(LocalReferenceNumber, message.GetLogbookLocalReferenceNumber());
		}

		public void TestCancelWarehouse_ShouldSendCancelEvent_WhenDepartureStatus191() =>
			TestCancelWarehouse(A0115DepartureStatusCodeList.Codes._191, true);
		public void TestCancelWarehouse_ShouldSendCancelEvent_WhenDepartureStatus520() =>
			TestCancelWarehouse(A0115DepartureStatusCodeList.Codes._520, true);
		public void TestCancelWarehouse_ShouldNotSendCancelEvent_WhenDepartureStatusNotCancelled() =>
			TestCancelWarehouse(A0115DepartureStatusCodeList.Codes._110, false);
		void TestCancelWarehouse(ZString departureStatus, bool expectEventExists)
		{
			dataProviderMock.Setup(m => m.DepartureStatus).Returns(departureStatus);
			((IWarehouseIntegrationSupporter)nctsHeader).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = nctsHeader.Logs.Find(query);
			var expectedLogNumber = expectEventExists ? 1 : 0;
			AssertEquals(expectedLogNumber, dataTransferEvents.Length);
		}

		public void TestProcessMessage_CustomStatusUnchanged_WhenDepartureStatus510()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			nctsHeader.MovementHeader.BM_CustomsStatus = string.Empty;

			dataProviderMock.Setup(m => m.DepartureStatus).Returns(A0115DepartureStatusCodeList.Codes._510);
			ProcessMessage(message);
			AssertEquals("Customs Status NOT changed when departure status 510", string.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);

			dataProviderMock.Setup(m => m.DepartureStatus).Returns(A0115DepartureStatusCodeList.Codes._110);
			ProcessMessage(message);
			AssertEquals("Customs Status changed when departure status NOT 510", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, nctsHeader.MovementHeader.BM_CustomsStatus);
		}

		public void TestGuaranteeTransactionsAddedCounterBalance510() =>
			AssertGuaranteeTransactionsAddedCounterBalance(A0115DepartureStatusCodeList.Codes._510, expectGuaranteeTransactionAdded: true);

		public void TestGuaranteeTransactionsAddedCounterBalance520() =>
			AssertGuaranteeTransactionsAddedCounterBalance(A0115DepartureStatusCodeList.Codes._520, expectGuaranteeTransactionAdded: false);

		protected override ZString MessageFriendlyName => "NCTS DEPSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDEPSTA>> Processor => new NctsDEPSTAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_JobReference = "ATB150000620520195875";
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = LocalReferenceNumber;

			dataProviderMock = new Mock<IDEPSTA>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.DepartureStatus).Returns(A0115DepartureStatusCodeList.Codes._570);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDEPSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		void AssertGuaranteeTransactionsAddedCounterBalance(string statusCode, bool expectGuaranteeTransactionAdded)
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.DepartureStatus).Returns(statusCode);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

			guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
					"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
					"001",
					ZString.Empty,
					guarantee.PW_BondAmount * -1,
					0,
					status: PermitTransactionStatusList.Codes.Confirmed);

			CombineAssertions(() =>
			{
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
				ProcessMessage(message);
				if (expectGuaranteeTransactionAdded)
				{
					AssertEquals("After processing GUA1", 3, guaranteeHeader.GetTransactions().Count());
					AssertEquals("After processing GUA1", PermitTransactionStatusList.Codes.Confirmed, guaranteeHeader.GetTransactions().Last().CPL_TransactionStatus);
				}
				else
				{
					AssertEquals("After processing GUA1", 2, guaranteeHeader.GetTransactions().Count());
				}
			});
		}

		Mock<IDEPSTA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDEPSTA>> messageMock;
		AtlasInboundEDIMessage<IDEPSTA> message;
		NctsHeader nctsHeader;

		const string ReferencedMessageIdentifier = "DE302989100000000000000000000487287";
		const string MovementReferenceNumber = "22DE000000001234E0";
		const string LocalReferenceNumber = "19DE485154386041M4";
	}
}
