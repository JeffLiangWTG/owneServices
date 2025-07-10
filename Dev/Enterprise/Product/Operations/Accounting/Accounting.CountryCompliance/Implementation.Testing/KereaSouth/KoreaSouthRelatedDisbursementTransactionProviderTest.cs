using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.KoreaSouth.Testing
{
	public class KoreaSouthRelatedDisbursementTransactionProviderTest : TestCaseWithFactory
	{
		public void TestIsEnableRelatedDisbursementTransaction()
		{
			Assert(GetFeatureInterface().IsEnableRelatedDisbursementTransaction());
		}

		public void TestAppendAdditionalDescription()
		{
			AssertEquals(" 총합계 Test 원", GetFeatureInterface().AppendAdditionalDescription("Test"));
		}

		IRelatedDisbursementTransaction GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IRelatedDisbursementTransaction>(Constants.CountryCodes.KoreaSouth);
	}
}
