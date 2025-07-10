using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class ForwardingShipmentDueDateSetterTest : InvoiceAndDueDateCalculatorTest
	{
		public void TestShipmentDateWithConsolImport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestShipment.Consols.AddNew();
			TestShipment.Consols[0].JK_RL_NKLoadPort = USLAX.Code;
			TestShipment.Consols[0].JK_RL_NKDischargePort = CNSHA.Code;

			Transport transport = TestShipment.Consols[0].Transports[0];
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ATA = expectedDate;

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			ZDateTime shipmentDate = testDateSetter.GetDateByInvoiceTerm();

			AssertEquals("Shipment Date is incorrect.", expectedDate, shipmentDate);
		}

		[TestDate(2011, 01, 05)]
		public void TestShipmentDateWithConsolImportWithETA()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestShipment.Consols.AddNew();
			Transport transport = TestShipment.Consols[0].Transports[0];
			transport.JW_RL_NKLoadPort = USLAX.Code;
			transport.JW_RL_NKDiscPort = CNSHA.Code;
			transport.JW_ETA = expectedDate;

			TestShipment.JS_RL_NKOrigin = USLAX.Code;
			TestShipment.JS_RL_NKDestination = CNSHA.Code;

			TestShipment.Consols[0].JK_RL_NKLoadPort = USLAX.Code;
			TestShipment.Consols[0].JK_RL_NKDischargePort = CNSHA.Code;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			//Fallback to shipment dates if consol dates are empty
			transport.JW_ETA = ZDateTime.Empty;
			expectedDate = new ZDateTime(2008, 4, 22);
			TestShipment.JS_E_ARV = expectedDate;
			AssertEquals("Shipment Date should be get from SHIPMENT if Consol dates are empty", expectedDate, testDateSetter.GetDateByInvoiceTerm());
		}

		[TestDate(2011, 01, 05)]
		public void TestShipmentDateWithConsolExport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			TestShipment.Consols.AddNew();
			TestShipment.Consols[0].JK_RL_NKLoadPort = CNSHA.Code;
			TestShipment.Consols[0].JK_RL_NKDischargePort = USLAX.Code;
			Transport transport = TestShipment.Consols[0].Transports[0];

			TestShipment.JS_RL_NKOrigin = CNSHA.Code;
			transport.JW_RL_NKLoadPort = CNSHA.Code;
			transport.JW_RL_NKDiscPort = USLAX.Code;
			transport.JW_ATD = expectedDate;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			//Fallback to shipment dates if consol dates are empty
			transport.JW_ATD = ZDateTime.Empty;
			expectedDate = new ZDateTime(2008, 4, 22);
			TestShipment.JS_E_DEP = expectedDate;
			AssertEquals("Shipment Date should be get from SHIPMENT if Consol dates are empty", expectedDate, testDateSetter.GetDateByInvoiceTerm());
		}

		public void TestDueDate_InvoiceTermLSI_SingleShipment()
		{
			var term = new InvoiceTerm("LSI", "", 30);
			TestShipment.JS_RL_NKOrigin = "CNSHA";
			TestShipment.JS_RL_NKDestination = "NZAKL";
			TestShipment.JS_E_DEP = new ZDateTime(2022, 09, 24);
			TestShipment.JS_E_ARV = new ZDateTime(2022, 09, 30);

			var invoiceDate = new ZDateTime(2022, 09, 15);
			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 09, 24), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 09, 24), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 10, 24), testDateSetter.DueDate);

			invoiceDate = new ZDateTime(2022, 09, 28);
			testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 09, 28), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 09, 28), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 10, 28), testDateSetter.DueDate);
		}

		public void TestDueDate_InvoiceTermLSI_ShipmentWithConsol()
		{
			var term = new InvoiceTerm("LSI", "", 30);
			TestShipment.JS_RL_NKOrigin = "CNSHA";
			TestShipment.JS_RL_NKDestination = "NZAKL";
			TestShipment.JS_E_DEP = new ZDateTime(2022, 09, 24);
			TestShipment.JS_E_ARV = new ZDateTime(2022, 09, 30);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = new ZDateTime(2022, 09, 25);
			transport.JW_ETA = new ZDateTime(2022, 10, 01);
			consol.Shipments.Add(TestShipment);

			var invoiceDate = new ZDateTime(2022, 09, 15);
			var testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 09, 25), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 09, 25), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 10, 25), testDateSetter.DueDate);

			invoiceDate = new ZDateTime(2022, 09, 28);
			testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 09, 28), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 09, 28), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 10, 28), testDateSetter.DueDate);

			TestShipment.JS_RL_NKOrigin = "NZAKL";
			TestShipment.JS_RL_NKDestination = "CNSHA";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "CNSHA";
			invoiceDate = new ZDateTime(2022, 09, 15);
			testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 10, 01), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 10, 01), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 10, 31), testDateSetter.DueDate);

			invoiceDate = new ZDateTime(2022, 10, 03);
			testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, invoiceDate, term);
			AssertEquals(new ZDateTime(2022, 10, 03), testDateSetter.GetDateByInvoiceTerm());
			AssertEquals(new ZDateTime(2022, 10, 03), testDateSetter.InvoiceDate);
			AssertEquals(new ZDateTime(2022, 11, 02), testDateSetter.DueDate);
		}

		protected override Type ShipmentType
		{
			get { return typeof(ForwardingShipment); }
		}
	}
}
