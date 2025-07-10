using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	[TestedType(typeof(ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapterForTest))]
	sealed class ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapterTest : IntercompanyTransactionFinancialInvoiceDataAdapterTest
	{
		protected override ValueObjectDataAdapter<InvoicingBase, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapterForTest();
		}

		class ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapterForTest : ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapter
		{
			protected override void SetTxnLineGuid(TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine) => xmlInvoiceLine.TxnLineGUID = "lineGUID";
		}
	}
}
