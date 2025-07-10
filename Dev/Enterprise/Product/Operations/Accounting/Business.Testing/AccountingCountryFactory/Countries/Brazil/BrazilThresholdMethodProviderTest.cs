using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Brazil;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(BrazilThresholdMethodProvider))]
	sealed class BrazilThresholdMethodProviderTest : ThresholdMethodProviderTest
	{
		protected override string CountryCode => CountryCodes.Brazil;

		protected override bool ExpectedIsTransactionLevelGroupThresholdMethodSupported => true;

		protected override bool ExpectedIsTransactionLevelTaxBaseThresholdMethodSupported => false;
	}
}
