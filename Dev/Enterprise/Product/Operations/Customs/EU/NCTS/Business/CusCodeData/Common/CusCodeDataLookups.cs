using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusCodeDataLookups : Customs.Business.CusCodeDataLookups
	{
		public CusCodeDataLookups(CusCodeData parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<CusCodeDataTypeList>();
	}
}
