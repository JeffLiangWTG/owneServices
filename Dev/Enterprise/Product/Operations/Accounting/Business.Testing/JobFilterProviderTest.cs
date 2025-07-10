using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JobFilterProvider))]
	public class JobFilterProviderTest : FilterStripBusinessObjectTestCase
	{
		public void TestExcludingRatingHeaderJob()
		{
			var orgInvoiceType = TestObjectCreator.TestOrganisation.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Module = "ALL";
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "USD";
			periodicInvoice.DebtorPK = TestObjectCreator.TestOrganisation.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var spotQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var spotQuoteJob = new Job.Loader(spotQuote).TryLoadOrCreateWithoutMutexForTestOnly();
			spotQuote.Quote.CurrentOneOffQuote.TT_OH_Creditor = TestObjectCreator.AALSHI.PK;
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			spotQuoteJob.LocalChargesPK = localClient.PK;
			var spotQuoteCharge = TestObjectCreator.CreateCharge(spotQuoteJob, TestObjectCreator.CC1, "Test1", TestObjectCreator.USD, 200m, TestObjectCreator.AALSHI, TestObjectCreator.USD, 200m, TestObjectCreator.TestOrganisation);
			spotQuoteCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			periodicInvoice.LoadJobs();
			AssertEquals(0, periodicInvoice.Jobs.Count);

			spotQuote.ConvertQuoteToQuotedBooking();
			var quotedBookingJob = new Job.Loader(spotQuote).TryLoadOrCreateWithoutMutexForTestOnly();
			quotedBookingJob.CopyChargesFromSpotQuote();
			Factory.Save();

			Factory.ClearQueryCache();
			periodicInvoice.LoadJobs();
			AssertEquals(0, periodicInvoice.Jobs.Count);

			var shipment = TestObjectCreator.CreateShipment("SHP001");
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.JH_TH_NKQuoteNumber = spotQuote.QuotedBookingNumber;
			Factory.Save();

			periodicInvoice.LoadJobs();
			AssertEquals(1, periodicInvoice.Jobs.Count);
			Assert(periodicInvoice.Jobs.Contains(shipmentJob));

			var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var quickBookingJob = new Job.Loader(quickBooking).TryLoadOrCreateWithoutMutexForTestOnly();
			var quickBookingCharge = TestObjectCreator.CreateCharge(quickBookingJob, TestObjectCreator.CC1, "Test2", TestObjectCreator.USD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.USD, 100m, TestObjectCreator.TestOrganisation);
			quickBookingCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			periodicInvoice.LoadJobs();
			AssertEquals(2, periodicInvoice.Jobs.Count);
			Assert(periodicInvoice.Jobs.Contains(quickBookingJob));
			Assert(periodicInvoice.Jobs.Contains(shipmentJob));
		}

		#region Service Level Filters Tests
		public void TestServiceLevelFilter()
		{
			// SHP AND CFS
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_RS_NKServiceLevel = "STD";
			var job = TestObjectCreator.CreateJob(shipment);

			var shipment1 = TestObjectCreator.CreateShipment("S002");
			shipment1.JS_RS_NKServiceLevel = "D2D";
			var job1 = TestObjectCreator.CreateJob(shipment1);

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job1.PK));

			// BRK
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RS_NKServiceLevel = "STD";
			var job2 = TestObjectCreator.CreateJob(declaration);

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_RS_NKServiceLevel = "D2D";
			var job3 = TestObjectCreator.CreateJob(declaration1);

			Factory.Save();

			SelectJobType(false, false, true, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job2.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job3.PK));

			// AGS
			var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
			agencyBooking.JS_RS_NKServiceLevel = "STD";
			var job4 = TestObjectCreator.CreateJob(agencyBooking);

			var agencyBooking1 = Factory.NewWithValidTestData<AgencyBooking>();
			agencyBooking1.JS_RS_NKServiceLevel = "D2D";
			var job5 = TestObjectCreator.CreateJob(agencyBooking1);

			Factory.Save();

			SelectJobType(false, false, true, false, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job4.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job5.PK));

			// AGB
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_RS_NKServiceLevel = "STD";
			var job6 = TestObjectCreator.CreateJob(billOfLading);

			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading1.JS_RS_NKServiceLevel = "D2D";
			var job7 = TestObjectCreator.CreateJob(billOfLading1);

			Factory.Save();

			SelectJobType(false, false, true, false, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job6.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job7.PK));

			// TRN
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_RS_NKServiceLevel = "STD";
			var job8 = TestObjectCreator.CreateJob(cartage);

			var cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			cartage1.JJ_RS_NKServiceLevel = "D2D";
			var job9 = TestObjectCreator.CreateJob(cartage1);

			Factory.Save();

			SelectJobType(false, false, false, false, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job8.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job9.PK));

			// QSH
			var quickBooking = QuotedBooking.New(Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quickBooking.Booking.JS_RS_NKServiceLevel = "STD";
			var job10 = TestObjectCreator.CreateJob(quickBooking);

			var quickBooking1 = QuotedBooking.New(Freight.Integration.QuoteBookingType.QuickBooking, Factory);
			quickBooking1.Booking.JS_RS_NKServiceLevel = "CUS";
			var job11 = TestObjectCreator.CreateJob(quickBooking1);

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job10.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job11.PK));

			var bookingWithQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote.Booking.JS_RS_NKServiceLevel = "STD";
			var job12 = TestObjectCreator.CreateJob(bookingWithQuote);

			var bookingWithQuote1 = QuotedBooking.New(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			bookingWithQuote1.Booking.JS_RS_NKServiceLevel = "CUS";
			var job13 = TestObjectCreator.CreateJob(bookingWithQuote1);

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetServiceLevel("STD")));

			AssertEquals("Expecting collection contain Job", true, JobCollection.Any(x => x.PK == job12.PK));
			AssertEquals("Expecting collection not to contain Job", false, JobCollection.Any(x => x.PK == job13.PK));
		}

		#endregion

		#region Transport Mode Filters Tests

		public void TestTransportModeFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetTransportMode(Enterprise.Core.Constants.TransportModes.Air)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetTransportMode(Enterprise.Core.Constants.TransportModes.Air)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetTransportMode(Enterprise.Core.Constants.TransportModes.Air)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		#endregion

		#region Dates Filters Tests

		public void TestETAFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_E_ARV = new ZDateTime(2011, 03, 15);
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_E_DEP = new ZDateTime(2011, 04, 20);
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			ForwardingConsol consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2012, 11, 23);
			ForwardingShipment shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_E_ARV = new ZDateTime(2012, 12, 21);
			Job3.JH_ParentID = shipment3.PK;

			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_E_ARV = new ZDateTime(2012, 11, 23);
			Job4.JH_ParentID = shipment4.PK;

			var vessel = LoadOrCreateVessel("Vessel001", "0000v1");
			var sailing = CreateSailing(vessel, "111", "AUSYD", "USLAX", new ZDate(2012, 11, 1), new ZDate(2012, 11, 2), new ZDate(2012, 12, 3), new ZDate(2012, 12, 30));
			var shipment5 = TestObjectCreator.CreateShipment("S0005");
			shipment5.JS_JX = sailing.PK;
			var job5 = TestObjectCreator.CreateJob(shipment5);

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 31))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job3", new[] { Job3 }, JobCollection);

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 31))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job3", new[] { Job3 }, JobCollection);

			SelectJobType(false, false, false, false, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 25))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job5", new[] { Job3, job5 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 2), new ZDateTime(2012, 12, 4))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job5", new[] { job5 }, JobCollection);

			//BRK
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DateAtFinalDestination = new ZDate(2019, 12, 25);
			var job6 = TestObjectCreator.CreateJob(declaration);

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_DateAtFinalDestination = new ZDate(2019, 11, 20);
			var job7 = TestObjectCreator.CreateJob(declaration1);

			Factory.Save();

			SelectJobType(false, false, true, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETAQuery(DateComparisonOperator.HasDateInRange, new ZDate(2019, 12, 1), new ZDate(2019, 12, 31))));

			AssertEquals("Expecting collection contain Job6", true, JobCollection.Any(x => x.PK == job6.PK));
			AssertEquals("Expecting collection not to contain Job7", false, JobCollection.Any(x => x.PK == job7.PK));
		}

		public void TestETDFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			ForwardingConsol consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2012, 11, 23);
			ForwardingShipment shipment3 = consol3.Shipments.AddNew();
			shipment3.JS_E_DEP = new ZDateTime(2012, 12, 21);
			Job3.JH_ParentID = shipment3.PK;

			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_E_DEP = new ZDateTime(2012, 11, 23);
			Job4.JH_ParentID = shipment4.PK;

			var vessel = LoadOrCreateVessel("Vessel001", "0000v1");
			var sailing = CreateSailing(vessel, "111", "AUSYD", "USLAX", new ZDate(2012, 12, 1), new ZDate(2013, 1, 2), new ZDate(2013, 12, 3), new ZDate(2013, 12, 4));
			var shipment5 = TestObjectCreator.CreateShipment("S0005");
			shipment5.JS_JX = sailing.PK;
			var job5 = TestObjectCreator.CreateJob(shipment5);

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 31))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job3", new[] { Job3 }, JobCollection);

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 31))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job3", new[] { Job3 }, JobCollection);

			SelectJobType(false, false, false, false, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 11, 1), new ZDateTime(2012, 11, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job4", new[] { Job3, Job4 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 30))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job3 and Job5", new[] { Job3, job5 }, JobCollection);

			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2012, 12, 1), new ZDateTime(2012, 12, 2))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job5", new[] { job5 }, JobCollection);

			// BRK
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DateAtOrigin = new ZDate(2019, 12, 25);
			var job6 = TestObjectCreator.CreateJob(declaration);

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_DateAtOrigin = new ZDate(2019, 11, 20);
			var job7 = TestObjectCreator.CreateJob(declaration1);

			Factory.Save();

			SelectJobType(false, false, true, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetETDQuery(DateComparisonOperator.HasDateInRange, new ZDate(2019, 12, 1), new ZDate(2019, 12, 31))));

			AssertEquals("Expecting collection contain Job6", true, JobCollection.Any(x => x.PK == job6.PK));
			AssertEquals("Expecting collection not to contain Job7", false, JobCollection.Any(x => x.PK == job7.PK));
		}

		public void TestATAFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ATA = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ATA = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			var vessel = LoadOrCreateVessel("Vessel001", "0000v1");
			var sailing = CreateSailing(vessel, "111", "AUSYD", "USLAX", new ZDate(2010, 12, 1), new ZDate(2010, 12, 2), new ZDate(2010, 12, 30), new ZDate(2011, 3, 25));
			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			shipment3.JS_JX = sailing.PK;
			Job3 = TestObjectCreator.CreateJob(shipment3);

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(true, false, true, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job1 and Job3", new[] { Job1, Job3 }, JobCollection);

			SelectJobType(false, false, true, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATAQuery(DateComparisonOperator.HasDateInRange, Date1, Date2.AddMonths(1))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job1 Job2 and Job3", new[] { Job1, Job2, Job3 }, JobCollection);
		}

		public void TestATDFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ATD = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ATD = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			var vessel = LoadOrCreateVessel("Vessel001", "0000v1");
			var sailing = CreateSailing(vessel, "111", "AUSYD", "USLAX", new ZDate(2010, 12, 30), new ZDate(2011, 3, 25), new ZDate(2011, 6, 20), new ZDate(2011, 6, 25));
			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			shipment3.JS_JX = sailing.PK;
			Job3 = TestObjectCreator.CreateJob(shipment3);

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain only Job1", new[] { Job1 }, JobCollection);

			SelectJobType(false, false, false, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain Job1 and Job3", new[] { Job1, Job3 }, JobCollection);

			SelectJobType(false, false, false, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetATDQuery(DateComparisonOperator.HasDateInRange, Date1, Date2.AddMonths(1))));
			AssertContainsExactElementsInAnyOrder("Expecting collection to contain job1, Job2 and Job3", new[] { Job1, Job2, Job3 }, JobCollection);
		}

		public void TestDeliveryDateFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 01, 05);
			Job1.JH_ParentID = shipment1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 05, 18);
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetDeliveryDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetDeliveryDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		public void TestPickupDate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 01, 05);
			Job1.JH_ParentID = shipment1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 05, 18);
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(false, false, true, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetPickupDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetPickupDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetPickupDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		public void TestAWBCutOffDate()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_MasterBillIssueDate = new ZDateTime(2011, 02, 02);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_MasterBillIssueDate = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetAWBCutOffDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetAWBCutOffDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetAWBCutOffDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		public void TestCustomsClearanceDate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			StmALog log1 = shipment1.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log1.SL_EventTime = new ZDateTime(2011, 02, 02);
			}

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			StmALog log2 = shipment1.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log2.SL_EventTime = new ZDateTime(2011, 05, 31);
			}

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCustomsClearanceDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCustomsClearanceDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCustomsClearanceDateQuery(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		public void TestCompletionDate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			CommonCartage cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			cartage1.JJ_A_JCL = new DateTime(2011, 02, 01);
			cartage1.JJ_ParentID = shipment1.PK;
			cartage1.JJ_ParentTableCode = shipment1.TablePrefix;
			cartage1.JJ_ConsignmentID = shipment1.JS_UniqueConsignRef + "/I";
			Job1.JH_ParentID = shipment1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			CommonCartage cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			cartage2.JJ_A_JCL = new DateTime(2009, 01, 01);
			cartage2.JJ_ParentID = shipment2.PK;
			cartage2.JJ_ParentTableCode = shipment2.TablePrefix;
			cartage2.JJ_ConsignmentID = shipment2.JS_UniqueConsignRef + "/I";
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, false, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCompletionDate(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(false, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCompletionDate(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));

			SelectJobType(true, false, true, true, true, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCompletionDate(DateComparisonOperator.HasDateInRange, Date1, Date2)));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Contains(Job2));
		}

		#endregion

		#region Operation Filters Test

		public void TestCarrierFilter()
		{
			TestObjectCreator.AALSHI.OH_IsShippingProvider = true;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			Job1.JH_ParentID = shipment1.PK;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			Job2.JH_ParentID = shipment2.PK;

			var agencyShipment1 = Factory.NewWithValidTestData<AgencyShipment>();
			agencyShipment1.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			Job3.JH_ParentID = agencyShipment1.PK;

			var agencyShipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			Job4.JH_ParentID = agencyShipment2.PK;

			Factory.Save();

			SelectJobType(true, false, false, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetCarrierQuery(TestObjectCreator.AALSHI.PK)));

			AssertEquals("Expecting collection not to contain Job1, as Carrier filter has not been applied because it is not an Agency Job", false, JobCollection.Any(x => x.PK == Job1.PK));
			AssertEquals("Expecting collection not to contain Job2, as Carrier filter has not been applied because it is not an Agency Job", false, JobCollection.Any(x => x.PK == Job2.PK));
			AssertEquals("Expecting collection to contain Job3, as Carrier filter has been applied", true, JobCollection.Any(x => x.PK == Job3.PK));
			AssertEquals("Expecting collection not to contain Job4, as Carrier filter has been applied", false, JobCollection.Any(x => x.PK == Job4.PK));
		}

		public void TestPrincipalFilter()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = TestObjectCreator.ABIGAS.PK;
			Job1.JH_ParentID = shipment1.PK;

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			var agencyShipment1 = Factory.NewWithValidTestData<AgencyShipment>();
			agencyShipment1.JS_OH_DeliveryAgent = TestObjectCreator.ABIGAS.PK;
			Job3.JH_ParentID = agencyShipment1.PK;

			var agencyShipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			Job4.JH_ParentID = agencyShipment2.PK;

			Factory.Save();

			SelectJobType(false, false, false, true, false, true);
			JobCollection.Load(GetQuery(JobFilterProvider.GetPrincipalQuery(TestObjectCreator.ABIGAS.PK)));

			AssertEquals("Expecting collection not to contain Job1, as Principal filter has not been applied because it is not an Agency Job", false, JobCollection.Any(x => x.PK == Job1.PK));
			AssertEquals("Expecting collection not to contain Job2, as Principal filter has not been applied because it is not an Agency Job", false, JobCollection.Any(x => x.PK == Job2.PK));
			AssertEquals("Expecting collection to contain Job3, as Principal filter has been applied", true, JobCollection.Any(x => x.PK == Job3.PK));
			AssertEquals("Expecting collection not to contain Job4, as Principal filter has been applied", false, JobCollection.Any(x => x.PK == Job4.PK));
		}

		public void TestOrderReferenceFilter()
		{
			var orderShipment = Factory.New<ForwardingShipment>();
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "ORDER1";
			orderShipment.AttachedOrders.Add(order1);
			var job1 = TestObjectCreator.CreateJob(orderShipment);
			Factory.Save();

			SelectJobType(false, false, false, true, false, false);

			JobCollection.Load(GetQuery(JobFilterProvider.GetOrderNoQuery(SQLComparisonOperator.StartsWith, "Order1")));
			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Any(x => x.PK == job1.PK));
		}

		public void TestVoyageVesselFilter()
		{
			var testVessel1 = LoadOrCreateVessel("Vessel001", "8610033");
			var testVessel2 = LoadOrCreateVessel("Vessel002", "8610034");

			JobSailing sailing1 = CreateSailing(testVessel1, "111", "AUSYD", "USLAX", ZDateTime.Today);
			JobSailing sailing2 = CreateSailing(testVessel2, "222", "AUMEL", "USCHI", ZDateTime.Today);

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_JX = sailing1.PK;
			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			shipment2.JS_JX = sailing2.PK;
			var shipment3 = TestObjectCreator.CreateShipment("S0003");
			var shipment4 = TestObjectCreator.CreateShipment("S0004");

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var job3 = TestObjectCreator.CreateJob(shipment3);
			var job4 = TestObjectCreator.CreateJob(shipment4);

			Factory.Save();

			SelectJobType(true, false, false, true, false, false);
			JobCollection.Load(GetQuery(JobFilterProvider.GetVoyageAndVesselQuery(SQLComparisonOperator.StartsWith, "11", "Vessel001")));

			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Any(x => x.PK == job2.PK));
			AssertEquals("Expecting collection not to contain Job3", false, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection not to contain Job4", false, JobCollection.Any(x => x.PK == job4.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetVoyageAndVesselQuery(SQLComparisonOperator.StartsWith, "2", "Vessel002")));

			AssertEquals("Expecting collection not to contain Job1", false, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection to contain Job2", true, JobCollection.Any(x => x.PK == job2.PK));
			AssertEquals("Expecting collection not to contain Job3", false, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection not to contain Job4", false, JobCollection.Any(x => x.PK == job4.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetVoyageAndVesselQuery(SQLComparisonOperator.StartsWith, "44", "Vessel002")));

			AssertEquals("Expecting collection not to contain Job1", false, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Any(x => x.PK == job2.PK));
			AssertEquals("Expecting collection not to contain Job3", false, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection not to contain Job4", false, JobCollection.Any(x => x.PK == job4.PK));
		}

		public void TestSendingAndReceivingAgentFilter()
		{
			var org1 = TestObjectCreator.CreateOrgHeader("ORG1", true, true);
			var org2 = TestObjectCreator.CreateOrgHeader("ORG2", true, true);

			//Consol Job
			var consol1 = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var job1 = TestObjectCreator.CreateJob(consol1);
			consol1.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var consol2 = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var job2 = TestObjectCreator.CreateJob(consol2);
			consol2.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			SelectJobType(false, true, false, false, false, false);

			//Sending Agent
			JobCollection.Load(GetQuery(JobFilterProvider.GetSendingAgentQuery(org1.PK)));
			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Any(x => x.PK == job2.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetSendingAgentQuery(org2.PK)));
			AssertEquals("Expecting collection not to contain Job1", false, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection to contain Job2", true, JobCollection.Any(x => x.PK == job2.PK));

			//Receiving Agent
			JobCollection.Load(GetQuery(JobFilterProvider.GetReceivingAgentQuery(org1.PK)));
			AssertEquals("Expecting collection not to contain Job1", false, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection to contain Job2", true, JobCollection.Any(x => x.PK == job2.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetReceivingAgentQuery(org2.PK)));
			AssertEquals("Expecting collection to contain Job1", true, JobCollection.Any(x => x.PK == job1.PK));
			AssertEquals("Expecting collection not to contain Job2", false, JobCollection.Any(x => x.PK == job2.PK));

			//Shipment Job
			var consol3 = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C0003");
			consol3.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol3.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var consol4 = TestObjectCreator.CreateConsol("AUMEL", "AUSYD", "C0004");
			consol4.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol4.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol3);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol4);
			var shipment3 = TestObjectCreator.CreateShipment("S0003", consol3);
			var shipment4 = TestObjectCreator.CreateShipment("S0004", consol4);

			var job3 = TestObjectCreator.CreateJob(shipment1);
			var job4 = TestObjectCreator.CreateJob(shipment2);
			var job5 = TestObjectCreator.CreateJob(shipment3);
			var job6 = TestObjectCreator.CreateJob(shipment4);

			Factory.Save();

			SelectJobType(false, false, false, true, false, false);

			//Sending Agent
			JobCollection.Load(GetQuery(JobFilterProvider.GetSendingAgentQuery(org1.PK)));
			AssertEquals("Expecting collection to contain Job3", true, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection not to contain Job4", false, JobCollection.Any(x => x.PK == job4.PK));
			AssertEquals("Expecting collection to contain Job5", true, JobCollection.Any(x => x.PK == job5.PK));
			AssertEquals("Expecting collection not to contain Job6", false, JobCollection.Any(x => x.PK == job6.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetSendingAgentQuery(org2.PK)));
			AssertEquals("Expecting collection not to contain Job3", false, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection to contain Job4", true, JobCollection.Any(x => x.PK == job4.PK));
			AssertEquals("Expecting collection not to contain Job5", false, JobCollection.Any(x => x.PK == job5.PK));
			AssertEquals("Expecting collection nto contain Job6", true, JobCollection.Any(x => x.PK == job6.PK));

			//Receiving Agent
			JobCollection.Load(GetQuery(JobFilterProvider.GetReceivingAgentQuery(org1.PK)));
			AssertEquals("Expecting collection not to contain Job3", false, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection to contain Job4", true, JobCollection.Any(x => x.PK == job4.PK));
			AssertEquals("Expecting collection not to contain Job5", false, JobCollection.Any(x => x.PK == job5.PK));
			AssertEquals("Expecting collection nto contain Job6", true, JobCollection.Any(x => x.PK == job6.PK));

			JobCollection.Load(GetQuery(JobFilterProvider.GetReceivingAgentQuery(org2.PK)));
			AssertEquals("Expecting collection to contain Job3", true, JobCollection.Any(x => x.PK == job3.PK));
			AssertEquals("Expecting collection not to contain Job4", false, JobCollection.Any(x => x.PK == job4.PK));
			AssertEquals("Expecting collection to contain Job5", true, JobCollection.Any(x => x.PK == job5.PK));
			AssertEquals("Expecting collection not to contain Job6", false, JobCollection.Any(x => x.PK == job6.PK));
		}

		JobSailing CreateSailing(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime originETD)
		{
			return CreateSailing(vessel, voyage, load, discharge, originETD, originETD, originETD, originETD);
		}

		JobSailing CreateSailing(RefVessel vessel, ZString voyage, ZString load, ZString discharge, ZDateTime originETD, ZDateTime originATD, ZDateTime destinationETA, ZDateTime destinationATA)
		{
			JobVoyage resultVoyage = Factory.New<JobVoyage>();
			resultVoyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			resultVoyage.JV_RV_NKVessel = vessel.RV_FK;
			resultVoyage.JV_VoyageFlight = voyage;

			VoyageOrigin origin = resultVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = originETD;
			origin.JA_A_DEP = originATD;

			VoyageDestination destination = resultVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = destinationETA;
			destination.JB_A_ARV = destinationATA;

			resultVoyage.GenerateSailings();
			return resultVoyage.Sailings[0];
		}

		RefVessel LoadOrCreateVessel(string name, string lloydsNumber)
		{
			var vessel = RefVessel.LookupVesselByName(name, Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = lloydsNumber;
				vessel.RV_Name = name;
			}

			return vessel;
		}
		#endregion

		#region Implementation

		protected JobHeader Job1;
		protected JobHeader Job2;
		protected JobHeader Job3;
		protected JobHeader Job4;

		protected Charge Charge1;
		protected Charge Charge2;
		protected Charge Charge3;
		protected Charge Charge4;

		protected ZDate Date1;
		protected ZDate Date2;

		protected JobFilterProvider JobFilterProvider;
		protected JobCollection JobCollection;

		protected override void SetUp()
		{
			Job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job2.JH_GB = GlbBranch.CurrentBranch.PK;
			Job3.JH_GB = GlbBranch.CurrentBranch.PK;
			Job4.JH_GB = GlbBranch.CurrentBranch.PK;

			Job1.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job2.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job3.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job4.JH_GC = GlbDepartment.CurrentDepartment.PK;

			Job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Job2.JH_GC = GlbCompany.CurrentCompany.PK;
			Job3.JH_GC = GlbCompany.CurrentCompany.PK;
			Job4.JH_GC = GlbCompany.CurrentCompany.PK;

			Charge1 = Factory.NewWithValidTestData<Charge>();
			Charge2 = Factory.NewWithValidTestData<Charge>();
			Charge3 = Factory.NewWithValidTestData<Charge>();
			Charge4 = Factory.NewWithValidTestData<Charge>();

			Charge1.JR_JH = Job1.PK;
			Charge2.JR_JH = Job2.PK;
			Charge3.JR_JH = Job1.PK;
			Charge4.JR_JH = Job2.PK;

			JobFilterProvider = GetFilterProvider();
			JobCollection = new JobCollection(Factory);

			Date1 = new ZDate(2011, 01, 01);
			Date2 = new ZDate(2011, 04, 01);
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion

		protected virtual JobFilterProvider GetFilterProvider()
		{
			return new JobFilterProvider();
		}

		protected virtual ZQuery GetQuery(ZQuery seedQuery)
		{
			return seedQuery;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobFilterProvider();
		}

		void SelectJobType(bool cfsModule, bool consolModule, bool customsModule, bool fwdModule, bool transportModule, bool agencyModule)
		{
			JobFilterProvider.IsCFSModule = cfsModule;
			JobFilterProvider.IsConsolModule = consolModule;
			JobFilterProvider.IsCustomsModule = customsModule;
			JobFilterProvider.IsForwardingModule = fwdModule;
			JobFilterProvider.IsTransportModule = transportModule;
			JobFilterProvider.IsAgencyModule = agencyModule;
		}
	}
}
