using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobComInvoiceHeader))]
	public class WoolworthsJobComInvoiceHeaderValidationTest : InvoiceOrderLinkTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Enterprise.Metadata.Business.BaseJobComInvoiceHeader);
			}
		}

		#endregion
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestValidateInvoiceMatchesOrderDelivery_InvoiceCurrency()
		{
			fOrder.JD_RX_NKOrderCurrency = "USD";
			fInvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			fInvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			AssertEquals("Has warning because the currency is not consistent", true, fInvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasWarnings());
			fOrder.JD_RX_NKOrderCurrency = "AUD";
			fInvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			fInvoiceHeader.JZ_RX_NKInvoice_Currency = "AUD";
			AssertEquals("No warning because the currency is now consistent", false, fInvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasWarnings());
		}

		public void TestValidateInvoiceMatchesOrderDelivery_Supplier()
		{
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			fInvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			fOrder.SupplierPK = supplier2.PK;
			fInvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("Has warning because the Supplier is not consistent", true, fInvoiceHeader.JZ_OH_SupplierInfo.HasWarnings());
			fOrder.SupplierPK = supplier1.PK;
			fInvoiceHeader.JZ_OH_Supplier = supplier1.PK;
			AssertEquals("No warning because the Supplier is consistent", false, fInvoiceHeader.JZ_OH_SupplierInfo.HasWarnings());
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader;
		}
		#endregion
	}
}
