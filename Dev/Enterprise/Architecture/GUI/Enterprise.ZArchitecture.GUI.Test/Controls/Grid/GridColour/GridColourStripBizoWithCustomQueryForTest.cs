using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class GridColourStripBizoWithCustomQueryForTest : GridColourStripBusinessObject
	{
		public GridColourStripBizoWithCustomQueryForTest(ZQuery filter)
			: base(new FilterStripBusinessObjectForTest(), null, null, false)
		{
			this.filter = filter;
			filter.AddOptionRecompileConditionally = true;
		}

		public override ZQuery Filter => filter;
		public override ZQuery GetFilterForGridItems(IEnumerable<BusinessObject> businessObjects) => filter;

		readonly ZQuery filter;
	}
}
