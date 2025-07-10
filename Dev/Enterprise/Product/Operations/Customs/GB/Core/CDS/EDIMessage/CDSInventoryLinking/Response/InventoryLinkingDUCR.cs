using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingDUCR
	{
		public ZString UCR { get; set; }
		public ZString DeclarationID { get; set; }
		public ZString ParentMUCR { get; set; }
		public ZString ICS { get; set; }
		public ZString ROE { get; set; }
		public ZString SOE { get; set; }
		public List<InventoryLinkingMovement> Movement { get; set; }
		public List<InventoryLinkingGoodsItemObject> GoodsItem { get; set; }
	}
}
