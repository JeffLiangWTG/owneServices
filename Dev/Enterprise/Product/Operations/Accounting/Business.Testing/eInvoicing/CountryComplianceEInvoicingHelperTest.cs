using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.eInvoicing
{
	public class CountryComplianceEInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestGetExistActivePivotCheckProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			{
				AssertNotNull(CountryComplianceEInvoicingHelper.GetExistPivotCheckProvider(GlbCompany.CurrentCompany));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				AssertNull(CountryComplianceEInvoicingHelper.GetExistPivotCheckProvider(GlbCompany.CurrentCompany));
			}
		}
	}
}
