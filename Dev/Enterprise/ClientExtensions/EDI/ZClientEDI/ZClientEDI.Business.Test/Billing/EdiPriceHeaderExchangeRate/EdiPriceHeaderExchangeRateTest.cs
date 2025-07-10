using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderExchangeRate))]
	internal class EdiPriceHeaderExchangeRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCalculatePrice()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDNYC";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_HasExchangeRates = true;
			priceHeader.L6_Rounding = "V1";
			var price1 = priceHeader.Items.AddNew();
			price1.L7_PGM_DiscountGroupCode = "G1";

			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			var rate = Factory.NewWithValidTestData<EdiPriceHeaderExchangeRate>();

			price1.L7_RX_NKCurrency = "USD";
			price1.L7_Price = 0.25m;
			price1.L7_Code = "USR";
			price1.L7_LicenceUnits = 25;
			link.PHL_VolumeCode = "LV";
			link.PHL_VolumePercent = 55.38m;
			link.PHL_CoreUpliftPercent = 5.22m;
			link.PHL_CorePackCode = "UP";
			rate.PHE_Rate = 1.33m;
			rate.PHE_UpliftPercent = 5.44m;

			CombineAssertions(() =>
			{
				decimal price = 0;
				decimal licenceUnits = 0;

				rate.CalculatePriceAndLicenceUnits(price1, link, false, out price, out licenceUnits);
				AssertEquals(0.29m, price);
				AssertEquals(20.8m, licenceUnits);

				price1.L7_Price = 21.22m;
				price1.L7_LicenceUnits = 210m;
				rate.CalculatePriceAndLicenceUnits(price1, link, false, out price, out licenceUnits);
				AssertEquals(24.7m, price);
				AssertEquals(174.4m, licenceUnits);

				price1.L7_Price = 145.33m;
				price1.L7_LicenceUnits = 1453m;
				rate.CalculatePriceAndLicenceUnits(price1, link, false, out price, out licenceUnits);
				AssertEquals(170m, price);
				AssertEquals(1207m, licenceUnits);

				price1.L7_Price = -221.44m;
				price1.L7_LicenceUnits = -2214m;
				rate.CalculatePriceAndLicenceUnits(price1, link, false, out price, out licenceUnits);
				AssertEquals(-260m, price);
				AssertEquals(-1839.2m, licenceUnits);

				price1.L7_IsVolumeAdjustmentEligible = false;
				price1.L7_Price = 21.22m;
				price1.L7_LicenceUnits = 210m;
				rate.CalculatePriceAndLicenceUnits(price1, link, false, out price, out licenceUnits);
				AssertEquals(44.6m, price);
				AssertEquals(315.0m, licenceUnits);

				price1.L7_IsVolumeAdjustmentEligible = false;
				link.PHL_VolumeCode = "HV";
				link.PHL_VolumePercent = 20m;
				price1.L7_Price = 31.22m;
				price1.L7_LicenceUnits = 310m;
				rate.CalculatePriceAndLicenceUnits(price1, link, true, out price, out licenceUnits);
				AssertEquals(13.1m, price);
				AssertEquals(93.0m, licenceUnits);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateRate(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateRate(Factory);
		}

		static EdiPriceHeaderExchangeRate CreateRate(BusinessObjectFactory factory)
		{
			var org = factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var header = org.LicCompany.PriceHeaders.AddNew();
			header.L6_HasExchangeRates = true;
			var bizo = header.ExchangeRates.AddNew();
			bizo.PHE_RX_NKCurrency = "AUD";
			bizo.PHE_Rate = 1m;
			return bizo;
		}
	}
}
