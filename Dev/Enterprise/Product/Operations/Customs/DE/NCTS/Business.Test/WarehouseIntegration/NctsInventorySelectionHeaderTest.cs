using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsInventorySelectionHeader))]
sealed class NctsInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestSelectionLines()
	{
		var bill = Factory.New<NctsBill>();
		var inventorySelectionHeader = new NctsInventorySelectionHeader(bill);
		AssertType<InventorySelectionLineCollection>("SelectionLines Type", inventorySelectionHeader.SelectionLines);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var bill = Factory.New<NctsBill>();
		return new NctsInventorySelectionHeader(bill);
	}
}
