using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class PresenterFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Presenter";

	public PresenterFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
		FilterColumn = AsycudaManifestHeaderSchema.AMA_OA_Presenter;
	}

	public PresenterFilterInflator(FilterStripBusinessObject bizObj, SchemaGuidColumn filterColumn) : base(bizObj)
	{
		FilterColumn = Argument.NotNull(filterColumn, nameof(filterColumn));
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var presenterFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.OrgAddresses, FilterColumn, new OrgAddressCollection(Factory));
		presenterFilter.Category = FilterCategories.Organisations;
		presenterFilter.MultilingualDescription = ResString.GetMultilingualString("C3B84C05-7037-41DA-9806-122FD45C8502", FilterDescription);
	}

	SchemaGuidColumn FilterColumn { get; }
}
