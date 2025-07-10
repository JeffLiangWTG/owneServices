using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class PreviousDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentWrapperFieldsAreTruncated()
		{
			var helper = new DeclarationTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var previousDocument = declaration.PreviousDocuments.AddNew();

			previousDocument.CSI_ReferenceNumber = helper.GetStringOfMaxSizePlusOneToTrim(35);
			IPreviousDocument wrapper = PreviousDocumentWrapper.New(previousDocument);
			AssertEquals("ID should be truncated to 35 characters", 35, wrapper.ID.Length);
		}
	}
}
