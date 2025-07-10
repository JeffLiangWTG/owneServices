using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZFilterStripControlForTest : ZFilterStripControl
	{
		public ZFilterStripControlForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		public void RebuildFilterStrips_Exposed()
		{
			RebuildFilterStrips();
		}

		public new List<ZFilterStrip> Strips => base.Strips;
		public new Dictionary<string, List<ZFilterStrip>> GroupStrips => base.GroupStrips;
		public new FilterStripBusinessObject FilterBusinessObject => base.FilterBusinessObject;
	}
}
