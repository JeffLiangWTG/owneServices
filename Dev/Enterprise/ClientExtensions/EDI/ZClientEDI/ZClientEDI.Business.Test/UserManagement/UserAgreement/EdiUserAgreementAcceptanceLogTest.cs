namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EdiUserAgreementAcceptanceLog))]
	public class EdiUserAgreementAcceptanceLogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUserAccount()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			AssertNull(acceptanceLog.UserAccount);

			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			acceptanceLog.EUL_EUA = userAccount.PK;

			AssertNotNull(acceptanceLog.UserAccount);
			AssertEquals("Should match the user account", userAccount.PK, acceptanceLog.UserAccount.PK);
		}

		public void TestUserAgreement()
		{
			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			AssertNull(acceptanceLog.UserAgreement);

			var userAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			acceptanceLog.EUL_ERA = userAgreement.PK;

			AssertNotNull(acceptanceLog.UserAgreement);
			AssertEquals("Should match the user account", userAgreement.PK, acceptanceLog.UserAgreement.PK);
		}

		public void TestLogProperties()
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Contact Name Test";
			userAccount.EUA_OC_WebAccessContact = contact.PK;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "AAA";
			contact.OC_OH = organisation.PK;

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_Product = "CW1";
			database.LD_ServerCode = "BBB";
			database.LD_DatabaseNumber = 9;
			database.LD_TenantID = "Ref025";
			userAccount.EUA_LD = database.PK;

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence.LE_EnterpriseCode = "CCC";
			licence.LE_EnterpriseID = "025";
			database.LD_LE = licence.PK;

			var date = ZDateTime.UtcNow;
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog.EUL_EUA = userAccount.PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = date;

			Factory.Save();

			AssertEquals("AAA", acceptanceLog.OrgCode);
			AssertEquals("Contact Name Test", acceptanceLog.ContactName);
			AssertEquals("CW1", acceptanceLog.Product);
			AssertEquals("BBB", acceptanceLog.ServerCode);
			AssertEquals(9, acceptanceLog.DatabaseNumber);
			AssertEquals("CCC", acceptanceLog.EnterpriseCode);
			AssertEquals("Ref025", acceptanceLog.TenantID);
			AssertEquals(date, acceptanceLog.EUL_AcceptanceTimeUtc);
			AssertEquals(date.ToLocalBranchTime(Factory), acceptanceLog.AcceptanceTimeLocal);
			AssertEquals("CargoWise One", acceptanceLog.ProductName);
			AssertEquals("025", acceptanceLog.EnterpriseId);
		}

		public void TestLogPropertiesForClientAcceptance()
		{
			var staffProxy = Factory.NewWithValidTestData<GlbStaff>();
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "AAA";
			organisation.MainAddress.OA_CompanyNameOverride = "Alpacas Incorporated";
			organisation.OH_FullName = "Aardvark Inc";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_Product = "CW1";
			database.LD_ServerCode = "BBB";
			database.LD_DatabaseNumber = 9;
			database.LD_TenantID = "Ref025";
			database.LD_OH_WebAccessOrg = organisation.PK;

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence.LE_EnterpriseCode = "CCC";
			licence.LE_EnterpriseID = "025";
			database.LD_LE = licence.PK;

			var date = ZDateTime.UtcNow;
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog.EUL_OH = organisation.PK;
			acceptanceLog.EUL_GS = staffProxy.PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = date;

			Factory.Save();

			AssertEquals("AAA", acceptanceLog.OrgCode);
			AssertEquals("Alpacas Incorporated", acceptanceLog.OrgName);
			AssertEquals(string.Empty, acceptanceLog.ContactName);
			AssertEquals(string.Empty, acceptanceLog.Product);
			AssertEquals(string.Empty, acceptanceLog.ServerCode);
			AssertEquals(0, acceptanceLog.DatabaseNumber);
			AssertEquals(string.Empty, acceptanceLog.EnterpriseCode);
			AssertEquals(string.Empty, acceptanceLog.TenantID);
			AssertEquals(date, acceptanceLog.EUL_AcceptanceTimeUtc);
			AssertEquals(date.ToLocalBranchTime(Factory), acceptanceLog.AcceptanceTimeLocal);
			AssertEquals(string.Empty, acceptanceLog.ProductName);
			AssertEquals(string.Empty, acceptanceLog.EnterpriseId);
		}

		public void TestOrgName()
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Contact Name Test";
			userAccount.EUA_OC_WebAccessContact = contact.PK;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "AAA";
			organisation.OH_FullName = "Org Fullname 1";
			contact.OC_OH = organisation.PK;

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_Product = "CW1";
			database.LD_ServerCode = "BBB";
			database.LD_DatabaseNumber = 9;
			database.LD_TenantID = "Ref025";
			userAccount.EUA_LD = database.PK;

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence.LE_EnterpriseCode = "CCC";
			licence.LE_EnterpriseID = "025";
			database.LD_LE = licence.PK;

			var date = ZDateTime.UtcNow;
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog.EUL_EUA = userAccount.PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = date;
			Factory.Save();

			AssertEquals("Org Fullname 1", acceptanceLog.OrgName);

			organisation.MainAddress.OA_Phone = "111111111";
			organisation.MainAddress.OA_Address1 = "100 Fake St";
			organisation.MainAddress.OA_CompanyNameOverride = "Main Address Company Name";
			Factory.Save();

			AssertEquals("Main Address Company Name", acceptanceLog.OrgName);

			var branchAddress = organisation.Addresses.AddNew();
			branchAddress.OA_Phone = "919919";
			branchAddress.OA_CompanyNameOverride = "Branch Address Company Name";
			branchAddress.OA_Address1 = "address branch 1";
			contact.OC_OA_OrgAddress = branchAddress.PK;
			Factory.Save();

			AssertEquals("Branch Address Company Name", acceptanceLog.OrgName);
		}

		public void TestOrgNameForClientAcceptance()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "AAA";
			organisation.OH_FullName = "Org Fullname 1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_Product = "CW1";
			database.LD_ServerCode = "BBB";
			database.LD_DatabaseNumber = 9;
			database.LD_TenantID = "Ref025";
			database.LD_OH_WebAccessOrg = organisation.PK;

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			licence.LE_EnterpriseCode = "CCC";
			licence.LE_EnterpriseID = "025";
			database.LD_LE = licence.PK;

			var date = ZDateTime.UtcNow;
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = Factory.NewWithValidTestData<EdiUserAgreement>().PK;
			acceptanceLog.EUL_OH = organisation.PK;
			acceptanceLog.EUL_GS = Factory.NewWithValidTestData<GlbStaff>().PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = date;
			Factory.Save();

			AssertEquals("Org Fullname 1", acceptanceLog.OrgName);

			organisation.MainAddress.OA_Phone = "111111111";
			organisation.MainAddress.OA_Address1 = "100 Fake St";
			organisation.MainAddress.OA_CompanyNameOverride = "Main Address Company Name";
			Factory.Save();

			AssertEquals("Main Address Company Name", acceptanceLog.OrgName);
		}

		public void TestLongEmail()
		{
			var acceptanceLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_AcceptedByEmail = "alexaalexaalexaalexaalexaalexaalexaalexaalexaalexaalexaalexaalexaalexa@alexa.com";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject(Factory);
		}

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var agreement = factory.NewWithValidTestData<EdiUserAgreement>();
			var userAccount = factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var acceptanceLog = factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = agreement.PK;
			acceptanceLog.EUL_EUA = userAccount.PK;

			return acceptanceLog;
		}
	}
}
