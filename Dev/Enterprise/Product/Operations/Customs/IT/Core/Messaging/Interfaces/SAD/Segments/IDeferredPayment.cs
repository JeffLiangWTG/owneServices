using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IDeferredPayment
{
	ZString AuthorizationReference { get; }
	ZString CinOfAuthorizationReference { get; }
}
