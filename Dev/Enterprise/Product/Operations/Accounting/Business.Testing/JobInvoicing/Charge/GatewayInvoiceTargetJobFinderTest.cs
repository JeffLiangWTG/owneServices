using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class GatewayInvoiceTargetJobFinderTest : TestCaseWithFactory
	{
		public void TestOrderedInvoiceTargetsDuplicates()
		{
			ForwardingConsol getConsol(string load, string discharge, string jobNumber, ForwardingShipment s)
			{
				var c = s.Consols.AddNew();
				c.JK_UniqueConsignRef = jobNumber;
				c.JK_RL_NKLoadPort = load;
				c.JK_RL_NKDischargePort = discharge;
				c.JK_UniqueConsignRef = jobNumber;

				return c;
			}

			Transport getTransport(string load, string discharge, string jobNumber, ForwardingShipment s)
			{
				var c = getConsol(load, discharge, jobNumber, s);
				var t = Factory.NewWithValidTestData<Transport>();
				t.ParentType = typeof(ForwardingConsol);
				t.JW_ParentType = "CON";
				t.JW_ParentGUID = c.PK;
				return t;
			}

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "C00C";

			var t1 = getTransport("USLAX", "USNYC", "C00A", shipment);
			var t2 = getTransport("SGSIN", "USLAX", "C00D", shipment);
			var t3 = getTransport("AUBNE", "AUMEL", "C00B", shipment);
			var t4 = getTransport("AUMEL", "SGSIN", "C00B", shipment);
			var t5 = getTransport("NZAKL", "AUBNE", "C00C", shipment);

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			{
				var targets = GatewayInvoiceTargetJobFinder.GetInvoiceTargets(shipment);
				var actual = targets.Select(x => x.JobNumber).ToArray();
				var expected = new[] { "C00C", "C00C", "C00B", "C00B", "C00D", "C00A" };

				AssertArrayEqualsByElements(expected, actual);
			}
		}

		public void TestOrderedInvoiceTargetsDuplicates2()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C001";

			var transport1 = consol.Transports.AddNew();
			transport1.ParentType = typeof(ForwardingConsol);
			transport1.JW_ParentType = "CON";
			transport1.JW_ParentGUID = consol.PK;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			var transport2 = consol.Transports.AddNew();
			transport2.ParentType = typeof(ForwardingConsol);
			transport2.JW_ParentType = "CON";
			transport2.JW_ParentGUID = consol.PK;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			{
				var targets = GatewayInvoiceTargetJobFinder.GetInvoiceTargets(shipment);
				var actual = targets.Select(x => x.JobNumber).ToArray();
				var expected = new[] { "S001", "C001" };

				AssertArrayEqualsByElements(expected, actual);
			}
		}

		public void TestOrderedInvoiceTargetsDuplicates3()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";

			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var transport1 = consol1.Transports.AddNew();
			transport1.ParentType = typeof(ForwardingConsol);
			transport1.JW_ParentType = "CON";
			transport1.JW_ParentGUID = consol1.PK;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			var transport2 = consol2.Transports.AddNew();
			transport2.ParentType = typeof(ForwardingConsol);
			transport2.JW_ParentType = "CON";
			transport2.JW_ParentGUID = consol2.PK;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";

			ErrorReporter.SuppressReportingOfErrors = true;
			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			{
				var targets = GatewayInvoiceTargetJobFinder.GetInvoiceTargets(shipment);
				var actual = targets.Select(x => x.JobNumber).ToArray();
				var expected = new[] { "S001" };

				AssertArrayEqualsByElements(expected, actual);
			}

			shipment.JS_UniqueConsignRef = "";

			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			{
				var targets = GatewayInvoiceTargetJobFinder.GetInvoiceTargets(shipment);
				var actual = targets.Select(x => x.JobNumber).ToArray();
				var expected = System.Array.Empty<string>();

				AssertArrayEqualsByElements(expected, actual);
			}
		}
	}
}
