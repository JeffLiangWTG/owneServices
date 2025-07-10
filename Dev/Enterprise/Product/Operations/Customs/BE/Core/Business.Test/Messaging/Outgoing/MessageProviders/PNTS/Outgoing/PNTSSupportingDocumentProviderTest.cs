using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSSupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSSupportingDocumentProvider>
{
	public void TestReferenceNumber()
	{
		supportingDocument.CSI_ReferenceNumber = "Description";
		AssertEquals("Description", provider.ReferenceNumber);
	}

	public void TestType()
	{
		supportingDocument.CSI_Code = "Code";
		AssertEquals("Code", provider.Type);
	}

	protected override PNTSSupportingDocumentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = Factory.New<TemporaryStorageSupportingDocument>();
		provider = new PNTSSupportingDocumentProvider(supportingDocument);
	}

	TemporaryStorageSupportingDocument supportingDocument;
	PNTSSupportingDocumentProvider provider;
}
