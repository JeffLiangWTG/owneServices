using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceHeaderDiscountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPHD_IsDefaultEnabled()
		{
			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Type = BillingConstants.DiscountCalculator.SingleCountry;
			discount.PHD_IsDefaultEnabled = false;
			AssertNoNotifications(discount.PHD_IsDefaultEnabledInfo);

			discount.PHD_IsDefaultEnabled = true;
			AssertHasWarnings(discount.PHD_IsDefaultEnabledInfo);

			discount.PHD_Type = BillingConstants.DiscountCalculator.WiseCloud;
			discount.Validation.ValidatePHD_IsDefaultEnabled();
			AssertNoNotifications(discount.PHD_IsDefaultEnabledInfo);
		}

		public void TestCheckPHD_Type()
		{
			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Type = BillingConstants.DiscountCalculator.ProductBundle;
			discount.PHD_Version = "STL1";

			var discount2 = Factory.New<EdiPriceHeaderDiscount>();
			discount2.PHD_Version = "STL1";
			discount2.PHD_Type = BillingConstants.DiscountCalculator.ProductBundle;
			AssertHasError(discount2.PHD_TypeInfo, "Only 1 Bundle discount can be configured against a pricelist.");

			discount2.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			AssertNoErrors(discount2.PHD_TypeInfo);
		}
	}
}
