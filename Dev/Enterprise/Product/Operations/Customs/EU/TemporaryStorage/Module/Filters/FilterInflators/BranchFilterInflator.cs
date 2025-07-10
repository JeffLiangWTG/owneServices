using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public sealed class BranchFilterInflator : FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Branch";

	public BranchFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var branchFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.GlbBranch, AsycudaManifestHeaderSchema.AMA_GB, new GlbBranchCollection(Factory));
		branchFilter.Category = FilterCategories.Organisations;
		branchFilter.MultilingualDescription = ResString.GetMultilingualString("287F0052-1584-42D5-9085-106FAB424CD8", FilterDescription);
	}
}
