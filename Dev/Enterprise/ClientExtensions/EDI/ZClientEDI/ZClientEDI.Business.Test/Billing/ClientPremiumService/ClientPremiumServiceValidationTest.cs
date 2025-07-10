using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business.Test;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientPremiumServiceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPS_Type()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var prices = BillingTestHelper.CreateStlPriceList(stdCompany, "AAA", "BBB", "CCC", "DDD");
			prices.Items.FindByCode("AAA").L7_Category = BillingConstants.BillingSystem.Service;
			prices.Items.FindByCode("BBB").L7_Category = BillingConstants.BillingSystem.Service;
			prices.Items.FindByCode("CCC").L7_Category = BillingConstants.BillingSystem.Service;

			var today = ZDateTime.Today;
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var db = lic1.Database;

			var priceLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink1.PHL_L6 = prices.PK;
			priceLink1.PHL_RX_NKCurrency = "USD";
			priceLink1.PHL_ValidFrom = today;

			db.LD_HostedLocation = "MEL";
			var premiumService1 = db.PremiumServices.AddNew();

			premiumService1.CPS_Type = "ZZZ";
			AssertHasErrors(premiumService1.CPS_TypeInfo);

			premiumService1.CPS_Type = "";
			AssertHasErrors(premiumService1.CPS_TypeInfo);

			premiumService1.CPS_Type = "DDD";
			AssertHasErrors("Type exists, but not in Service category", premiumService1.CPS_TypeInfo);

			premiumService1.CPS_Type = "BBB";
			AssertNoNotifications(premiumService1.CPS_TypeInfo);

			var premiumService2 = db.PremiumServices.AddNew();
			premiumService2.CPS_Type = "BBB";
			AssertNoNotifications(premiumService1.CPS_TypeInfo);

			premiumService1.CPS_Type = "AAA";
			premiumService2.CPS_Type = "AAA";
			AssertNoNotifications(premiumService2.CPS_TypeInfo);

			db.LD_HostedLocation = Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			premiumService2.CPS_Type = "CCC";
			AssertNoNotifications(premiumService2.CPS_TypeInfo);

			db.LD_HostedLocation = "SYD";
			premiumService2.Validation.ValidateCPS_Type();
			AssertNoNotifications(premiumService1.CPS_TypeInfo);
		}

		public void TestCheckCPS_Units()
		{
			var db = Factory.New<LicenceDatabase>();
			var premiumService1 = db.PremiumServices.AddNew();

			var info = premiumService1.CPS_UnitsInfo;
			premiumService1.CPS_Type = "AAA";
			premiumService1.CPS_Units = 2;
			AssertNoNotifications(info);

			premiumService1.CPS_Units = 1;
			AssertNoNotifications(info);

			premiumService1.CPS_Type = "BBB";
			premiumService1.CPS_Units = 2;
			AssertNoNotifications(info);

			premiumService1.CPS_Units = 0;
			AssertHasErrors(info);

			premiumService1.CPS_Units = -1;
			AssertHasErrors(info);
		}
		public void TestCheckCPS_StartDate()
		{
			var db = Factory.New<LicenceDatabase>();
			var premiumService1 = db.PremiumServices.AddNew();

			premiumService1.CPS_StartDate = new ZDateTime(2010, 01, 01);
			AssertNoErrors(premiumService1.CPS_StartDateInfo);

			premiumService1.CPS_StartDate = new ZDateTime(2010, 01, 01);
			premiumService1.CPS_EndDate = new ZDateTime(2020, 01, 01);
			AssertNoErrors(premiumService1.CPS_StartDateInfo);

			premiumService1.CPS_EndDate = new ZDateTime(2000, 01, 01);
			premiumService1.CPS_StartDate = new ZDateTime(2010, 01, 01);
			AssertHasErrors(premiumService1.CPS_StartDateInfo);
		}

		public void TestCheckCPS_EndDate()
		{
			var db = Factory.New<LicenceDatabase>();
			var premiumService1 = db.PremiumServices.AddNew();

			premiumService1.CPS_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(premiumService1.CPS_EndDateInfo);

			premiumService1.CPS_StartDate = new ZDateTime(2010, 01, 01);
			premiumService1.CPS_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(premiumService1.CPS_EndDateInfo);

			premiumService1.CPS_StartDate = new ZDateTime(2013, 01, 01);
			premiumService1.CPS_EndDate = new ZDateTime(2012, 01, 31);
			AssertHasErrors(premiumService1.CPS_EndDateInfo);
		}

		public void TestCheckDatabaseProduct()
		{
			UsageBillingSettingsTest.SetupValidTestRegistry();
			var db = Factory.New<LicenceDatabase>();
			var premiumService1 = db.PremiumServices.AddNew();

			AssertIsValidProduct(true, ProductTypes.Codes.Enterprise, premiumService1);
			AssertIsValidProduct(true, ProductTypes.Codes.CargoWiseOne, premiumService1);
			AssertIsValidProduct(true, "ABC", premiumService1);
			AssertIsValidProduct(false, ProductTypes.Codes.BorderWise, premiumService1);
			AssertIsValidProduct(false, ProductTypes.Codes.EHub, premiumService1);
		}

		void AssertIsValidProduct(bool expectedisValid, string productCode, ClientPremiumService premiumService)
		{
			premiumService.Database.LD_Product = productCode;
			premiumService.Validation.ValidateAll();
			if (expectedisValid)
			{
				AssertNoErrors(productCode, premiumService.CPS_LDInfo);
			}
			else
			{
				AssertHasErrors(productCode, premiumService.CPS_LDInfo);
			}
		}
	}
}
