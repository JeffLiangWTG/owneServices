using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADDeferredPaymentWrapper : IDeferredPayment
{
	public SADDeferredPaymentWrapper(JobDeclaration jobDeclaration)
	{
		this.jobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
	}
	readonly JobDeclaration jobDeclaration;

	public ZString AuthorizationReference
	{
		get
		{
			var defermentAccountNumber = jobDeclaration.JE_DefermentAccountNumber;
			return defermentAccountNumber.IsEmpty ? ZString.Empty : defermentAccountNumber.Left(defermentAccountNumber.Length - 1);
		}
	}
	public ZString CinOfAuthorizationReference => jobDeclaration.JE_DefermentAccountNumber.Right(1);
}
