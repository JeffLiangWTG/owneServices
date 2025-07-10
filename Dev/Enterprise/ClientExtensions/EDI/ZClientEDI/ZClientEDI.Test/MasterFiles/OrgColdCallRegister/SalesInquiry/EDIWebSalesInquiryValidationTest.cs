using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIWebSalesInquiryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAddress1()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_Address1Info);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_Address1Info);
			InquiryForTest.O1_Address1 = "Test Address";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_Address1Info);
		}

		public void TestValidateCity()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_CityInfo);
			InquiryForTest.O1_City = "Test City";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_CityInfo);
		}

		public void TestValidateCompanyName()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_CompanyNameInfo);
			InquiryForTest.O1_CompanyName = "Test Company Name";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_CompanyNameInfo);
		}

		public void TestValidateContactName()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_ContactNameInfo);
			InquiryForTest.O1_ContactName = "Test Contact Name";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_ContactNameInfo);
		}

		public void TestValidateEmail()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_EmailInfo);
			InquiryForTest.O1_Email = "tester@temp.com";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_EmailInfo);
		}

		public void TestValidateState()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_StateInfo);
			InquiryForTest.O1_State = "state";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_StateInfo);
		}

		public void TestValidatePostCode()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_PostCodeInfo);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_PostCodeInfo);
			InquiryForTest.O1_PostCode = "2003";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_PostCodeInfo);
		}

		public void TestValidatePhone()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.O1_PhoneInfo);
			InquiryForTest.O1_Phone = "229393939";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.O1_PhoneInfo);
		}

		public void TestValidateWorkPhoneNationalCode()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.WorkPhoneNationalCodeInfo);
			InquiryForTest.WorkPhoneNationalCode = "61";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.WorkPhoneNationalCodeInfo);
		}

		public void TestValidateJobTitle()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.JobTitleInfo);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.JobTitleInfo);
			InquiryForTest.JobTitle = "Tester";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.JobTitleInfo);
		}

		public void TestValidateJobRole()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.JobRoleInfo);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.JobRoleInfo);
			InquiryForTest.JobRole = "IT Department";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.JobRoleInfo);
		}

		public void TestValidateCompanySize()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.CompanySizeInfo);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasErrors(InquiryForTest.CompanySizeInfo);
			InquiryForTest.CompanySize = "More Than 50 Employees";
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.CompanySizeInfo);
		}

		public void TestValidateTypeOfBusiness()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertNoErrors(InquiryForTest.TypeOfBusinessOtherInfo);
			SetTestInquiryToBeMyAccountRequest();
			InquiryForTest.Validation.ValidateAll();
			AssertHasError("Should have error if nothing checked and leave other text empty", InquiryForTest.TypeOfBusinessOtherInfo, "Please select at least one business type or specify if other.");
			InquiryForTest.TypeOfBusinessSelections[0].BoolValue = true;
			InquiryForTest.Validation.ValidateAll();
			AssertNoError("Should not have error if any are checked", InquiryForTest.TypeOfBusinessOtherInfo, "Please select at least one business type or specify if other.");
			InquiryForTest.TypeOfBusinessSelections[0].BoolValue = false;
			InquiryForTest.TypeOfBusinessOther = "Other business type";
			InquiryForTest.Validation.ValidateAll();
			AssertNoError("Should not have error if other text is filled", InquiryForTest.TypeOfBusinessOtherInfo, "Please select at least one business type or specify if other.");
		}

		public void TestValidateReasonOfRequestingAccess()
		{
			InquiryForTest.Validation.ValidateAll();
			AssertHasError("Should have error if nothing checked and leave other text empty", InquiryForTest.ReasonForRequestingAccessOverwriteInfo, "Please select one reason for requesting access.");
			InquiryForTest.ReasonForRequestingAccessSelections[0].BoolValue = true;
			InquiryForTest.Validation.ValidateAll();
			AssertNoError("Should not have error if one is checked", InquiryForTest.ReasonForRequestingAccessOverwriteInfo, "Please select one reason for requesting access.");
		}

		EDIWebSalesInquiry InquiryForTest;
		void SetTestInquiryToBeMyAccountRequest()
		{
			InquiryForTest.ReasonForRequestingAccessSelections[1].BoolValue = true;
			AssertEquals("Precondition", true, InquiryForTest.IsMyAccountRequest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var reasonsForRequestingAccess = new CodeDescriptionBoolCollection(50);
			reasonsForRequestingAccess.Add("REQUEST PRODUCT INFORMATION", (NoResString)"Request Product Information", false);
			reasonsForRequestingAccess.Add("ACCESS TECHNICAL GUIDES", (NoResString)"Access Technical Guides", true);
			EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasonsForRequestingAccess);
			InquiryForTest = Factory.New<EDIWebSalesInquiry>();
		}
	}
}
