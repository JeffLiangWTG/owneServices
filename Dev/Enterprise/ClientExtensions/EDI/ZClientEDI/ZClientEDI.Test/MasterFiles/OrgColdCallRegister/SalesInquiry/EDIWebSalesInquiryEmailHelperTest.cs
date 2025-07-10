using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIWebSalesInquiryEmailHelperTest : TestCaseWithFactory
	{
		public void TestCreateEmail()
		{
			EDIWebSalesInquiry inquiry = Factory.New<EDIWebSalesInquiry>();
			inquiry.ReasonForRequestingAccessOverwrite = "For Testing";
			inquiry.O1_ContactName = "New User";
			inquiry.O1_CompanyName = "Demo Company";
			inquiry.O1_Address1 = "Unit 123";
			inquiry.O1_Address2 = "45 Barker St";
			inquiry.O1_City = "Sydney";
			inquiry.O1_State = "NSW";
			inquiry.O1_PostCode = "6789";
			inquiry.O1_PortOrCountry = "AU";
			inquiry.O1_Email = "tester@hotmail.com";
			inquiry.O1_Phone = "02 8888 8888";
			inquiry.O1_Mobile = "0428 888 888";
			inquiry.JobTitle = "Tester";
			inquiry.JobRole = "IT";
			inquiry.TypeOfBusinessOther = "Other Business";
			inquiry.CompanySize = "Only me";
			inquiry.AdditionalInformation = "Some more information...";
			EDIWebSalesInquiryEmailHelper helper = new EDIWebSalesInquiryEmailHelper(inquiry);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "test@cargowise.com";
			staff.GS_FullName = "Tester";
			GlbStaffManyToManyCollection staffs = new GlbStaffManyToManyCollection(Factory.NewWithValidTestData<GlbGroup>());
			staffs.Add(staff);
			HtmlEmailDef htmlEmail = helper.CreateEmail(staffs);
			AssertNotNull("Email should be in HTML format", htmlEmail);
			AssertEquals("CargoWise My Account User Registration", htmlEmail.Subject);
			AssertEquals("PleaseDoNotReply@wisetechglobal.com", htmlEmail.FromAddress);
			AssertEquals("ediProd System", htmlEmail.FromDisplayName);
			AssertContains("Has standard email banner", "<td class=\"banner\"><img src=\"cid:Banner.jpg\" alt=\"Banner Image\" /></td>", htmlEmail.Body);
			AssertContains("Has standard email footer", "<td><img src=\"cid:Footer.jpg\" alt=\"Footer Image\" /></td>", htmlEmail.Body);
			AssertContains("New User", htmlEmail.Body);
			AssertContains("Demo Company", htmlEmail.Body);
			AssertContains("Unit 123", htmlEmail.Body);
			AssertContains("45 Barker St", htmlEmail.Body);
			AssertContains("Sydney", htmlEmail.Body);
			AssertContains("NSW", htmlEmail.Body);
			AssertContains("6789", htmlEmail.Body);
			AssertContains("AU", htmlEmail.Body);
			AssertContains("tester@hotmail.com", htmlEmail.Body);
			AssertContains("02 8888 8888", htmlEmail.Body);
			AssertContains("0428 888 888", htmlEmail.Body);
			AssertContains("Tester", htmlEmail.Body);
			AssertContains("IT", htmlEmail.Body);
			AssertContains("For Testing", htmlEmail.Body);
			AssertContains("Only me", htmlEmail.Body);
			AssertContains("Some more information...", htmlEmail.Body);
			AssertEquals(2, htmlEmail.Attachments.Count);
		}
	}
}
