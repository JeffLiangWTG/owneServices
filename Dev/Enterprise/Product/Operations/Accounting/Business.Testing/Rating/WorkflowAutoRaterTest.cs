using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class WorkflowAutoRaterTest : TestCaseWithFactory
	{
		public void TestProcess_NoCharges()
		{
			const string expectedNotifications = @"Resetting previously auto-rated charges for Shipment SHIPMENT123
AUTORATING COSTS FOR Shipment SHIPMENT123
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-costed.
	No costs were found.
AUTORATING REVENUE FOR Shipment SHIPMENT123
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-rated.
	No rates were found.
AUTORATING PROFIT SHARE FOR Shipment SHIPMENT123
CHARGES CALCULATED:";

			var localClient = GetLocalClientOrg();
			var shipment = GetShipment("SHIPMENT123", "AUSYD", "USLAX", localClient);
			Factory.Save();

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment)).Process(notifications);

			AssertContains("", expectedNotifications, notifications.AsString);
		}

		public void TestProcess_Invoicing()
		{
			const string expectedNotifications = @"AUTORATING COSTS FOR Shipment SHIPMENT123
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-costed.
	No costs were found.
AUTORATING REVENUE FOR Shipment SHIPMENT123
RatingHeader Found Client Rate MERCEDES Entries: 1
RateLine Found ODOC-FLT-Client Rate MERCEDES
CHARGES CALCULATED:
	ODOC: Base Rate AUD 45.00
Shipment SHIPMENT123 was auto-rated.
	The following rates were found:
	  • ODOC charge from Client Rate MERCEDES
	Charges created: ODOC";

			var localClient = GetLocalClientOrg();
			var header = Factory.New<ClientRate>();
			header.TH_OH = localClient.PK;
			AddCalculator(header, "AUBNE", "NZAKL", "ODOC");
			var shipment = GetShipment("SHIPMENT123", "AUBNE", "NZAKL", localClient);
			Factory.Save();

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment)).Process(notifications);

			AssertContains(expectedNotifications, notifications.AsString);
		}

		public void TestProcess_Costing()
		{
			const string expectedNotifications = @"Resetting previously auto-rated charges for Shipment SHIPMENT456
AUTORATING COSTS FOR Consol CONSOL123
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
(Searching for Freight Cost to update the job) RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
CHARGES CALCULATED:
	FRT: Base Rate AUD 45.00
Consol CONSOL123 was auto-costed.
	The following costs were found:
	  • FRT charge from Standard Costs (TACT/General Rates)
	Charges created: FRT
AUTORATING COSTS FOR Shipment SHIPMENT456
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 2
RateEntry Filtered Standard Costs (TACT/General Rates) reason: Destination didn't match job NLAMS,,AMS,NL,,.
RateEntry Filtered Standard Costs (TACT/General Rates) reason: Origin didn't match job GBLON,,,GB,, and Destination didn't match job NLAMS,,AMS,NL,,.
RateLine Found ODOC-FLT-Standard Costs (TACT/General Rates)
CHARGES CALCULATED:
	ODOC: Base Rate AUD 45.00
Shipment SHIPMENT456 was auto-costed.
	The following costs were found:
	  • ODOC charge from Standard Costs (TACT/General Rates)
	Charges created: ODOC
AUTORATING REVENUE FOR Shipment SHIPMENT456
CHARGES CALCULATED:
Shipment SHIPMENT456 was auto-rated.
	No rates were found.
AUTORATING PROFIT SHARE FOR Shipment SHIPMENT456
CHARGES CALCULATED:";

			var costing = Factory.New<Costing>();
			AddCalculator(costing, "AUSYD", "GBLON", "FRT");
			AddCalculator(costing, "AUSYD", "NLAMS", "ODOC");

			var localClient = GetLocalClientOrg();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL123";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			var shipment = GetShipment("SHIPMENT456", "AUSYD", "NLAMS", localClient);
			consol.Shipments.Add(shipment);
			Factory.Save();

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(consol)).Process(notifications);

			AssertContains("Expected Output", expectedNotifications, notifications.AsString);
		}

		public void TestProcess_SetsJobParentForRatingObjects()
		{
			const string expectedNotifications = @"Resetting previously auto-rated charges for Quick Booking - Booking (S100216)
AUTORATING COSTS FOR Quick Booking - Booking (S100216)
CHARGES CALCULATED:
Quick Booking - Booking (S100216) was auto-costed.
	No costs were found.
AUTORATING REVENUE FOR Quick Booking - Booking (S100216)
RatingHeader Found Client Rate MERCEDES Entries: 1
RateLine Found ODOC-FLT-Client Rate MERCEDES
CHARGES CALCULATED:
	ODOC: Base Rate AUD 45.00
Quick Booking - Booking (S100216) was auto-rated.
	The following rates were found:
	  • ODOC charge from Client Rate MERCEDES
	Charges created: ODOC
AUTORATING PROFIT SHARE FOR Quick Booking - Booking (S100216)
RatingHeader Found Client Rate MERCEDES Entries: 1
RateLine Found ODOC-FLT-Client Rate MERCEDES
RateLine Filtered ODOC-FLT-Client Rate MERCEDES	reason:	Not applicable in rebate calculation mode
CHARGES CALCULATED:";

			var localClient = GetLocalClientOrg();
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "SLO";
			salesRep.GS_EmailAddress = "sales.one@test.com";
			var assignment = localClient.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = salesRep.GS_Code;
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			var header = Factory.New<ClientRate>();
			header.TH_OH = localClient.PK;
			AddCalculator(header, "AUBNE", "NZAKL", "ODOC");
			var booking = QuotedBooking.CreateNewBooking(Factory);
			booking.JS_UniqueConsignRef = "S100216";
			booking.JS_RL_NKOrigin = "AUBNE";
			booking.JS_RL_NKDestination = "NZAKL";
			booking.JS_TransportMode = Core.Constants.TransportModes.Air;
			booking.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			var job = Factory.NewJobForTesting<Job>();
			using (job.GetValidationSuspender())
			{
				job.JH_JobNum = booking.JS_UniqueConsignRef;
				job.JH_ParentID = booking.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = FEADepartment.PK;
				job.JH_GS_NKRepSales = salesRep.GS_Code;
				job.LocalChargesPK = localClient.PK;
				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var quotedBookingReloaded = newFactory.Load<QuotedBooking>(quotedBooking.PK);

				AssertEquals(salesRep.PK, quotedBookingReloaded.Job.RepSales.PK);

				var notifications = new NotificationBuffer();
				((IProcessor)new WorkflowAutoRaterForTest(quotedBookingReloaded)).Process(notifications);

				AssertEquals("Workflow autorater should load Job's Parent to run validation correctly", quotedBookingReloaded.PK, quotedBookingReloaded.Job.Parent.PK);
				AssertContains("Expected Output", expectedNotifications, notifications.AsString);
			}
		}

		public void TestProcess_AutoratesRevenueAndCostsAsExpected()
		{
			var localClient = GetLocalClientOrg();
			var header = Factory.New<ClientRate>();
			header.TH_OH = localClient.PK;
			AddCalculator(header, "AUBNE", "NZAKL", "FRT");
			var shipment = GetShipment("SHIPMENT123", "AUBNE", "NZAKL", localClient);
			Factory.Save();

			string expectedNotifications = @"AUTORATING REVENUE FOR Shipment SHIPMENT123
RatingHeader Found Client Rate MERCEDES Entries: 1
RateLine Found FRT-FLT-Client Rate MERCEDES
CHARGES CALCULATED:
	FRT: Base Rate AUD 45.00
Shipment SHIPMENT123 was auto-rated.
	The following rates were found:
	  • FRT charge from Client Rate MERCEDES
	Charges created: FRT
";

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: true, autoRateCosts: false)).Process(notifications);

			AssertContains("Should only autorate revenue", expectedNotifications, notifications.AsString);
			expectedNotifications = @"AUTORATING COSTS FOR Shipment SHIPMENT123
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-costed.
	No costs were found.";

			notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: false, autoRateCosts: true)).Process(notifications);

			AssertContains(notifications.AsString, expectedNotifications, notifications.AsString);
		}

		public void TestProcess_AutoratesRevenueAndCostsWhenCostsHaveErrors()
		{
			var costing = Factory.New<Costing>();
			AddCalculator(costing, "AU", "", "FRT", 200m);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL123";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			var localClient = GetLocalClientOrg();
			Factory.Save();

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(consol)).Process(notifications);

			#region AutoRating Log when No costs were changed or created due to errors

			var expectedNotifications = @"AUTORATING COSTS FOR Consol CONSOL123
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
(Searching for Freight Cost to update the job) RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
CHARGES CALCULATED:
	FRT: Base Rate AUD 200.00
