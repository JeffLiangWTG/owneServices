using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureCargoDescValueSetStrategy : EU.NCTS.Business.NctsDepartureCargoDescValueSetStrategy
	{
		public NctsDepartureCargoDescValueSetStrategy(NctsDepartureCargoDesc goodItem) : base(goodItem)
		{
			this.goodItem = goodItem;
		}

		readonly NctsDepartureCargoDesc goodItem;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case NctsDepartureCargoDesc.Schema.BY_GrossWeight:
				case NctsDepartureCargoDesc.Schema.BY_GrossWeightUnit:
					new HarbourFeeDepartureMovementCalculationManager(goodItem.Factory).Calculate(goodItem.MoveHeader);
					break;
			}
		}
	}
}
