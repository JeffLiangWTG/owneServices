using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeaderForIPRDeclarationCreation))]
	sealed class InventorySelectionHeaderForIPRDeclarationCreationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilterDefaults()
		{
			CombineAssertions(() =>
			{
				var filters = header.GetFilterDefaults();
				AssertEquals("Only Client, Area Type and Customs Deadline should be defaulted now that a Warehouse is set for the OrgProxy", 5, filters.OfType<FilterBusinessObjectDefault>().Count());

				var clientFilter = filters["Client:Property"];
				AssertEquals("Client should default to the Current branch's OrgProxy", GlbBranch.CurrentBranch.OrgProxy.PK, clientFilter.Value);
				AssertEquals("Default filter value can be edited", expected: true, clientFilter.IsRemovable);

				var whsWarehouse = helper.GetNewWhsWarehouse(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, isVirtualWarehouse: false, "WH1");
				Factory.Save();

				filters = header.GetFilterDefaults();
				AssertEquals("Warehouse, Client and Area Type should be defaulted now that a Warehouse is set for the OrgProxy", 6, filters.OfType<FilterBusinessObjectDefault>().Count());

				var warehouseFilter = filters["Warehouse:Property"];
				AssertEquals("Warehouse filter should default to the Warehouse linked to the OrgProxy", whsWarehouse.PK, warehouseFilter.Value);
				AssertEquals("Default filter value can be edited", expected: true, warehouseFilter.IsRemovable);

				var customsDeadlineFilter = filters["Customs Deadline:PropertySearch"];
				var customsDeadlineFilter1 = filters["Customs Deadline:Property1"];
				var customsDeadlineFilter2 = filters["Customs Deadline:Property2"];
				AssertEquals("Customs Deadline filter should default to the Customs Deadline linked to the createDeclarationBizObj", ZDateTime.BrettsBirthday, customsDeadlineFilter1.Value);
				AssertEquals("Customs Deadline filter should default to the Customs Deadline linked to the createDeclarationBizObj", ZDateTime.BrettsBirthday, customsDeadlineFilter2.Value);
				AssertEquals("Customs Deadline filter can not be edited", expected: false, customsDeadlineFilter.IsRemovable);

				var pickAreaTypeFilter = filters["Pick Area Type:Property"];
				AssertEquals("Pick Area Type filter should be IPR for 'Select From Inventory IPR'", "IPR", pickAreaTypeFilter.Value);
				AssertEquals("Pick Area Type filter can not be edited", expected: false, pickAreaTypeFilter.IsRemovable);

				createDeclarationBizObj.DeclarationType = "EZA";
				filters = header.GetFilterDefaults();
				AssertEquals("No Customs Deadline filter for non-AVABR declaration", 3, filters.OfType<FilterBusinessObjectDefault>().Count());
				AssertEquals("Customs Deadline filter not present for non-AVABR declaration", false, filters.ContainsDefaultFor("Customs Deadline:PropertySearch"));
			});
		}

		[GuiTest]
		public void TestCreatedDeclaration_AVABR()
		{
			var importerAddressPk = helper.Importer.MainAddress.PK;
			var supplierAddressPk = helper.Supplier.MainAddress.PK;

			createDeclarationBizObj.CustomsOffice = "DE00001";
			createDeclarationBizObj.CPC = ImportMainProcedureCodeList.Codes._42;
			createDeclarationBizObj.CustomsDeadline = new ZDateTime(2024, 12, 2);

			var error = PrepareInventoryAndImportInventories();
			AssertNullOrEmpty(error);

			var declaration = GetCreatedImportDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("Message type", MessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("Importer address", importerAddressPk, declaration.ImporterDocumentaryAddress.E2_OA_Address);
				AssertEquals("Declarant", importerAddressPk, declaration.Declarant.PK);
				// Entry instruction
				AssertEquals("Single Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);
				AssertEquals("CEI_Style should be AVABR", ImportDeclarationTypeList.Codes.AVABR, declaration.CustomsEntryInstructions[0].CEI_Style);
				AssertEquals("CEI_Procedure should be CPC", ImportMainProcedureCodeList.Codes._42, declaration.CustomsEntryInstructions[0].CEI_Procedure);
				// invoice
				AssertEquals("Valuation date should be as provided Customs Deadline", new ZDateTime(2024, 12, 2), declaration.Invoices[0].JZ_ValuationDateOverride);
				// invoice line
				AssertEquals("JI_Procedure should be 4251", "4251", declaration.InvoiceLines[0].JI_Procedure);
			});
		}

		[GuiTest]
		[TestDate(2024, 11, 5)]
		public void TestCreatedDeclaration_non_AVABR()
		{
			var importerAddressPk = helper.Importer.MainAddress.PK;
			var supplierAddressPk = helper.Supplier.MainAddress.PK;

			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			createDeclarationBizObj.CustomsOffice = "DE00001";
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";
			createDeclarationBizObj.CPC = ImportMainProcedureCodeList.Codes._42;

			var error = PrepareInventoryAndImportInventories("Supplier=SUP324;SUP ADDRESS 1*Importer=IMP;IMP ADDRESS 1*Transport=AIR*PortOfLoading=DEWIB*FirstEUArrival=DEFRA");
			AssertNullOrEmpty(error);

			var declaration = GetCreatedImportDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("Importer address", importerAddressPk, declaration.ImporterDocumentaryAddress.E2_OA_Address);
				AssertEquals("Supplier address", supplierAddressPk, declaration.SupplierDocumentaryAddress.E2_OA_Address);
				AssertEquals("Message type", MessageTypeList.Codes.Import, declaration.JE_MessageType);

				AssertEquals("Transport Mode", "AIR", declaration.JE_TransportMode);
				AssertEquals("DEWIB", declaration.JE_RL_NKPortOfLoading);
				AssertEquals("DEFRA", declaration.JE_RL_NKPortOfFirstArrival);
				AssertEquals(MethodOfPaymentTypes.E, declaration.ZG_MethodOfPayment);
				AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
				AssertEquals(Core.Constants.CountryCodes.Germany, declaration.JE_RN_NKTransportNationality);
				AssertEquals(UniversalReferenceConstants.TransportIds.LKW, declaration.ZG_Box18TransportID);
				AssertEquals("Owner ref", "EZA-FVZL01_01", declaration.JE_OwnerRef);
				AssertEquals("House bill", "EZA-FVZL01_01", declaration.JE_HouseBillForGenericWrapper);
				AssertEquals("Customs office should be DE00001", "DE00001", declaration.JE_CustomsOffice);

				// Entry instruction
				AssertEquals("Single Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);
				AssertEquals("CEI_Style should be EZA", ImportDeclarationTypeList.Codes.EZA, declaration.CustomsEntryInstructions[0].CEI_Style);
				AssertEquals("CEI_Procedure should be CPC", ImportMainProcedureCodeList.Codes._42, declaration.CustomsEntryInstructions[0].CEI_Procedure);
				AssertEquals("JI_Procedure should be 4251", "4251", declaration.InvoiceLines[0].JI_Procedure);

				// Invoices
				AssertEquals("INV01", declaration.Invoices[0].JZ_InvoiceNumber);
				AssertEquals(new ZDateTime(2024, 11, 5), declaration.Invoices[0].JZ_InvoiceDate);
				AssertEquals("Has 1 invoice line", declaration.Invoices[0].InvoiceLines.Count, 1);
			});
		}

		[GuiTest]
		[TestDate(2024, 11, 5)]
		public void TestCreatedDeclaration_WhsOrderCreated_AVABR()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			// we are setting registry setting to "Interfaced" to make sure that test still pass with this setting
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, customsInterface))
			{
				var importerAddressPk = helper.Importer.MainAddress.PK;

				createDeclarationBizObj.CustomsDeadline = new ZDateTime(2024, 12, 2);

				var error = PrepareInventoryAndImportInventories();
				AssertNullOrEmpty(error);

				var declaration = GetCreatedImportDeclaration();
				var orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, Warehouse.Transactions.CodeLists.DocketType.Codes.Order));

				AssertEquals("Should be 1 order", 1, orders.Length);
				var order = orders[0];
				AssertEquals("Should contain 1 line", 1, order.Lines.Count);
				var line = order.Lines[0];

				CombineAssertions(() =>
				{
					AssertEquals("The same reference", declaration.JE_DeclarationReference, order.WD_ExternalReference);
					AssertEquals("Entry number", "AV-Abrechnung vom 05.11.2024", order.WD_CustomerReference);
					AssertEquals("Line entry key", "AV-Abrechnung vom 05.11.2024", line.CustomsData.WB_EntryKey);
					AssertEquals("Line entry key no", (ZShort)1, line.CustomsData.WB_EntryLineNo);
				});
			}
		}

		[GuiTest]
		[TestDate(2024, 11, 4)]
		public void TestGrouping()
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo1 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUP324;SUP ADDRESS 1*Importer=IMP;IMP ADDRESS 1*Transport=AIR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21*InvoiceNumber=12345*InvoiceDate=01-Oct-24";
			var attr1 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo1, PreviousEntryNumber, 1);
			attr1.WB_InwardProcedure = "5100";

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUP324;SUP ADDRESS 1*Importer=IMP;IMP ADDRESS 1*Transport=AIR*IncotermCode=DAP*IncotermPlace=Frankfurt*TransNature=21*InvoiceNumber=12345*InvoiceDate=01-Oct-24";
			var attr2 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);
			attr2.WB_InwardProcedure = "5100";

			var part3 = helper.CreateProduct(helper.Importer.PK, "~~3");
			var receiveLine3 = helper.GetNewWhsReceiveLine(receive.PK, part3.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-3", ZDateTime.Today.AddMonths(-1));
			var addInfo3 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUP324;SUP ADDRESS 1*Importer=IMP;IMP ADDRESS 1*IncotermCode=FOB*IncotermPlace=Hamburg*TransNature=21*InvoiceNumber=12345*InvoiceDate=01-Nov-24";
			var attr3 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo3, PreviousEntryNumber, 3);
			attr3.WB_InwardProcedure = "5100";

			var part4 = helper.CreateProduct(helper.Importer.PK, "~~4");
			var receiveLine4 = helper.GetNewWhsReceiveLine(receive.PK, part4.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-4", ZDateTime.Today.AddMonths(-1));
			var addInfo4 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUP324;SUP ADDRESS 1*Importer=IMP;IMP ADDRESS 1*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=20*InvoiceNumber=12346*InvoiceDate=01-Oct-24";
			var attr4 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine4.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo4, PreviousEntryNumber, 4);
			attr4.WB_InwardProcedure = "5100";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine3.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine4.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			createDeclarationBizObj.CustomsDeadline = new ZDateTime(2024, 11, 1);
			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory, receiveLine3.Inventory, receiveLine4.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 100m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.SelectionLines[2].US_ProductQtyToDraw = 100m;
			header.SelectionLines[3].US_ProductQtyToDraw = 100m;
			header.ImportInventories();

			AssertNullOrEmpty(header.ImportInventoriesResult);

			CombineAssertions(() =>
			{
				var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));
				AssertEquals("Should be single declaration", 1, declarations.Length);
				var declaration = declarations.Single();
				AssertEquals("Should be single Entry Instruction", 1, declaration.CustomsEntryInstructions.Count);
				AssertEquals("Should be 3 Invoice Headers", 3, declaration.Invoices.Count);
				var sortedInvoices = declaration.Invoices.OrderBy(x => x.JZ_InvoiceNumber).ThenBy(x => x.JZ_InvoiceDate).ToArray();
				AssertEquals("Invoice 1 should have 2 lines", 2, sortedInvoices[0].InvoiceLines.Count);
				AssertEquals("Invoice 2 should have 1 line", 1, sortedInvoices[1].InvoiceLines.Count);
				AssertEquals("Invoice 3 should have 1 line", 1, sortedInvoices[2].InvoiceLines.Count);
				var invoiceNumbers = declaration.Invoices.Select(x => new { Number = (string)x.JZ_InvoiceNumber, Date = x.JZ_InvoiceDate }).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { new { Number = "12345", Date = new ZDateTime(2024, 10, 1) }, new { Number = "12345", Date = new ZDateTime(2024, 11, 1) }, new { Number = "12346", Date = new ZDateTime(2024, 10, 1) } }, invoiceNumbers);

				AssertEquals("Merged, one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("Lines are merged", 4, declaration.CustomsEntryHeaders[0].MergedLines.Count);
				AssertEquals("Entry number", "AV-Abrechnung vom 04.11.2024", declaration.CustomsEntryHeaders[0].EntryNumber);
			});
		}

		[GuiTest]
		public void TestInvoices_AVABR()
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMP;IMP ADDRESS 1*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			var attr1 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 100m, "KG", ZString.Empty, 100m, "NO", addInfo, PreviousEntryNumber, 1);
			attr1.WB_InwardProcedure = "5100";
			AddWarehouseCustomsAttributeAddInfo(attr1, "CCT", "*Amount=240*ChargeType=ADD*Currency=CNY*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMP;IMP ADDRESS 1*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			var attr2 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 100m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);
			attr2.WB_InwardProcedure = "5100";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 60m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.ImportInventories();

			AssertNullOrEmpty(header.ImportInventoriesResult);

			var declaration = GetCreatedImportDeclaration();
			var invoice = declaration.Invoices.Single() as JobComInvoiceHeader;
			var invoiceLine1 = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.JI_LineNo == 1);
			var invoiceLine2 = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.JI_LineNo == 2);

			CombineAssertions(() =>
			{
				// part1 ValueForDuty = 1000 / 100 * 60 = 600
				AssertEquals("Line1 JI_LinePrice", 600m, invoiceLine1.JI_LinePrice);
				AssertEquals("Line1 JI_NetPrice", 600m, invoiceLine1.JI_NetPrice);

				AssertEquals("Line2 JI_LinePrice", 1000m, invoiceLine2.JI_LinePrice);
				AssertEquals("Line2 JI_NetPrice", 1000m, invoiceLine2.JI_NetPrice);

				AssertEquals("IncoTerm Code", "FOB", invoice.JZ_IncoTerm);
				AssertEquals("IncoTerm Place", "Frankfurt", invoice.JZ_IncoTermPlace);
				AssertEquals("Currency", "EUR", invoice.JZ_RX_NKInvoice_Currency);

				AssertEquals("No charges calculated", 0, invoiceLine1.Charges.Count);
			});
		}

		[GuiTest]
		public void TestEntryInstruction_ShouldBeLinkedToDV1DetailsPivots_WhenDeclarationZG_IsHighValueOvrdIsTrue_Non_AVABR()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			createDeclarationBizObj.CustomsOffice = "DE00001";
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";

			EU.Registry.EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var addInfo1 = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMP;IMP ADDRESS 1*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr1 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo1, PreviousEntryNumber, 1);
			attr1.WB_InwardProcedure = "5100";
			var addInfo2 = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMP;IMP ADDRESS 1*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var attr2 = helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);
			attr2.WB_InwardProcedure = "5100";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory });

			var line1 = header.SelectionLines[0];
			line1.US_ProductQtyToDraw = 50m;
			var line2 = header.SelectionLines[1];
			line2.US_ProductQtyToDraw = 50m;

			header.ImportInventories();
			AssertNullOrEmpty(header.ImportInventoriesResult);

			var declaration = GetCreatedImportDeclaration();
			Assert("Precondition", declaration.ZG_IsHighValueOvrd);

			Assert(declaration.CustomsEntryInstructions[0].DV1DetailsPivots[0].IsForEntryInstruction);
		}

		[GuiTest]
		public void TestBill_ShouldBeCreated_Non_AVABR()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			createDeclarationBizObj.CustomsOffice = "DE00001";
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";

			var error = PrepareInventoryAndImportInventories();
			AssertNullOrEmpty(error);

			var declaration = GetCreatedImportDeclaration();
			AssertEquals("Precondition", "EZA-FVZL01_01", declaration.JE_HouseBill);
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(BillTypeList.Codes.HouseBill, declaration.Bills[0].CU_BillType);
			AssertEquals("EZA-FVZL01_01", declaration.Bills[0].CU_BillNum);
		}

		[GuiTest]
		public void TestPackage_ShouldBeCreated_Non_AVABR()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			createDeclarationBizObj.CustomsOffice = "DE00001";
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"PK", "PK", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var error = PrepareInventoryAndImportInventories();
			AssertNullOrEmpty(error);

			var declaration = GetCreatedImportDeclaration();
			AssertEquals("Precondition", 1, declaration.Bills.Count);
			AssertEquals("Precondition", "EZA-FVZL01_01", declaration.Bills[0].CU_BillNum);

			AssertEquals(1, declaration.Packages.Count);
			AssertEquals(RefCusCodeUnPackedPackageUnitType.Unpacked, declaration.Packages[0].CW_PackType);
			AssertEquals(declaration.Bills[0], declaration.Packages[0].Bill);

			AssertEquals("Precondition", (short)1, declaration.InvoiceLines[0].JI_LineNo);
			Assert(declaration.InvoiceLines[0].PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pp => pp.CHC_CW == declaration.Packages[0].PK));
		}

		public void TestAutoFillOutDrawQuantities()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Prerequisite", "AVABR", createDeclarationBizObj.DeclarationType);
				AssertEquals("AutoFillOutDrawQuantities true for AVABR", true, header.AutoFillOutDrawQuantities);

				createDeclarationBizObj.DeclarationType = "EZA";
				AssertEquals("AutoFillOutDrawQuantities false for non AVABR", false, header.AutoFillOutDrawQuantities);
			});
		}

		void AddWarehouseCustomsAttributeAddInfo(IWhsBondedWarehouseAttribute customsData, string type, string addInfoData)
		{
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_ParentTableCode = "WB";
			addInfo.B7_ParentID = customsData.PK;
			addInfo.B7_Type = type;
			addInfo.B7_AddInfoData = addInfoData;
		}

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.EnableInwardProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var eun = refDataHelper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			refDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, string.Empty, "40", "51", "", "DES2", "IMP", group: "EZA", outOfWarehouse: true);
			refDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, string.Empty, "42", "51", "", "DES2", "IMP", group: "EZA", outOfWarehouse: true);

			helper = new WhsDataTestHelper(Factory);
			createDeclarationBizObj = new CreateDeclarationIPR();
			createDeclarationBizObj.CustomsDeadline = ZDateTime.BrettsBirthday;
			header = new InventorySelectionHeaderForIPRDeclarationCreation(createDeclarationBizObj);
		}
		CreateDeclarationIPR createDeclarationBizObj;
		InventorySelectionHeaderForIPRDeclarationCreation header;
		WhsDataTestHelper helper;

		const string PreviousEntryNumber = "ENT1234";

		JobDeclaration GetCreatedImportDeclaration() => Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));

		ZString PrepareInventoryAndImportInventories(string addInfoString = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMP;IMP ADDRESS 1")
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfoString, PreviousEntryNumber, 1);
			attr.WB_InwardProcedure = "5100";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });

			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;

			header.ImportInventories();

			return header.ImportInventoriesResult;
		}
	}
}
