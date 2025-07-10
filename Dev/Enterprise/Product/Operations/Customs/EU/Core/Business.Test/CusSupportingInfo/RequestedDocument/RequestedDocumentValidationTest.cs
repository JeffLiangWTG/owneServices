using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class RequestedDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Status()
		{
			var requestedDocument = Factory.New<RequestedDocument>();
			ValidationTestHelper.AssertInvalidCodeMessageError(requestedDocument.CSI_StatusInfo, "XXX", "OPE");
		}
	}
}
