using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NonPersistentDepartureContainerPivot : EU.NCTS.Business.NonPersistentDepartureContainerPivot
	{
		public NonPersistentDepartureContainerPivot(NctsCommonCargoDesc line) : base(line)
		{
		}

		public override ZBool ContainerSelected
		{
			get => base.ContainerSelected;
			set
			{
				var oldValue = ContainerSelected;
				base.ContainerSelected = value;

				if (oldValue != ContainerSelected)
				{
					new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(Container?.Header?.MovementHeader as NctsDepartureMovementHeader);
				}
			}
		}
	}
}
