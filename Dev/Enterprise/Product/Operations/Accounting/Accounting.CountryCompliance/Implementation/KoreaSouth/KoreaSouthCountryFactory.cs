using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class KoreaSouthCountryFactory : IInstanceProvider<IRelatedDisbursementTransaction>
	{
		public IRelatedDisbursementTransaction Get() => new KoreaSouthRelatedDisbursementTransactionProvider();
	}
}
