using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class MFunctionalError01Provider
	{
		public MFunctionalError01Provider(MFunctionalErrorType01 functionalError)
		{
			this.functionalError = Argument.NotNull(functionalError, nameof(functionalError));
		}
		readonly MFunctionalErrorType01 functionalError;

		public ZString SequenceNumber => functionalError.SequenceNumber;

		public ZString ErrorPointer => functionalError.ErrorPointer;

		public ZString ErrorCode => functionalError.ErrorCode;

		public ZString ErrorReason => functionalError.ErrorReason;

		public ZString Remarks => functionalError.Remarks;

		public ZString OriginalAttributeValue => functionalError.OriginalAttributeValue;
	}
}
