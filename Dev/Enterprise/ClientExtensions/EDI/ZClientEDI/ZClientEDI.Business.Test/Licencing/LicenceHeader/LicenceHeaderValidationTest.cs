using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	internal class LicenceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLA_AgreedLiveDate()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic, "BBB");
			lic.Database.LD_Product = ProductTypes.Codes.Enterprise;
			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Production;
			lic.Database.LD_OH_BillingParty = lic.Company.LC_OH;
			lic.LA_AgreedLiveDate = ZDateTime.Empty;
			var prices = lic.Company.PriceHeaders.AddNew();
			var now = ZDateTime.Now;
			var firstDay = new DateTime(now.Year, now.Month, 1);

			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertNoNotifications("Not mandatory if no STL price link", lic.LA_AgreedLiveDateInfo);

			var priceLink = lic.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = prices.PK;
			priceLink.PHL_ValidFrom = firstDay;

			lic.Database.LD_Product = "ZZZ";
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertNoNotifications("Not mandatory if not ENT/CW1", lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_Product = ProductTypes.Codes.Enterprise;
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertHasErrors("Mandatory if no STL price link for ENT", lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertHasErrors("Mandatory if no STL price link for CW1", lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_IsActive = false;
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertNoNotifications("Not mandatory if db inactive", lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_IsActive = true;
			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertNoNotifications("No notification for test system", lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Production;
			lic.LA_AgreedLiveDate = firstDay;
			AssertNoNotifications("No notification if value given", lic.LA_AgreedLiveDateInfo);
			lic.LA_AgreedLiveDate = new DateTime(now.Year, now.Month, 2);
			AssertHasErrors("Date must be the first day of the month", lic.LA_AgreedLiveDateInfo);

			Factory.Save();
			lic.Validation.ValidateLA_AgreedLiveDate();
			AssertNoNotifications(lic.LA_AgreedLiveDateInfo);
			lic.LA_AgreedLiveDate = new DateTime(now.Year, now.Month, 3);
			AssertHasErrors("Date must be the first day of the month", lic.LA_AgreedLiveDateInfo);
			lic.LA_AgreedLiveDate = firstDay;
			AssertNoNotifications(lic.LA_AgreedLiveDateInfo);

			lic.Database.LD_OH_BillingParty = lic2.Company.LC_OH;
			lic.LA_AgreedLiveDate = ZDateTime.Empty;
			AssertNoNotifications("No notification if not owner", lic.LA_AgreedLiveDateInfo);
		}

		public void TestLA_SupportStartDateValidation()
		{
			var testHeader = Factory.NewWithValidTestData<LicenceHeader>();
			testHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;

			AssertNotEquals(testHeader.Modules.Count, 0);
			AssertNotNull("Precondition: FindByCode(LegacyLicence.Codes.Core) should not return null", testHeader.Modules.FindByCode(LegacyLicence.Codes.Core));
			AssertNoErrors("Support Start Date should not be empty", testHeader.LA_SupportStartDateInfo);

			testHeader.Modules.FindByCode(LegacyLicence.Codes.Core).LM_LicenceType = LicenceTypes.Codes.PUR;
			testHeader.LA_SupportStartDate = ZDate.Empty;
			AssertHasError(testHeader.LA_SupportStartDateInfo, "Please enter a Date Support Start.");
			testHeader.LA_SupportStartDate = ZDate.Today;
			AssertNoErrors("Support Start Date should not be empty", testHeader.LA_SupportStartDateInfo);
		}

		public void TestAllowDatesInThePast()
		{
			var past = ZDateTime.Today.AddMonths(-7);
			LicenceHeader testHeader = Factory.New<LicenceHeader>();
			testHeader.LA_InstallationStartDate = past;
			testHeader.LA_AgreedLiveDate = new ZDateTime(past.Year, past.Month, 1);
			testHeader.LA_InstallationCompleteDate = past;
			testHeader.LA_ContractExpiryDate = past;
			testHeader.LA_SiteLiveDate = past;
			testHeader.LA_SupportStartDate = past;
			testHeader.LA_LastFaxReport = past;

			AssertNoNotifications(testHeader.LA_InstallationStartDateInfo);
			AssertNoNotifications(testHeader.LA_AgreedLiveDateInfo);
			AssertNoNotifications(testHeader.LA_InstallationCompleteDateInfo);
			AssertNoNotifications(testHeader.LA_ContractExpiryDateInfo);
			AssertNoNotifications(testHeader.LA_SiteLiveDateInfo);
			AssertNoNotifications(testHeader.LA_SupportStartDateInfo);
			AssertNoNotifications(testHeader.LA_LastFaxReportInfo);
		}

		public void TestSupportMode()
		{
			LicenceHeader testHeader = Factory.New<LicenceHeader>();
			testHeader.LA_SupportMode = "";
			AssertHasErrors("Support Mode Has errors", testHeader.LA_SupportModeInfo);

			testHeader.LA_SupportMode = testHeader.Lookups.SupportModeList[0].Code;
			AssertNoErrors("Support Mode Has no errors", testHeader.LA_SupportModeInfo);

			testHeader.LA_SupportMode = "XXX";
			AssertHasErrors("Support Mode Has errors", testHeader.LA_SupportModeInfo);
		}

		public void TestAMSUSMode()
		{
			LicenceHeader testHeader = Factory.New<LicenceHeader>();
			testHeader.LA_AMS_USMode = "";
			AssertNoErrors("Support Mode Has no errors", testHeader.LA_AMS_USModeInfo);

			testHeader.LA_AMS_USMode = "XXX";
			AssertHasErrors("Support Mode Has errors", testHeader.LA_AMS_USModeInfo);

			testHeader.LA_AMS_USMode = testHeader.Lookups.AMSModeList[0].Code;
			AssertNoErrors("Support Mode Has no errors", testHeader.LA_AMS_USModeInfo);
		}

		public void TestLicenceAdvStdOthValidationHasErr()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			var testHeader = Factory.NewWithValidTestData<LicenceHeader>();
			testHeader.Database.LD_Product = "AAA";
			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OtherLegacyApplication;
			AssertNoErrors("LA_LicenceAdvStdOthInfo should have no error", testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.Database.LD_Product = "AAA";
			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			AssertNoErrors("LA_LicenceAdvStdOthInfo should have no error", testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			testHeader.Validation.ValidateLA_LicenceAdvStdOth();
			AssertNoErrors("LA_LicenceAdvStdOthInfo should have no error", testHeader.LA_LicenceAdvStdOthInfo);
		}

		public void TestAdvStdOthValidation()
		{
			LicenceHeader testHeader = Factory.New<LicenceHeader>();
			testHeader.LA_LicenceAdvStdOth = "";
			AssertHasErrors(testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			AssertNoErrors(testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.LA_LicenceAdvStdOth = "XXX";
			AssertHasErrors(testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertNoNotifications(testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConversionToODPL;
			AssertNoNotifications(testHeader.LA_LicenceAdvStdOthInfo);

			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;

			((INeedRow)testHeader).Row.AcceptChanges();
			Assert(testHeader.IsInDatabase);

			testHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConversionToODPL;
			AssertHasError(testHeader.LA_LicenceAdvStdOthInfo, "Inactive Edition");
		}
	}
}