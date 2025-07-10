using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	class DocumentSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentType()
		{
			var bill = Factory.New<AsycudaBill>();
			var sendingObject = new DocumentSendingObject(new AdditionalInfoSendingObject(bill, null, null));
			sendingObject.Validation.ValidateDocumentType();
			AssertNoErrors(sendingObject.DocumentTypeInfo);
		}

		public void TestCheckLocalReferenceNumber()
		{
			var bill = Factory.New<AsycudaBill>();
			var sendingObject = new DocumentSendingObject(new AdditionalInfoSendingObject(bill, null, null));
			sendingObject.Validation.ValidateLocalReferenceNumber();
			AssertNoErrors(sendingObject.LocalReferenceNumberInfo);
		}
	}
}
