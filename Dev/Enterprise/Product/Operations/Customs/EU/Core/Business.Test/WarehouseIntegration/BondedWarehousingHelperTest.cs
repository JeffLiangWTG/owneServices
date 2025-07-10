using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.EU.Business.Testing
{
	public sealed class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestFiltersForWhsOrderSelection()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, isVirtualWarehouse: true, "WH");
			Factory.Save();

			var collection = BondedWarehousingHelper.GetCollectionForWhsOrderSelection(Factory);
			var filters = ((IFilterBusinessObjectDefaultsProvider)collection).FilterBusinessObjectDefaults;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "Order Status:Property=DEP", $"Warehouse:Property={whsWarehouse.PK}" }, filters.Cast<FilterBusinessObjectDefault>().Select(x => $"{x.Key}={x.Value}"));
				AssertEquals(false, filters["Order Status:Property"].IsRemovable);
			});
		}

		public void TestGetPickLinesForOrderLines_SinglePickLine()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, line) = CreateOrderWithPick(helper, Factory, false);

			var picks = BondedWarehousingHelper.GetPickLinesForOrderLines(Factory, order).Single();
			var lines = picks.Value.ToArray();

			AssertEquals(line.PK, picks.Key.PK);
			AssertEquals(1, lines.Length);
			AssertEquals(2m, lines.First().WZ_Units);
		}

		public void TestGetPickLinesForOrderLines_MultiplePickLines()
		{
			var helper = new WhsDataTestHelper(Factory);

			var (order, line) = CreateOrderWithPick(helper, Factory, true);

			var picks = BondedWarehousingHelper.GetPickLinesForOrderLines(Factory, order).Single();
			var lines = picks.Value.ToArray();
			AssertEquals(2, lines.Length);
			AssertContainsExactElementsInAnyOrder(new[] { 1m, 2m }, lines.Select(l => l.WZ_Units.XmlSerializedValue));
		}

		public void TestGetReceiveLineForPickLine()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, line) = CreateOrderWithPick(helper, Factory, false);

			var pickLine = BondedWarehousingHelper.GetPickLinesForOrderLines(Factory, order).Values.Single().Single();

			var receiveLine = BondedWarehousingHelper.GetReceiveLineForPickLine(Factory, pickLine);

			AssertEquals(receiveLine.CustomsData.WB_EntryKey, "ENTRY1");
			AssertEquals(receiveLine.CustomsData.WB_EntryLineNo, (ZShort)1);
		}

		public static (IWhsOrder, IWhsDocketLine) CreateOrderWithPick(WhsDataTestHelper helper, BusinessObjectFactory factory, bool createMultiplePickLines)
		{
			helper.WhsHelper.SetUpBondedWarehouse(helper.WhsWarehouse.PK, helper.WhsWarehouse.WarehouseAddress.PK);

			var receivePk = helper.WhsHelper.CreateWhsReceive(helper.Importer.PK, helper.WhsWarehouse.PK, "REF1", null);
			var view = helper.WhsHelper.CreateWhsReceiveInventoryLine(receivePk, helper.Part.PK, 1.0m, 2.0m, 2.0m, ZString.Empty, ZString.Empty, ZDateTimeOffset.Today, "ENTRY1", ZString.Empty);
			var receive2Pk = helper.WhsHelper.CreateWhsReceive(helper.Importer.PK, helper.WhsWarehouse.PK, "REF2", null);
			var view2 = helper.WhsHelper.CreateWhsReceiveInventoryLine(receive2Pk, helper.Part.PK, 1.0m, 1.0m, 1.0m, ZString.Empty, ZString.Empty, ZDateTimeOffset.Today, "ENTRY1", ZString.Empty);

			var row = helper.WhsWarehouse.Rows.Cast<IWhsRow>().FirstOrDefault(r => r.Locations.ToArray().Cast<IWhsLocation>().Any(l => l.WLV_PickingAreaType == "BON"));
			var location = row.Locations.ToArray().Cast<IWhsLocation>().First(x => x.WLV_PickingAreaType == "BON");
			view.WI_WL = location.PK;

			var newRow = helper.WhsHelper.CreateRowAndGenerateLocations(helper.WhsWarehouse, "BOND2");
			var newLocation = (IWhsLocation)newRow.Locations[0];
			view2.WI_WL = newLocation.PK;

			factory.Save();

			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(receivePk);
			factory.Save();
			helper.WhsHelper.FinaliseDocketWithoutUserConfirmation(receive2Pk);
			factory.Save();

			var order = (IWhsOrder)helper.WhsHelper.CreateWhsOrder(helper.Importer.PK, helper.WhsWarehouse.PK, helper.Importer.PK, "REF3");
			order.WD_DocketSubType = "CUS";

			var orderQuantity = createMultiplePickLines ? 3.0m : 2.0m;
			var docketLinePk = helper.GetNewWhsOrderLine(order, helper.Part, orderQuantity);
			docketLinePk.CustomsData.WB_EntryKey = "ABC";
			docketLinePk.CustomsData.WB_EntryLineNo = 0;
			factory.Save();

			helper.GetNewWhsPick(new[] { order });

			var receiveLineCustomsData = helper.GetNewWhsBondedWarehouseAttribute(view.WI_WE_InDocketLine, 1000m, 50m, "KG", ZString.Empty, 1m, "NO", ZString.Empty, "ENTRY1", 1);

			var receiveLineCustomsData2 = helper.GetNewWhsBondedWarehouseAttribute(view2.WI_WE_InDocketLine, 1000m, 50m, "KG", ZString.Empty, 1m, "NO", ZString.Empty, "ENTRY1", 2);
			factory.Save();

			return (order, docketLinePk);
		}
	}
}
