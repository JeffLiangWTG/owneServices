using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	public sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportInventories_PreservesQuantity()
		{
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			var whsInventory = helper.GetNewReceiveInventory(whsReceive, helper.Part, ZString.Empty, 1m, 1000m, 1000m, "NO", "", "", "", "", "EN00123-1");
			var attr = helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", ZString.Empty, ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			attr.WB_InwardProcedure = "1234";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Procedure = "56";
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { whsInventory });

			CombineAssertions("Before 'ImportInventories'", () =>
			{
				AssertEquals("SelectionLines count", 1, header.SelectionLines.Count);
				var line = header.SelectionLines[0];
				line.US_ProductQtyToDraw = 800m;
				AssertEquals("Invoices count", 0, declaration.Invoices.Count);
				AssertEquals("InvoiceLines count", 0, declaration.InvoiceLines.Count);
			});

			CombineAssertions("After 'ImportInventories'", () =>
			{
				header.ImportInventories();
				AssertEquals("WarehouseAddress", helper.Warehouse.MainAddress, declaration.WarehouseAddress);
				AssertEquals("Invoices count", 1, declaration.Invoices.Count);
				AssertEquals("InvoiceLines count", 1, declaration.InvoiceLines.Count);
				var invoiceLine = declaration.InvoiceLines[0];
				AssertEquals("JI_Procedure has been set", "5612", invoiceLine.JI_FormattedProcedure);
				AssertEquals("JI_BondedWhsUnitQty", "NO", invoiceLine.JI_BondedWhsUnitQty);
				AssertEquals("JI_BondedWhsQuantity", 800m, invoiceLine.JI_BondedWhsQuantity);
			});
		}

		[TestDate(2021, 02, 23)]
		public void TestFillInventoryDetails_NetPrice()
		{
			var invoiceLine = CreateInvoiceLine(declaration, "ENT1234");

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);

			receiveLine.WE_TransactionQuantity = 10m;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			receiveLineCustomsData.WB_ValueForDuty = 1000m;
			receiveLineCustomsData.WB_AddInfo = "*LineNetPrice=100*LinePriceCurrency=USD";

			Factory.Save();

			var inventorySelectionHeader = new ImportInventorySelectionHeader(declaration);
			var invoiceLines = new List<JobComInvoiceLine> { invoiceLine };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			AssertEquals(35.97m, invoiceLine.JI_NetPrice); // 5 / 10 * 100 * exchange rate (USD => EUR) = 35.97
		}

		public void TestJI_Weight()
		{
			// Arrange
			var invoiceLine = CreateInvoiceLine(declaration, PreviousEntryNumber);

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, PreviousEntryNumber, 1);

			invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_BondedWhsQuantity = 50;
			Factory.Save();

			//Act
			header.UpdateOutwardLinesWithInventoryDetails(new[] { invoiceLine });

			// Assert
			AssertEquals("Precondition", new ZDecimal(25), invoiceLine.JI_CustomsQuantity);
			AssertEquals(new ZDecimal(25), invoiceLine.JI_Weight);
		}

		public void TestGetFirstOrCreateInvoiceHeader_InvalidCurrency()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CHF";
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, BondedEntryKey, ZDateTime.Today.AddMonths(-1));
			var addInfo = "LinePrice=4000.5000*LinePriceCurrency=XXX";
			helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, PreviousEntryNumber, 1);
			Factory.Save();

			helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });

			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 50m;

			header.ImportInventories();

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];
			AssertEquals(invoiceHeader.PK, invoiceLine.InvoiceHeader.PK);
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

		protected override BusinessObject GetNewBusinessObject() => new InventorySelectionHeaderForTest(Factory.New<JobDeclaration>());

		protected override void SetUp()
		{
			base.SetUp();
			helper = new WhsDataTestHelper(Factory);
			declaration = CreateImportJobDeclaration();
			header = new InventorySelectionHeaderForTest(declaration);
		}
		JobDeclaration declaration;
		WhsDataTestHelper helper;
		InventorySelectionHeaderForTest header;
		const string PreviousEntryNumber = "ENT1234";
		const string BondedEntryKey = "ENT1234-1";

		JobDeclaration CreateImportJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			return declaration;
		}

		JobComInvoiceLine CreateInvoiceLine(JobDeclaration declaration, ZString previousEntryNumber)
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			return invoiceLine;
		}
	}

	sealed class InventorySelectionHeaderForTest : InventorySelectionHeader
	{
		public InventorySelectionHeaderForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override BaseJobComInvoiceHeader GetMatchingInvoiceHeaderAndPopulateData(InvoiceHeaderGroupingDefinitionProvider invoiceHeaderGroupingDefinitionProvider)
		{
			var invoiceHeader = Declaration.Invoices.FirstOrDefault(x =>
				x.JZ_RX_NKInvoice_Currency == invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency
				&& x.IncoTerm == invoiceHeaderGroupingDefinitionProvider.IncotermCode
				&& x.JZ_IncoTermPlace == invoiceHeaderGroupingDefinitionProvider.IncotermPlace
				&& x.JZ_ValuationCode == invoiceHeaderGroupingDefinitionProvider.TransNature)
					?? Declaration.Invoices.AddNew();

			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeaderGroupingDefinitionProvider.LinePriceCurrency;
			invoiceHeader.JZ_IncoTerm = invoiceHeaderGroupingDefinitionProvider.IncotermCode;
			invoiceHeader.JZ_IncoTermPlace = invoiceHeaderGroupingDefinitionProvider.IncotermPlace;
			invoiceHeader.JZ_ValuationCode = invoiceHeaderGroupingDefinitionProvider.TransNature;
			return invoiceHeader;
		}

		protected override void SetFormattedProcedure(JobComInvoiceLine invoiceLine, IWhsDocketLine receiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
			=> invoiceLine.JI_FormattedProcedure = invoiceLine.EntryInstruction?.CEI_Procedure + ((string)whsBondedWarehouseAttribute.WB_InwardProcedure).LeftOrNull(2);
	}
}
