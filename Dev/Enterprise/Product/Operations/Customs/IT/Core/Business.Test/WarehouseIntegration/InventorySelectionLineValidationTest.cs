using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InventorySelectionLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckUS_ProductQtyToDraw_CustomsEntryKeyIsPendingResponse()
	{
		const string expectedMessage = "The Customs Entry Key is pending for a response from the customs system, it cannot be selected.";

		var whsWarehouse = dataTestHelper.GetNewWhsWarehouse(dataTestHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
		var whsInventory1 = SetUpWhsInventory(whsWarehouse, "RCV1", "<PENDINGCUSTOMSRESPONSE>-1");
		var whsInventory2 = SetUpWhsInventory(whsWarehouse, "RCV2", "22ITQXT04CE98155R2-2");
		var whsInventory3 = SetUpWhsInventory(whsWarehouse, "RCV3", "<PendingCustomsResponse>-1");
		Factory.Save();

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory1 });
		var inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertHasErrorContaining("When selected entry has pending response entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionLine.US_ProductQtyToDraw = 0;
		AssertNoErrorContaining("When the are no selected entries", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory2 });
		inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertNoErrorContaining("When selected entry has valid customs entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory3 });
		inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertHasErrorContaining("When selected entry has pending response entry key (case insensitive check)", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);
	}

	public void TestCheckUS_ProductQtyToDraw_CustomsEntryKeyIsNotValid()
	{
		const string expectedMessage = "The Customs Entry Key is not an MRN and does not start with '7', it cannot be selected.";

		var whsWarehouse = dataTestHelper.GetNewWhsWarehouse(dataTestHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
		var whsInventory1 = SetUpWhsInventory(whsWarehouse, "RCV1", "XYZ-1");
		var whsInventory2 = SetUpWhsInventory(whsWarehouse, "RCV2", "22ITQXT04CE98155R2-2");
		var whsInventory3 = SetUpWhsInventory(whsWarehouse, "RCV3", "7-95884-03052021-279100-2");
		var whsInventory4 = SetUpWhsInventory(whsWarehouse, "RCV4", "22AAQXT04CE98155R2-2");
		Factory.Save();

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory1 });
		var inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertHasErrorContaining("When selected entry has invalid entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionLine.US_ProductQtyToDraw = 0;
		AssertNoErrorContaining("When the are no selected entries", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory2 });
		inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertNoErrorContaining("When selected entry has valid MRN entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory4 });
		inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertHasErrorContaining("When selected entry has invalid MRN entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);

		inventorySelectionHeader.SelectedLines.RemoveAndDeleteAll();
		inventorySelectionHeader.UpdateSelectionLinesDetails(new[] { whsInventory3 });
		inventorySelectionLine = inventorySelectionHeader.SelectionLines[0];
		inventorySelectionLine.US_ProductQtyToDraw = 1;
		AssertNoErrorContaining("When selected entry has valid '7' (old SAD) entry key", inventorySelectionLine.US_ProductQtyToDrawInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dataTestHelper = new WhsDataTestHelper(Factory);
		var declaration = Factory.New<JobDeclaration>();
		inventorySelectionHeader = declaration.InventorySelectionHeader;
	}

	WhsDataTestHelper dataTestHelper;
	DeclarationInventorySelectionHeader inventorySelectionHeader;

	IWhsInventoryView SetUpWhsInventory(IWhsWarehouse whsWarehouse, ZString receiveReference, ZString customsEntryKey)
	{
		var whsReceive = dataTestHelper.GetNewWhsReceive(whsWarehouse.PK, dataTestHelper.Importer.PK, receiveReference, ZDateTimeOffset.Today.AddDays(-3));
		var whsInventory = dataTestHelper.GetNewReceiveInventory(whsReceive, dataTestHelper.Part, "", 1m, 900m, 900m, bondedEntryKey: customsEntryKey);
		dataTestHelper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
		whsReceive.FinaliseDocketWithoutUserConfirmation();
		return whsInventory;
	}
}
