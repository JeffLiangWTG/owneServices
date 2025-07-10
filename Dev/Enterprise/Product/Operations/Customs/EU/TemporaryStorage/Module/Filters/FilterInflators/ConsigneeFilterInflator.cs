using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class ConsigneeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Consignee";

	public ConsigneeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var consigneeFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
		consigneeFilter.SubGroup = new PartySubGroup(FilterDescription);
		consigneeFilter.Category = FilterCategories.Organisations;
		consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("C1A5CCAA-CA0B-4EC9-A834-A35130C40626", FilterDescription);
	}
}
