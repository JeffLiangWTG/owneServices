using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	public class AccBillingHandlerTest : TestCaseWithFactory
	{
		public void TestCreateGSHBillingAuditLog()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var setup = objectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "PST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 3, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());
			AssertFirstHeader(summary.BillingHeaders.First());

			var shipment4 = objectCreator.CreateShipment("Z0001", setup.gC0002);
			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "REV");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 4, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003, Z0001", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("New billing header added", 2, summary.BillingHeaders.Count());

			var sortedHeaders = summary.BillingHeaders.OrderBy(h => h.ABH_InternalReferenceNumber).ToArray();
			AssertFirstHeader(sortedHeaders.First());
			AssertSecondHeader(sortedHeaders.Skip(1).First());

			setup.gC0002.Shipments.Remove(shipment4);
			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "CST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 4, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003, Z0001", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("No new BillingHeader is added, as current shipment count is less than the already billed shipment count.", 2, summary.BillingHeaders.Count());

			var shipment5 = objectCreator.CreateShipment("Z0002", setup.gC0002);
			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "PST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 4, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003, Z0001", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("No new BillingHeader is added, as current shipment count is equal to the already billed shipment count.", 2, summary.BillingHeaders.Count());

			var shipment6 = objectCreator.CreateShipment("Z0003", setup.gC0002);
			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "PST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
			AssertEquals("BillingCounter", 5, summary.BilledItemCount);
			AssertEquals("Billed Shipments", "S0001, S0002, S0003, Z0002, Z0003", summary.BilliedShipmentNumbersAsCSV);
			AssertEquals("New BillingHeader is added, as current shipment count is more than the already billed shipment count.", 3, summary.BillingHeaders.Count());

			sortedHeaders = summary.BillingHeaders.OrderBy(h => h.ABH_InternalReferenceNumber).ToArray();
			AssertFirstHeader(sortedHeaders.First());
			AssertSecondHeader(sortedHeaders.Skip(1).First());
			AssertThirdHeader(sortedHeaders.Skip(2).First());

			void AssertFirstHeader(AccBillingHeader header)
			{
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

			void AssertSecondHeader(AccBillingHeader header)
			{
				AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
				AssertEquals("Billing Counter", 1, header.ABH_BillingCounter);
				AssertEquals("Billing EventType", "REV", header.ABH_EventType);
				AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
				AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
				AssertEquals("Billing Reference Number", "00001001", header.ABH_InternalReferenceNumber);
				AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
				AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
				AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

				var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
				AssertEquals("BillingLines", 4, lineItems.Length);

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

				line = lineItems[3];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", shipment4.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", shipment4.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
			}

			void AssertThirdHeader(AccBillingHeader header)
			{
				AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
				AssertEquals("Billing Counter", 1, header.ABH_BillingCounter);
				AssertEquals("Billing EventType", "PST", header.ABH_EventType);
				AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
				AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
				AssertEquals("Billing Reference Number", "00001002", header.ABH_InternalReferenceNumber);
				AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
				AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
				AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

				var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
				AssertEquals("BillingLines", 5, lineItems.Length);

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

				line = lineItems[3];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", shipment5.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", shipment5.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[4];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", shipment6.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", shipment6.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
			}
		}

		public void TestGetBillingSummary()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var setup = objectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			AccBillingEventCollector.GetInstance(Factory).AddEvent(AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK", "PST");
			AccBillingHandler.CreateGSHBillingAuditLog(Factory, ZDateTime.UtcNow, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
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
