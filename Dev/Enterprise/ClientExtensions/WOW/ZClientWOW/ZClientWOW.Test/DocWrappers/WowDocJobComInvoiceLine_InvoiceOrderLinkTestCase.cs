using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.DocWrappers.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	public class WowDocJobComInvoiceLine_InvoiceOrderLinkTestCase : InvoiceOrderLinkTestCase
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
		public void TestOrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine()
		{
			fDelivery.J4_RL_NKDestinationPort = "AUMEL";
			WowDocJobComInvoiceLine docInvoiceLine = (WowDocJobComInvoiceLine)WowDocJobComInvoiceLine.New(fInvoiceLine, Factory);
			AssertEquals("Should find the correct linked order line delivery", "AUMEL", docInvoiceLine.OrderLineDeliveryUseFirstForDuplicatesOnSameOrderLine.DestinationPort.Code);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader;
		}
	}
}
