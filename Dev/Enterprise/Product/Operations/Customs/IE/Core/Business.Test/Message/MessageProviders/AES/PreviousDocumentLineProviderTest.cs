using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class PreviousDocumentLineProviderTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentLineProvider>
	{
		protected override PreviousDocumentLineProvider GetProvider() => provider;

		public void TestLineNumber()
		{
			previousDocument.CSI_ItemNumber = 1;
			AssertEquals("LineNumber", "1", provider.LineNumber);
		}

		public void TestPackageType()
		{
			previousDocument.CSI_PackType = "P1";
			AssertEquals("PackageType", "P1", provider.PackageType);
		}

		public void TestPackageQuantity()
		{
			previousDocument.CSI_PackQty = 2;
			AssertEquals("PackageQuantity", "2", provider.PackageQuantity);
		}

		public void TestMeasurementUnit()
		{
			previousDocument.CSI_UnitOfQuantity = "KG";
			AssertEquals("MeasurementUnit", "KG", provider.MeasurementUnit);
		}

		public void TestQuantity()
		{
			previousDocument.CSI_Quantity = 1.23m;
			AssertEquals("Quantity", 1.23m, provider.Quantity);
		}

		public void TestType()
		{
			previousDocument.CSI_Code = "PD1";
			AssertEquals("Type", "PD1", provider.Type);
		}

		public void TestReference()
		{
			previousDocument.CSI_ReferenceNumber = "PD001";
			AssertEquals("Reference", "PD001", provider.Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			previousDocument = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().PreviousDocuments.AddNew();
			provider = new PreviousDocumentLineProvider(previousDocument);
		}

		PreviousDocument previousDocument;
		PreviousDocumentLineProvider provider;
	}
}
