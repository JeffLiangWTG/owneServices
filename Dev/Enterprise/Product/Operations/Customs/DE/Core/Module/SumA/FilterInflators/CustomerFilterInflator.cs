using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class CustomerFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Customer";

	public CustomerFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customerFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, CusTempStorageJobHeaderSchema.SJH_OH_Customer, Lookups.OrganisationList);
		customerFilter.Category = FilterCategories.Organisations;
		customerFilter.MultilingualDescription = ResString.GetMultilingualString("6b02a6be-328d-43c4-b09f-012e596610af", FilterDescription);
	}
}
