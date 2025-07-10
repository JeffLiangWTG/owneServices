using Enterprise.Accounting.Business.AccountingCountryFactory.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam.Testing
{
	[TestedType(typeof(VietnamEInvoicingPreEligibilityProvider))]
	public class VietnamEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.VietNam;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;
	}
}
