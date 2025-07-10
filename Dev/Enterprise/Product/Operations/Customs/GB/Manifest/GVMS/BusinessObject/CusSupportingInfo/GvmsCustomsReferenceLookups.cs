using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsCustomsReferenceLookups : GvmsItemReferenceLookups
	{
		public GvmsCustomsReferenceLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override IEnumerable<string> ValidCodeList => GvmsItemReferencePartitions.CustomsReferenceCodes;
	}
}
