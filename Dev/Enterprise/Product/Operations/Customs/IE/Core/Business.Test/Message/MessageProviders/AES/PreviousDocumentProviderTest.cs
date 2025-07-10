using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class PreviousDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentProvider>
	{
		public void TestType()
		{
			previousDocument.CSI_Code = "A";
			AssertEquals("Type", "A", provider.Type);
		}

		public void TestReference()
		{
			previousDocument.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("Reference", "REFERENCE", provider.Reference);
		}

		protected override PreviousDocumentProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();

			previousDocument = declaration.PreviousDocuments.AddNew();

			provider = new PreviousDocumentProvider(previousDocument);
		}

		protected JobDeclaration declaration;
		protected PreviousDocument previousDocument;

		PreviousDocumentProvider provider;
	}
}
