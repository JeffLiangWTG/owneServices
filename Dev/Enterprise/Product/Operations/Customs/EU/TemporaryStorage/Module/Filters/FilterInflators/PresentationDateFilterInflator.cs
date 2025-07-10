using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class PresentationDateFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Presentation Date";

	public PresentationDateFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
		FilterColumn = AsycudaManifestHeaderSchema.AMA_DateAtCustomsOffice;
	}

	public PresentationDateFilterInflator(FilterStripBusinessObject bizObj, SchemaDateTimeColumn filterColumn) : base(bizObj)
	{
		FilterColumn = Argument.NotNull(filterColumn, nameof(filterColumn));
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var presentationDateFilter = filterCollection.AddDateFilter(FilterDescription, FilterColumn);
		presentationDateFilter.Category = FilterCategories.Dates;
		presentationDateFilter.MultilingualDescription = ResString.GetMultilingualString("FE72604B-5A32-4918-91D0-9336C1AE7D5E", FilterDescription);
	}

	SchemaDateTimeColumn FilterColumn { get; }
}
