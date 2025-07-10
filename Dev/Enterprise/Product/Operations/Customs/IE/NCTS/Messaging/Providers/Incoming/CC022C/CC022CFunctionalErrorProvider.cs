using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC022CFunctionalErrorProvider
	{
		public CC022CFunctionalErrorProvider(FunctionalErrorType01 functionalErrorType)
		{
			this.functionalErrorType = Argument.NotNull(functionalErrorType, nameof(functionalErrorType));
		}
		readonly FunctionalErrorType01 functionalErrorType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(functionalErrorType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString ErrorPointer => functionalErrorType.ErrorPointer;

		public ZString ErrorCode => functionalErrorType.ErrorCode.GetXmlEnumAttributeValue();

		public ZString ErrorReason => functionalErrorType.ErrorReason;

		public ZString OriginalAttributeValue => functionalErrorType.OriginalAttributeValue;
	}
}
