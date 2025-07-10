using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415SimplifiedDeclarationDocumentWritingOffProviderTest : DataProviderTestCase<IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider>
	{
		public void TestIGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff()
		{
			Assert("Should implement IGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff", Provider is IGoodsShipmentItemTypeSimplifiedDeclarationDocumentsWritingOff);
		}

		public void TestPreviousDocumentType()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = "ACB";
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("ACB", provider.PreviousDocumentType);

			previousDocument.CSI_Code = "XFT";
			provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("XFT", provider.PreviousDocumentType);
		}

		public void TestPreviousDocumentIdentifier()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_ReferenceNumber = "REF1";
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("REF1", provider.PreviousDocumentIdentifier);

			previousDocument.CSI_ReferenceNumber = "REFERENCE NUMBER";
			provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("REFERENCE NUMBER", provider.PreviousDocumentIdentifier);
		}

		public void TestPreviousDocumentLineId()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_LineNo = 1;
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("1", provider.PreviousDocumentLineId);

			previousDocument.CSI_LineNo = 5;
			provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("5", provider.PreviousDocumentLineId);
		}

		public void TestMeasurementUnit()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_UnitOfQuantity = "MEA";
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("MEA", provider.MeasurementUnit);

			previousDocument.CSI_UnitOfQuantity = "KGM";
			AssertEquals("KGM", provider.MeasurementUnit);
		}

		public void TestQuantity()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Quantity = 12.36;
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals(12.36m, provider.Quantity);

			previousDocument.CSI_Quantity = 500;
			AssertEquals(500m, provider.Quantity);
		}

		public void TestPackType()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_PackType = "PT";
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("PT", provider.PackType);

			previousDocument.CSI_PackType = "CT";
			AssertEquals("CT", provider.PackType);
		}

		public void TestPackQuantity()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_PackQty = 12;
			var provider = IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(previousDocument);
			AssertEquals("12", provider.PackQuantity);

			previousDocument.CSI_PackQty = 24;
			AssertEquals("24", provider.PackQuantity);
		}

		protected override IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider GetProvider() => IM413AndIM415SimplifiedDeclarationDocumentWritingOffProvider.New(Factory.New<PreviousDocument>());
	}
}
