using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonGoodsMeasureWrapper : INCTSCommonGoodsMeasure
	{
		public NCTS5CommonGoodsMeasureWrapper(NctsDepartureCargoDesc item)
		{
			itemDeparture = Argument.NotNull(item, nameof(item));
		}

		public NCTS5CommonGoodsMeasureWrapper(NctsArrivalCargoDesc item)
		{
			itemArrival = Argument.NotNull(item, nameof(item));
		}

		protected readonly NctsDepartureCargoDesc itemDeparture;
		protected readonly NctsArrivalCargoDesc itemArrival;

		const int WeightMaxDecimalsTransitionalPeriod = 3;
		protected const int WeightMaxDecimalsFinalPeriod = 6;
		protected int WeightMaxDecimals => (itemDeparture?.IsInPhase5TransitionPeriod ?? itemArrival.IsInPhase5TransitionPeriod) ? WeightMaxDecimalsTransitionalPeriod : WeightMaxDecimalsFinalPeriod;

		public ZDecimal GrossMass
		{
			get
			{
				if (itemDeparture != null)
				{
					return itemDeparture.GrossMassInKilograms.Round(WeightMaxDecimals);
				}
				else
				{
					var grossMassDeclared = itemArrival.GrossMassInKilograms.Round(WeightMaxDecimals);
					var grossMassUnloaded = itemArrival.UnloadedGoodsItem?.GrossMassInKilograms.Round(WeightMaxDecimals) ?? ZDecimal.Zero;
					return itemArrival.BY_UnloadedState.IsUnloadingStateNEW()
												? grossMassDeclared
												: grossMassDeclared != grossMassUnloaded
													? grossMassUnloaded
													: ZDecimal.Zero;
				}
			}
		}

		public ZBool GrossMassSpecified
		{
			get
			{
				if (itemDeparture != null)
				{
					return true;
				}
				else
				{
					return itemArrival.BY_UnloadedState.IsUnloadingStateNEW() || itemArrival.GrossMassInKilograms.Round(WeightMaxDecimals) != itemArrival.UnloadedGoodsItem.GrossMassInKilograms.Round(WeightMaxDecimals);
				}
			}
		}

		public ZDecimal NetMass
		{
			get
			{
				if (itemDeparture != null)
				{
					return itemDeparture.MoveHeader.BM_ReducedDatasetIndicator
											? ZDecimal.Zero
											: itemDeparture.CustomsFirstQuantityInKilograms.Round(WeightMaxDecimals);
				}
				else
				{
					var netMassDeclared = itemArrival.NetMassInKilograms.Round(WeightMaxDecimals);
					var netMassUnloaded = itemArrival.UnloadedGoodsItem?.NetMassInKilograms.Round(WeightMaxDecimals) ?? ZDecimal.Zero;
					return itemArrival.BY_UnloadedState.IsUnloadingStateNEW()
												? netMassDeclared
												: netMassDeclared != netMassUnloaded
													? netMassUnloaded
													: ZDecimal.Zero;
				}
			}
		}

		public ZBool NetMassSpecified
		{
			get
			{
				if (itemDeparture != null)
				{
					return !itemDeparture.MoveHeader.BM_ReducedDatasetIndicator;
				}
				else
				{
					return itemArrival.BY_UnloadedState.IsUnloadingStateNEW() || itemArrival.NetMassInKilograms.Round(WeightMaxDecimals) != itemArrival.UnloadedGoodsItem.NetMassInKilograms.Round(WeightMaxDecimals);
				}
			}
		}
	}
}
