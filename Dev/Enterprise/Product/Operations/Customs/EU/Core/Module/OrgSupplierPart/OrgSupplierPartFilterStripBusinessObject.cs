using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.EU.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		public override IBaseClassificationCollection<BaseCusClassification> Classifications => new BaseClassificationCollection<CusClassification>(Factory);

		protected override bool NeedTariffCodeFilter => false;

		protected override ModuleFilterCollection GetOrgSupplierPartModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetOrgSupplierPartModuleFiltersCore();
			AddCommodityTariffFilter(result);
			return result;
		}

		protected override CodeDescriptionPairList GetClassificationTypeList() => Factory.GetCachedValue<ClassificationTypeList>();

		void AddCommodityTariffFilter(ModuleFilterCollection result)
		{
			var tariffFilter = result.AddTextFilter("Tariff", GetTariffQuery).WithMaxLengthOf<ModuleTextFilter>(CusClassPartPivotSchema.CI_TariffNum);
			tariffFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|OrgSupplierPartFilter|Tariff", "Tariff");
			tariffFilter.Category = FilterCategories.NumbersAndReferences;
		}

		protected ZQuery GetTariffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			ZDBOnlyQuery classificationFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(CusClassPartPivot), CusClassPartPivotSchema.CI_OP);
			pivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, comparisonOperator, CleanUpTariff(value));
			pivotSubQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			classificationFilter.AddSubQuery(pivotSubQuery, JoinCondition.And);
			result.AddToFilter(classificationFilter);
			return result;
		}

		ZString CleanUpTariff(ZString value)
		{
			return value.Replace(" ", "").Replace(".", "");
		}
	}
}
