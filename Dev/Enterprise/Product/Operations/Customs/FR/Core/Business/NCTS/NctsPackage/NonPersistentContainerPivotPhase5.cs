using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.NCTS;

public class NonPersistentContainerPivotPhase5 : EU.NCTS.Business.NonPersistentContainerPivotPhase5
{
	public NonPersistentContainerPivotPhase5(EU.NCTS.Business.NctsPackage package, NctsCusInBondContainer container) : base(package, container)
	{
	}

	public override ZBool ContainerSelected
	{
		get => base.ContainerSelected;
		set
		{
			var oldValue = ContainerSelected;
			base.ContainerSelected = value;
			if (ContainerSelected != oldValue)
			{
				new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(package.Parent.MoveHeader as NctsDepartureMovementHeader);
			}
		}
	}
}
