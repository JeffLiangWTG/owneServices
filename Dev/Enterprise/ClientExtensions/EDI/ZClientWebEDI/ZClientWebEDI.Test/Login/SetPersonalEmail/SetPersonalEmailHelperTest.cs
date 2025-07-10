using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(SetPersonalEmailHelper))]
	[HttpContextEnabledTest]
	internal class SetPersonalEmailHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSavePersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			var otherContact = org.Contacts.AddNew();
			otherContact.OC_ContactName = "OtherUser";
			otherContact.OC_Email = "otheruser@cargowise.com";
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_PER = newContact.OC_PER;
			Factory.Save();
			var page = GetPageForTest();
			var helper = new SetPersonalEmailHelper(Factory, page);
			page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			var result = helper.SaveEmail(newContact, string.Empty);
			AssertEquals("The personal email address cannot be empty.", result.Item2);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			result = helper.SaveEmail(newContact, "newuser");
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("The personal email address is invalid.", result.Item2);
			result = helper.SaveEmail(newContact, new string('a', GlbPersonSchema.PER_EmailAddress.MaxLength + 1));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(FormattableString.Invariant($"The length of personal email address exceeds the max length {GlbPersonSchema.PER_EmailAddress.MaxLength}"), result.Item2);
			result = helper.SaveEmail(newContact, "newuser@cargowise.com");
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("The entered email is already set as a login email. A personal recovery email must be different.", result.Item2);
			result = helper.SaveEmail(newContact, "otheruser@cargowise.com");
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("The entered email is already set as a login email. A personal recovery email must be different.", result.Item2);
			result = helper.SaveEmail(newContact, "newuser@gmail.com");
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("An email was sent to newuser@gmail.com to confirm personal email.", result.Item2);
		}

		public void TestSavePersonalEmail_ContactWithoutPerson()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.OC_PER = ZGuid.Empty;
			var page = GetPageForTest();
			var helper = new SetPersonalEmailHelper(Factory, page);
			page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			var result = helper.SaveEmail(newContact, "newuser@gmail.com");
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("An email was sent to newuser@gmail.com to confirm personal email.", result.Item2);
		}

		#region Implementation
		Profile GetPageForTest()
		{
			var page = new ProfileForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			return page;
		}

		class ProfileForTest : Profile
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			var page = GetPageForTest();
			return new SetPersonalEmailHelper(Factory, page);
		}
		#endregion
		#endregion
	}
}
