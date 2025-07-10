using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AutoRatingActionMethodApplicator))]
	internal class AutoRateCostsRevenueActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestRunAction_EmptyTargets

		public void TestRunAction_EmptyTargets()
		{
			ApplyApplicator(Array.Empty<BusinessObject>(), "");
		}

		#endregion

		#region TestRunAction_NoJob

		public void TestRunAction_NoJob()
		{
			const string jobErrorLog = @"ERROR: [HL Shipment S00000101]: Shipment S00000101 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Please enter a Department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep.";

			const string jobCreatedLog =
				@"INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-rated.
	No rates were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-rated.
	No rates were found.";

			var shipment1 = CreateShipment("S00000101", "K00000101");
			Factory.Save();

			ApplyApplicator("Expected errors. AutoRatingStarter will always select yes to try to create a job when called from logs", new BusinessObject[] { shipment1 }, jobErrorLog);

			var validDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES"));
			CreateJob(shipment1);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), validDepartment.PK.ToGuid()))
			{
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				var docAddress = shipment1.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
				docAddress.E2_OA_Address = localClient.MainAddress.PK;

				Factory.Save();

				using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
				{
					ApplyApplicator("Expected no errors on generated job which should be autorated without finding any rates/charges", new BusinessObject[] { shipment1 }, jobCreatedLog);
				}
			}
		}

		#endregion

		#region TestRunAction_NoCharges

		public void TestRunAction_NoCharges()
		{
			const string expectedLog =
@"INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-rated.
	No rates were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-rated.
	No rates were found.
INFO: [HL Shipment S00000102]: Shipment S00000102 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000102]: Warnings Encountered:
Shipment S00000102 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000102]: Shipment S00000102 was auto-rated.
	No rates were found.
WARNING: [HL Shipment S00000102]: Warnings Encountered:
Shipment S00000102 was auto-rated.
	No rates were found.";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var shipment1 = CreateShipment("S00000101", "K00000101");
			CreateJob(shipment1).LocalChargesPK = org.PK;

			var shipment2 = CreateShipment("S00000102", "K00000102");
			CreateJob(shipment2).LocalChargesPK = org.PK;

			Factory.Save();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				ApplyApplicator(new BusinessObject[] { shipment1, shipment2 }, expectedLog);
			}
		}

		#endregion

		#region TestRunAction_CompanyTariff

		public void TestRunAction_CompanyTariff()
		{
			var header = Factory.New<CompanyTariff>();
			header.TH_GlobalRateLevel = 42;
			header.TH_GlobalRateDescription = "the answer to all rates";

			AddCalculator(header, "AU", "LCL", "ORG", "ODOC", "OWHARF");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			SetTariffLevel(org, "DEF", 42);

			var shipment = CreateShipment("S00000101", "K00000101");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_INCO = "FOB";

			CreateJob(shipment).LocalChargesPK = org.PK;

			Factory.Save();

			const string expectedLog =
				@"INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000101]: Autorating Notifications: Shipment S00000101 : There are matching:
	  • Company Tariff entries that are going to expire within the next 90 days
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Autorating Notifications: Shipment S00000101 : There are matching:
	  • Company Tariff entries that are going to expire within the next 90 days";

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
			}
		}

		#endregion

		#region TestRunAction_ClientRate

		public void TestRunAction_ClientRate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "THE WONDERFUL WIZZARD OF OZ";

			var header = Helper.NewClientRate(org);
			AddCalculator(header, "AU", "LCL", "ORG", "ODOC", "OWHARF");

			var shipment = CreateShipment("S00000101", "K00000101");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_INCO = "FOB";

			var job = CreateJob(shipment);
			job.LocalChargesPK = org.PK;

			Factory.Save();

			const string expectedLog =
				@"INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000101]: Autorating Notifications: Shipment S00000101 : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Autorating Notifications: Shipment S00000101 : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days";

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
			}

			AssertEquals("Job validation suspended to emulate op actions", true, job.IsValidationSuspended);
			AssertNotNull("Parent loaded during autorating - to retrieve correct invoice type defaults etc - even with validation suspended", job.Parent);
		}

		#endregion

		#region TestRunAction_AttachedCustomsDeclaration

		public void TestRunAction_AttachedCustomsDeclaration()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "THE WONDERFUL WIZZARD OF OZ";

			var header = Helper.NewClientRate(org);
			AddCalculator(header, "AU", "LCL", "DST", "CCLR");

			var shipment = CreateShipment("S00000101", "K00000101");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_RL_NKOrigin = "NLAMS";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_INCO = "FOB";

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_OH_Importer] = org.PK;
			declaration[JobDeclarationSchema.JE_TransportMode] = "SEA";
			declaration[JobDeclarationSchema.JE_ContainerMode] = "LCL";
			declaration[JobDeclarationSchema.JE_MessageType] = JobMessageTypeList.Codes.Import;
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = shipment.JS_RL_NKOrigin;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = shipment.JS_RL_NKDestination;

			CreateJob(shipment).LocalChargesPK = org.PK;

			Factory.Save();

			string expectedLog = @"INFO: [HL Shipment S00000101]: Shipment S00000101 was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Shipment S00000101 was auto-costed.
	No costs were found.
