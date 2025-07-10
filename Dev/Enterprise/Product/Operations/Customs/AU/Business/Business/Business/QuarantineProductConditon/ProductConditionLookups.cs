using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ProductConditionLookups : CusCodeDataLookups
	{
		public ProductConditionLookups(ProductCondition parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSProductCondition, ZDate.Today);
			}
		}
	}
}
