using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	class LoginServiceTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestGetAutoLoginUrl()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
				licEnterprise.LE_EnterpriseCode = "_X1";
				licEnterprise.LE_OH = org.PK;
				var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
				licCompany.LC_LE = licEnterprise.PK;
				licCompany.LC_CompanyCode = "_X2";
				licCompany.LC_OH = org.PK;
				var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licDatabase.LD_LE = licEnterprise.PK;
				licDatabase.LD_ServerCode = "_X3";
				licDatabase.LD_OH_WebAccessOrg = org.PK;
				var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
				licHeader.LA_LC = licCompany.PK;
				licHeader.LA_LD = licDatabase.PK;
				Factory.Save();
				var xsdContact = new Xsd.OrgContact { EmailAddress = "newuser@cargowise.com", Name = "Lindsay Lohan" };
				var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = "_X1_X2_X3", [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(xsdContact) };
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetAutoLoginUrlWithReturnUrl(queryString.ToString(), "http://www.cargowise.com/eLearning.aspx"));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx", uri.AbsoluteUri);
				var resultQueryString = new QueryString(uri.Query);
				var resultSecureQueryString = new SecureQueryString(resultQueryString["?" + SecureQueryString.QueryStringKey]);
				ZGuid.TryParse(resultSecureQueryString[OrgContactSchema.Constants.Prefix], out ZGuid contactPK);
				var contact = Factory.Load<OrgContact>(contactPK);
				Assert(contact.IsInDatabase);
				AssertEquals("newuser@cargowise.com", contact.OC_Email);
				AssertEquals("Lindsay Lohan", contact.OC_ContactName);
				AssertEquals(org.PK, contact.OC_OH);
				AssertEquals("http://www.cargowise.com/eLearning.aspx", resultSecureQueryString["ReturnURL"]);
			}
		}

		[HttpContextEnabledTest]
		public void TestGetAutoLoginUrl_ClientCompany()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DDDCOMSYD";
				var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = "DDD";
				enterprise.LE_OH = org.PK;
				var database = Factory.NewWithValidTestData<LicenceDatabase>();
				database.LD_LE = enterprise.PK;
				database.LD_ServerCode = "PRD";
				database.LD_DatabaseNumber = 2200;
				database.LD_OH_WebAccessOrg = org.PK;
				Factory.Save();
				var xsdContact = new Xsd.OrgContact();
				xsdContact.EmailAddress = "newuser@cargowise.com";
				xsdContact.Name = "Lindsay Lohan";
				var queryString = new SecureQueryString();
				queryString[StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = "DDDSYDPRD";
				queryString[StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = "2200";
				queryString[StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(xsdContact);
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetAutoLoginUrl(queryString.ToString()));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx", uri.AbsoluteUri);
			}
		}

		[HttpContextEnabledTest]
		public void TestGetContactAutoLoginUrlWithReturnUrl()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				Factory.Save();
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetContactAutoLoginUrlWithReturnUrl(contact.PK.ToGuid(), "http://www.cargowise.com/eLearning.aspx"));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount", uri.AbsoluteUri);
				var resultQueryString = new QueryString(uri.Query);
				var resultSecureQueryString = new SecureQueryString(resultQueryString["?" + SecureQueryString.QueryStringKey]);
				ZGuid.TryParse(resultSecureQueryString[OrgContactSchema.Constants.Prefix], out ZGuid contactPK);
				AssertEquals(contact.PK, contactPK);
				AssertEquals("http://www.cargowise.com/eLearning.aspx", resultSecureQueryString["ReturnURL"]);
			}
		}

		[HttpContextEnabledTest]
		public void TestGetAutoLoginUrl_NotAllowed()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "DDDCOMSYD";
				var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = "DDD";
				enterprise.LE_OH = org.PK;
				var database = Factory.NewWithValidTestData<LicenceDatabase>();
				database.LD_LE = enterprise.PK;
				database.LD_ServerCode = "PRD";
				database.LD_DatabaseNumber = 2200;
				database.LD_AllowAutoLogin = false;
				Factory.Save();
				var xsdContact = new Xsd.OrgContact { EmailAddress = "newuser@cargowise.com", Name = "New User" };
				var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = "DDDSYDPRD", [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = "2200", [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(xsdContact) };
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetAutoLoginUrl(queryString.ToString()));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount", uri.AbsoluteUri);
			}
		}

		[HttpContextEnabledTest]
		public void TestGetAutoLoginUrlWithUserAccountPK()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var licEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
				licEnterprise.LE_EnterpriseCode = "_X1";
				licEnterprise.LE_OH = org.PK;
				var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
				licCompany.LC_LE = licEnterprise.PK;
				licCompany.LC_CompanyCode = "_X2";
				licCompany.LC_OH = org.PK;
				var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licDatabase.LD_LE = licEnterprise.PK;
				licDatabase.LD_ServerCode = "_X3";
				licDatabase.LD_OH_WebAccessOrg = org.PK;
				var licHeader = Factory.NewWithValidTestData<LicenceHeader>();
				licHeader.LA_LC = licCompany.PK;
				licHeader.LA_LD = licDatabase.PK;
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "Lindsay Lohan";
				contact.OC_Email = "newuser@cargowise.com";
				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_UserID = "U01";
				userAccount.EUA_LD = licDatabase.PK;
				userAccount.EUA_OC_WebAccessContact = contact.PK;
				Factory.Save();
				var xsdContact = new Xsd.OrgContact { EmailAddress = "newuser@cargowise.com", Name = "Lindsay Lohan" };
				var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = "_X1_X2_X3", [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(xsdContact), [StaffContactValueObjectHelper.QueryStringKeys.StaffCode] = "U01", };
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetAutoLoginUrlWithReturnUrl(queryString.ToString(), "http://www.cargowise.com/eLearning.aspx"));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx", uri.AbsoluteUri);
				var resultQueryString = new QueryString(uri.Query);
				var resultSecureQueryString = new SecureQueryString(resultQueryString["?" + SecureQueryString.QueryStringKey]);
				ZGuid.TryParse(resultSecureQueryString[EdiCustomerUserAccountSchema.Constants.Prefix], out ZGuid userAccountPK);
				AssertEquals(userAccountPK, userAccount.PK);
				AssertEquals("http://www.cargowise.com/eLearning.aspx", resultSecureQueryString["ReturnURL"]);
			}
		}

		[HttpContextEnabledTest]
		public void TestGetAutoLoginUrl_NoStaffCode()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance))
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", true);
				var database = licence.Database;
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				var org = licence.Company.Header;
				org.Contacts.RemoveAndDeleteAll();
				Factory.Save();
				var contactXsd = new Xsd.OrgContact { Name = "Lindsay Lohan", EmailAddress = "newuser@cargowise.com" };
				var queryString = new SecureQueryString { [StaffContactValueObjectHelper.QueryStringKeys.LicenceCode] = licence.LicenceCode, [StaffContactValueObjectHelper.QueryStringKeys.DatabaseNumber] = licence.Database.LD_DatabaseNumber.ToString(), [StaffContactValueObjectHelper.QueryStringKeys.ContactData] = ValueObjectEncoder.Serialize(contactXsd), };
				var loginService = new LoginService();
				var uri = new Uri(loginService.GetAutoLoginUrl(queryString.ToString()));
				AssertStartsWith("AbsoluteUri", "https://myaccount-portal.cargowise.com/myaccount", uri.AbsoluteUri);
			}
		}
	}
}
