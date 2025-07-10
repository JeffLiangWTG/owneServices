using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentValidation))]
	sealed class AdditionalDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_CodeWhenItIsEmptyAndReferenceNumberHasValue()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_CodeInfo;
			var errorMessage = "You have not entered a Transport Document Type.";
			AssertNoMessageError("Both CSI_ReferenceNumber and CSI_Code empty", targetInfo, errorMessage);
			additionalDocument.CSI_ReferenceNumber = "EPJ110100053F";
			additionalDocument.CSI_Code = "1234";
			AssertNoMessageError("Both CSI_ReferenceNumber and CSI_Code not empty", targetInfo, errorMessage);
			additionalDocument.CSI_Code = "";
			AssertHasMessageError(targetInfo, errorMessage);
		}

		public void TestCheckCSI_CodeWhenItIsNotInTheList()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_CodeInfo;
			var errorMessage = "The code you have selected is not in the list.";
			additionalDocument.CSI_ReferenceNumber = "EPJ110100053F";
			additionalDocument.CSI_Code = "TEST";
			AssertHasMessageError(targetInfo, errorMessage);
		}

		public void TestCheckCSI_ReferenceNumberWhenItIsEmptyAndCSI_CodeHasValue()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_ReferenceNumberInfo;
			var errorMessage = "You have not entered a Transport Document Reference.";
			AssertNoMessageError("Both CSI_ReferenceNumber and CSI_Code empty", targetInfo, errorMessage);
			additionalDocument.CSI_Code = "1234";
			additionalDocument.CSI_ReferenceNumber = "1234";
			AssertNoMessageError("Both CSI_ReferenceNumber and CSI_Code not empty", targetInfo, errorMessage);
			additionalDocument.CSI_ReferenceNumber = "";
			AssertHasMessageError(targetInfo, errorMessage);
		}

		public void TestCheckCSI_ReferenceNumberWhenItIsNotAlphanumeric()
		{
			var additionalDocument = Factory.New<AdditionalDocument>();
			var targetInfo = additionalDocument.CSI_ReferenceNumberInfo;
			var errorMessage = "Transport Document Reference must be alphanumeric.";
			additionalDocument.CSI_Code = "1234";
			additionalDocument.CSI_ReferenceNumber = "123ABC";
			AssertNoMessageError(targetInfo, errorMessage);
			additionalDocument.CSI_ReferenceNumber = "~123ABC";
			AssertHasMessageError(targetInfo, errorMessage);
		}
	}
}
