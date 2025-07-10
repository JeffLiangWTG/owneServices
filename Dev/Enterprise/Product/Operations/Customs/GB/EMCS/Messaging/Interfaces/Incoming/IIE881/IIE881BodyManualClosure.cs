using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE881BodyManualClosure
	{
		ZString BodyRecordUniqueReference { get; }
		ZString IndicatorOfShortageOrExcess { get; }
		ZBool IndicatorOfShortageOrExcessSpecified { get; }
		ZDecimal ObservedShortageOrExcess { get; }
		ZBool ObservedShortageOrExcessSpecified { get; }
		ZString ExciseProductCode { get; }
		ZDecimal RefusedQuantity { get; }
		ZBool RefusedQuantitySpecified { get; }
		ITextAndLanguage ComplementaryInformation { get; }
	}
}
