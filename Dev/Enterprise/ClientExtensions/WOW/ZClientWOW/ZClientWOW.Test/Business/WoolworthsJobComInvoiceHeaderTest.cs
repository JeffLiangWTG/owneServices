using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobComInvoiceHeader))]
	public class WoolworthsJobComInvoiceHeaderTest : BaseJobComInvoiceHeaderAbstractTest<WoolworthsJobComInvoiceHeader, WoolworthsJobComInvoiceLine>
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

		public void TestJZ_InvoiceCurrLandedCostExRate_NOTReadOnly()
		{
			WoolworthsJobComInvoiceHeader invoiceHeader = (WoolworthsJobComInvoiceHeader)GetNewBusinessObject();
			AssertEquals("Landed costing ex-rate should be NOT read-only any more", false, invoiceHeader.JZ_InvoiceCurrLandedCostExRateInfo.ReadOnly);
			invoiceHeader.JZ_InvoiceCurrLandedCostExRate = 0.4m;
			AssertEquals("Setting the landed cost ex rate should have effect", 0.4m, invoiceHeader.JZ_InvoiceCurrLandedCostExRate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader;
		}
	}
}
