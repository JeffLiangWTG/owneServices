using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContactWrapper))]
	sealed class ContactWrapperTest : GenericWrapperTest
	{
		[SetOrgAllowMixedCase(true)]
		public void TestFallbackForAddress()
		{
			OrgContact contact = Factory.New<OrgContact>();
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_Address1 = "44 OrgHeader St";
			OrgAddress extraAddress = orgHeader.Addresses.AddNew();
			extraAddress.OA_Address1 = "37 ExtraAddress Ave";
			ContactWrapper wrapper = new ContactWrapper(contact, extraAddress, Factory);
			AssertEquals("Address should default to Extra Address.", "37 ExtraAddress Ave", wrapper.Address.AddressLine1);
			wrapper = new ContactWrapper(contact, orgHeader, Factory);
			AssertEquals("Address should default to OrgHeader Address.", "44 OrgHeader St", wrapper.Address.AddressLine1);
		}

		public void TestEitherConstructorAllowsANullOrganisationWhenThereIsANonNullContact()
		{
			ContactWrapper emptyWrapper = new ContactWrapper(Factory.New<OrgContact>(), Factory);
			AssertEquals("emptyWrapper.Address.Phone", ZString.Empty, emptyWrapper.Address.Phone);
			AssertEquals("emptyWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, emptyWrapper.AttachmentType.Code);
			AssertEquals("emptyWrapper.Language.Code", Constants.Languages.English, emptyWrapper.Language.Code);
			AssertEquals("emptyWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, emptyWrapper.NotifyMode.Code);
			AssertEquals("emptyWrapper.FullName", ZString.Empty, emptyWrapper.FullName);
			AssertEquals("emptyWrapper.Salutation", ZString.Empty, emptyWrapper.Salutation);
			AssertEquals("emptyWrapper.JobTitle", ZString.Empty, emptyWrapper.JobTitle);
			AssertEquals("emptyWrapper.JobCategory", "EMU", emptyWrapper.JobCategory);
			AssertEquals("emptyWrapper.Phone", ZString.Empty, emptyWrapper.Phone);
			AssertEquals("emptyWrapper.Extension", ZString.Empty, emptyWrapper.Extension);
			AssertEquals("emptyWrapper.Fax", ZString.Empty, emptyWrapper.Fax);
			AssertEquals("emptyWrapper.Mobile", ZString.Empty, emptyWrapper.Mobile);
			AssertEquals("emptyWrapper.Email", ZString.Empty, emptyWrapper.Email);
			AssertEquals("emptyWrapper.Pager", ZString.Empty, emptyWrapper.Pager);
			AssertEquals("emptyWrapper.HomePhone", ZString.Empty, emptyWrapper.HomePhone);
			AssertEquals("emptyWrapper.OtherPhone", ZString.Empty, emptyWrapper.OtherPhone);
			AssertEquals("emptyWrapper.IsActive", ZBool.True, emptyWrapper.IsActive);

			emptyWrapper = new ContactWrapper(Factory.New<OrgContact>(), Factory.GetNull<OrgHeader>(), Factory);
			AssertEquals("emptyWrapper.Address.Phone", ZString.Empty, emptyWrapper.Address.Phone);
			AssertEquals("emptyWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, emptyWrapper.AttachmentType.Code);
			AssertEquals("emptyWrapper.Language.Code", Constants.Languages.English, emptyWrapper.Language.Code);
			AssertEquals("emptyWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, emptyWrapper.NotifyMode.Code);
			AssertEquals("emptyWrapper.FullName", ZString.Empty, emptyWrapper.FullName);
			AssertEquals("emptyWrapper.Salutation", ZString.Empty, emptyWrapper.Salutation);
			AssertEquals("emptyWrapper.JobTitle", ZString.Empty, emptyWrapper.JobTitle);
			AssertEquals("emptyWrapper.JobCategory", "EMU", emptyWrapper.JobCategory);
			AssertEquals("emptyWrapper.Phone", ZString.Empty, emptyWrapper.Phone);
			AssertEquals("emptyWrapper.Extension", ZString.Empty, emptyWrapper.Extension);
			AssertEquals("emptyWrapper.Fax", ZString.Empty, emptyWrapper.Fax);
			AssertEquals("emptyWrapper.Mobile", ZString.Empty, emptyWrapper.Mobile);
			AssertEquals("emptyWrapper.Email", ZString.Empty, emptyWrapper.Email);
			AssertEquals("emptyWrapper.Pager", ZString.Empty, emptyWrapper.Pager);
			AssertEquals("emptyWrapper.HomePhone", ZString.Empty, emptyWrapper.HomePhone);
			AssertEquals("emptyWrapper.OtherPhone", ZString.Empty, emptyWrapper.OtherPhone);
			AssertEquals("emptyWrapper.IsActive", ZBool.True, emptyWrapper.IsActive);
		}

		public override void TestWrapperMappingsEmpty()
		{
			ContactWrapper emptyWrapper = new ContactWrapper((OrgContact)null, Factory);
			AssertEquals("emptyWrapper.Address.Phone", ZString.Empty, emptyWrapper.Address.Phone);
			AssertEquals("emptyWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, emptyWrapper.AttachmentType.Code);
			AssertEquals("emptyWrapper.Language.Code", Constants.Languages.English, emptyWrapper.Language.Code);
			AssertEquals("emptyWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, emptyWrapper.NotifyMode.Code);
			AssertEquals("emptyWrapper.FullName", ZString.Empty, emptyWrapper.FullName);
			AssertEquals("emptyWrapper.Salutation", ZString.Empty, emptyWrapper.Salutation);
			AssertEquals("emptyWrapper.JobTitle", ZString.Empty, emptyWrapper.JobTitle);
			AssertEquals("emptyWrapper.JobCategory", "EMU", emptyWrapper.JobCategory);
			AssertEquals("emptyWrapper.Phone", ZString.Empty, emptyWrapper.Phone);
			AssertEquals("emptyWrapper.Extension", ZString.Empty, emptyWrapper.Extension);
			AssertEquals("emptyWrapper.Fax", ZString.Empty, emptyWrapper.Fax);
			AssertEquals("emptyWrapper.Mobile", ZString.Empty, emptyWrapper.Mobile);
			AssertEquals("emptyWrapper.Email", ZString.Empty, emptyWrapper.Email);
			AssertEquals("emptyWrapper.Pager", ZString.Empty, emptyWrapper.Pager);
			AssertEquals("emptyWrapper.HomePhone", ZString.Empty, emptyWrapper.HomePhone);
			AssertEquals("emptyWrapper.OtherPhone", ZString.Empty, emptyWrapper.OtherPhone);
			AssertEquals("emptyWrapper.IsActive", ZBool.False, emptyWrapper.IsActive);

			emptyWrapper = new ContactWrapper((JobDocAddress)null, Factory);
			AssertEquals("emptyWrapper.Address.Phone", ZString.Empty, emptyWrapper.Address.Phone);
			AssertEquals("emptyWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, emptyWrapper.AttachmentType.Code);
			AssertEquals("emptyWrapper.Language.Code", Constants.Languages.English, emptyWrapper.Language.Code);
			AssertEquals("emptyWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, emptyWrapper.NotifyMode.Code);
			AssertEquals("emptyWrapper.FullName", ZString.Empty, emptyWrapper.FullName);
			AssertEquals("emptyWrapper.Salutation", ZString.Empty, emptyWrapper.Salutation);
			AssertEquals("emptyWrapper.JobTitle", ZString.Empty, emptyWrapper.JobTitle);
			AssertEquals("emptyWrapper.JobCategory", "EMU", emptyWrapper.JobCategory);
			AssertEquals("emptyWrapper.Phone", ZString.Empty, emptyWrapper.Phone);
			AssertEquals("emptyWrapper.Extension", ZString.Empty, emptyWrapper.Extension);
			AssertEquals("emptyWrapper.Fax", ZString.Empty, emptyWrapper.Fax);
			AssertEquals("emptyWrapper.Mobile", ZString.Empty, emptyWrapper.Mobile);
			AssertEquals("emptyWrapper.Email", ZString.Empty, emptyWrapper.Email);
			AssertEquals("emptyWrapper.Pager", ZString.Empty, emptyWrapper.Pager);
			AssertEquals("emptyWrapper.HomePhone", ZString.Empty, emptyWrapper.HomePhone);
			AssertEquals("emptyWrapper.OtherPhone", ZString.Empty, emptyWrapper.OtherPhone);
			AssertEquals("emptyWrapper.IsActive", ZBool.False, emptyWrapper.IsActive);
		}

		public void TestWrapperMappingUsingFallbacks()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("FUGGER", Factory);
			Factory.Save();
			ContactWrapperCollection collection = new ContactWrapperCollection(organisation, Factory);
			ContactWrapper wrapperUsingFallbacks = collection[ContactTypeList.Codes.Administration];
			AssertEquals("wrapperUsingFallbacks.AttachmentType.Code", OrgConstants.AttachmentType.XLS, wrapperUsingFallbacks.AttachmentType.Code);
			AssertEquals("wrapperUsingFallbacks.Language.Code", Constants.Languages.English, wrapperUsingFallbacks.Language.Code);
			AssertEquals("wrapperUsingFallbacks.NotifyMode.Code", Constants.ContactNotifyModes.Email, wrapperUsingFallbacks.NotifyMode.Code);
			AssertEquals("wrapperUsingFallbacks.FullName", ContactType.Administration.DefaultName, wrapperUsingFallbacks.FullName);
			AssertEquals("wrapperUsingFallbacks.Salutation", "Dear The Administration Manager", wrapperUsingFallbacks.Salutation);
			AssertEquals("wrapperUsingFallbacks.JobTitle", "", wrapperUsingFallbacks.JobTitle);
			AssertEquals("wrapperUsingFallbacks.JobCategory", "EMU", wrapperUsingFallbacks.JobCategory);
			AssertEquals("wrapperUsingFallbacks.Extension", "", wrapperUsingFallbacks.Extension);
			AssertEquals("wrapperUsingFallbacks.Mobile", "FUGGER_M", wrapperUsingFallbacks.Mobile);
			AssertEquals("wrapperUsingFallbacks.Pager", "", wrapperUsingFallbacks.Pager);
			AssertEquals("wrapperUsingFallbacks.HomePhone", "", wrapperUsingFallbacks.HomePhone);
			AssertEquals("wrapperUsingFallbacks.OtherPhone", "", wrapperUsingFallbacks.OtherPhone);
			AssertEquals("wrapperUsingFallbacks.Phone", "FUGGER_P", wrapperUsingFallbacks.Phone);
			AssertEquals("wrapperUsingFallbacks.Fax", "FUGGER_F", wrapperUsingFallbacks.Fax);
			AssertEquals("wrapperUsingFallbacks.Email", "FUGGER_E", wrapperUsingFallbacks.Email);
			AssertEquals("wrapperUsingFallbacks.Address.Phone", "FUGGER_P", wrapperUsingFallbacks.Address.Phone);
			AssertEquals("wrapperUsingFallbacks.IsActive", ZBool.True, wrapperUsingFallbacks.IsActive);
		}

		public void TestWrapperMappingOverridingFallbacks()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("FUGGER", Factory);
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.CustomerService, "FAGGER");
			ContactWrapper wrapperOverridingFallbacks = new ContactWrapper(contact, Factory);
			AssertEquals("wrapperOverridingFallbacks.Address.Phone", "FUGGER_P", wrapperOverridingFallbacks.Address.Phone);
			AssertEquals("wrapperOverridingFallbacks.AttachmentType.Code", OrgConstants.AttachmentType.PDF, wrapperOverridingFallbacks.AttachmentType.Code);
			AssertEquals("wrapperOverridingFallbacks.Language.Code", Constants.Languages.English, wrapperOverridingFallbacks.Language.Code);
			AssertEquals("wrapperOverridingFallbacks.NotifyMode.Code", Constants.ContactNotifyModes.Email, wrapperOverridingFallbacks.NotifyMode.Code);
			AssertEquals("wrapperOverridingFallbacks.FullName", "FAGGER_NAME", wrapperOverridingFallbacks.FullName);
			AssertEquals("wrapperOverridingFallbacks.Salutation", "FAGGER_S", wrapperOverridingFallbacks.Salutation);
			AssertEquals("wrapperOverridingFallbacks.JobTitle", "FAGGER_T", wrapperOverridingFallbacks.JobTitle);
			AssertEquals("wrapperOverridingFallbacks.Phone", "FAGGER_P", wrapperOverridingFallbacks.Phone);
			AssertEquals("wrapperOverridingFallbacks.Extension", "FAGGER_X", wrapperOverridingFallbacks.Extension);
			AssertEquals("wrapperOverridingFallbacks.Fax", "FAGGER_F", wrapperOverridingFallbacks.Fax);
			AssertEquals("wrapperOverridingFallbacks.Mobile", "FAGGER_M", wrapperOverridingFallbacks.Mobile);
			AssertEquals("wrapperOverridingFallbacks.Email", "FAGGER_E", wrapperOverridingFallbacks.Email);
			AssertEquals("wrapperOverridingFallbacks.HomePhone", "FAGGER_H", wrapperOverridingFallbacks.HomePhone);
			AssertEquals("wrapperOverridingFallbacks.JobCategory", "FAGGER_J", wrapperOverridingFallbacks.JobCategory);
			AssertEquals("wrapperOverridingFallbacks.OtherPhone", "FAGGER_O", wrapperOverridingFallbacks.OtherPhone);
			AssertEquals("wrapperOverridingFallbacks.Pager", "FAGGER_G", wrapperOverridingFallbacks.Pager);
			AssertEquals("wrapperOverridingFallbacks.IsActive", ZBool.True, wrapperOverridingFallbacks.IsActive);
		}

		public void TestWrapperMappingFromJobDocAddress()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("FUGGER", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "FAGGER");

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;

			ContactWrapper contactWrapper = new ContactWrapper(docAddress, Factory);
			AssertEquals("contactWrapper.Address.Phone", "FUGGER_P", contactWrapper.Address.Phone);
			AssertEquals("contactWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, contactWrapper.AttachmentType.Code);
			AssertEquals("contactWrapper.Language.Code", Constants.Languages.English, contactWrapper.Language.Code);
			AssertEquals("contactWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, contactWrapper.NotifyMode.Code);
			AssertEquals("contactWrapper.FullName", "FAGGER_NAME", contactWrapper.FullName);
			AssertEquals("contactWrapper.Salutation", "FAGGER_S", contactWrapper.Salutation);
			AssertEquals("contactWrapper.JobTitle", "FAGGER_T", contactWrapper.JobTitle);
			AssertEquals("contactWrapper.Phone", "FAGGER_P", contactWrapper.Phone);
			AssertEquals("contactWrapper.Extension", "FAGGER_X", contactWrapper.Extension);
			AssertEquals("contactWrapper.Fax", "FAGGER_F", contactWrapper.Fax);
			AssertEquals("contactWrapper.Mobile", "FAGGER_M", contactWrapper.Mobile);
			AssertEquals("contactWrapper.Email", "FAGGER_E", contactWrapper.Email);
			AssertEquals("contactWrapper.HomePhone", "FAGGER_H", contactWrapper.HomePhone);
			AssertEquals("contactWrapper.JobCategory", "FAGGER_J", contactWrapper.JobCategory);
			AssertEquals("contactWrapper.OtherPhone", "FAGGER_O", contactWrapper.OtherPhone);
			AssertEquals("contactWrapper.Pager", "FAGGER_G", contactWrapper.Pager);

			AddressWrapper addressWrapper = contactWrapper.Address;
			AssertEquals("addressWrapper.Country.Code", "ZZ", addressWrapper.Country.Code);
			AssertEquals("addressWrapper.Location.UNLOCO", "ZZFUG", addressWrapper.Location.UNLOCO);
			AssertEquals("addressWrapper.CompanyName", "FUGGER_NAME", addressWrapper.CompanyName);
			AssertEquals("addressWrapper.CompanyNameAndAddress", "FUGGER_NAME\nFUGGER_ADDRESS1\nFUGGER_ADDRESS2\nFUGGER_CITY FUGGER_S FUGGER_PC", addressWrapper.CompanyNameAndAddress);
			AssertEquals("addressWrapper.Address", "FUGGER_ADDRESS1\nFUGGER_ADDRESS2\nFUGGER_CITY FUGGER_S FUGGER_PC", addressWrapper.Address);
			AssertEquals("addressWrapper.Phone", "FUGGER_P", addressWrapper.Phone);
			AssertEquals("addressWrapper.Fax", "FUGGER_F", addressWrapper.Fax);
			AssertEquals("addressWrapper.Mobile", "FUGGER_M", addressWrapper.Mobile);
			AssertEquals("addressWrapper.Email", "FUGGER_E", addressWrapper.Email);
			AssertEquals("addressWrapper.ContactName", "FAGGER_NAME", addressWrapper.ContactName);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "OV_COMPANYNAME";
			docAddress.E2_Address1 = "OV_ADDRESS1";
			docAddress.E2_Address2 = "OV_ADDRESS2";
			docAddress.E2_City = "OV_CITY";
			docAddress.E2_State = "OV_STATE";
			docAddress.E2_RN_NKCountryCode = "ZA";
			docAddress.E2_Postcode = "OV_PCODE";
			docAddress.E2_Contact = "OV_CONTACT";
			docAddress.E2_Phone = "OV_PHONE";
			docAddress.E2_Fax = "OV_FAX";
			docAddress.E2_Email = "OV_EMAIL";
			docAddress.E2_Mobile = "OV_MOBILE";

			contactWrapper = new ContactWrapper(docAddress, Factory);
			AssertEquals("contactWrapper.AttachmentType.Code", OrgConstants.AttachmentType.PDF, contactWrapper.AttachmentType.Code);
			AssertEquals("contactWrapper.Language.Code", Constants.Languages.English, contactWrapper.Language.Code);
			AssertEquals("contactWrapper.NotifyMode.Code", Constants.ContactNotifyModes.Email, contactWrapper.NotifyMode.Code);
			AssertEquals("contactWrapper.FullName", "OV_CONTACT", contactWrapper.FullName);
			AssertEquals("contactWrapper.Salutation", "", contactWrapper.Salutation);
			AssertEquals("contactWrapper.JobTitle", "", contactWrapper.JobTitle);
			AssertEquals("contactWrapper.Phone", "OV_PHONE", contactWrapper.Phone);
			AssertEquals("contactWrapper.Extension", "", contactWrapper.Extension);
			AssertEquals("contactWrapper.Fax", "OV_FAX", contactWrapper.Fax);
			AssertEquals("contactWrapper.Mobile", "OV_MOBILE", contactWrapper.Mobile);
			AssertEquals("contactWrapper.Email", "OV_EMAIL", contactWrapper.Email);
			AssertEquals("contactWrapper.HomePhone", "", contactWrapper.HomePhone);
			AssertEquals("contactWrapper.JobCategory", "EMU", contactWrapper.JobCategory);
			AssertEquals("contactWrapper.OtherPhone", "", contactWrapper.OtherPhone);
			AssertEquals("contactWrapper.Pager", "", contactWrapper.Pager);

			addressWrapper = contactWrapper.Address;
			AssertEquals("addressWrapper.Country.Code", "ZA", addressWrapper.Country.Code);
			AssertEquals("addressWrapper.Location.UNLOCO", "AUSYD", addressWrapper.Location.UNLOCO);
			AssertEquals("addressWrapper.CompanyName", "OV_COMPANYNAME", addressWrapper.CompanyName);
			AssertMultilineASCIIEquals("addressWrapper.CompanyNameAndAddress", "OV_COMPANYNAME\nOV_ADDRESS1\nOV_ADDRESS2\nOV_CITY\nOV_PCODE\nSOUTH AFRICA", addressWrapper.CompanyNameAndAddress);
			AssertMultilineASCIIEquals("addressWrapper.Address", "OV_ADDRESS1\nOV_ADDRESS2\nOV_CITY\nOV_PCODE\nSOUTH AFRICA", addressWrapper.Address);
			AssertEquals("addressWrapper.Phone", "OV_PHONE", addressWrapper.Phone);
			AssertEquals("addressWrapper.Fax", "OV_FAX", addressWrapper.Fax);
			AssertEquals("addressWrapper.Mobile", "OV_MOBILE", addressWrapper.Mobile);
			AssertEquals("addressWrapper.Email", "OV_EMAIL", addressWrapper.Email);
			AssertEquals("addressWrapper.ContactName", "OV_CONTACT", addressWrapper.ContactName);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Contact                                      (Default Field: FullName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
AttachmentType                          CodeAndDescription
Language                                CodeAndDescription
NotifyMode                              CodeAndDescription
Email                                   String
Extension                               String
Fax                                     String
FullName                                String
HomePhone                               String
IsActive                                Bool
JobCategory                             String
JobTitle                                String
Mobile                                  String
OtherPhone                              String
Pager                                   String
Phone                                   String
Salutation                              String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Address : FUGGER_NAME\nFUGGER_ADDRESS1\nFUGGER_ADDRESS2\nFUGGER_CITY FUGGER_S FUGGER_PC
AttachmentType : PDF - Portable Document Format
Language : EN - English
NotifyMode : EML - E-Mail
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("FUGGER", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "FAGGER");

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;

			return new ContactWrapper(docAddress, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContactWrapper((OrgContact)null, Factory);
		}
	}
}
