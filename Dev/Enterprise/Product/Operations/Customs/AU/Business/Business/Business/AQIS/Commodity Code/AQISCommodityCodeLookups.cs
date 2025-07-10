using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISCommodityCodeLookups : ZLookups
	{
		public AQISCommodityCodeLookups(AQISCommodityCode parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AQISCommodityCodeList
		{
			get
			{
				return Factory.GetCachedValue("AQISCommodityCodeLookups.AQISCommodityCodeList", () =>
				{
					var fAQISCommodityCodeList = CMRReferenceDataHelper.SetupAQISCommodityCodeList(Factory);
					fAQISCommodityCodeList.SortByDescription();
					return fAQISCommodityCodeList;
				});
			}
		}
	}
}
