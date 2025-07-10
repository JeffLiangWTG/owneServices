using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class PreviousDocumentsProviderTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentsProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(1, provider.SequenceNumber);
	}

	public void TestType()
	{
		previousDocument.CSI_Code = "123A";
		AssertEquals("123A", provider.Type);
	}

	public void TestReferenceNumber()
	{
		previousDocument.CSI_ReferenceNumber = "asd123";
		AssertEquals("asd123", provider.ReferenceNumber);
	}

	protected override PreviousDocumentsProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.PreviousDocuments.AddNew();
		provider = new PreviousDocumentsProvider(previousDocument, 1);
	}
	EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument;
	PreviousDocumentsProvider provider;
}
