using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

[TestedType(typeof(NctsInventorySelectionHeader))]
sealed class NctsInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestGetFilterDefaults()
	{
		var helper = new WhsDataTestHelper(Factory);
		var organisationPk = bill.Consignor.OrganisationPK.IsEmpty ? bill.Header.Consignor.OrganisationPK : bill.Consignor.OrganisationPK;

		if (organisationPk == ZGuid.Empty)
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			organisationPk = orgProxy?.PK ?? ZGuid.Empty;
		}

		var header1 = Factory.NewWithValidTestData<OrgHeader>();
		header1.OH_Code = "WH1";
		header1.OH_IsWarehouseClient = true;
		header1.OrganisationTypes = OrganisationTypes.WarehouseClient;
		var address = header1.Addresses.AddNew();
		address.OA_Address1 = "ADD1";
		address.AddAddressType(OrgAddressType.Office);
		bill.Header.MovementHeader.BM_OA_WarehouseAddress = address.PK;

		var whsWarehouse = helper.GetNewWhsWarehouse(bill.Header.MovementHeader.WarehouseAddress.PK, true, "WH");
		Factory.Save();
		var selectionHeader = (NctsInventorySelectionHeader)GetNewBusinessObject();
		var filters = selectionHeader.GetFilterDefaults();

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder(new[] { $"Client:Property={organisationPk}", $"Warehouse:Property={whsWarehouse.PK}" }, filters.Cast<FilterBusinessObjectDefault>().Select(x => $"{x.Key}={x.Value}"));
			AssertEquals(false, filters["Client:Property"].IsRemovable);
			AssertEquals(false, filters["Warehouse:Property"].IsRemovable);
		});
	}

	public void TestCreateGoodsItemForInventoryLine() => CombineAssertions(() =>
	{
		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
		var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 2m, 2m, "XX", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
		var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);
		var inventory = receiveLine.Inventory;

		var pivot = helper.Part.PivotsForBinding.AddNew();
		pivot.CI_RN_NKCountry = "LV";
		pivot.CI_ChildType = "IMP";
		pivot.CI_FormattedTariffNum = "100001";
		receiveLine.WE_LineNo = 5;
		Factory.Save();
		var selectionHeader = new NctsInventorySelectionHeader(bill);

		var wrapper = new WhsInventoryWrapper(inventory, selectionHeader);
		wrapper.QuantityToDraw = 2m;
		selectionHeader.SelectedLines.Add(wrapper);
		selectionHeader.ImportInventories();
		Factory.Save();
		AssertEquals(1, bill.GoodsItems.Count);

		var goodsItem = bill.GoodsItems[0];

		AssertEquals("", goodsItem.BY_BondedWHSOrderNumber);
		AssertEquals((ZShort)0, goodsItem.BY_BondedWHSOrderLineNumber);
		AssertEquals(2m, goodsItem.BY_BondedWhsQuantity);
		AssertEquals("NO", goodsItem.BY_BondedWhsUnitQty);
		AssertEquals("ENT1234", goodsItem.BY_WarehouseEntryNumber);
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

	public void TestCreateGoodsItemForInventoryLine_PackTypeFallback() => CombineAssertions(() =>
	{
		var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
		var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, Helper.Part.PK, ZString.Empty, 1m, 2m, 2m, "XX", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
		var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, ZString.Empty, ZString.Empty, "ENT1234", 1);
		var inventory = receiveLine.Inventory;

		var pivot = helper.Part.PivotsForBinding.AddNew();
		pivot.CI_RN_NKCountry = "LV";
		pivot.CI_ChildType = "IMP";
		pivot.CI_FormattedTariffNum = "100001";
		receiveLine.WE_LineNo = 5;
		Factory.Save();
		var selectionHeader = new NctsInventorySelectionHeader(bill);

		var wrapper = new WhsInventoryWrapper(inventory, selectionHeader);
		wrapper.QuantityToDraw = 2m;
		selectionHeader.SelectedLines.Add(wrapper);
		selectionHeader.ImportInventories();
		Factory.Save();
		AssertEquals(1, bill.GoodsItems.Count);

		var goodsItem = bill.GoodsItems[0];

		AssertEquals("", goodsItem.BY_BondedWHSOrderNumber);
		AssertEquals((ZShort)0, goodsItem.BY_BondedWHSOrderLineNumber);
		AssertEquals(2m, goodsItem.BY_BondedWhsQuantity);
		AssertEquals("XX", goodsItem.BY_BondedWhsUnitQty);
		AssertEquals("ENT1234", goodsItem.BY_WarehouseEntryNumber);
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

	protected override BusinessObject GetNewBusinessObject() => new NctsInventorySelectionHeader(bill);

	protected override void SetUp()
	{
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		bill = header.Bills.AddNew();
	}

	WhsDataTestHelper Helper => helper ??= new WhsDataTestHelper(Factory);
	WhsDataTestHelper helper;

	NctsHeader header;
	NctsBill bill;
}
