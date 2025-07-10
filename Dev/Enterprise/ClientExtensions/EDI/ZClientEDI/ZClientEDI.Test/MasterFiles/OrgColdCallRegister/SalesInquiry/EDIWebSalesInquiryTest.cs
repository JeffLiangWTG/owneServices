using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIWebSalesInquiry))]
	public class EDIWebSalesInquiryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			EDIWebSalesInquiry inquiry = Factory.New<EDIWebSalesInquiry>();
			AssertEquals(SalesEnquiry.Codes.SalesEnquiry, inquiry.O1_EnquiryType);
		}

		public void TestHtmlLink()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.O1_LeadUniqueReference = "I00001001";
			string link = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.SalesEnquiry, inquiry.PK);
			AssertContains("I00001001", inquiry.HtmlLink);
			AssertContains(link, inquiry.HtmlLink);
		}

		public void TestWorkPhoneNationalCode()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.WorkPhoneNationalCode);
			inquiry.WorkPhoneNationalCode = "61";
			AssertEquals("61", inquiry.WorkPhoneNationalCode);
		}

		public void TestWorkPhoneExtension()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.WorkPhoneExtension);
			inquiry.WorkPhoneExtension = "303";
			AssertEquals("303", inquiry.WorkPhoneExtension);
		}

		public void TestJobTitle()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.JobTitle);
			inquiry.JobTitle = "Developer";
			AssertEquals("Developer", inquiry.JobTitle);
		}

		public void TestJobRole()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.JobRole);
			inquiry.JobRole = "IT";
			AssertEquals("IT", inquiry.JobRole);
		}

		public void TestTypeOfBusiness()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.TypeOfBusinessSelectionsAsText);
			AssertEquals("", inquiry.TypeOfBusinessOther);
			inquiry.TypeOfBusinessSelections[0].BoolValue = true;
			inquiry.TypeOfBusinessOther = "Other Business Type";
			AssertNotEquals("", inquiry.TypeOfBusinessSelectionsAsText);
			AssertEquals("Other Business Type", inquiry.TypeOfBusinessOther);
		}

		public void TestCompanySize()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.CompanySize);
			inquiry.CompanySize = "10-25 Employees";
			AssertEquals("10-25 Employees", inquiry.CompanySize);
		}

		public void TestAdditionalInformation()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.CompanySize);
			inquiry.AdditionalInformation = "Additional Information";
			AssertEquals("Additional Information", inquiry.AdditionalInformation);
		}

		public void TestReasonForRequestingAccess()
		{
			var reasons = new CodeDescriptionBoolCollection(50);
			reasons.Add("REQUEST PRODUCT INFORMATION", (NoResString)"Request Product Information", false);
			reasons.Add("ACCESS TECHNICAL GUIDES", (NoResString)"Access Technical Guides", true);
			EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasons);
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("", inquiry.ReasonForRequestingAccess);
			AssertEquals("", inquiry.ReasonForRequestingAccessOverwrite);
			inquiry.ReasonForRequestingAccessSelections[0].BoolValue = true;
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = false;
			AssertEquals("Request Product Information", inquiry.ReasonForRequestingAccess);
			inquiry.ReasonForRequestingAccessSelections[0].BoolValue = false;
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = true;
			AssertEquals("Access Technical Guides", inquiry.ReasonForRequestingAccess);
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = false;
			AssertEquals("", inquiry.ReasonForRequestingAccess);
			inquiry.ReasonForRequestingAccessOverwrite = "Other Reason";
			AssertEquals("Other Reason", inquiry.ReasonForRequestingAccess);
		}

		[TestDate(2020, 10, 9, 12, 10, 12)]
		public void TestLeadCalledDate()
		{
			var inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, inquiry.O1_LeadCalledDate);
		}

		public void TestIsMyAccountRequest()
		{
			var reasons = new CodeDescriptionBoolCollection(50);
			reasons.Add("REQUEST PRODUCT INFORMATION", (NoResString)"Request Product Information", false);
			reasons.Add("ACCESS TECHNICAL GUIDES", (NoResString)"Access Technical Guides", true);
			EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasons);
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			AssertEquals("Precondition", false, inquiry.IsMyAccountRequest);
			inquiry.ReasonForRequestingAccessSelections[0].BoolValue = true;
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = false;
			AssertEquals("Precondition", "Request Product Information", inquiry.ReasonForRequestingAccess);
			AssertEquals(false, inquiry.IsMyAccountRequest);
			inquiry.ReasonForRequestingAccessSelections[0].BoolValue = false;
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = true;
			AssertEquals("Precondition", "Access Technical Guides", inquiry.ReasonForRequestingAccess);
			AssertEquals(true, inquiry.IsMyAccountRequest);
		}

		[TestDate(2011, 1, 14)]
		public void TestCopyNonPersistentValues()
		{
			var opportunitySources = new CodeDescriptionBoolRelatedItemCollection();
			opportunitySources.Add("WEB", (NoResString)"Website", true);
			opportunitySources.Add("WEX", (NoResString)"MyAccount", true);
			opportunitySources.Add("XXX", (NoResString)"XXX", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunitySources);
			var reasonsForRequestingAccess = new CodeDescriptionBoolCollection(50);
			reasonsForRequestingAccess.Add("REQUEST PRODUCT INFORMATION", (NoResString)"Request Product Information", false);
			reasonsForRequestingAccess.Add("ACCESS TECHNICAL GUIDES", (NoResString)"Access Technical Guides", true);
			EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasonsForRequestingAccess);
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.O1_LeadUniqueReference = "00003333";
			inquiry.WorkPhoneNationalCode = "0061";
			inquiry.O1_Phone = "2 32494949";
			inquiry.WorkPhoneExtension = "303";
			inquiry.MobileNationalCode = "0061";
			inquiry.O1_Mobile = "423948128";
			inquiry.ReasonForRequestingAccessSelections[1].BoolValue = true;
			AssertEquals("Precondtion", true, inquiry.IsMyAccountRequest);
			GlbStaff.CurrentUser.GS_FullName = "Samuel";
			GlbStaff.CurrentUser.GS_LoginName = "SamsLogin";
			Factory.Save();
			AssertEquals("+61 2 32494949<303>", inquiry.O1_Phone);
			AssertEquals("+61 423948128", inquiry.O1_Mobile);
			AssertEquals(new ZDate(2011, 1, 14), inquiry.O1_LeadCalledDate);
			AssertEquals("SamsLogin", inquiry.O1_LeadSourcePerson);
			AssertEquals("WEX", inquiry.O1_LeadSource);
			AssertEquals("Access Technical Guides", inquiry.O1_OpportunitySourceDetails);
			inquiry.O1_LeadSource = "XXX";
			Factory.Save();
			AssertEquals("Lead Source value should be preserved when provided", "XXX", inquiry.O1_LeadSource);
			inquiry.O1_LeadSource = ZString.Empty;
			Factory.Save();
			AssertEquals("Empty Lead Source value should be defaulted to WEX/WEB", "WEX", inquiry.O1_LeadSource);
		}

		public void TestCopyNonPersistentValues_NoMaxLengthExceededException()
		{
			var opportunitySources = new CodeDescriptionBoolRelatedItemCollection { { "WEB", (NoResString)"Website", true }, { "WEX", (NoResString)"MyAccount", true } };
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunitySources);
			var inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.O1_LeadUniqueReference = "00003333";
			inquiry.WorkPhoneNationalCode = "0061";
			inquiry.O1_Phone = "+61 02 32494949";
			inquiry.WorkPhoneExtension = "8888";
			inquiry.MobileNationalCode = "0061";
			inquiry.O1_Mobile = "+61       423948128";
			inquiry.ReasonForRequestingAccessOverwrite = ZString.Replicate('A', 300);
			GlbStaff.CurrentUser.GS_FullName = "Samuel";
			GlbStaff.CurrentUser.GS_LoginName = "SamsLogin.VeryLongLoginName.1234567890";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Phone", "+61 02 32494949", inquiry.O1_Phone);
				AssertEquals("Mobile", "+61       423948128", inquiry.O1_Mobile);
				AssertEquals("Lead Source Person", "SamsLogin.VeryLongLoginName.1234567", inquiry.O1_LeadSourcePerson);
				AssertEquals("Lead SOurce", "WEB", inquiry.O1_LeadSource);
				AssertEquals("Opportunity Source Details", ZString.Replicate('A', OrgColdCallRegister.Schema.O1_OpportunitySourceDetailsMaxLength), inquiry.O1_OpportunitySourceDetails);
			});
		}

		public void TestPopulateInfoNote()
		{
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.O1_LeadUniqueReference = "00003333";
			inquiry.JobTitle = "Tester";
			inquiry.JobRole = "IT";
			inquiry.TypeOfBusinessOther = "Other type of busienss";
			inquiry.CompanySize = "Less than 10 Employees";
			inquiry.AdditionalInformation = "No more information.";
			Factory.Save();
			AssertContains("Tester", inquiry.EnquiryNotesContent.ToUTF8());
			AssertContains("IT", inquiry.EnquiryNotesContent.ToUTF8());
			AssertContains("Other type of busienss", inquiry.EnquiryNotesContent.ToUTF8());
			AssertContains("Less than 10 Employees", inquiry.EnquiryNotesContent.ToUTF8());
			AssertContains("No more information.", inquiry.EnquiryNotesContent.ToUTF8());
		}

		public void TestCreateAnonymousUserRegistrationEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "XXX";
			newStaff.FillWithValidTestData();
			newStaff.GS_EmailAddress = "notify@cargowise.com";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			EDIDataRegistry.Instance.UserRegistrationNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/");
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.CreateUserRegistrationNotificationEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("notify@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "XXY";
			newStaff.FillWithValidTestData();
			newStaff.GS_EmailAddress = "admin@cargowise.com";
			Factory.Save();
			inquiry.CreateUserRegistrationNotificationEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("notify@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals("admin@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[1].Email);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			inquiry.CreateUserRegistrationNotificationEmail("New subject");
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("New subject", Env.OutgoingMailManager.EmailsCreated[0].Subject);
		}

		public void TestCreateAnonymousUserRegistrationEmailWithoutStaff()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff newPostMaster = postMasters.Staff.AddNew();
			newPostMaster = postMasters.Staff.AddNew();
			newPostMaster.GS_Code = "XXY";
			newPostMaster.FillWithValidTestData();
			newPostMaster.GS_EmailAddress = "postmaster@cargowise.com";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			EDIDataRegistry.Instance.UserRegistrationNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/");
			EDIWebSalesInquiry inquiry = Factory.NewWithValidTestData<EDIWebSalesInquiry>();
			inquiry.CreateUserRegistrationNotificationEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Body, "postmaster@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("You are receiving this email because:"));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			GlbStaff newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "XXX";
			newStaff.FillWithValidTestData();
			newStaff.GS_EmailAddress = "notify@cargowise.com";
			Factory.Save();
			inquiry.CreateUserRegistrationNotificationEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("notify@cargowise.com", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("You are receiving this email because:"));
		}
	}
}
