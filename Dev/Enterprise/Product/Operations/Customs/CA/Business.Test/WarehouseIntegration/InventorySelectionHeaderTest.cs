using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPair()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;

			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PreviousEntryNumber = "123";
			invoiceLine1.JI_PreviousEntryLineNumber = 0;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "PTLN must be supplied if the Prev. Tran. # has been specified"));

			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. # must be supplied if the PTLN has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPairForCAD()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;

			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PreviousEntryNumber = "123";
			invoiceLine1.JI_PreviousEntryLineNumber = 0;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "PTLN must be supplied if the Prev. Tran. # has been specified"));

			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. # must be supplied if the PTLN has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombination()
		{
			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. #, PTLN and Product Code combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Prev. Tran. #, PTLN and Product Code combination cannot be matched in the Warehouse Inventory"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombinationForCAD()
		{
			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. #, PTLN and Product Code combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Prev. Tran. #, PTLN and Product Code combination cannot be matched in the Warehouse Inventory"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKey()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. #, PTLN combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Prev. Tran. #, PTLN combination cannot be matched in the Warehouse Inventory"));
			AssertEquals(Helper.Part.PK, invoiceLine1.JI_OP);
			AssertEquals("~~1", invoiceLine1.JI_PartNo);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyForCAD()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Prev. Tran. #, PTLN combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Prev. Tran. #, PTLN combination cannot be matched in the Warehouse Inventory"));
			AssertEquals(Helper.Part.PK, invoiceLine1.JI_OP);
			AssertEquals("~~1", invoiceLine1.JI_PartNo);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCode()
		{
			var part3 = Helper.CreateProduct(helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryLineNumber = 0;
			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_OP = part3.PK;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));

			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCodeForCAD()
		{
			var part3 = Helper.CreateProduct(helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "CA", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryLineNumber = 0;
			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_OP = part3.PK;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));

			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestImportInventories_WarrantyRepair()
		{
			var branch = Declaration.Branch;
			branch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Declaration.Invoices.DeleteAll();

			Helper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "WHS");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "12345000000012-1");
			var addInfoString = "TreatmentCode=03*CalculationMethod=W";
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", 12m, "MGM", 1.11111m, "LTR", "NZ", ZDecimal.Zero, "", addInfoString, "12345000000012", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, Header.SelectionLines.Count);
			var line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			AssertEquals(0, Declaration.Invoices.Count);
			AssertEquals(0, Declaration.InvoiceLines.Count);
			Header.ImportInventories();

			AssertEquals("The warranty repair child line is created.", 2, Declaration.InvoiceLines.Count);
			var invoiceLine = Declaration.InvoiceLines[0];
			var childLine = Declaration.InvoiceLines[1];
			AssertEquals(CalculationMethods.Codes.WarrantyRepairsRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(CalculationMethods.Codes.WarrantyRepairsRemission, childLine.CA_CalculationMethod);
			AssertEquals(invoiceLine.PK, childLine.JI_ParentID);
		}

		public void TestImportInventories_DutyDeferral()
		{
			var branch = Declaration.Branch;
			branch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Declaration.Invoices.DeleteAll();

			Helper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "WHS");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "12345000000012-1");
			var addInfoString = "Qty2=15*Qty2UM=MGM*Qty3=1.11111*Qty3UM=LTR*TreatmentCode=03*CalculationMethod=DD";
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", "NZ", ZDecimal.Zero, "", addInfoString, "12345000000012", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, Header.SelectionLines.Count);
			var line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			AssertEquals(0, Declaration.Invoices.Count);
			AssertEquals(0, Declaration.InvoiceLines.Count);
			Header.ImportInventories();

			AssertEquals("The duty defferal child line is created.", 2, Declaration.InvoiceLines.Count);
			var invoiceLine = Declaration.InvoiceLines[0];
			var childLine = Declaration.InvoiceLines[1];
			AssertEquals(CalculationMethods.Codes.DutyDeferral, invoiceLine.CA_CalculationMethod);
			AssertEquals(CalculationMethods.Codes.DutyDeferral, childLine.CA_CalculationMethod);
			AssertEquals(invoiceLine.PK, childLine.JI_ParentID);
			AssertEquals("The parent line charges 40% of the original value", 4800m, invoiceLine.JI_LinePrice);
			AssertEquals("The child line charges 60% of the original value", 7200m, childLine.JI_LinePrice);
		}

		public void TestImportInventories()
		{
			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", ZDateTime.Today.ToShortDateString(), "1234567890", false));
			var branch = Declaration.Branch;
			branch.Company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			Declaration.Invoices.DeleteAll();

			Helper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "WHS");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "EA", "PATT1", "PATT2", "PATT3", "", "12345000000012-1");
			var addInfoString = "TreatmentCode=03";
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", 15m, "MGM", 1.11111m, "LTR", "NZ", ZDecimal.Zero, "", addInfoString, "12345000000012", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, Header.SelectionLines.Count);
			var line = Header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			AssertNull("WarehouseAddress", Declaration.WarehouseAddress);
			AssertEquals(0, Declaration.Invoices.Count);
			AssertEquals(0, Declaration.InvoiceLines.Count);
			Header.ImportInventories();
			AssertEquals("WarehouseAddress", Helper.Warehouse.MainAddress, Declaration.WarehouseAddress);
			AssertEquals(1, Declaration.Invoices.Count);
			var invoice = Declaration.Invoices[0];
			AssertEquals("JZ_InvoiceAmount", 12000m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.NewZealand, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(1, Declaration.InvoiceLines.Count);
			var invoiceLine = Declaration.InvoiceLines[0];
			AssertEquals("JI_PartAttrib1", "PATT1", invoiceLine.JI_PartAttrib1);
			AssertEquals("JI_PartAttrib2", "PATT2", invoiceLine.JI_PartAttrib2);
			AssertEquals("JI_PartAttrib3", "PATT3", invoiceLine.JI_PartAttrib3);
			AssertEquals("JI_TAriff has Number", "1234567890", invoiceLine.JI_Tariff);
			AssertInvoiceLine(invoiceLine, Helper.Part.OP_PartNum, Helper.Part.PK, 12000m, 800m, "EA", 800m, "NMB", "NZ", "12345000000012", 1, 12m, "MGM", 0.888888m, "LTR", "03");
			AssertEquals("C1_PreviousTranNumber of first Duty", "12345000000012", invoiceLine.DutiesAndTaxes.First(t => t.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).C1_PreviousTranNumber);
			AssertEquals("C1_PreviousTranLine of first Duty", 1, invoiceLine.DutiesAndTaxes.First(t => t.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty).C1_PreviousTranLine);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whsWarehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "R");
			Helper.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 1, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 2, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 3, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 1, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 2, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 3, true);
			whsWarehouse.WW_WarehouseName = Helper.Warehouse.MainAddress.OA_Address1;
			whsWarehouse.WW_OA_WarehouseAddress = Helper.Warehouse.MainAddress.PK;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
			Factory.Save();

			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive3 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive4 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive5 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive6 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive1.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive2.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-3);
			whsReceive3.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive4.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-15);
			whsReceive5.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive6.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive1.WD_ExternalReference = "01";
			whsReceive2.WD_ExternalReference = "02";
			whsReceive3.WD_ExternalReference = "03";
			whsReceive4.WD_ExternalReference = "04";
			whsReceive5.WD_ExternalReference = "05";
			whsReceive6.WD_ExternalReference = "06";
			var locationPK = Helper.WhsHelper.FindLocation(whsWarehouse.PK, "R").PK;
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 150m, 20m, "KG", 100m, "MGM", 1.11111m, "LTR", "AU", ZDecimal.Zero, "",
				"TreatmentCode=01", "12345000000012", (ZShort)1);
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive2.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-2", ZDateTime.Today.AddMonths(-3));
			var whsReceiveLine2CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1500m, 120m, "KG", 100m, "MGM", 1.11111m, "LTR", "NZ", ZDecimal.Zero, "",
				"TreatmentCode=02", "12345000000012", (ZShort)2);
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive3.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-3", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine3CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 100m, 10m, "KG", 100m, "MGM", 1.11111m, "LTR", "US", ZDecimal.Zero, "",
				"TreatmentCode=03", "12345000000012", (ZShort)3);
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive4.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-4", ZDateTime.Today.AddDays(-15));
			var whsReceiveLine4CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 300m, 40m, "KG", 100m, "MGM", 1.11111m, "LTR", "SG", ZDecimal.Zero, "",
				"TreatmentCode=04", "12345000000012", (ZShort)4);
			var whsReceiveLine5 = Helper.GetNewWhsReceiveLine(whsReceive5.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-5", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine5CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 2000m, 150m, "KG", 100m, "MGM", 1.11111m, "LTR", "IT", ZDecimal.Zero, "",
				"TreatmentCode=05", "12345000000012", (ZShort)5);
			whsReceiveLine5.WE_BondedEntryKey = "12345000000012-5";
			var whsReceiveLine6 = Helper.GetNewWhsReceiveLine(whsReceive6.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-6", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine6CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 3000m, 200m, "KG", 100m, "MGM", 1.11111m, "LTR", "DE", ZDecimal.Zero, "",
				"TreatmentCode=06", "12345000000012", (ZShort)6);
			var whsInventory = whsReceiveLine1.Inventory;
			whsReceiveLine1.WE_WL = locationPK;
			whsReceiveLine2.WE_WL = locationPK;
			whsReceiveLine3.WE_WL = locationPK;
			whsReceiveLine4.WE_WL = locationPK;
			whsReceiveLine5.WE_WL = locationPK;
			whsReceiveLine6.WE_WL = locationPK;
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsReceive3.FinaliseDocketWithoutUserConfirmation();
			whsReceive4.FinaliseDocketWithoutUserConfirmation();
			whsReceive5.FinaliseDocketWithoutUserConfirmation();
			whsReceive6.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.WhsHelper.CreateWhsAdjustment(Helper.Importer.PK, whsWarehouse.PK, "AD1", null);
			adjustment[WhsDocketSchema.WD_DocketSubType] = "CUS";
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -40m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-2");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-3");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-4");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-5");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-6");
			Helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_TreatmentCode = ZString.Empty;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.CA_TreatmentCode = ZString.Empty;
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			invoice1Line1.JI_PreviousEntryNumber = "12345000000012";
			invoice1Line1.JI_PreviousEntryLineNumber = 4;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, ZDecimal.Zero, ZDecimal.Zero, "UNT", ZDecimal.Zero, "KG", ZString.Empty, "12345000000012", 4, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", ZDateTime.Today.ToShortDateString(), "1234567890", false));
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 120m, 4m, "EA", 4m, "NMB", "SG", "12345000000012", 4, 40m, "MGM", 0.444444m, "LTR", "04");
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 120m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_InvoiceQuantity = 5m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "EA", 5m, "NMB", "SG", "12345000000012", 4, 50m, "MGM", 0.555555, "LTR", "04");
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 150m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PreviousEntryNumber = "";
			invoice1Line1.JI_PreviousEntryLineNumber = 0;
			invoice2Line1.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "EA", 5m, "NMB", "US", "12345000000012", 3, 50m, "MGM", 0.555555, "LTR", "03");
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", "12345000000012", 2, 40m, "MGM", 0.444444, "LTR", "02");
			AssertEquals("invoice1.JZ_InvoiceAmount", 50m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", 600m, invoice2.JZ_InvoiceAmount);

			invoice1.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice2.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.JI_InvoiceQuantity = 5m;
			invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_PartNo = Helper.Part.OP_PartNum;
			invoice2Line2.JI_PartAttrib1 = "PATT1";
			invoice2Line2.JI_PartAttrib2 = "PATT2";
			invoice2Line2.JI_PartAttrib3 = "PATT3";
			invoice2Line2.JI_CustomsUnitQty = "KG";
			invoice2Line2.JI_InvoiceQuantity = 5m;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice1Line2.JI_PartAttrib1 = "PATT1";
			invoice1Line2.JI_PartAttrib2 = "PATT2";
			invoice1Line2.JI_PartAttrib3 = "PATT3";
			invoice1Line2.JI_CustomsUnitQty = "KG";
			invoice1Line2.JI_InvoiceQuantity = 40m;
			var invoice2Line3 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line3.JI_PartNo = Helper.Part.OP_PartNum;
			invoice2Line3.JI_PartAttrib1 = "PATT1";
			invoice2Line3.JI_PartAttrib2 = "PATT2";
			invoice2Line3.JI_PartAttrib3 = "PATT3";
			invoice2Line3.JI_CustomsUnitQty = "KG";
			invoice2Line3.JI_InvoiceQuantity = 5m;
			invoice2Line3.JI_PreviousEntryNumber = "12345000000012";
			invoice2Line3.JI_PreviousEntryLineNumber = 3;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice1Line3.JI_PartAttrib1 = "PATT1";
			invoice1Line3.JI_PartAttrib2 = "PATT2";
			invoice1Line3.JI_PartAttrib3 = "PATT3";
			invoice1Line3.JI_CustomsUnitQty = "KG";
			invoice1Line3.JI_InvoiceQuantity = 40m;
			invoice1Line3.JI_PreviousEntryNumber = "12345000000012";
			invoice1Line3.JI_PreviousEntryLineNumber = 5;
			invoiceLines.Clear();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			invoiceLines.Add(invoice1Line2);
			invoiceLines.Add(invoice2Line2);
			invoiceLines.Add(invoice1Line3);
			invoiceLines.Add(invoice2Line3);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 75m, 5m, "EA", 10m, "KG", "AU", "12345000000012", 1, 50m, "MGM", 0.555555, "LTR", "01");
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", "12345000000012", 2, 40m, "MGM", 0.444444, "LTR", "02");
			AssertInvoiceLine(invoice1Line2, Helper.Part2.OP_PartNum, Helper.Part2.PK, 1200m, 40m, "NO", 80m, "KG", "DE", "12345000000012", 6, 40m, "MGM", 0.444444, "LTR", "06");
			AssertInvoiceLine(invoice2Line2, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "EA", 20m, "KG", "SG", "12345000000012", 4, 50m, "MGM", 0.555555, "LTR", "04");
			AssertInvoiceLine(invoice2Line3, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "EA", 5m, "KG", "US", "12345000000012", 3, 50m, "MGM", 0.555555, "LTR", "03");
			AssertInvoiceLine(invoice1Line3, Helper.Part2.OP_PartNum, Helper.Part2.PK, 800m, 40m, "NO", 60m, "KG", "IT", "12345000000012", 5, 40m, "MGM", 0.444444, "LTR", "05");
			AssertEquals("invoice1.JZ_InvoiceAmount", 2075m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", 800m, invoice2.JZ_InvoiceAmount);
		}

		public void TestUpdateOutwardLinesWithInventoryDetailsForCAD()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whsWarehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "R");
			Helper.Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 1, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 2, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part, 3, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 1, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 2, true);
			Helper.Importer.PartAttributeManager.SetProductToUseAttribute(Helper.Part2, 3, true);
			whsWarehouse.WW_WarehouseName = Helper.Warehouse.MainAddress.OA_Address1;
			whsWarehouse.WW_OA_WarehouseAddress = Helper.Warehouse.MainAddress.PK;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
			Factory.Save();

			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive3 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive4 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive5 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceive6 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive1.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive2.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-3);
			whsReceive3.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive4.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-15);
			whsReceive5.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive6.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive1.WD_ExternalReference = "01";
			whsReceive2.WD_ExternalReference = "02";
			whsReceive3.WD_ExternalReference = "03";
			whsReceive4.WD_ExternalReference = "04";
			whsReceive5.WD_ExternalReference = "05";
			whsReceive6.WD_ExternalReference = "06";
			var locationPK = Helper.WhsHelper.FindLocation(whsWarehouse.PK, "R").PK;
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 150m, 20m, "KG", 100m, "MGM", 1.11111m, "LTR", "AU", ZDecimal.Zero, "",
				"TreatmentCode=01", "12345000000012", (ZShort)1);
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive2.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-2", ZDateTime.Today.AddMonths(-3));
			var whsReceiveLine2CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1500m, 120m, "KG", 100m, "MGM", 1.11111m, "LTR", "NZ", ZDecimal.Zero, "",
				"TreatmentCode=02", "12345000000012", (ZShort)2);
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive3.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-3", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine3CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 100m, 10m, "KG", 100m, "MGM", 1.11111m, "LTR", "US", ZDecimal.Zero, "",
				"TreatmentCode=03", "12345000000012", (ZShort)3);
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive4.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "EA", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-4", ZDateTime.Today.AddDays(-15));
			var whsReceiveLine4CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 300m, 40m, "KG", 100m, "MGM", 1.11111m, "LTR", "SG", ZDecimal.Zero, "",
				"TreatmentCode=04", "12345000000012", (ZShort)4);
			var whsReceiveLine5 = Helper.GetNewWhsReceiveLine(whsReceive5.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-5", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine5CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 2000m, 150m, "KG", 100m, "MGM", 1.11111m, "LTR", "IT", ZDecimal.Zero, "",
				"TreatmentCode=05", "12345000000012", (ZShort)5);
			whsReceiveLine5.WE_BondedEntryKey = "12345000000012-5";
			var whsReceiveLine6 = Helper.GetNewWhsReceiveLine(whsReceive6.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "12345000000012-6", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine6CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 3000m, 200m, "KG", 100m, "MGM", 1.11111m, "LTR", "DE", ZDecimal.Zero, "",
				"TreatmentCode=06", "12345000000012", (ZShort)6);
			var whsInventory = whsReceiveLine1.Inventory;
			whsReceiveLine1.WE_WL = locationPK;
			whsReceiveLine2.WE_WL = locationPK;
			whsReceiveLine3.WE_WL = locationPK;
			whsReceiveLine4.WE_WL = locationPK;
			whsReceiveLine5.WE_WL = locationPK;
			whsReceiveLine6.WE_WL = locationPK;
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsReceive3.FinaliseDocketWithoutUserConfirmation();
			whsReceive4.FinaliseDocketWithoutUserConfirmation();
			whsReceive5.FinaliseDocketWithoutUserConfirmation();
			whsReceive6.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.WhsHelper.CreateWhsAdjustment(Helper.Importer.PK, whsWarehouse.PK, "AD1", null);
			adjustment[WhsDocketSchema.WD_DocketSubType] = "CUS";
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -40m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-2");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-3");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-4");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-5");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "12345000000012-6");
			Helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_TreatmentCode = ZString.Empty;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.CA_TreatmentCode = ZString.Empty;
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			invoice1Line1.JI_PreviousEntryNumber = "12345000000012";
			invoice1Line1.JI_PreviousEntryLineNumber = 4;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, ZDecimal.Zero, ZDecimal.Zero, "UNT", ZDecimal.Zero, "KG", ZString.Empty, "12345000000012", 4, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			Factory.ClearCachedValue<CACClassHeader>(string.Format("CACClassHeader_{0}_{1}_{2}", ZDateTime.Today.ToShortDateString(), "1234567890", false));
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 120m, 4m, "EA", 4m, "NMB", "SG", "12345000000012", 4, 40m, "MGM", 0.444444m, "LTR", "04");
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 120m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_InvoiceQuantity = 5m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "EA", 5m, "NMB", "SG", "12345000000012", 4, 50m, "MGM", 0.555555, "LTR", "04");
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 150m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PreviousEntryNumber = "";
			invoice1Line1.JI_PreviousEntryLineNumber = 0;
			invoice2Line1.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "EA", 5m, "NMB", "US", "12345000000012", 3, 50m, "MGM", 0.555555, "LTR", "03");
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", "12345000000012", 2, 40m, "MGM", 0.444444, "LTR", "02");
			AssertEquals("invoice1.JZ_InvoiceAmount", 50m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", 600m, invoice2.JZ_InvoiceAmount);

			invoice1.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice2.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.JI_InvoiceQuantity = 5m;
			invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_PartNo = Helper.Part.OP_PartNum;
			invoice2Line2.JI_PartAttrib1 = "PATT1";
			invoice2Line2.JI_PartAttrib2 = "PATT2";
			invoice2Line2.JI_PartAttrib3 = "PATT3";
			invoice2Line2.JI_CustomsUnitQty = "KG";
			invoice2Line2.JI_InvoiceQuantity = 5m;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice1Line2.JI_PartAttrib1 = "PATT1";
			invoice1Line2.JI_PartAttrib2 = "PATT2";
			invoice1Line2.JI_PartAttrib3 = "PATT3";
			invoice1Line2.JI_CustomsUnitQty = "KG";
			invoice1Line2.JI_InvoiceQuantity = 40m;
			var invoice2Line3 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line3.JI_PartNo = Helper.Part.OP_PartNum;
			invoice2Line3.JI_PartAttrib1 = "PATT1";
			invoice2Line3.JI_PartAttrib2 = "PATT2";
			invoice2Line3.JI_PartAttrib3 = "PATT3";
			invoice2Line3.JI_CustomsUnitQty = "KG";
			invoice2Line3.JI_InvoiceQuantity = 5m;
			invoice2Line3.JI_PreviousEntryNumber = "12345000000012";
			invoice2Line3.JI_PreviousEntryLineNumber = 3;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice1Line3.JI_PartAttrib1 = "PATT1";
			invoice1Line3.JI_PartAttrib2 = "PATT2";
			invoice1Line3.JI_PartAttrib3 = "PATT3";
			invoice1Line3.JI_CustomsUnitQty = "KG";
			invoice1Line3.JI_InvoiceQuantity = 40m;
			invoice1Line3.JI_PreviousEntryNumber = "12345000000012";
			invoice1Line3.JI_PreviousEntryLineNumber = 5;
			invoiceLines.Clear();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			invoiceLines.Add(invoice1Line2);
			invoiceLines.Add(invoice2Line2);
			invoiceLines.Add(invoice1Line3);
			invoiceLines.Add(invoice2Line3);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 75m, 5m, "EA", 10m, "KG", "AU", "12345000000012", 1, 50m, "MGM", 0.555555, "LTR", "01");
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", "12345000000012", 2, 40m, "MGM", 0.444444, "LTR", "02");
			AssertInvoiceLine(invoice1Line2, Helper.Part2.OP_PartNum, Helper.Part2.PK, 1200m, 40m, "NO", 80m, "KG", "DE", "12345000000012", 6, 40m, "MGM", 0.444444, "LTR", "06");
			AssertInvoiceLine(invoice2Line2, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "EA", 20m, "KG", "SG", "12345000000012", 4, 50m, "MGM", 0.555555, "LTR", "04");
			AssertInvoiceLine(invoice2Line3, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "EA", 5m, "KG", "US", "12345000000012", 3, 50m, "MGM", 0.555555, "LTR", "03");
			AssertInvoiceLine(invoice1Line3, Helper.Part2.OP_PartNum, Helper.Part2.PK, 800m, 40m, "NO", 60m, "KG", "IT", "12345000000012", 5, 40m, "MGM", 0.444444, "LTR", "05");
			AssertEquals("invoice1.JZ_InvoiceAmount", 2075m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", 800m, invoice2.JZ_InvoiceAmount);
		}

		public void TestCalculateLinePriceFromAMMVPerUnit()
		{
			#region SetUp
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PART1";
			product1.OP_StockKeepingUnit = "CS";
			product1.RelatedOrganisations.AddOwner(Helper.Importer);
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CCA_AMMVPerUnit = 2m;

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, product1, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			var header = new InventorySelectionHeader(Declaration);
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			#endregion

			CombineAssertions(() =>
			{
				AssertEquals(1, header.SelectionLines.Count);
				var line = header.SelectionLines[0];
				line.US_ProductQtyToDraw = 80m;
				AssertEquals(0, Declaration.Invoices.Count);
				AssertEquals(0, Declaration.InvoiceLines.Count);
				header.ImportInventories();
				AssertEquals(1, Declaration.Invoices.Count);
				AssertEquals(1, Declaration.InvoiceLines.Count);
				var invoiceLine = Declaration.InvoiceLines[0];
				AssertEquals(1040m, invoiceLine.JI_LinePrice);
			});
		}

		public void TestCalculateLinePriceFromAMMVPercentage()
		{
			#region SetUp
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PART1";
			product1.OP_StockKeepingUnit = "CS";
			product1.RelatedOrganisations.AddOwner(Helper.Importer);
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CCA_AMMVPercentage = 20m;

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, product1, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			var header = new InventorySelectionHeader(Declaration);
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			#endregion

			CombineAssertions(() =>
			{
				AssertEquals(1, header.SelectionLines.Count);
				var line = header.SelectionLines[0];
				line.US_ProductQtyToDraw = 80m;
				AssertEquals(0, Declaration.Invoices.Count);
				AssertEquals(0, Declaration.InvoiceLines.Count);
				header.ImportInventories();
				AssertEquals(1, Declaration.Invoices.Count);
				AssertEquals(1, Declaration.InvoiceLines.Count);
				var invoiceLine = Declaration.InvoiceLines[0];
				AssertEquals(1000m, invoiceLine.JI_LinePrice);
			});
		}

		#region Implementation

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZString partNo, ZGuid partPK, ZDecimal linePrice, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal customsQuantity, ZString customsUnitQty,
			ZString countryOfOrigin, ZString ptn, ZInt ptln, ZDecimal qty2, ZString qty2UM, ZDecimal qty3, ZString qty3UM, ZString treatmentCode)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_PartNo", partNo, invoiceLine.JI_PartNo);
				AssertEquals("JI_OP", partPK, invoiceLine.JI_OP);
				AssertEquals("JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
				AssertEquals("JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_CustomsQuantity", customsQuantity, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", customsUnitQty, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CountryOfOrigin", countryOfOrigin, invoiceLine.JI_CountryOfOrigin);
				AssertEquals("JI_PreviousEntryNumber", ptn, invoiceLine.JI_PreviousEntryNumber);
				AssertEquals("JI_PreviousEntryLineNumber", ptln, invoiceLine.JI_PreviousEntryLineNumber);
				AssertEquals("JI_CustomsSecondQuantity", qty2, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals("JI_CustomsSecondUnitQty", qty2UM, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("JI_CustomsThirdQuantity", qty3, invoiceLine.JI_CustomsThirdQuantity);
				AssertEquals("JI_CustomsThirdUnitQty", qty3UM, invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("CA_TreatmentCode", treatmentCode, invoiceLine.CA_TreatmentCode);
			});
		}

		WhsDataTestHelper Helper
		{
			get { return helper ?? (helper = new WhsDataTestHelper(Factory)); }
		}

		WhsDataTestHelper helper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InventorySelectionHeader(Factory.New<JobDeclaration>());
		}

		InventorySelectionHeader Header
		{
			get { return header ?? (header = new InventorySelectionHeader(Declaration)); }
		}
		InventorySelectionHeader header;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
					declaration.JE_OH_Importer = Helper.Importer.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
					declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.DefaultGeneralRateOfDuty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DecimalEffectiveDate { NewValue = 20, PreviousValue = 20, EffectiveDate = ZDateTime.Today.AddDays(-1) });

			var classificationNumber = "1234567890";
			var cusUQ = "NMB";

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, classificationNumber, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Tariff needs UQ to default CustomsUQ");
			universalHelper.CreateTariffUOM(tariff1, "CU1", cusUQ);

			var classHeader = CACClassHeader.Load(Factory, ZDateTime.Today, classificationNumber);
			if (classHeader == null)
			{
				classHeader = Factory.New<CACClassHeader>();
				classHeader.ZA_ClassificationNumber = classificationNumber;
				classHeader.ZA_EffectiveDate = ZDateTime.Now.AddYears(-1);
				classHeader.ZA_ExpiryDate = ZDateTime.Now.AddYears(1);
				classHeader.ZA_AreaCode = "900";
				classHeader.ZA_StatisticalUOMCode = cusUQ;
			}

			var classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = ZDateTime.Now.AddYears(-1);
			classRate.ZB_ExpiryDate = ZDateTime.Now.AddYears(1);
			classRate.ZB_UnitOfMeasure = "NMG";
			classRate.ZB_FreeInd = true;

			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.General;
			var rateLine = rate.RateLines.AddNew();
			rateLine.ZR_DutyRateMax = 0.7;
			rateLine.ZR_DutyRateMin = 0;
			rateLine.ZR_DutyRateRegular = 0;
			rateLine.ZR_DutyRateType = RateTypes.Codes.AdValorem;

			var pivot = Helper.Part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";
			pivot.CI_OH = Helper.Importer.PK;
			pivot.CI_TariffNum = "1234567890";

			pivot.Attributes1.AddNew().BG_AttributeValue1 = "PATT1";
			pivot.Attributes2.AddNew().BG_AttributeValue1 = "PATT2";
			pivot.Attributes3.AddNew().BG_AttributeValue1 = "PATT3";

			Factory.Save();
		}

		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }
			public bool IsBondedWhsQuantityRequiredForBondedWarehouseReturns { get; set; }

			protected override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;

			protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse => IsBondedWhsQuantityRequiredForBondedWarehouseReturns;
		}

		#endregion
	}
}
