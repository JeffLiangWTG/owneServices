using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class ConsignorFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Consignor";

	public ConsignorFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var consignorFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, OrgAddressSchema.OA_OH, new OrganisationsFindBoxCollection(Factory));
		consignorFilter.SubGroup = new PartySubGroup(FilterDescription);
		consignorFilter.Category = FilterCategories.Organisations;
		consignorFilter.MultilingualDescription = ResString.GetMultilingualString("F7719AB0-8E01-44BC-8C4C-5B399600651F", FilterDescription);
	}
}
