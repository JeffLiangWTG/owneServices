using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Accounting.KR;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.KR.Testing
{
	[TestedType(typeof(DocElectronicInvoiceLine))]
	sealed class DocElectronicInvoiceLineTest : DocumentWrapperTestCase
	{
		public void TestPropertiesAllEmpty()
		{
			var lineItem = new TaxInvoiceTradeLineItem();
			var wrapper = DocElectronicInvoiceLine.New(lineItem, Factory);
			AssertNullOrEmpty(wrapper.DescriptionText);
			AssertNullOrEmpty(wrapper.InvoiceAmount);
			AssertNullOrEmpty(wrapper.CalculatedAmount);
			AssertNullOrEmpty(wrapper.PurchaseExpiryDateTime);
			AssertNullOrEmpty(wrapper.NameText);
		}

		public void TestPropertiesNotEmpty()
		{
			var lineItem = new TaxInvoiceTradeLineItem()
			{
				DescriptionText = "DescriptionText",
				InvoiceAmount = "InvoiceAmount",
				CalculatedAmount = "CalculatedAmount",
				PurchaseExpiryDateTime = "PurchaseExpiryDateTime",
				NameText = "NameText"
			};
			var wrapper = DocElectronicInvoiceLine.New(lineItem, Factory);
			AssertEquals("DescriptionText", wrapper.DescriptionText);
			AssertEquals("InvoiceAmount", wrapper.InvoiceAmount);
			AssertEquals("CalculatedAmount", wrapper.CalculatedAmount);
			AssertEquals("PurchaseExpiryDateTime", wrapper.PurchaseExpiryDateTime);
			AssertEquals("NameText", wrapper.NameText);
		}

		public void TestNewUsingNullLineObject()
		{
			AssertNull(DocElectronicInvoiceLine.New(null, Factory));
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { CreateDocumentWrapperFromStaticNewMethod() };
		}
		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			var lineItem = new TaxInvoiceTradeLineItem();
			return DocElectronicInvoiceLine.New(lineItem, Factory);
		}
	}
}
