using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(UserAccountDeactivationManager))]
	public class UserAccountDeactivationManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContactGroupingsShouldOnlyIncludeActiveWebAccessContactsWithRelatedUserAccounts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.Header.OH_Code = "ZEBRA";
			contact1.OC_Email = "ze@bra.com";
			var userAccount1 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.Header.OH_Code = "AARDVARK";
			contact2.OC_Email = "aard@vark.com";
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			var userAccount3 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount3.EUA_OC_WebAccessContact = inactiveContact.PK;
			var nonWebAccessContact = Factory.NewWithValidTestData<OrgContact>();
			nonWebAccessContact.OC_IsActive = true;
			nonWebAccessContact.OC_WebAccessEnabled = false;
			var userAccount4 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount4.EUA_OC_WebAccessContact = nonWebAccessContact.PK;
			var nonUserAccountContact = Factory.NewWithValidTestData<OrgContact>();
			nonUserAccountContact.OC_IsActive = true;
			nonUserAccountContact.OC_WebAccessEnabled = true;
			var contactWithInactiveUserAccount = Factory.NewWithValidTestData<OrgContact>();
			contactWithInactiveUserAccount.OC_IsActive = true;
			contactWithInactiveUserAccount.OC_WebAccessEnabled = true;
			var inactiveUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			inactiveUserAccount.EUA_OC_WebAccessContact = contactWithInactiveUserAccount.PK;
			inactiveUserAccount.EUA_IsActive = false;
			contact2.OC_PER = contact1.OC_PER;
			inactiveContact.OC_PER = contact1.OC_PER;
			nonWebAccessContact.OC_PER = contact1.OC_PER;
			nonUserAccountContact.OC_PER = contact1.OC_PER;
			contactWithInactiveUserAccount.OC_PER = contact1.OC_PER;
			Factory.Save();
			var deactivationManager = new UserAccountDeactivationManager(contact1.Person);
			var databaseTypes = new DatabaseTypes();
			AssertEquals(4, deactivationManager.ContactUserAccountWrappers.Count);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", FormattableString.Invariant($"{contact2.OrgCode} - {contact2.WorkingAddressCompanyName} ({contact2.OC_Email})"), deactivationManager.ContactUserAccountWrappers[0].Organisation);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", string.Empty, deactivationManager.ContactUserAccountWrappers[0].LicenceType);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", string.Empty, deactivationManager.ContactUserAccountWrappers[1].Organisation);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", databaseTypes.GetDescriptionFromCode(userAccount2.Database.LD_LicenceType), deactivationManager.ContactUserAccountWrappers[1].LicenceType);
			AssertEquals("Should add reference number", 0, ((UserAccountDeactivationWrapper)deactivationManager.ContactUserAccountWrappers[1]).ReferenceNumber);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", FormattableString.Invariant($"{contact1.OrgCode} - {contact1.WorkingAddressCompanyName} ({contact1.OC_Email})"), deactivationManager.ContactUserAccountWrappers[2].Organisation);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", string.Empty, deactivationManager.ContactUserAccountWrappers[2].LicenceType);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", string.Empty, deactivationManager.ContactUserAccountWrappers[3].Organisation);
			AssertEquals("Should contain active web access contacts with associated user account(s) in alphabetical order by org code", databaseTypes.GetDescriptionFromCode(userAccount1.Database.LD_LicenceType), deactivationManager.ContactUserAccountWrappers[3].LicenceType);
			AssertEquals("Should add reference number", 1, ((UserAccountDeactivationWrapper)deactivationManager.ContactUserAccountWrappers[3]).ReferenceNumber);
		}

		public void TestConstructor_NullPerson_ShouldNotThrowException()
		{
			AssertNoExceptionThrown(() => new UserAccountDeactivationManager(null));
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UserAccountDeactivationManager(Factory.NewWithValidTestData<GlbPerson>());
		}
		#endregion
	}
}
