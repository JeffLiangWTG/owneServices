using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI.Module
{
	public class CommodityDetailFilterBusinessObject : FilterStripBusinessObject
	{
		public CommodityDetailFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = AutoComplianceCommodityDetail.Schema.TableName;
			QueryObjectType = typeof(ComplianceCommodityDetail);
		}

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			// override to prevent adding initial audit filters
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddRiskStatusFilter(filters);
			AddHarmonizedCodesFilter(filters);
			AddGoodsDescriptionFilter(filters);
			AddOriginOfGoodsFilter(filters);
			return filters;
		}

		#region Filters

		void AddRiskStatusFilter(ModuleFilterCollection filters)
		{
			var riskStatusFilter = filters.AddTextFilter("Risk Status", ComplianceCommodityDetailSchema.CCD_RiskStatus, () => ComplianceRiskStatusCodeList.GetCommodityAllStatusList());
			riskStatusFilter.MultilingualDescription = ResString.GetMultilingualString("14797771-11F9-422C-A081-179BFBC85F29", "Risk Status");
			riskStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		void AddHarmonizedCodesFilter(ModuleFilterCollection filters)
		{
			var harmonizedCodesFilter = filters.AddTextFilter("Harmonized Codes", ComplianceCommodityDetailSchema.CCD_HarmonizedCode);
			harmonizedCodesFilter.MultilingualDescription = ResString.GetMultilingualString("76DF0CC7-6BB1-422A-B7A6-5834DDC0F7F2", "Harmonized Codes");
			harmonizedCodesFilter.Category = FilterCategories.Other;
		}

		void AddGoodsDescriptionFilter(ModuleFilterCollection filters)
		{
			var goodsDescriptionFilter = filters.AddTextFilter("Goods Description", ComplianceCommodityDetailSchema.CCD_Description);
			goodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("AB2DFE2D-CBBD-40CE-A500-D76813EBCD9F", "Goods Description");
			goodsDescriptionFilter.Category = FilterCategories.Other;
		}

		void AddOriginOfGoodsFilter(ModuleFilterCollection filters)
		{
			var originOfGoodsFilter = filters.AddNkFilter("Origin Of Goods", ComplianceCommodityDetailSchema.CCD_RN_NKOrigin, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			originOfGoodsFilter.MultilingualDescription = ResString.GetMultilingualString("C9472669-E73A-487D-A1E9-1352F607B48B", "Origin of Goods");
			originOfGoodsFilter.Category = FilterCategories.Locations;
		}

		#endregion
	}
}
