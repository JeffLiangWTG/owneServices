using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE704ErrorProvider : IIE704FunctionalError
	{
		public IE704ErrorProvider(FunctionalErrorType functionalError)
		{
			this.functionalError = Argument.NotNull(functionalError, nameof(functionalError));
		}
		readonly FunctionalErrorType functionalError;

		public ZString ErrorLocation => functionalError.ErrorLocation;

		public ZString ErrorType => functionalError.ErrorType.XmlEnumToString();

		public ZString ErrorReason => functionalError.ErrorReason;

		public ZString OriginalAttributeValue => functionalError.OriginalAttributeValue;
	}
}
