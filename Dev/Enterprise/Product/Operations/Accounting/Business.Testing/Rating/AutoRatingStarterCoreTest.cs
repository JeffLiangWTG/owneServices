using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WiseRates.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Accounting.Business
{
	public class AutoRatingStarterCoreTest : TestCaseWithFactory
	{
		public void TestUnsupportedObjectAutorated()
		{
			var businessMock = new Mock<IBusiness>();
			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			new AutoRatingStarterCore(interactor, businessMock.Object, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null)
				.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals("You cannot perform Autorating. Please enter your invoice charges manually for this kind of job.", interactor.Errors[0]);
		}

		public void TestRateAdditionalCharges()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var dummy = new DummyAutoRatingObject(Factory);
			var ratingContext = new RatingContext();
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var strategy = new AutoRateInvoicingStrategy(dummy, null);
			var starter = new AutoRatingStarterCore(new TestInteractor(), dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			var jobs = starter.GetCustomsJobsToRate(strategy);
			AssertEquals(1, jobs.Length);
			AssertEquals("should be the correct job", true, ReferenceEquals(chargesMock.Object, jobs[0]));
			strategy.Job.Dispose();
		}

		public void TestClearLogNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var starter = new AutoRatingStarterCore(new TestInteractor(), shipment, new RatingContext(), BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
				var note1 = shipment.Notes.AddNew();
				note1.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
				note1.ST_GC_RelatedCompany = Env.CurrentCompanyPK;
				note1.ST_NoteText = "Note for current company";
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var note2 = shipment.Notes.AddNew();
				note2.ST_Description = PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description;
				note2.ST_GC_RelatedCompany = company.PK;
				note2.ST_NoteText = "Note for some other company";
				starter.ClearLogNotes();
				AssertEquals("Current company's notes should be cleared", string.Empty, note1.ST_NoteText);
				AssertEquals("Notes from other companies should be untouched", "Note for some other company", note2.ST_NoteText);
			}
		}

		#region Raw Data

		public void TestRawData_WhenDiagnosticSettingsIncludeRawDataIsTrue_ThenShouldStoreRawData()
		{
			AssertStoreRawData(diagnosticSettingsIncludeRawData: true, expectStoreWiseRatesRawData: true);
		}

		public void TestRawData_WhenDiagnosticSettingsIncludeRawDataIsTrue_ThenShouldNotStoreRawData()
		{
			AssertStoreRawData(diagnosticSettingsIncludeRawData: false, expectStoreWiseRatesRawData: false);
		}

		void AssertStoreRawData(bool diagnosticSettingsIncludeRawData, bool expectStoreWiseRatesRawData)
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216", "NZAKL", "AUSYD");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");

			Factory.Save();

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, diagnosticSettingsIncludeRawData))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var debtor = debtorCompany.OrgProxy;
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;

				Factory.Save();

				var ratingContext = new MockRatingContext(Factory);
				ratingContext.RawResponse = "test";

				var autoRatingStarter = new AutoRatingStarter(new[] { shipment }, ratingContext);
				autoRatingStarter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var ratingSupporterWithAdapter = (IRatingSupporterWithAdapter)shipment;
				var ratingAdapter = (RatingAdapter)ratingSupporterWithAdapter.RatingAdapter;

				var cache = Factory.GetCachedValue("AutoRatingStarerCore.RateCharges.RawResponse", () => new Dictionary<ZGuid, string>());
				cache.TryGetValue(job.Parent.PK, out string rawResponses);
				if (expectStoreWiseRatesRawData)
				{
					AssertEquals("test", rawResponses);
				}
				else
				{
					AssertNullOrEmpty(rawResponses);
				}
			}
		}

		class MockRatingContext : IRatingContext
		{
			public MockRatingContext(BusinessObjectFactory factory)
			{
				Factory = factory;
				Logger = new LoggerDecorator(new TestLogger());
				ProviderCW1Rates = new CW1RatesProvider(Factory, Logger);
				RatesProvider = ProviderCW1Rates;
			}

			public ILogger Logger { get; }
			public IDialogService DialogService { get; }
			public BusinessObjectFactory Factory { get; }
			public bool SearchForRatesMode { get; set; }
			public bool IsInRebateCalculationMode { get; set; }
			public Dictionary<ZGuid, HashSet<ZGuid>> PercentageLinesApplied { get; } = new Dictionary<ZGuid, HashSet<ZGuid>>();
			public string RawResponse { get; set; }
			public bool IsManualCostSelectMode => false;
			public IRatesProvider RatesProvider { get; }
			public IUrsRatesProvider ProviderUrsRates => null;
			public ICW1RatesProvider ProviderCW1Rates { get; }
			public List<AutoRateInfo> InterimAutoRatingResults { get; } = new List<AutoRateInfo>();
			public CalculationLogsWrapper RateCalculationLogWrapper { get; } = new CalculationLogsWrapper();
		}

		#endregion

		public void TestNoErrorForJobsThatAreBothInvoicingAndCostingPlugin()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var ratingContext = new RatingContext();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var dummy2 = new DummyAutoRatingConsolObject(Factory);
			dummy2.AdditionalCharges.Add(chargesMock.Object);
			dummy2.IsImport = true;
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job1.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job2.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var starter = new AutoRatingStarterCore(new TestInteractor(), dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			Assert(starter.ValidatePreAutorating());
			starter = new AutoRatingStarterCore(new TestInteractor(), dummy2, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			Assert(starter.ValidatePreAutorating());
		}

		public void TestExecuteAutorating_AdditionalJobs()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestObjectCreator.CreateFlatCalculatorClientRate("AIR", "LSE", "AUSYD", "NZAKL", TestObjectCreator.LocalClient, "FRT", 500m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol, transportMode: Core.Constants.TransportModes.Air, incoTerm: "CIF");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.LocalClient.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder
			(
				"Precondition: shipment job should not exist yet",
				Array.Empty<JobHeader>(),
				JobsAttachedToParentPK(shipment.PK)
			);

			AssertExecuteAutorating_AdditionalJobs
			(
				consol,
				shipment.PK,
				expectedShipmentJobExist: true,
				expectedShipmentCharges: new[] { "FRT => 500.00" },
				expectedWarnings: new[]
				{
					"An Invoicing Record for Shipment S00001 has not been created. \r\nAuto rating can only run on Jobs with Invoicing Records.\r\nDo you want to create all the required Invoicing Records now?",
				},
				"Autorate"
			);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
			AssertExecuteAutorating_AdditionalJobs
			(
				loadedConsol,
				shipment.PK,
				expectedShipmentJobExist: false,
				expectedShipmentCharges: Array.Empty<string>(),
				expectedWarnings: new[]
				{
					"An Invoicing Record for Shipment S00001 has not been created. \r\nAuto rating can only run on Jobs with Invoicing Records.\r\nDo you want to create all the required Invoicing Records now?",
					"Job header with billing charges for Shipment S00001 could NOT be created because it is locked by another process. Please try again later."
				},
				"Re-autorate on new factory"
			);

			shipment.Job.Dispose();
		}

		static void AssertExecuteAutorating_AdditionalJobs(ForwardingConsol consol, ZGuid shipmentPK, bool expectedShipmentJobExist, string[] expectedShipmentCharges, string[] expectedWarnings, string message)
		{
			var interactor = new TestInteractor();
			var starter = new AutoRatingStarterCore(interactor, consol, new RatingContext(), BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);

			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			{
				starter.ExecuteAutorating(AutoRateOptions.AutorateRevenue);
			}

			var shipmentJobs = JobsAttachedToParentPK(consol.Factory, shipmentPK);
			CombineAssertions(message, () =>
			{
				if (expectedShipmentJobExist)
				{
					AssertNotNull("Shipment job", shipmentJobs.FirstOrDefault());
				}
				else
				{
					AssertNull("Shipment job", shipmentJobs.FirstOrDefault());
				}

				AssertContainsExactElementsInAnyOrder
				(
					"Charges",
					expectedShipmentCharges,
					shipmentJobs.Cast<Job>()
						.SelectMany(job => job.Charges.Select(charge => $"{charge.ChargeCode.AC_Code} => {charge.JR_OSSellAmt}"))
				);

				AssertContainsExactElementsInAnyOrder
				(
					"Warnings",
					expectedWarnings,
					interactor.YesNoWarnings
				);
			});
		}

		public void TestExecuteAutoRating_WithIAutoRatingCreateJobHeaderPromptOverride()
		{
			var dummy = new DummyAutoRatingObjectWithJobHeaderCreateOverride(Factory);
			dummy.IsInDatabaseOverride = true;
			dummy.IsImport = true;
			dummy.SuppressJobCreationPrompt = true;

			var interactor = new TestInteractor();
			interactor.Answers.Add(false); // for prompt
			var ratingContext = new RatingContext();
			try
			{
				AssertNull("Precondition", new Job.Loader(dummy).Load(false));

				new AutoRatingStarterCore(interactor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
				AssertNotNull(new Job.Loader(dummy).Load(false));
				AssertEquals(0, interactor.YesNoWarnings.Count);
			}
			finally
			{
				new Job.Loader(dummy).Load(false)?.Delete();
			}
		}

		public void TestExecuteAutoRating_WithIAutoRatingCreateJobHeaderPromptOverride_ShouldOverrideIsFalse()
		{
			var dummy = new DummyAutoRatingObjectWithJobHeaderCreateOverride(Factory);
			dummy.IsInDatabaseOverride = true;
			dummy.IsImport = true;
			dummy.SuppressJobCreationPrompt = false;

			var interactor = new TestInteractor();
			interactor.Answers.Add(false); // for prompt
			var ratingContext = new RatingContext();
			AssertNull("Precondition", new Job.Loader(dummy).Load(false));

			new AutoRatingStarterCore(interactor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			AssertNull(new Job.Loader(dummy).Load(false));
			AssertEquals(1, interactor.YesNoWarnings.Count);
		}

		public void TestValidatePreAutorating_RunPreSaveValidationWithIValidationSuspender_ShouldRunValidation()
		{
			AssertValidatePreAutorating_RunPreSaveValidationWithIValidationSuspender(shouldRunPreSaveValidationBeforeAutorating: true);
		}

		public void TestValidatePreAutorating_RunPreSaveValidationWithIValidationSuspender_ShouldNotRunValidation()
		{
			AssertValidatePreAutorating_RunPreSaveValidationWithIValidationSuspender(shouldRunPreSaveValidationBeforeAutorating: false);
		}

		void AssertValidatePreAutorating_RunPreSaveValidationWithIValidationSuspender(bool shouldRunPreSaveValidationBeforeAutorating)
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var dummy = new DummyAutoRatingObjectWithValidationSuspender(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			job.JH_Status = JobHeaderStatus.Working.Code;

			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(interactor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);

			var jobValidationHitCount = 0;
			job.JH_OA_LocalChargesAddrInfo.AdditionalValidation += () => jobValidationHitCount++;
			dummy.ShouldRunPreSaveValidationBeforeAutoratingForTesting = shouldRunPreSaveValidationBeforeAutorating;
			starter.ValidatePreAutorating();
			AssertEquals("When ShouldRunPreSaveValidationBeforeAutorating is false, Job Validation should not be run.", shouldRunPreSaveValidationBeforeAutorating ? 1 : 0, jobValidationHitCount);
		}

		public void TestExecuteAutoRating_WithIValidationSuspender()
		{
			var creator = new TestObjectCreator(Factory);
			var validationAssertionRun = false;
			var chargesMock = new Mock<ICustomsCharges>();
			chargesMock.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>()))
				.Returns(new[] { new CustomsCharge(creator.CC1.PK, "TEST", 10m, 2m, true, creator.Creditor1.PK) })
				.Callback(() =>
				{
					validationAssertionRun = true;
					AssertEquals("Factory Validation should have been suspended during Autorating.", true, Factory.IsValidationSuspended);
				});

			var dummy = new DummyAutoRatingObjectWithValidationSuspender(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsInDatabaseOverride = true;
			dummy.IsImport = true;
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			job.JH_Status = JobHeaderStatus.Working.Code;

			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(interactor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);

			var jobValidationHitCount = 0;
			var onValidationResumedWasRun = false;
			job.JH_OA_LocalChargesAddrInfo.AdditionalValidation += () => jobValidationHitCount++;
			dummy.ShouldRunPreSaveValidationBeforeAutoratingForTesting = false;
			dummy.OnValidationResumedForTesting += () =>
			{
				onValidationResumedWasRun = true;
				AssertEquals("Factory Validation should no longer be suspended as Autorating is finished.", false, Factory.IsValidationSuspended);
			};

			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(dummy))
			{
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			}

			AssertEquals("Factory Validation should have been suspended during Autorating.", true, validationAssertionRun);
			AssertEquals("On Validation Resumed should have been invoked correctly.", true, onValidationResumedWasRun);
			AssertEquals("Job Validation should not be run during AutoRating if it was Suspended.", 0, jobValidationHitCount);
		}

		public void TestJobValidationErrorBasedOnJobStatus()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			job.JH_Status = JobHeaderStatus.Working.Code;
			AssertJobStatusError(dummy, false);
			job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
			AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run while the Job has Ready for Revenue or Cost Posting status.");
			job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run while the Job has Ready for Revenue or Cost Posting status.");
			job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run while the Job has Ready for Revenue or Cost Posting status.");
			job.JH_Status = JobHeaderStatus.Closed.Code;
			AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run as its Invoicing Job is closed.");
			job.JH_Status = JobHeaderStatus.ScheduledForArchive.Code;
			AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run as it is scheduled for archiving.");

			var oldvalue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = oldvalue))
			{
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				AssertJobStatusError(dummy, true, "Dummy Object : Autorating cannot be run as its Invoicing Job has Ready For Financial Closure status.");
			}
		}

		public void TestJobValidationErrorForConsol()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ActiveOrg.PK;
			TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			Factory.Save();

			job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			AssertContainJobStatusError(consol, false);

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();
			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
			AssertContainJobStatusError(consol, true, "Autorating cannot be run as its apportionment Job has Ready For Financial Closure status.");

			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true;
			AssertContainJobStatusError(consol, false);
		}

		void AssertJobStatusError(IBusiness ratingSupportor, bool fail, string expectedError = "")
		{
			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(interactor, ratingSupportor, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			var errorCount = fail ? 1 : 0;
			AssertEquals("Validation for Autorating both", !fail, starter.ValidatePreAutorating());
			AssertEquals("Errors reported", errorCount, interactor.Errors.Count);
			if (!string.IsNullOrEmpty(expectedError))
			{
				AssertEquals("Expected Error", fail, interactor.Errors.All(x => x == expectedError));
			}
		}

		void AssertContainJobStatusError(IBusiness ratingSupportor, bool fail, string expectedError = "")
		{
			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(interactor, ratingSupportor, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			AssertEquals("Validation for Autorating both", !fail, starter.ValidatePreAutorating());
			if (!string.IsNullOrEmpty(expectedError))
			{
				AssertEquals("Expected Error", fail, interactor.Errors.Any(x => x.Contains(expectedError)));
			}
		}

		public void TestExecuteAutoratingRunsOnTheCollection()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var ratingContext = new RatingContext();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var dummy2 = new DummyAutoRatingObject(Factory);
			dummy2.AdditionalCharges.Add(chargesMock.Object);
			dummy2.IsImport = true;
			Job job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Job job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			dummy.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy, dummy2 });
			AssertNoErrors("should have no errors as the job hasn't been validated yet.", job1);
			AssertNoErrors("should have no errors as the job hasn't been validated yet.", job2);
			var interactor = new TestInteractor();
			new AutoRatingStarterCore(interactor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals("ExecuteAutorating() should validate job on all plugins", true, job1.HasErrors);
			AssertEquals("ExecuteAutorating() should validate job on all plugins", true, job2.HasErrors);
			AssertContains("Dummy Object : Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", interactor.Errors[0]);
		}

		public void TestExecuteAutorating_CAREvent_WhenLogDoesNotExistInDB_DbHits()
		{
			var creator = new TestObjectCreator(Factory);
			var localChargesAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
			var chargesMock = new Mock<ICustomsCharges>();
			chargesMock.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { new CustomsCharge(creator.CC1.PK, "TEST", 10m, 2m, true, creator.Creditor1.PK) });

			var ratingContext = new RatingContext();
			var dummy1 = new DummyAutoRatingObject(Factory);
			dummy1.AdditionalCharges.Add(chargesMock.Object);
			dummy1.IsInDatabaseOverride = true;
			dummy1.IsImport = true;
			var dummy2 = new DummyAutoRatingObject(Factory);
			dummy2.AdditionalCharges.Add(chargesMock.Object);
			dummy2.IsInDatabaseOverride = true;
			dummy2.IsImport = true;
			var dummy3 = new DummyAutoRatingObject(Factory);
			dummy3.AdditionalCharges.Add(chargesMock.Object);
			dummy3.IsInDatabaseOverride = true;
			dummy3.IsImport = true;

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GE = department.PK;
			job1.JH_OA_LocalChargesAddr = localChargesAddress.PK;
			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_OA_LocalChargesAddr = localChargesAddress.PK;
			job2.JH_GE = department.PK;
			var job3 = Factory.NewJobForTesting<Job>();
			job3.JH_ParentID = dummy3.PK;
			job3.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_OA_LocalChargesAddr = localChargesAddress.PK;
			job3.JH_GE = department.PK;

			dummy1.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy1, dummy2, dummy3 });

			using (TestConnection.TrackExecutedCommands())
			{
				var interactor = new TestInteractor();
				using (_Rating.Start(interactor, isEqualization: false))
				using (interactor.StartRatingSession())
				using (_Rating.StartSubSession(dummy1))
				{
					new AutoRatingStarterCore(interactor, dummy1, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				}

				AssertEquals(2, dummy1.Logs.LogsNotInDB.Count(l => l.SL_SE_NKEvent == Events.ChargesHaveBeenAutoRatedCode));
				AssertEquals(2, dummy2.Logs.LogsNotInDB.Count(l => l.SL_SE_NKEvent == Events.ChargesHaveBeenAutoRatedCode));
				AssertEquals(2, dummy3.Logs.LogsNotInDB.Count(l => l.SL_SE_NKEvent == Events.ChargesHaveBeenAutoRatedCode));

				var logsDBHitCount = Factory.GetTableHitCount(StmALogSchema.Constants.TableName);
				AssertEquals("Should not have hit the Database once for StmALog through the BizOFactory, since none of the Jobs had a Log in the DB.", 0, logsDBHitCount);
				AssertEquals("Should only hit the DB if the Job has queued a Log to Create. ExecuteAutoRating is called 3 times, 2 of those times Logs are Queued.", 2,
					TestConnection.ExecutedCommands.Count(c => c.Contains("@LogParents")));
			}
		}

		public void TestExecuteAutorating_ShouldGenerateCAREventWithTriggerSourceParameter()
		{
			var customsChargesMock = new Mock<ICustomsCharges>();
			customsChargesMock
				.Setup(c => c.GetCustomsCharges(It.IsAny<ILogger>()))
				.Returns(new[]
				{
					new CustomsCharge(TestObjectCreator.CC1.PK, "TEST", 10m, 2m, true, TestObjectCreator.Creditor1.PK)
				});

			void AssertAutorating(AutoRateOptions options, string expectedTrigger)
			{
				var interactor = new TestInteractor();
				using (_Rating.Start(interactor, isEqualization: false))
				using (interactor.StartRatingSession())
				{
					var job = CreateDummyAutoRatingObjectWithJob(customsChargesMock.Object, NonMiscDeparment, TestObjectCreator.Debtor.MainAddress);
					var starter = new AutoRatingStarterCore(
						interactor,
						job,
						new RatingContext(),
						BillingType.Default,
						AdditionalJobsAction.AutoRateAdditionalInvoicingJobs,
						null,
						additionalJobsToDispose: null);

					starter.ExecuteAutorating(options);

					var log = job.Logs.MostRecentLogByPostedTime(Events.ChargesHaveBeenAutoRated);

					AssertEquals(expectedTrigger, log.Parameters[EventReferenceParameters.TriggerCode]);
				}
			}

			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Api), "API");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Menu), "Menu");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Workflow), "Workflow");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Unspecified), "Unspecified");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.PrintInvoicing), "Print Invoicing");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.GatewayBilling), "Gateway Billing");
			AssertAutorating(new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.OperationalActions), "Operational Actions");
		}

		public void TestExecuteAutorating_ShouldGenerateCAREventWithModeParameter()
		{
			var customsChargesMock = new Mock<ICustomsCharges>();
			customsChargesMock
				.Setup(c => c.GetCustomsCharges(It.IsAny<ILogger>()))
				.Returns(new[]
				{
					new CustomsCharge(TestObjectCreator.CC1.PK, "TEST", 10m, 2m, true, TestObjectCreator.Creditor1.PK)
				});

			void AssertAutorating(AutoRateOptions options, params string[] expectedTypes)
			{
				var interactor = new TestInteractor();
				using (_Rating.Start(interactor, isEqualization: false))
				using (interactor.StartRatingSession())
				{
					var job = CreateDummyAutoRatingObjectWithJob(customsChargesMock.Object, NonMiscDeparment, TestObjectCreator.Debtor.MainAddress);
					var starter = new AutoRatingStarterCore(
						interactor,
						job,
						new RatingContext(),
						BillingType.Default,
						AdditionalJobsAction.AutoRateAdditionalInvoicingJobs,
						null,
						additionalJobsToDispose: null);

					starter.ExecuteAutorating(options);

					var logs = job.Logs.GetAllLogs()
						.Cast<StmALog>()
						.Where(l => l.SL_SE_NKEvent == Events.ChargesHaveBeenAutoRated.Code)
						.ToList();

					AssertContainsExactElementsInAnyOrder(expectedTypes, logs.Select(l => l.Parameters[EventReferenceParameters.Mode]));
				}
			}

			AssertAutorating(new AutoRateOptions(autoRateCost: true, autoRateRevenue: false), "Cost");
			AssertAutorating(new AutoRateOptions(autoRateCost: false, autoRateRevenue: true), "Revenue");
			AssertAutorating(new AutoRateOptions(autoRateCost: false, autoRateRevenue: false));
			AssertAutorating(new AutoRateOptions(autoRateCost: true, autoRateRevenue: true), "Cost", "Revenue");
		}

		public void TestExecuteAutorating_ShouldGenerateCAREventWithTypeParameter()
		{
			var customsChargesMock = new Mock<ICustomsCharges>();
			customsChargesMock
				.Setup(c => c.GetCustomsCharges(It.IsAny<ILogger>()))
				.Returns(new[]
				{
					new CustomsCharge(TestObjectCreator.CC1.PK, "TEST", 10m, 2m, true, TestObjectCreator.Creditor1.PK)
				});

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			{
				var job = CreateDummyAutoRatingObjectWithJob(customsChargesMock.Object, NonMiscDeparment, TestObjectCreator.Debtor.MainAddress);
				var starter = new AutoRatingStarterCore(
					interactor,
					job,
					new RatingContext(),
					BillingType.Default,
					AdditionalJobsAction.AutoRateAdditionalInvoicingJobs,
					null,
					additionalJobsToDispose: null);

				starter.ExecuteAutorating(AutoRateOptions.AutorateCosts);

				var log = job.Logs.GetAllLogs()
					.Cast<StmALog>()
					.FirstOrDefault(l => l.SL_SE_NKEvent == Events.ChargesHaveBeenAutoRated.Code);

				AssertEquals("Dummy Object", log.Parameters[EventReferenceParameters.Type]);
				AssertEquals("JOB1234", log.Parameters[EventReferenceParameters.JobNumber]);
			}
		}

		public void TestExecuteAutoRating_ShouldReportAutoRateUsageEvent()
		{
			var mockedCharges = new AutoRateInfoCollection(Factory);

			var charge1 = mockedCharges.AddNew(TestObjectCreator.FRT, "USD", 100);
			charge1.RateId = "Rate 1";
			charge1.RateProviderCode = WRConstants.RateProviders.CargoSphere;

			var charge2 = mockedCharges.AddNew(TestObjectCreator.FRT, "USD", 200);
			charge2.RateId = "Rate 2";
			charge2.RateProviderCode = WRConstants.RateProviders.CargoSphere;

			var charge3 = mockedCharges.AddNew(TestObjectCreator.FRT, "USD", 300);
			charge3.RateId = "Rate 3";
			charge3.RateProviderCode = WRConstants.RateProviders.CargoGuide;

			var charge4 = mockedCharges.AddNew(TestObjectCreator.FRT, "USD", 400);
			charge4.RateId = "Rate 4";
			charge4.RateProviderCode = ZString.Empty;

			var charge5 = mockedCharges.AddNew(TestObjectCreator.FRT, "USD", 500);
			charge5.RateId = "Rate 4";
			charge5.RateProviderCode = ZString.Empty;

			var dummy = new DummyAutoRatingObject(Factory)
			{
				IsInDatabaseOverride = true,
				IsImport = true,
				FreightMode = FreightMode.AIR,
				ContainerMode = "ULD",
				JobID = "McLaren"
			};

			var adapter = new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(dummy);
			dummy.adaptersExposed = new List<IAutoRating> { adapter };

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = NonMiscDeparment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;

			var options = new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Menu);
			Autorate(dummy, options, mockedCharges);

			var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.Autorate);
			var properties = messages.First().UsageProperties;

			AssertEquals("Cost", properties.Value<string>(UsageProperties.Mode));
			AssertEquals("BillOfLading", properties.Value<string>(UsageProperties.JobType));
			AssertEquals("Menu", properties.Value<string>(UsageProperties.TriggerSource));
			Assert(!properties.Value<bool>(UsageProperties.IsRateSelector));
			AssertEquals("ULD", properties.Value<string>(UsageProperties.ContainerMode));
			AssertEquals("AIR", properties.Value<string>(UsageProperties.TransportMode));
			AssertEquals("McLaren", properties.Value<string>(UsageProperties.JobID));
			Assert(properties.Value<int>(UsageProperties.ElapsedTime) > 0);

			var ratesResult = properties.Value<JObject>(UsageProperties.RatesSearchResult);

			var cargoSphereResults = ratesResult.Value<JObject>("CargoSphere");
			AssertEquals(2, cargoSphereResults.Value<int>("TotalRates"));
			AssertEquals(2, cargoSphereResults.Value<int>("TotalCharges"));

			var cargoguideResults = ratesResult.Value<JObject>("Cargoguide");
			AssertEquals(1, cargoguideResults.Value<int>("TotalRates"));
			AssertEquals(1, cargoguideResults.Value<int>("TotalCharges"));

			var cw1Results = ratesResult.Value<JObject>("CW1");
			AssertEquals(1, cw1Results.Value<int>("TotalRates"));
			AssertEquals(2, cw1Results.Value<int>("TotalCharges"));
		}

		public void TestExecuteAutoRating_ShouldReportAutoRateChargeUsageEvents()
		{
			var mockedCharges = new AutoRateInfoCollection(Factory);

			var header = Factory.New<RatingHeader>();
			header.TH_RateType = "COS";

			var entry1 = header.AddRateEntry("FCL", "SEA");
			var line1 = entry1.AddRateLine("FRT", "UNT", "KG");
			var charge1 = new AutoRateInfo(Factory, line1);

			var entry2 = header.AddRateEntry("AIR", "ULD");
			var line2 = entry2.AddRateLine("FRT", "CMB", "KG");
			var charge2 = new AutoRateInfo(Factory, line2);
			var charge3 = new AutoRateInfo(Factory, line2);

			mockedCharges.Add(charge1);
			mockedCharges.Add(charge2);
			mockedCharges.Add(charge3);

			var dummy = new DummyAutoRatingObject(Factory)
			{
				IsInDatabaseOverride = true,
				IsImport = true,
				FreightMode = FreightMode.AIR,
				ContainerMode = "ULD"
			};

			var adapter = new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(dummy);
			dummy.adaptersExposed = new List<IAutoRating> { adapter };

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = NonMiscDeparment.PK;
			job.JH_OA_LocalChargesAddr = TestObjectCreator.Debtor.MainAddress.PK;

			var options = new AutoRateOptions(autoRateCost: true, triggerSource: AutoRateTriggerSource.Menu);
			Autorate(dummy, options, mockedCharges);

			var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ChargeAutorated);
			var actual = messages.Select(m => new
			{
				Mode = m.UsageProperties.Value<string>(UsageProperties.Mode),
				JobType = m.UsageProperties.Value<string>(UsageProperties.JobType),
				ContainerMode = m.UsageProperties.Value<string>(UsageProperties.ContainerMode),
				TransportMode = m.UsageProperties.Value<string>(UsageProperties.TransportMode),
				ChargeCalculator = m.UsageProperties.Value<string>(UsageProperties.ChargeCalculator),
				RateCategory = m.UsageProperties.Value<string>(UsageProperties.RateCategory),
				RateMode = m.UsageProperties.Value<string>(UsageProperties.RateMode),
				RateType = m.UsageProperties.Value<string>(UsageProperties.RateType),
				Count = m.UsageProperties.Value<int>(UsageProperties.Count),
			}).ToArray();

			var expected = new[]
			{
				new
				{
					Mode = "Cost",
					JobType = "BillOfLading",
					ContainerMode = "ULD",
					TransportMode = "AIR",
					ChargeCalculator = "UNT",
					RateCategory = "FCL",
					RateMode = "SEA",
					RateType = "COS",
					Count = 1,
				},
				new
				{
					Mode = "Cost",
					JobType = "BillOfLading",
					ContainerMode = "ULD",
					TransportMode = "AIR",
					ChargeCalculator = "CMB",
					RateCategory = "AIR",
					RateMode = "ULD",
					RateType = "COS",
					Count = 2,
				}
			};

			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		IEnumerable<RatesAdditionInfo> Autorate(BusinessObject job, AutoRateOptions options, IEnumerable<AutoRateInfo> autoRateInfos)
		{
			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			{
				var starter = new MockAutoRatingStarter(
					interactor,
					job,
					new RatingContext(),
					autoRateInfos);

				return starter.ExecuteAutorating(options);
			}
		}

		DummyAutoRatingObject CreateDummyAutoRatingObjectWithJob(ICustomsCharges customsCharges, GlbDepartment department, OrgAddress localChargesAddress)
		{
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(customsCharges);
			dummy.IsInDatabaseOverride = true;
			dummy.IsImport = true;

			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = department.PK;
			job.JH_OA_LocalChargesAddr = localChargesAddress.PK;

			return dummy;
		}

		public void TestExecuteAutoratingRunsOnASingleObject()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var ratingContext = new RatingContext();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			AssertNoErrors("should have no errors as the job hasn't been validated yet.", job1);
			var testInteractor = new TestInteractor();
			var ratingPlugin = new AutoRatingStarterCore(testInteractor, dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			ratingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals("ExecuteAutorating() should validate job on all plugins and found errors on job", true, job1.HasErrors);
			AssertContains("Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.", testInteractor.Errors[0]);
		}

		public void TestExecuteAutoratingThrowsExceptionsNotErrorMessages()
		{
			var dummy = new DummyAutoRatingObject(Factory);
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			job.JH_UniqueJobInvoiceNumber = 212;

			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();

			AssertExceptionThrownInAutoRating(interactor, dummy, ratingContext, new ArgumentException());
		}

		void AssertExceptionThrownInAutoRating(TestInteractor interactor, IBusiness autoRatingObject, RatingContext context, Exception exceptionToThrow)
		{
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(autoRatingObject))
			{
				var starter = new MockAutoRatingStarter(interactor, autoRatingObject, context, exceptionToThrow);
				AssertExceptionThrown(typeof(ArgumentException), () => starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue));
			}
		}

		class MockAutoRatingStarter : AutoRatingStarterCore
		{
			public MockAutoRatingStarter(TestInteractor interactor, IBusiness autoRatingObject, RatingContext context, Exception exceptionToThrow = null)
				: base(interactor, autoRatingObject, context, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null)
			{
				this.AutoRatingObject = autoRatingObject;
				this.Context = context;
				this.toThrow = exceptionToThrow;
			}

			public MockAutoRatingStarter(TestInteractor interactor, IBusiness autoRatingObject, RatingContext context, IEnumerable<AutoRateInfo> autoRateInfos)
				: base(interactor, autoRatingObject, context, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null)
			{
				this.AutoRatingObject = autoRatingObject;
				this.Context = context;
				this.autoRateInfos = autoRateInfos;
			}

			readonly IBusiness AutoRatingObject;
			readonly RatingContext Context;
			readonly Exception toThrow;
			readonly IEnumerable<AutoRateInfo> autoRateInfos;

			internal override AutoRatingRunner GetRunner(IAutoRatingStrategy strategy)
			{
				return autoRateInfos != null
					? new MockAutoRatingRunner(AutoRatingObject, Context, autoRateInfos)
					: new MockAutoRatingRunner(AutoRatingObject, Context, toThrow);
			}

			class MockAutoRatingRunner : AutoRatingRunner
			{
				public MockAutoRatingRunner(IBusiness parentConsumer, IRatingContext ratingContext, Exception toThrow) : base(parentConsumer, ratingContext)
				{
					this.toThrow = toThrow;
				}

				public MockAutoRatingRunner(IBusiness parentConsumer, IRatingContext ratingContext, IEnumerable<AutoRateInfo> autoRateInfos) : base(parentConsumer, ratingContext)
				{
					this.autoRateInfos = autoRateInfos;
				}

				override protected AutoRater GetNewAutoRater()
				{
					if (autoRateInfos != null)
					{
						return new MockAutorater(autoRateInfos);
					}

					throw toThrow;
				}

				readonly Exception toThrow;
				readonly IEnumerable<AutoRateInfo> autoRateInfos;
			}

			class MockAutorater : AutoRater
			{
				public MockAutorater(IEnumerable<AutoRateInfo> autoRateInfos)
				{
					this.autoRateInfos = autoRateInfos;
				}

				public override AutoRateResult AutoRate(
					BusinessObjectFactory factory,
					IAutoRating itemToRate,
					CostSell costOrSell,
					IRatingContext ratingContext)
				{
					var result = new AutoRateResult(factory, costOrSell);
					result.RateInfoCollection.AddRange(autoRateInfos);

					return result;
				}

				readonly IEnumerable<AutoRateInfo> autoRateInfos;
			}
		}

		public void TestRateForExportAlso()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var ratingContext = new RatingContext();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = false;
			var strategy = new AutoRateInvoicingStrategy(dummy, null);
			var ratingPlugin = new AutoRatingStarterCore(new TestInteractor(), dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			var jobs = ratingPlugin.GetCustomsJobsToRate(strategy);
			AssertEquals(1, jobs.Length);
			AssertEquals("should be the correct job", true, ReferenceEquals(chargesMock.Object, jobs[0]));
			strategy.Job.Dispose();
		}

		public void TestRateWhereJobIsInActive()
		{
			var dummy = new DummyAutoRatingObject(Factory);
			var ratingContext = new RatingContext();
			var strategy = new AutoRateInvoicingStrategy(dummy, null);
			var autoratingPlugin = new AutoRatingStarterCore(new LoggerDecorator(), dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			dummy.adaptersExposed = new List<IAutoRating> { new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = true }, new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = true } };
			AssertEquals(2, strategy.GetRatingAdaptersCount(AutoRateOptions.AutorateRevenue));
			dummy.adaptersExposed = new List<IAutoRating> { new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = true }, new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = false } };
			AssertEquals(1, strategy.GetRatingAdaptersCount(AutoRateOptions.AutorateRevenue));
			dummy.adaptersExposed = new List<IAutoRating> { new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = false }, new DummyAutoRatingObjectRatingAdapter<DummyAutoRatingObject>(new DummyAutoRatingObject())
			{ IsActive = false } };
			AssertEquals(0, strategy.GetRatingAdaptersCount(AutoRateOptions.AutorateRevenue));
			strategy.Job.Dispose();
		}

		[ExpectNoExceptions]
		public void TestRateChargesNullJob()
		{
			var shipment = (BusinessObject)new BusinessObjectFactory().New<Enterprise.Integration.Freight.ICommonShipment>();
			shipment[ZArchitecture.Schema.JobShipmentSchema.JS_UnitFreightRate] = 100m;
			shipment[ZArchitecture.Schema.JobShipmentSchema.JS_RX_NKFrtRateCurrency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(new TestInteractor(), shipment, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
		}

		public void TestDebtorMustBeValidWhenAutorateJob()
		{
			var chargesMock = new Mock<ICustomsCharges>();
			var ratingContext = new RatingContext();
			var dummy = new DummyAutoRatingObject(Factory);
			dummy.AdditionalCharges.Add(chargesMock.Object);
			dummy.IsImport = true;
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			var strategy = new AutoRateInvoicingStrategy(dummy, job);
			var starter = new AutoRatingStarterCore(new TestInteractor(), dummy, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			Assert(starter.GetPreAutoratingErrors(strategy).Contains("Autorating cannot be run. Please enter Local Client or Overseas Agent."));
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = orgHeader1.PK;
			job.AgentCollectPK = orgHeader1.PK;
			Assert(starter.GetPreAutoratingErrors(strategy).Contains("Overseas Agent and Local Client must not be the same."));
			job.AgentCollectPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, starter.GetPreAutoratingErrors(strategy));
			job.LocalChargesPK = ZGuid.Empty;
			job.AgentCollectPK = orgHeader2.PK;
			AssertEquals(ZString.Empty, starter.GetPreAutoratingErrors(strategy));
			job.LocalChargesPK = orgHeader1.PK;
			job.AgentCollectPK = orgHeader2.PK;
			AssertEquals(ZString.Empty, starter.GetPreAutoratingErrors(strategy));
			job.Delete();
			Assert(starter.GetPreAutoratingErrors(strategy).Contains("Job is deleted."));
		}

		public void TestReasonNotToAllowAutoRate()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var ratingContext = new RatingContext();
			var dummyParent = new DummyJobInvoicingPlugIn(Factory);
			job.Parent = dummyParent;
			((DummyJobInvoicingPlugInJobInvoicingSupporter)dummyParent.InvoicingSupporter).ReasonNotToAllowAutoRate = "Testing the ReasonNotToAllowAutoRate";
			((DummyJobInvoicingPlugInJobInvoicingSupporter)dummyParent.InvoicingSupporter).WarningForContinueAutoRate = "Testing the WarningForContinueAutoRate";
			var strategy = new AutoRateInvoicingStrategy(dummyParent, job);
			var starter = new AutoRatingStarterCore(new TestInteractor(), dummyParent, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
			AssertEquals("Testing the ReasonNotToAllowAutoRate", starter.GetPreAutoratingErrors(strategy));
			AssertEquals("Testing the WarningForContinueAutoRate", starter.GetPreAutoratingWarnings(strategy));
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = new DummyAutoRatingObject();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestExecuteAutoratingNoCharges()
		{
			var dummy = new DummyAutoRatingObject(Factory);
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var testInteractor = new TestInteractor();
			var ratingPlugin = new AutoRatingStarter(dummy, testInteractor);
			dummy.HumanReadableNameForTest = ZString.Empty;
			ratingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			var expected = @" was auto-costed.
	No costs were found.";
			AssertCollectionContains(expected, testInteractor.YesNoWarnings);
			dummy.HumanReadableNameForTest = "Dummy Object 0001";
			testInteractor.Information.Clear();
			ratingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			expected = @"Dummy Object 0001 was auto-costed.
	No costs were found.";
			AssertCollectionContains(expected, testInteractor.YesNoWarnings);
		}

		public void TestExecuteAutoratingShouldNotRecalculateChargesExRate()
		{
			var dummy = new DummyAutoRatingObject(Factory);
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")).PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 7.1105m);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100, 100);
			charge1.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge1.JR_RX_NKCostCurrency = "USD";
			charge1.JR_OSCostExRate = 7.1105m;
			charge1.JR_LocalCostAmt = 592.93m;
			charge1.JR_OSCostAmt = 4216m;
			charge1.JR_CostRatingOverride = false;
			charge1.JR_RX_NKSellCurrency = "USD";
			charge1.JR_OSSellExRate = 7.1105m;
			charge1.JR_LocalSellAmt = 592.93m;
			charge1.JR_OSSellAmt = 4216m;
			charge1.JR_SellRatingOverride = false;
			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(7.1105m, charge1.JR_OSCostExRate);
			AssertEquals(592.93m, charge1.JR_LocalCostAmt);
			AssertEquals(4216m, charge1.JR_OSCostAmt);
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);
			AssertEquals(7.1105m, charge1.JR_OSSellExRate);
			AssertEquals(592.93m, charge1.JR_LocalSellAmt);
			AssertEquals(4216m, charge1.JR_OSSellAmt);
			var apLine = Factory.NewWithValidTestData<AccTransactionLines>();
			apLine.AL_LineType = "CST";
			apLine.AL_RevRecognitionType = "DEF";
			charge1.JR_AL_APLine = apLine.PK;
			AssertEquals(true, charge1.IsCostPosted);
			var testInteractor = new TestInteractor();
			var ratingPlugin = new AutoRatingStarter(dummy, testInteractor);
			dummy.HumanReadableNameForTest = ZString.Empty;
			ratingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			AssertEquals(0, testInteractor.Errors.Count);
			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals(7.1105m, charge1.JR_OSCostExRate);
			AssertEquals(592.93m, charge1.JR_LocalCostAmt);
			AssertEquals(4216m, charge1.JR_OSCostAmt);
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);
			AssertEquals(7.1105m, charge1.JR_OSSellExRate);
			AssertEquals(592.93m, charge1.JR_LocalSellAmt);
			AssertEquals(4216m, charge1.JR_OSSellAmt);
		}

		public void TestExecuteAutoRating_ShouldntModifyInaccessibleCharges()
		{
			#region Job/Charge Setup
			var dummy = new DummyAutoRatingObject(Factory);
			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "Example";
			job.JH_ParentID = dummy.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = TestObjectCreator.FEADepartment.PK;
			job.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 7.1105m);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100, 100);
			charge.JR_GE = TestObjectCreator.FEADepartment.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostExRate = 7.1105m;
			charge.JR_LocalCostAmt = 592.93m;
			charge.JR_OSCostAmt = 4216m;
			charge.JR_CostRatingOverride = false;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 7.1105m;
			charge.JR_LocalSellAmt = 592.93m;
			charge.JR_OSSellAmt = 4216m;
			charge.JR_SellRatingOverride = false;
			#endregion

			#region Security Setup
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();

			var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			// Don't allow user to access FEA.
			var loginSecurityCurrentBranch = securityFactory.New<GlbSecurity>();
			loginSecurityCurrentBranch.GU_GB = Env.CurrentBranch.PK;
			loginSecurityCurrentBranch.GU_GE = TestObjectCreator.FEADepartment.PK;
			loginSecurityCurrentBranch.GU_GS = testUser.PK;
			loginSecurityCurrentBranch.GU_SecurityRight = security.Login.Code;
			loginSecurityCurrentBranch.GU_SecurityItemIsAllowed = false;

			// Disallow the user modifying charges outside their department (FES).
			var chargeSecurity = securityFactory.New<GlbSecurity>();
			chargeSecurity.GU_GB = Env.CurrentBranch.PK;
			chargeSecurity.GU_GE = TestObjectCreator.FESDepartment.PK;
			chargeSecurity.GU_GS = testUser.PK;
			chargeSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			chargeSecurity.GU_SecurityItemIsAllowed = false;
			securityFactory.Save();
			#endregion

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			{
				// SetTemporaryUserContext doesn't wipe the cache so the cached auth check needs to be cleared.
				Factory.ClearCachedValue<bool>("Login BRN:BNE DEP:FEA");

				AssertEquals(expected: false, charge.HasChanges);

				var testInteractor = new TestInteractor();
				var ratingPlugin = new AutoRatingStarter(dummy, testInteractor);

				ratingPlugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals("Charge should not be modified as testUser does not have access to modify it", expected: false, charge.HasChanges);
			}
		}

		public void TestUsersPromptedToCreateJobs_NoJobsExist()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			TestObjectCreator.CreateFlatCalculatorClientRate("AIR", "LSE", "AUSYD", "NZAKL", consignee, "FRT", 500m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol, transportMode: Core.Constants.TransportModes.Air);
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();
			var consolJob = JobsAttachedToParentPK(consol.PK).FirstOrDefault();
			var shipmentJob = JobsAttachedToParentPK(shipment.PK).FirstOrDefault();
			AssertNull("Pre-condition: Expected not to find any job attached to the consol", consolJob);
			AssertNull("Pre-condition: Expected not to find any job attached to the shipment", shipmentJob);
			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();

			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			{
				var starter = new AutoRatingStarterCore(interactor, consol, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			}
			consolJob = JobsAttachedToParentPK(consol.PK).FirstOrDefault();
			shipmentJob = JobsAttachedToParentPK(shipment.PK).FirstOrDefault();
			AssertNull("Expected not to have created a job for the consol. Consols default to apportionment strategies which we do not create jobs for", consolJob);
			AssertNotNull("Expected have created a job for the shipment as shipments will default to job invoicing strategies which we will create jobs", shipmentJob);
			shipmentJob.Dispose();
		}

		public void TestUsersPromptedToCreateJobs_OnRelatedBusinessObjects()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			TestObjectCreator.CreateFlatCalculatorClientRate("AIR", "LSE", "AUSYD", "NZAKL", consignee, "FRT", 500m);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001", "AUSYD", "NZAKL", consol, transportMode: Core.Constants.TransportModes.Air);
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			var manuallyCreatedShipmentJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			manuallyCreatedShipmentJob.JH_ParentID = shipment2.PK;
			manuallyCreatedShipmentJob.JH_GC = GlbCompany.CurrentCompany.PK;
			manuallyCreatedShipmentJob.JH_GE = NonMiscDeparment.PK;
			Factory.Save();
			var generatedShipmentJob = JobsAttachedToParentPK(shipment1.PK).FirstOrDefault();
			AssertNull("Expected not to find any job attached to the shipment", generatedShipmentJob);
			var interactor = new TestInteractor();
			var ratingContext = new RatingContext();
			var starter = new AutoRatingStarterCore(interactor, consol, ratingContext, BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);

			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			{
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
			}
			generatedShipmentJob = JobsAttachedToParentPK(shipment1.PK).FirstOrDefault();
			AssertNotNull("Expected have created a job for the jobless shipment", generatedShipmentJob);
			AssertEquals("Expected not to have created any additional jobs for shipment 2.", 1, JobsAttachedToParentPK(shipment2.PK).Length);
			generatedShipmentJob.Dispose();
		}

		public void TestJobIsDeleted_WhenThereIsNoChargeAndNotInDatabase()
		{
			var shipment = TestObjectCreator.CreateShipment("S000001", "AUSYD", "NZAKL", null, transportMode: Core.Constants.TransportModes.Air);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = TestObjectCreator.LocalClient.PK;
			Factory.Save();

			var shipmentJob = JobsAttachedToParentPK(shipment.PK).FirstOrDefault();
			AssertNull("Pre-condition: Expected not to find any job attached to the shipment", shipmentJob);

			var newFactoryForLoading = new BusinessObjectFactory();
			shipment = newFactoryForLoading.Load<ForwardingShipment>(shipment.PK);

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor))
			using (interactor.StartRatingSession())
			{
				var starter = new AutoRatingStarterCore(interactor, shipment, new RatingContext(), BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				shipmentJob = JobsAttachedToParentPK(newFactoryForLoading, shipment.PK).FirstOrDefault();
				AssertNull("Since there is no charge and job was not in database initially, job should be deleted upon finishing the auto-rating process", shipmentJob);
			}

			using (TestObjectCreator.NonCurrentBranch.SetAsTemporaryContext())
			{
				var factoryToSimulateSecondUser = new BusinessObjectFactory();
				var shipmentLoadedBySecondUser = factoryToSimulateSecondUser.Load<ForwardingShipment>(shipment.PK);

				interactor = new TestInteractor();
				using (_Rating.Start(interactor))
				using (interactor.StartRatingSession())
				{
					var starter = new AutoRatingStarterCore(interactor, shipmentLoadedBySecondUser, new RatingContext(), BillingType.Default, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null, additionalJobsToDispose: null);
					starter.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
				}

				shipmentJob = JobsAttachedToParentPK(factoryToSimulateSecondUser, shipmentLoadedBySecondUser.PK).FirstOrDefault();
				AssertNull("Since there is no charge and job was not in database initially, job should be deleted upon finishing the auto-rating process", shipmentJob);

				factoryToSimulateSecondUser.Save();
			}

			AssertNoExceptionThrown(() => newFactoryForLoading.Save());
		}

		JobHeader[] JobsAttachedToParentPK(ZGuid parentPK)
			=> JobsAttachedToParentPK(Factory, parentPK);

		static JobHeader[] JobsAttachedToParentPK(BusinessObjectFactory factory, ZGuid parentPK)
			=> factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, parentPK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));

		[CodeProperty("JobID")]
		public class DummyAutoRatingObject : NonPersistentBusinessObject, IRatingSupporter, IJobInvoicingPlugIn, ICustomsChargesAdditionalCharges, IStmALogParent, IStmNoteParent
		{
			public DummyAutoRatingObject(BusinessObjectFactory factory) : base(factory)
			{
			}

			public DummyAutoRatingObject()
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get
				{
					return HumanReadableNameForTest;
				}
			}

			public ZString HumanReadableNameForTest = "Dummy Object";
			public ZString Destination
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ZString Origin
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ZString Via
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader Carrier
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader Consignee
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ZString ConsigneeCartageEquipment
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public IDocAddress ConsigneeDeliveryAddress
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader Consignor
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ZString ConsignorCartageEquipment
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public IDocAddress ConsignorPickupAddress
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader[] TransportProviders
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public FreightMode FreightMode { get; set; } = FreightMode.FCL;
			public ZString ContainerMode { get; set; }

			public ZString HousebillReleaseType
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public PaymentTermInfos PaymentTerm
			{
				get
				{
					return null;
				}
			}

			public ZBool IsImport
			{
				[System.Diagnostics.DebuggerStepThrough]
				get
				{
					return isImport;
				}

				[System.Diagnostics.DebuggerStepThrough]
				set
				{
					isImport = value;
				}
			}

			ZBool isImport;
			public ZBool IsExport
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ZBool IsDomestic
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public MoneyType MonetaryValues
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public ServiceLevelRatingInformation ServiceLevel
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgAddress WharfCTOAddress
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader ReceivingAgent
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			public OrgHeader SendingAgent
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			JobInvoicingSupporter fInvoicingSupporter;
			public IJobInvoicingSupporter InvoicingSupporter
			{
				get
				{
					return fInvoicingSupporter ?? (fInvoicingSupporter = new JobInvoicingSupporter(this));
				}
			}

			public void OnJobCreating(JobHeader job)
			{
			}

			public void OnJobCreated(JobHeader job)
			{
			}

			public void OnJobDeleting(JobHeader job)
			{
			}

			public void OnJobDeleted(JobHeader job)
			{
			}

			public void SetJobNumberFieldOnSaving()
			{
			}

			public bool AllowInvoiceDeletion
			{
				get
				{
					return true;
				}
			}

			public string JobNumber { get; set; } = "JOB1234";
			public ZString JobID { get; set; }

			ICustomsCharges[] ICustomsChargesAdditionalCharges.AdditionalCharges
			{
				get
				{
					return AdditionalCharges.ToArray();
				}
			}

			public List<ICustomsCharges> AdditionalCharges
			{
				[System.Diagnostics.DebuggerStepThrough]
				get
				{
					return additionalCharges;
				}
			}

			readonly List<ICustomsCharges> additionalCharges = new List<ICustomsCharges>();
			public Directions JobDirection
			{
				get
				{
					return Directions.Unknown;
				}
			}

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get
				{
					var provider = new DummyRatingAdaptersProvider<DummyAutoRatingObject>(this);
					provider.additionalJobsExposed = additionalJobsExposed;
					provider.ratingAdaptersExposed = adaptersExposed;
					return provider;
				}
			}

			public ZGuid LogsParentPK => PK;

			public string LogsParentTableName => TableName;

			public BusinessObject[] BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

			public bool DeferFiringWorkflow => false;

			public Logs Logs => logs ?? (logs = new Logs(this));
			Logs logs;

			public BusinessObjectFactory LogsFactory => Factory;

			public void ProcessLog(IStmALog log)
			{
			}

			public bool? IsInDatabaseOverride { get; set; }

			public override bool IsInDatabase => IsInDatabaseOverride ?? base.IsInDatabase;

			public override string TableName => "Dummy";

			public Notes Notes => notes ?? (notes = new Notes(this));
			Notes notes;

			public ZGuid NotesParentPK => PK;

			public string NotesParentTableName => TableName;

			public BusinessObjectFactory NotesFactory => Factory;

			public GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate { get; set; }

			public bool SupportsNotes => true;

			public BusinessObject[] BusinessObjectsWithRelatedNotes => Array.Empty<BusinessObject>();

			public StmNoteContexts NoteContextsForRelatedNotes => StmNoteContexts.Default;

			public NoteTypeCollection NoteTypes => new NoteTypeCollection();

			public ReadOnlyCollection<IJobInvoicingPlugIn> additionalJobsExposed;
			public List<IAutoRating> adaptersExposed;
		}

		class DummyAutoRatingObjectRatingAdapter<T> : RatingAdapter<T> where T : DummyAutoRatingObject
		{
			public DummyAutoRatingObjectRatingAdapter(T parent) : base(parent)
			{
			}

			public override IJobDatesProvider JobDatesProvider
			{
				get
				{
					return new JobDatesProvider<DummyAutoRatingObject>(Parent);
				}
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get
				{
					return JobInvoicingConsumerTypes.Consol;
				}
			}

			public override MergeChargeOptions MergeCharges
			{
				get
				{
					return MergeChargeOptions.WithinAdapter;
				}
			}

			public override RateType RateTypeToUse
			{
				get
				{
					return RateType.Forwarding;
				}
			}

			public override AutoRatingStatusInfo StatusInformation
			{
				get
				{
					var info = new AutoRatingStatusInfo(true, "");
					info.IsActive = IsActive;
					return info;
				}
			}

			public override IJobInvoicingSupporter InvoicingSupporter
			{
				get
				{
					return Parent.InvoicingSupporter;
				}
			}

			public override FreightMode FreightMode => Parent.FreightMode;
			public override ZString ContainerMode => Parent.ContainerMode;

			public override Directions JobDirection
			{
				get
				{
					return Parent.JobDirection;
				}
			}

			public override ServiceLevelRatingInformation ServiceLevel
			{
				get
				{
					return new ServiceLevelRatingInformation(new ServiceLevelInfo());
				}
			}

			public override PaymentTermInfos PaymentTerm
			{
				get
				{
					return Parent.PaymentTerm;
				}
			}

			public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
			{
				return true;
			}

			public bool IsActive = true;
		}

		internal class DummyRatingAdaptersProvider<T> : RatingOrCostingAdaptersProvider<T> where T : DummyAutoRatingObject
		{
			public DummyRatingAdaptersProvider(T parent) : base(parent, JobInvoicingConsumerTypes.Consol)
			{
			}

			public List<IAutoRating> ratingAdaptersExposed;
			public ReadOnlyCollection<IJobInvoicingPlugIn> additionalJobsExposed;
			protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
			{
				return ratingAdaptersExposed ?? base.GetAdapters(parent, uiInteractor, options);
			}

			protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(T parent)
			{
				return additionalJobsExposed ?? base.GetAdditionalJobs(parent);
			}

			public override ZString ConsumerTypeDescription { get; } = "Dummy Object";
		}

		public class DummyAutoRatingConsolObject : DummyAutoRatingObject, IJobCostingPlugIn, IRatingSupporter
		{
			public DummyAutoRatingConsolObject(BusinessObjectFactory factory) : base(factory)
			{
			}

			public DummyAutoRatingConsolObject()
			{
			}

			public virtual IGenericJobCostSupporter CostSupporter
			{
				get
				{
					return new GenericJobCostSupporter();
				}
			}

			public CodeDescriptionPairList PrepaidCollectList
			{
				get
				{
					return new CodeDescriptionPairList();
				}
			}

			public RefCurrency ConsolCurrency
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public decimal ConsolExchangeRate
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
			{
				throw new NotImplementedException();
			}

			public bool IsExportConsol
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool IsImportConsol
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool IsMasterCollect
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString JK_UniqueConsignRef
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString JK_MasterBillNum
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public JobProfitLossCollection ProfitLossContainer
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public OrgHeader ReceivingAgentAPInvoicingParty
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public OrgHeader ReceivingAgentARInvoicingParty
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public OrgHeader SendingAgentAPInvoicingParty
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public OrgHeader SendingAgentARInvoicingParty
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public IJobInvoicingPlugIn[] ShipmentsList
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZGuid[] ShipmentsListPKs
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString TransportMode
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString JK_TransportMode
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZGuid JK_OA_CreditorAddress
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZString JK_ConsolChargeableUnit
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ZBool IsDomesticFreight
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool IsBuyersConsol
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public void AddNewToLogs(Event @event, ZString reference)
			{
				throw new NotImplementedException();
			}

			public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob)
			{
				return ZString.Empty;
			}

			public IEnumerable<ZString> ExcludedApportionmentMethods
			{
				get
				{
					return Enumerable.Empty<ZString>();
				}
			}

			public bool IsApportionmentFilterEnabled
			{
				get
				{
					return true;
				}
			}

			public RefUNLOCO DischargePort
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public RefUNLOCO LoadPort
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			RatingAdaptersProvider IRatingSupporter.AdaptersProvider
			{
				get
				{
					return new DummyRatingAdaptersProvider<DummyAutoRatingConsolObject>(this);
				}
			}

			public ZString ConsolType => throw new NotImplementedException();

			public ZString Module => throw new NotImplementedException();

			public ZString Direction => throw new NotImplementedException();
		}

		class DummyAutoRatingObjectWithValidationSuspender : DummyAutoRatingObject, IValidationSuspenderForAutoRating
		{
			public DummyAutoRatingObjectWithValidationSuspender(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public int OnValidationResumedHitCount { get; private set; }

			public void OnValidationResumed()
			{
				OnValidationResumedForTesting?.Invoke();
			}

			public Action OnValidationResumedForTesting { get; set; }

			public bool ShouldRunPreSaveValidationBeforeAutorating(IBusiness hostEntity)
			{
				return ShouldRunPreSaveValidationBeforeAutoratingForTesting;
			}

			public bool ShouldRunPreSaveValidationBeforeAutoratingForTesting { get; set; }
		}

		class DummyAutoRatingObjectWithJobHeaderCreateOverride : DummyAutoRatingObject, IAutoRatingSuppressJobCreationPrompt
		{
			public DummyAutoRatingObjectWithJobHeaderCreateOverride(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public bool SuppressJobCreationPrompt { get; set; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected UsageCollectorTestHelper UsageCollectorTestHelper => usageCollectorTestHelper ?? (usageCollectorTestHelper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper usageCollectorTestHelper;

		protected TestObjectCreator TestObjectCreator;
		GlbDepartment NonMiscDeparment
		{
			get
			{
				return nonMiscDeparment ?? (nonMiscDeparment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA")));
			}
		}

		GlbDepartment nonMiscDeparment;
	}
}
