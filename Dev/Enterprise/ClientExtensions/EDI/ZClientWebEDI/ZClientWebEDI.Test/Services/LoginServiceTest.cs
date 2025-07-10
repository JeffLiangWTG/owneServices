using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.WebSecurityRight;

namespace Enterprise.ZClientWebCargoWiseEDI.Services.Testing
{
	class LoginServiceTest : TestCaseWithFactory
	{
		public void TestLogin()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDAAASYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "sam@test.com";
			contact.SetHashedPassword("1234");
			Factory.Save();
			var service = new LoginServiceForTest();
			var result = service.Login("DDDAAASYD", "sam@test.com", "1234");
			AssertEquals(contact.PK, result.ContactPk);
			AssertEquals(false, result.IsSuperUser);
			result = service.Login("DDDAAASYD", "sam@test.com", "4444");
			AssertEquals(Guid.Empty, result.ContactPk);
			AssertEquals(false, result.IsSuperUser);
			result = service.Login("DDDAAASYD", User.SupportUserName, CWSupportLoginToken.TokenForTest);
			AssertNotEquals(Guid.Empty, result.ContactPk);
			AssertEquals(true, result.IsSuperUser);
			result = service.Login("DDDAAASYD", "sam@test.com", "1234");
			AssertEquals(contact.PK, result.ContactPk);
			AssertEquals(false, result.IsSuperUser);
		}

		public void TestHasSecurityRight()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDAAASYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "sam@test.com";
			contact.SetHashedPassword("1234");
			Factory.Save();
			var service = new LoginServiceForTest();
			bool result = service.HasSecurityRight(contact.PK.ToGuid(), false, EDIWebSecurityRightsList.CustomerService.Code);
			AssertEquals("Security right is not granted", false, result);
			OrgSecurity orgRight = org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.CustomerService.Code))[0] as OrgSecurity;
			ZQuery contactRightQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight.PK);
			contactRightQuery.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact.PK);
			OrgSecurityContacts contactRight = contact.SecurityRightsForBindingOnly.Find(contactRightQuery)[0] as OrgSecurityContacts;
			orgRight.OX_Granted = true;
			contactRight.OZ_Granted = true;
			WebSecurityRight webOrgRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);
			WebSecurityRight webContactRight = new WebSecurityRight(contactRight.Security.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);
			Factory.Save();
			result = service.HasSecurityRight(contact.PK.ToGuid(), false, EDIWebSecurityRightsList.CustomerService.Code);
			AssertEquals("Security right is granted", true, result);
			result = service.HasSecurityRight(Guid.Empty, true, EDIWebSecurityRightsList.CustomerService.Code);
			AssertEquals("Super user always has security right granted", true, result);
			result = service.HasSecurityRight(contact.PK.ToGuid(), false, "AAA");
			AssertEquals(false, result);
			AssertEquals("Security right does not exist", service.LastErrorMessage);
			result = service.HasSecurityRight(Guid.Empty, false, EDIWebSecurityRightsList.CustomerService.Code);
			AssertEquals(false, result);
			AssertEquals("Contact does not exist", service.LastErrorMessage);
		}

		#region Implementation
		class LoginServiceForTest : LoginService
		{
			protected override void HandleError(string errorMessage, string errorDetail = "")
			{
				LastErrorMessage = errorMessage;
				LastErrorDetail = errorDetail;
			}

			public string LastErrorMessage { get; private set; }

			public string LastErrorDetail { get; private set; }

			protected override void ValidateRequestIpAddress()
			{
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EDIWebSecurityRightsList.RegisterThisSubTypeOverride();
		}
		#endregion
	}
}
