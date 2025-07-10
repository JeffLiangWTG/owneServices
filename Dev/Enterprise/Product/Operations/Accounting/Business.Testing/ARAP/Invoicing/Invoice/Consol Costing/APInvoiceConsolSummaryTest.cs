using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCosting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolSummary))]
	public class APInvoiceConsolSummaryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			APInvoice invoice = Factory.New<APInvoice>();
			return new APInvoiceConsolSummary(consol, invoice);
		}
	}
}
