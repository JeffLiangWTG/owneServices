using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class BrandTypeElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "00422"; // 品牌类型

		public override bool ProvideList => true;

		public override bool IsMergeKey => true;

		public override ICodeDescriptionPairList GetList(BusinessObjectFactory factory, EnteringOrExiting enteringOrExiting)
		{
			return
				enteringOrExiting == EnteringOrExiting.Entering
				? factory.GetCachedValue("CNBrandTypeList_ENT", () =>
				{
					var fullList = new BrandTypeList();
					fullList.RemoveCode(BrandTypeList.Codes._3);
					return fullList;
				})
				: factory.GetCachedValue<BrandTypeList>();
		}
	}
}
