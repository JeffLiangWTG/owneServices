using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingFlattened))]
	public class ClientLicenceBillingFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsValueProvided()
		{
			var obj = new ClientLicenceBillingFlattened();
			AssertEquals(false, obj.IsCurrentPrepaymentBalanceProvided);
			AssertEquals(false, obj.IsCurrentPrepaymentCurrencyProvided);
			AssertEquals(false, obj.IsFuturePrepaymentBalanceProvided);
			AssertEquals(false, obj.IsFuturePrepaymentCurrencyProvided);

			obj.CurrentPrepaymentBalance = 1m;
			AssertEquals(true, obj.IsCurrentPrepaymentBalanceProvided);
			obj.CurrentPrepaymentCurrency = "AUD";
			AssertEquals(true, obj.IsCurrentPrepaymentCurrencyProvided);
			obj.FuturePrepaymentBalance = 2m;
			AssertEquals(true, obj.IsFuturePrepaymentBalanceProvided);
			obj.FuturePrepaymentCurrency = "USD";
			AssertEquals(true, obj.IsFuturePrepaymentCurrencyProvided);
		}
	}
}
