using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CValidityLimitationProvider
	{
		public CC037CValidityLimitationProvider(ValidityLimitationType validityLimitation)
		{
			this.validityLimitation = Argument.NotNull(validityLimitation, nameof(validityLimitation));
		}
		readonly ValidityLimitationType validityLimitation;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => int.TryParse(validityLimitation.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCache;

		public ZString GuaranteeNotValidIn => validityLimitation.GuaranteeNotValidIn;
	}
}
