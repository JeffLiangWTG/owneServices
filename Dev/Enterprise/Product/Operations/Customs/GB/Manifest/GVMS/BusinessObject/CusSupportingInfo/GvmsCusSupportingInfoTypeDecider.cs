using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GvmsCusSupportingInfoTypeDecider : ApplicationSpecificTypeDecider
	{
		protected override IEnumerable<ApplicationSpecificType> ApplicationSpecificTypesCore
		{
			get
			{
				foreach (var code in GvmsItemReferencePartitions.CustomsReferenceCodes)
				{
					yield return new ApplicationSpecificType(code, () => typeof(GvmsCustomsReference));
				}
				foreach (var code in GvmsItemReferencePartitions.TransitReferenceCodes)
				{
					yield return new ApplicationSpecificType(code, () => typeof(GvmsTransitReference));
				}
				foreach (var code in GvmsItemReferencePartitions.EidrReferenceCodes)
				{
					yield return new ApplicationSpecificType(code, () => typeof(GvmsEidrReference));
				}
			}
		}
	}
}
