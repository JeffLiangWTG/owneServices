using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class FRNctsDepartureHeaderContainerValueSetStrategy : IValueSetStrategy
	{
		public FRNctsDepartureHeaderContainerValueSetStrategy(FRNctsDepartureHeaderContainer container)
		{
			this.container = container;
		}

		readonly FRNctsDepartureHeaderContainer container;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case FRNctsDepartureHeaderContainer.Schema.BC_RC:
				case FRNctsDepartureHeaderContainer.Schema.BC_Mode:
					new HarbourFeeDepartureMovementCalculationManager(container.Factory).Calculate(container.Header?.MovementHeader);
					break;
			}
		}
	}
}
