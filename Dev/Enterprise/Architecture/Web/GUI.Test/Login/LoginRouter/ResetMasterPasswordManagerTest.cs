using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(ResetMasterPasswordManager))]
	public class ResetMasterPasswordManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPersonsForBinding()
		{
			var commonEmail = "hamilton@island.com";
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_IsActive = true;
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_Email = commonEmail;
			Factory.Save();

			var duplicateEmailContact = Factory.NewWithValidTestData<OrgContact>();
			duplicateEmailContact.OC_IsActive = true;
			duplicateEmailContact.OC_WebAccessEnabled = true;
			duplicateEmailContact.OC_PER = activeContact.OC_PER;
			duplicateEmailContact.OC_Email = commonEmail;
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_Email = commonEmail;
			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_IsActive = true;
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_Email = commonEmail;
			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_IsActive = true;
			unrelatedContact.OC_WebAccessEnabled = true;
			unrelatedContact.OC_Email = "fraser@island.com";
			Factory.Save();

			var manager = new ResetMasterPasswordManager(commonEmail, Factory);
			AssertEquals("Only persons linked to active related web enabled contacts matching the email should be returned", 1, manager.PersonsForBinding.Count);
			AssertEquals("Active contact person", activeContact.OC_PER, manager.PersonsForBinding[0].Person.PK);
		}

		public void TestPersonsForBinding_OrgCodeRestriction()
		{
			var commonEmail = "hamilton@island.com";
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_IsActive = true;
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_Email = commonEmail;
			Factory.Save();

			var duplicateEmailContact = Factory.NewWithValidTestData<OrgContact>();
			duplicateEmailContact.OC_IsActive = true;
			duplicateEmailContact.OC_WebAccessEnabled = true;
			duplicateEmailContact.OC_Email = commonEmail;
			Factory.Save();

			AssertNotEquals("Precondition: Should be in separate organisations", activeContact.OC_OH, duplicateEmailContact.OC_OH);
			AssertNotEquals("Precondition: Should be on separate persons", activeContact.OC_PER, duplicateEmailContact.OC_PER);
			AssertNotEquals("Precondition: Should have an org code", string.Empty, activeContact.OrganisationCode);
			var manager = new ResetMasterPasswordManager(commonEmail, Factory, activeContact.OrganisationCode);
			AssertEquals("Only persons matching the email and org code should be returned", 1, manager.PersonsForBinding.Count);
			AssertEquals("Active contact person", activeContact.OC_PER, manager.PersonsForBinding[0].Person.PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ResetMasterPasswordManager("random@email.com", Factory);
		}
	}
}
