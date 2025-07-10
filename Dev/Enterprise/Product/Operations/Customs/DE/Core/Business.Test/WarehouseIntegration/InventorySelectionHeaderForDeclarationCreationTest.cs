using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeaderForDeclarationCreation))]
	sealed class InventorySelectionHeaderForDeclarationCreationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreatesEntryInstruction()
		{
			PrepareInventoryAndImportInventories(ZString.Empty);
			AssertEquals(1, GetSingleCreatedImportDeclaration().CustomsEntryInstructions.Count);
			AssertEquals("40", GetSingleCreatedImportDeclaration().CustomsEntryInstructions.First().CEI_Procedure);
		}

		public void TestFilterDefaults()
		{
			CombineAssertions(() =>
			{
				var filters = header.GetFilterDefaults();
				AssertEquals("Only Client and Area Type filter should be present as the OrgProxy does not have a related Warehouse", 2, filters.OfType<FilterBusinessObjectDefault>().Count());

				var clientFilter = filters["Client:Property"];
				AssertEquals("Client should default to the Current branch's OrgProxy", GlbBranch.CurrentBranch.OrgProxy.PK, clientFilter.Value);
				AssertEquals("Default filter value can be edited", expected: true, clientFilter.IsRemovable);

				var whsWarehouse = helper.GetNewWhsWarehouse(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, isVirtualWarehouse: false, "WH1");
				Factory.Save();

				filters = header.GetFilterDefaults();
				AssertEquals("Warehouse, Client and Area Type should be defaulted now that a Warehouse is set for the OrgProxy", 3, filters.OfType<FilterBusinessObjectDefault>().Count());

				var warehouseFilter = filters["Warehouse:Property"];
				AssertEquals("Warehouse filter should default to the Warehouse linked to the OrgProxy", whsWarehouse.PK, warehouseFilter.Value);
				AssertEquals("Default filter value can be edited", expected: true, warehouseFilter.IsRemovable);

				var pickAreaTypeFilter = filters["Pick Area Type:Property"];
				AssertEquals("Pick Area Type filter should be BON for 'Select From Inventory'", "BON", pickAreaTypeFilter.Value);
				AssertEquals("Default filter value can not be edited", expected: false, pickAreaTypeFilter.IsRemovable);

				createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
				header = new InventorySelectionHeaderForDeclarationCreation(createDeclarationBizObj, updateWarehouse: false);
				filters = header.GetFilterDefaults();
				AssertEquals("Pick Area Type filter should not be present for CreateFromWarehouseOrder", false, filters.ContainsDefaultFor("Pick Area Type:Property"));
			});
		}

		public void TestSupplier()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.OH_Code = "SUPPLIER";
			var suppAddress = Factory.NewWithValidTestData<OrgAddress>();
			suppAddress.OA_Address1 = "Supplier Street 1";
			suppAddress.OA_Code = "SUPP_ADDRESS";
			suppAddress.OA_OH = supplierOrg.PK;
			PrepareInventoryAndImportInventories("Supplier=SUPPLIER;SUPP_ADDRESS");
			AssertEquals(suppAddress.PK, GetSingleCreatedImportDeclaration().SupplierDocumentaryAddress.E2_OA_Address);
		}

		public void TestImporter()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_Code = "IMPORTER";
			var impAddress = Factory.NewWithValidTestData<OrgAddress>();
			impAddress.OA_Code = "IMP_ADDRESS";
			impAddress.OA_OH = importerOrg.PK;
			PrepareInventoryAndImportInventories("Importer=IMPORTER;IMP_ADDRESS");
			AssertEquals(impAddress.PK, GetSingleCreatedImportDeclaration().ImporterDocumentaryAddress.E2_OA_Address);
		}

		public void TestJE_MessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			PrepareInventoryAndImportInventories(ZString.Empty);
			AssertEquals(MessageTypeList.Codes.Import, GetSingleCreatedImportDeclaration().JE_MessageType);
		}

		public void TestJE_TransportMode()
		{
			PrepareInventoryAndImportInventories("Transport=AIR");
			AssertEquals("AIR", GetSingleCreatedImportDeclaration().JE_TransportMode);
		}

		public void TestJE_RL_NKPortOfLoading()
		{
			PrepareInventoryAndImportInventories("PortOfLoading=DEWIB");
			AssertEquals("DEWIB", GetSingleCreatedImportDeclaration().JE_RL_NKPortOfLoading);
		}

		public void TestJE_RL_NKPortOfFirstArrival()
		{
			PrepareInventoryAndImportInventories("FirstEUArrival=DEWIB");
			AssertEquals("DEWIB", GetSingleCreatedImportDeclaration().JE_RL_NKPortOfFirstArrival);
		}

		public void TestZG_MethodOfPayment_ShouldBeE_WhenDeclarationTypeIsEZA()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(MethodOfPaymentTypes.E, declaration.ZG_MethodOfPayment);
		}

		public void TestZG_MethodOfPayment_ShouldBeEmpty_WhenDeclarationTypeIsNotEZA()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EAV;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNullOrEmpty(declaration.ZG_MethodOfPayment);
		}

		public void TestJE_ContainerMode_ShouldBeNCT()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
		}

		public void TestJE_RN_NKTransportNationality_ShouldBeDE()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(Core.Constants.CountryCodes.Germany, declaration.JE_RN_NKTransportNationality);
		}

		public void TestZG_Box18TransportID_ShouldBeLKW()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(UniversalReferenceConstants.TransportIds.LKW, declaration.ZG_Box18TransportID);
		}

		public void TestJE_HouseBill_ShouldBeDeclarantsReferenceSequential()
		{
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "Transport=AIR", PreviousEntryNumber, 1);
			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "Transport=SEA", PreviousEntryNumber, 2);
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

			var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));
			AssertEquals(2, declarations.Length);
			AssertNotNull(declarations.SingleOrDefault(d => d.JE_HouseBill == "EZA-FVZL01_01"));
			AssertNotNull(declarations.SingleOrDefault(d => d.JE_HouseBill == "EZA-FVZL01_02"));
		}

		public void TestJE_HouseBill_ShouldBeEmpty_WhenDeclarantsReferenceIsEmpty()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNullOrEmpty(declaration.JE_HouseBill);
		}

		public void TestJE_OwnerRef_ShouldBeDeclarantsReferenceSequential()
		{
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "Transport=AIR", PreviousEntryNumber, 1);
			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "Transport=SEA", PreviousEntryNumber, 2);
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

			var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));
			AssertEquals(2, declarations.Length);
			AssertNotNull(declarations.SingleOrDefault(d => d.JE_OwnerRef == "EZA-FVZL01_01"));
			AssertNotNull(declarations.SingleOrDefault(d => d.JE_OwnerRef == "EZA-FVZL01_02"));
		}

		public void TestJE_OwnerRef_ShouldBeEmpty_WhenDeclarantsReferenceIsEmpty()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNullOrEmpty(declaration.JE_HouseBill);
		}

		public void TestJE_CustomsOffice_ShouldBeCustomsOffice()
		{
			createDeclarationBizObj.CustomsOffice = "DE00001";
			PrepareInventoryAndImportInventories("CustomsOffice=DE00001");

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals("DE00001", declaration.JE_CustomsOffice);
		}

		public void TestImportInventory_DeclarationGrouping()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.OH_Code = "SUPPLIER";
			var supplierAddress = Factory.NewWithValidTestData<OrgAddress>();
			supplierAddress.OA_Code = "SUPP_ADDRESS";
			supplierAddress.OA_OH = supplierOrg.PK;
			var supplierAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			supplierAddress2.OA_Code = "SUPP_ADDRESS2";
			supplierAddress2.OA_OH = supplierOrg.PK;
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_Code = "IMPORTER";
			var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
			importerAddress.OA_Code = "IMP_ADDRESS";
			importerAddress.OA_OH = importerOrg.PK;
			var importerAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			importerAddress2.OA_Code = "IMP_ADDRESS2";
			importerAddress2.OA_OH = importerOrg.PK;

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo1 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo1, PreviousEntryNumber, 1);

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);

			var part3 = helper.CreateProduct(helper.Importer.PK, "~~3");
			var receiveLine3 = helper.GetNewWhsReceiveLine(receive.PK, part3.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-3", ZDateTime.Today.AddMonths(-1));
			var addInfo3 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS2*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo3, PreviousEntryNumber, 3);

			var part4 = helper.CreateProduct(helper.Importer.PK, "~~4");
			var receiveLine4 = helper.GetNewWhsReceiveLine(receive.PK, part4.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-4", ZDateTime.Today.AddMonths(-1));
			var addInfo4 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS2*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine4.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo4, PreviousEntryNumber, 4);

			var part5 = helper.CreateProduct(helper.Importer.PK, "~~5");
			var receiveLine5 = helper.GetNewWhsReceiveLine(receive.PK, part5.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-5", ZDateTime.Today.AddMonths(-1));
			var addInfo5 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEBBR*FirstEUArrival=DEFRA*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine5.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo5, PreviousEntryNumber, 5);

			var part6 = helper.CreateProduct(helper.Importer.PK, "~~6");
			var receiveLine6 = helper.GetNewWhsReceiveLine(receive.PK, part6.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-6", ZDateTime.Today.AddMonths(-1));
			var addInfo6 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEBER*Transport=AIR";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine6.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo6, PreviousEntryNumber, 6);

			var part7 = helper.CreateProduct(helper.Importer.PK, "~~7");
			var receiveLine7 = helper.GetNewWhsReceiveLine(receive.PK, part7.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-7", ZDateTime.Today.AddMonths(-1));
			var addInfo7 = "LinePrice=1000.0000*LinePriceCurrency=USD*Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=SEA";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine7.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo7, PreviousEntryNumber, 6);
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine3.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine4.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine5.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine6.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine7.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory, receiveLine3.Inventory, receiveLine4.Inventory, receiveLine5.Inventory, receiveLine6.Inventory, receiveLine7.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 100m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.SelectionLines[2].US_ProductQtyToDraw = 100m;
			header.SelectionLines[3].US_ProductQtyToDraw = 100m;
			header.SelectionLines[4].US_ProductQtyToDraw = 100m;
			header.SelectionLines[5].US_ProductQtyToDraw = 100m;
			header.SelectionLines[6].US_ProductQtyToDraw = 100m;
			header.ImportInventories();

			CombineAssertions(() =>
			{
				var declarations = Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));
				AssertEquals("Declarations: count", 6, declarations.Length);
				AssertEquals("Declaration with 2 InvoiceLines", 1, declarations.Where(x => x.InvoiceLines.Count == 2).Count());
				AssertEquals("Declaration with 1 InvoiceLine", 5, declarations.Where(x => x.InvoiceLines.Count == 1).Count());
				declarations.ForEach(x => AssertContainsExactElementsInAnyOrder("RelatedDeclarations", x.RelatedDeclarations.Cast<JobDeclaration>(), declarations.Except(new[] { x })));
			});
		}

		public void TestWarehouseUpdateIsCalledAndResultsInAnError()
		{
			header = new InventorySelectionHeaderForDeclarationCreation(createDeclarationBizObj, updateWarehouse: true);
			PrepareInventoryAndImportInventories("Transport=AIR");
			AssertEquals("Error - Cannot Import Order\r\nNo Client Address was provided.", header.ImportInventoriesResult);
		}

		public void TestCreateAndPopulateEntryInstructions()
		{
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg.OH_Code = "SUPPLIER";
			var supplierAddress = Factory.NewWithValidTestData<OrgAddress>();
			supplierAddress.OA_Code = "SUPP_ADDRESS";
			supplierAddress.OA_OH = supplierOrg.PK;
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg.OH_Code = "IMPORTER";
			var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
			importerAddress.OA_Code = "IMP_ADDRESS";
			importerAddress.OA_OH = importerOrg.PK;

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);

			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var addInfo = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, PreviousEntryNumber, 1);

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			var addInfo2 = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo2, PreviousEntryNumber, 2);

			var part3 = helper.CreateProduct(helper.Importer.PK, "~~3");
			var receiveLine3 = helper.GetNewWhsReceiveLine(receive.PK, part3.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-3", ZDateTime.Today.AddMonths(-1));
			var addInfo3 = "Supplier=SUPPLIER;SUPP_ADDRESS*Importer=IMPORTER;IMP_ADDRESS*PortOfLoading=DEWIB*FirstEUArrival=DEFRA*Transport=AIR*LinePriceCurrency=EUR*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 3000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo3, PreviousEntryNumber, 3);
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine1.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine2.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			receiveLine3.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine1.Inventory, receiveLine2.Inventory, receiveLine3.Inventory });
			header.SelectionLines[0].US_ProductQtyToDraw = 100m;
			header.SelectionLines[1].US_ProductQtyToDraw = 100m;
			header.SelectionLines[2].US_ProductQtyToDraw = 100m;
			header.ImportInventories();
			var declaration = GetSingleCreatedImportDeclaration();
			var entryInstructions = declaration.CustomsEntryInstructions;
			AssertEquals("EntryInstruction: count", 2, entryInstructions.Count);

			CombineAssertions("EntryInstruction USD", () =>
			{
				var invoiceHeaderUSD = declaration.Invoices.Single(x => x.JZ_RX_NKInvoice_Currency == "USD");
				var entryInstructionUSD = declaration.CustomsEntryInstructions.Single(x => x.PK == invoiceHeaderUSD.InvoiceLines[0].JI_CEI);
				AssertEquals("CEI_OA_Warehouse", helper.WhsWarehouse.WW_OA_WarehouseAddress, entryInstructionUSD.CEI_OA_Warehouse);
				AssertEquals("Second InvoiceLine linked", entryInstructionUSD.PK, invoiceHeaderUSD.InvoiceLines[1].JI_CEI);
			});

			CombineAssertions("EntryInstruction EUR", () =>
			{
				var invoiceHeaderEUR = declaration.Invoices.Single(x => x.JZ_RX_NKInvoice_Currency == "EUR");
				var entryInstructionEUR = declaration.CustomsEntryInstructions.Single(x => x.PK == invoiceHeaderEUR.InvoiceLines[0].JI_CEI);
				AssertEquals("CEI_OA_Warehouse", helper.WhsWarehouse.WW_OA_WarehouseAddress, entryInstructionEUR.CEI_OA_Warehouse);
			});
		}

		public void TestEntryInstruction_CEI_Style_ShouldBeDeclarationType()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.VZA;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(ImportDeclarationTypeList.Codes.VZA, declaration.CustomsEntryInstructions[0].CEI_Style);
		}

		public void TestEntryInstruction_CEI_SubStyle_ShouldBeA_WhenDeclarationTypeIsEZA()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(EntrySubStyleList.Codes.NormalDeclaration, declaration.CustomsEntryInstructions[0].CEI_SubStyle);
		}

		public void TestEntryInstruction_CEI_SubStyle_ShouldBeC_WhenDeclarationTypeIsVZA()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.VZA;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(EntrySubStyleList.Codes.SimplifiedDeclaration, declaration.CustomsEntryInstructions[0].CEI_SubStyle);
		}

		public void TestEntryInstruction_CEI_SubStyle_ShouldBeC_WhenDeclarationTypeIsAZ()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.AZ;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(EntrySubStyleList.Codes.SimplifiedDeclaration, declaration.CustomsEntryInstructions[0].CEI_SubStyle);
		}

		public void TestEntryInstruction_CEI_Procedure_ShouldBeCPC()
		{
			createDeclarationBizObj.CPC = ImportMainProcedureCodeList.Codes._42;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(ImportMainProcedureCodeList.Codes._42, declaration.CustomsEntryInstructions[0].CEI_Procedure);
		}

		[TestDate(2023, 8, 23)]
		public void TestEntryInstruction_CEI_LocalClearanceDate_ShouldBeToday_WhenDeclarationTypeIsAZ()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.AZ;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(new ZDateTime(2023, 8, 23), declaration.CustomsEntryInstructions[0].CEI_LocalClearanceDate);
		}

		[TestDate(2023, 8, 23)]
		public void TestEntryInstruction_CEI_LocalClearanceDate_ShouldBeEmpty_WhenDeclarationTypeIsNotAZ()
		{
			createDeclarationBizObj.DeclarationType = ImportDeclarationTypeList.Codes.EZA;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			Assert(declaration.CustomsEntryInstructions[0].CEI_LocalClearanceDate.IsEmpty);
		}

		public void TestEntryInstruction_ShouldBeLinkedToDV1DetailsPivots_WhenDeclarationZG_IsHighValueOvrdIsTrue()
		{
			EU.Registry.EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine1 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine1.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "LinePriceCurrency=EUR", PreviousEntryNumber, 1);
			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", "LinePriceCurrency=USD", PreviousEntryNumber, 2);
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

			var declaration = GetSingleCreatedImportDeclaration();
			Assert("Precondition", declaration.ZG_IsHighValueOvrd);

			AssertEquals("Precondition", 2, declaration.CustomsEntryInstructions.Count);
			Assert(declaration.CustomsEntryInstructions.All(ei => ei.DV1DetailsPivots[0].IsForEntryInstruction));
		}

		public void TestBill_ShouldBeCreated()
		{
			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";

			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals("Precondition", "EZA-FVZL01_01", declaration.JE_HouseBill);
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(BillTypeList.Codes.HouseBill, declaration.Bills[0].CU_BillType);
			AssertEquals("EZA-FVZL01_01", declaration.Bills[0].CU_BillNum);
		}

		public void TestPackage_ShouldBeCreated()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"PK", "PK", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			createDeclarationBizObj.DeclarantsReference = "EZA-FVZL01";

			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals("Precondition", 1, declaration.Bills.Count);
			AssertEquals("Precondition", "EZA-FVZL01_01", declaration.Bills[0].CU_BillNum);

			AssertEquals(1, declaration.Packages.Count);
			AssertEquals(RefCusCodeUnPackedPackageUnitType.Unpacked, declaration.Packages[0].CW_PackType);
			AssertEquals(declaration.Bills[0], declaration.Packages[0].Bill);

			AssertEquals("Precondition", (short)1, declaration.InvoiceLines[0].JI_LineNo);
			Assert(declaration.InvoiceLines[0].PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pp => pp.CHC_CW == declaration.Packages[0].PK));
		}

		[TestDate(2023, 8, 23)]
		public void TestHeaders_ShouldHaveDefaultNumberAndDate()
		{
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals("INV01", declaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals(new ZDateTime(2023, 8, 23), declaration.Invoices[0].JZ_InvoiceDate);
		}

		public void TestInvoiceLine_JI_Procedure_ShouldStartFromCPC()
		{
			createDeclarationBizObj.CPC = ImportMainProcedureCodeList.Codes._42;
			PrepareInventoryAndImportInventories(string.Empty);

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals("4211", declaration.InvoiceLines[0].JI_Procedure);
		}

		public void TestOOIIsSetWhenImportingOrder()
		{
			_ = PrepareAndImportOrder();

			var declaration = GetSingleCreatedImportDeclaration();
			AssertEquals(true, declaration.IsOutwardOrderImported);
		}

		public void TestDeclarationIsLinkedToOrder()
		{
			var order = PrepareAndImportOrder();

			var declaration = GetSingleCreatedImportDeclaration();

			var docketJobPivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, declaration.PK);
			var docketJobPivot = Factory.LoadTop1<IWhsDocketJobPivot>(docketJobPivotQuery);

			AssertEquals(order.PK, docketJobPivot.WV_WD_Docket);
		}

		public void TestInvoiceLineOrderInfoIsSet()
		{
			var order = PrepareAndImportOrder();

			var declaration = GetSingleCreatedImportDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertEquals(order.WD_DocketID, invoiceLine.JI_BondedWHSOrderNumber);
				AssertEquals((ZShort)1, invoiceLine.JI_BondedWHSOrderLineNumber);
			});
		}

		public void TestNonEuropeanImporter_CreateDeclarationFromOrder()
		{
			CreateUSAddressOrgWithAddress();

			PrepareInventoryAndImportInventories("Importer=W1;W1 ADDRESS 1");

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNotNull("Declaration with JE_MessageType = EXP is created", declaration);
		}

		public void TestNonEuropeanSupplier_CreateDeclarationFromOrder()
		{
			CreateUSAddressOrgWithAddress();

			PrepareInventoryAndImportInventories("Supplier=W1;W1 ADDRESS 1");

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNotNull("Declaration with JE_MessageType = EXP is created", declaration);
		}

		public void TestNonEuropeanImporter_CreateDeclarationFromInventory()
		{
			CreateUSAddressOrgWithAddress();

			PrepareAndImportOrder("Importer=W1;W1 ADDRESS 1");

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNotNull("Declaration with JE_MessageType = EXP is created", declaration);
		}

		public void TestNonEuropeanSupplier_CreateDeclarationFromInventory()
		{
			CreateUSAddressOrgWithAddress();

			PrepareAndImportOrder("Supplier=W1;W1 ADDRESS 1");

			var declaration = GetSingleCreatedImportDeclaration();
			AssertNotNull("Declaration with JE_MessageType = EXP is created", declaration);
		}

		public void TestUpdateSelectionLinesDetails_IsInventoryValid()
		{
			var whsHelper = helper.WhsHelper;
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
			var whsWarehouse2 = helper.GetNewWhsWarehouse(helper.Warehouse2.MainAddress.PK, true, "N20");

			var area = whsHelper.CreateWhsArea(whsWarehouse.PK, "REC", "REC");
			var row = whsHelper.CreateRowAndGenerateLocations(whsWarehouse, "R");
			var location = row.Locations[0] as IWhsLocation;
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			whsReceive.WD_DocketSubType = "REC";
			var whsReceive2 = helper.GetNewWhsReceive(whsWarehouse2.PK, helper.Importer2.PK);

			var part2 = helper.CreateProduct(helper.Importer2.PK, "~~2");
			part2.OP_Desc = "~~2 DESC";

			var whsInventory1 = helper.GetNewReceiveInventory(whsReceive, helper.Part, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN1", bondedEntryKey: "EN00122-1");
			var whsInventory2 = helper.GetNewReceiveInventory(whsReceive2, part2, ZString.Empty, 1m, 1m, 1m, serialNumber: "SN2", bondedEntryKey: "EN00122-2");
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			header.IsGroupByInventory = true;
			header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2 });

			AssertEquals("1 line should be selected, as as GetOnlyCustomsInventories  is true", 1, header.SelectionLines.Count);

			var line = header.SelectionLines[0];

			string docketSubTypeOnSelectedLine = Factory.Load<IWhsDocket>(line.InventoryWrappers[0].Receive.PK)?.WD_DocketSubType;
			AssertEquals("WD_DocketSubType is 'CUS'", "CUS", docketSubTypeOnSelectedLine);
		}

		protected override BusinessObject GetNewBusinessObject() => header;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new WhsDataTestHelper(Factory);
			createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
			header = new InventorySelectionHeaderForDeclarationCreation(createDeclarationBizObj, updateWarehouse: false);
		}
		CreateDeclarationBizObj createDeclarationBizObj;
		InventorySelectionHeaderForDeclarationCreation header;
		WhsDataTestHelper helper;
		const string PreviousEntryNumber = "ENT1234";

		JobDeclaration GetSingleCreatedImportDeclaration() => Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import));

		void PrepareInventoryAndImportInventories(ZString addInfoString)
		{
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfoString, PreviousEntryNumber, 1);
			attr.WB_InwardProcedure = "1140";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });

			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;

			header.ImportInventories();
		}

		IWhsOrder PrepareAndImportOrder(string recvBwhAttributeAddInfo = "")
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper1.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper1.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper1.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper1.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper1.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);

			helper1.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, string.Empty, "40", "78", "", "DES2", "IMP", group: "EZA");

			Factory.Save();

			helper.WhsWarehouse.WW_WarehouseType = "PRW";
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", recvBwhAttributeAddInfo, PreviousEntryNumber, 1);
			attr.WB_InwardProcedure = "1140";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save(); // subsequent calls use ZDBOnlyQuery

			// create and pick order on created inventory
			var order = helper.WhsHelper.CreateWhsOrder(helper.Importer.PK, helper.WhsWarehouse.PK, helper.Importer.PK, "ORDER1") as IWhsOrder;
			order.WD_DocketSubType = "CUS";
			var orderLinePK = helper.WhsHelper.CreateWhsOrderLine(order.PK, helper.Part.PK, 1);
			var orderAttr = helper.GetNewWhsBondedWarehouseAttribute(orderLinePK, 1000m, 1m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "<PENDINGCUSTOMSRESPONSE>", 1);
			orderAttr.WB_OutwardType = "EXS";

			_ = helper.WhsHelper.CreatePickNew(true, true, new[] { order.PK });

			Factory.Save(); // subsequent calls use ZDBOnlyQuery

			var data = new CreateDeclarationBizObj(createFromWarehouseOrder: true)
			{
				DeclarationType = "EZA",
				CustomsOffice = "DE004323",
				CPC = "40",
				DeclarantsReference = "Reference"
			};
			var msgs = data.CreateDeclarationsForWarehouseOrder(order);

			return order;
		}

		void CreateUSAddressOrgWithAddress()
		{
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W1";
			warehouse.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "US")).RL_Code;
			warehouse.MainAddress.OA_Address1 = "W1 ADDRESS 1";
			warehouse.MainAddress.LocalControlledPremisesID = "23423";
			warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
		}
	}
}
