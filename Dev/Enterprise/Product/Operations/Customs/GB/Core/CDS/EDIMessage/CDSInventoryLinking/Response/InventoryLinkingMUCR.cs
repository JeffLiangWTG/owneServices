using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingMUCR
	{
		public ZString UCR { get; set; }
		public ZBool Shut { get; set; }
		public ZString ParentMUCR { get; set; }
		public ZString ICS { get; set; }
		public ZString ROE { get; set; }
		public ZString SOE { get; set; }
		public List<InventoryLinkingMovement> Movement { get; set; }
	}
}
