using Enterprise.Customs.Common.CA;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TaxRateFormatterTest : TestCase
	{
		public void TestFormat()
		{
			AssertEquals(".1", TaxRateFormatter.Format(0.1m, RateTypes.Codes.AdValorem));
			AssertEquals("20.0", TaxRateFormatter.Format(20m, RateTypes.Codes.AdValorem));
			AssertEquals("22.5", TaxRateFormatter.Format(22.5m, RateTypes.Codes.AdValorem));
			AssertEquals(".01", TaxRateFormatter.Format(0.01m, RateTypes.Codes.Specific));
			AssertEquals(".00667", TaxRateFormatter.Format(2m / 3m / 100m, RateTypes.Codes.Specific));
		}
	}
}
