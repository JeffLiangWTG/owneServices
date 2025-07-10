using System;
using System.Web.Security.AntiXss;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(ChangeMasterPasswordPerson))]
	sealed class ChangeMasterPasswordPersonTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "BEAVER";
			org.OH_FullName = "Boston";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Avocado";
			contact.OC_Email = "barj@marj.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_Email = "barj@marj.com";
			inactiveContact.OC_ContactName = "Avocado 1";
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = contact.OC_PER;
			var webAccessDisabledContact = org.Contacts.AddNew();
			webAccessDisabledContact.OC_Email = "barj@marj.com";
			webAccessDisabledContact.OC_ContactName = "Avocado 2";
			webAccessDisabledContact.OC_IsActive = true;
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = contact.OC_PER;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "AARDVARK";
			org2.OH_FullName = "Alabama";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = "baraaj@marj.com";
			contact2.OC_ContactName = "Avocado";
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact.Person.PER_FullName = "Avocado";

			Factory.Save();

			var person = ChangeMasterPasswordPerson.New(contact.Person);
			AssertEquals(contact.Person.PER_FullName, person.Name);
			AssertEquals("Should list active contact organisations with email in brackets separated by line breaks. It should be in alphabetical order of org codes.",
				FormattableString.Invariant($"<p><b>{contact2.WorkingAddressCompanyName}</b><br />{contact2.OrganisationCode} - {contact2.OC_Email}</p><p><b>{contact.WorkingAddressCompanyName}</b><br />{contact.OrganisationCode} - {contact.OC_Email}</p>"),
				person.RelatedAccounts);
		}

		public void TestHtmlEncodeWorkingAddressCompanyNameWithinHtmlCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "BEAVER";
			org.OH_FullName = "Boston";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "<svg/onload=alert(document.domain)>";
			contact.OC_Email = "barj@marj.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "AARDVARK";
			org2.OH_FullName = "Alabama";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = "<svg/onload=alert(document.domain)>";
			contact2.OC_ContactName = "Avocado";
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact.Person.PER_FullName = "<svg/onload=alert(document.domain)>";

			Factory.Save();

			var person = ChangeMasterPasswordPerson.New(contact.Person);
			AssertEquals("Should list active contact organisations with email in brackets separated by line breaks. It should be in alphabetical order of org codes.",
				FormattableString.Invariant($"<p><b>{AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(contact2.WorkingAddressCompanyName, false), false)}</b><br />{contact2.OrganisationCode} - {contact2.OC_Email}</p><p><b>{AntiXssEncoder.HtmlEncode(contact.WorkingAddressCompanyName, false)}</b><br />{contact.OrganisationCode} - {contact.OC_Email}</p>"),
				person.RelatedAccounts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ChangeMasterPasswordPerson.New(Factory.NewWithValidTestData<GlbPerson>());
		}
	}
}
