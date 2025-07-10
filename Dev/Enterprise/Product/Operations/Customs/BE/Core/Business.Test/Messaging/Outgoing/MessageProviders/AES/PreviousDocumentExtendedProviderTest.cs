using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class PreviousDocumentExtendedProviderTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentExtendedProvider>
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

	public void TestGoodsItemNumber()
	{
		previousDocument.CSI_ItemNumber = 123;
		AssertEquals(123, provider.GoodsItemNumber);
	}

	public void TestTypeOfPackages()
	{
		previousDocument.CSI_PackType = "Pck";
		AssertEquals("Pck", provider.TypeOfPackages);
	}

	public void TestNumberOfPackages()
	{
		previousDocument.CSI_PackQty = 1556;
		AssertEquals(1556, provider.NumberOfPackages);
	}

	public void TestMeasurementUnitAndQualifier()
	{
		previousDocument.CSI_UnitOfQuantity = "Pak";
		AssertEquals("Pak", provider.MeasurementUnitAndQualifier);
	}

	public void TestQuantity()
	{
		previousDocument.CSI_Quantity = 1558;
		AssertEquals(1558m, provider.Quantity);
	}

	public void TestComplementOfInformation()
	{
		AssertNullOrEmpty(provider.ComplementOfInformation);
	}

	public void TestGoodsItemIdentifier()
	{
		previousDocument.CSI_ItemNumber = 9764;
		AssertEquals("9764", provider.GoodsItemIdentifier);
	}

	protected override PreviousDocumentExtendedProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.PreviousDocuments.AddNew();
		provider = new PreviousDocumentExtendedProvider(previousDocument, 1);
	}
	EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument;
	PreviousDocumentExtendedProvider provider;
}
