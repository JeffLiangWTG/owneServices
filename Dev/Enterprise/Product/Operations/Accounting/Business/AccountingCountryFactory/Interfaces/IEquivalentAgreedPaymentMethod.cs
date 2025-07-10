using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEquivalentAgreedPaymentMethod
	{
		CodeDescriptionPair GetEquivalentAgreedPaymentMethod(ZString agreedPaymentMethod, ZString paymentMethod);
	}
}
