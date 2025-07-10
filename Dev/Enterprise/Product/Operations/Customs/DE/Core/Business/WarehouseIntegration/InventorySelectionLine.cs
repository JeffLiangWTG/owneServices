using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public sealed class InventorySelectionLine : Customs.Business.InventorySelectionLine
	{
		readonly Customs.Business.InventorySelectionHeader header;

		public InventorySelectionLine(Customs.Business.InventorySelectionHeader header) : base(header) => this.header = header;

		public override ZString US_DeclarantsReference => header.IsGroupByInventory
			? InventoryWrappers?.FirstOrDefault()?.Receive?.WD_CustomerReference ?? ZString.Empty
			: ZString.Empty;

		public override ZString US_SerialNumber => InventoryWrappers?.FirstOrDefault()?.ReceiveLine?.WE_SerialNumber ?? ZString.Empty;

		public override ZDateTime US_CustomsDeadline =>
			InventoryWrappers.Select(wrapper => wrapper.ReceiveLine?.CustomsData?.WB_CustomsDeadline)
			.Where(date => date.HasValue)
			.OrderBy(date => date.Value)
			.FirstOrDefault()
			.GetValueOrDefault();
	}
}
