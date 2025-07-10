using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class ContainerNumberFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Container Number";

	public ContainerNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var filter = filterCollection.AddTextFilter(FilterDescription, CusCodeDataSchema.CY_Code);
		filter.Category = FilterCategories.NumbersAndReferences;
		filter.MultilingualDescription = ResString.GetMultilingualString("EFD51281-23B2-4572-9042-929F89CFE648", FilterDescription);
		filter.SubGroup = new ContainerSubGroup();
	}
}
