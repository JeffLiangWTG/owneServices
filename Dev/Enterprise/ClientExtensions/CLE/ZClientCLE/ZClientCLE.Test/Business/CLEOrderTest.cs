using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.CLE.Testing
{
	public class CLEOrderTest : TestCaseWithFactory
	{
		public void TestCanBeUpdatedByImport()
		{
			var order = Factory.NewWithValidTestData<CLEOrder>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			Assert("Valid. No shipment with declaration / no declaration attached", order.CanBeUpdatedByImport);
			order.JD_JS = shipment.PK;
			Assert("Valid. Shipment attached", order.CanBeUpdatedByImport);
			order.JD_JE = declaration.PK;
			Assert("Not Valid. Declaration attached", !order.CanBeUpdatedByImport);
			order.JD_JE = ZGuid.Empty;
			order.JD_JS = shipment.PK;
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			var order2 = Factory.Load<CLEOrder>(order.PK);
			Assert("Not Valid. Shipment with declaration attached", !order2.CanBeUpdatedByImport);
			order2.JD_JS = ZGuid.Empty;
			var line1 = order2.OrderLines.AddNew();
			line1.JO_Partno = "PANCAKE";
			var line2 = order2.OrderLines.AddNew();
			line2.JO_Partno = "PENCIL";
			Assert("Valid as order is neither attached to shipment/declaration nor commercial invoice", order2.CanBeUpdatedByImport);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JO = line2.PK;
			Assert("Invalid as order 's line 2 is linked to commercial invoice", !order2.CanBeUpdatedByImport);
		}
	}
}
