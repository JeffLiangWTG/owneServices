using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881BodyManualClosureProvider : IIE881BodyManualClosure
	{
		public static IE881BodyManualClosureProvider NewOrNull(BodyManualClosureType body) => body != null ? new IE881BodyManualClosureProvider(body) : null;

		IE881BodyManualClosureProvider(BodyManualClosureType body)
		{
			this.body = body;
		}
		readonly BodyManualClosureType body;

		public ZString BodyRecordUniqueReference => body.BodyRecordUniqueReference;

		public ZString IndicatorOfShortageOrExcess => IndicatorOfShortageOrExcessSpecified ? body.IndicatorOfShortageOrExcess.Value.XmlEnumToString() : ZString.Empty;

		public ZBool IndicatorOfShortageOrExcessSpecified => body.IndicatorOfShortageOrExcessValueSpecified;

		public ZDecimal ObservedShortageOrExcess => ObservedShortageOrExcessSpecified ? (ZDecimal)body.ObservedShortageOrExcess : ZDecimal.Zero;

		public ZBool ObservedShortageOrExcessSpecified => body.IndicatorOfShortageOrExcessValueSpecified;

		public ZString ExciseProductCode => body.ExciseProductCode;

		public ZDecimal RefusedQuantity => RefusedQuantitySpecified ? (ZDecimal)body.RefusedQuantity : ZDecimal.Zero;

		public ZBool RefusedQuantitySpecified => body.RefusedQuantityValueSpecified;

		public ITextAndLanguage ComplementaryInformation => complementaryInformation ?? (complementaryInformation = new IE881ComplementaryInformationProvider(body.ComplementaryInformation));
		ITextAndLanguage complementaryInformation;
	}
}
