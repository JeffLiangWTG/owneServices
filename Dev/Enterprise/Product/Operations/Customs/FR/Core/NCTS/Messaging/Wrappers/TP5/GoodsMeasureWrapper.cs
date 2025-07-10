using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GoodsMeasureWrapper : IGoodsMeasure
	{
		protected GoodsMeasureWrapper(EU.NCTS.Business.NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		protected readonly EU.NCTS.Business.NctsCommonCargoDesc item;

		public static GoodsMeasureWrapper New(EU.NCTS.Business.NctsCommonCargoDesc item) => item == null ? null : new GoodsMeasureWrapper(item);

		public virtual decimal? GrossMass => grossMass ?? (grossMass = item.BY_GrossWeight);
		decimal? grossMass;

		public virtual decimal? NetMass => netMass ?? (netMass = item.BY_NetWeight);
		decimal? netMass;

		public virtual decimal? SupplementaryUnits => supplementaryUnits ?? (supplementaryUnits = item.BY_CustomsSecondQuantity);
		decimal? supplementaryUnits;
	}
}
