using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Module
{
	public class GoodsCatalogFilterBusinessObject : Customs.Module.GoodsCatalogFilterBusinessObject
	{
		public GoodsCatalogFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddTextFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var localPartNumberFilter = filters.AddTextFilter("Local Part Number", CusGoodsCatalogProductionInfoSchema.CGI_Reference);
			localPartNumberFilter.MaxLength = CusGoodsCatalogProductionInfoSchema.CGI_Reference.MaxLength;
			localPartNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|BR|CusGoodsCatalogFilter|LocalPartNumber", "Local Part Number");
			localPartNumberFilter.SubGroup = CusGoodsCatalogProductionInfoSubGroup;
		}

		CusGoodsCatalogProductionInfoFilterSubGroup CusGoodsCatalogProductionInfoSubGroup => cusGoodsCatalogProductionInfoSubgroup ?? (cusGoodsCatalogProductionInfoSubgroup = new CusGoodsCatalogProductionInfoFilterSubGroup());
		CusGoodsCatalogProductionInfoFilterSubGroup cusGoodsCatalogProductionInfoSubgroup;

		class CusGoodsCatalogProductionInfoFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var cusGoodsCatalogProductionInfoQuery = new ZDBOnlySubQuery(typeof(BaseCusGoodsCatalogProductionInfo), CusGoodsCatalogProductionInfoSchema.CGI_CGC_Catalog);
				cusGoodsCatalogProductionInfoQuery.AddToFilter(filter);
				cusGoodsCatalogProductionInfoQuery.AddToFilter(CusGoodsCatalogProductionInfoSchema.CGI_Type, CusGoodsCatalogProductionInfoTypeList.Codes.LPN);

				var catalogQuery = new ZDBOnlyQuery(typeof(CusGoodsCatalog));
				catalogQuery.AddSubQuery(cusGoodsCatalogProductionInfoQuery, JoinCondition.And);
				return catalogQuery;
			}
		}
	}
}
