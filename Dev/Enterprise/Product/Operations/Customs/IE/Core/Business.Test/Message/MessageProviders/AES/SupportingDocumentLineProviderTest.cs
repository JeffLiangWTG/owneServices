using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class SupportingDocumentLineProviderTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentLineProvider>
	{
		protected override SupportingDocumentLineProvider GetProvider() => provider;

		public void TestMeasurementUnitAndQualifier()
		{
			supportingDocument.CSI_UnitOfQuantity = "KG";
			AssertEquals("Type", "KG", provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			supportingDocument.CSI_Quantity = 1.23m;
			AssertEquals("Quantity", 1.23m, provider.Quantity);
		}

		public void TestCurrency()
		{
			supportingDocument.CSI_RX_NKCurrency = "USD";
			AssertEquals("Currency", "USD", provider.Currency);
		}

		public void TestAmount()
		{
			supportingDocument.CSI_Value = 4.56m;
			AssertEquals("Amount", 4.56m, provider.Amount);
		}

		public void TestLineNumber()
		{
			supportingDocument.CSI_ItemNumber = 1;
			AssertEquals("LineNumber", "1", provider.LineNumber);
		}

		public void TestType()
		{
			supportingDocument.CSI_Code = "SD1";
			AssertEquals("Type", "SD1", provider.Type);
		}

		public void TestReference()
		{
			supportingDocument.CSI_ReferenceNumber = "SD001";
			AssertEquals("Reference", "SD001", provider.Reference);
		}

		public void TestIssuingAuthority()
		{
			supportingDocument.CSI_AdditionalDescription = "Issuing Authority";
			AssertEquals("IssuingAuthority", "Issuing Authority", provider.IssuingAuthority);
		}

		public void TestExpirationDate()
		{
			supportingDocument.CSI_DateOfExpiry = new CargoWise.Types.ZDateTime(2022, 6, 12);
			AssertEquals("ExpirationDate", new DateTime(2022, 6, 12), provider.ExpirationDate);
		}

		public void TestExpirationDate_Empty()
		{
			supportingDocument.CSI_DateOfExpiry = CargoWise.Types.ZDateTime.Empty;
			AssertEquals("ExpirationDate Empty", DateTime.MinValue, provider.ExpirationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			supportingDocument = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().SupportingDocuments.AddNew();
			provider = new SupportingDocumentLineProvider(supportingDocument);
		}

		SupportingDocument supportingDocument;
		SupportingDocumentLineProvider provider;
	}
}
