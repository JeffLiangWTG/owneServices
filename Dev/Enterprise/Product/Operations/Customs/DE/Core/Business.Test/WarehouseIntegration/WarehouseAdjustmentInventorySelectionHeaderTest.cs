using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(WarehouseAdjustmentInventorySelectionHeader))]
	public sealed class WarehouseAdjustmentInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInvoiceLineData() => CombineAssertions(() =>
		{
			PrepareSelectionDataAndImport();

			AssertEquals(100m, InvoiceLine.JI_BondedWhsQuantity);
			AssertEquals("EN00123", InvoiceLine.JI_PreviousEntryNumber);
			AssertEquals((ZShort)2, InvoiceLine.JI_PreviousEntryLineNumber);
			AssertEquals(helper.Part.OP_PartNum, InvoiceLine.JI_PartNo);
			AssertEquals(ZString.Empty, InvoiceLine.JI_FormattedProcedure);
			AssertEquals(ZString.Empty, InvoiceLine.OutwardMRN);
			AssertEquals(0, InvoiceLine.Charges.Count);
		});

		protected override BusinessObject GetNewBusinessObject() => new WarehouseAdjustmentInventorySelectionHeader(Factory.New<JobDeclaration>());

		void PrepareSelectionDataAndImport()
		{
			helper = new WhsDataTestHelper(Factory);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse;
			entryInstruction.CEI_OA_Warehouse = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			header = new WarehouseAdjustmentInventorySelectionHeader(declaration);

			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			var whsReceiveLine1 = helper.GetNewWhsReceiveLine(whsReceive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-2", ZDateTime.Today.AddMonths(-1));
			whsReceiveLine1.Inventory.WI_AllocationKey = "D04C-3892-4353-A9FE";
			var whsReceiveLine1CustomsData = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)2);
			AddWarehouseCustomsAttributeAddInfo(whsReceiveLine1CustomsData, "CCT", "*Amount=240*ChargeType=ADD*Currency=CNY*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=Y*IsStatisticalValueApplicable=Y");
			var whsInventory = whsReceiveLine1.Inventory;

			var newSelectedLine = new WhsInventoryWrapper(whsInventory, header);
			newSelectedLine.QuantityToDraw = 100m;
			header.SelectedLines.Add(newSelectedLine);

			Factory.Save();

			header.ImportInventories();
		}

		void AddWarehouseCustomsAttributeAddInfo(IWhsBondedWarehouseAttribute customsData, string type, string addInfoData)
		{
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_ParentTableCode = "WB";
			addInfo.B7_ParentID = customsData.PK;
			addInfo.B7_Type = type;
			addInfo.B7_AddInfoData = addInfoData;
		}

		JobDeclaration declaration;
		WarehouseAdjustmentInventorySelectionHeader header;
		WhsDataTestHelper helper;
		JobComInvoiceLine InvoiceLine => declaration.InvoiceLines[0];
	}
}
