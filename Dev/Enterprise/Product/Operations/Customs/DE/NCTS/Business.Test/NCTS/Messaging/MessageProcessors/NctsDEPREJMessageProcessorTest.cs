using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business;
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
	[TestedType(typeof(NctsDEPREJMessageProcessor))]
	sealed class NctsDEPREJMessageProcessorTest : MessageProcessorAbstractTest<NctsDEPREJMessageProcessor, AtlasInboundEDIMessage<IDEPREJ>>
	{
		public void TestDocumentsAttachedAsEDocs()
		{
			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestEventsCreated()
		{
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IDEPREJ)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(nctsHeader.MovementHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "YSN";
			staff.GS_FullName = "Joseph";
			staff.GS_EmailAddress = "YSN@where.com";

			Factory.Save();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("BM_Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
				AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Error, nctsHeader.MovementHeader.BM_MessageStatus);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var reference = nctsHeader.BH_JobReference;
				var title = $"NCTS Departure Rejection Notification Response for {reference}";
				var subject = $"{title} LRN: {LocalReferenceNumber}";
				var bodyMessageTitle = $"<title>{title}</title>";

				var bodyMessageHeader = ZString.Empty;
				var bodyMessageSummary = "Your NCTS Departure Declaration for Job REFERENCE1 has received a Rejection Notification. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								+ "<tr><td>LRN</td><td>19DE485154386041M4</td></tr>"
								+ "<tr><td>Rejection Type</td><td>015 - Anmeldung nicht angenommen</td></tr>"
								+ "</table>";
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "YSN@where.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals(LocalReferenceNumber, message.GetLogbookLocalReferenceNumber());
		}

		public void TestCancelWarehouse()
		{
			((IWarehouseIntegrationSupporter)nctsHeader).WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = nctsHeader.Logs.Find(query);
			AssertEquals(1, dataTransferEvents.Length);
		}

		public void TestGuaranteeTransactionsSetToDEL()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
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
					LocalReferenceNumber,
					ZString.Empty,
					guarantee.PW_BondAmount * -1,
					0,
					status: PermitTransactionStatusList.Codes.Pending);

			CombineAssertions(() => {
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
				ProcessMessage(message);
				AssertEquals("After processing GUA1", 2, guaranteeHeader.GetTransactions().Count());
				AssertEquals("After processing GUA1", PermitTransactionStatusList.Codes.Deleted, guaranteeHeader.GetTransactions().Last().CPL_TransactionStatus);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS DEPREJ Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDEPREJ>> Processor => new NctsDEPREJMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "REFERENCE1";
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = LocalReferenceNumber;
			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			dataProviderMock = new Mock<IDEPREJ>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.RejectionType).Returns("015");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDEPREJ>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		Mock<IDEPREJ> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDEPREJ>> messageMock;
		AtlasInboundEDIMessage<IDEPREJ> message;
		NctsHeader nctsHeader;
		EDIMessage outgoingMessage;

		const string ReferencedMessageIdentifier = "DE302989100000000000000000000487287";
		const string LocalReferenceNumber = "19DE485154386041M4";
		const string MovementReferenceNumber = "22DE000000001234E0";
	}
}
