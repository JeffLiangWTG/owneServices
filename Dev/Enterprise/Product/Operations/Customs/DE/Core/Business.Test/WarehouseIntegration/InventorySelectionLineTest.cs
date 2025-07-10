using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InventorySelectionLine))]
	sealed class InventorySelectionLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUS_DeclarantsReference()
		{
			const string customerReference = "Ref123";

			PrepareInventoryWithCustomerReference(customerReference);

			AssertEquals("Precondition", true, Header.IsGroupByInventory);
			AssertEquals("Precondition", 1, Header.SelectionLines.Count);
			AssertEquals(customerReference, Header.SelectionLines[0].US_DeclarantsReference);

			Header.IsGroupByCarton = true;
			AssertNullOrEmpty(Header.SelectionLines[0].US_DeclarantsReference);
		}

		void PrepareInventoryWithCustomerReference(ZString customerReference)
		{
			var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
			receive.WD_CustomerReference = customerReference;
			var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", string.Empty, "ENT1234", 1);
			attr.WB_InwardProcedure = "1140";
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });
		}

		public void TestUS_SerialNumber()
		{
			const string serialNumber = "SN1";

			PrepareInventoryWithSerialNumber(serialNumber);

			AssertEquals(true, Header.IsGroupByInventory);
			AssertEquals(1, Header.SelectionLines.Count);
			AssertEquals(serialNumber, Header.SelectionLines[0].US_SerialNumber);

			Header.IsGroupByCarton = true;
			AssertEquals(serialNumber, Header.SelectionLines[0].US_SerialNumber);

			Header.IsGroupByProduct = true;
			AssertEquals(serialNumber, Header.SelectionLines[0].US_SerialNumber);
		}

		void PrepareInventoryWithSerialNumber(ZString serialNumber)
		{
			var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
			var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 1m, 1m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, serialNumber, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1m, 1m, "KG", ZString.Empty, 1m, "NO", string.Empty, "ENT1234", 1);
			attr.WB_InwardProcedure = "1140";
			Factory.Save();

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			receiveLine.Inventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			Header.UpdateSelectionLinesDetails(new[] { receiveLine.Inventory });
		}

		public void TestUS_CustomsDeadline()
		{
			helper = new WhsDataTestHelper(Factory);

			var whsReceive1 = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var whsReceive2 = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer2.PK);

			var whsInventory1 = helper.GetNewReceiveInventory(whsReceive1, helper.Part, ZString.Empty, 1m, 1m, 1m);
			var whsInventory2 = helper.GetNewReceiveInventory(whsReceive2, helper.Part, ZString.Empty, 1m, 1m, 1m);

			var customsDeadline1 = new ZDateTime(new ZDateTime(2025, 1, 28));
			var customsDeadline2 = new ZDateTime(new ZDateTime(2025, 2, 10));
			var attr1 = helper.GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 1m, 1m, "KG", ZString.Empty, 1m, "NO", string.Empty, "ENT1234", 1);
			var attr2 = helper.GetNewWhsBondedWarehouseAttribute(whsInventory2.PK, 1m, 1m, "KG", ZString.Empty, 1m, "NO", string.Empty, "ENT1234", 1);

			attr1.WB_CustomsDeadline = customsDeadline1.Date;
			attr2.WB_CustomsDeadline = customsDeadline2.Date;

			Factory.Save();

			header = new InventorySelectionHeaderForDeclarationCreation(createDeclarationBizObj)
			{
				IsGroupByInventory = true,
			};
			var line = Header.SelectionLines.AddNew();
			line.UpdateSelectionLinesDetails(new WhsInventoryWrapper(whsInventory1, header), new WhsInventoryWrapper(whsInventory2, header));
			AssertEquals(2, line.InventoryWrappers.Length);
			AssertEquals(customsDeadline1, line.US_CustomsDeadline);
		}

		protected override BusinessObject GetNewBusinessObject() => Header.SelectionLines.AddNew();

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		CreateDeclarationBizObj CreateDeclarationBizObj => createDeclarationBizObj ?? (createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: true));
		CreateDeclarationBizObj createDeclarationBizObj;

		InventorySelectionHeaderForDeclarationCreation Header => header ?? (header = new InventorySelectionHeaderForDeclarationCreation(CreateDeclarationBizObj));
		InventorySelectionHeaderForDeclarationCreation header;
	}
}
