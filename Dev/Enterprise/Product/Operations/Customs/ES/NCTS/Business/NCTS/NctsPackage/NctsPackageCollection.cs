using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsPackageCollection<TMaster> : NctsPackageCollection<NctsPackage, TMaster>
		where TMaster : NctsCommonCargoDesc
{
	public NctsPackageCollection(TMaster master) : base(master)
	{
		if (master is NctsArrivalCargoDesc)
		{
			arrivalGoodsItem = master as NctsArrivalCargoDesc;
		}
	}
	readonly NctsArrivalCargoDesc arrivalGoodsItem;

	protected override bool AllowNewCore
	{
		get
		{
			if (arrivalGoodsItem != null)
			{
				var arrivalMovement = (NctsArrivalMovementHeader)arrivalGoodsItem.Bill.Header.ArrivalMovementHeader;
				return base.AllowNewCore && !(arrivalMovement.UnloadingDifferenceDataReadOnly || arrivalMovement.IsUnloadingRemarksReadOnlySpain);
			}
			return base.AllowNewCore;
		}
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);

		var package = (NctsPackage)child;
		if (package.IsPhase5DepartureAndIsVehicles)
		{
			package.B5_UnitType = RefCusCodeList.PackageType.Frame;
		}
	}
}
