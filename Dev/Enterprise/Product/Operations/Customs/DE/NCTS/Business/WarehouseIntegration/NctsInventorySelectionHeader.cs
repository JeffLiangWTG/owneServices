using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsInventorySelectionHeader : EU.NCTS.Business.NctsInventorySelectionHeader
{
	public NctsInventorySelectionHeader(NctsBill parent) : base(parent)
	{
	}

	protected override IInventorySelectionLineCollection<Customs.Business.InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection(this);
}
