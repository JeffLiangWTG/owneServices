using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderDiscount))]
	internal class EdiPriceHeaderDiscountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConfig()
		{
			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Name = "DEVCOUNTRY";
			discount.PHD_Version = "STL1";
			discount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;
			var countryDiscount = (CountryDiscount)discount.Config;
			countryDiscount.Lines.RemoveAll();
			var line1 = countryDiscount.Lines.AddNew();
			line1.Country = "CN";
			line1.Percent = 30m;

			var line2 = countryDiscount.Lines.AddNew();
			line2.Country = "IN";
			line2.Percent = 20m;

			var line3 = countryDiscount.Lines.AddNew();
			line3.Country = "MX";
			line3.Percent = 20m;

			line3.Delete();

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var discountReloaded = factory2.Load<EdiPriceHeaderDiscount>(discount.PK);
			var config2 = (CountryDiscount)discountReloaded.Config;
			AssertEquals(2, config2.Lines.Count);
			AssertEquals("CN", config2.Lines[0].Country);
			AssertEquals(30m, config2.Lines[0].Percent);
			AssertEquals("IN", config2.Lines[1].Country);
			AssertEquals(20m, config2.Lines[1].Percent);
			AssertEquals(false, config2.HasChanges);

			//test ClearHasChanges()
			AssertEquals(false, discountReloaded.HasChanges);
			config2.Lines[1].Percent = 20.1m;
			AssertEquals(true, discountReloaded.HasChanges);
			AssertEquals(true, config2.HasChanges);
			factory2.Save();
			AssertEquals(false, discountReloaded.HasChanges);
			AssertEquals(false, config2.HasChanges);
			AssertEquals(20.1m, config2.Lines[1].Percent);

			AssertEquals(true, discount.PHD_Percent_ReadOnly);
			discount.PHD_Type = BillingConstants.DiscountCalculator.WiseCloud;
			AssertEquals(false, discount.PHD_Percent_ReadOnly);
		}

		public void TestPHD_Type()
		{
			var discount = Factory.New<EdiPriceHeaderDiscount>();
			discount.PHD_Name = "DEVCOUNTRY";
			discount.PHD_Version = "STL1";
			discount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;

			var countryDiscount = (CountryDiscount)discount.Config;
			var line1 = countryDiscount.Lines.AddNew();
			line1.Country = "CN";
			line1.Percent = 30m;
			var line2 = countryDiscount.Lines.AddNew();
			line2.Country = "IN";
			line2.Percent = 20m;

			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			AssertEquals("", discount.PHD_ConfigXml);
			AssertNull(discount.Config);
			discount.PHD_Percent = 10;

			discount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;
			AssertEquals(0m, discount.PHD_Percent);
		}
	}
}
