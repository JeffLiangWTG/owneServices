using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentValidation))]
	sealed class AdditionalDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_CodeInfo;
			additionalDocument.CSI_ReferenceNumber = "EPJ110100053F";
			additionalDocument.CSI_Code = "";
			AssertHasMessageError(targetInfo, "You have not entered a type.");
			additionalDocument.CSI_Code = "1234";
			AssertNoMessageError(targetInfo, "Type must be a 4-character alphanumeric code.");
			additionalDocument.CSI_Code = "123";
			AssertHasMessageError(targetInfo, "Type must be a 4-character alphanumeric code.");
			additionalDocument.CSI_Code = "12345";
			AssertHasMessageError(targetInfo, "Type must be a 4-character alphanumeric code.");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_ReferenceNumberInfo;
			additionalDocument.CSI_Code = "1234";
			additionalDocument.CSI_ReferenceNumber = "";
			AssertHasMessageError(targetInfo, "You have not entered a reference number.");
			additionalDocument.CSI_ReferenceNumber = "123456789012345678901234567890123456789012345678901234567890123456789";
			AssertNoMessageError(targetInfo, "Reference number length cannot exceed 70 alphanumeric characters.");
			additionalDocument.CSI_ReferenceNumber = "1234567890123456789012345678901234567890123456789012345678901234567890";
			AssertNoMessageError(targetInfo, "Reference number length cannot exceed 70 alphanumeric characters.");
			additionalDocument.CSI_ReferenceNumber = "12345678901234567890123456789012345678901234567890123456789012345678901";
			AssertHasMessageError(targetInfo, "Reference number length cannot exceed 70 alphanumeric characters.");
		}
	}
}
