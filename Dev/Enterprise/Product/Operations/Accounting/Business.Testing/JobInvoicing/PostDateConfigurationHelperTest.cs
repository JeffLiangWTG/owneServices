using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PostDateConfigurationHelperTest : TestCaseWithFactory
	{
		#region Tests

		[TestDate(2011, 04, 01)]
		public void TestPriorClosedPeriod_EPM()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Air, true, 201103, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 05));
		}

		[TestDate(2011, 04, 01)]
		public void TestPriorClosedPeriod_EPP()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Sea, true, 201103, new ZDateTime(2011, 03, 28), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 05));
		}

		[TestDate(2011, 04, 01)]
		public void TestPriorOpenPeriod()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01));
		}

		[TestDate(2011, 04, 25)]
		public void TestCurrentPeriod()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 04, 15), new ZDateTime(2011, 04, 15), new ZDateTime(2011, 04, 20));
		}

		[TestDate(2011, 04, 25)]
		public void TestCurrentPeriod_EPPAndPriorPeriodClosed()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Courier, true, 201103, new ZDateTime(2011, 03, 28), new ZDateTime(2011, 04, 25), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 20));
		}

		[TestDate(2011, 04, 25)]
		public void TestCurrentPeriod_EPMClosed()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.SeaAir, true, 201103, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 04, 25), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 20));
		}

		[TestDate(2011, 04, 25)]
		public void TestPostDateNotInPeriod()
		{
			ZDateTime invoiceDate = new ZDateTime("2010, 12, 01");
			ZDateTime defaultDate = ZDateTime.Today;

			AssertEquals("Precondition: Invoice Date does not fall into an accounting period.", PostDateValidationResult.PeriodNotFound, new AccountingPeriodCalculator(Factory).IsPostDateValid(invoiceDate));

			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();
			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "ALL", "", "", "", "ADD", "", "", "INV", "");
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
			PostDateConfigurationHelper helper = new PostDateConfigurationHelper(new DummyJobInvoicingPlugIn(Factory));
			PostDateConfiguration config = helper.FindPostDateConfiguration();

			AssertEquals("JobType", "ALL", config.JobType);
			AssertEquals("SignificantDateCode", "ADD", config.SignificantDateCode);
			AssertEquals("CurrentPeriod", "INV", config.CurrentPeriod);
			AssertEquals("Post Date should fall back to Default Date", defaultDate, helper.GetPostDate(defaultDate, invoiceDate));
		}

		[TestDate(2011, 04, 01)]
		public void TestFuturePeriod()
		{
			CreateShipmentAndAssertPostDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 04, 01), new ZDateTime(2011, 05, 15), new ZDateTime(2011, 04, 03));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentActualArrivalDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.Rail,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentActualDepartureDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Rail,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentCustomsClearanceDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate, Constants.TransportModes.Air,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 04), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentJobOpenDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Sea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentPickupDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.PickupDate, Constants.TransportModes.Sea,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentDeliveryDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.DeliveryDate, Constants.TransportModes.Road,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentAWBIssueDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.AWBIssueDate, Constants.TransportModes.Air,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentInvoiceAddDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate, Constants.TransportModes.Road,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 01), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 10)]
		public void TestShipmentHouseBillIssueDate()
		{
			CreateShipmentAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.HouseBillIssueDate, Constants.TransportModes.AirSea,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 09), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestConsolActualArrivalDate()
		{
			CreateConsolAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.Air,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2011, 04, 01)]
		public void TestConsolActualDepartureDate()
		{
			CreateConsolAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Air,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2011, 04, 05)]
		public void TestConsolInvoiceDate()
		{
			CreateConsolAndAssertPostDate(PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate, Constants.TransportModes.Road,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 04), new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2012, 04, 01)]
		public void TestShipment_Reversal_StandardPostingRules()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.SeaAir,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 02, 28));
		}

		[TestDate(2012, 04, 01)]
		public void TestShipment_Reversal_FirstDayOfFirstOpenPeriodAfterSignificantDate()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate, Constants.TransportModes.SeaAir,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 03, 01), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 02, 28));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionPostDateOrCurrentDate()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.AirSea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionPostDateOrCurrentDate_ClosedPeriod()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.AirSea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate, new ZDateTime(2012, 02, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Courier,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod_ClosedPeriod()
		{
			CreateShipmentAndAssertPostDateForReversal(PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Courier,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 01), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 03, 12),
				PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod, new ZDateTime(2012, 02, 06));
		}

		#endregion

		#region Implementation

		void CreateShipmentAndAssertPostDate(ZString transportMode, bool closePriorLedger, ZInt periodToClose, ZDateTime periodEndDate, ZDateTime postDate, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				if (closePriorLedger)
				{
					CloseSubLedger(periodToClose);
					AlterSubLedgerEndDate(periodToClose, periodEndDate);
				}
				Factory.Save();

				SetupRegistryForShipmentsAndConsols();

				creator = new TestObjectCreator(Factory);

				ForwardingShipment shipment;
				SetupShipmentJob(out shipment, out job, transportMode, "AUSYD", "SGSIN", departureDate, arrivalDate);

				PostDateConfigurationHelper helper = new PostDateConfigurationHelper(shipment);
				PostDateConfiguration config = helper.FindPostDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", "EXP", config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);

				AssertEquals("Post Date", postDate, helper.GetPostDate(ZDateTime.Today, ZDateTime.Today.AddDays(-1)));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateShipmentAndAssertPostDate(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime postDate, ZDateTime departureDate, ZDateTime arrivalDate, ZDateTime customsClearanceDate,
			ZDateTime pickupDate, ZDateTime deliveryDate, ZDateTime jobOpeningDate, ZDateTime aWBIssueDate, ZDateTime houseBillIssueDate)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				SetupRegistryForShipments();

				creator = new TestObjectCreator(Factory);

				ForwardingShipment shipment;
				ForwardingConsol consol;
				SetupShipmentJobWithConsol(out consol, out shipment, out job, transportMode, origin, destination, departureDate, arrivalDate, houseBillIssueDate);

				StmALog log = shipment.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = Events.CustomsCleared.Code;
					log.SL_EventTime = customsClearanceDate;
				}

				job.JH_A_JOP = jobOpeningDate;

				shipment.DocsAndCartage.JP_EstimatedPickup = pickupDate;
				shipment.DocsAndCartage.JP_EstimatedDelivery = deliveryDate;

				consol.JK_MasterBillIssueDate = aWBIssueDate;

				consol.Transports.ArrivalTransport.JW_ATA = arrivalDate;
				consol.Transports.DepartureTransport.JW_ATD = departureDate;

				Factory.Save();

				PostDateConfigurationHelper helper = new PostDateConfigurationHelper(shipment);
				PostDateConfiguration config = helper.FindPostDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);

				AssertEquals("Post Date", postDate, helper.GetPostDate(ZDateTime.Today, ZDateTime.Today.AddDays(-1)));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateShipmentAndAssertPostDateForReversal(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime postDate, ZDateTime departureDate, ZDateTime arrivalDate, ZDateTime customsClearanceDate,
			ZDateTime pickupDate, ZDateTime deliveryDate, ZDateTime jobOpeningDate, ZDateTime aWBIssueDate,
			string reversalRule, ZDateTime originalPostDate)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				CloseSubLedger(Convert.ToInt32(string.Format("{0}01", ZDateTime.Today.Year)));
				CloseSubLedger(Convert.ToInt32(string.Format("{0}02", ZDateTime.Today.Year)));
				SetupRegistryForShipments();

				creator = new TestObjectCreator(Factory);

				ForwardingShipment shipment;
				ForwardingConsol consol;
				SetupShipmentJobWithConsol(out consol, out shipment, out job, transportMode, origin, destination, departureDate, arrivalDate);

				StmALog log = shipment.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = Events.CustomsCleared.Code;
					log.SL_EventTime = customsClearanceDate;
				}

				job.JH_A_JOP = jobOpeningDate;

				shipment.DocsAndCartage.JP_EstimatedPickup = pickupDate;
				shipment.DocsAndCartage.JP_EstimatedDelivery = deliveryDate;

				consol.JK_MasterBillIssueDate = aWBIssueDate;

				consol.Transports.ArrivalTransport.JW_ATA = arrivalDate;
				consol.Transports.DepartureTransport.JW_ATD = departureDate;

				Factory.Save();

				PostDateConfigurationHelper helper = new PostDateConfigurationHelper(shipment, true);
				PostDateConfiguration config = helper.FindPostDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);
				AssertEquals("ReversalRule", reversalRule, config.ReversalRule);

				AssertEquals("Post Date", postDate, helper.GetPostDate(ZDateTime.Today, ZDateTime.Today.AddDays(-1), originalPostDate));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateConsolAndAssertPostDate(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime postDate, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				SetupRegistryForShipmentsAndConsols();

				creator = new TestObjectCreator(Factory);

				ForwardingShipment shipment;
				ForwardingConsol consol;
				SetupShipmentJobWithConsol(out consol, out shipment, out job, transportMode, origin, destination, departureDate, arrivalDate);

				Factory.Save();

				PostDateConfigurationHelper helper = new PostDateConfigurationHelper(consol);
				PostDateConfiguration config = helper.FindPostDateConfiguration();
				AssertEquals("JobType", "FCN", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);

				AssertEquals("Post Date", postDate, helper.GetPostDate(ZDateTime.Today, ZDateTime.Today.AddDays(-1)));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CloseSubLedger(ZInt period)
		{
			ZQuery query = new ZQuery(AccPeriodManagementSchema.AM_Period, period);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			AccPeriodManagement periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			periodManagement.AM_IsSubLedgerClosed = true;
		}

		void AlterSubLedgerEndDate(ZInt period, ZDateTime endDate)
		{
			ZQuery query = new ZQuery(AccPeriodManagementSchema.AM_Period, period);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			AccPeriodManagement periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			periodManagement.AM_EndDate = endDate;

			query = new ZQuery(AccPeriodManagementSchema.AM_Period, period + 1);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			periodManagement.AM_StartDate = endDate.AddDays(1);
		}

		TestObjectCreator creator;

		void SetupShipmentJob(out ForwardingShipment shipment, out Job job, ZString transportMode, ZString origin, ZString destination, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			shipment = creator.CreateShipment("S00001001", origin, destination);
			shipment.JS_TransportMode = transportMode;
			shipment.JS_E_DEP = departureDate;
			shipment.JS_E_ARV = arrivalDate;

			job = creator.CreateJob(shipment);
			job.LocalChargesPK = creator.ABIGAS.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OSSellAmt = 200m;
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;

			Factory.Save();
		}

		void SetupShipmentJobWithConsol(out ForwardingConsol consol, out ForwardingShipment shipment, out Job job, ZString transportMode, ZString origin, ZString destination, ZDateTime departureDate
			, ZDateTime arrivalDate, ZDateTime houseBillIssueDate = default(ZDateTime))
		{
			consol = creator.CreateConsol(origin, destination, "C00001001");
			consol.JK_TransportMode = transportMode;
			consol.Transports.ArrivalTransport.JW_ATA = arrivalDate;
			consol.Transports.DepartureTransport.JW_ATD = departureDate;

			shipment = creator.CreateShipment("S00001001", origin, destination, consol);
			shipment.JS_TransportMode = transportMode;
			shipment.JS_E_DEP = departureDate;
			shipment.JS_E_ARV = arrivalDate;
			shipment.JS_HouseBillIssueDate = houseBillIssueDate;

			job = creator.CreateJob(shipment);
			job.LocalChargesPK = creator.ABIGAS.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OSSellAmt = 200m;
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;

			Factory.Save();
		}

		void SetupPeriodManagement()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void SetupRegistryForShipmentsAndConsols()
		{
			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();

			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();

			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "AIR", "ALL", "DEP", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "SEA", "ALL", "DEP", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "AIR", "ALL", "ARV", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "SEA", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "ADD", "", "", "EPP", "");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "FSA", "ALL", "ADD", "", "", "EPM", "");

			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "EXP", "AIR", "", "DEP", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "EXP", "SEA", "", "DEP", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "IMP", "AIR", "", "ARV", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "IMP", "SEA", "", "ARV", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "FCN", "IMP", "ROA", "", "INV", "ADD", "ADD", "INV", "ADD");

			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
		}

		void SetupRegistryForShipments()
		{
			BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration = new BackDateAPInvoicesConfiguration();

			backDateAPInvoicesConfiguration.PostDateConfigurationCollection.RemoveAll();

			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "FAS", "ALL", "HBD", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "AIR", "ALL", "CUS", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "SEA", "ALL", "PIC", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "ROA", "ALL", "DEL", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "RAI", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "AIR", "ALL", "AWB", "EPM", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "SEA", "ALL", "JOP", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "ROA", "ALL", "ADD", "", "", "ADD", "");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "RAI", "ALL", "DEP", "EPP", "SGN", "SGN", "ADD");
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "FSA", "ALL", "DEP", "EPM", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "EXP", "FSA", "ALL", "CUS", "FDO", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "FAS", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate);
			AddPostDateConfiguration(backDateAPInvoicesConfiguration, "SHP", "IMP", "COU", "ALL", "JOP", "EPM", "SGN", "SGN", "ADD", PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);

			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateAPInvoicesConfiguration);
		}

		void AddPostDateConfiguration(BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration, string jobType, string direction, string mode, string broker,
									string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod,
									string reversalRule = PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules)
		{
			PostDateConfiguration config = backDateAPInvoicesConfiguration.PostDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
			config.ReversalRule = reversalRule;
		}

		#endregion
	}
}