Consol CONSOL123 has encountered the following errors while AutoRating:
	•  Unapportioned Amount: Please ensure that this Cost Amount is fully apportioned.
Consol CONSOL123 was auto-costed.
	The following costs were found:
	  • FRT charge from Standard Costs (TACT/General Rates)
	No costs were changed or created.
";
			#endregion

			AssertContains("Should log that 'No costs were changed or created.'", expectedNotifications, notifications.AsString);

			var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			Assert("Should have not have tried to save a cost with a validation error", !costs.Any());

			consol.Shipments.Add(GetShipment("SHIPMENT123", "AUMEL", "NZAKL", localClient));
			consol.Shipments.Add(GetShipment("SHIPMENT456", "AUMEL", "NZAKL", localClient));
			Factory.Save();

			notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(consol)).Process(notifications);

			#region AutoRating Log when successful

			expectedNotifications = @"Resetting previously auto-rated charges for Shipment SHIPMENT123
Resetting previously auto-rated charges for Shipment SHIPMENT456
AUTORATING COSTS FOR Consol CONSOL123
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
(Searching for Freight Cost to update the job) RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
CHARGES CALCULATED:
	FRT: Base Rate AUD 200.00
Consol CONSOL123 was auto-costed.
	The following costs were found:
	  • FRT charge from Standard Costs (TACT/General Rates)
	Charges created: FRT
