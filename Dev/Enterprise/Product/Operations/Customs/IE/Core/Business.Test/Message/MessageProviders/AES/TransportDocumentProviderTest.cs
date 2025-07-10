using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class TransportDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportDocumentProvider>
	{
		protected override TransportDocumentProvider GetProvider() => provider;

		public void TestType()
		{
			AssertEquals("Type", "TS1", provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("Reference", "TS001", provider.Reference);
		}

		protected override void SetUp()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, "TS1");
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var transportDocument = invoiceHeader.AdditionalInfos.AddNew();
			transportDocument.CSI_Code = "TS1";
			transportDocument.CSI_ReferenceNumber = "TS001";
			transportDocument.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			provider = new TransportDocumentProvider(transportDocument);
		}
		TransportDocumentProvider provider;
	}
}
