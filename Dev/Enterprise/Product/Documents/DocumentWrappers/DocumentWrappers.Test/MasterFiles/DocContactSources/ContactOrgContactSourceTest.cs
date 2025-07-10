using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(ContactOrgContactSource))]
	public class ContactOrgContactSourceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				ContactOrgContactSource.New(Contact, Factory)
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
			Contact.OC_Phone = "03 9289 2829";
			AssertEquals("+61 3 9289 2829", ContactWrapper.Phone);
		}

		public void TestTitle()
		{
			Contact.OC_Title = "MISS";
			AssertEquals("MISS", ContactWrapper.Title);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Org.OH_Code = "XYZXYZ";

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var address = Factory.New<OrgAddress>();
				address.OA_Address1 = "12A TOWER BUILDING";
				address.OA_Address2 = "HIGH TOWER";
				address.OA_City = "Star City";
				address.OA_PostCode = "1111";
				address.OA_RL_NKRelatedPortCode = "DEHAM";
				address.OA_RN_NKCountryCode = "DE";

				Contact.OC_ContactName = "A N Other";
				Contact.OC_OA_OrgAddress = address.PK;

				AssertEquals("PostalAddress", "A N OTHER\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nDEUTSCHLAND", ContactWrapper.PostalAddress);
				AssertEquals("PostalAddressInEnglish", "A N OTHER\n12A TOWER BUILDING\nHIGH TOWER\n1111 STAR CITY\nGERMANY", ContactWrapper.PostalAddressInEnglish);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			Org = Factory.New<OrgHeader>();
			Contact = Org.Contacts.AddNew();
			Org.OH_RL_NKClosestPort = "AUSYD";
			ContactWrapper = ContactOrgContactSource.New(Contact, Factory);
			AssertNotNull("Wrapper created not null", ContactWrapper);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			base.SetUp();
		}

		ContactOrgContactSource ContactWrapper;
		OrgContact Contact;
		OrgHeader Org;
		#endregion
	}
}
