using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Jordan;
using Enterprise.Accounting.Business.AccountingCountryFactory.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Countries.Jordan
{
	[TestedType(typeof(JordanEInvoicingPreEligibilityProvider))]
	public class JordanEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => Constants.CountryCodes.Jordan;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;
	}
}
