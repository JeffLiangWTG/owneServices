using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5GoodsMeasureDepartureAndAmendmentWrapper : NCTS5CommonGoodsMeasureWrapper, INCTSGoodsMeasureDepartureAndAmendment
	{
		public NCTS5GoodsMeasureDepartureAndAmendmentWrapper(NctsDepartureCargoDesc item) : base(item)
		{
		}

		public ZDecimal SupplementaryUnits => new ZWeight(itemDeparture.BY_CustomsSecondQuantity, itemDeparture.BY_CustomsSecondUnitQty).InKilogramsSafe.Round(WeightMaxDecimalsFinalPeriod);

		public ZBool SupplementaryUnitsSpecified => !SupplementaryUnits.IsEmpty;
	}
}
