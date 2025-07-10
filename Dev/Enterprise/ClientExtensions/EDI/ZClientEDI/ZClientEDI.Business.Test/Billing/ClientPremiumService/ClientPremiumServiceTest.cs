using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientPremiumService))]
	public class ClientPremiumServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetPriceHeader()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);
			var stdLicCompany = stdHeader.Company;

			var oldStlPrices = BillingTestHelper.CreateStlPrices(stdLicCompany, new UsageCodeKey(BillingConstants.BillingSystem.Service, "ST1"));
			oldStlPrices.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			oldStlPrices.L6_PricelistVersion = "STL v1.0";
			var stlPrices = BillingTestHelper.CreateStlPrices(stdLicCompany, new UsageCodeKey(BillingConstants.BillingSystem.Service, "ST1"));
			stlPrices.L6_ValidFrom = new ZDateTime(2018, 9, 1);
			stlPrices.L6_PricelistVersion = "STL v2.0";

			var ldsPriceList = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList.L6_RX_NKCurrency = "AUD";
			ldsPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			var item1 = ldsPriceList.Items.AddNew();
			item1.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "AAA");

			var goldenTaxPriceList = stdLicCompany.PriceHeaders.AddNew();
			goldenTaxPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			goldenTaxPriceList.L6_RX_NKCurrency = "AUD";
			goldenTaxPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			var item2 = goldenTaxPriceList.Items.AddNew();
			item2.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "GTD");

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV");
			BillingTestHelper.CreatePriceLink(lic.Database, oldStlPrices, new ZDateTime(2016, 1, 1));
			BillingTestHelper.CreatePriceLink(lic.Database, stlPrices, new ZDateTime(2018, 9, 1));
			Factory.Save();

			var service = BillingTestHelper.CreatePremiumService(lic.Database, "ST1", new ZDateTime(2018, 9, 1), ZDateTime.Empty);
			AssertNoErrors(service);
			Factory.Save();

			AssertEquals(stlPrices, service.GetPriceHeader());

			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			service.CPS_Type = "AAA";
			AssertNoErrors(service);
			AssertEquals(ldsPriceList.PK, service.GetPriceHeader().PK);

			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;
			service.CPS_Type = "GTD";
			AssertNoErrors(service);
			AssertEquals(goldenTaxPriceList.PK, service.GetPriceHeader().PK);
		}

		public void TestDatabase()
		{
			ClientPremiumService premiumService = Factory.New<ClientPremiumService>();
			AssertNull(premiumService.Database);

			LicenceDatabase parent = Factory.New<LicenceDatabase>();
			premiumService = parent.PremiumServices.AddNew();
			AssertEquals("Parent", parent, premiumService.Database);
		}

		public void TestUsageOwner()
		{
			var premiumService = Factory.New<ClientPremiumService>();
			AssertNull(premiumService.UsageOwner);

			var usageOwner = Factory.New<ClientCompany>();
			var licDb = Factory.New<LicenceDatabase>();
			premiumService.CPS_LD = licDb.PK;
			usageOwner.LCC_LD = licDb.PK;
			premiumService.CPS_LCC = usageOwner.PK;
			AssertEquals(usageOwner, premiumService.UsageOwner);
		}

		public void TestIsDateRangeMatched()
		{
			ClientPremiumService service = Factory.New<ClientPremiumService>();
			ZDateTime billingDate = new ZDateTime(2010, 12, 01);
			AssertEquals("Both dates are empty", true, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = billingDate.AddMonths(-1);
			AssertEquals("Billing date >= Start Date, End Date is empty", true, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = billingDate.AddMonths(1);
			AssertEquals("Start Date > Billing date", false, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = ZDateTime.Empty;
			service.CPS_EndDate = billingDate.AddMonths(1);
			AssertEquals("Start Date is empty, Billing date < End Date", true, service.IsDateRangeMatched(billingDate));

			service.CPS_EndDate = billingDate.AddMonths(-1);
			AssertEquals("End Date < Billing date", false, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = billingDate.AddMonths(-1);
			service.CPS_EndDate = billingDate.AddMonths(1);
			AssertEquals("Start Date <= Billing Date <= End Date", true, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = billingDate;
			service.CPS_EndDate = billingDate.AddMonths(-1);
			AssertEquals("End Date < Billing date", false, service.IsDateRangeMatched(billingDate));

			service.CPS_StartDate = billingDate.AddMonths(1);
			service.CPS_EndDate = ZDateTime.Empty;
			AssertEquals("Start Date > Billing date", false, service.IsDateRangeMatched(billingDate));
		}

		public void TestPropertiesReadOnly()
		{
			ClientPremiumService service = Factory.New<ClientPremiumService>();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in service.ZPropertyInfoHash)
			{
				AssertEquals(false, propertyInfo.ReadOnly);
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in service.ZPropertyInfoHash)
			{
				AssertEquals(true, propertyInfo.ReadOnly);
			}
		}

		public void TestLogChanges()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var db = organisation.LicCompany.ActiveOrAllLicDatabases[0];
			var service = db.PremiumServices.AddNew();

			service.CPS_Type = "AAA";
			service.CPS_StartDate = new ZDateTime(2010, 12, 1);
			Factory.Save();

			ZQuery logQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Premium");
			StmALog[] logs = db.Logs.Find(logQuery);
			AssertEquals("No log for new added", 0, logs.Length);

			service.CPS_Type = "BBB";
			Factory.Save();

			logs = db.Logs.Find(logQuery);
			AssertEquals("Should be one log for changes", 1, logs.Length);

			string expectedMessage = "Premium | Typ:AAA=>BBB | Sta:01-Dec-10";
			AssertEquals(expectedMessage, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);

			service.CPS_Type = "AAA";
			service.CPS_StartDate = new ZDateTime(2011, 12, 1);
			service.CPS_EndDate = new ZDateTime(2020, 12, 31);
			Factory.Save();

			logs = db.Logs.Find(logQuery);
			AssertEquals("Should be 2 logs for changes", 2, logs.Length);

			expectedMessage = "Premium | Typ:BBB=>AAA | Sta:01-Dec-10=>01-Dec-11 | End:=>31-Dec-20";
			AssertEquals(true, logs.Any(x => x.SL_Reference == expectedMessage));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var db = factory.NewWithValidTestData<LicenceDatabase>();
			var service = db.PremiumServices.AddNew();
			service.CPS_StartDate = new ZDateTime(2013, 8, 1);
			return service;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizObj = (ClientPremiumService)base.GetNewBusinessObject();

			if (bizObj.CPS_LD.IsEmpty)
			{
				bizObj.CPS_LD = Factory.New<LicenceDatabase>().PK;
			}

			return bizObj;
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