AUTORATING COSTS FOR Shipment SHIPMENT123
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
RateLine Filtered FRT-FLT-Standard Costs (TACT/General Rates)	reason:	FRT charge code is flagged as Consol Level. Consol Level Costs don't apply to SHIPMENT123.
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-costed.
	No costs were found.
AUTORATING COSTS FOR Shipment SHIPMENT456
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found FRT-FLT-Standard Costs (TACT/General Rates)
RateLine Filtered FRT-FLT-Standard Costs (TACT/General Rates)	reason:	FRT charge code is flagged as Consol Level. Consol Level Costs don't apply to SHIPMENT456.
CHARGES CALCULATED:
Shipment SHIPMENT456 was auto-costed.
	No costs were found.
AUTORATING REVENUE FOR Shipment SHIPMENT123
CHARGES CALCULATED:
Shipment SHIPMENT123 was auto-rated.
	No rates were found.
AUTORATING REVENUE FOR Shipment SHIPMENT456
CHARGES CALCULATED:
Shipment SHIPMENT456 was auto-rated.
	No rates were found.
AUTORATING PROFIT SHARE FOR Shipment SHIPMENT123
CHARGES CALCULATED:
AUTORATING PROFIT SHARE FOR Shipment SHIPMENT456
CHARGES CALCULATED:
";

			#endregion

			AssertContains("Should log that errors were found", expectedNotifications, notifications.AsString);

			var consolCost = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).Single();
			AssertNotNull("Now that shipments with jobs have been added, we can create this consol cost", consolCost);
			AssertEquals(200m, consolCost.E6_LocalCostAmount);
			AssertEquals("FRT", consolCost.ChargeCode.AC_Code);

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				var charge = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK)).Single();
				AssertNotNull("Should have created the job charge", charge);
				AssertEquals("FRT", charge.ChargeCode.AC_Code);
				AssertEquals(true, charge.JR_IsApportioned);
				AssertEquals("Consol cost was halved between the two shipments", 100m, charge.JR_LocalCostAmt);
			}
		}

		public void TestProcess_AutoratingCosts_NonConsolLevelChargeOnlyEnabled()
		{
			var shipment = SetupShipmentToTestAutorateNonConsolLevelChargeOnlyOption();

			Factory.Save();

			const string expectedNotifications = @"AUTORATING COSTS FOR Shipment S00001000
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found BAF-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Found FRT-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Filtered FRT-UNT-KG-Standard Costs (TACT/General Rates)	reason:	FRT charge code is flagged as Consol Level. Consol Level Costs don't apply to S00001000.
Chargeable was added for
				RateLine BAF-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
CHARGES CALCULATED:
	BAF: 5000 Kilogram(s) @ AUD 2.00/KG
Shipment S00001000 was auto-costed.
	The following costs were found:
	  • BAF charge from Standard Costs (TACT/General Rates)
	Charges created: BAF";

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: false, autoRateCosts: true, excludeConsolLevelCharges: true)).Process(notifications);

			AssertContains("Expected Output", expectedNotifications, notifications.AsString);
		}

		public void TestProcess_AutoratingCosts_NonConsolLevelChargeOnlyDisabled()
		{
			var shipment = SetupShipmentToTestAutorateNonConsolLevelChargeOnlyOption();

			Factory.Save();

			const string expectedNotifications = @"AUTORATING COSTS FOR Shipment S00001000
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found BAF-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Found FRT-UNT-KG-Standard Costs (TACT/General Rates)
Chargeable was added for
				RateLine BAF-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
				RateLine FRT-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
CHARGES CALCULATED:
	BAF: 5000 Kilogram(s) @ AUD 2.00/KG
	FRT: 5000 Kilogram(s) @ AUD 1.00/KG
Shipment S00001000 was auto-costed.
	The following costs were found:
	  • BAF charge from Standard Costs (TACT/General Rates)
	  • FRT charge from Standard Costs (TACT/General Rates)
	Charges created: BAF, FRT";

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: false, autoRateCosts: true, excludeConsolLevelCharges: false)).Process(notifications);

			var actualNotifications = notifications.AsString;

			AssertContains(
				"Expected Output",
				expectedNotifications,
				actualNotifications);
			AssertNotContains(
				"Not Expected Output",
				"charge code is flagged as Consol Level. Consol Level Costs don't apply to",
				actualNotifications);
		}

		public void TestProcess_AutoratingCostsAndRevenue_NonConsolLevelChargeOnlyEnabled()
		{
			var shipment = SetupShipmentToTestAutorateNonConsolLevelChargeOnlyOption();

			Factory.Save();

			const string expectedNotifications = @"AUTORATING COSTS FOR Shipment S00001000
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found BAF-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Found FRT-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Filtered FRT-UNT-KG-Standard Costs (TACT/General Rates)	reason:	FRT charge code is flagged as Consol Level. Consol Level Costs don't apply to S00001000.
Chargeable was added for
				RateLine BAF-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
CHARGES CALCULATED:
	BAF: 5000 Kilogram(s) @ AUD 2.00/KG
Shipment S00001000 was auto-costed.
	The following costs were found:
	  • BAF charge from Standard Costs (TACT/General Rates)
	Charges created: BAF
AUTORATING REVENUE FOR Shipment S00001000
CHARGES CALCULATED:
Shipment S00001000 was auto-rated.
	No rates were found.";

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: true, autoRateCosts: true, excludeConsolLevelCharges: true)).Process(notifications);

			AssertContains("Expected Output", expectedNotifications, notifications.AsString);
		}

		public void TestProcess_AutoratingCostsAndRevenue_NonConsolLevelChargeOnlyDisabled()
		{
			var shipment = SetupShipmentToTestAutorateNonConsolLevelChargeOnlyOption();

			Factory.Save();

			const string expectedNotifications = @"AUTORATING COSTS FOR Shipment S00001000
RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1
RateLine Found BAF-UNT-KG-Standard Costs (TACT/General Rates)
RateLine Found FRT-UNT-KG-Standard Costs (TACT/General Rates)
Chargeable was added for
				RateLine BAF-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
				RateLine FRT-UNT-KG-Standard Costs (TACT/General Rates)
					Job's info:
					: 100 Weight
					: 30 Volume
					: 100 Chargeable
CHARGES CALCULATED:
	BAF: 5000 Kilogram(s) @ AUD 2.00/KG
	FRT: 5000 Kilogram(s) @ AUD 1.00/KG
Shipment S00001000 was auto-costed.
	The following costs were found:
	  • BAF charge from Standard Costs (TACT/General Rates)
	  • FRT charge from Standard Costs (TACT/General Rates)
	Charges created: BAF, FRT
AUTORATING REVENUE FOR Shipment S00001000
CHARGES CALCULATED:
Shipment S00001000 was auto-rated.
	No rates were found.";

			var notifications = new NotificationBuffer();
			((IProcessor)new WorkflowAutoRaterForTest(shipment, autoRateRevenue: true, autoRateCosts: true, excludeConsolLevelCharges: false)).Process(notifications);

			var actualNotifications = notifications.AsString;

			AssertContains(
				"Expected Output",
				expectedNotifications,
				actualNotifications);
			AssertNotContains(
				"Not Expected Output",
				"charge code is flagged as Consol Level. Consol Level Costs don't apply to",
				actualNotifications);
		}

		ForwardingShipment SetupShipmentToTestAutorateNonConsolLevelChargeOnlyOption()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Debtor Full Name";
			consignor.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromMonthEnd;
			consignor.CompanyData.OB_APPaymentTermDays = 90;
			consignor.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			creditor.CompanyData.OB_APPaymentTermDays = 100;
			creditor.OH_FullName = "Transport Provider One";
			creditor.OH_IsCreditor = true;

			var rate = Factory.New<Costing>();
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtRateLine.TL_LineOrder = 0;
			frtRateLine.ChargeCode.AC_IsGroupageCharge = true;
			frtRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			frtRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			frtRateLine.TL_RX_NKCurrency = "AUD";

			var bafRateLine = entry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafRateLine.TL_LineOrder = 1;
			bafRateLine.ChargeCode.AC_IsGroupageCharge = false;
			bafRateLine.ChargeCode.AC_DepartmentFilterList = "ALL";
			bafRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;
			bafRateLine.TL_RX_NKCurrency = "AUD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = creditor.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";
			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 30m;
			shipment.JS_ActualChargeable = 100m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD";
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			var testJob = Factory.NewJobForTesting<Job>();
			testJob.SuspendValidation();
			testJob.Parent = shipment;
			testJob.JH_JobNum = shipment.JS_UniqueConsignRef;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")).PK;
			testJob.PlugInData = shipment;
			testJob.LocalChargesPK = creditor.PK;
			testJob.JH_A_JOP = ZDateTime.Now;

			return shipment;
		}

		ForwardingShipment GetShipment(string shipmentNo, string origin, string destination, OrgHeader localClient)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;

			var job = Factory.NewJobForTesting<Job>();
			job.SuspendValidation();
			job.JH_JobNum = shipmentNo;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = FEADepartment.PK;
			job.LocalChargesPK = localClient.PK;

			return shipment;
		}

		void AddCalculator(RatingHeader header, string origin, string destination, string chargeCode, decimal amount = 45m)
		{
			var rateCategory = chargeCode == "ODOC" ? RatingConstants.RateCategory.ORG : RatingConstants.RateCategory.AIR;
			var entry = header.AddRateEntry(rateCategory, Core.Constants.ContainerModes.Loose, origin, destination);
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = amount;
		}

		OrgHeader GetLocalClientOrg()
		{
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_IsDebtor = true;
			localClient.OH_Code = "MERCEDES";

			return localClient;
		}

		GlbDepartment FEADepartment => feaDepartment ?? (feaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")));
		GlbDepartment feaDepartment;

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();

			disableRatesServiceSubscription = DataRegistryRating.Instance.RatesServiceSubscription
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
		}

		protected override void TearDown()
		{
			disableRatesServiceSubscription.Dispose();

			base.TearDown();
		}

		IDisposable disableRatesServiceSubscription;

		#endregion
	}

	public class WorkflowAutoRaterForTest : WorkflowAutoRater
	{
		public WorkflowAutoRaterForTest(IBusiness ratingObject, bool autoRateRevenue = true, bool autoRateCosts = true, bool excludeConsolLevelCharges = false)
			: base(ratingObject, autoRateRevenue, autoRateCosts, excludeConsolLevelCharges)
		{
		}
	}
}
