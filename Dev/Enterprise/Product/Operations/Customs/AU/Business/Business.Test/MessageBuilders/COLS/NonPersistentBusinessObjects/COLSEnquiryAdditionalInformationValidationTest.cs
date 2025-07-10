using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSEnquiryAdditionalInformationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEnquiryType()
		{
			additionalInformation.EnquiryType = "INVALID";
			additionalInformation.Validation.ValidateEnquiryType();
			AssertHasMessageErrorContaining(additionalInformation.EnquiryTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(additionalInformation.EnquiryTypeInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInformation.EnquiryType = COLSEnquiryTypeList.Codes.ConsignmentSpecificEnquiry;
			additionalInformation.Validation.ValidateEnquiryType();
			AssertNoMessageErrorContaining(additionalInformation.EnquiryTypeInfo, ListValidation.InvalidCodeMessageError);

			additionalInformation.EnquiryType = ZString.Empty;
			additionalInformation.Validation.ValidateEnquiryType();
			AssertHasMessageErrorContaining(additionalInformation.EnquiryTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckContactName()
		{
			additionalInformation.ContactName = ZString.Empty;
			additionalInformation.Validation.ValidateContactName();
			AssertHasMessageErrorContaining(additionalInformation.ContactNameInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInformation.ContactName = "Test Name";
			additionalInformation.Validation.ValidateContactName();
			AssertNoMessageErrorContaining(additionalInformation.ContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckContactPhone()
		{
			additionalInformation.ContactPhone = ZString.Empty;
			additionalInformation.Validation.ValidateContactPhone();
			AssertHasMessageErrorContaining(additionalInformation.ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInformation.ContactPhone = "0297448000";
			additionalInformation.Validation.ValidateContactPhone();
			AssertNoMessageErrorContaining(additionalInformation.ContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(additionalInformation.ContactPhoneInfo, PhoneNumberFormatterAndValidator.InvalidPhoneNumberFormat);

			additionalInformation.ContactPhone = "12345";
			additionalInformation.Validation.ValidateContactPhone();
			AssertHasMessageErrorContaining(additionalInformation.ContactPhoneInfo, PhoneNumberFormatterAndValidator.InvalidPhoneNumberFormat);
		}

		public void TestCheckContactEmail()
		{
			additionalInformation.ContactEmail = ZString.Empty;
			additionalInformation.Validation.ValidateContactEmail();
			AssertHasMessageErrorContaining(additionalInformation.ContactEmailInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInformation.ContactEmail = "Test Email";
			additionalInformation.Validation.ValidateContactEmail();
			AssertNoMessageErrorContaining(additionalInformation.ContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			additionalInformation = new COLSEnquiryAdditionalInformation(colsHeader);
		}
		COLSEnquiryAdditionalInformation additionalInformation;
	}
}
