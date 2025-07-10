using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderExchangeRateCollection))]
	internal class EdiPriceHeaderExchangeRateCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceHeaderExchangeRateCollection>
	{
		public void TestFindByGroupCodeAndCurrency()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			priceHeader.L6_HasExchangeRates = true;
			var rate = priceHeader.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";

			AssertNull(priceHeader.ExchangeRates.FindByGroupCodeAndCurrency("GP1", "AUD"));
			AssertEquals(rate, priceHeader.ExchangeRates.FindByGroupCodeAndCurrency("STL", "AUD"));
		}
	}
}
