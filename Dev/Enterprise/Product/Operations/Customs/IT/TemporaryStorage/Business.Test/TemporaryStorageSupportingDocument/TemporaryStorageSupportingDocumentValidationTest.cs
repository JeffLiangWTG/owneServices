using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageSupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ReferenceNumber()
	{
		var supportingDocument = Factory.New<TemporaryStorageSupportingDocument>();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocument.CSI_ReferenceNumberInfo);
	}
}
