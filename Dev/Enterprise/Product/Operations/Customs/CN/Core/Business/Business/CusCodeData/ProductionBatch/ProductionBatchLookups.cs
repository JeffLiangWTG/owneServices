using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class ProductionBatchLookups : CusCodeDataLookups
	{
		public ProductionBatchLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.CN.Business.ProductionBatchLookups.CY_CodeList", () =>
				{
					var result = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Constants.CusCodeDataCode.BatchNumber, Constants.CusCodeDataCode.BatchNumber)
					};
					return result;
				});
			}
		}
	}
}
