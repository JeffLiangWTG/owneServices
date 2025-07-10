using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EnterpriseUsageTest : TestCaseWithFactory
	{
		public void TestFirstOdplChargeableUsagePeriod()
		{
			var org1Db1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var enterprise1 = org1Db1.Company.LicEnterprise;
			var org1Db2 = BillingTestHelper.CreateAnotherDatabase(org1Db1, "AA2");
			var org1TestDb = BillingTestHelper.CreateAnotherDatabase(org1Db1, "AA3");
			org1TestDb.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			LicenceCompany company2 = org1Db1.Company.LicEnterprise.Companies.AddNew();
			company2.LC_CompanyCode = "BBB";
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise1.PK;
			var org2Db1 = Factory.New<LicenceHeader>();
			org2Db1.LA_LC = company2.PK;
			org2Db1.LA_LD = org1Db1.LA_LD;

			var other1 = BillingTestHelper.CreateLicence(Factory, "ZZZ");

			var firstUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2012, 1, 1), org1Db2, 100);

			var earlierUsageOnTestSystem = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2011, 1, 1), org1TestDb, 100);
			var earlierUsageOnAnotherEnterprise = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2011, 2, 1), other1, 100);
			var earlierUsageOfAnotherModule = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "ACC", new ZDateTime(2011, 6, 1), org1Db1, 100);

			var laterUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2012, 2, 1), org1Db2, 100);
			var laterUsageOnAnotherDb = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2012, 2, 1), org1Db1, 100);
			var laterUsageOnRelatedOrg = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", new ZDateTime(2012, 2, 1), org2Db1, 100);

			Factory.Save();

			var usage = new EnterpriseUsage();
			AssertEquals(new ZDateTime(2012, 1, 1), usage.FirstOdplChargeableUsagePeriod(enterprise1.PK, "COR"));
			AssertEquals("no usage of module", ZDateTime.Empty, usage.FirstOdplChargeableUsagePeriod(enterprise1.PK, "BBB"));
			AssertEquals("ACC module", new ZDateTime(2011, 6, 1), usage.FirstOdplChargeableUsagePeriod(enterprise1.PK, "ACC"));
			AssertEquals("COR module on other enterprise", new ZDateTime(2011, 2, 1), usage.FirstOdplChargeableUsagePeriod(other1.Company.LC_LE, "COR"));
		}

		public void TestCache()
		{
			EnterpriseUsageForTest usage = new EnterpriseUsageForTest();
			var cache = new EnterpriseUsage.Cache(usage);

			ZGuid enterprisePk1 = ZGuid.NewZGuid();
			ZGuid enterprisePk2 = ZGuid.NewZGuid();
			ZGuid enterprisePk3 = ZGuid.NewZGuid();

			AssertCache(usage, cache, enterprisePk1, "COR", new ZDateTime(2012, 1, 1));
			AssertCache(usage, cache, enterprisePk1, "ACC", new ZDateTime(2012, 2, 1));
			AssertCache(usage, cache, enterprisePk1, "FAX", new ZDateTime(2012, 3, 1));

			AssertCache(usage, cache, enterprisePk2, "COR", new ZDateTime(2012, 4, 1));
			AssertCache(usage, cache, enterprisePk2, "ACC", new ZDateTime(2012, 5, 1));
			AssertCache(usage, cache, enterprisePk2, "FAX", new ZDateTime(2012, 6, 1));

			AssertCache(usage, cache, enterprisePk3, "COR", new ZDateTime(2012, 7, 1));
			AssertCache(usage, cache, enterprisePk3, "ACC", new ZDateTime(2012, 8, 1));
			AssertCache(usage, cache, enterprisePk3, "FAX", new ZDateTime(2012, 9, 1));

			AssertEquals("cached", new ZDateTime(2012, 1, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk1, "COR"));
			AssertEquals("cached", new ZDateTime(2012, 2, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk1, "ACC"));
			AssertEquals("cached", new ZDateTime(2012, 3, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk1, "FAX"));

			AssertEquals("cached", new ZDateTime(2012, 4, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk2, "COR"));
			AssertEquals("cached", new ZDateTime(2012, 5, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk2, "ACC"));
			AssertEquals("cached", new ZDateTime(2012, 6, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk2, "FAX"));

			AssertEquals("cached", new ZDateTime(2012, 7, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk3, "COR"));
			AssertEquals("cached", new ZDateTime(2012, 8, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk3, "ACC"));
			AssertEquals("cached", new ZDateTime(2012, 9, 1), cache.FirstOdplChargeableUsagePeriod(enterprisePk3, "FAX"));
		}

		void AssertCache(EnterpriseUsageForTest usage, IEnterpriseUsage cache, ZGuid pk, string moduleCode, ZDateTime expected)
		{
			usage.Date = expected;
			AssertEquals(expected, cache.FirstOdplChargeableUsagePeriod(pk, moduleCode));
			AssertEquals("base interface returns 1999 on extra call", new ZDateTime(1999, 9, 1), usage.FirstOdplChargeableUsagePeriod(pk, moduleCode));
			AssertEquals("cache returns original value on extra call", expected, cache.FirstOdplChargeableUsagePeriod(pk, moduleCode));
		}

		internal class EnterpriseUsageForTest : IEnterpriseUsage
		{
			public ZDateTime Date;

			public ZDateTime FirstOdplChargeableUsagePeriod(ZGuid licEnterprisePk, string moduleCode)
			{
				ZDateTime result = Date;
				Date = new ZDateTime(1999, 9, 1);
				return result;
			}
		}
	}
}