INFO: [HL Shipment S00000101]: Autorating Notifications: Shipment S00000101 : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days
WARNING: [HL Shipment S00000101]: Warnings Encountered:
Autorating Notifications: Shipment S00000101 : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days";

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
			}
		}

		#endregion

		public void TestRunAction_OverseasAgentSetOnConsolOnly()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";

			var ratingHeader = Helper.NewClientRate(consignor);
			var entry = AddCalculator(ratingHeader, "AU", "FCL", "ORG", "ODOC");
			AddCalculator(entry, "AU", "FCL", "ORG", "OWHARF");

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "AGENT";
			agent.OH_IsDebtor = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C000001";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NLAMS";
			consol.Transports[0].JW_IsLinked = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";
			shipment.JS_INCO = "DAP";

			var job = CreateJob(shipment);
			job.LocalChargesPK = consignor.PK;

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;

			Factory.Save();

			const string expectedLog =
				@"INFO: [HL Shipment S00001000 (House Bill='S00001000')]: Shipment S00001000 (House Bill='S00001000') was auto-costed.
	No costs were found.
WARNING: [HL Shipment S00001000 (House Bill='S00001000')]: Warnings Encountered:
Shipment S00001000 (House Bill='S00001000') was auto-costed.
	No costs were found.
INFO: [HL Shipment S00001000 (House Bill='S00001000')]: Autorating Notifications: Shipment S00001000 (House Bill='S00001000') : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days
WARNING: [HL Shipment S00001000 (House Bill='S00001000')]: Warnings Encountered:
Autorating Notifications: Shipment S00001000 (House Bill='S00001000') : There are matching:
	  • Client Rate entries that are going to expire within the next 90 days
