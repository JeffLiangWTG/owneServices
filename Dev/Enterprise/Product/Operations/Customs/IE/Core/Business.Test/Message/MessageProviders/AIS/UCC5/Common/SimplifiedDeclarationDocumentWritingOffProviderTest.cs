using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class SimplifiedDeclarationDocumentWritingOffProviderTest : DataProviderTestCase<SimplifiedDeclarationDocumentWritingOffProvider>
	{
		public void TestPreviousDocumentType()
		{
			previousDocument.CSI_Code = "A";
			AssertEquals("A", Provider.PreviousDocumentType);

			previousDocument.CSI_Code = "F";
			AssertEquals("F", GetProvider().PreviousDocumentType);
		}

		public void TestPreviousDocumentIdentifier()
		{
			previousDocument.CSI_ReferenceNumber = "Test222";
			AssertEquals("Test222", Provider.PreviousDocumentIdentifier);

			previousDocument.CSI_ReferenceNumber = "444Test";
			AssertEquals("444Test", GetProvider().PreviousDocumentIdentifier);
		}

		public void TestPreviousDocumentLineId()
		{
			previousDocument.CSI_LineNo = 2;
			AssertEquals("2", Provider.PreviousDocumentLineId);

			previousDocument.CSI_LineNo = 4;
			AssertEquals("4", GetProvider().PreviousDocumentLineId);
		}

		protected override SimplifiedDeclarationDocumentWritingOffProvider GetProvider() => new SimplifiedDeclarationDocumentWritingOffProvider(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}

		PreviousDocument previousDocument;
	}
}
