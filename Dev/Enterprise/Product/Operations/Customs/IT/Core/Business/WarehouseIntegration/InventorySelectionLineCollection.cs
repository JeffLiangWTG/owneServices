using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

sealed class InventorySelectionLineCollection : Customs.Business.InventorySelectionLineCollection<InventorySelectionLine>
{
	public InventorySelectionLineCollection(InventorySelectionHeader inventorySelectionHeader) : base(inventorySelectionHeader)
	{
		this.inventorySelectionHeader = inventorySelectionHeader;
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => new InventorySelectionLine(inventorySelectionHeader);

	readonly InventorySelectionHeader inventorySelectionHeader;
}
