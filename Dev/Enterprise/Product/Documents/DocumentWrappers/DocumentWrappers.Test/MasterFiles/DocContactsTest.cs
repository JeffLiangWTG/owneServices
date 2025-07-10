using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocContacts))]
	public class DocContactsTest : DocumentWrapperTestCase
	{
		public void TestCountryFallbackLogicOnPostalAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "Parent Org Address1";
			var addressAlternate = org.Addresses.AddNew();
			addressAlternate.Address1 = "Parent Org Alternative Address1";
			var orgOverride = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride.MainAddress.Address1 = "Override Org Address1";
			var addressAlternateOverride = orgOverride.Addresses.AddNew();
			addressAlternateOverride.Address1 = "Override Org Alternative Address1";

			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Just1n";

			org.MainAddress.OA_RN_NKCountryCode = "AU";
			var docContact = DocContacts.New(orgContact, Factory);
			AssertEndsWith("Should end with AUSTRALIA", "AUSTRALIA", docContact.PostalAddress);

			org.MainAddress.OA_RN_NKCountryCode = "";
			org.MainAddress.OA_RL_NKRelatedPortCode = "ADALV";
			docContact = DocContacts.New(orgContact, Factory);
			AssertEndsWith("Should end with ANDORRA", "ANDORRA", docContact.PostalAddress);

			addressAlternate.OA_RN_NKCountryCode = "CN";
			orgContact.OC_OA_OrgAddress = addressAlternate.PK;
			docContact = DocContacts.New(orgContact, Factory);
			AssertEndsWith("Should end with CHINA", "CHINA", docContact.PostalAddress);

			orgContact.OC_OA_OrgAddress = ZGuid.Empty;
			orgOverride.MainAddress.OA_RN_NKCountryCode = "US";
			orgContact.OC_OH_AddressOverride = orgOverride.PK;
			docContact = DocContacts.New(orgContact, Factory);
			AssertEndsWith("Should end with UNITED STATES", "UNITED STATES", docContact.PostalAddress);

			addressAlternateOverride.OA_RN_NKCountryCode = "FR";
			orgContact.OC_OA_OrgAddress = addressAlternateOverride.PK;
			docContact = DocContacts.New(orgContact, Factory);
			AssertEndsWith("Should end with FRANCE", "FRANCE", docContact.PostalAddress);
		}

		public void TestPostalAddressForOverrideOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "Parent Org Address1";
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = "CN";
			org.MainAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			var orgOverride = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride.MainAddress.Address1 = "Override Org Address1";
			orgOverride.MainAddress.OA_RN_NKCountryCode = "AU";

			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Just1n";
			orgContact.OC_OH_AddressOverride = orgOverride.PK;

			var docContact = DocContacts.New(orgContact, Factory);
			AssertEquals("JUST1N\nOVERRIDE ORG ADDRESS1\nAUSTRALIA", docContact.PostalAddress);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocContacts.New(Contact, Factory)
			};
		}

		public void TestCode()
		{
			Contact.OC_ContactName = "TEST TEST";
			AssertEquals("TEST TEST", ContactWrapper.Code);
		}

		public void TestAttachmentType()
		{
			Contact.OC_AttachmentType = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("EML", ContactWrapper.AttachmentType);
			Contact.OC_AttachmentType = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("FAX", ContactWrapper.AttachmentType);
		}

		public void TestBirthday()
		{
			Contact.OC_Birthday = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, ContactWrapper.Birthday);
		}

		public void TestContactName()
		{
			Contact.OC_ContactName = "MICHAEL SMITH JNR.";
			AssertEquals("MICHAEL SMITH JNR.", ContactWrapper.ContactName);
		}

		public void TestEmail()
		{
			Contact.OC_Email = "nicolem@syd.pacnet.com.au";
			AssertEquals("nicolem@syd.pacnet.com.au", ContactWrapper.Email);
		}

		public void TestFax()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Contact.OC_Fax_Formatted = "039 338 0716";
			AssertEquals("+61 3 9338 0716", ContactWrapper.Fax);
		}

		public void TestHomePhone()
		{
			Contact.OC_HomePhone_Formatted = "02 9384 3838";
			AssertEquals("+61 2 9384 3838", ContactWrapper.HomePhone);
		}

		public void TestLanguage()
		{
			Contact.OC_Language = Core.SharedConstants.Languages.Indonesian;
			AssertEquals(Core.SharedConstants.Languages.Indonesian, ContactWrapper.Language);
		}

		public void TestMobile()
		{
			Contact.OC_Mobile_Formatted = "0407 927 282";
			AssertEquals("+61 407 927 282", ContactWrapper.Mobile);
		}

		public void TestNotifyMode()
		{
			Contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("FAX", ContactWrapper.NotifyMode);
			Contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("EML", ContactWrapper.NotifyMode);
		}

		public void TestOrgAddress()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_Address1 = "Test address";
			Contact.OC_OA_OrgAddress = address.PK;
			AssertEquals("Test address", ContactWrapper.OrgAddress.Address1);
		}

		public void TestOrganisation()
		{
			Org.OH_FullName = "TEST PTY";
			AssertEquals("TEST PTY", ContactWrapper.Organisation.Name);
		}

		public void TestAddressOverride()
		{
			var orgOverride = Factory.New<OrgHeader>();
			orgOverride.OH_FullName = "TEST OVERRIDE";
			Contact.OC_OH_AddressOverride = orgOverride.PK;
			AssertEquals("TEST OVERRIDE", ContactWrapper.AddressOverride.Name);
		}

		public void TestOtherPhone()
		{
			Contact.OC_OtherPhone_Formatted = "03 3833 8383";
			AssertEquals("+61 3 3833 8383", ContactWrapper.OtherPhone);
		}

		public void TestPager()
		{
			Contact.OC_Pager_Formatted = "03 3833 8383";
			AssertEquals("+61 3 3833 8383", ContactWrapper.Pager);
		}

		public void TestPersonalInfo()
		{
			Contact.OC_PersonalInfo = "My personal info here.";
			AssertEquals("My personal info here.", ContactWrapper.PersonalInfo);
		}

		public void TestPhone()
		{
			Contact.OC_Phone_Formatted = "03 9289 2829";
			AssertEquals("+61 3 9289 2829", ContactWrapper.Phone);
		}

		public void TestTitle()
		{
			Contact.OC_Title = "MISS";
			AssertEquals("MISS", ContactWrapper.Title);
		}

		#region Implementation
		protected override void SetUp()
		{
			Org = Factory.New<OrgHeader>();
			Contact = Org.Contacts.AddNew();
			Org.OH_RL_NKClosestPort = "AUSYD";
			ContactWrapper = DocContacts.New(Contact, Factory);
			AssertNotNull("Wrapper created not null", ContactWrapper);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			base.SetUp();
		}

		DocContacts ContactWrapper;
		OrgContact Contact;
		OrgHeader Org;
		#endregion
	}
}
