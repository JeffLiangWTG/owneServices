using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class SupervisingCustomsOfficeFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Supervising Customs Office";

	public SupervisingCustomsOfficeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsOfficeCodeList = TemporaryStorageHeader.Lookups.CustomsOfficeCodeList;
		var supervisingCustomsOfficeFilter = filterCollection.AddNkFilter(FilterDescription, AsycudaManifestHeaderSchema.AMA_CustomsOffice, ModuleIDs.Customs.Universal.ZZRefCusCodeList, customsOfficeCodeList);
		supervisingCustomsOfficeFilter.Category = FilterCategories.Locations;
		supervisingCustomsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("A4D436F3-2B8F-45E5-B112-51D906723888", FilterDescription);
	}
}
