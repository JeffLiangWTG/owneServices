using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.BR.Business.OrgSupplierPart;

namespace Enterprise.Customs.BR.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Brazil);

		protected override ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			var result = base.GetOrgSupplierPartModuleFiltersCore();
			AddCatalogLookupCode(result);

			return result;
		}

		void AddCatalogLookupCode(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(OrgSupplierPartFilterConstants.CatalogLookupCode, ModuleIDs.Customs.GoodsCatalog, GetCatalogLookupCode, new BaseCusGoodsCatalogCollection<CusGoodsCatalog>(Factory));
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = ResString.GetMultilingualString("B71A25E3-7F40-4D7C-BF35-07B6FD70581B", OrgSupplierPartFilterConstants.CatalogLookupCode);
		}

		protected ZQuery GetCatalogLookupCode(ZGuid value)
		{
			var partFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			pivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Core.Constants.CountryCodes.Brazil);

			var goodsCatalogSubQuery = new ZDBOnlySubQuery(typeof(CusGoodsCatalog), CusClassPartPivotSchema.CI_CGC_Catalog);
			goodsCatalogSubQuery.AddToFilter(CusGoodsCatalogSchema.PK, value);
			pivotSubQuery.AddSubQuery(goodsCatalogSubQuery, JoinCondition.And);

			partFilter.AddSubQuery(pivotSubQuery, JoinCondition.And);
			return partFilter;
		}
	}
}
