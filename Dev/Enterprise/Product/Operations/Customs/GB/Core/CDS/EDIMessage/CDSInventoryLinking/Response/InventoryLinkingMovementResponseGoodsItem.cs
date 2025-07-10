using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingGoodsItem
	{
		public ZString CommodityCode { get; set; }
		public ZString TotalPackages { get; set; }
		public ZDecimal TotalNetMass { get; set; }
	}
}
