using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.IO;
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
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDEPRELMessageProcessor))]
	sealed class NctsDEPRELMessageProcessorTest : MessageProcessorAbstractTest<NctsDEPRELMessageProcessor, AtlasInboundEDIMessage<IDEPREL>>
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
			messageMock.Setup(m => m.DataProvider).Returns((IDEPREL)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
			messageMock.Verify(m => m.DataProvider);
		}

		public void TestReferenceNumberAndStatus()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("entry.MovementReferenceNumber", MovementReferenceNumber, nctsHeader.MovementReferenceNumber);

				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("BM_Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
				AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, nctsHeader.MovementHeader.BM_MessageStatus);
				AssertEquals("ValuationDate", nctsHeader.MovementReferenceEntryNumber.CE_IssueDate, nctsHeader.MovementHeader.BM_ValuationDate);
			});
		}

		public void TestEventsCreated()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGenerateEmail()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier, "test@mail.com");

			ProcessMessage(message);

			var reference = nctsHeader.BH_JobReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var title = $"NCTS Departure Release Message Response for {reference}";
			var subject = $"{title} LRN: {LocalReferenceNumber}";
			var bodyMessageTitle = $"<title>{title}</title>";

			var bodyMessageHeader = $@"<strong>NCTS Departure Release Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=NctsMovementController&BusinessEntityPK={nctsHeader.PK}";
			var bodyMessageSummary = $@"Your NCTS Declaration Message for Job {reference} have a Release Message. For details please follow the Link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ $"<tr><td>MRN:</td><td>{MovementReferenceNumber}</td></tr>"
									+ "</table>";

			AssertEmailForSingleRecipientWithTable("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestEmailAttachments()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier, "test@mail.com");

			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new ()
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX=")),
				},
				new ()
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY=")),
				},
			});

			ProcessMessage(message);

			var reference = nctsHeader.BH_JobReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject.Contains(reference));
			var emailAttachments = email.Attachments.Cast<AttachmentDef>();
			var file1 = emailAttachments.Single(x => x.DisplayName == "file1.pdf");
			var file2 = emailAttachments.Single(x => x.DisplayName == "file2.pdf");
			CombineAssertions(() =>
			{
				AssertEquals("File1 content", Convert.FromBase64String("XXX="), file1.Data);
				AssertEquals("File2 content", Convert.FromBase64String("YYY="), file2.Data);
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

		public void TestUpdateExportDate()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			dataProviderMock.Setup(e => e.LimitDate).Returns(new DateTime(2023, 12, 25));
			ProcessMessage(message);

			AssertEquals(new ZDateTime(2023, 12, 25), nctsHeader.MovementHeader.BM_ExportDate);
		}

		public void Test_MovementReferenceEntryNumber_CE_ExpiryDate_ShouldBeUpdated()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			dataProviderMock.Setup(x => x.LimitDate).Returns(new DateTime(2024, 2, 14));
			ProcessMessage(message);

			AssertEquals("ExpiryDate", new ZDateTime(2024, 2, 14), nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
		}

		public void TestUpdateRegistrationNumberBWH()
		{
			var testHelper = new WhsDataTestHelper(Factory);
			var order = testHelper.WhsHelper.CreateWhsOrder(testHelper.Importer.PK, testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "OrderRef123") as IWhsOrder;
			Factory.Save();
			var orderLine1 = testHelper.WhsHelper.CreateWhsOrderLine(order.PK, testHelper.Part.PK, 1, "1234", "old value 1", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as IWhsDocketLine;
			Factory.Save();

			var pivot = Factory.New<IWhsDocketJobPivot>();
			pivot.WV_ParentId = nctsHeader.PK;
			pivot.WV_ParentTableCode = nctsHeader.TablePrefix;
			pivot.WV_WD_Docket = order.PK;
			pivot.WV_DocketType = order.WD_DocketType;

			Factory.Save();

			var bill = nctsHeader.Bills.AddNew();
			bill.IsOutwardOrderImported = true;
			var bill1 = bill.GoodsItems.AddNew();
			bill1.BY_BondedWHSOrderNumber = order.WD_DocketID;
			bill1.BY_BondedWHSOrderLineNumber = 1;
			bill1.BY_DeclarationGoodsItemNumber = 7;

			Factory.Save();
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);

			var attribute1 = DE.Business.BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(orderLine1, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(MovementReferenceNumber, attribute1.WB_EntryKey);
				AssertEquals((ZShort)7, attribute1.WB_EntryLineNo);
			});
		}

		public void TestUpdateRegistrationNumberBWH_MultipleDockets()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			var order1 = testHelper.WhsHelper.CreateWhsOrder(testHelper.Importer.PK, testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "OrderRef123") as IWhsOrder;
			var order2 = testHelper.WhsHelper.CreateWhsOrder(testHelper.Importer.PK, testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "OrderRef456") as IWhsOrder;
			Factory.Save();
			var orderLine1 = testHelper.WhsHelper.CreateWhsOrderLine(order1.PK, testHelper.Part.PK, 1, "1234", "old value 1", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as IWhsDocketLine;
			var orderLine2 = testHelper.WhsHelper.CreateWhsOrderLine(order2.PK, testHelper.Part.PK, 3, "4567", "old value 2", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 3, ZGuid.Empty, ZString.Empty) as IWhsDocketLine;
			var orderLine3 = testHelper.WhsHelper.CreateWhsOrderLine(order2.PK, testHelper.Part.PK, 3, "4567", "old value 2", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as IWhsDocketLine;
			Factory.Save();

			var pivot1 = Factory.New<IWhsDocketJobPivot>();
			pivot1.WV_ParentId = nctsHeader.PK;
			pivot1.WV_ParentTableCode = nctsHeader.TablePrefix;
			pivot1.WV_WD_Docket = order1.PK;
			pivot1.WV_DocketType = order1.WD_DocketType;

			var pivot2 = Factory.New<IWhsDocketJobPivot>();
			pivot2.WV_ParentId = nctsHeader.PK;
			pivot2.WV_ParentTableCode = nctsHeader.TablePrefix;
			pivot2.WV_WD_Docket = order2.PK;
			pivot2.WV_DocketType = order2.WD_DocketType;

			Factory.Save();

			var bill = nctsHeader.Bills.AddNew();
			bill.IsOutwardOrderImported = true;

			var bill1 = bill.GoodsItems.AddNew();
			bill1.BY_BondedWHSOrderNumber = order1.WD_DocketID;
			bill1.BY_BondedWHSOrderLineNumber = 1;
			bill1.BY_DeclarationGoodsItemNumber = 7;

			var bill2 = bill.GoodsItems.AddNew();
			bill2.BY_BondedWHSOrderNumber = order2.WD_DocketID;
			bill2.BY_BondedWHSOrderLineNumber = 1;
			bill2.BY_DeclarationGoodsItemNumber = 3;

			var bill3 = bill.GoodsItems.AddNew();
			bill3.BY_BondedWHSOrderNumber = order2.WD_DocketID;
			bill3.BY_BondedWHSOrderLineNumber = 2;
			bill3.BY_DeclarationGoodsItemNumber = 5;

			Factory.Save();

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);

			var attribute1 = DE.Business.BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(orderLine1, Factory);
			var attribute2 = DE.Business.BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(orderLine2, Factory);
			var attribute3 = DE.Business.BondedWarehousingHelper.GetBondedWarehouseAttributeFromWhsDocketLine(orderLine3, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(MovementReferenceNumber, attribute1.WB_EntryKey);
				AssertEquals((ZShort)7, attribute1.WB_EntryLineNo);
				AssertEquals(MovementReferenceNumber, attribute2.WB_EntryKey);
				AssertEquals((ZShort)3, attribute2.WB_EntryLineNo);
				AssertEquals(MovementReferenceNumber, attribute3.WB_EntryKey);
				AssertEquals((ZShort)5, attribute3.WB_EntryLineNo);
			});
		}

		public void TestNotUpdateWarehouse()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			nctsHeader.Bills.AddNew().IsOutwardOrderImported = true;
			var supporter = (IWarehouseIntegrationSupporter)nctsHeader;
			supporter.WarehouseTransactionStatus = "OCP";
			nctsHeader.MovementHeader.BM_OA_WarehouseAddress = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", nctsHeader.Consignee, "3");
			ProcessMessage(message);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = nctsHeader.Logs.Find(query);

			AssertEquals("IsOutwardOrderImported is true and BM_OA_WarehouseAddress filled", false, dataTransferEvents.Any(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
		}

		public void TestUpdateWarehouse()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);
			nctsHeader.Bills.AddNew().IsOutwardOrderImported = false;
			var supporter = nctsHeader as IWarehouseIntegrationSupporter;
			supporter.WarehouseTransactionStatus = "OCP";
			nctsHeader.MovementHeader.BM_OA_WarehouseAddress = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", nctsHeader.Consignee, "3");
			ProcessMessage(message);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = nctsHeader.Logs.Find(query);

			var universalShipmentMessage = dataTransferEvents.Single(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML).RelatedEDIMessage.Message;
			var parsedShipment = universalShipmentMessage.GetEM_MessageTextReader().Parse<Shipment>();

			CombineAssertions(() =>
			{
				AssertNotNull("shipment has been exported", universalShipmentMessage);
				AssertEquals("IsOutwardOrderImported is false and BM_OA_WarehouseAddress filled", MovementReferenceNumber, parsedShipment.EntryNumberCollection?.FirstOrDefault()?.Number);
			});
		}

		public void TestGuaranteeTransactionsSetToConfirmed()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

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
					status: PermitTransactionStatusList.Codes.Pending);

			CombineAssertions(() => {
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
				ProcessMessage(message);
				AssertEquals("After processing GUA1", 2, guaranteeHeader.GetTransactions().Count());
				AssertEquals("After processing GUA1", PermitTransactionStatusList.Codes.Confirmed, guaranteeHeader.GetTransactions().Last().CPL_TransactionStatus);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS DEPREL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDEPREL>> Processor => new NctsDEPRELMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_JobReference = "ATB150000620520195875";
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = LocalReferenceNumber;

			dataProviderMock = new Mock<IDEPREL>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDEPREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<IDEPREL> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDEPREL>> messageMock;
		AtlasInboundEDIMessage<IDEPREL> message;
		NctsHeader nctsHeader;

		const string ReferencedMessageIdentifier = "DE302989100000000000000000000487287";
		const string LocalReferenceNumber = "19DE485154386041M4";
		const string MovementReferenceNumber = "22DE000000001234E0";
	}
}
