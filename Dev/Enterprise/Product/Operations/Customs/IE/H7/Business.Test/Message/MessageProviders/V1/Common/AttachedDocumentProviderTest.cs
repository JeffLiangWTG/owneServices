using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class AttachedDocumentProviderTest : DataProviderTestCase<AttachedDocumentProvider>
	{
		public void TestDocumentType()
		{
			AssertEquals("A101", Provider.DocumentType);
		}

		public void TestDocumentIdentifier()
		{
			AssertEquals("DOC001", Provider.DocumentIdentifier);
		}

		public void TestDocumentDate()
		{
			AssertEquals("20240816", Provider.DocumentDate);
		}

		public void TestDocumentDateWhenEmpty()
		{
			var provider = new AttachedDocumentProvider("A101", "DOC001", ZDateTime.Empty);
			AssertEquals(string.Empty, provider.DocumentDate);
		}

		protected override AttachedDocumentProvider GetProvider() => new AttachedDocumentProvider("A101", "DOC001", new ZDateTime(2024, 8, 16));
	}
}
