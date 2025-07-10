using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public abstract class BaseComplianceReportGUIActionProviderTest : TestCaseWithFactory
	{
		public void TestIsCountrySupportGenerateSAFT()
		{
			var provider = GetProvider();

			AssertNotNull(provider);
			AssertEquals(ExpectedIsCountrySupportGenerateSAFT, provider.IsCountrySupportGenerateSAFT);
		}

		protected abstract IComplianceReportGUIActionProvider GetProvider();

		public abstract void TestValidateBeforeGenerateSAFT();

		protected abstract bool ExpectedIsCountrySupportGenerateSAFT { get; }
	}
}
