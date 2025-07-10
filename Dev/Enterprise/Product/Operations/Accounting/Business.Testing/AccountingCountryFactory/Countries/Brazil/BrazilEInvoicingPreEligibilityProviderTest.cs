using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(BrazilEInvoicingPreEligibilityProvider))]

	public class BrazilEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.Brazil;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;
	}
}
