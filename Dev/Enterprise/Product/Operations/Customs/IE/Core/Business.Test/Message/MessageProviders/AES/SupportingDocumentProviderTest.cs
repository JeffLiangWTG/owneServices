using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class SupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentProvider>
	{
		public void TestLineNumber()
		{
			supportingDocuments.CSI_ItemNumber = 1;
			AssertEquals("LineNumber", "1", provider.LineNumber);
		}

		public void TestType()
		{
			supportingDocuments.CSI_Code = "A";
			AssertEquals("Type", "A", provider.Type);
		}

		public void TestReference()
		{
			supportingDocuments.CSI_ReferenceNumber = "REFERENCE";

			AssertEquals("Reference", "REFERENCE", provider.Reference);
		}

		public void TestIssuingAuthority()
		{
			supportingDocuments.CSI_AdditionalDescription = "TestIssuingAuthority";

			AssertEquals("IssuingAuthority", "TestIssuingAuthority", provider.IssuingAuthority);
		}

		public void TestExpirationDate()
		{
			supportingDocuments.CSI_DateOfExpiry = new ZDateTime(2022, 5, 19, 15, 45, 26, 345, DateTimeKind.Utc);
			AssertEquals("Utc - ExpirationDate", new DateTime(2022, 5, 19, 15, 45, 26, 345, DateTimeKind.Unspecified), provider.ExpirationDate);
			supportingDocuments.CSI_DateOfExpiry = new ZDateTime(2022, 5, 19, 15, 45, 26, 345, DateTimeKind.Local);
			AssertEquals("Local - ExpirationDate", new DateTime(2022, 5, 19, 15, 45, 26, 345, DateTimeKind.Unspecified), provider.ExpirationDate);
			supportingDocuments.CSI_DateOfExpiry = ZDateTime.Invalid;
			AssertEquals("ExpirationDate - Invalid", DateTime.MinValue, provider.ExpirationDate);
			supportingDocuments.CSI_DateOfExpiry = ZDateTime.Empty;
			AssertEquals("ExpirationDate - Empty", DateTime.MinValue, provider.ExpirationDate);
		}

		protected override SupportingDocumentProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();

			supportingDocuments = declaration.SupportingDocuments.AddNew();

			provider = new SupportingDocumentProvider(supportingDocuments);
		}

		protected JobDeclaration declaration;
		protected SupportingDocument supportingDocuments;

		SupportingDocumentProvider provider;
	}
}
