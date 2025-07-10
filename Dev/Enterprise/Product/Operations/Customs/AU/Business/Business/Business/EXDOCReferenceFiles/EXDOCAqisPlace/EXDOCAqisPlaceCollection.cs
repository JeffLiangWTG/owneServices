using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCAqisPlaceCollection : EXDOCRefCodeCollection
	{
		public EXDOCAqisPlaceCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "AQISP")
		{
			UpdateFilterDefaults();
		}

		public EXDOCAqisPlaceCollection(IEXDOCRefCodeTypeProvider typeProvider, ZString baseCodeType)
			: base(typeProvider, baseCodeType)
		{
			UpdateFilterDefaults();
		}

		protected void UpdateFilterDefaults()
		{
			var filterDefault = new FilterBusinessObjectDefault(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", GetAttributeName);   // identifier
			FilterBusinessObjectDefaults.Add(filterDefault);
		}

		IZType GetAttributeName()
		{
			return (ZString)EXDOCCommodityCodeAttributes.GetAttributeNameFromThreeCharCode(typeProvider?.Type);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();

			var refCusCodeListQuery = new ZDBOnlyQuery(typeof(RefCusCodeList));
			refCusCodeListQuery.AddToFilter(filter);

			if ((string)typeProvider?.Type == EXDOCCommodityCodes.Codes.Meat)
			{
				var codeListAttributeSubQuery = new ZDBOnlySubQuery(typeof(RefCusCodeListAttribute), RefCusCodeListAttributeSchema.ZZE_ZZD_CodeList, ZZRefCusCodeListCombinedSchema.PK);
				codeListAttributeSubQuery.AddToFilter(RefCusCodeListAttributeSchema.ZZE_ZXE_NKName, EXDOCCommodityCodeAttributes.Codes.QuarantineRegion);
				refCusCodeListQuery.AddSubQuery(codeListAttributeSubQuery, JoinCondition.And);
			}

			return refCusCodeListQuery;
		}
	}
}
