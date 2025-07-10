using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.Certification;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MarketingManager.Testing
{
	[TestedType(typeof(DocCompanyCampaignItem))]
	sealed class DocCompanyCampaignItemTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Tony";

			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			return new DocumentWrapper[] { DocCompanyCampaignItem.New(item, Factory) };
		}

		[TestDate(2005, 11, 30, 10, 20, 30)]
		public void TestProperties()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ZC";
			staff.GS_FriendlyName = "Mate";
			staff.GS_FullName = "Coord Inator";
			staff.GS_WorkPhone = "(02) 9025 1180";
			staff.GS_MobilePhone = "0411 526 280";
			staff.GS_EmailAddress = "zappoo@zappoo.com";
			staff.GS_Title = "Product Manager";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "RA";
			staff2.GS_FullName = "Sales Rep";
			staff2.GS_WorkPhone = "(02) 8001 2280";
			staff2.GS_MobilePhone = "0430 089 671";
			staff2.GS_EmailAddress = "john@john.com";
			staff2.GS_Title = "I Am Rep";

			GlbStaff staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "PP";
			staff3.GS_FriendlyName = "Job";
			staff3.GS_FullName = "hehe";
			staff3.GS_WorkPhone = "32523";
			staff3.GS_MobilePhone = "325";
			staff3.GS_EmailAddress = "bug@bug.me";
			staff3.GS_Title = "Senior Person";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Zubins Organisation";
			org.OH_Code = "ZUBORG";

			OrgAddress address = org.Addresses.AddNew(OrgAddressType.Office, true);
			address.OA_OH = org.PK;
			address.OA_Address1 = "Unit 3a";
			address.OA_Address2 = "72 O'Riordan Street";
			address.OA_City = "Alexandria";
			address.OA_State = "NSW";
			address.OA_PostCode = "2015";
			address.OA_IsActive = true;

			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff3.GS_Code;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Tony Wallace";
			contact.OC_Title = "CEO";
			contact.OC_Email = "test@edi.com.au";
			contact.OC_Phone = "1234";
			contact.OC_Fax = "55556";
			contact.OC_Mobile = "0430";
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			DocCompanyCampaignItem docItem = DocCompanyCampaignItem.New(item, Factory);

			AssertEquals("Org name", org.OH_FullName, docItem.OrganisationName);
			AssertEquals("Org code", org.OH_Code, docItem.OrganisationCode);
			AssertEquals("Org main address", org.MainAddress.AddressAsASingleLine, docItem.OrganisationMainAddress);

			string expectedAddressAsString = "UNIT 3A<br />72 O'RIORDAN STREET<br />ALEXANDRIA NSW 2015";
			AssertEquals("Org main address", expectedAddressAsString, docItem.OrganisationMainAddressInHTMLWithoutCompanyName);

			AssertEquals("Contact name", contact.OC_ContactName, docItem.ContactName);
			AssertEquals("Salutation", contact.OC_ContactName, docItem.ContactSalutation);
			contact.OC_Salutation = "Tony";
			AssertEquals("Salutation", contact.OC_Salutation, docItem.ContactSalutation);
			AssertEquals("Contact title", "CEO", docItem.ContactJobTitle);
			AssertEquals("Contact email", "test@edi.com.au", docItem.ContactEmail);
			AssertEquals("Contact phone", "1234", docItem.ContactPhone);
			AssertEquals("Contact fax", "55556", docItem.ContactFax);
			AssertEquals("Contact mobile", "0430", docItem.ContactMobile);
			AssertEquals("Contact login to learning centre", "test@edi.com.au", docItem.ContactLearningCentreLogin);

			AssertEquals("Date is correct", new ZDateTime(2005, 11, 30).ToShortDateString(), docItem.ShortDate);
			AssertEquals("Date is correct", new ZDateTime(2005, 11, 30).ToDateTime().ToLongDateString(), docItem.LongDate);
			AssertEquals("Date is correct", "30 November 2005", docItem.LongDateNoDay);

			AssertEquals("Campaign Name", "Hello Campaign", docItem.CampaignName);
			AssertEquals("Campaign ID", campaign.CampaignID, docItem.CampaignID);

			AssertEquals("Coordinator Name", staff.GS_FriendlyName, docItem.CampaignCoordinatorName);
			staff.GS_FriendlyName = "";
			AssertEquals("Coordinator Name", staff.GS_FullName, docItem.CampaignCoordinatorName);
			AssertEquals("Coordinator Email", staff.GS_EmailAddress, docItem.CampaignCoordinatorEmail);
			AssertEquals("Coordinator Work Phone", staff.GS_WorkPhone_Formatted, docItem.CampaignCoordinatorWorkPhone);
			AssertEquals("Coordinator Mobile Phone", staff.GS_MobilePhone_Formatted, docItem.CampaignCoordinatorMobilePhone);
			AssertEquals("Coordinator Title", staff.GS_Title, docItem.CampaignCoordinatorTitle);

			AssertEquals("Manager Name", staff3.GS_FriendlyName, docItem.CampaignManagerName);
			staff3.GS_FriendlyName = "";
			AssertEquals("Manager Name", staff3.GS_FullName, docItem.CampaignManagerName);
			AssertEquals("Manager Email", staff3.GS_EmailAddress, docItem.CampaignManagerEmail);
			AssertEquals("Manager Work Phone", staff3.GS_WorkPhone_Formatted, docItem.CampaignManagerWorkPhone);
			AssertEquals("Manager Mobile Phone", staff3.GS_MobilePhone_Formatted, docItem.CampaignManagerMobilePhone);
			AssertEquals("Manager Title", staff3.GS_Title, docItem.CampaignManagerTitle);

			AssertEquals("Company name", GlbCompany.CurrentCompany.GC_Name, docItem.CurrentCompanyName);
			AssertEquals("Company phone", GlbCompany.CurrentCompany.GC_Phone_Formatted, docItem.CurrentCompanyPhone);
			AssertEquals("Company fax", GlbCompany.CurrentCompany.GC_Fax_Formatted, docItem.CurrentCompanyFax);
			AssertEquals("Company web", GlbCompany.CurrentCompany.GC_WebAddress, docItem.CurrentCompanyWebSite);

			AssertEquals("Sales Rep Name", ZString.Empty, docItem.SalesRepresentativeName);
			AssertEquals("Sales Rep Email", ZString.Empty, docItem.SalesRepresentativeEmail);
			AssertEquals("Sales Rep Work Phone", ZString.Empty, docItem.SalesRepresentativeWorkPhone);
			AssertEquals("Sales Rep Mobile Phone", ZString.Empty, docItem.SalesRepresentativeMobilePhone);
			AssertEquals("Sales Rep Title", ZString.Empty, docItem.SalesRepresentativeTitle);

			AssertEquals("Email Sender Name", campaign.CampaignCoordinator.GS_FullName, docItem.EmailSenderName);
			AssertEquals("Email Sender Title", campaign.CampaignCoordinator.GS_Title, docItem.EmailSenderTitle);
			AssertEquals("Email Sender Email", campaign.CampaignCoordinator.GS_EmailAddress, docItem.EmailSenderEmail);
			AssertEquals("Email Sender Work Phone", campaign.CampaignCoordinator.GS_WorkPhone, docItem.EmailSenderWorkPhone);
			AssertEquals("Email Sender Mobile Phone", campaign.CampaignCoordinator.GS_MobilePhone, docItem.EmailSenderMobilePhone);

			org.StaffAssignments.OverallSalesRep = staff2.GS_Code;
			AssertEquals("Sales Rep Name", staff2.GS_FullName, docItem.SalesRepresentativeName);
			AssertEquals("Sales Rep Email", staff2.GS_EmailAddress, docItem.SalesRepresentativeEmail);
			AssertEquals("Sales Rep Work Phone", staff2.GS_WorkPhone_Formatted, docItem.SalesRepresentativeWorkPhone);
			AssertEquals("Sales Rep Mobile Phone", staff2.GS_MobilePhone_Formatted, docItem.SalesRepresentativeMobilePhone);
			AssertEquals("Sales Rep Title", staff2.GS_Title, docItem.SalesRepresentativeTitle);

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.G0_CampaignName = "Hello Campaign with sender option set";
			campaign2.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign2.G0_GS_NKCampaignManager = staff3.GS_Code;
			campaign2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign2.G0_EmailSenderRole = StaffAssignmentRoles.Codes.SalesRep;

			GlbCompanyCampaignItem item2 = campaign2.CampaignsItemsSent.AddNew();
			item2.G8_RecipientID = contact.PK;

			DocCompanyCampaignItem docItem2 = DocCompanyCampaignItem.New(item2, Factory);

			AssertEquals("Email Sender Name", staff2.GS_FullName, docItem2.EmailSenderName);
			AssertEquals("Email Sender Title", staff2.GS_Title, docItem2.EmailSenderTitle);
			AssertEquals("Email Sender Email", staff2.GS_EmailAddress, docItem2.EmailSenderEmail);
			AssertEquals("Email Sender Work Phone", staff2.GS_WorkPhone, docItem2.EmailSenderWorkPhone);
			AssertEquals("Email Sender Mobile Phone", staff2.GS_MobilePhone, docItem2.EmailSenderMobilePhone);

			AssertEquals("Vote / Survey / Exam Campaign URL", item.VoteExamSurveyCampaignURL, docItem.CampaignURL);
			AssertEquals("Unsubscribe URL", item.GetUnsubscribeUrlString(UnsubscribeType.Sender), docItem.UnsubscribeFromAllCampaignOfthisSenderUrl);
			AssertEquals("Unsubscribe URL", item.GetUnsubscribeUrlString(UnsubscribeType.MediaCategory), docItem.UnsubscribeFromMediaCategoryUrl);
			AssertEquals("Unsubscribe URL", item.GetUnsubscribeUrlString(UnsubscribeType.MediaType), docItem.UnsubscribeFromMediaTypeUrl);
			AssertEquals("Unsubscribe URL", item.GetUnsubscribeUrlString(UnsubscribeType.MediaCategoryAndType), docItem.UnsubscribeFromMediaCategoryAndTypeUrl);

			var campaign3 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign3.G0_CampaignName = "Hello Campaign with sender option set";
			campaign3.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign3.G0_GS_NKCampaignManager = staff3.GS_Code;
			campaign3.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign3.G0_SenderEmail = staff.GS_EmailAddress;

			GlbCompanyCampaignItem item3 = campaign3.CampaignsItemsSent.AddNew();
			item3.G8_RecipientID = contact.PK;

			DocCompanyCampaignItem docItem3 = DocCompanyCampaignItem.New(item3, Factory);

			AssertEquals("Email Sender Name", staff.GS_FullName, docItem3.EmailSenderName);
			AssertEquals("Email Sender Title", "", docItem3.EmailSenderTitle);
			AssertEquals("Email Sender Email", staff.GS_EmailAddress, docItem3.EmailSenderEmail);
			AssertEquals("Email Sender Work Phone", "", docItem3.EmailSenderWorkPhone);
			AssertEquals("Email Sender Mobile Phone", "", docItem3.EmailSenderMobilePhone);
		}

		public void TestCampaignItemPk()
		{
			var item = Factory.New<GlbCompanyCampaignItem>();
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);
			AssertEquals(item.PK.ToString().Replace("-", ""), docWrapper.CampaignItemPk);
		}

		public void TestContactFirstName()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Telstra Organisation";
			org.OH_Code = "ZUBORG";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Wallace";

			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign";

			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			DocCompanyCampaignItem docItem = DocCompanyCampaignItem.New(item, Factory);

			Assert("Full name is not empty", !docItem.ContactName.IsEmpty);
			Assert("First name is the first word in the full name", "John" == docItem.ContactFirstName);
		}

		public void TestSalesRep()
		{
			GlbStaff sCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			sCoordinator.GS_EmailAddress = "florenzo@cargowise.com";
			sCoordinator.GS_Code = "FL";

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			campaign.G0_GC = company.PK;
			campaign.G0_GS_NKCampaignCoordinator = sCoordinator.GS_Code;

			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgStaffAssignmentsCollection staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);

			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "NED";
			assignment1.O8_GC = company.PK;
			assignment1.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment1);

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Tony";

			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "eddy@cargowise.com";
			staff1.GS_Code = "NED";

			AssertEquals("The keys should be similar", docWrapper.SalesRepresentativeEmail, staff1.GS_EmailAddress);
		}

		public void TestGetStaffAssignment()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@test.com";
			staff1.GS_Code = "ST1";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "ST1";
			assignment1.O8_GC = Env.CurrentCompany.PK;
			assignment1.O8_OH = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact One";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			var docWrapper = DocCompanyCampaignItem.New(item, Factory);
			AssertEquals("staff1@test.com", docWrapper.GetStaffAssignment("SAL").EmailAddress);
		}

		public void TestRecipientAndExam_Contact()
		{
			var exam = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";
			contact.OC_Language = Core.SharedConstants.Languages.German;

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", contact, "", language: Core.SharedConstants.Languages.German, examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExam("CRT000001", "CRT000001STD"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExam("CRT000002", "CRT000002STD"));
		}

		public void TestRecipientAndExam_Staff()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "sam@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = staff.PK;
			item.G8_RecipientTableCode = staff.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", staff, "", language: Core.SharedConstants.Languages.German, examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExam("CRT000001", "CRT000001STD"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExam("CRT000002", "CRT000002STD"));
		}

		public void TestRecipientAndExam_Applicant()
		{
			var exam = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "sam@test.org";

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = applicant.PK;
			item.G8_RecipientTableCode = applicant.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", applicant, "", language: string.Empty, examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExam("CRT000001", "CRT000001STD"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExam("CRT000002", "CRT000002STD"));
		}

		public void TestRecipientAndExamWithLanguage_Contact()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", contact, "", language: "ENG", examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExamWithLanguage("CRT000002", "CRT000002STD", "ENG"));
		}

		public void TestRecipientAndExamWithLanguage__Staff()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "sam@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = staff.PK;
			item.G8_RecipientTableCode = staff.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", staff, "", language: "ENG", examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExamWithLanguage("CRT000002", "CRT000002STD", "ENG"));
		}

		public void TestRecipientAndExamWithLanguage_Applicant()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "sam@test.org";

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = applicant.PK;
			item.G8_RecipientTableCode = applicant.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", applicant, "", language: "ENG", examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExamWithLanguage("CRT000002", "CRT000002STD", "ENG"));
		}

		public void TestRecipientAndExamWithoutJobSkillTestShouldNotGenerateUrl()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			AssertEquals(ZString.Empty, docWrapper.RecipientAndExam("CRT000001", "STD"));
			AssertEquals(ZString.Empty, docWrapper.RecipientAndExamWithLanguage("CRT000001", "STD", "ENG"));
		}

		public void TestRecipientAndExamWithLanguage__StaffWithDuplicateEmail()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "sam@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "sam@test.org";

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = staff.PK;
			item.G8_RecipientTableCode = staff.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			AssertEquals("Exam URL could not be generated for this recipient because there was a duplicate of their email in the database.", docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
			AssertEquals("Exam URL could not be generated for this recipient because there was a duplicate of their email in the database.", docWrapper.RecipientAndExamWithLanguage("CRT000002", "CRT000002STD", "ENG"));
		}

		public void TestRecipientAndExamWithLanguage__StaffWithEmailMatchingNonLearningCentreApplicant()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name 1";
			staff.GS_EmailAddress = "sam@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "name 2";
			staff2.GS_EmailAddress = "alex@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.English;

			Factory.Save();

			var nonLearningCentreApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			nonLearningCentreApplicant.HA_EmailAddress = staff.GS_EmailAddress;
			nonLearningCentreApplicant.HA_PER = staff.GS_PER;

			var learningCentreApplicant = ObjectFactory.Get<IGlbStaffCertificateApplicantCreator>().LoadOrCreateFromStaff(staff2.PK);

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = staff.PK;
			item.G8_RecipientTableCode = staff.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientID = staff2.PK;
			item2.G8_RecipientTableCode = staff2.TablePrefix;
			var docWrapper2 = DocCompanyCampaignItem.New(item2, Factory);

			AssertEquals("Exam URL could not be generated for this recipient because there was a duplicate of their email in the database.", docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
			var expectedUrl = LearningCentreTestUrlHelper.GetTestUrl("CRT000001", staff2, string.Empty, language: "ENG", examSettingsCode: "CRT000001STD");
			AssertEquals(expectedUrl, docWrapper2.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
		}

		public void TestRecipientAndExamWithLanguage__StaffWithEmailMatchingUnrelatedApplicant()
		{
			var exam = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			exam.G0_BroadcastVoteSurveyExam = "LCT";
			exam.G0_Type = "EXM";
			exam.G0_CampaignID = "CRT000001";
			exam.G0_CampaignName = ZGuid.NewZGuid().ToString();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name 1";
			staff.GS_EmailAddress = "sam@test.org";
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "name 2";
			staff2.GS_EmailAddress = "alex@test.org";
			staff2.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			var learningCentreApplicant = (HRJobApplicant)ObjectFactory.Get<IGlbStaffCertificateApplicantCreator>().LoadOrCreateFromStaff(staff2.PK);

			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = exam.PK;

			Factory.Save();

			learningCentreApplicant.HA_EmailAddress = staff.GS_EmailAddress;
			learningCentreApplicant.Factory.Save();

			AssertEquals("Email addresses should match", staff.GS_EmailAddress, learningCentreApplicant.HA_EmailAddress);
			AssertNotEquals("Should be associated to different people", staff.GS_PER, learningCentreApplicant.HA_PER);

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = staff.PK;
			item.G8_RecipientTableCode = staff.TablePrefix;
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);

			AssertEquals("Exam URL could not be generated for this recipient because there was a duplicate of their email in the database.", docWrapper.RecipientAndExamWithLanguage("CRT000001", "CRT000001STD", "ENG"));
		}

		public void TestLinkTracking()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "someone@test.org";
			campaign.G0_GC = Env.CurrentCompany.PK;
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;
			var link = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link.GCL_G0_Campaign = campaign.PK;
			link.GCL_Context = "Display Me";
			var url = "http://www.wisetechglobal.com";
			link.GCL_URL = url;

			var docWrapper = DocCompanyCampaignItem.New(item, Factory);
			var expected = GlbCompanyCampaignLink.GenerateTrackedUrl(item, link.GCL_Context, link.GCL_URL);
			AssertEquals(expected, docWrapper.LinkTracking("Display Me", url));
			AssertEquals(expected, docWrapper.LinkTracking("Display Me"));
			AssertEquals("www.another.com", docWrapper.LinkTracking("Display Me", "www.another.com"));
		}

		public void TestGetContextPK()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "Apple";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "Banana";

			var item = campaign.CampaignsItemsSent.AddNew();
			var docWrapper = DocCompanyCampaignItem.New(item, Factory);
			AssertEquals(link1.PK.ToString().Replace("-", ""), docWrapper.GetContextPK("Apple"));
			AssertEquals(link2.PK.ToString().Replace("-", ""), docWrapper.GetContextPK("Banana"));
			AssertEquals(ZString.Empty, docWrapper.GetContextPK(""));
			AssertEquals(ZString.Empty, docWrapper.GetContextPK("Carrot"));
		}

		public void TestEmailSenderStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "N1";
			staff.GS_FriendlyName = "N1";
			staff.GS_FullName = "N1 N2";
			staff.GS_WorkPhone = "(02) 1111 1111";
			staff.GS_MobilePhone = "1111 111 111";
			staff.GS_EmailAddress = "n1@noemail.com";
			staff.GS_Title = "Product Manager";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var item1 = campaign.CampaignsItemsSent.AddNew();
			var docItem1 = DocCompanyCampaignItem.New(item1, Factory);
			AssertEquals("n1@noemail.com", docItem1.EmailSenderStaff.EmailAddress);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals(null, docItem1.EmailSenderStaff);
		}

		protected override void SetUp()
		{
			base.SetUp();
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/WebVotingForTest/");
		}
	}
}
