using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SupportingDocumentProviderTest : DataProviderTestCase<SupportingDocumentProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull(SupportingDocumentProvider.NewOrNull(null));
				AssertNotNull(SupportingDocumentProvider.NewOrNull(supportingDocument));
			});
		}

		public void TestIdentifier()
		{
			supportingDocument.CSI_ReferenceNumber = "TestSupportingDocument";
			AssertEquals("Identifier", "TestSupportingDocument", Provider.Identifier);
		}

		public void TestType()
		{
			supportingDocument.CSI_Code = "TST";
			AssertEquals("Type", "TST", Provider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			supportingDocument = Factory.NewWithValidTestData<SupportingDocument>();
		}
		SupportingDocument supportingDocument;

		protected override SupportingDocumentProvider GetProvider()
		{
			return SupportingDocumentProvider.NewOrNull(supportingDocument);
		}
	}
}
