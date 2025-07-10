using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

class TransportDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportDocumentProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(1, provider.SequenceNumber);
	}

	public void TestType()
	{
		transportDocument.CSI_Code = "ABC";
		AssertEquals("ABC", provider.Type);
	}

	public void TestReferenceNumber()
	{
		transportDocument.CSI_ReferenceNumber = "REF123";
		AssertEquals("REF123", provider.ReferenceNumber);
	}

	protected override TransportDocumentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		transportDocument = Factory.New<CusSupportingInfo>();
		transportDocument.CSI_Type = "OTH";
		transportDocument.CSI_ParentTableCode = "JI";
		transportDocument.CSI_SubType = "TRA";
		transportDocument.CSI_DataModel = "BE";

		Factory.Save();
		provider = new TransportDocumentProvider(transportDocument, 1);
	}
	CusSupportingInfo transportDocument;
	TransportDocumentProvider provider;
}
