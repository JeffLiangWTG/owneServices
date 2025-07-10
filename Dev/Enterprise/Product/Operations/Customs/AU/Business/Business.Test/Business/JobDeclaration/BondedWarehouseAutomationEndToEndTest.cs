using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BondedWarehouseAutomationEndToEndTest : TestCaseWithFactory
	{
		public void TestBondedWarehouseRequiredFieldsMessageError()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse1.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = warehouse2.PK;
			var errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing, errorMessage);
			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = warehouse1.PK;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains(JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing, errorMessage);
		}

		public void TestBondedWarehouseLicenceIsLogged()
		{
			var factory = new BusinessObjectFactory();
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "IM3@42";
			importer.OH_FullName = "BOB";
			importer.MainAddress.OA_Address1 = "BOB'S ADDRESS";
			importer.OH_IsConsignee = true;
			importer.CompanyData.OB_IMUsedBondedWhs = ZBool.True;

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			var declaration = factory.New<JobDeclaration>();
			var count = 0;
			declaration.BondedWarehouseLicenceLogin += new LicenceLoginEventHandler((object sender, LicenceLoginEventArgs e) =>
			{
				count++;
				e.LoginHasBeenAttempted = true;
				e.LicenceCheckPoint.Login(new Customs.Business.Testing.TestLicensedComponent());
			});
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("licence login count", 0, count);
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_IsPackToBondForLine = true;
			declaration.RunPreSaveValidation();
			AssertEquals("licence login count", 1, count);

			line.JI_IsPackToBondForLine = false;
			declaration.RunPreSaveValidation();
			AssertEquals("licence login count", 1, count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("licence login count", 2, count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("licence login count", 2, count);

			line.JI_IsPackToBondForLine = true;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("licence login count", 2, count);

			var importer2 = factory.New<OrgHeader>();
			importer2.OH_Code = "WEM3@42";
			importer2.OH_FullName = "WENDY";
			importer2.MainAddress.OA_Address1 = "WENDY'S ADDRESS";
			importer2.OH_IsConsignee = true;
			importer2.CompanyData.OB_IMUsedBondedWhs = ZBool.False;
			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals("licence login count", 2, count);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("licence login count", 3, count);
		}

		MergedDeclarationCreator2Line warehousing1;
		MergedDeclarationCreator2Line warehousing2;
		AUOrgSupplierPart part1;
		AUOrgSupplierPart part2;
		OrgAddress warehouse1;
		OrgAddress warehouse2;

		AUOrgSupplierPart SetupPart(ZGuid importerPK, ZString partNumber, bool includeSupplierRelationship)
		{
			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			part.OP_PartNum = partNumber;
			part.RelatedOrganisations.AddOrganisationIfNotExist(importerPK, OrgPartRelation.RelationshipTypes.Owner);
			if (includeSupplierRelationship)
			{
				part.RelatedOrganisations.AddOrganisationIfNotExist(Factory.NewWithValidTestData<OrgHeader>().PK, OrgPartRelation.RelationshipTypes.Supplier);
			}
			Classification @class = Factory.New<Classification>();
			@class.CC_ClassificationType = Classification.ClassificationType.IMP;
			@class.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			@class.CC_LookupCode = partNumber + "L";
			@class.CC_TariffNum = "0000000000";
			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_CC = @class.PK;
			pivot.CI_OP = part.PK;

			return part;
		}

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			Factory.Save();

			Factory.RefreshEnabled = false;
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			warehouse1 = OrgHeader.New(Factory).MainAddress;
			warehouse1.Header.FillWithValidTestData();
			warehouse1.Header.OH_Code = "W1";
			warehouse2 = OrgHeader.New(Factory).MainAddress;
			warehouse2.Header.FillWithValidTestData();
			warehouse2.Header.OH_Code = "W2";

			warehousing1 = GetNewWarehousingCreator();
			warehousing1.Declaration.WarehouseDocAddress.E2_OA_Address = warehouse1.PK;
			warehousing2 = GetNewWarehousingCreator();
			warehousing2.Declaration.JE_OH_Importer = warehousing1.Declaration.JE_OH_Importer;
			warehousing2.Declaration.WarehouseDocAddress.E2_OA_Address = warehouse2.PK;
			warehousing2.InvoiceLine1.AddInfo.ZA_ORG = Core.Constants.CountryCodes.Haiti;
			warehousing2.Buyer.OH_Code = "o2.1";
			var whs1 = Factory.New<IWhsWarehouse>();
			whs1.WW_WarehouseCode = "1";
			SetupVirtualWarehouse(whs1, warehouse1);
			var whs2 = Factory.New<IWhsWarehouse>();
			whs2.WW_WarehouseCode = "2";
			SetupVirtualWarehouse(whs2, warehouse2);

			CMRStatisticalClassificationPeriodSnapshot statTariff = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statTariff.SC_TariffClassificationNumber = "00000000";
			statTariff.SC_StatisticalClassificationCode = "00";
			statTariff.SC_QuantityUnit = "KG";
			statTariff.SC_StartDate = new ZDateTime(2005, 1, 1);

			part1 = SetupPart(warehousing1.Declaration.Importer.PK, "~~1", true);
			part2 = SetupPart(warehousing2.Declaration.Importer.PK, "~~2", false);
			part1.OP_StockKeepingUnit = "NO";
			part2.OP_StockKeepingUnit = "KG";

			SetupWarehousingCreator(warehousing1, 1);
			SetupWarehousingCreator(warehousing2, 2);

			AssertNotNull("part", warehousing1.InvoiceLine1.Part);
			AssertNotNull("part", warehousing1.InvoiceLine2.Part);
			AssertNotNull("part", warehousing2.InvoiceLine1.Part);
			AssertNotNull("part", warehousing2.InvoiceLine2.Part);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;

		#region VirtualWarehouse

		void SetupVirtualWarehouse(IWhsWarehouse warehouse, OrgAddress address)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var locationType = Factory.LoadTop1<IWhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			helper.SetUpBondedWarehouse(warehouse.PK, address.PK);
		}

		#endregion

		void BatchProcessorGetsPayMessage(MergedDeclarationCreator2Line warehousingEntry, string entryNumber, ZDateTime date)
		{
			BusinessObjectFactory batchProcessorFactory = new BusinessObjectFactory();
			batchProcessorFactory.RefreshEnabled = false;
			CusEntryHeader entryOnBatchProcessor = batchProcessorFactory.Load<CusEntryHeader>(warehousingEntry.Entry1.PK);
			entryOnBatchProcessor.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entryOnBatchProcessor.EntryNumber = entryNumber;

			var mockMessage = batchProcessorFactory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			mockMessage.Setup(m => m.EM_DateTimeInterchangeSent).Returns(date);
			message.EM_ReceiveTransmit = "RCV";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			entryOnBatchProcessor.Messages.Add(message);
			AssertEquals(date, entryOnBatchProcessor.Messages.LastIncomingMessage.EM_DateTimeInterchangeSent);
			message.EM_MessageText = TestMessages.IMDWithPaymentText;
			entryOnBatchProcessor.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			entryOnBatchProcessor.CH_Status = CustomsEntryStatus.ClearPayment.Code;
			batchProcessorFactory.Save();

			ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, entryNumber + "-INW W0");
			filter.AddToFilter(WhsDocketSchema.WD_OH_Client, warehousingEntry.Declaration.Importer.PK);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
			BusinessObject whsReceive = (BusinessObject)new BusinessObjectFactory().LoadTop1<IWhsReceive>(filter);
			AssertNotNull("Receive docket should be created for entry number: " + entryNumber, whsReceive);
			AssertEquals("Should be finalised", false, ((ZDateTimeOffset)whsReceive[WhsDocketSchema.WD_FinalisedDate.Name]).IsEmpty);
		}

		MergedDeclarationCreator2Line GetNewWarehousingCreator()
		{
			MergedDeclarationCreator2Line creator = new MergedDeclarationCreator2Line(Factory, ZDateTime.Now);
			creator.Declaration.JE_OH_Supplier = ZGuid.Empty;
			creator.InvoiceLine1.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			creator.InvoiceLine2.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			creator.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			creator.InvoiceLine1.JI_IsPackToBondForLine = true;
			creator.InvoiceLine2.JI_IsPackToBondForLine = true;
			return creator;
		}

		void SetupWarehousingCreator(MergedDeclarationCreator2Line creator, int no)
		{
			creator.Declaration.Importer.MiscServ.OM_IMPartAttrib1Name = "VIN" + no;
			creator.Declaration.Importer.MiscServ.OM_IMPartAttrib1Type = "NON";

			creator.Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 100m + 1000 * no, "AUD");

			creator.InvoiceLine1.JI_PartNo = "~~1";
			creator.InvoiceLine2.JI_PartNo = "~~2";

			creator.InvoiceLine1.JI_InvoiceQuantity = 10 + 100 * no;
			creator.InvoiceLine1.JI_InvoiceUQ = "NO";
			creator.InvoiceLine1.JI_CustomsUnitQty = "KG";
			creator.InvoiceLine1.JI_CustomsQuantity = 100 + 1000 * no;
			creator.InvoiceLine1.JI_LinePrice = 1000 + 10000 * no;
			creator.InvoiceLine1.InvoiceHeader.AddInfo.ZA_ORG = Core.Constants.CountryCodes.France;

			creator.InvoiceLine2.AddInfo.ZA_ORG = Core.Constants.CountryCodes.Jamaica;
			creator.InvoiceLine2.JI_InvoiceQuantity = 20 + 100 * no;
			creator.InvoiceLine2.JI_InvoiceUQ = "KG";
			creator.InvoiceLine2.JI_LinePrice = 2000 + 10000 * no;

			creator.InvoiceLine1.InvoiceHeader.JZ_InvoiceAmount = creator.InvoiceLine1.JI_LinePrice + creator.InvoiceLine2.JI_LinePrice;
			creator.InvoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			creator.Declaration.ResumeApportionment();
			AssertEquals("PreCondition: Invoice Line1 should have OFT/ONS apportioned", 1, creator.InvoiceLine1.ApportionedCharges.Count);

			AssertEquals("PreCondition: Invoice Line2 should have OFT/ONS apportioned", 1, creator.InvoiceLine2.ApportionedCharges.Count);

			AssertEquals((100m + 1000 * no) * (creator.InvoiceLine1.JI_LinePrice / (creator.InvoiceLine1.JI_LinePrice + creator.InvoiceLine2.JI_LinePrice)), creator.InvoiceLine1.TransportAndInsurance.Amount, 0.01m);
			AssertEquals((100m + 1000 * no) * (creator.InvoiceLine2.JI_LinePrice / (creator.InvoiceLine1.JI_LinePrice + creator.InvoiceLine2.JI_LinePrice)), creator.InvoiceLine2.TransportAndInsurance.Amount, 0.01m);
		}

		public void TestByProduct()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();
				AssertEquals("Warehousing1.InvoiceLine1.JI_CustomsQuantity", 1100m, warehousing1.InvoiceLine1.JI_CustomsQuantity);
				AssertEquals("Warehousing1.InvoiceLine2.JI_CustomsQuantity", 120m, warehousing1.InvoiceLine2.JI_CustomsQuantity);
				AssertEquals("Warehousing2.InvoiceLine1.JI_CustomsQuantity", 2100m, warehousing2.InvoiceLine1.JI_CustomsQuantity);
				AssertEquals("Warehousing2.InvoiceLine2.JI_CustomsQuantity", 220m, warehousing2.InvoiceLine2.JI_CustomsQuantity);

				AssertEquals("Warehousing1.InvoiceLine1.JI_LinePrice", 11000m, warehousing1.InvoiceLine1.JI_LinePrice);
				AssertEquals("Warehousing1.InvoiceLine2.JI_LinePrice", 12000m, warehousing1.InvoiceLine2.JI_LinePrice);
				AssertEquals("Warehousing2.InvoiceLine1.JI_LinePrice", 21000m, warehousing2.InvoiceLine1.JI_LinePrice);
				AssertEquals("Warehousing2.InvoiceLine2.JI_LinePrice", 22000m, warehousing2.InvoiceLine2.JI_LinePrice);

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartNo = "~~1";
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.JI_InvoiceUQ = "NO";
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 55m, n30Line1.JI_InvoiceQuantity);
				CheckInvoiceLine1Basic(n30Line1);
				CheckInvoiceLine1Calc(55, n30Line1, warehousing1.InvoiceLine1);

				n30Line1.JI_InvoiceQuantity = 110 + 1;
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("should get error", "Cannot release stock from the bonded warehouse. Check the errors on lines for details.", ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);
				CheckInvoiceLine1Basic(n30Line1);
				AssertEquals("InvoiceQty", 111m, n30Line1.JI_InvoiceQuantity);
				Assert("Has error on qty", n30Line1.JI_InvoiceQuantityInfo.HasErrors());

				n30Line1.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartNo = "~~1";
				n30Line1.JI_InvoiceUQ = "NO";
				n30Line1.JI_InvoiceQuantity = 110 + 210;
				var stockWarningMessage = @"Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:
