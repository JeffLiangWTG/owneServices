using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class DatabaseCountryUserSetTest : TestCaseWithFactory
	{
		public void TestGet()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic1b = BillingTestHelper.CreateAnotherLicence(lic1a, "BBB");
			var lic1c = BillingTestHelper.CreateAnotherLicence(lic1a, "CCC");

			lic1a.ClientCompany.LCC_RN_NKCountryCode = "CN";
			lic1b.ClientCompany.LCC_RN_NKCountryCode = "HK";
			lic1c.ClientCompany.LCC_RN_NKCountryCode = "US";

			var periodStart = new ZDateTime(2016, 4, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1a.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1b.ClientCompany, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1c.ClientCompany, 13);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(-1), lic1a.ClientCompany, 99);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(+1), lic1a.ClientCompany, 99);

			var lic2a = BillingTestHelper.CreateLicence(Factory, "D2A");
			lic2a.ClientCompany.LCC_RN_NKCountryCode = "AU";
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2a.ClientCompany, 17);

			var lic3a = BillingTestHelper.CreateLicence(Factory, "TTT");
			var lic3b = BillingTestHelper.CreateAnotherLicence(lic3a, "EEE");
			lic3a.ClientCompany.LCC_RN_NKCountryCode = "AE";
			lic3b.ClientCompany.LCC_RN_NKCountryCode = "OM";
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic3a.ClientCompany, 30);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic3b.ClientCompany, 15);

			Factory.Save();

			var dbSet = new DatabaseCountryUserSet();
			var dbUser = dbSet.Get(periodStart.ToDateTime(), 1, lic1a.LA_LD.ToGuid())[0];
			var userGroup = dbUser.DevelopingRegionUserGroup;
			AssertEquals(2, userGroup.DomesticCompanyCount);
			AssertEquals("CN", userGroup.DomesticCountry);
			AssertEquals(7 + 11, userGroup.DomesticUserCount);
			AssertEquals(2, userGroup.MainCountryCount);
			AssertEquals(3, userGroup.TotalCompanyCount);
			AssertEquals(7 + 11 + 13, userGroup.TotalUserCount);

			dbUser = dbSet.Get(periodStart.ToDateTime(), 3, lic2a.LA_LD.ToGuid())[0];
			userGroup = dbUser.DevelopingRegionUserGroup;
			AssertEquals(1, userGroup.DomesticCompanyCount);
			AssertEquals("AU", userGroup.DomesticCountry);
			AssertEquals(17, userGroup.DomesticUserCount);
			AssertEquals(1, userGroup.MainCountryCount);
			AssertEquals(1, userGroup.TotalCompanyCount);
			AssertEquals(17, userGroup.TotalUserCount);

			dbUser = dbSet.Get(periodStart.ToDateTime(), 1, lic3a.LA_LD.ToGuid())[0];
			userGroup = dbUser.DomesticCountryUserGroup;
			AssertEquals(1, userGroup.DomesticCompanyCount);
			AssertEquals("AE", userGroup.DomesticCountry);
			AssertEquals(30, userGroup.DomesticUserCount);
			AssertEquals(2, userGroup.MainCountryCount);
			AssertEquals(2, userGroup.TotalCompanyCount);
			AssertEquals(30 + 15, userGroup.TotalUserCount);

			userGroup = dbUser.DevelopingRegionUserGroup;
			AssertEquals(2, userGroup.DomesticCompanyCount);
			AssertEquals("AE", userGroup.DomesticCountry);
			AssertEquals(45, userGroup.DomesticUserCount);
			AssertEquals(1, userGroup.MainCountryCount);
			AssertEquals(2, userGroup.TotalCompanyCount);
			AssertEquals(30 + 15, userGroup.TotalUserCount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var list = new CodeDescriptionBoolCollection();
			list.Add("CN", (NoResString)"CN");
			list.Add("HK", (NoResString)"CN");
			list.Add("MO", (NoResString)"CN");
			list.Add("TW", (NoResString)"CN");
			list.Add("AE", (NoResString)"AE", false);
			list.Add("OM", (NoResString)"AE", false);
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}
	}

	public class DatabaseCountryLanguageBillingHelperTest : TestCaseWithFactory
	{
		//DAL means free if all users are in the same country group, and the language of the item is a local language.
		public void TestShouldBillPriceItem_DAL()
		{
			var period = new ZDateTime(2016, 1, 1);
			var licFR = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			licFR.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var priceList = BillingTestHelper.CreatePriceHeader(licFR.Company, BillingConstants.PriceHeaderType.STL, "STL1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price = BillingTestHelper.AddPriceItem(priceList, "C01", "DAL", "", 100m);
			price.L7_Language = Core.SharedConstants.Languages.French;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licFR.ClientCompany, 1);
			AssertHelper(false, price, period, licFR.LA_LD);

			var licUS = BillingTestHelper.CreateLicence(Factory, "EN1", "US1", "DB1");
			licUS.ClientCompany.LCC_RN_NKCountryCode = "US";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licUS.ClientCompany, 3);
			AssertHelper(true, price, period, licFR.LA_LD);
		}

		//Two countries with french as the local language then french item should NOT be free
		public void TestShouldBillPriceItem_DAL_2()
		{
			var countryCodeToLanguages = EDIDataRegistry.Instance.LicenceCountryLanguages.Value
				.OfType<ICodeDescription>()
				.Select(x => new { CountryCode = x.Code, Languages = new HashSet<string>(x.Description.Split(',')) })
				.ToDictionary(x => x.CountryCode, x => x.Languages);

			AssertEquals(true, countryCodeToLanguages["TN"].Contains(Core.SharedConstants.Languages.French));
			AssertEquals(true, countryCodeToLanguages["WF"].Contains(Core.SharedConstants.Languages.French));

			var period = new ZDateTime(2016, 1, 1);
			var licTN = BillingTestHelper.CreateLicence(Factory, "EN1", "TN1", "DB1");
			licTN.ClientCompany.LCC_RN_NKCountryCode = "TN";

			var priceList = BillingTestHelper.CreatePriceHeader(licTN.Company, BillingConstants.PriceHeaderType.STL, "STL1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price = BillingTestHelper.AddPriceItem(priceList, "C01", "DAL", "", 100m);
			price.L7_Language = Core.SharedConstants.Languages.French;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licTN.ClientCompany, 1);
			AssertHelper(false, price, period, licTN.LA_LD);

			var licWF = BillingTestHelper.CreateLicence(Factory, "EN1", "WF1", "DB1");
			licWF.ClientCompany.LCC_RN_NKCountryCode = "WF";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licWF.ClientCompany, 3);
			AssertHelper(true, price, period, licTN.LA_LD);
		}

		//DAZ means free if the country group with most users has the majority of users 
		public void TestShouldBillPriceItem_DAZ()
		{
			var period = new ZDateTime(2016, 1, 1);
			var licCN = BillingTestHelper.CreateLicence(Factory, "EN1", "CN1", "DB1");
			licCN.ClientCompany.LCC_RN_NKCountryCode = "CN";

			var priceList = BillingTestHelper.CreatePriceHeader(licCN.Company, BillingConstants.PriceHeaderType.STL, "STL1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price = BillingTestHelper.AddPriceItem(priceList, "C02", "DAZ", "", 100m);
			price.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licCN.ClientCompany, 5);
			AssertHelper(false, price, period, licCN.LA_LD);

			var licHK = BillingTestHelper.CreateLicence(Factory, "EN1", "HK1", "DB1");
			licHK.ClientCompany.LCC_RN_NKCountryCode = "HK";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licHK.ClientCompany, 6);
			AssertHelper(false, price, period, licCN.LA_LD);

			var licUS = BillingTestHelper.CreateLicence(Factory, "EN1", "US1", "DB1");
			licUS.ClientCompany.LCC_RN_NKCountryCode = "US";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licUS.ClientCompany, 10);
			AssertHelper(false, price, period, licCN.LA_LD);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licUS.ClientCompany, 30);
			AssertHelper(true, price, period, licCN.LA_LD);
		}

		//DAC means free if all users are in the same country group.
		public void TestShouldBillPriceItem_DAC()
		{
			var period = new ZDateTime(2016, 1, 1);
			var licCN = BillingTestHelper.CreateLicence(Factory, "EN1", "CN1", "DB1");
			licCN.ClientCompany.LCC_RN_NKCountryCode = "CN";

			var priceList = BillingTestHelper.CreatePriceHeader(licCN.Company, BillingConstants.PriceHeaderType.STL, "STL1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price = BillingTestHelper.AddPriceItem(priceList, "C02", "DAC", "", 100m);
			price.L7_Language = "";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licCN.ClientCompany, 5);
			AssertHelper(false, price, period, licCN.LA_LD);

			var licHK = BillingTestHelper.CreateLicence(Factory, "EN1", "HK1", "DB1");
			licHK.ClientCompany.LCC_RN_NKCountryCode = "HK";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licHK.ClientCompany, 6);
			AssertHelper(false, price, period, licCN.LA_LD);

			var licUS = BillingTestHelper.CreateLicence(Factory, "EN1", "US1", "DB1");
			licUS.ClientCompany.LCC_RN_NKCountryCode = "US";

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period, licUS.ClientCompany, 30);
			AssertHelper(true, price, period, licCN.LA_LD);
		}

		void AssertHelper(bool expectd, ClientLicencePriceItem priceItem, ZDateTime period, ZGuid databasePk)
		{
			Factory.Save();
			var helper = new DatabaseCountryLanguageBillingHelper();
			var dbSet = new DatabaseCountryUserSet();
			var dbUsers = dbSet.Get(period.ToDateTime(), 1, databasePk.ToGuid());
			AssertEquals(1, dbUsers.Length);
			AssertEquals(expectd, helper.ShouldBillPriceItem(priceItem, dbUsers[0]));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var list = new CodeDescriptionBoolCollection();
			list.Add("CN", (NoResString)"CN");
			list.Add("HK", (NoResString)"CN");
			list.Add("MO", (NoResString)"CN");
			list.Add("TW", (NoResString)"CN");
			list.Add("AE", (NoResString)"AE", false);
			list.Add("BH", (NoResString)"AE", false);
			list.Add("KW", (NoResString)"AE", false);
			list.Add("OM", (NoResString)"AE", false);
			list.Add("QA", (NoResString)"AE", false);
			list.Add("SA", (NoResString)"AE", false);
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}
	}
}