";

			var newFactory = new BusinessObjectFactory();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var shipmentInNewFactory = consolInNewFactory.Shipments[0];

			AssertEquals(ZGuid.Empty, shipmentInNewFactory.Job.JH_OA_AgentCollectAddr);
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				ApplyApplicator(new BusinessObject[] { shipmentInNewFactory }, expectedLog);
			}

			var shipmentJob = shipmentInNewFactory.Job as Job;

			AssertNotNull(shipmentJob);
			AssertEquals("Should be Consignor", consignor.PK, shipmentJob.Charges[0].JR_OH_SellAccount);
			AssertEquals("Should be Consignor", consignor.PK, shipmentJob.Charges[1].JR_OH_SellAccount);
		}

		public void TestRunAction_ValidationOnJobIsNotSuspended()
		{
			var department = GlbDepartment.CurrentDepartment;
			department.GE_Code = "BRN";
			Factory.Save();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();

			using (Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var ratingHeader = Helper.NewClientRate(localClient);

				AddCalculator(ratingHeader, "AU", "FCL", "ORG", "ODOC");

				Factory.Save();

				var shipment = CreateShipment("S00000101", "C00000101");
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_INCO = "FOB";
				var docAddress = shipment.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty);
				docAddress.E2_OA_Address = localClient.MainAddress.PK;

				Factory.Save();

				const string expectedLog =
				@"ERROR: [HL Shipment S00000101]: Shipment S00000101 : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.
	Error - JH_GE: Cannot issue job charges for a miscellaneous department.
	Warning - JH_GS_NKRepSales: You have not entered a Sales Rep.";

				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
			}
		}

		public void TestRunAction_AutorateCostsAndRevenue()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsDebtor = true;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_IsShippingProvider = true;

			var origin = "AUSYD";
			var destination = "HKHKG";
			var chargeCode = "FRT";

			Helper.NewClientRateWithSingleRateLine(consignee, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, origin, destination, chargeCode, 200m);

			var costingRate = Helper.NewCosting(transportProvider);
			var costingEntry = costingRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, origin, destination);
			costingEntry.RateLines.RemoveAndDeleteAll();
			var costingLine = costingEntry.AddRateLine(chargeCode, FlatCalculator.Code, "", Constants.CurrencyCodes.Australia);
			costingLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var shipment = CreateShipment("S00000101", "K00000101", origin, destination, transportProvider);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			Factory.Save();

			using (var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (job.GetValidationSuspender())
			{
				var expectedLog = "";
				ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);

				var charges = job.Charges.Cast<Charge>().ToArray();
				AssertEquals(1, charges.Length);
				var charge = charges[0];
				AssertEquals(100m, (decimal)charge.JR_LocalCostAmt);
				AssertEquals(200m, (decimal)charge.JR_LocalSellAmt);
				AssertEquals(true, charge.JR_CostRated);
				AssertEquals(true, charge.JR_SellRated);
			}
		}

		#region Implementation

		ForwardingShipment CreateShipment(string shipmentNumber, string consolNumber, string loadPort = "AUBNE", string dischargePort = "NLAMS", OrgHeader transportProvider = null)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.Transports[0].JW_IsLinked = false;
			if (transportProvider != null)
			{
				consol.JK_OA_ShippingLineAddress = transportProvider.MainAddress.PK;
				consol.CreditorPK = transportProvider.PK;
			}

			ForwardingShipment result = consol.Shipments.AddNew();
			result.JS_UniqueConsignRef = shipmentNumber;
			return result;
		}

		void SetTariffLevel(OrgHeader header, string type, byte tariffLevel)
		{
			foreach (OrgRateTariffLevel tariff in header.CompanyData.RateTariffLevels)
			{
				if (tariff.P7_TariffType == type)
				{
					tariff.P7_TariffLevel = tariffLevel;
					return;
				}
			}

			OrgRateTariffLevel newTariff = header.CompanyData.RateTariffLevels.AddNew();
			newTariff.P7_TariffType = type;
			newTariff.P7_TariffLevel = tariffLevel;
		}

		RateEntry AddCalculator(RatingHeader header, string relevantLocation, string mode, string category, params string[] chargeCodes)
		{
			var entry = header.AddRateEntry(category);
			entry.TI_RateEndDate = ZDateTime.Now.Date.AddMonths(1);

			return AddCalculator(entry, relevantLocation, mode, category, chargeCodes);
		}

		RateEntry AddCalculator(RateEntry entry, string relevantLocation, string mode, string category, params string[] chargeCodes)
		{
			if (category == "ORG")
			{
				entry.TI_OriginLRC = relevantLocation;
			}
			else if (category == "DST")
			{
				entry.TI_DestinationLRC = relevantLocation;
			}

			entry.TI_Mode = mode;

			foreach (var chargeCode in chargeCodes)
			{
				var chargeCodeFilter = new ZQuery();
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

				var line = entry.RateLines.AddNew();
				line.TL_AC = Factory.LoadTop1<AccChargeCode>(chargeCodeFilter).PK;
				line.TL_RateCalculator = FlatCalculator.Code;

				var calculator = (FlatCalculator)line.Calculator;
				calculator.BaseRate = 60m;
			}

			return entry;
		}

		Job CreateJob(ForwardingShipment shipment)
		{
			Job result = Factory.NewJobForTesting<Job>();
			result.SuspendValidation();
			result.JH_JobNum = shipment.JS_UniqueConsignRef;
			result.JH_ParentID = shipment.PK;
			result.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			result.JH_GB = GlbBranch.CurrentBranch.PK;
			result.JH_GE = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES").PK;

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AutoRatingActionMethodApplicator(Factory);
		}

		Rating.Business.Testing.TestHelper Helper => helper ?? (helper = new Rating.Business.Testing.TestHelper(Factory));
		Rating.Business.Testing.TestHelper helper;

		#endregion
	}
}
