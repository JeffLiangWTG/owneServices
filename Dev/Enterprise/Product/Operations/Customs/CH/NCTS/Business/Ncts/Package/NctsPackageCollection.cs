using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsPackageCollection : EU.NCTS.Business.NctsPackageCollection<NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc>
{
	public NctsPackageCollection(EU.NCTS.Business.NctsCommonCargoDesc master) : base(master)
	{
	}

	protected override ZBool IsDefaultSelectSingleContainer => Master.Header.IsDepartureMovement;
}
