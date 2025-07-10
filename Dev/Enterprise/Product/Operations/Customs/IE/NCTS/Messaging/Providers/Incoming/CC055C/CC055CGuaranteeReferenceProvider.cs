using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC055CGuaranteeReferenceProvider
	{
		public CC055CGuaranteeReferenceProvider(GuaranteeReferenceType08 guaranteeReference)
		{
			this.guaranteeReference = Argument.NotNull(guaranteeReference, nameof(guaranteeReference));
		}
		readonly GuaranteeReferenceType08 guaranteeReference;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(guaranteeReference.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString GRN => guaranteeReference.Grn;

		public IEnumerable<(ZInt SequenceNumber, ZString InvalidGuaranteeReasonCode, ZString InvalidGuaranteeReasonText)> InvalidGuaranteeReasons => guaranteeReference.InvalidGuaranteeReason?.Cast<InvalidGuaranteeReasonType01>().Select(x =>
		{
			return (new ZInt(x?.SequenceNumber), new ZString(x?.Code), new ZString(x?.Text));
		}) ?? Enumerable.Empty<(ZInt SequenceNumber, ZString InvalidGuaranteeReasonCode, ZString InvalidGuaranteeReasonText)>();
	}
}
