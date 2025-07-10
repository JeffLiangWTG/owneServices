using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class AdditionalInfoSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestValidateDocumentType()
		{
			var validation = additionalInfoSendingObject.Validation;
			var targetInfo = additionalInfoSendingObject.DocumentTypeInfo;
			validation.ValidateAll();
			AssertHasError("Document Type should be marked as required when value is empty", targetInfo, "Please enter a Document Type.");

			additionalInfoSendingObject.DocumentType = "9001";
			validation.ValidateAll();
			AssertNoErrors("Should pass the Validation with Document Type entered", targetInfo);
		}

		public void TestValidateDocumentInformation()
		{
			var validation = additionalInfoSendingObject.Validation;
			var targetInfo = additionalInfoSendingObject.DocumentInformationInfo;
			validation.ValidateAll();
			AssertHasError("Document Information should be marked as required when value is empty", targetInfo, "Please enter a Document Information.");

			additionalInfoSendingObject.DocumentInformation = "Some info";
			validation.ValidateAll();
			AssertNoErrors("Should pass the Validation with Document Information entered", targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalInfoSendingObject = new AdditionalInfoSendingObject("");
		}

		AdditionalInfoSendingObject additionalInfoSendingObject;
	}
}
