using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam.Testing
{
	public class VietnamComplianceInvoiceBookRegimeTest : TestCaseWithFactory
	{
		public void TestCountryFactoryCanReturnVietnamComplianceInvoiceBookRegime()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestAllowSeriesPrefixEmpty()
		{
			var featureInterface = GetFeatureInterface();
			AssertEquals(expected: false, featureInterface.AllowSeriesPrefixEmpty(true));
			AssertEquals(expected: true, featureInterface.AllowSeriesPrefixEmpty(false));
		}

		IComplianceInvoiceBookRegime GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IComplianceInvoiceBookRegime>(Constants.CountryCodes.VietNam);
	}
}
