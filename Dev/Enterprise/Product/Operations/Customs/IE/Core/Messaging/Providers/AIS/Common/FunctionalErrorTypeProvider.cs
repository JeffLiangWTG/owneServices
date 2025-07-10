using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class FunctionalErrorTypeProvider
	{
		public FunctionalErrorTypeProvider(IFunctionalErrorType functionalError)
		{
			this.functionalError = Argument.NotNull(functionalError, nameof(functionalError));
		}
		readonly IFunctionalErrorType functionalError;

		public ZString ErrorReason => functionalError.ErrorReason;

		public ZString ErrorType => functionalError.ErrorType;

		public ZString ErrorMessage => functionalError.ErrorMessage;

		public ZString OriginalAttributeValue => functionalError.OriginalAttributeValue;

		public ZString ErrorPointer => functionalError.ErrorPointer;
	}
}
