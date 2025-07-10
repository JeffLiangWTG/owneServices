using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AttachedDocumentProviderTest : DataProviderTestCase<AttachedDocumentProvider>
	{
		public void TestType()
		{
			AssertEquals("Type should return documentType.", "A101", Provider.Type);
		}

		public void TestIdentifier()
		{
			AssertEquals("Identifier should return documentIdentifier.", "DOC001", Provider.Identifier);
		}

		public void TestDate()
		{
			var date = Provider.Date;
			AssertEquals("Date should return documentDate.", new DateTime(2024, 1, 2), date);
			AssertEquals("Unspecified", DateTimeKind.Unspecified, date.Kind);
		}

		public void TestDate_Empty()
		{
			documentDate = ZDateTime.Empty;
			AssertEquals("Date should return DateTime.MinValue when documentDate is empty.", DateTime.MinValue, Provider.Date);
		}

		public void TestDate_Invalid()
		{
			documentDate = ZDateTime.Invalid;
			AssertEquals("Date should return DateTime.MinValue when documentDate is invalid.", DateTime.MinValue, Provider.Date);
		}

		protected override AttachedDocumentProvider GetProvider() => new AttachedDocumentProvider(documentType, documentIdentifier, documentDate);
		readonly ZString documentIdentifier = "DOC001";
		readonly ZString documentType = "A101";
		ZDateTime documentDate = new ZDateTime(2024, 1, 2);
	}
}
