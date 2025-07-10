using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingEntry
	{
		public ZString UCR { get; set; }
		public ZString UCRPartNo { get; set; }
		public ZString UCRType { get; set; }
		public List<InventoryLinkingGoodsItem> GoodsItem { get; set; }
		public ZString SubmitRole { get; set; }
		public ZString ICS { get; set; }
		public ZString ROE { get; set; }
		public ZString SOE { get; set; }
	}
}
