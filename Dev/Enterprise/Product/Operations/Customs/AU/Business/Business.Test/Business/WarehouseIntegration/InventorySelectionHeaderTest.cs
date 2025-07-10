using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPair()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();

			var declarationMock = Factory.New<JobDeclarationForTest>();
			declarationMock.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declarationMock.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			var declaration = declarationMock;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.AddInfo.ZA_WRN = "123";
			invoiceLine1.AddInfo.ZA_WRL = 0;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WRL must be supplied if the WRN has been specified"));

			invoiceLine1.AddInfo.ZA_WRN = "";
			invoiceLine1.AddInfo.ZA_WRL = 1;

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WRN must be supplied if the WRL has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombination()
		{
			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.New<JobDeclarationForTest>();
			declarationMock.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declarationMock.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			var declaration = declarationMock;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.AddInfo.ZA_WRN = "ENT3333";
			invoiceLine1.AddInfo.ZA_WRL = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WRN, WRL and Product Code combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.AddInfo.ZA_WRN = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "WRN, WRL and Product Code combination cannot be matched in the Warehouse Inventory"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKey()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.New<JobDeclarationForTest>();
			declarationMock.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declarationMock.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			var declaration = declarationMock;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.AddInfo.ZA_WRN = "ENT3333";
			invoiceLine1.AddInfo.ZA_WRL = 3;

			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WRN, WRL combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.AddInfo.ZA_WRN = "ENT3243";
			invoiceLine1.AddInfo.ZA_WRL = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "WRN, WRL combination cannot be matched in the Warehouse Inventory"));
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
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.AddInfo.ZA_WRL = 0;
			invoiceLine1.AddInfo.ZA_WRN = "";
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

		public void TestSelectInventory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "12332542", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "NO");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				Helper.Classification.CC_TariffNum = "12332542";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var header = new InventorySelectionHeader(declaration);

				Helper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

				var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
				var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "EN00123-1");
				Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "",
					string.Format("{0}=CSC3*{1}=3*{2}=NO*{3}=SG", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
					"EN00123", (ZShort)1, 150m, Core.Constants.CurrencyCodes.NewZealand);
				Factory.Save();

				Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive.FinaliseDocketWithoutUserConfirmation();
				whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
				Factory.Save();

				header.IsGroupByInventory = true;
				AssertEquals(0, header.SelectionLines.Count);
				header.UpdateSelectionLinesDetails(new[] { whsInventory });
				AssertEquals(1, header.SelectionLines.Count);
				var line = header.SelectionLines[0];
				line.US_ProductQtyToDraw = 800m;
				header.ImportInventories();
				AssertEquals(Helper.Warehouse.MainAddress, declaration.WarehouseAddress);
				AssertEquals(1, declaration.Invoices.Count);
				var invoice = declaration.Invoices[0];
				AssertEquals("JZ_InvoiceAmount", 12000m, invoice.JZ_InvoiceAmount);
				AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Australia, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals("JZ_IncoTerm", "FOB", invoice.JZ_IncoTerm);
				AssertEquals(1, declaration.InvoiceLines.Count);
				var invoiceLine = declaration.InvoiceLines[0];
				AssertInvoiceLine(invoiceLine, Helper.Part.OP_PartNum, Helper.Part.PK, 12000m, 800m, "PK", 80m, "KG", "SG", whsInventory.WI_WE_InDocketLine, "CSC3", "EN00123", 1, "120.00NZD", 80m, "KG");
			}
		}

		public void TestSelectInventory_AUCClass()
		{
			var tariff = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			tariff.SC_TariffClassificationNumber = "12332542";
			tariff.SC_StatisticalClassificationCode = "";
			tariff.SC_StartDate = ZDateTime.BrettsBirthday;
			tariff.SC_EndDate = ZDateTime.MaxSmallDateTime;
			tariff.SC_QuantityUnit = "KG";
			tariff.SC_SecondQuantityUnit = "NO";
			Helper.Classification.CC_TariffNum = tariff.SC_TariffClassificationNumber;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var header = new InventorySelectionHeader(declaration);

			Helper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "",
				string.Format("{0}=CSC3*{1}=3*{2}=NO*{3}=SG", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)1, 150m, Core.Constants.CurrencyCodes.NewZealand);
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, header.SelectionLines.Count);
			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			header.ImportInventories();
			AssertEquals(Helper.Warehouse.MainAddress, declaration.WarehouseAddress);
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals("JZ_InvoiceAmount", 12000m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Australia, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("JZ_IncoTerm", "FOB", invoice.JZ_IncoTerm);
			AssertEquals(1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertInvoiceLine(invoiceLine, Helper.Part.OP_PartNum, Helper.Part.PK, 12000m, 800m, "PK", 80m, "KG", "SG", whsInventory.WI_WE_InDocketLine, "CSC3", "EN00123", 1, "120.00NZD", 80m, "KG");
		}

		public void TestUpdateOutwardLinesWithInventoryDetailsMissingCustomsAttribute()
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
			whsWarehouse.WW_IsBondedWarehouse = true;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
			Factory.Save();

			var locationPK = helper.FindLocation(whsWarehouse.PK, "R").PK;
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			whsReceive1.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive1.WD_ExternalReference = "01";
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 150m, 20m, "KG", "AU", ZDecimal.Zero, "",
				string.Format("{0}=CSC1*{1}=5*{2}=NO*{3}=AU", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)1, 50m, "AUD");
			var whsInventory = whsReceiveLine1.Inventory;
			whsReceiveLine1.WE_WL = locationPK;
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			((BusinessObject)whsReceiveLine1CustomsData).Delete();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();

			var invoiceLines = new List<BaseJobComInvoiceLine>();
			invoiceLines.Add(invoice1Line1);
			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.AddInfo.ZA_WRN = "EN00123";
			invoice1Line1.AddInfo.ZA_WRL = 1;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.AddInfo.ZA_ISS = 30m;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, ZDecimal.Zero, 10m, "NO", ZDecimal.Zero, "KG", ZString.Empty, whsReceiveLine1.PK, ZString.Empty, "EN00123", 1, ZString.Empty, ZDecimal.Zero, "UNT", ZString.Empty, iss: 30m);
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
			whsWarehouse.WW_IsBondedWarehouse = true;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
			Factory.Save();

			var locationPK = helper.FindLocation(whsWarehouse.PK, "R").PK;
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
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 150m, 20m, "KG", "AU", ZDecimal.Zero, "",
				string.Format("{0}=CSC1*{1}=5*{2}=NO*{3}=AU", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)1, 50m, "AUD");
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive2.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-2", ZDateTime.Today.AddMonths(-3));
			var whsReceiveLine2CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1500m, 120m, "KG", "NZ", ZDecimal.Zero, "",
				string.Format("{0}=CSC2*{1}=30*{2}=NO*{3}=AU", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)2, 200m, "NZD");
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive3.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-3", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine3CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 100m, 10m, "KG", "US", ZDecimal.Zero, "",
				string.Format("{0}=CSC3*{1}=30*{2}=NO*{3}=US", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)3, 150m, "USD");
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive4.PK, Helper.Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-4", ZDateTime.Today.AddDays(-15));
			var whsReceiveLine4CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 300m, 40m, "KG", "SG", ZDecimal.Zero, "",
				string.Format("{0}=CSC4*{1}=30*{2}=AU", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)4, 100m, "SGD");
			var whsReceiveLine5 = Helper.GetNewWhsReceiveLine(whsReceive5.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-5", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine5CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 2000m, 150m, "KG", "IT", ZDecimal.Zero, "",
				string.Format("{0}=CSC5*{1}=40*{2}=NO*{3}=AU*{4}=200", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3), AUAddInfo.Schema.ZA_QT2.Substring(3)),
				"EN00123", (ZShort)5, 100m, "EUR");
			whsReceiveLine5.WE_BondedEntryKey = "EN00123-5";
			var whsReceiveLine6 = Helper.GetNewWhsReceiveLine(whsReceive6.PK, Helper.Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-6", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine6CustomsData = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 3000m, 200m, "KG", "DE", ZDecimal.Zero, "",
				string.Format("{0}=CSC6*{1}=50*{2}=NO*{3}=AU", AUAddInfo.Schema.ZA_CSC.Substring(3), AUAddInfo.Schema.ZA_WRQ.Substring(3), AUAddInfo.Schema.ZA_WRU.Substring(3), AUAddInfo.Schema.ZA_ORG.Substring(3)),
				"EN00123", (ZShort)6, 100m, "EUR");
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
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-1");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -40m, "R", "PATT1", "PATT2", "PATT3", "EN00123-2");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-3");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-4");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "EN00123-5");
			Helper.WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Helper.Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "EN00123-6");
			Helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			invoice1Line1.AddInfo.ZA_ISS = 30m;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, iss: 30m);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PartNo = Helper.Part.OP_PartNum;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			invoice1Line1.AddInfo.ZA_WRN = "EN00123";
			invoice1Line1.AddInfo.ZA_WRL = 4;
			invoice1Line1.AddInfo.ZA_ISS = 30m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, ZDecimal.Zero, ZDecimal.Zero, "UNT", ZDecimal.Zero, "KG", ZString.Empty, ZGuid.Empty, ZString.Empty, "EN00123", 4, ZString.Empty, ZDecimal.Zero, "UNT", ZString.Empty, iss: 30m);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, 40m, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", ZDecimal.Zero, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 120m, 4m, "NO", 16m, "KG", "SG", whsReceiveLine4.PK, "CSC4", "EN00123", 4, "40.00SGD", 12m, ZString.Empty, "N", iss: 30m);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, 40m, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 120m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.JI_InvoiceQuantity = 5m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "NO", 20m, "KG", "SG", whsReceiveLine4.PK, "CSC4", "EN00123", 4, "50.00SGD", 15m, ZString.Empty, "N", iss: 30m);
			AssertInvoiceLine(invoice2Line1, ZString.Empty, ZGuid.Empty, ZDecimal.Zero, 40m, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, ZInt.Zero, ZString.Empty, 40m, ZString.Empty, ZString.Empty);
			AssertEquals("invoice1.JZ_InvoiceAmount", 150m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", ZDecimal.Zero, invoice2.JZ_InvoiceAmount);

			invoice1Line1.AddInfo.ZA_WRN = "";
			invoice1Line1.AddInfo.ZA_WRL = 0;
			invoice2Line1.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "NO", 5m, "KG", "US", whsReceiveLine3.PK, "CSC3", "EN00123", 3, "75.00USD", 5m, "KG", "N", iss: 30m);
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", whsReceiveLine2.PK, "CSC2", "EN00123", 2, "80.00NZD", 48m, "KG", "N", 0m);
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
			invoice2Line3.AddInfo.ZA_WRN = "EN00123";
			invoice2Line3.AddInfo.ZA_WRL = 3;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_PartNo = Helper.Part2.OP_PartNum;
			invoice1Line3.JI_PartAttrib1 = "PATT1";
			invoice1Line3.JI_PartAttrib2 = "PATT2";
			invoice1Line3.JI_PartAttrib3 = "PATT3";
			invoice1Line3.JI_CustomsUnitQty = "KG";
			invoice1Line3.JI_InvoiceQuantity = 40m;
			invoice1Line3.AddInfo.ZA_WRN = "EN00123";
			invoice1Line3.AddInfo.ZA_WRL = 5;
			invoiceLines.Clear();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			invoiceLines.Add(invoice1Line2);
			invoiceLines.Add(invoice2Line2);
			invoiceLines.Add(invoice1Line3);
			invoiceLines.Add(invoice2Line3);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, Helper.Part.OP_PartNum, Helper.Part.PK, 75m, 5m, "NO", 10m, "KG", "AU", whsReceiveLine1.PK, "CSC1", "EN00123", 1, "25.00AUD", 10m, "KG", "N");
			AssertInvoiceLine(invoice1Line2, Helper.Part2.OP_PartNum, Helper.Part2.PK, 1200m, 40m, "NO", 80m, "KG", "DE", whsReceiveLine6.PK, "CSC6", "EN00123", 6, "40.00EUR", 80m, "KG", "N");
			AssertInvoiceLine(invoice2Line1, Helper.Part2.OP_PartNum, Helper.Part2.PK, 600m, 40m, "NO", 48m, "KG", "NZ", whsReceiveLine2.PK, "CSC2", "EN00123", 2, "80.00NZD", 48m, "KG", "N");
			AssertInvoiceLine(invoice2Line2, Helper.Part.OP_PartNum, Helper.Part.PK, 150m, 5m, "NO", 20m, "KG", "SG", whsReceiveLine4.PK, "CSC4", "EN00123", 4, "50.00SGD", 15m, ZString.Empty, "N");
			AssertInvoiceLine(invoice2Line3, Helper.Part.OP_PartNum, Helper.Part.PK, 50m, 5m, "NO", 5m, "KG", "US", whsReceiveLine3.PK, "CSC3", "EN00123", 3, "75.00USD", 5m, "KG", "N");
			AssertInvoiceLine(invoice1Line3, Helper.Part2.OP_PartNum, Helper.Part2.PK, 800m, 40m, "NO", 60m, "KG", "IT", whsReceiveLine5.PK, "CSC5", "EN00123", 5, "40.00EUR", 60m, "KG", "N", 80m);
			AssertEquals("invoice1.JZ_InvoiceAmount", 2075m, invoice1.JZ_InvoiceAmount);
			AssertEquals("invoice2.JZ_InvoiceAmount", 800m, invoice2.JZ_InvoiceAmount);
		}

		protected override BusinessObject GetNewBusinessObject() => new InventorySelectionHeader(Factory.New<JobDeclaration>());

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZString partNo, ZGuid partPK, ZDecimal linePrice, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal customsQuantity, ZString customsUnitQty, ZString countryOfOrigin, ZGuid bondedWarehouseLineKey,
			ZString csc, ZString wrn, ZInt wrl, ZString tilv, ZDecimal wrq, ZString wru, string isPackToBondForLine = "N", decimal qt2 = 0m, decimal iss = 0m)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_PartNo", partNo, invoiceLine.JI_PartNo);
				AssertEquals("JI_OP", partPK, invoiceLine.JI_OP);
				AssertEquals("JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
				AssertEquals("JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);  // ""
				AssertEquals("JI_CustomsQuantity", customsQuantity, invoiceLine.JI_CustomsQuantity);    // 0
				AssertEquals("JI_CustomsUnitQty", customsUnitQty, invoiceLine.JI_CustomsUnitQty);   // ""
				AssertEquals("JI_CountryOfOrigin", countryOfOrigin, invoiceLine.JI_CountryOfOrigin);
				AssertEquals("JI_BondedWarehouseLineKey", bondedWarehouseLineKey, invoiceLine.JI_BondedWarehouseLineKey);
				AssertEquals("UseBondedWarehouseAutomation", true, invoiceLine.UseBondedWarehouseAutomation);

				var addInfo = invoiceLine.AddInfo;
				AssertEquals("ZA_CSC", csc, invoiceLine.AddInfo.ZA_CSC);
				AssertEquals("ZA_WRN", wrn, addInfo.ZA_WRN);
				AssertEquals("ZA_WRL", wrl, addInfo.ZA_WRL);
				AssertEquals("ZA_TILV", tilv, addInfo.ZA_TILV);
				AssertEquals("ZA_IsPackToBondForLine_Hidden", isPackToBondForLine, addInfo.ZA_IsPackToBondForLine_Hidden);
				AssertEquals("ZA_WRQ", wrq, addInfo.ZA_WRQ);    // 2.4
				AssertEquals("ZA_WRU", wru, addInfo.ZA_WRU);    // NO
				AssertEquals("ZA_QT2", qt2, addInfo.ZA_QT2);
				AssertEquals("ZA_ISS", iss, addInfo.ZA_ISS);
			});
		}

		WhsDataTestHelper helper;
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }

			public bool IsBondedWhsQuantityRequiredForBondedWarehouseReturns { get; set; }

			protected override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;

			protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse => IsBondedWhsQuantityRequiredForBondedWarehouseReturns;
		}
	}
}
