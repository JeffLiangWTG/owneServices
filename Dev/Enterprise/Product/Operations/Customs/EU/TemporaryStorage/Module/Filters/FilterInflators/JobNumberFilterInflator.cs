using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class JobNumberFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Job #";

	public JobNumberFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
		FilterColumn = AsycudaManifestHeaderSchema.AMA_JobReference;
	}

	public JobNumberFilterInflator(FilterStripBusinessObject bizObj, SchemaStringColumn filterColumn) : base(bizObj)
	{
		FilterColumn = Argument.NotNull(filterColumn, nameof(filterColumn));
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var jobNumberFilter = filterCollection.AddTextFilter(FilterDescription, FilterColumn);
		jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
		jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("B309F8B6-6A4A-41DB-AC8F-749D92C3AC02", FilterDescription);
	}

	SchemaStringColumn FilterColumn { get; }
}
