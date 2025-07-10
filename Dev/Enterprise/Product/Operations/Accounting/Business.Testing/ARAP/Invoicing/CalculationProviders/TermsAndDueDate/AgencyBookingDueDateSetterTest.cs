using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class AgencyBookingDueDateSetterTest : InvoiceAndDueDateCalculatorTest
	{
		public void TestShipmentDateWithVoyageImport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = USLAX.Code;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = CNSHA.Code;

			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge);
			TestShipment.JS_JX = sailing.PK;
			sailing.Destination.JB_E_ARV = expectedDate;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			sailing.Destination.JB_A_ARV = expectedDate.AddDays(1);

			AssertEquals("Shipment Date is incorrect.", expectedDate.AddDays(1), testDateSetter.GetDateByInvoiceTerm());

			TestShipment.JS_E_ARV = expectedDate.AddDays(2);

			AssertEquals("Shipment Date is incorrect.", expectedDate.AddDays(2), testDateSetter.GetDateByInvoiceTerm());
		}

		public void TestShipmentDateWithVoyageExport()
		{
			ZDateTime expectedDate = new ZDateTime(2004, 9, 13);

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = CNSHA.Code;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = USLAX.Code;

			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge);
			TestShipment.JS_JX = sailing.PK;
			sailing.Origin.JA_E_DEP = expectedDate;

			InvoiceAndDueDateCalculatorForTest testDateSetter = new InvoiceAndDueDateCalculatorForTest(TestShipment, Invoice.AH_InvoiceDate, new InvoiceTerm(InvoiceTermsList.FromShipmentDate.Code, "", 0));

			AssertEquals("Shipment Date is incorrect.", expectedDate, testDateSetter.GetDateByInvoiceTerm());

			sailing.Origin.JA_A_DEP = expectedDate.AddDays(1);

			AssertEquals("Shipment Date is incorrect.", expectedDate.AddDays(1), testDateSetter.GetDateByInvoiceTerm());

			TestShipment.JS_E_DEP = expectedDate.AddDays(2);

			AssertEquals("Shipment Date is incorrect.", expectedDate.AddDays(2), testDateSetter.GetDateByInvoiceTerm());
		}

		protected override Type ShipmentType
		{
			get { return ObjectFactory.GetType<Freight.Integration.Agency.IAgencyBooking>(); }
		}
	}
}