using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Malaysia.Testing
{
	public class MalaysiaTransactionReasonFormProviderTest : TestCaseWithFactory
	{
		public void TestShouldShowAmendInFull()
		{
			var transactionFormShowItemProvider = GetFeatureInterface();
			Assert(transactionFormShowItemProvider.ShouldShowAmendInFull());
		}

		ITransactionReasonFormProvider GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<ITransactionReasonFormProvider>(Constants.CountryCodes.Malaysia);
	}
}
