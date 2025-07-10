using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module;

public sealed class BranchFilterInflator : SumAFilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	public const string FilterDescription = "Branch";

	public BranchFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var branchFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.GlbBranch, CusTempStorageJobHeaderSchema.SJH_GB, Lookups.BranchList);
		branchFilter.Category = FilterCategories.Organisations;
		branchFilter.MultilingualDescription = ResString.GetMultilingualString("7bb0713d-e330-4c5b-8a1e-687c529978ca", FilterDescription);
	}
}
