using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSResponsiblePartyAddressRequirementTest : TestCaseWithFactory
	{
		public void TestResponsiblePartyMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			const string message = "You have not entered a COLS Responsible Party";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("Responsible Party not specified", responsibleParty.OrganisationPKInfo, message);

			responsibleParty.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrorContaining("Responsible Party specified", responsibleParty.OrganisationPKInfo, message);
		});

		public void TestOrgContactMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.Validation.ValidateContactPK();
			AssertHasMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, MandatoryValidation.YouHaveNotEntered);

			responsibleParty.E2_Contact = contact.OC_ContactName;
			AssertNoMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, MandatoryValidation.YouHaveNotEntered);
		});

		public void TestOrgContactMustBeSelected() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			Factory.Save();

			const string message = "Need to select a Contact.";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_Contact = "XYZ";
			AssertHasMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, message);

			responsibleParty.E2_Contact = "John Smith";
			AssertNoMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, message);
		});

		public void TestOrgContactEmailMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Jane Smith";
			contact2.OC_Email = "jane.smith@email.com";
			Factory.Save();

			const string message = "Email must be specified.";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_Contact = contact1.OC_ContactName;
			AssertHasMessageError("Email not specified", responsibleParty.E2_ContactInfo, message);

			responsibleParty.E2_Contact = contact2.OC_ContactName;
			AssertNoMessageError("Email specified", responsibleParty.E2_ContactInfo, message);
		});

		public void TestOrgContactPhoneOrMobileMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Jane Smith";
			contact2.OC_Phone = "+61 2 9922 9918";
			var contact3 = orgHeader.Contacts.AddNew();
			contact3.OC_ContactName = "Joe Bloggs";
			contact3.OC_Mobile = "+61 410 291029";
			Factory.Save();

			const string message = "Phone or Mobile must be specified.";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_Contact = contact1.OC_ContactName;
			AssertHasMessageError("Phone not specified", responsibleParty.E2_ContactInfo, message);

			responsibleParty.E2_Contact = contact2.OC_ContactName;
			AssertNoMessageError("Phone specified", responsibleParty.E2_ContactInfo, message);

			responsibleParty.E2_Contact = contact3.OC_ContactName;
			AssertNoMessageError("Phone specified", responsibleParty.E2_ContactInfo, message);
		});

		public void TestOverrideContactMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.Validation.ValidateContactPK();
			AssertHasMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, MandatoryValidation.YouHaveNotEntered);

			responsibleParty.E2_AddressOverride = true;
			responsibleParty.E2_Contact = "John Smith";
			AssertNoMessageErrorContaining("Contact not specified", responsibleParty.E2_ContactInfo, MandatoryValidation.YouHaveNotEntered);
		});

		public void TestOverrideContactEmailMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_AddressOverride = true;
			responsibleParty.Validation.ValidateE2_Email();
			AssertHasMessageErrorContaining("Email not specified", responsibleParty.E2_EmailInfo, MandatoryValidation.YouHaveNotEntered);

			responsibleParty.E2_Email = "abc@email.com";
			AssertNoMessageErrorContaining("Email specified", responsibleParty.E2_EmailInfo, MandatoryValidation.YouHaveNotEntered);
		});

		public void TestOverrideContactPhoneMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_AddressOverride = true;
			responsibleParty.Validation.ValidateE2_Phone_Formatted();
			AssertHasMessageErrorContaining("Phone not specified", responsibleParty.E2_Phone_FormattedInfo, MandatoryValidation.YouHaveNotEntered);

			responsibleParty.E2_Phone_Formatted = "+61 2 9283 9929";
			AssertNoMessageErrorContaining("Email specified", responsibleParty.E2_Phone_FormattedInfo, MandatoryValidation.YouHaveNotEntered);
		});

		public void TestOverrideContactMobileMandatory() => CombineAssertions(() =>
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			var colsHeader = Factory.New<QuarantineColsHeader>();
			var responsibleParty = colsHeader.ResponsibleParty;
			responsibleParty.OrganisationPK = orgHeader.PK;
			responsibleParty.E2_AddressOverride = true;
			responsibleParty.Validation.ValidateE2_Mobile_Formatted();
			AssertHasMessageErrorContaining("Mobile not specified", responsibleParty.E2_Mobile_FormattedInfo, MandatoryValidation.YouHaveNotEntered);

			responsibleParty.E2_Mobile_Formatted = "+61 415 283827";
			AssertNoMessageErrorContaining("Mobile specified", responsibleParty.E2_Mobile_FormattedInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}
}
