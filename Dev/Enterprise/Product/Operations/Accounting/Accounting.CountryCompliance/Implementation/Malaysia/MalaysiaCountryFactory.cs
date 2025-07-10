using Enterprise.Accounting.CountryCompliance.Implementation.Malaysia;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class MalaysiaCountryFactory :
			IInstanceProvider<ITransactionReasonFormProvider>
	{
		ITransactionReasonFormProvider IInstanceProvider<ITransactionReasonFormProvider>.Get() => new MalaysiaTransactionReasonFormProvider();
	}
}
