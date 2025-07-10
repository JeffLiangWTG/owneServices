using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class CurrentRegistrationNumberFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Current Registration No.";

	public CurrentRegistrationNumberFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var currentRegistrationNumberFilter = filterCollection.AddTextFilter(FilterDescription, CusEntryNumSchema.CE_EntryNum);
		currentRegistrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("e37be1eb-4c05-4a1f-8452-5a5d12d47bad", FilterDescription);
		currentRegistrationNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
		currentRegistrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
		currentRegistrationNumberFilter.SubGroup = new MostRecentlyModifiedDeclarationSubGroup();
	}
}
