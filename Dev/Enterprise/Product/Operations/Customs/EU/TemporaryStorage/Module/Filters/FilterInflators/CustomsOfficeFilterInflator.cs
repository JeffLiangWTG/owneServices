using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class CustomsOfficeFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Customs Office";

	public CustomsOfficeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
		FilterColumn = AsycudaManifestHeaderSchema.AMA_CustomsOffice;
		Category = FilterCategories.Locations;
	}

	public CustomsOfficeFilterInflator(FilterStripBusinessObject bizObj, SchemaStringColumn filterColumn, FilterCategory category) : base(bizObj)
	{
		FilterColumn = Argument.NotNull(filterColumn, nameof(filterColumn));
		Category = Argument.NotNull(category, nameof(category));
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsOfficeFilter = filterCollection.AddTextFilter(FilterDescription, FilterColumn);
		customsOfficeFilter.Category = Category;
		customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("932E0D5F-69D1-442D-90F2-EFC7C8CD23EA", FilterDescription);
	}

	SchemaStringColumn FilterColumn { get; }
	FilterCategory Category { get; }
}
