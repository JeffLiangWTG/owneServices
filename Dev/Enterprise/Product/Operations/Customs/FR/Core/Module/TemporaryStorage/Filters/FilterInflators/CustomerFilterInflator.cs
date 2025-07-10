using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

public sealed class CustomerFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Customer";

	public CustomerFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customerFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, CusTempStorageJobHeaderSchema.SJH_OH_Customer, new OrganisationsFindBoxCollection(Factory));
		customerFilter.MultilingualDescription = ResString.GetMultilingualString("15BA7ADB-27A8-4EFE-9CF6-08EA0E864C0D", FilterDescription);
		customerFilter.Category = FilterCategories.Organisations;
	}
}
