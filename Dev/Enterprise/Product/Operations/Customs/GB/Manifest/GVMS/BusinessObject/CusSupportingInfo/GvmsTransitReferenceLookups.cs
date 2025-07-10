using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsTransitReferenceLookups : GvmsItemReferenceLookups
	{
		public GvmsTransitReferenceLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override IEnumerable<string> ValidCodeList => GvmsItemReferencePartitions.TransitReferenceCodes;
	}
}
