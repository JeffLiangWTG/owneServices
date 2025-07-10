using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class CusClassificationFilterBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		public CusClassificationFilterBusinessObject()
		{
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter() => Business.TariffFormatter.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var cpcFilter = GetAddInfoTextFilter("CPC", AutoCusClassification.Schema.CC_ProcedureCode.Substring(3));
			cpcFilter.MultilingualDescription = ResString.GetMultilingualString("3EFB38E4-8351-4AF5-8311-AC563073D519", "CPC");
			cpcFilter.Category = FilterCategories.TextSearch;
			result.AddFilter(cpcFilter);

			return result;
		}
	}
}
