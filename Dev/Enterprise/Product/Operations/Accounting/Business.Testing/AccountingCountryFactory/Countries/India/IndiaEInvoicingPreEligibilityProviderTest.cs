using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(IndiaEInvoicingPreEligibilityProvider))]
	public class IndiaEInvoicingPreEligibilityProviderTest : EInvoicingPreEligibilityProviderTest
	{
		protected override string CountryCode => CountryCodes.India;

		protected override bool ExpectedCanEvaluateByTransaction_IsInDatabase => true;
	}
}
