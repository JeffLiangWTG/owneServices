using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class FreeTrialsTest : TestCaseWithFactory
	{
		public void TestFreeTrialCreate()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR", "CES");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "COM", "SRV");
			lic1.LA_AgreedLiveDate = new ZDateTime(2018, 1, 1);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "COM", "SRV");
			lic2.LA_AgreedLiveDate = new ZDateTime(2018, 1, 1);

			var discountTypes = new CodeDescriptionBoolCollection(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength);
			discountTypes.Add("SOMEFREETRIAL", (NoResString)"desc");
			EDIDataRegistry.Instance.StlDiscountTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discountTypes);

			var trialConfig = new CodeDescriptionPairList();
			trialConfig.AddPair("SOMEFREETRIAL", "CSC,CES");
			trialConfig.AddPair("SOMEFREETRIAL", "CSU,CSU");
			EDIDataRegistry.Instance.StlFreeTrialDiscounts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trialConfig);

			BillingTestHelper.CreateChargeableUsage(Factory, "CSC", "CES", new ZDateTime(2019, 2, 1), lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "CSC", "CES", new ZDateTime(2019, 2, 1), lic2.ClientCompany, 5);

			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "CES", new ZDateTime(2019, 2, 15, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "CES", new ZDateTime(2019, 2, 16, 0, 0, 0), lic2, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);

			var logger = new SimpleLogger();

			var freeTrials = new FreeTrials();
			freeTrials.CreateFreeTrialsFromNewUsage(new DateTime(2019, 2, 1), logger);
			var logText = logger.ToString();
			AssertContains(@"Free trial discount SOMEFREETRIAL created for server EN1-SRV 2019-02-01 to 2019-02-28", logText);
			AssertContains(@"Free trial discount SOMEFREETRIAL created for server EN2-SRV 2019-02-01 to 2019-03-31", logText);
			{
				AssertEquals(1, lic1.Database.LicenceSettings.Count);
				AssertType<DiscountLicenceSetting>(lic1.Database.LicenceSettings[0]);
				var setting1 = (DiscountLicenceSetting)lic1.Database.LicenceSettings[0];
				AssertEquals("SOMEFREETRIAL", setting1.LS9_Name);
				AssertEquals(new ZDateTime(2019, 2, 1), setting1.LS9_ValidFrom);
				AssertEquals(new ZDateTime(2019, 2, 28), setting1.LS9_ValidTo);
			}
			{
				AssertEquals(1, lic2.Database.LicenceSettings.Count);
				AssertType<DiscountLicenceSetting>(lic2.Database.LicenceSettings[0]);
				var setting2 = (DiscountLicenceSetting)lic2.Database.LicenceSettings[0];
				AssertEquals("SOMEFREETRIAL", setting2.LS9_Name);
				AssertEquals(new ZDateTime(2019, 2, 1), setting2.LS9_ValidFrom);
				AssertEquals(new ZDateTime(2019, 3, 31), setting2.LS9_ValidTo);
			}

			logger = new SimpleLogger();
			freeTrials.CreateFreeTrialsFromNewUsage(new DateTime(2019, 2, 1), logger);
			freeTrials.CreateFreeTrialsFromNewUsage(new DateTime(2019, 2, 1), logger);
			lic1.Database.LicenceSettings.RefreshFromDb();
			AssertEquals("now new trials if called again", 1, lic1.Database.LicenceSettings.Count);
			AssertEquals("", logger.ToString());
		}

		public void TestFreeTrialCreate_NotLive()
		{
			var licHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var licCompany = licHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(licCompany, "USR", "CES");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "COM", "SRV");
			lic1.LA_AgreedLiveDate = new ZDateTime(2019, 3, 1);

			var discountTypes = new CodeDescriptionBoolCollection(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength);
			discountTypes.Add("SOMEFREETRIAL", (NoResString)"desc");
			EDIDataRegistry.Instance.StlDiscountTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discountTypes);

			var trialConfig = new CodeDescriptionPairList();
			trialConfig.AddPair("SOMEFREETRIAL", "CSC,CES");
			trialConfig.AddPair("SOMEFREETRIAL", "CSU,CSU");
			EDIDataRegistry.Instance.StlFreeTrialDiscounts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trialConfig);

			BillingTestHelper.CreateChargeableUsage(Factory, "CSC", "CES", new ZDateTime(2019, 2, 1), lic1.ClientCompany, 5);

			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CSC", "CES", new ZDateTime(2019, 2, 15, 0, 0, 0), lic1, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);

			var freeTrials = new FreeTrials();
			freeTrials.CreateFreeTrialsFromNewUsage(new DateTime(2019, 2, 1), null);
			AssertEquals(0, lic1.Database.LicenceSettings.Count);
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
	}
}