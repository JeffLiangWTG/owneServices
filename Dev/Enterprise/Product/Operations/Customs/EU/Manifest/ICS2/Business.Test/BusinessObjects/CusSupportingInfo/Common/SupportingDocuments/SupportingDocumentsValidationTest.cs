using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class SupportingDocumentsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var supportingDocuments = Factory.NewWithValidTestData<SupportingDocument>();
			supportingDocuments.CSI_Code = string.Empty;
			AssertHasMessageErrorContaining(supportingDocuments.CSI_CodeInfo, "You have not entered a Supporting Document Type.");

			supportingDocuments.CSI_Code = "AAAA";
			AssertHasMessageErrorContaining(supportingDocuments.CSI_CodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var supportingDocuments = Factory.NewWithValidTestData<SupportingDocument>();
			supportingDocuments.CSI_ReferenceNumber = string.Empty;
			AssertHasMessageErrorContaining(supportingDocuments.CSI_ReferenceNumberInfo, "You have not entered a Supporting Document Number.");

			supportingDocuments.CSI_ReferenceNumber = "Test Reference";
			AssertNoMessageErrors(supportingDocuments.CSI_ReferenceNumberInfo);
		}
	}
}
