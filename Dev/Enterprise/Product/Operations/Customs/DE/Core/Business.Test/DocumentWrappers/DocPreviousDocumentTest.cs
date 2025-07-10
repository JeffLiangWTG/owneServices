using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	sealed class DocPreviousDocumentTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertNull(DocPreviousDocument.New(null, Factory));
		}

		public void TestRegistrationNumber()
		{
			previousDocument.CSI_ReferenceNumber = "1234";
			AssertEquals("1234", Wrapper.RegistrationNumber);
		}

		public void TestLineNumber()
		{
			previousDocument.CSI_LineNo = 2;
			AssertEquals(2, Wrapper.LineNumber);
		}

		public void TestQuantity()
		{
			previousDocument.CSI_Quantity = 1.2m;
			AssertEquals("1,200", Wrapper.Quantity);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => DocPreviousDocument.New(previousDocument, Factory);

		new DocPreviousDocument Wrapper => (DocPreviousDocument)base.Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}
		PreviousDocument previousDocument;
	}
}
