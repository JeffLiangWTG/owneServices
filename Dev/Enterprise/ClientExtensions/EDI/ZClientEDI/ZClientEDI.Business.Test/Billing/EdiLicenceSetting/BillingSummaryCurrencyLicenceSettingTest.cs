using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BillingSummaryCurrencyLicenceSetting))]
	internal class BillingSummaryCurrencyLicenceSettingTest : EdiLicenceSettingTest
	{
		public void TestBillingSummaryCurrencyLicenceSetting()
		{
			var obj = Factory.New<BillingSummaryCurrencyLicenceSetting>();
			AssertEquals(BillingConstants.LicenceSetting.BillingSummaryCurrency, obj.LS9_Type);
			AssertEquals("", obj.LS9_RX_NKPriceCurrency);

			obj.LS9_RX_NKPriceCurrency = "AUD";
			AssertEquals("AUD", obj.Summary);
			AssertType<BillingSummaryCurrencyLicenceSettingValidation>(obj.Validation);
		}
	}

	internal class BillingSummaryCurrencyLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCurrency()
		{
			var db = Factory.New<LicenceDatabase>();
			db.FillWithValidTestData();

			var setting = Factory.New<BillingSummaryCurrencyLicenceSetting>();
			setting.LS9_RX_NKPriceCurrency = "";
			AssertHasError(setting.LS9_RX_NKPriceCurrencyInfo, "Please enter a value.");
			setting.LS9_RX_NKPriceCurrency = "xxx";
			AssertHasError(setting.LS9_RX_NKPriceCurrencyInfo, "Enter a valid selection.");

			setting.LS9_RX_NKPriceCurrency = "AUD";
			AssertNoErrors(setting.LS9_RX_NKPriceCurrencyInfo);
		}
	}
}
