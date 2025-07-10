using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	sealed class WineCodeDataLookups : CusCodeDataLookups
	{
		public WineCodeDataLookups(WineCodeData parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get => Factory.GetCachedValue<EMCSOperationCodeList>();
		}
	}
}
