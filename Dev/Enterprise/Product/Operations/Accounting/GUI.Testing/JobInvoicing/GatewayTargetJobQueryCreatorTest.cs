using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class GatewayTargetJobQueryCreatorTest : TestCaseWithFactory
	{
#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: forwardingShipment")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'forwardingShipment')")]
#endif
		public void TestShipmentIsNull()
		{
			ForwardingShipment shipment = null;
			_ = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, shipment);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: forwardingConsol")]
#else
		[ExpectExceptionMessage(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'forwardingConsol')")]
#endif
		public void TestConsolIsNull()
		{
			ForwardingConsol forwardingConsol = null;
			GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, forwardingConsol);
		}

		public void TestPluginParentIsShipment()
		{
			var shipment = TestObjectCreator.CreateShipment("S1223");
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C001");
			consol1.Shipments.Add(shipment);
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C002");
			var consol3 = TestObjectCreator.CreateConsol(consolNum: "C003");
			consol3.Shipments.Add(shipment);

			var query = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, shipment);
			AssertNull(query);

			var jobForConsol1 = TestObjectCreator.CreateJobHeader();
			jobForConsol1.JH_ParentID = consol1.PK;
			var jobForConsol3 = TestObjectCreator.CreateJobHeader();
			jobForConsol3.JH_ParentID = consol3.PK;
			Factory.Save();

			query = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, shipment);
			var expectedJobHeaderPks = new[] { jobForConsol1.PK, jobForConsol3.PK };
			AssertContainsExactElementsInAnyOrder("Should contain all consol PKs that this shipment is attached to", expectedJobHeaderPks, query.Params.Where(x => x.SchemaColumn == AccTransactionHeaderSchema.AH_JH).Select(x => x.Value));
			AssertEquals("ZQuery contain search for invoice targets by shipment PK", true, query.ParameterisedText.LiteralTextSql.Contains($"JRT_InvoiceTargetID = '{shipment.PK}'"));
		}

		public void TestPluginParentIsConsol()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1222");
			var shipment2 = TestObjectCreator.CreateShipment("S1223");
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C001");
			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C002");
			consol2.Shipments.Add(shipment1);
			var consol3 = TestObjectCreator.CreateConsol(consolNum: "C003");
			var consol4 = TestObjectCreator.CreateConsol(consolNum: "C004");
			consol4.Shipments.Add(shipment2);

			var query = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, consol1);
			AssertNull(query);

			var jobForConsol1 = TestObjectCreator.CreateJobHeader();
			jobForConsol1.JH_ParentID = consol1.PK;
			var jobForConsol2 = TestObjectCreator.CreateJobHeader();
			jobForConsol2.JH_ParentID = consol2.PK;
			var jobForConsol4 = TestObjectCreator.CreateJobHeader();
			jobForConsol4.JH_ParentID = consol4.PK;
			Factory.Save();

			query = GatewayTargetJobQueryCreator.CreateQueryForInvoiceTargetJob(Factory, consol1);
			var expectedJobHeaderPks = new[] { jobForConsol1.PK, jobForConsol2.PK, jobForConsol4.PK };
			AssertContainsExactElementsInAnyOrder("Should contain all consol PKs, that shipments of consol 1 is attached to", expectedJobHeaderPks, query.Params.Where(x => x.SchemaColumn == AccTransactionHeaderSchema.AH_JH).Select(x => x.Value));
			Assert("Should search for invoice targets by consol PK", query.ParameterisedText.LiteralTextSql.Contains($"JRT_InvoiceTargetID = '{consol1.PK}'"));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
