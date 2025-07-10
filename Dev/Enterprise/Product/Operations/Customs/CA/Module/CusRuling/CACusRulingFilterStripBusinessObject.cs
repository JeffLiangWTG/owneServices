using Enterprise.Customs.Universal.Module;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.Universal.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CACusRulingFilterStripBusinessObject : ZZRefCusRulingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			((ModuleTextFilter)filters[ZZRefCusRulingFilters.RulingType]).MultilingualDescription = ResString.GetMultilingualString("F87DF149-D22A-4CE7-854A-640D0846A6CA", "Remission Type");
			((ModuleTextFilter)filters[ZZRefCusRulingFilters.RulingNumber]).MultilingualDescription = ResString.GetMultilingualString("05523D98-B0F6-4311-B2B3-ED88B2E3B976", "Remission Number");
			return filters;
		}

		public new CACusRulingFilterLookups Lookups => (CACusRulingFilterLookups)base.Lookups;

		protected override ZZRefCusRulingFilterLookups GetNewLookups()
		{
			return new CACusRulingFilterLookups(this);
		}
	}
}
