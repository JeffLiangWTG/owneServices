using System;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class AccreditationLinkCreatorTest : TestCaseWithFactory
	{
		public void TestGetUrl_NotLoggedIn()
		{
			OrgContactWebUser webUser = new OrgContactWebUser();
			AssertEquals("", AccreditationLinkCreator.GetUrl(webUser));
		}

		public void TestGetUrl_SuperUser()
		{
			WebDataRegistry.Instance.WebCertificationUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cert.cargowise.com");
			OrgContactWebUser webUser = new OrgContactWebUser();
			webUser.LoginSupportForTest("ASALOG");
			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(OrgHeaderSchema.Constants.OH_Code, webUser.LoggedInOrganisation.OH_Code);
			queryString.Add(OrgContactSchema.Constants.Prefix, ZGuid.Missing.ToString());
			string expectedUrl = string.Format("http://www.cert.cargowise.com/default.aspx?{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, AccreditationLinkCreator.GetUrl(webUser));
		}

		public void TestGetUrl()
		{
			OrgHeader header = OrgHeader.LoadFromCode(Factory, "ASALOG");
			OrgContact contact = header.Contacts.AddNew();
			contact.FillWithValidTestData();
			contact.OC_Email = "newuser@cargowise.com";
			contact.SetHashedPassword("test123");
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			WebDataRegistry.Instance.WebCertificationUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cert.cargowise.com");
			OrgContactWebUser webUser = new OrgContactWebUser();
			webUser.Login("ASALOG", "newuser@cargowise.com", "test123");
			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(OrgHeaderSchema.Constants.OH_Code, webUser.LoggedInOrganisation.OH_Code);
			queryString.Add(OrgContactSchema.Constants.Prefix, contact.PK.ToString());
			string expectedUrl = string.Format("http://www.cert.cargowise.com/default.aspx?{0}={1}", SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, AccreditationLinkCreator.GetUrl(webUser));
		}
	}
}
