using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupplementaryDeclarantLookups : CusCodeDataLookups
	{
		public SupplementaryDeclarantLookups(CusCodeData parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList SupplementaryDeclarantFilingTypesList => Factory.GetCachedValue<EUICS2SupplementaryDeclarantFilingTypeList>();
	}
}