One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.";
				DoN30Integration(n30Declaration, stockWarningMessage);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				JobComInvoiceLine n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(210, n30Line2, warehousing2.InvoiceLine1);

				n30Line1.Delete();
				n30Line2.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartNo = "~~1";
				n30Line1.JI_InvoiceUQ = "NO";
				n30Line1.JI_InvoiceQuantity = 200;
				n30Line2 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line2.JI_PartNo = "~~1";
				n30Line2.JI_InvoiceUQ = "NO";
				n30Line2.JI_InvoiceQuantity = 100;

				DoN30Integration(n30Declaration, stockWarningMessage);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(190, n30Line2, warehousing2.InvoiceLine1);
			}
		}

		void DoN30Integration(JobDeclaration n30Declaration, string warning = null)
		{
			n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
			AssertEquals("No problem", warning, ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);
		}

		void CheckInvoiceLine1Basic(JobComInvoiceLine n30Line1)
		{
			AssertEquals("Warehouse", warehouse1.PK, n30Line1.AddInfo.WarehouseAddress.PK);
			AssertEquals("Product", "~~1", n30Line1.JI_PartNo);
			AssertEquals("CustomsUQ", "KG", n30Line1.JI_CustomsUnitQty);
			AssertEquals("InvoiceUQ", "NO", n30Line1.JI_InvoiceUQ);
			AssertEquals("CountryOfOrigin", Core.Constants.CountryCodes.France, n30Line1.AddInfo.ZA_ORG);
			AssertEquals("WRN", "EEE1", n30Line1.AddInfo.ZA_WRN);
			AssertEquals("WRL", 1, n30Line1.AddInfo.ZA_WRL);
			AssertEquals("CustomsQty", 550m, n30Line1.JI_CustomsQuantity);
			AssertEquals("LinePrice", 5500m, n30Line1.JI_LinePrice);
			AssertEquals("TILV", 263.04m, n30Line1.AddInfo.TILVInAUD, 0.01m);
		}

		void CheckInvoiceLine1Calc(ZDecimal invoiceQty, JobComInvoiceLine n30Line, JobComInvoiceLine n20Line)
		{
			ZDecimal factor = invoiceQty / n20Line.JI_InvoiceQuantity;
			AssertEquals("Warehouse", n20Line.WarehouseCCP, n30Line.WarehouseCCP);
			AssertEquals("Product", "~~1", n30Line.JI_PartNo);
			AssertEquals("InvoiceQty", invoiceQty, n30Line.JI_InvoiceQuantity);
			AssertEquals("CustomsUQ", "KG", n30Line.JI_CustomsUnitQty);
			AssertEquals("InvoiceUQ", "NO", n30Line.JI_InvoiceUQ);
			AssertEquals("CountryOfOrigin", n20Line.AddInfo.ZA_ORG.IsEmpty ? n20Line.InvoiceHeader.ZA_ORG : n20Line.AddInfo.ZA_ORG, n30Line.AddInfo.ZA_ORG);
			AssertEquals("WRN", n20Line.CusEntryLine.Header.EntryNumber, n30Line.AddInfo.ZA_WRN);
			AssertEquals("WRL", n20Line.CusEntryLine.CL_LineNumber, n30Line.AddInfo.ZA_WRL);
			AssertEquals("CustomsQty", factor * n20Line.JI_CustomsQuantity, n30Line.JI_CustomsQuantity, 0.0001m);
			AssertEquals("LinePrice", factor * n20Line.JI_LinePrice, n30Line.JI_LinePrice, 0.01m);
			AssertEquals("TILV", factor * n20Line.TransportAndInsurance.Amount, n30Line.AddInfo.TILVInAUD, 0.01m);
		}

		public void TestByEntryNo()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EEE1";
				n30Line1.AddInfo.ZA_WRL = 1;
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 55m, n30Line1.JI_InvoiceQuantity);
				CheckInvoiceLine1Basic(n30Line1);

				n30Line1.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EOEOEO";
				n30Line1.AddInfo.ZA_WRL = 99;
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("should get error", "Cannot release stock from the bonded warehouse. Check the errors on lines for details.", ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);
				Assert("Has error on WRN", n30Line1.AddInfo.ZA_WRNInfo.HasErrors());

				n30Line1.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EEE1";
				n30Line1.AddInfo.ZA_WRL = 1;
				JobComInvoiceLine n30Line2 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line2.AddInfo.ZA_WRN = "EEE2";
				n30Line2.AddInfo.ZA_WRL = 1;
				var stockWarningMessage = @"Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:
