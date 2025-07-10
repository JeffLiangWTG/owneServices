using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCCategoryCodesCollection : EXDOCRefCodeCollection
	{
		public NEXDOCCategoryCodesCollection(IEXDOCRefCodeTypeProvider typeProvider, ZString productCode)
			: base(typeProvider, "NPRC")
		{
			UpdateFilterDefaults(productCode);
		}

		protected override ZString GetCodeTypeCore()
		{
			var singleCharCommodityCode = (ZString)EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(typeProvider?.Type);
			if (!singleCharCommodityCode.IsEmpty)
			{
				return baseCodeType + singleCharCommodityCode;
			}
			return ZString.Empty;
		}

		public readonly ZString AttributeProductType = "ProductType";

		protected void UpdateFilterDefaults(ZString productCode)
		{
			var filterDefault1 = new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", AttributeProductType);
			var filterDefault2 = new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", productCode);

			FilterBusinessObjectDefaults.Add(filterDefault1);
			FilterBusinessObjectDefaults.Add(filterDefault2);
		}
	}
}
