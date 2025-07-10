using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderDeferredPaymentWrapper : IDeferredPayment
{
	public NctsSADHeaderDeferredPaymentWrapper(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}
	readonly NctsDepartureMovementHeader movementHeader;

	public ZString AuthorizationReference
	{
		get
		{
			var accountNumber = movementHeader.DefermentAccountNumber;
			return accountNumber.Left(accountNumber.Length - 1);
		}
	}

	public ZString CinOfAuthorizationReference => movementHeader.DefermentAccountNumber.Right(1);
}
