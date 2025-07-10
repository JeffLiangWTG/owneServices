using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class NotifyFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Notify";

	public NotifyFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var notifyFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
		notifyFilter.SubGroup = new PartySubGroup(FilterDescription);
		notifyFilter.Category = FilterCategories.Organisations;
		notifyFilter.MultilingualDescription = ResString.GetMultilingualString("E0D38371-3082-4AB9-AEF6-4BC9CECACF47", FilterDescription);
	}
}
