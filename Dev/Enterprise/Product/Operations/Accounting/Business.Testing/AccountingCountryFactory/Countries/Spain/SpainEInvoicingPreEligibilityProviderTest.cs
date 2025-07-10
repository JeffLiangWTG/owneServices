using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(SpainEInvoicingPreEligibilityProvider))]
	public class SpainEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.Spain;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;
	}
}
