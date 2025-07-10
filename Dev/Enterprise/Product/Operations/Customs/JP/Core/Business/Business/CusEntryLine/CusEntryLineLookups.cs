using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
	{
		public CusEntryLineLookups(CusEntryLine parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PriceCheckTypeList => Factory.GetCachedValue<PriceCheckTypeList>();
	}
}
