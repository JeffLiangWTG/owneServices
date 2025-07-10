using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsOrderInventorySelectionHeader))]
	sealed class NctsOrderInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new NctsOrderInventorySelectionHeader(bill);

		public void TestFiltersForWhsOrderSelection()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, isVirtualWarehouse: true, "WH");
			Factory.Save();
			var selectionHeader = new NctsOrderInventorySelectionHeader(bill);
			var collection = selectionHeader.GetCollectionForWhsOrderSelection(Factory);
			var filters = ((IFilterBusinessObjectDefaultsProvider)collection).FilterBusinessObjectDefaults;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "Order Status:Property=DEP", $"Warehouse:Property={whsWarehouse.PK}" }, filters.Cast<FilterBusinessObjectDefault>().Select(x => $"{x.Key}={x.Value}"));
				AssertEquals(false, filters["Order Status:Property"].IsRemovable);
			});
		}

		public void TestAttemptedImportOrderWithMultiplePicksOnOrderLine()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, pick) = BondedWarehousingHelperTest.CreateOrderWithPick(helper, Factory, createMultiplePickLines: false);

			Factory.Save();
			var selectionHeader = new NctsOrderInventorySelectionHeader(bill);

			selectionHeader.ImportInventories(order);

			AssertEquals("", selectionHeader.ImportInventoriesResult);
			var docketLinePk2 = helper.WhsHelper.CreateWhsOrderLine(order.PK, helper.Part.PK, 5);
			Factory.Save();
			selectionHeader.ImportInventories(order);

			AssertEquals("You cannot import order REF3 with multiple picks on order line 2", selectionHeader.ImportInventoriesResult);
		}

		public void TestCreateGoodsItemForOrderLine() => CombineAssertions(() =>
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, line) = BondedWarehousingHelperTest.CreateOrderWithPick(helper, Factory, createMultiplePickLines: false);

			var pivot = helper.Part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = "LV";
			pivot.CI_ChildType = "IMP";
			pivot.CI_FormattedTariffNum = "100001";
			line.WE_LineNo = 5;
			Factory.Save();
			var selectionHeader = new NctsOrderInventorySelectionHeader(bill);

			selectionHeader.ImportInventories(order);
			Factory.Save();
			AssertEquals(1, bill.GoodsItems.Count);

			var goodsItem = bill.GoodsItems[0];

			AssertEquals("W00000003", goodsItem.BY_BondedWHSOrderNumber);
			AssertEquals((ZShort)5, goodsItem.BY_BondedWHSOrderLineNumber);
			AssertEquals(2m, goodsItem.BY_BondedWhsQuantity);
			AssertEquals("UNT", goodsItem.BY_BondedWhsUnitQty);
			AssertEquals("ENTRY1", goodsItem.BY_WarehouseEntryNumber);
			AssertEquals((ZShort)1, goodsItem.BY_WarehouseEntryLineNo);
			AssertEquals(helper.Part.PK, goodsItem.BY_OP_Part);
			AssertEquals("~~1 DESC", goodsItem.BY_Description);
			AssertEquals(50m, goodsItem.BY_GrossWeight);
			AssertEquals("KG", goodsItem.BY_GrossWeightUnit);
			AssertEquals(50m, goodsItem.BY_NetWeight);
			AssertEquals("KG", goodsItem.BY_NetWeightUnit);
			AssertEquals("1000.01", goodsItem.BY_FormattedHarmonisedTariff);
			AssertEquals(1000m, goodsItem.BY_MonetaryValue);
			AssertEquals("EUR", goodsItem.BY_RX_NKCurrency);
		});

		public void TestLinkHeaderToOrder()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, pick) = BondedWarehousingHelperTest.CreateOrderWithPick(helper, Factory, createMultiplePickLines: false);
			Factory.Save();
			var selectionHeader = new NctsOrderInventorySelectionHeader(bill);
			selectionHeader.ImportInventories(order);

			var docketJobPivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK);
			var docketJobPivot = header.Factory.LoadTop1<IWhsDocketJobPivot>(docketJobPivotQuery);
			AssertEquals(order.PK, docketJobPivot.WV_WD_Docket);
		}

		public void TestIsOutwardOrderImported()
		{
			var helper = new WhsDataTestHelper(Factory);
			var (order, pick) = BondedWarehousingHelperTest.CreateOrderWithPick(helper,Factory, createMultiplePickLines: false);
			Factory.Save();
			var selectionHeader = new NctsOrderInventorySelectionHeader(bill);
			selectionHeader.ImportInventories(order);

			AssertEquals(expected: true, bill.IsOutwardOrderImported);
		}

		protected override void SetUp()
		{
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			bill = header.Bills.AddNew();
		}

		NctsHeader header;
		NctsBill bill;
	}
}
