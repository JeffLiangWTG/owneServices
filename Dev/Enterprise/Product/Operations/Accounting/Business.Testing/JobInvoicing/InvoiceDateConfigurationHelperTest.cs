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
	public class InvoiceDateConfigurationHelperTest : TestCaseWithFactory
	{
		#region Tests

		[TestDate(2011, 04, 01)]
		public void TestPriorClosedPeriod_EPM()
		{
			CreateShipmentAndAssertInvoiceDate(Constants.TransportModes.Air, true, 201103, new ZDateTime(2011, 03, 31), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01));
		}

		[TestDate(2011, 04, 01)]
		public void TestPriorClosedPeriod_EPP()
		{
			CreateShipmentAndAssertInvoiceDate(Constants.TransportModes.Sea, true, 201103, new ZDateTime(2011, 03, 28), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01));
		}

		[TestDate(2011, 04, 01)]
		public void TestPriorOpenPeriod()
		{
			CreateShipmentAndAssertInvoiceDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 15), new ZDateTime(2011, 04, 01));
		}

		[TestDate(2011, 04, 01)]
		public void TestCurrentPeriod()
		{
			CreateShipmentAndAssertInvoiceDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 04, 15), new ZDateTime(2011, 04, 15), new ZDateTime(2011, 04, 20));
		}

		[TestDate(2011, 04, 01)]
		public void TestFuturePeriod()
		{
			CreateShipmentAndAssertInvoiceDate(Constants.TransportModes.Air, false, 0, new ZDateTime(2011, 04, 01), new ZDateTime(2011, 05, 15), new ZDateTime(2011, 04, 03));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentActualArrivalDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.Rail,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentActualDepartureDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Rail,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentCustomsClearanceDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate, Constants.TransportModes.Air,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 04), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentJobOpenDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Sea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentPickupDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.PickupDate, Constants.TransportModes.Sea,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentDeliveryDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.DeliveryDate, Constants.TransportModes.Road,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentAWBIssueDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.AWBIssueDate, Constants.TransportModes.Air,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentInvoiceAddDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate, Constants.TransportModes.Road,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 04, 01), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestShipmentHouseBillIssueDate()
		{
			CreateShipmentAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.HouseBillIssueDate, Constants.TransportModes.AirSea,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 04, 09), new ZDateTime(2011, 04, 02), new ZDateTime(2011, 04, 03), new ZDateTime(2011, 04, 04),
				new ZDateTime(2011, 04, 05), new ZDateTime(2011, 04, 06), new ZDateTime(2011, 04, 07), new ZDateTime(2011, 04, 08), new ZDateTime(2011, 04, 09));
		}

		[TestDate(2011, 04, 01)]
		public void TestConsolActualArrivalDate()
		{
			CreateConsolAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.Air,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2011, 03, 15), new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2011, 04, 01)]
		public void TestConsolActualDepartureDate()
		{
			CreateConsolAndAssertInvoiceDate(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Air,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 05), new ZDateTime(2011, 03, 15));
		}

		[TestDate(2012, 04, 01)]
		public void TestShipment_Reversal_StandardPostingRules()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.SeaAir,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 01)]
		public void TestShipment_Reversal_FirstDayOfFirstOpenPeriodAfterSignificantDate()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.CustomsClearanceDate, Constants.TransportModes.SeaAir,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 03, 01), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionInvoiceDateOrCurrentDate()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.AirSea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionInvoiceDateOrCurrentDate_ClosedPeriod()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate, Constants.TransportModes.AirSea,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 03, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate, new ZDateTime(2012, 02, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Courier,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod, new ZDateTime(2012, 03, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod_ClosedPeriod()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.JobOpenDate, Constants.TransportModes.Courier,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 03, 01), new ZDateTime(2012, 03, 05), new ZDateTime(2012, 03, 10), new ZDateTime(2012, 03, 09),
				new ZDateTime(2012, 03, 08), new ZDateTime(2012, 03, 11), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 03, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod, new ZDateTime(2012, 02, 06));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_ClosedPeriod_OriginalInvoiceDateIsClosed()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 02, 01), new ZDateTime(2012, 02, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 02, 05));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_ClosedPeriod_OriginalInvoiceDateNotClosed()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 02, 01), new ZDateTime(2012, 02, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 04, 01));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_FuturePeriod_OriginalInvoiceDateIsClosed()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.SeaAir,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 06, 06), new ZDateTime(2012, 06, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 02, 05));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_FuturePeriod_OriginalInvoiceDateNotClosed()
		{
			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.SeaAir,
				"SGSIN", "AUSYD", "IMP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 06, 06), new ZDateTime(2012, 06, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 04, 01));
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_OpenPeriod_OriginalInvoiceDateIsClosed()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;
			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "DEP", "ADD", "ADD", "SGN", "SGN", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);

			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 01, 02), false);
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_OpenPeriod_OriginalInvoiceDateNotClosed()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;
			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "DEP", "ADD", "ADD", "SGN", "SGN", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);

			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 03, 06), new ZDateTime(2012, 03, 07), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 03, 08), false);
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_CurrentPeriod_OriginalInvoiceDateIsClosed()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;
			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "DEP", "ADD", "ADD", "ADD", "SGN", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);

			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 04, 01), new ZDateTime(2012, 04, 02), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 01, 02), false);
		}

		[TestDate(2012, 04, 03)]
		public void TestShipment_Reversal_Standard_CurrentPeriod_OriginalInvoiceDateNotClosed()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;
			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "DEP", "ADD", "ADD", "ADD", "SGN", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);

			CreateShipmentAndAssertInvoiceDateForReversal(InvoiceDateConfigurationLookups.SignificantDateCodes.ActualDepartureDate, Constants.TransportModes.Courier,
				"AUSYD", "SGSIN", "EXP", new ZDateTime(2012, 04, 03), new ZDateTime(2012, 04, 01), new ZDateTime(2012, 04, 02), new ZDateTime(2012, 02, 09),
				new ZDateTime(2012, 02, 12), new ZDateTime(2012, 02, 13), new ZDateTime(2012, 02, 04), new ZDateTime(2012, 02, 12),
				InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules, new ZDateTime(2012, 04, 02), false);
		}

		#endregion

		#region Implementation

		void CreateShipmentAndAssertInvoiceDate(ZString transportMode, bool closePriorLedger, ZInt periodToClose, ZDateTime invoiceDate, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				if (closePriorLedger)
				{
					CloseSubLedger(periodToClose);
					AlterSubLedgerEndDate(periodToClose, invoiceDate);
				}
				Factory.Save();

				SetupRegistryForShipmentsAndConsols();

				creator = new TestObjectCreator(Factory);

				ForwardingShipment shipment;
				SetupShipmentJob(out shipment, out job, transportMode, "AUSYD", "SGSIN", departureDate, arrivalDate);

				InvoiceDateConfigurationHelper helper = GetInvoiceDateConfigurationHelperForShipment(shipment);
				InvoiceDateConfiguration config = helper.FindInvoiceDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", "EXP", config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);

				AssertEquals("Invoice Date", invoiceDate, helper.GetInvoiceDate(ZDateTime.Today));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateShipmentAndAssertInvoiceDate(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime invoiceDate, ZDateTime departureDate, ZDateTime arrivalDate, ZDateTime customsClearanceDate,
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

				InvoiceDateConfigurationHelper helper = GetInvoiceDateConfigurationHelperForShipment(shipment);
				InvoiceDateConfiguration config = helper.FindInvoiceDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);

				AssertEquals("Invoice Date", invoiceDate, helper.GetInvoiceDate(ZDateTime.Today));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateShipmentAndAssertInvoiceDateForReversal(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime invoiceDate, ZDateTime departureDate, ZDateTime arrivalDate, ZDateTime customsClearanceDate,
			ZDateTime pickupDate, ZDateTime deliveryDate, ZDateTime jobOpeningDate, ZDateTime aWBIssueDate,
			string reversalRule, ZDateTime originalTransactionDate, bool needToSetupRegistry = true)
		{
			Job job = null;
			try
			{
				SetupPeriodManagement();
				CloseSubLedger(Convert.ToInt32(string.Format("{0}01", ZDateTime.Today.Year)));
				CloseSubLedger(Convert.ToInt32(string.Format("{0}02", ZDateTime.Today.Year)));
				if (needToSetupRegistry)
				{
					SetupRegistryForShipments();
				}

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

				InvoiceDateConfigurationHelper helper = GetInvoiceDateConfigurationHelperForShipment(shipment, true);
				InvoiceDateConfiguration config = helper.FindInvoiceDateConfiguration();
				AssertEquals("JobType", "SHP", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "ALL", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);
				AssertEquals("ReversalRule", reversalRule, config.ReversalRule);

				AssertEquals("Invoice Date", invoiceDate, helper.GetInvoiceDate(originalTransactionDate));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		void CreateConsolAndAssertInvoiceDate(string significantDateCode, string transportMode, string origin, string destination, string direction,
			ZDateTime invoiceDate, ZDateTime departureDate, ZDateTime arrivalDate)
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

				InvoiceDateConfigurationHelper helper = GetInvoiceDateConfigurationHelperForShipment(consol);
				InvoiceDateConfiguration config = helper.FindInvoiceDateConfiguration();
				AssertEquals("JobType", "FCN", config.JobType);
				AssertEquals("Direction", direction, config.DirectionCode);
				AssertEquals("Transport Mode", transportMode, config.Mode);
				AssertEquals("Broker", "", config.BrokerCode);
				AssertEquals("SignificantDateCode", significantDateCode, config.SignificantDateCode);

				AssertEquals("Invoice Date", invoiceDate, helper.GetInvoiceDate(ZDateTime.Today));
			}
			finally
			{
				if (job != null)
				{
					job.Dispose();
				}
			}
		}

		InvoiceDateConfigurationHelper GetInvoiceDateConfigurationHelperForShipment(IJobInvoicingPlugIn plugIn, bool reversing = false)
		{
			OperationsJobConfigurationCodes codes = new OperationsJobConfigurationCodes(plugIn);
			InvoiceDateConfigurationHelper helper = new InvoiceDateConfigurationHelper(codes, plugIn, reversing);
			return helper;
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
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;

			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();

			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "AIR", "ALL", "DEP", "EPM", "SGN", "SGN", "ADD", false, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "SEA", "ALL", "DEP", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "AIR", "ALL", "ARV", "EPM", "SGN", "SGN", "ADD", false, false);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "SEA", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", false, false);

			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "EXP", "AIR", "", "DEP", "EPM", "SGN", "SGN", "ADD", false, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "EXP", "SEA", "", "DEP", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "IMP", "AIR", "", "ARV", "EPM", "SGN", "SGN", "ADD", false, false);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "FCN", "IMP", "SEA", "", "ARV", "EPP", "SGN", "SGN", "ADD", false, false);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);
		}

		void SetupRegistryForShipments()
		{
			BackDateInvoicesConfiguration backDateInvoicesConfiguration = new BackDateInvoicesConfiguration();
			backDateInvoicesConfiguration.OverridePostDate = false;
			backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate = false;

			backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.RemoveAll();

			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "FAS", "ALL", "HBD", "SGN", "SGN", "SGN", "ADD", false, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "AIR", "ALL", "CUS", "EPM", "SGN", "SGN", "ADD", false, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "SEA", "ALL", "PIC", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "ROA", "ALL", "DEL", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "RAI", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "AIR", "ALL", "AWB", "EPM", "SGN", "SGN", "ADD", false, false);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "SEA", "ALL", "JOP", "EPP", "SGN", "SGN", "ADD", false, false);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "ROA", "ALL", "ADD", "", "", "ADD", "", false, false);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "RAI", "ALL", "DEP", "EPP", "SGN", "SGN", "ADD", true, true);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "FSA", "ALL", "DEP", "EPM", "SGN", "SGN", "ADD", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "FSA", "ALL", "CUS", "FDO", "SGN", "SGN", "ADD", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "FAS", "ALL", "ARV", "EPP", "SGN", "SGN", "ADD", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "IMP", "COU", "ALL", "JOP", "EPM", "SGN", "SGN", "ADD", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);
			AddInvoiceDateConfiguration(backDateInvoicesConfiguration, "SHP", "EXP", "COU", "ALL", "DEP", "ADD", "SGN", "SGN", "SGN", true, true, InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoicesConfiguration);
		}

		void AddInvoiceDateConfiguration(BackDateInvoicesConfiguration backDateInvoicesConfiguration, string jobType, string direction, string mode, string broker,
				string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod, bool @override, bool today,
				string reversalRule = InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules)
		{
			InvoiceDateConfiguration config = backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
			config.Override = @override;
			config.Today = today;
			config.ReversalRule = reversalRule;
		}

		#endregion
	}
}