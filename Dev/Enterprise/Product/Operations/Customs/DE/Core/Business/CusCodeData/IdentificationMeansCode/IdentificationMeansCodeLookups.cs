using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class IdentificationMeansCodeLookups : CusCodeDataLookups
	{
		public IdentificationMeansCodeLookups(IdentificationMeansCode officeCode)
			: base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<IdentificationMeansList>();
	}
}
