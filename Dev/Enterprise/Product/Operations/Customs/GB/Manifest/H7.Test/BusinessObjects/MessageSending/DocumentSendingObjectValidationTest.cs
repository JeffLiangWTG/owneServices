using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	class DocumentSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentType()
		{
			var sendingObject = CreateNewDocumentSendingObject();
			sendingObject.Validation.ValidateDocumentType();
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.DocumentTypeInfo);
		}

		public void TestCheckLocalReferenceNumber()
		{
			var sendingObject = CreateNewDocumentSendingObject();
			sendingObject.Validation.ValidateLocalReferenceNumber();
			AssertNoErrors("No error because validation on LocalReferenceNumber is removed", sendingObject.LocalReferenceNumberInfo);
		}

		public void TestFileDescription()
		{
			var sendingObject = CreateNewDocumentSendingObject();
			sendingObject.Validation.ValidateFileDescription();
			ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.FileDescriptionInfo);
		}

		DocumentSendingObject CreateNewDocumentSendingObject()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			return new DocumentSendingObject(new UploadDocumentsSendingAction(bill));
		}
	}
}
