using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class InventorySelectionLineCollection : InventorySelectionLineCollection<InventorySelectionLine>
	{
		public InventorySelectionLineCollection(Customs.Business.InventorySelectionHeader header) : base(header)
		{
			this.header = header;
		}

		readonly Customs.Business.InventorySelectionHeader header;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new InventorySelectionLine(header);
	}
}
