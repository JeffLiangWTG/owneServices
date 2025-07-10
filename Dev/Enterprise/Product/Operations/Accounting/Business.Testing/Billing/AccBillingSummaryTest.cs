using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	public class AccBillingSummaryTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var setup = objectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "PST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			var query = new ZQuery(AccBillingHeaderSchema.ABH_ParentId, setup.gC0002.PK);
			query.AddToFilter(AccBillingHeaderSchema.ABH_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccBillingHeaderSchema.ABH_BillingCode, AccBillingCodes.GatewayBilling);
			var headers = Factory.Load<AccBillingHeader>(query);
			var summary = new AccBillingSummary(headers);

			AssertEquals("BillingCounter", 3, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());

			var header = summary.BillingHeaders.First();
			AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
			AssertEquals("Billing Counter", 3, header.ABH_BillingCounter);
			AssertEquals("Billing EventType", "PST", header.ABH_EventType);
			AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
			AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
			AssertEquals("Billing Reference Number", "00001000", header.ABH_InternalReferenceNumber);
			AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
			AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
			AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

			var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
			AssertEquals("BillingLines", 3, lineItems.Length);

			var line = lineItems[0];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0001.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0001.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

			line = lineItems[1];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0002.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0002.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

			line = lineItems[2];
			AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
			AssertEquals("Shipment PK", setup.s0003.PK, line.ABI_ParentId);
			AssertEquals("Shipment Number", setup.s0003.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
			AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
		}
	}
}
