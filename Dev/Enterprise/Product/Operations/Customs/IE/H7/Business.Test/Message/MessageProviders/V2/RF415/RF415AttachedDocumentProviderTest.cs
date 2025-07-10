using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415AttachedDocumentProviderTest : DataProviderTestCase<RF415AttachedDocumentProvider>
	{
		public void TestType()
		{
			AssertEquals("DocumentType", expectedDocumentType, Provider.Type);
		}

		public void TestIdentifier()
		{
			AssertEquals("DocumentIdentifier", expectedDocumentIdentifier, Provider.Identifier);
		}

		public void TestDate()
		{
			AssertEquals("DocumentDate", expectedDocumentDate, Provider.Date);
		}

		protected sealed override RF415AttachedDocumentProvider GetProvider()
		{
			return new RF415AttachedDocumentProvider(expectedDocumentType, expectedDocumentIdentifier, expectedDocumentDate);
		}

		readonly string expectedDocumentType = "test document type";
		readonly string expectedDocumentIdentifier = "test document identifier";
		readonly DateTime expectedDocumentDate = new DateTime(2019, 05, 09, 9, 15, 0);
	}
}

