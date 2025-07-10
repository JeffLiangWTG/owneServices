using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class RegistrationNumberFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Registration Number";

	public RegistrationNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var registrationNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryNum);
		registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
		registrationNumberFilter.SubGroup = new BillCusEntryNumModuleFilterSubGroup(CusEntryNumberTypes.EU.CustomsRegistry);
		registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("B4D5AA80-34DE-4BF9-88C9-093AC0FF9F14", FilterDescription);
	}
}