One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.";
				DoN30Integration(n30Declaration, stockWarningMessage);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(210, n30Line2, warehousing2.InvoiceLine1);

				n30Line2.JI_InvoiceQuantity = 150;
				DoN30Integration(n30Declaration);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(150, n30Line2, warehousing2.InvoiceLine1);
			}
		}

		public void TestOrderReferenceOnN30WHSSide()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EEE1";
				n30Line1.AddInfo.ZA_WRL = 1;
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				n30Factory.Save();

				BusinessObjectFactory whsFactory = new BusinessObjectFactory();
				ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, n30Declaration.JE_DeclarationReference + "-1");
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, n30Declaration.JE_OH_Importer);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "ORD");
				BusinessObject whsOrder = (BusinessObject)whsFactory.LoadTop1<IWhsOrder>(filter);
				AssertNotNull("Order should be created with correct reference", whsOrder);
				AssertEquals("Should be finalised", false, ((ZDateTimeOffset)whsOrder[WhsDocketSchema.WD_FinalisedDate.Name]).IsEmpty);
			}
		}

		public void TestWithWRQandWRUAndADJ()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.InvoiceLine1.AddInfo.ZA_WRQ = warehousing1.InvoiceLine1.JI_CustomsQuantity;
				warehousing1.InvoiceLine1.AddInfo.ZA_WRU = warehousing1.InvoiceLine1.JI_CustomsUnitQty;
				warehousing1.InvoiceLine1.JI_CustomsQuantity = 5;
				warehousing1.InvoiceLine1.JI_CustomsUnitQty = "L";
				warehousing1.InvoiceLine1.AddInfo.ZA_ADJ = "11000AUD";
				warehousing1.InvoiceLine1.JI_LinePrice = 0;
				warehousing1.InvoiceLine1.AddInfo.ZA_TILV = "95.04AUD";
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EEE1";
				n30Line1.AddInfo.ZA_WRL = 1;
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 55m, n30Line1.JI_InvoiceQuantity);
				CheckInvoiceLine1BasicWRUWRQ(n30Line1);
			}
		}

		void CheckInvoiceLine1BasicWRUWRQ(JobComInvoiceLine n30Line1)
		{
			AssertEquals("Warehouse", warehouse1.PK, n30Line1.AddInfo.WarehouseAddress.PK);
			AssertEquals("Product", "~~1", n30Line1.JI_PartNo);
			AssertEquals("ZA_WRU", "KG", n30Line1.AddInfo.ZA_WRU);
			AssertEquals("InvoiceUQ", "NO", n30Line1.JI_InvoiceUQ);
			AssertEquals("CountryOfOrigin", Core.Constants.CountryCodes.France, n30Line1.AddInfo.ZA_ORG);
			AssertEquals("WRN", "EEE1", n30Line1.AddInfo.ZA_WRN);
			AssertEquals("WRL", 1, n30Line1.AddInfo.ZA_WRL);
			AssertEquals("ZA_WRQ", 550m, n30Line1.AddInfo.ZA_WRQ);
			AssertEquals("LinePrice", 5500m, n30Line1.JI_LinePrice);
			AssertEquals("TILV", 47.52m, n30Line1.AddInfo.TILVInAUD, 0.01m);
		}

		public void TestRecyclingOfInvoiceLinesWithMultipleSyncs()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.AddInfo.ZA_WRN = "EEE1";
				n30Line1.AddInfo.ZA_WRL = 1;
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				CheckInvoiceLine1Basic(n30Line1);

				DoN30Integration(n30Declaration);
				DoN30Integration(n30Declaration);
				DoN30Integration(n30Declaration);
				CheckInvoiceLine1Basic(n30Line1);

				AssertEquals("InvoiceLines count", 1, n30Declaration.FilteredInvoiceLines.Count);
				AssertEquals("Line is recyled", n30Line1, n30Declaration.FilteredInvoiceLines[0]);
				AssertEquals("Line is should not be deleted", false, n30Line1.IsDeleted);

				JobComInvoiceLine n30Line2 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line2.AddInfo.ZA_WRN = "EEE2";
				n30Line2.AddInfo.ZA_WRL = 1;
				n30Line2.JI_InvoiceQuantity = 55;
				n30Line2.AddInfo.UseBondedWarehouseAutomation = true;
				DoN30Integration(n30Declaration);

				AssertEquals("InvoiceLines count", 2, n30Declaration.FilteredInvoiceLines.Count);
				CheckInvoiceLine1Basic(n30Line1);
				n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(55, n30Line2, warehousing2.InvoiceLine1);
				AssertEquals("Line is recyled", n30Line1, n30Declaration.FilteredInvoiceLines[0]);
				AssertEquals("Line is should not be deleted", false, n30Line1.IsDeleted);

				DoN30Integration(n30Declaration);
				DoN30Integration(n30Declaration);
				DoN30Integration(n30Declaration);

				AssertEquals("InvoiceLines count", 2, n30Declaration.FilteredInvoiceLines.Count);
				AssertEquals("Line is recyled", n30Line1, n30Declaration.FilteredInvoiceLines[0]);
				AssertEquals("Line is should not be deleted", false, n30Line1.IsDeleted);
				AssertEquals("Line is recyled", n30Line2, n30Declaration.FilteredInvoiceLines[1]);
				AssertEquals("Line is should not be deleted", false, n30Line2.IsDeleted);
				CheckInvoiceLine1Basic(n30Line1);
				CheckInvoiceLine1Calc(55, n30Line2, warehousing2.InvoiceLine1);
			}
		}

		public void TestPartAttrib1()
		{
			CheckPartAttribN(1);
		}

		public void TestPartAttrib2()
		{
			CheckPartAttribN(2);
		}

		public void TestPartAttrib3()
		{
			CheckPartAttribN(3);
		}

		void CheckPartAttribN(int n)
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.Declaration.Importer.MiscServ["OM_IMPartAttrib" + n + "Name"] = "VIN";
				warehousing1.Declaration.Importer.MiscServ["OM_IMPartAttrib" + n + "Type"] = "NON";

				warehousing1.Declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part1, n, true);
				warehousing1.Declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part2, n, true);
				warehousing1.InvoiceLine1["JI_PartAttrib" + n] = "PA1";
				warehousing1.InvoiceLine2["JI_PartAttrib" + n] = "PA2";
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1["JI_PartAttrib" + n] = "PA1";
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 55m, n30Line1.JI_InvoiceQuantity);
				AssertEquals("JI_PartAttrib1", "PA1", n30Line1["JI_PartAttrib" + n]);
				CheckInvoiceLine1Basic(n30Line1);

				// TO BE CHECKED IN WHEN THIS BUG IS FIXED BY CRAIG. IT IS SHELVED BY USER JAMES, SHELF NAME 'forCraig'.
				//N30Line1["JI_PartAttrib" + N] = "";
				//DoN30Integration(N30Declaration);
				//AssertEquals(1, N30Declaration.InvoiceLines.Count);
				//N30Line1 = N30Declaration.InvoiceLines[0];
				//AssertEquals("InvoiceQty", 55m, N30Line1.JI_InvoiceQuantity);
				//AssertEquals("JI_PartAttrib1", "PA1", N30Line1["JI_PartAttrib" + N]);
				//CheckInvoiceLine1Basic(N30Line1);
			}
		}

		public void TestByPartAttribExtended()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				warehousing1.Declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part1, 1, true);
				warehousing1.Declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part2, 1, true);
				warehousing1.InvoiceLine1.JI_PartAttrib1 = "PA1";
				warehousing1.InvoiceLine2.JI_PartAttrib1 = "PA2";
				warehousing2.InvoiceLine1.JI_PartAttrib1 = "PA1";
				warehousing2.InvoiceLine2.JI_PartAttrib1 = "PA2";
				warehousing1.Declaration.DoMerge();
				warehousing2.Declaration.DoMerge();
				Factory.Save();

				BatchProcessorGetsPayMessage(warehousing1, "EEE1", ZDateTime.Now.AddDays(-2));
				BatchProcessorGetsPayMessage(warehousing2, "EEE2", ZDateTime.Now);

				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				JobDeclaration n30Declaration = JobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				JobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "PA1";
				n30Line1.JI_InvoiceQuantity = 55;
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration);
				AssertEquals(1, n30Declaration.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 55m, n30Line1.JI_InvoiceQuantity);
				AssertEquals("JI_PartAttrib1", "PA1", n30Line1.JI_PartAttrib1);
				CheckInvoiceLine1Basic(n30Line1);

				n30Line1.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "ZZZ";
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("should get error", "Cannot release stock from the bonded warehouse. Check the errors on lines for details.", ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);
				Assert("Has error on PartAttrib1", n30Line1.JI_PartAttrib1Info.HasErrors());

				n30Line1.Delete();
				n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "PA1";
				var stockWarningMessage = @"Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:
