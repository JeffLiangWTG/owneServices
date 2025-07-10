using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class AccountVerificationControlTest : TestCaseWithFactory
	{
		public void TestActionInstuction()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "COM", "SRV");
			var org = licence.Company.Header;
			org.OH_Code = "SCWAAASYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			var user = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user.EUA_LD = licence.Database.PK;
			user.EUA_OC_WebAccessContact = contact.PK;
			user.EUA_IsContactRelationshipActive = false;
			user.EUA_IsActive = true;
			Factory.Save();
			var page = GetPageForTest(user, contact);
			page.DoPageLoad();
			var label = page.AccountVerification.ActionInstructionAccountReactivatedLabelExposed;
			AssertEquals("Account reactivated instruction label should be shown when EUA_IsContactRelationshipActive is false and no status", true, label.Visible);
			user.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.ProductDeactivation;
			Factory.Save();
			page.DoPageLoad();
			AssertEquals("Account reactivated instruction label should be shown when EUA_IsContactRelationshipActive is false and status is PDA", true, label.Visible);
			var statusList = new ContactRelationshipStatusList().Cast<CodeDescriptionPair>().Where(x => x.Code != ContactRelationshipStatusList.Codes.DistinctEmailRequired);
			foreach (var status in statusList)
			{
				user.EUA_ContactRelationshipStatus = status.Code;
				Factory.Save();
				page.DoPageLoad();
				var instructionVisible = page.AccountVerification.ActionInstructionAccountReactivatedLabelExposed.Visible || page.AccountVerification.ActionInstructionEmailChangedLabelExposed.Visible || page.AccountVerification.ActionInstructionMultipleUIDLinkedLabelExposed.Visible || page.AccountVerification.ActionInstructionContactMovedLabelExposed.Visible;
				Assert($"Instruction should be visible for status {status.Code}", instructionVisible);
			}
		}

		DummyPageForTest GetPageForTest(EdiCustomerUserAccount user, OrgContact contact)
		{
			var page = new DummyPageForTest(user, contact);
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.InitialiseControls();
			return page;
		}
	}
}