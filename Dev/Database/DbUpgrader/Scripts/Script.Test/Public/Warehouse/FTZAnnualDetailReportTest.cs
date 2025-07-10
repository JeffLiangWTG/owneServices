using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(FTZAnnualDetailReport))]
	class FTZAnnualDetailReportTest : DbCreateScriptTest
	{
		public void TestFTZAnnualDetailReport()
		{
			var companyPK = TestDataCreator.CreateCompany("US#", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "US#", "USPHL");
			var warehouseOrgPK = CreateWarehouseOrganization("WFC", "WISFTZUSCHI", "USCHI");
			var warehouseAddressPK = TestDataCreator.CreateAddress(warehouseOrgPK, "Address", "1051 E WOODFIELD RD");
			var importerPK = TestDataCreator.CreateOrganisation("TESTIMP", "TEST IMPORTER", "USPHL");
			var importer = OrgHeader.ShallowLoadFromDB(TestConnection, importerPK);
			CreateOrgMiscServ(importerPK, "TEST NAME");
			var product = new OrgSupplierPart("12345") { OP_Desc = "TEST", OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productPK = product.PK;
			new OrgPartRelation(importer, product, "OWN").InsertAndReturnObject(TestConnection);
			var warehouse = CreateWarehouse("WFT", "WISETECH FTZ", branchPK, warehouseAddressPK, "FTZ");
			var whsAreaPK = CreateWhsArea("FTZ AREA", "BON", warehouse.PK, true, true);
			var whsRowPK = CreateWhsRow("INCFTZ", 10, 2, 1, warehouse.PK, 2);
			var whsLocationTypePK = CreateWhsDockDoorLocationType("BND", "BOND PICKUP");
			var whsLocation = CreateWhsLocation(whsAreaPK, whsRowPK, whsAreaPK, whsLocationTypePK, "NOR");

			var recipient1 = CreateWhsDocket("INW", "CUS", new DateTime(2021, 09, 24), new DateTime(2021, 09, 24), warehouse.PK, importerPK, "W00000001", new DateTime(2021, 09, 24), null, null);
			var inwardLine1 = CreateWhsDocketLine(recipient1, "4011231A121TST00265-1", "NO", productPK, 1000m, 1000m, "AVL", "AVL", new DateTime(2021, 09, 24), whsLocation.PK);
			CreateWhsBondedWarehouseAttribute(inwardLine1.PK, "4011231A121TST00265", 1, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			var inwardLine2 = CreateWhsDocketLine(recipient1, "4011231A121TST00265-1", "NO", productPK, 500m, 500m, "AVL", "AVL", new DateTime(2021, 09, 24), whsLocation.PK);
			CreateWhsBondedWarehouseAttribute(inwardLine2.PK, "4011231A121TST00265", 1, 500m, 500m, "KG", "BXX00221475", "HK", "3920790500", 2500m, "P", new DateTime(2021, 10, 01));

			var whsPick = CreateWhsPick("P00002021", warehouse, whsLocation);

			var orderDocket = CreateWhsDocket("ORD", "CPS", new DateTime(2021, 10, 15), new DateTime(2021, 10, 15), warehouse.PK, importerPK, "W00000002", null, new DateTime(2021, 10, 15), whsPick.PK);
			var outwardLine1 = CreateWhsDocketLine(orderDocket, "4011231A121TST00265-1", "NO", productPK, 100m, 0m, "", "", null, null);
			CreateWhsBondedWarehouseAttribute(outwardLine1.PK, "XJ5-73013573", 0, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			var ftzAnnualLineDatas = GetFTZAnnualDetailLineDatas(warehouse.PK, new DateTime(2021, 09, 01), new DateTime(2021, 10, 31));
			AssertEquals("There are 3 FTZ Annual Details lines", 3, ftzAnnualLineDatas.Count);

			var firstInwardLine = ftzAnnualLineDatas.Select(kvp => kvp.Value).Single(l => l.TransactionType == "INWARDS" && l.ZoneStatus == "NPF");
			var secondInwardLine = ftzAnnualLineDatas.Select(kvp => kvp.Value).Single(l => l.TransactionType == "INWARDS" && l.ZoneStatus == "PF");
			var outwardLine = ftzAnnualLineDatas.Select(kvp => kvp.Value).Single(l => l.TransactionType == "OUTWARDS");

			AssertFTZAnnualDetailLineData("FirstInwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 5000m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 09, 24),
				RelativeInvoiceQty = 1000m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 1000m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 7500m,
				GTRelativeCustomsQty = 1500m,
				ReasonCode = ""
			}, firstInwardLine);
			AssertFTZAnnualDetailLineData("SecondInwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "PF",
				PartAttrib1 = "",
				RelativeValueForDuty = 2500m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 10, 01),
				RelativeInvoiceQty = 500m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 500m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 7500m,
				GTRelativeCustomsQty = 1500m,
				ReasonCode = ""
			}, secondInwardLine);
			AssertFTZAnnualDetailLineData("FirstOutwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "XJ5-73013573",
				OutwardsEntryLineNo = 0,
				TransactionType = "OUTWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 0m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 10, 15),
				RelativeInvoiceQty = 0m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 0m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 7500m,
				GTRelativeCustomsQty = 1500m,
				ReasonCode = ""
			}, outwardLine);

			var ftzAnnualLineDatas2 = GetFTZAnnualDetailLineDatas(warehouse.PK, new DateTime(2021, 09, 30), new DateTime(2021, 10, 01));
			AssertEquals("There are 2 FTZ Annual Details lines", 2, ftzAnnualLineDatas2.Count);

			var balanceLine = ftzAnnualLineDatas2.Select(kvp => kvp.Value).Single(l => l.TransactionType == "BALANCE");
			var inwardLineSecondLoad = ftzAnnualLineDatas2.Select(kvp => kvp.Value).Single(l => l.TransactionType == "INWARDS");

			AssertFTZAnnualDetailLineData("FirstBalanceLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "BALANCE",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 5000m,
				Origin = "HK",
				EntryDate = null,
				RelativeInvoiceQty = 1000m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 1000m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 7500m,
				GTRelativeCustomsQty = 1500m,
				ReasonCode = ""
			}, balanceLine);
			AssertFTZAnnualDetailLineData("FirstInward", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "PF",
				PartAttrib1 = "",
				RelativeValueForDuty = 2500m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 10, 01),
				RelativeInvoiceQty = 500m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 500m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 7500m,
				GTRelativeCustomsQty = 1500m,
				ReasonCode = ""
			}, inwardLineSecondLoad);
		}

		public void TestFTZAnnualDetailReport_WithOutboundTransfer()
		{
			var companyPK = TestDataCreator.CreateCompany("US#", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "US#", "USPHL");
			var warehouseOrgPK = CreateWarehouseOrganization("WFC", "WISFTZUSCHI", "USCHI");
			var warehouseAddressPK = TestDataCreator.CreateAddress(warehouseOrgPK, "Address", "1051 E WOODFIELD RD");
			var importerPK = TestDataCreator.CreateOrganisation("TESTIMP", "TEST IMPORTER", "USPHL");
			var importer = OrgHeader.ShallowLoadFromDB(TestConnection, importerPK);
			CreateOrgMiscServ(importerPK, "TEST NAME");
			var product = new OrgSupplierPart("12345") { OP_Desc = "TEST", OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productPK = product.PK;
			new OrgPartRelation(importer, product, "OWN").InsertAndReturnObject(TestConnection);
			var warehouse = CreateWarehouse("WFT", "WISETECH FTZ", branchPK, warehouseAddressPK, "FTZ");
			var whsAreaPK = CreateWhsArea("FTZ AREA", "BON", warehouse.PK, true, true);
			var whsRowPK = CreateWhsRow("INCFTZ", 10, 2, 1, warehouse.PK, 2);
			var whsLocationTypePK = CreateWhsDockDoorLocationType("BND", "BOND PICKUP");
			var whsLocation = CreateWhsLocation(whsAreaPK, whsRowPK, whsAreaPK, whsLocationTypePK, "NOR");

			var recipient1 = CreateWhsDocket("INW", "CUS", new DateTime(2021, 09, 24), new DateTime(2021, 09, 24), warehouse.PK, importerPK, "W00000001", new DateTime(2021, 09, 24), null, null);
			var inwardLine1 = CreateWhsDocketLine(recipient1, "4011231A121TST00265-1", "NO", productPK, 1000m, 1000m, "AVL", "AVL", new DateTime(2021, 09, 24), whsLocation.PK);
			CreateWhsBondedWarehouseAttribute(inwardLine1.PK, "4011231A121TST00265", 1, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			var whsPick = CreateWhsPick("P00002021", warehouse, whsLocation);

			var orderDocket = CreateWhsDocket("ORD", "CPS", new DateTime(2021, 10, 15), new DateTime(2021, 10, 15), warehouse.PK, importerPK, "W00000002", null, new DateTime(2021, 10, 15), whsPick.PK);
			var outwardLine1 = CreateWhsDocketLine(orderDocket, "4011231A121TST00265-1", "NO", productPK, 100m, 0m, "", "", null, null);
			CreateWhsBondedWarehouseAttribute(outwardLine1.PK, "XJ5-73013573", 0, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine DISABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine DISABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced_Insert");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine DISABLE TRIGGER TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsPickLine DISABLE TRIGGER TG_WhsPickLine_StockOnHandIsBalanced");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsPickLine DISABLE TRIGGER TG_WhsPickLine_TransactionAndPickedQtyIsCorrect");
			CreateWhsPickLinesAndOutboundTransfer(warehouse.PK, warehouseOrgPK, whsPick, inwardLine1, outwardLine1, 100m, whsLocation.PK);
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine ENABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine ENABLE TRIGGER TG_WhsDocketLine_StockOnHandIsBalanced_Insert");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsDocketLine ENABLE TRIGGER TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsPickLine ENABLE TRIGGER TG_WhsPickLine_StockOnHandIsBalanced");
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.WhsPickLine ENABLE TRIGGER TG_WhsPickLine_TransactionAndPickedQtyIsCorrect");
			TestConnection.ExecuteNonQuery(@"DECLARE @PKs dbo.TVP_uniqueidentifier;
INSERT INTO @PKs
SELECT DISTINCT WE_WD FROM WhsDocketLine
EXEC WhsCheckStockOnHandIsBalanced @PKs;
EXEC WhsCheckTransactionAndPickQtyIsCorrect_V3 @PKs;");

			var ftzAnnualLineDatas = GetFTZAnnualDetailLineDatas(warehouse.PK, new DateTime(2021, 09, 01), new DateTime(2021, 10, 31));
			AssertEquals("There are 2 FTZ Annual Details lines", 2, ftzAnnualLineDatas.Count);

			var inwardLine = ftzAnnualLineDatas.Select(kvp => kvp.Value).Single(l => l.TransactionType == "INWARDS");
			var outwardLine = ftzAnnualLineDatas.Select(kvp => kvp.Value).Single(l => l.TransactionType == "OUTWARDS");

			AssertFTZAnnualDetailLineData("FirstInwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 5000m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 09, 24),
				RelativeInvoiceQty = 1000m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 1000m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 4500m,
				GTRelativeCustomsQty = 900m,
				ReasonCode = ""
			}, inwardLine);
			AssertFTZAnnualDetailLineData("OutwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "XJ5-73013573",
				OutwardsEntryLineNo = 0,
				TransactionType = "OUTWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = -500m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 10, 15),
				RelativeInvoiceQty = -100m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = -100m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 4500m,
				GTRelativeCustomsQty = 900m,
				ReasonCode = ""
			}, outwardLine);
		}

		public void TestFTZAnnualDetailReport_Adjusment()
		{
			var companyPK = TestDataCreator.CreateCompany("US#", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "US#", "USPHL");
			var warehouseOrgPK = CreateWarehouseOrganization("WFC", "WISFTZUSCHI", "USCHI");
			var warehouseAddressPK = TestDataCreator.CreateAddress(warehouseOrgPK, "Address", "1051 E WOODFIELD RD");
			var importerPK = TestDataCreator.CreateOrganisation("TESTIMP", "TEST IMPORTER", "USPHL");
			var importer = OrgHeader.ShallowLoadFromDB(TestConnection, importerPK);
			CreateOrgMiscServ(importerPK, "TEST NAME");
			var product = new OrgSupplierPart("12345") { OP_Desc = "TEST", OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productPK = product.PK;
			new OrgPartRelation(importer, product, "OWN").InsertAndReturnObject(TestConnection);
			var warehouse = CreateWarehouse("WFT", "WISETECH FTZ", branchPK, warehouseAddressPK, "FTZ");
			var whsAreaPK = CreateWhsArea("FTZ AREA", "BON", warehouse.PK, true, true);
			var whsRowPK = CreateWhsRow("INCFTZ", 10, 2, 1, warehouse.PK, 2);
			var whsLocationTypePK = CreateWhsDockDoorLocationType("BND", "BOND PICKUP");
			var whsLocation = CreateWhsLocation(whsAreaPK, whsRowPK, whsAreaPK, whsLocationTypePK, "NOR");

			var adjusment = CreateWhsDocket("ADJ", "CUS", new DateTime(2021, 09, 15), new DateTime(2021, 09, 15), warehouse.PK, importerPK, "W00000001", new DateTime(2021, 09, 15), null, null);
			var adjusmentLine = CreateWhsDocketLine(adjusment, "4011231A121TST00266-2", "NO", productPK, 100m, 100m, "AVL", "AVL", new DateTime(2021, 09, 15), whsLocation.PK, "AMD");
			CreateWhsBondedWarehouseAttribute(adjusmentLine.PK, "4011231A121TST00266", 2, 100m, 100m, "KG", "BXX00221475", "HK", "3920790500", 4000m, "N", null);

			var ftzAnnualLineDatas = GetFTZAnnualDetailLineDatas(warehouse.PK, new DateTime(2021, 09, 01), new DateTime(2021, 10, 31));
			AssertEquals("There are 1 FTZ Annual Details lines", 1, ftzAnnualLineDatas.Count);
			AssertFTZAnnualDetailLineData("FirstInwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00266",
				InwardsEntryLineNo = 2,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 4000m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 09, 15),
				RelativeInvoiceQty = 100m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 100m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 4000m,
				GTRelativeCustomsQty = 100m,
				ReasonCode = "AMD"
			}, ftzAnnualLineDatas[0]);
		}

		public void TestFTZAnnualDetailReport_ExcludeDynamicWorkOrder()
		{
			TestFTZAnnualDetailReport_ExcludeWorkOrderCore("DWO");
		}

		public void TestFTZAnnualDetailReport_ExcludeWorkOrder()
		{
			TestFTZAnnualDetailReport_ExcludeWorkOrderCore("WOR");
		}

		void TestFTZAnnualDetailReport_ExcludeWorkOrderCore(string orderType)
		{
			var companyPK = TestDataCreator.CreateCompany("US#", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "US#", "USPHL");
			var warehouseOrgPK = CreateWarehouseOrganization("WFC", "WISFTZUSCHI", "USCHI");
			var warehouseAddressPK = TestDataCreator.CreateAddress(warehouseOrgPK, "Address", "1051 E WOODFIELD RD");
			var importerPK = TestDataCreator.CreateOrganisation("TESTIMP", "TEST IMPORTER", "USPHL");
			var importer = OrgHeader.ShallowLoadFromDB(TestConnection, importerPK);
			CreateOrgMiscServ(importerPK, "TEST NAME");
			var product = new OrgSupplierPart("12345") { OP_Desc = "TEST", OP_StockKeepingUnit = "UNT" }.InsertAndReturnObject(TestConnection);
			var productPK = product.PK;
			new OrgPartRelation(importer, product, "OWN").InsertAndReturnObject(TestConnection);
			var warehouse = CreateWarehouse("WFT", "WISETECH FTZ", branchPK, warehouseAddressPK, "FTZ");
			var whsAreaPK = CreateWhsArea("FTZ AREA", "BON", warehouse.PK, true, true);
			var whsRowPK = CreateWhsRow("INCFTZ", 10, 2, 1, warehouse.PK, 2);
			var whsLocationTypePK = CreateWhsDockDoorLocationType("BND", "BOND PICKUP");
			var whsLocation = CreateWhsLocation(whsAreaPK, whsRowPK, whsAreaPK, whsLocationTypePK, "NOR");

			var recipient1 = CreateWhsDocket("INW", "CUS", new DateTime(2021, 09, 24), new DateTime(2021, 09, 24), warehouse.PK, importerPK, "W00000001", new DateTime(2021, 09, 24), null, null);
			var inwardLine1 = CreateWhsDocketLine(recipient1, "4011231A121TST00265-1", "NO", productPK, 1000m, 1000m, "AVL", "AVL", new DateTime(2021, 09, 24), whsLocation.PK);
			CreateWhsBondedWarehouseAttribute(inwardLine1.PK, "4011231A121TST00265", 1, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			var whsPick = CreateWhsPick("P00002021", warehouse, whsLocation);

			var dynamicWorkOrderDocket = CreateWhsDocket(orderType, "ASS", new DateTime(2021, 10, 15), new DateTime(2021, 10, 15), warehouse.PK, importerPK, "W00000002", null, new DateTime(2021, 10, 15), whsPick.PK);
			var outwardLine1 = CreateWhsDocketLine(dynamicWorkOrderDocket, "", "NO", productPK, 100m, 0m, "", "", null, null);
			CreateWhsBondedWarehouseAttribute(outwardLine1.PK, "XJ5-73013573", 0, 1000m, 1000m, "KG", "BXX00221475", "HK", "3920790500", 5000m, "N", null);

			var ftzAnnualLineDatas = GetFTZAnnualDetailLineDatas(warehouse.PK, new DateTime(2021, 09, 01), new DateTime(2021, 10, 31));
			AssertEquals("There are 1 FTZ Annual Details lines", 1, ftzAnnualLineDatas.Count);
			AssertFTZAnnualDetailLineData("FirstInwardLine", new FTZAnnualDetailLineData
			{
				InwardsEntryNo = "4011231A121TST00265",
				InwardsEntryLineNo = 1,
				OutwardsEntryNo = "",
				OutwardsEntryLineNo = 0,
				TransactionType = "INWARDS",
				DeclarationReference = "BXX00221475",
				ZoneStatus = "NPF",
				PartAttrib1 = "",
				RelativeValueForDuty = 5000m,
				Origin = "HK",
				EntryDate = new DateTime(2021, 09, 24),
				RelativeInvoiceQty = 1000m,
				InvoiceUQ = "NO",
				RelativeCustomsQty = 1000m,
				CustomsUQ = "KG",
				Tariff = "3920790500",
				ProdCode = "12345",
				ProdDesc = "TEST",
				WarehouseName = "WISETECH FTZ",
				ClientCode = "TESTIMP",
				GTRelativeValueForDuty = 5000m,
				GTRelativeCustomsQty = 1000m,
				ReasonCode = ""
			}, ftzAnnualLineDatas[0]);
		}

		Dictionary<int, FTZAnnualDetailLineData> GetFTZAnnualDetailLineDatas(Guid warehousePK, DateTime dateFrom, DateTime dateTo)
		{
			var result = new Dictionary<int, FTZAnnualDetailLineData>();
			var queryScript = @"
SELECT InwardsEntryNo, InwardsEntryLineNo, OutwardsEntryNo, OutwardsEntryLineNo, TransactionType, DeclarationReference, ZoneStatus, PartAttrib1, RelativeValueForDuty,
Origin, EntryDate, RelativeInvoiceQty, InvoiceUQ, RelativeCustomsQty, CustomsUQ, Tariff, ProdCode, ProdDesc, WarehouseName, ClientCode, GTRelativeValueForDuty, GTRelativeCustomsQty, ReasonCode
FROM dbo.FTZAnnualDetailReport(@warehousePK, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(queryScript))
			{
				command.AddParameter("@warehousePK", SqlDbType.UniqueIdentifier, warehousePK);
				command.AddParameter("@dateFrom", SqlDbType.DateTime, dateFrom);
				command.AddParameter("@dateTo", SqlDbType.DateTime, dateTo);
				using (var reader = command.ExecuteReader())
				{
					var lineIndex = 0;
					while (reader.Read())
					{
						result.Add(lineIndex++, new FTZAnnualDetailLineData
						{
							InwardsEntryNo = reader["InwardsEntryNo"] == DBNull.Value ? string.Empty : reader.GetString(0),
							InwardsEntryLineNo = reader["InwardsEntryLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(1)),
							OutwardsEntryNo = reader["OutwardsEntryNo"] == DBNull.Value ? string.Empty : reader.GetString(2),
							OutwardsEntryLineNo = reader["OutwardsEntryLineNo"] == DBNull.Value ? 0 : Convert.ToInt32(reader.GetValue(3)),
							TransactionType = reader["TransactionType"] == DBNull.Value ? string.Empty : reader.GetString(4),
							DeclarationReference = reader["DeclarationReference"] == DBNull.Value ? string.Empty : reader.GetString(5),
							ZoneStatus = reader["ZoneStatus"] == DBNull.Value ? string.Empty : reader.GetString(6),
							PartAttrib1 = reader["PartAttrib1"] == DBNull.Value ? string.Empty : reader.GetString(7),
							RelativeValueForDuty = reader["RelativeValueForDuty"] == DBNull.Value ? 0m : reader.GetDecimal(8),
							Origin = reader["Origin"] == DBNull.Value ? string.Empty : reader.GetString(9),
							EntryDate = reader["EntryDate"] == DBNull.Value ? null : reader.GetDateTime(10),
							RelativeInvoiceQty = reader["RelativeInvoiceQty"] == DBNull.Value ? 0m : reader.GetDecimal(11),
							InvoiceUQ = reader["InvoiceUQ"] == DBNull.Value ? string.Empty : reader.GetString(12),
							RelativeCustomsQty = reader["RelativeCustomsQty"] == DBNull.Value ? 0m : reader.GetDecimal(13),
							CustomsUQ = reader["CustomsUQ"] == DBNull.Value ? string.Empty : reader.GetString(14),
							Tariff = reader["Tariff"] == DBNull.Value ? string.Empty : reader.GetString(15),
							ProdCode = reader["ProdCode"] == DBNull.Value ? string.Empty : reader.GetString(16),
							ProdDesc = reader["ProdDesc"] == DBNull.Value ? string.Empty : reader.GetString(17),
							WarehouseName = reader["WarehouseName"] == DBNull.Value ? string.Empty : reader.GetString(18),
							ClientCode = reader["ClientCode"] == DBNull.Value ? string.Empty : reader.GetString(19),
							GTRelativeValueForDuty = reader["GTRelativeValueForDuty"] == DBNull.Value ? 0m : reader.GetDecimal(20),
							GTRelativeCustomsQty = reader["GTRelativeCustomsQty"] == DBNull.Value ? 0m : reader.GetDecimal(21),
							ReasonCode = reader["ReasonCode"] == DBNull.Value ? string.Empty : reader.GetString(22)
						});
					}
				}
			}
			return result;
		}

		struct FTZAnnualDetailLineData
		{
			public string InwardsEntryNo;
			public int InwardsEntryLineNo;
			public string OutwardsEntryNo;
			public int OutwardsEntryLineNo;
			public string TransactionType;
			public string DeclarationReference;
			public string ZoneStatus;
			public string PartAttrib1;
			public decimal RelativeValueForDuty;
			public string Origin;
			public DateTime? EntryDate;
			public decimal RelativeInvoiceQty;
			public string InvoiceUQ;
			public decimal RelativeCustomsQty;
			public string CustomsUQ;
			public string Tariff;
			public string ProdCode;
			public string ProdDesc;
			public string WarehouseName;
			public string ClientCode;
			public decimal GTRelativeValueForDuty;
			public decimal GTRelativeCustomsQty;
			public string ReasonCode;
		}

		void AssertFTZAnnualDetailLineData(string message, FTZAnnualDetailLineData current, FTZAnnualDetailLineData expected)
		{
			AssertEquals($"{message}.InwardsEntryNo", current.InwardsEntryNo, expected.InwardsEntryNo);
			AssertEquals($"{message}.InwardsEntryLineNo", current.InwardsEntryLineNo, expected.InwardsEntryLineNo);
			AssertEquals($"{message}.OutwardsEntryNo", current.OutwardsEntryNo, expected.OutwardsEntryNo);
			AssertEquals($"{message}.OutwardsEntryLineNo", current.OutwardsEntryLineNo, expected.OutwardsEntryLineNo);
			AssertEquals($"{message}.TransactionType", current.TransactionType, expected.TransactionType);
			AssertEquals($"{message}.DeclarationReference", current.DeclarationReference, expected.DeclarationReference);
			AssertEquals($"{message}.ZoneStatus", current.ZoneStatus, expected.ZoneStatus);
			AssertEquals($"{message}.PartAttrib1", current.PartAttrib1, expected.PartAttrib1);
			AssertEquals($"{message}.RelativeValueForDuty", current.RelativeValueForDuty, expected.RelativeValueForDuty);
			AssertEquals($"{message}.Origin", current.Origin, expected.Origin);
			AssertEquals($"{message}.EntryDate", current.EntryDate, expected.EntryDate);
			AssertEquals($"{message}.RelativeInvoiceQty", current.RelativeInvoiceQty, expected.RelativeInvoiceQty);
			AssertEquals($"{message}.InvoiceUQ", current.InvoiceUQ, expected.InvoiceUQ);
			AssertEquals($"{message}.RelativeCustomsQty", current.RelativeCustomsQty, expected.RelativeCustomsQty);
			AssertEquals($"{message}.CustomsUQ", current.CustomsUQ, expected.CustomsUQ);
			AssertEquals($"{message}.Tariff", current.Tariff, expected.Tariff);
			AssertEquals($"{message}.ProdCode", current.ProdCode, expected.ProdCode);
			AssertEquals($"{message}.ProdDesc", current.ProdDesc, expected.ProdDesc);
			AssertEquals($"{message}.WarehouseName", current.WarehouseName, expected.WarehouseName);
			AssertEquals($"{message}.ClientCode", current.ClientCode, expected.ClientCode);
			AssertEquals($"{message}.GTRelativeValueForDuty", current.GTRelativeValueForDuty, expected.GTRelativeValueForDuty);
			AssertEquals($"{message}.GTRelativeCustomsQty", current.GTRelativeCustomsQty, expected.GTRelativeCustomsQty);
			AssertEquals($"{message}.ReasonCode", current.ReasonCode, expected.ReasonCode);
		}

		#region Create Test Data

		Guid CreateWarehouseOrganization(string code, string name, string closestPort)
		{
			var organisationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort, OH_IsWarehouseClient)
VALUES (@organisationPK, @code, @name, @closestPort, 1)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, code);
				command.AddParameter("@name", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, name);
				command.AddParameter("@closestPort", SqlDbType.VarChar, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength, closestPort);
				command.ExecuteNonQuery();
			}
			return organisationPK;
		}

		WhsWarehouse CreateWarehouse(string warehouseCode, string warehouseName, Guid branchPK, Guid warehouseAddressPK, string warehouseType)
		{
			var warehouse = new WhsWarehouse(warehouseCode, branchPK, warehouseAddressPK, new Guid("16C9FD62-730A-42ED-A20E-699606FFF360"))
			{
				WW_WarehouseName = warehouseName
			}.WithDockDoor(TestConnection);

			return warehouse;
		}

		Guid CreateWhsArea(string name, string areaType, Guid warehousePK, bool isPickingArea, bool isPutawayArea)
		{
			var area = new WhsArea(warehousePK, name)
			{
				WA_AreaType = areaType,
				WA_IsPickingArea = isPickingArea,
				WA_IsPutawayArea = isPutawayArea
			}.InsertAndReturnObject(TestConnection);

			return area.PK;
		}

		Guid CreateWhsRow(string name, int columns, int levels, int trays, Guid warehousePK, int pickPatchSequence)
		{
			var whsRowPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.WhsRow(WR_PK, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_WW_Whs, WR_PickPathSequence, WR_SystemCreateTimeUtc, WR_SystemCreateUser, WR_SystemLastEditTimeUtc, WR_SystemLastEditUser)
VALUES (@whsRowPK, @name, @columns, @levels, @trays, @warehousePK, @pickPathSequence, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@whsRowPK", SqlDbType.UniqueIdentifier, whsRowPK);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@columns", SqlDbType.Int, columns);
				command.AddParameter("@levels", SqlDbType.Int, levels);
				command.AddParameter("@trays", SqlDbType.Int, trays);
				command.AddParameter("@warehousePK", SqlDbType.UniqueIdentifier, warehousePK);
				command.AddParameter("@pickPathSequence", SqlDbType.Int, pickPatchSequence);
				command.ExecuteNonQuery();
			}

			return whsRowPK;
		}

		Guid CreateWhsDockDoorLocationType(string code, string description)
		{
			var locationTypeDDL = new WhsLocationType(code, "DDL") { WLT_Description = description }.InsertAndReturnObject(Db.Connection);
			return locationTypeDDL.PK;
		}

		WhsLocation CreateWhsLocation(Guid pickingAreaPK, Guid whsRowPK, Guid putawayAreaPK, Guid locationTypePK, string locationStatus)
		{
			return new WhsLocation(whsRowPK, pickingAreaPK, putawayAreaPK, locationTypePK)
			{
				WL_PutawayPathSequence = 7,
				WL_LocationStatus = locationStatus,
			}.InsertAndReturnObject(TestConnection);
		}

		void CreateOrgMiscServ(Guid orgPK, string partAttribute1Name)
		{
			var sql = @"
INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_IMPartAttrib1Name)
VALUES (@orgMiscServPK, @orgPK, @attribute1Name)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgMiscServPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@attribute1Name", SqlDbType.VarChar, partAttribute1Name);
				command.ExecuteNonQuery();
			}
		}

		WhsPick CreateWhsPick(string pickNo, WhsWarehouse warehouse, WhsLocation dockDoor)
		{
			return new WhsPick(warehouse, pickNo, "FIN") { WP_WL_DockDoor = dockDoor, WP_FinalizedDateUtc = new DateTime(2021, 09, 24), WP_GS_NKFinalizedBy = "A" }.InsertAndReturnObject(TestConnection);
		}

		void CreateWhsPickLinesAndOutboundTransfer(Guid warehousePK, Guid clientPK, WhsPick pick, WhsDocketLine inventoryLine, WhsDocketLine orderLine, decimal pickedUnit, Guid dockDoorLocationPK)
		{
			WhsDocketLine.UpdateWhere(inventoryLine.PK).Set(l => l.WE_StockOnHand, inventoryLine.WE_StockOnHand - pickedUnit).Post(TestConnection);
			var date = new DateTime(2021, 09, 24);
			var outboundTransfer = new WhsDocket(clientPK, warehousePK, "TFR", "TFR", "ENT", "TFR01")
			{
				WD_BookingDate = date,
				WD_ArrivalDate = date,
				WD_RequiredDate = date,
				WD_WP_ParentPickForTransfer = pick,
			}.InsertAndReturnObject(TestConnection);
			var outboundTransferLine = new WhsDocketLine(outboundTransfer, inventoryLine.WE_OP, inventoryLine.WE_TransactionQuantity, dockDoorLocationPK)
			{
				WE_AdjustmentArrivalDate = date,
				WE_CurrentInventoryStatus = "AVL",
				WE_FinalisedDate = date,
				WE_DocketLineStatus = "FIN",
				WE_WL_TransferFrom = inventoryLine.WE_WL,
				WE_PutawayTime = date,
				WE_GS_NKPutawayBy = "ABC",
			}.InsertAndReturnObject(TestConnection);
			var outboundPickLine = new WhsPickLine(inventoryLine, outboundTransferLine, pickedUnit)
			{
				WZ_PickedDateTime = date,
				WZ_GS_NKAssignedTo = "ABC"
			}.InsertAndReturnObject(TestConnection);
			var originalPickLine = new WhsPickLine(outboundTransferLine, orderLine, pickedUnit)
			{
				WZ_PickedDateTime = date,
				WZ_GS_NKAssignedTo = "ABC",
				WZ_WE_OriginalPickedInventoryLine = inventoryLine
			}.InsertAndReturnObject(TestConnection);
		}

		WhsDocket CreateWhsDocket(string docketType, string docketSubType, DateTime bookingDate, DateTime finalisedDate, Guid warehousePK, Guid clientPK, string docketID, DateTime? arrivalDate, DateTime? requiredDate, Guid? whsPickPK)
		{
			var status = docketType == "ORD" ? "DEP" : "FIN";
			return new WhsDocket(clientPK, warehousePK, docketType, docketSubType, status, docketID)
			{
				WD_BookingDate = bookingDate,
				WD_FinalisedDate = finalisedDate,
				WD_ArrivalDate = arrivalDate,
				WD_RequiredDate = requiredDate,
				WD_GS_NKFinalizedBy = "E",
				WD_WP = whsPickPK
			}.InsertAndReturnObject(TestConnection);
		}

		WhsDocketLine CreateWhsDocketLine(WhsDocket docket, string bondedEntryKey, string packType, Guid productPK, decimal transactionQty, decimal stockOnHand, string originalInventoryStatus,
			string currentInventoryStatus, DateTime? adjustmentArrivalDate, Guid? whsLocationPK, string reasonCode = null)
		{
			var docketLine = new WhsDocketLine(docket, productPK, transactionQty, whsLocationPK)
			{
				WE_BondedEntryKey = bondedEntryKey,
				WE_F3_NKPackType = packType,
				WE_StockOnHand = stockOnHand,
				WE_AdjustmentArrivalDate = adjustmentArrivalDate,
				WE_OriginalInventoryStatus = originalInventoryStatus,
				WE_CurrentInventoryStatus = currentInventoryStatus,
				WE_ReasonCode = reasonCode
			};
			if (docket.WD_FinalisedDate != null)
			{
				docketLine.WE_FinalisedDate = docket.WD_FinalisedDate;
				docketLine.WE_DocketLineStatus = docket.WD_DocketStatus;
			}
			docketLine.InsertAndReturnObject(TestConnection);

			return docketLine;
		}

		Guid CreateWhsBondedWarehouseAttribute(Guid docketLinePK, string entryKey, int entryLineNo, decimal bondedWhsQty, decimal customsQty, string customsUQ, string declarationReference, string countryOfOrigin, string tariff, decimal valueForDuty, string zoneStatus, DateTime? entryDate)
		{
			var bondedAttrib = new WhsBondedWarehouseAttribute(docketLinePK, "WE")
			{
				WB_BondedWhsQty = bondedWhsQty,
				WB_CustomsQty = customsQty,
				WB_CustomsUnitOfQty = customsUQ,
				WB_DeclarationReference = declarationReference,
				WB_EntryKey = entryKey,
				WB_EntryLineNo = (short)entryLineNo,
				WB_RN_NKCountryOfOrigin = countryOfOrigin,
				WB_Tariff = tariff,
				WB_ValueForDuty = valueForDuty,
				WB_ZoneStatus = zoneStatus,
				WB_EntryDate = entryDate
			}.InsertAndReturnObject(TestConnection);

			return bondedAttrib.PK;
		}

		#endregion
	}
}
