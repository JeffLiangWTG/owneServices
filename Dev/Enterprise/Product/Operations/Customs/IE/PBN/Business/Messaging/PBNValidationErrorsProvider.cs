using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging;
public class PBNValidationErrorsProvider
{
	public PBNValidationErrorsProvider(PBNValidationErrors validationError)
	{
		currentValidationError = validationError;
	}

	readonly PBNValidationErrors currentValidationError;

	public ZString code => currentValidationError.ErrorCode ?? ZString.Empty;

	public ZString path => currentValidationError.Path ?? ZString.Empty;

	public ZString description => currentValidationError.ErrorDescription ?? ZString.Empty;
}
