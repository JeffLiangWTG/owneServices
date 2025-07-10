using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	sealed class AccountingRatingServiceTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoRate_AutoRatingResults()
		{
			CreateTestShipmentAndConsol();

			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var service = new AccountingRatingService();
			var result = service.AutoRateAndCreateJobHeader(Shipment.PK.ToGuid(), Shipment.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			var logs = new List<string>(result.Logs);
			result.Logs = Array.Empty<string>();

			var file = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\Rating\RatingResult.json";
			var expected = File.ReadAllText(file);

			AssertEquals("Results Count", 1, result.Results.Length);
			AssertEquals("Target", Shipment.HumanReadableName, result.Results[0].Target);
			AssertMultilineASCIIEquals(logs.ToStringWithNewLineBetweenStrings(), expected, result.Results[0].Results.ToJSON());
			Assert("AutoRatingExplorer not be empty", !string.IsNullOrEmpty(result.Results[0].AutoRatingExplorer));

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object PK is passed in", 1, jobCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoRate_SearchForRates()
		{
			CreateTestShipmentAndConsol();

			var service = new AccountingRatingService();
			var result = service.SearchForRates(Shipment.PK.ToGuid(), Shipment.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			var logs = new List<string>(result.Logs);
			result.Logs = Array.Empty<string>();

			var file = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\Rating\SearchForRatesResult.json";
			var expected = File.ReadAllText(file);

			AssertEquals("Results Count", 1, result.Results.Length);
			AssertEquals("Target", Shipment.HumanReadableName, result.Results[0].Target);
			AssertMultilineASCIIEquals(logs.ToStringWithNewLineBetweenStrings(), expected, result.Results[0].Results.ToJSON());
		}

		public void TestAutoRate_QuoteNoCharges_Deleted_WhenNoSaveQuotesWithoutRates()
		{
			CreateQuotedBooking(0);
			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobChargeCount = Factory.GetDatabaseCount(typeof(JobCharge));
			var quoteCount = Factory.GetDatabaseCount(typeof(Quote));
			var oneOffShipmentCount = Factory.GetDatabaseCount(typeof(RateOneOffShipment));
			var oneOffContainerCount = Factory.GetDatabaseCount(typeof(RateOneOffContainers));
			var oneOffPackLinesCount = Factory.GetDatabaseCount(typeof(RateOneOffPackLine));

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(Booking.PK.ToGuid(), Booking.TablePrefix, GetWebUserPK(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			AssertEquals("Quote gets deleted with no charge and registry value is false", quoteCount - 1, Factory.GetDatabaseCount(typeof(Quote)));
			AssertEquals("JobCharge does not get saved when no charge and registry value is false", jobChargeCount, Factory.GetDatabaseCount(typeof(JobCharge)));
			AssertEquals("RateOneOffShipment cascade deleted", oneOffShipmentCount - 1, Factory.GetDatabaseCount(typeof(RateOneOffShipment)));
			AssertEquals("RateOneOffContainers cascade deleted", oneOffContainerCount - 1, Factory.GetDatabaseCount(typeof(RateOneOffContainers)));
			AssertEquals("RateOneOffPackLines cascade deleted", oneOffPackLinesCount - 1, Factory.GetDatabaseCount(typeof(RateOneOffPackLine)));
		}

		public void TestAutoRate_QuoteNoCharges_NotDeleted_WhenSaveQuotesWithoutRates()
		{
			void TestCase(bool registryValue, Guid userPK)
			{
				WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				var quoteCount = Factory.GetDatabaseCount(typeof(Quote));

				var service = new AccountingRatingService();
				service.AutoRateAndCreateJobHeader(Booking.PK.ToGuid(), Booking.TablePrefix, userPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

				AssertEquals("Quote not deleted", quoteCount, Factory.GetDatabaseCount(typeof(Quote)));
			}
			CreateQuotedBooking(0);
			TestCase(registryValue: true, userPK: GetWebUserPK());
			TestCase(registryValue: false, userPK: GetNonWebUserPK());
			TestCase(registryValue: true, userPK: GetNonWebUserPK());
		}

		public void TestAutoRate_QuoteCharges_NotDeleted()
		{
			CreateQuotedBooking(1);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobChargeCount = Factory.GetDatabaseCount(typeof(JobCharge));
			var quoteCount = Factory.GetDatabaseCount(typeof(Quote));

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(Booking.PK.ToGuid(), Booking.TablePrefix, GetWebUserPK(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			AssertEquals("JobCharge saved", jobChargeCount + 1, Factory.GetDatabaseCount(typeof(JobCharge)));
			AssertEquals("Quote not deleted", quoteCount, Factory.GetDatabaseCount(typeof(Quote)));
		}

		public void TestAutoRate_QuoteNegativeCharges_NotDeleted()
		{
			CreateQuotedBooking(-1);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobChargeCount = Factory.GetDatabaseCount(typeof(JobCharge));
			var quoteCount = Factory.GetDatabaseCount(typeof(Quote));

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(Booking.PK.ToGuid(), Booking.TablePrefix, GetWebUserPK(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			AssertEquals("JobCharge saved", jobChargeCount + 1, Factory.GetDatabaseCount(typeof(JobCharge)));
			AssertEquals("Quote not deleted", quoteCount, Factory.GetDatabaseCount(typeof(Quote)));
		}

		public void TestAutoRate_QuoteNoCost_NotDeleted()
		{
			CreateQuotedBooking(1, 0);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobChargeCount = Factory.GetDatabaseCount(typeof(JobCharge));
			var quoteCount = Factory.GetDatabaseCount(typeof(Quote));

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(Booking.PK.ToGuid(), Booking.TablePrefix, GetWebUserPK(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());

			AssertEquals("JobCharge saved", jobChargeCount + 1, Factory.GetDatabaseCount(typeof(JobCharge)));
			AssertEquals("Quote not deleted", quoteCount, Factory.GetDatabaseCount(typeof(Quote)));
		}

		public void TestAutoRate_NonQuoteNotDeleted()
		{
			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition for JobHeader", 0, jobCount);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var localChargesAddr = localClient.Addresses.AddNew(OrgAddressType.Receivables, true);
			localChargesAddr.FillWithValidTestData();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			AssertEquals("Precondition for Forwarding Shipment", 1, shipmentCount);

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GetWebUserPK(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), localClient.PK.ToGuid());

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			AssertEquals("JobHeader is created", 1, jobCount);
			AssertEquals("Forwarding Shipment is not deleted", 1, shipmentCount);
			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(localChargesAddr.PK, jobHeader.JH_OA_LocalChargesAddr);
		}
		
		public void TestAutoRate_SearchForRates_AutoRatingAuditLog()
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateTestShipmentAndConsol();
			AssertNull("Shipment job should not yet exist", Shipment.Job);

			var service = new AccountingRatingService();
			var result = service.SearchForRates(Shipment.PK.ToGuid(), JobShipmentSchema.Constants.Prefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());
			var actualNote = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).First();

			AssertNotNull("Autorating should produced results", result);
			AssertNotNull("Shipment job have been created and saved in another factory", Shipment.Job);
			AssertEquals("Expected the note to have been created and saved in another factory", false, actualNote.HasChanges);

			AssertContains("Autorating should produce log", "Information: AUTORATING COSTS FOR Shipment S000001", actualNote.ST_NoteDataAsText);
			AssertContains("Autorating should produce log", "Information: AUTORATING REVENUE FOR Shipment S000001", actualNote.ST_NoteDataAsText);

			var shipmentWithoutCharges = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentWithoutCharges.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipmentWithoutCharges.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipmentWithoutCharges.JS_TransportMode = Constants.TransportModes.Air;
			shipmentWithoutCharges.JS_PackingMode = Constants.ContainerModes.Loose;
			shipmentWithoutCharges.JS_RL_NKOrigin = "MXCAN";
			shipmentWithoutCharges.JS_RL_NKDestination = "MGNOS";
			shipmentWithoutCharges.JS_ActualWeight = 1500;
			shipmentWithoutCharges.JS_UniqueConsignRef = "S000002";
			Factory.Save();

			service.SearchForRates(shipmentWithoutCharges.PK.ToGuid(), JobShipmentSchema.Constants.Prefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Consignor.PK.ToGuid());
			actualNote = shipmentWithoutCharges.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).First();

			AssertNotNull("Autorating should produced results even if no charges were found", result);
			AssertNotNull("Shipment job have been created and saved in another factory", shipmentWithoutCharges.Job);
			AssertEquals("Expected the note to have been created and saved in another factory", false, actualNote.HasChanges);

			AssertContains("Autorating should produce log", "Information: AUTORATING COSTS FOR Shipment S000002", actualNote.ST_NoteDataAsText);
			AssertContains("Autorating should produce log", "Information: AUTORATING REVENUE FOR Shipment S000002", actualNote.ST_NoteDataAsText);
		}

		#region Test Run Sheets with Auto Rating

		public void TestAutoRate_AutorateRunsheetButNotConsignment()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_IsCreditor = true;
				transportCo.CompanyData.SetAPTaxApplicable(false);

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_RL_NKClosestPort = "AUBNE";

				var groupedChargeCode = CreateChargeCode("CARTGR", "Grouped Cartage", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
				groupedChargeCode.AC_AT_GSTRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", AccTaxRate.Types.Rated, 10).PK;
				var ungroupedChargeCode = CreateChargeCode("CARTNG", "Not-grouped Cartage", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: false);
				ungroupedChargeCode.AC_AT_GSTRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", AccTaxRate.Types.Rated, 10).PK;

				var costing = Helper.NewCosting(transportCo);
				var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ALL, "AU", "");
				entry.AddRateLine(groupedChargeCode, UnitCalculator.Code, Constants.PkgUnit.Box).GetCalculator<UnitCalculator>().PerUnit = 5;
				entry.AddRateLine(ungroupedChargeCode, UnitCalculator.Code, Constants.PkgUnit.Box).GetCalculator<UnitCalculator>().PerUnit = 10;

				var consignment = CreateConsignment(consignor, consignee, 3, Constants.PkgUnit.Box, 10m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);

				Factory.Save();

				var response = new AccountingRatingService().AutoRateAndCreateJobHeader(runSheet.PK.ToGuid(), runSheet.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, client.PK.ToGuid());
				var resultingResponseLogs = string.Join("\r\n", response.Logs);

				AssertEquals(@"Information: An Invoicing Record for Land Transport Consignment LTC001 has not been created. 
	Auto rating can only run on Jobs with Invoicing Records.
	Do you want to create all the required Invoicing Records now?
	Selected Yes
Information: AUTORATING COSTS FOR Run Sheet 70HEL1IWJLMTFD3
Information: RatingHeader Found Costing H5ZX52PAMCOI Entries: 1
Information: Rates Service: No Search Request is sent to Rates Service as Rates Service is NOT supported for 
Information: RateLine Found CARTGR-UNT-BOX-Costing H5ZX52PAMCOI
Information: RateLine Found CARTNG-UNT-BOX-Costing H5ZX52PAMCOI
Information: RateLine Filtered CARTNG-UNT-BOX-Costing H5ZX52PAMCOI	reason:	CARTNG charge code is not flagged as Consol Level. Non-Consol Level Costs don't apply to 70HEL1IWJLMTFD3.
Information: Chargeable was added for
				RateLine CARTGR-UNT-BOX-Costing H5ZX52PAMCOI
					Job's info:
					: 3 Unit
Information: CHARGES CALCULATED:
	CARTGR: 3 Box(s) @ AUD 5.00/Box
Information: Run Sheet 70HEL1IWJLMTFD3 was auto-costed.
	The following costs were found:
	  • CARTGR charge from Costing H5ZX52PAMCOI
	Charges created: CARTGR
Information: Results Saved",
					resultingResponseLogs
				);

				var resultingChargeCodes = response.Results.SelectMany(x => x.Results.SelectMany(y => y.CreatedCharges.Select(z => z.ChargeCode)));
				AssertContainsExactElementsInAnyOrder("Expecting the charge to be found", new[] { "CARTGR" }, resultingChargeCodes);
			}
		}

		public void TestAutoRate_ViaRunSheetsAndVerifyCosts()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, GetCartageDepartmentPK()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_IsCreditor = true;
				transportCo.CompanyData.SetAPTaxApplicable(false);

				var cnr = Factory.NewWithValidTestData<OrgHeader>();
				var cne = Factory.NewWithValidTestData<OrgHeader>();

				cnr.OH_RL_NKClosestPort = "AUBNE";

				var chargeCode = CreateChargeCode("CARTBC", "Bobs Cartage Cost", ChargeCodeGroupList.Codes.TransportBooking, isGroupage: true);
				chargeCode.AC_AT_GSTRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", AccTaxRate.Types.Rated, 10).PK;
				var costing = CreateCostingWithUnitCalculator(transportCo, chargeCode, "AU", Constants.PkgUnit.Box, 5m);

				var consignment = CreateConsignment(cnr, cne, 3, Constants.PkgUnit.Box, 10m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
				var runSheet = CreateRunSheet(transportCo, consignment.PickupAddress.PickupAction);

				var job = new Job.Loader(consignment).TryLoadOrCreateWithoutMutexForTestOnly();
				job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

				Factory.Save();

				AssertNoExceptionThrown(() => new AccountingRatingService().AutoRateAndCreateJobHeader(runSheet.PK.ToGuid(), runSheet.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, client.PK.ToGuid()));
				var autoRatedCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, ((IJobCostingPlugIn)runSheet).CostSupporter.PK));

				AssertEquals(autoRatedCosts.Length, 1);
				AssertEquals(autoRatedCosts[0].ChargeCode.AC_Code, "CARTBC");
				AssertEquals(autoRatedCosts[0].E6_OSCostAmount, 15m);
			}
		}

		public void TestAutoRate_ShouldSetLocalCharges()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var localChargesAddr = localClient.Addresses.AddNew(OrgAddressType.Receivables, true);
			localChargesAddr.FillWithValidTestData();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), localClient.PK.ToGuid());

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Job is created", 1, jobCount);

			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(localChargesAddr.PK, jobHeader.JH_OA_LocalChargesAddr);
		}

		public void TestAutoRate_ShouldIgnoreLocalChargesForExistingHeader()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var originalLocalClient = Factory.NewWithValidTestData<OrgHeader>();
			var originalLocalChargesAddr = originalLocalClient.Addresses.AddNew(OrgAddressType.Receivables, true);
			originalLocalChargesAddr.FillWithValidTestData();
			var otherLocalClient = Factory.NewWithValidTestData<OrgHeader>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), originalLocalClient.PK.ToGuid());

			service.AutoRateAndCreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), otherLocalClient.PK.ToGuid());

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Only one job is created", 1, jobCount);

			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(originalLocalChargesAddr.PK, jobHeader.JH_OA_LocalChargesAddr);
		}

		DtbConsignment CreateConsignment(OrgHeader consignor, OrgHeader consignee, int qty, string qtyUQ, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var consignment = ConsignmentTestHelper.CreateConsignment();
			consignment.Addresses.AddNew(InstructionTypes.Codes.PickUp).LTS_Sequence = 1;
			consignment.Addresses.AddNew(InstructionTypes.Codes.Delivery).LTS_Sequence = 2;
			var pickupAddress = consignment.PickupAddress;
			var deliveryAddress = consignment.DeliveryAddress;
			pickupAddress.Address.OrganisationPK = consignor.PK;
			deliveryAddress.Address.OrganisationPK = consignee.PK;

			ConsignmentTestHelper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			ConsignmentTestHelper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			AssignPackage(consignment.PackageJob, qty, qtyUQ, weight, weightUQ, volume, volumeUQ);

			return consignment;
		}

		TransportConsignmentTestHelper ConsignmentTestHelper
		{
			get { return consignmentTestHelper ?? (consignmentTestHelper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper consignmentTestHelper;

		DtbConsignmentRunSheet CreateRunSheet(OrgHeader transportCo, params DtbConsignmentAction[] actions)
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			runSheet.KG_OH_TransportCo = transportCo.PK;

			ConsignmentTestHelper.CreateRunSheetInstruction(runSheet, actions);

			return runSheet;
		}

		Guid GetCartageDepartmentPK()
		{
			return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "TOT")).PK.ToGuid();
		}

		Guid GetNonWebUserPK()
		{
			return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E")).PK.ToGuid();
		}
		Guid GetWebUserPK()
		{
			return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "ZZ")).PK.ToGuid();
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		Costing CreateCostingWithUnitCalculator(OrgHeader transportCo, AccChargeCode chargeCode, string location, string unit, decimal perUnit)
		{
			var costing = Helper.NewCosting(transportCo);
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ALL, location, "");
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			line.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return costing;
		}

		AccChargeCode CreateChargeCode(string code, string desc, string chargeGroup, bool isGroupage = false)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_Code = code;
			result.AC_Desc = desc;
			result.AC_ChargeGroup = chargeGroup;
			result.AC_IsGroupageCharge = isGroupage;
			result.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
			result.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
			result.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
			result.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;
			result.AC_RateCalculator = UnitCalculator.Code;
			result.AC_ChargeType = Constants.ChargeType.Margin;

			return result;
		}

		PkgPackage AssignPackage(PkgPackageJob packageJob, int quantity, string quantityUQ, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var package = packageJob.Packages.AddNew();
			AssignPackage(package, quantity, quantityUQ, weight, weightUQ, volume, volumeUQ);

			return package;
		}

		void AssignPackage(PkgPackage package, int quantity, string quantityUQ, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			package.KP_PackageQty = quantity;
			package.KP_F3_NKPackType = quantityUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
		}

		#endregion

		public void TestHasRates_NoJobCreation()
		{
			var dummyObject = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);
			AssertEquals("Precondition", false, dummyObject is IJobHeaderParent);

			var service = new AccountingRatingService();
			service.AutoRateAndCreateJobHeader(dummyObject.PK.ToGuid(), dummyObject.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Guid.Empty);

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object PK is passed in", 0, jobCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoRate_AutoRatingResultsWithDeletedCharges()
		{
			var creator = new TestObjectCreator(Factory);

			CreateTestShipmentAndConsol();

			//Create Job and existing Charge for Shipment
			var job = new Job.Loader(Shipment).TryLoadOrCreateWithoutMutexForTestOnly(GlbBranch.CurrentBranch);
			job.LocalChargesPK = Consignor.PK;

			var existingFRTCharge = creator.CreateChargeWithPaymentBasis(job, Helper.ChargeCodes["FRT"], creator.ABIGAS, 20m);
			existingFRTCharge.JR_OSCostAmt = 10m;
			existingFRTCharge.JR_CostRatingOverride = false;
			existingFRTCharge.JR_SellRatingOverride = false;

			var existingBAFCharge = creator.CreateChargeWithPaymentBasis(job, Helper.ChargeCodes["BAF"], creator.ABIGAS, 10m);
			existingBAFCharge.JR_OSCostAmt = 5m;
			existingBAFCharge.JR_CostRatingOverride = false;
			existingBAFCharge.JR_SellRatingOverride = false;

			Factory.Save();

			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 1, jobCount);

			var service = new AccountingRatingService();
			var result = service.AutoRateAndCreateJobHeader(Shipment.PK.ToGuid(), Shipment.TablePrefix, Env.CurrentUserPK, Env.CurrentBranchPK, creator.FIADepartment.PK.ToGuid(), Consignor.PK.ToGuid());

			var logs = new List<string>(result.Logs);
			result.Logs = Array.Empty<string>();

			var file = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Business.Testing\Rating\RatingResultWithDeletedCharges.json";
			var expected = File.ReadAllText(file);

			AssertEquals("Results Count", 1, result.Results.Length);
			AssertEquals("Target", Shipment.HumanReadableName, result.Results[0].Target);
			AssertMultilineASCIIEquals(logs.ToStringWithNewLineBetweenStrings(), expected, result.Results[0].Results.ToJSON());
		}

		public void TestGetAutoRatingExplorer_TargetRatingInfoAsJSON()
		{
			CreateTestShipmentAndConsol();

			var service = new AccountingRatingService();
			var result = service.GetAutoRatingExplorer(Shipment.PK.ToGuid(), JobShipmentSchema.Constants.Prefix, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			AssertNotNullOrEmpty(result);
		}

		#region Implementation

		void CreateTestShipmentAndConsol()
		{
			Creditor = Factory.NewWithValidTestData<OrgHeader>();
			Creditor.OH_IsCreditor = true;

			Consignor = Factory.NewWithValidTestData<OrgHeader>();
			Consignee = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_DepartmentFilterList = "ALL";

			var cost = Helper.NewCosting(Creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 5;

			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.TI_OH_Supplier = cost.Header.PK;
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 6;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(Creditor);
			consol.JK_OA_CreditorAddress = Creditor.MainAddress.PK;
			consol.JK_UniqueConsignRef = "C000001";
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			Shipment = consol.Shipments.AddNew();
			Shipment.ConsignorPK = Consignor.PK;
			Shipment.ConsigneePK = Consignee.PK;
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";
			Shipment.JS_ActualWeight = 1500;
			Shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			Shipment.JS_UniqueConsignRef = "S000001";

			Factory.Save();
		}

		void CreateQuotedBooking(ZDecimal charge, decimal margin = 100)
		{
			Consignor = Factory.NewWithValidTestData<OrgHeader>();
			Booking = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			Booking.ClientPK = Consignor.PK;
			Booking.Mode = "LSE";
			Booking.Origin = "HKHKG";
			Booking.Destination = "AUSYD";
			Booking.VolumeUnit = Constants.Volume.Litre;
			Booking.Volume = 1m;
			var rateOneOffPackLine = Factory.New<RateOneOffPackLine>();
			var rateOneOffShipment = Booking.Quote.CurrentOneOffQuote;
			rateOneOffPackLine.TPL_TT_RateOneOffShipment = rateOneOffShipment.PK;
			rateOneOffShipment.Containers.AddNew();

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Consignor.PK;

			var entry = clientRate.AddRateEntry("AIR", "LSE", "HKHKG", "AUSYD");
			var line = entry.RateLines[0];
			line.ChargeCode.AC_MarginPercentage = margin;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator[Calculator.Items.Operator.UNT] = charge;
			line.TL_WeightVolume = Constants.Volume.Litre;
			Factory.Save();
		}

		OrgHeader Creditor;
		OrgHeader Consignor;
		OrgHeader Consignee;
		ForwardingShipment Shipment;
		QuotedBooking Booking;
		#endregion
	}
}
