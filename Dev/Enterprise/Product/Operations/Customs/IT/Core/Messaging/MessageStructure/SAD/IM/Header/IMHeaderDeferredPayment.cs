using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderDeferredPayment
{
	public IMHeaderDeferredPayment(IDeferredPayment deferredPayment)
	{
		this.deferredPayment = Argument.NotNull(deferredPayment, nameof(deferredPayment));
	}

	readonly IDeferredPayment deferredPayment;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 6, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString AuthorisationReference => deferredPayment.AuthorizationReference;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString CINOfAuthorisationReference => deferredPayment.CinOfAuthorizationReference;
}