One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.";
				DoN30Integration(n30Declaration, stockWarningMessage);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				JobComInvoiceLine n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(210, n30Line2, warehousing2.InvoiceLine1);

				n30Line2.JI_InvoiceQuantity = 150;
				DoN30Integration(n30Declaration);
				AssertEquals(2, n30Declaration.FilteredInvoiceLines.Count);
				n30Declaration.FilteredInvoiceLines.Sort(JobComInvoiceLineSchema.JI_InvoiceQuantity.Name, ListSortDirection.Ascending);
				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				n30Line2 = n30Declaration.FilteredInvoiceLines[1];
				CheckInvoiceLine1Calc(110, n30Line1, warehousing1.InvoiceLine1);
				CheckInvoiceLine1Calc(150, n30Line2, warehousing2.InvoiceLine1);

				n30Declaration.Factory.Save();

				BusinessObjectFactory n30Factory2 = new BusinessObjectFactory();
				JobDeclaration n30Declaration2 = JobDeclaration.New(n30Factory2);
				n30Declaration2.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration2.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				n30Line1 = n30Declaration2.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "PA1";
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;

				DoN30Integration(n30Declaration2, stockWarningMessage);
				AssertEquals(1, n30Declaration2.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration2.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 60m, n30Line1.JI_InvoiceQuantity);
				CheckInvoiceLine1Calc(60, n30Line1, warehousing2.InvoiceLine1);

				n30Declaration2.Factory.Save();

				BusinessObjectFactory n30Factory3 = new BusinessObjectFactory();
				JobDeclaration n30Declaration3 = JobDeclaration.New(n30Factory3);
				n30Declaration3.JE_OH_Importer = warehousing1.Declaration.Importer.PK;
				n30Declaration3.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				n30Line1 = n30Declaration3.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "PA1";
				n30Line1.AddInfo.UseBondedWarehouseAutomation = true;
				n30Declaration3.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration3.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("should get error", "Cannot release stock from the bonded warehouse. Check the errors on lines for details.", ((SendsMessagesToCustomsShutterUpperer)n30Declaration3.MessageInitiator).Warning);
				Assert("Has error on PartAttrib1", n30Line1.JI_PartAttrib1Info.HasErrors());

				n30Declaration2.CancelBondedWarehouseIntegration();
				n30Declaration2.Factory.Save();

				// need to do this to get around WhsInventory.SubscribeToDataRefreshOnCreation == false
				n30Declaration3.Factory.Save();
				BusinessObjectFactory n30Factory33 = new BusinessObjectFactory();
				JobDeclaration n30Declaration33 = n30Factory33.Load<JobDeclaration>(n30Declaration3.PK);

				DoN30Integration(n30Declaration33, stockWarningMessage);
				AssertEquals(1, n30Declaration33.FilteredInvoiceLines.Count);
				n30Line1 = n30Declaration33.FilteredInvoiceLines[0];
				AssertEquals("InvoiceQty", 60m, n30Line1.JI_InvoiceQuantity);
				CheckInvoiceLine1Calc(60, n30Line1, warehousing2.InvoiceLine1);
			}
		}
	}
}
