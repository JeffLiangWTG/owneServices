using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.Billing.ODPL.Test;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ModuleUsersTest : TestCaseWithFactory
	{
		public void TestCompanyMonthlyUserCount()
		{
			var periodStart = new ZDateTime(2015, 1, 1);

			var lic1 = BillingTestHelper.CreateLicenceWithPrices(Factory, "AAA", Env.CurrentBranch.PK, "AUD", "AUD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var clientCompany1 = BillingTestHelper.FindOrCreateClientCompany(lic1);
			var clientCompany2 = BillingTestHelper.FindOrCreateClientCompany(lic2);
			BillingTestHelper.CreatePriceList(lic2);
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "WAR", periodStart.AddMonths(-1), clientCompany1, 77);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "WAR", periodStart.AddMonths(-2), clientCompany2, 22);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "ACC", periodStart.AddMonths(-2), clientCompany2, 10);
			Factory.Save();

			// A bill with 7 x COR and 99 x WAR
			OdplUsage usage1 = new OdplUsage(Factory, lic1, periodStart, clientCompany1);
			var module1 = OdplUsageTest.AddModuleUsage(usage1, BillingConstants.CoreModuleCode, 7, 100, 20);
			module1.PurchasedStaffCount = 3;
			var module2 = OdplUsageTest.AddModuleUsage(usage1, "WAR", 99, 100, 20);

			OdplSystemBill bill1 = new OdplSystemBill(Factory);
			bill1.PopulateFromSystemUsages(new[] { usage1 });

			var moduleUsers = new ModuleUsers(new[] { bill1 }, periodStart, ZDateTime.Today);
			AssertEquals(99, moduleUsers.CompanyMonthlyUserCount(clientCompany1, "WAR", periodStart));
			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany1, "WAR", periodStart.AddMonths(1)));
			AssertEquals(77, moduleUsers.CompanyMonthlyUserCount(clientCompany1, "WAR", periodStart.AddMonths(-1)));
			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany1, "ZZZ", periodStart));

			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany2, "WAR", periodStart));
			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany2, "WAR", periodStart.AddMonths(1)));
			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany2, "WAR", periodStart.AddMonths(-1)));
			AssertEquals(22, moduleUsers.CompanyMonthlyUserCount(clientCompany2, "WAR", periodStart.AddMonths(-2)));
			AssertEquals(0, moduleUsers.CompanyMonthlyUserCount(clientCompany2, "ZZZ", periodStart));
		}

		public void TestTotalUserLicenceUnits()
		{
			var periodStart = new ZDateTime(2015, 1, 1);

			var lic1 = BillingTestHelper.CreateLicenceWithPrices(Factory, "AAA", Env.CurrentBranch.PK, "AUD", "AUD");
			var lic2 = BillingTestHelper.CreateDependentLicence(lic1, "BBB");
			var licAnotherDb = BillingTestHelper.CreateAnotherDatabase(lic1, "ZZZ");
			var licNoPrices = BillingTestHelper.CreateLicence(Factory, "CCC");
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;
			var clientOnlyCompany = BillingTestHelper.CreateClientCompany(lic1.Database, "UUU");
			var prices = lic1.Company.PriceHeaders[0];
			prices.L6_LicenceUnitRate = 0.5;
			prices.Items.FindByCode("COR").L7_LicenceUnits = 10;
			prices.Items.FindByCode("WAR").L7_LicenceUnits = 17;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, lic1, 77);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "WAR", periodStart, lic1, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, lic2, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, clientOnlyCompany, 5);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licAnotherDb, 1000);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, licNoPrices, 999);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart.AddMonths(-1), lic1, 500);
			Factory.Save();

			var moduleUsers = new ModuleUsers(ZDateTime.Today);
			AssertEquals((77 + 3 + 5) * 10m + (13 * 17), moduleUsers.TotalUserLicenceUnits(lic1.Database, periodStart));
			AssertEquals(1000 * 10m, moduleUsers.TotalUserLicenceUnits(licAnotherDb.Database, periodStart));
			AssertEquals(500 * 10m, moduleUsers.TotalUserLicenceUnits(lic1.Database, periodStart.AddMonths(-1)));
			AssertEquals(0m, moduleUsers.TotalUserLicenceUnits(licNoPrices.Database, periodStart.AddMonths(-1)));
		}

		public void TestTotalUserLicenceUnits_UsageWithoutClientCompany()
		{
			var periodStart = new ZDateTime(2015, 1, 1);

			var lic1 = BillingTestHelper.CreateLicenceWithPrices(Factory, "AAA", Env.CurrentBranch.PK, "AUD", "AUD", false);
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

			var clientOnlyCompany = BillingTestHelper.CreateClientCompany(lic1.Database, "UUU");
			var clientOnlyCompany2 = BillingTestHelper.CreateClientCompany(lic1.Database, "VVV");
			var prices = lic1.Company.PriceHeaders[0];

			var perDatabasePrice = prices.Items.AddNew();
			perDatabasePrice.L7_Code = "GDE";
			perDatabasePrice.L7_FeeType = BillingConstants.FeeType.Database;
			perDatabasePrice.L7_Price = 1000;
			perDatabasePrice.L7_Description = "German Lang Pack";
			perDatabasePrice.L7_LicenceUnits = 50m;

			prices.L6_LicenceUnitRate = 0.5;
			prices.Items.FindByCode("COR").L7_LicenceUnits = 10;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", periodStart, clientOnlyCompany, 77);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GDE", periodStart, clientOnlyCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GDE", periodStart, clientOnlyCompany2, 7);
			Factory.Save();

			var moduleUsers = new ModuleUsers(ZDateTime.Today);
			AssertEquals(77 * 10m + 1 * 50m, moduleUsers.TotalUserLicenceUnits(lic1.Database, periodStart));
		}
	}
}