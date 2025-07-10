using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsEidrReferenceLookups : GvmsItemReferenceLookups
	{
		public GvmsEidrReferenceLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override IEnumerable<string> ValidCodeList => GvmsItemReferencePartitions.EidrReferenceCodes;
	}
}
