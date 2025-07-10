using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMAsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<CGMAsycudaBill, CGMAsycudaManifestHeader>
{
	public CGMAsycudaBillCollection(CGMAsycudaManifestHeader master) : base(master)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);

		if (child is CGMAsycudaBill bill && bill.Header is CGMAsycudaManifestHeader header)
		{
			if (header.IsAir)
			{
				bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Package;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
				bill.ABL_SpecialCargoCode = ShipmentTypeList.Codes.Total;

				if (header.AMA_OverrideFreightDefaults || header.AMA_ParentTableCode != JobConsolSchema.Constants.Prefix)
				{
					if (!header.AMA_RL_NKOrigin.IsEmpty)
					{
						bill.ABL_RL_NKOrigin = header.AMA_RL_NKOrigin;
					}
					if (!header.AMA_RL_NKFinalDestination.IsEmpty)
					{
						bill.ABL_RL_NKFinalDestination = header.AMA_RL_NKFinalDestination;
					}
				}
			}

			var messageStatus = header.MessageStatus;
			var registrationStatus = header.RegistrationStatus;

			if (bill.IsActionFixedToFresh)
			{
				bill.ABL_BillStatus = BillActionList.Codes.Fresh;
			}
			else if (bill.IsActionFixedToSupplementary)
			{
				bill.ABL_BillStatus = BillActionList.Codes.Supplementary;
			}
		}
	}
}
