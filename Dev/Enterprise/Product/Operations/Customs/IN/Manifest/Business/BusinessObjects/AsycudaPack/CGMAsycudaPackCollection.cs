using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<CGMAsycudaPack, CGMAsycudaBill>
{
	public CGMAsycudaPackCollection(CGMAsycudaBill master) : base(master)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);

		using (child.GetValidationSuspender())
		{
			if (child is CGMAsycudaPack pack)
			{
				pack.APA_WeightUQ = pack.Bill?.ABL_GrossWeightUQ ?? ZString.Empty;
			}
		}
	}

	protected override bool ShouldDefaultContainerPK(ASYCUDA.Business.AsycudaManifestHeader header) => false;
}
