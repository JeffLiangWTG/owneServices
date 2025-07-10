using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[TestedType(typeof(ApportionmentListing))]
	public class ApportionmentListingTest : NonPersistentBusinessObjectTestCase
	{
		public class ApportionmentListingForTest : ApportionmentListing
		{
			public ApportionmentListingForTest(BusinessObjectFactory factory, IGenericJobCostPlugIn consol) : base(factory, consol)
			{
			}

			public void DoReleaseMutexesOnUnusedJobs()
			{
				ReleaseMutexesOnUnusedJobs();
			}
		}

		public void TestApportionmentListingStates()
		{
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C1111");
			TestObjectCreator.CreateShipment("S1111", consol1);
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C2222");
			var shipment2 = TestObjectCreator.CreateShipment("S2222", consol2);
			TestObjectCreator.CreateJob(shipment2, createWithMutex: false);

			var apportionmentListing1 = new ApportionmentListing(Factory, consol1);
			var apportionmentListing2 = new ApportionmentListing(Factory, consol2);
			AssertEquals("Initial state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing1.State);
			AssertEquals("Initial state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing2.State);

			apportionmentListing1.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			apportionmentListing2.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			AssertEquals(1, apportionmentListing1.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("apportionmentListing1 creates job, state should be Loaded", ApportionmentListingStates.Loaded, apportionmentListing1.State);
			AssertEquals(0, apportionmentListing2.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("apportionmentListing2 loads existing job,, state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing2.State);

			Factory.Save();
			AssertEquals(0, apportionmentListing1.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("apportionmentListing1 deletes unused job, state should be Cleaned.", ApportionmentListingStates.Cleaned, apportionmentListing1.State);
			AssertEquals(0, apportionmentListing2.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("No jobs to clean in apportionmentListing2, but clean up code still runs, state should be Cleaned.", ApportionmentListingStates.Cleaned, apportionmentListing2.State);
		}

		public void TestLoadChildShipmentsWithSetJobHasChangesSuspender()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportSeaFcl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new JobInvoicingDefaultDepartmentsCollection());
			var consol = TestObjectCreator.CreateConsol(consolNum: "C1111");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var apportionmentListing1 = new ApportionmentListing(Factory, consol);
			apportionmentListing1.PrepareForConsolCosting();
			consol.Shipments.Add(shipment);
			Factory.Save();

			var loader = new Job.Loader(Factory, shipment);
			var job = loader.Load();
			Assert("HasChanges should no be true", !job.HasChanges);

			apportionmentListing1.ReleaseMutexes();
		}

		public void TestApportionmentListingOnlyDeletesJobsThatWereCreatedByItSelf()
		{
			AssertUnusedJobsCleanUp(false);
		}

		public void TestNewJobsCreatedWhenGettingLockedChildShipmentsJobsErrorMessageIsNotSavedToDatabase()
		{
			AssertUnusedJobsCleanUp(true);
		}

		void AssertUnusedJobsCleanUp(bool isTestingValidation)
		{
			var creator = new TestObjectCreator(Factory);
			var existingShipmentWithoutJob = creator.CreateShipment("S0001", true);
			var existingShipmentWithJobInDB = creator.CreateShipment("S0002", true);
			creator.CreateJob(existingShipmentWithJobInDB);
			var existingShipmentWithJobInMemory = creator.CreateShipment("S0003", true);
			var gatewayConsol = creator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			Factory.Save();

			gatewayConsol.Shipments.Add(existingShipmentWithoutJob);
			gatewayConsol.Shipments.Add(existingShipmentWithJobInDB);
			gatewayConsol.Shipments.Add(existingShipmentWithJobInMemory);
			var newShipmentWithoutJob = creator.CreateShipment("S0004", gatewayConsol);
			var newShipmentWithJobInMemory = creator.CreateShipment("S0005", gatewayConsol);
			using (creator.CreateJob(existingShipmentWithJobInMemory))
			using (creator.CreateJob(newShipmentWithJobInMemory))
			{
				var apportionmentListing = new ApportionmentListing(Factory, gatewayConsol);
				AssertEquals(0, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
				AssertShipmentJobsExist(new[] { existingShipmentWithJobInDB, existingShipmentWithJobInMemory, newShipmentWithJobInMemory }, false);

				if (isTestingValidation)
				{
					var errorMessage = apportionmentListing.GetLockedChildShipmentsJobsErrorMessage();
					Assert("No errors since all jobs not are unlocked.", errorMessage.IsEmpty);
					AssertEquals(0, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
				}
				else
				{
					apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();

					AssertEquals(2, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
					AssertContainsExactElementsInAnyOrder(new[] { existingShipmentWithoutJob.PK, newShipmentWithoutJob.PK }, apportionmentListing.JobsWithMutexes_ForTestOnly.Select(j => j.JH_ParentID));
					AssertShipmentJobsExist(new[] { existingShipmentWithJobInDB, existingShipmentWithJobInMemory, newShipmentWithJobInMemory, existingShipmentWithoutJob, newShipmentWithoutJob }, false);

					Factory.Save();
					AssertShipmentJobsExist(new[] { existingShipmentWithJobInDB, existingShipmentWithJobInMemory, newShipmentWithJobInMemory }, true);
				}
			}

			void AssertShipmentJobsExist(ForwardingShipment[] expectedShipmentsToHaveJobCreated, bool useNewFactory)
			{
				var factory = useNewFactory ? new BusinessObjectFactory() : Factory;
				foreach (ForwardingShipment shipment in gatewayConsol.Shipments)
				{
					var shipmentJob = new JobHeader.Loader(factory, shipment).Load();
					if (expectedShipmentsToHaveJobCreated.Contains(shipment))
					{
						AssertNotNull($"Job for {shipment.JS_UniqueConsignRef} must exist.", shipmentJob);
					}
					else
					{
						AssertNull($"Job for {shipment.JS_UniqueConsignRef} must NOT exist.", shipmentJob);
					}
				}
			}
		}

		public void TestErrorIsReportedWhenFactoryIsSavedSuccessfullyButJobsAreNotCleanedUp()
		{
			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1111", consol);
			var apportionmentListing = new ApportionmentListingThatDoesNotCleanUp(Factory, consol);
			AssertEquals("Initial state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing.State);

			apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			AssertEquals(1, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("apportionmentListing1 created job, state should be Loaded", ApportionmentListingStates.Loaded, apportionmentListing.State);

			using (new DisposableAction(() => ErrorReporter.Clear(), () => ErrorReporter.Clear()))
			{
				Factory.Save();
				AssertEquals("ApportionmentListing_FactorySavedButUnusedJobsWereNotCleanedUp", ErrorReporter.LastKeyReported);
				AssertEquals("Factory was saved successfully, but unused job clean up code did not run. Unused jobs might have been saved to database.", ErrorReporter.LastMessageReported);
			}
		}

		class ApportionmentListingThatDoesNotCleanUp : ApportionmentListing
		{
			public ApportionmentListingThatDoesNotCleanUp(BusinessObjectFactory factory, IGenericJobCostPlugIn consol) : base(factory, consol)
			{
			}

			protected internal override void ReleaseMutexesOnUnusedJobs()
			{
				//Do not clean up unused jobs
			}
		}

		public void TestErrorIsNotReportedWhenApportionmentListingCreatesJobWhenFactoryIsInTransaction()
		{
			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1111", consol);

			var apportionmentListing = new ApportionmentListingThatDoesNotCleanUp(Factory, consol);
			AssertEquals("Initial state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing.State);

			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
				AssertEquals(1, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
				AssertEquals("apportionmentListing1 created job, state should be Loaded", ApportionmentListingStates.Loaded, apportionmentListing.State);
				Assert(!ErrorReporter.HasBeenReported("ApportionmentListing_NewJobCreatedWhenFactoryIsInSaveTransaction"));
				apportionmentListing.ReleaseMutexes();
			}
		}

		public void TestErrorIsReportedWhenApportionmentListingCreatesJobWhenFactoryIsInSaveTransaction()
		{
			var consol = TestObjectCreator.CreateConsol();
			var apportionmentListing = new ApportionmentListingThatCreateJobDuringFactoryTransaction(Factory, consol);
			AssertEquals("Initial state should be NotLoaded", ApportionmentListingStates.NotLoaded, apportionmentListing.State);
			AssertEquals(0, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);

			using (new DisposableAction(() => ErrorReporter.Clear(), () => ErrorReporter.Clear()))
			{
				Factory.Save();
				Assert(ErrorReporter.HasBeenReported("ApportionmentListing_NewJobCreatedWhenFactoryIsInSaveTransaction"));
				Assert(ErrorReporter.HasBeenReported("ApportionmentListing_FactorySavedButUnusedJobsWereNotCleanedUp"));
			}
		}

		class ApportionmentListingThatCreateJobDuringFactoryTransaction : ApportionmentListing
		{
			public ApportionmentListingThatCreateJobDuringFactoryTransaction(BusinessObjectFactory factory, IGenericJobCostPlugIn consol) : base(factory, consol)
			{
			}

			protected override void OnFactorySavingCore()
			{
				base.OnFactorySavingCore();
				var newJob = Factory.NewWithValidTestData<Job>();
				AddJobToJobsWithMutexes(newJob);
			}
		}

		public void TestUpdateOrCreateCost()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			OrgHeader creditor1 = Factory.New<OrgHeader>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			OrgHeader creditor2 = Factory.New<OrgHeader>();
			ApportionmentListing apportionmentListing = new ApportionmentListing(Factory, consol);
			JobConsolCost existingCost = apportionmentListing.CostsCollection.TryAddNew();
			existingCost.E6_OH_Creditor = creditor1.PK;
			existingCost.E6_AC_ChargeCode = chargeCode1.PK;
			existingCost.E6_LocalCostAmount = 66m;
			existingCost.E6_OSCostAmount = 66m;
			JobConsolCost existingCost2 = apportionmentListing.CostsCollection.TryAddNew();
			existingCost2.E6_OH_Creditor = creditor2.PK;
			existingCost2.E6_AC_ChargeCode = chargeCode2.PK;
			existingCost2.E6_LocalCostAmount = 88m;
			existingCost2.E6_OSCostAmount = 88m;
			existingCost2.E6_AH_APInvoice = ZGuid.NewZGuid();
			AssertEquals("precondition", 2, apportionmentListing.CostsCollection.Count);
			((IApportionmentListing)apportionmentListing).UpdateOrCreateCost(chargeCode1, creditor1, 33m);
			AssertEquals("existing cost updated - no new costs", 2, apportionmentListing.CostsCollection.Count);
			AssertEquals(33m, existingCost.E6_LocalCostAmount);
			AssertEquals(33m, existingCost.E6_OSCostAmount);
			((IApportionmentListing)apportionmentListing).UpdateOrCreateCost(chargeCode2, creditor2, 11m);
			AssertEquals("new cost added as match found but posted", 3, apportionmentListing.CostsCollection.Count);
			AssertEquals(88m, existingCost2.E6_LocalCostAmount);
			AssertEquals(88m, existingCost2.E6_OSCostAmount);
			AssertEquals(11m, apportionmentListing.CostsCollection[2].E6_LocalCostAmount);
			AssertEquals(11m, apportionmentListing.CostsCollection[2].E6_OSCostAmount);
		}

		public void TestUpdateOrCreateCost_GatewayConsol()
		{
			ForwardingConsol gatewayConsol = Factory.New<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = gatewayConsol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			Assert("Is Gateway", gatewayConsol.IsGateway());
			Factory.Save();
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			OrgHeader creditor1 = Factory.New<OrgHeader>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			OrgHeader creditor2 = Factory.New<OrgHeader>();
			ApportionmentListing apportionmentListing = new ApportionmentListing(Factory, gatewayConsol, true);
			AssertEquals("precondition", 0, apportionmentListing.CostsCollection.Count);
			((IApportionmentListing)apportionmentListing).UpdateOrCreateCost(chargeCode1, creditor1, 33m);
			AssertEquals("no new cost created", 0, apportionmentListing.CostsCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			Apps = new ApportionmentListing(Factory, consol);
			return Apps;
		}

		public void TestConsolType()
		{
			GetNewBusinessObject();
			AssertEquals("JK", Apps.ConsolType);
		}

		public void TestConsolPK()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			AssertEquals(consol.PK, apps.ConsolPK);
		}

		public void TestIsPosting()
		{
			var apps = GetNewBusinessObject() as ApportionmentListing;
			try
			{
				Assert(!apps.CostsCollectionLoaded);
				apps.IsPosting = true;
				Assert("Should not instantiate costs collection", !apps.CostsCollectionLoaded);
				apps.IsPosting = false;
				Assert("Should not instantiate costs collection", !apps.CostsCollectionLoaded);
				AssertNotNull(apps.CostsCollection);

				var cost = apps.CostsCollection.TryAddNew();
				apps.IsPosting = true;
				Assert(cost.IsPosting);
				Assert(apps.CostsCollectionLoaded);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIsActivated()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BusinessObjectFactory mutexFactory = new BusinessObjectFactory();
			// Acquire mutex in a different factory
			using (Job shipmentJob = Job.CreateWithMutex(mutexFactory, shipment))
			{
				ApportionmentListing apps = new ApportionmentListing(Factory, consol);
				Factory.Save();
				AssertEquals(0, apps.JobsWithMutexes_ForTestOnly.Count);
			}
		}

		public void TestJobsWithMutexes()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001");
			creator.CreateShipment("S001", consol);
			creator.CreateShipment("S002", consol);
			Factory.Save();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = "SHP";
				AssertEquals(2, apps.JobsWithMutexes_ForTestOnly.Count);
				var invoice = creator.CreateInvoice(typeof(APInvoice), "inv1");
				creator.CreateInvoiceLine(invoice, 100);
				invoice.SaveAsIncomplete();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consol.PK);
				apps = new ApportionmentListing(newFactory, consol);
				AssertExceptionThrown<JobCreationException>(() => apps.CostsCollection.TryAddNew());
				invoice.MoveFromIncompleteToPayableLedger();
				Factory.Save();
				newFactory = new BusinessObjectFactory();
				consol = newFactory.Load<ForwardingConsol>(consol.PK);
				apps = new ApportionmentListing(newFactory, consol);
				cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = creator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_ApportionmentMethod = "SHP";
				AssertEquals(0, apps.JobsWithMutexes_ForTestOnly.Count);
			}
			catch
			{
				apps.ReleaseMutexes();
				throw;
			}
		}

		#region TestReleaseMutexesOnUnusedJobs

		[ExpectNoExceptions]
		public void TestReleaseMutexesOnUnusedJobs()
		{
			ZGuid consolPK;
			BusinessObjectFactory factoryForConsolShipmentWithMutexJob = new BusinessObjectFactory();
			{
				ForwardingShipment shipment = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S00001985";
				ForwardingConsol consol = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);
				ForwardingShipment shipment2 = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_UniqueConsignRef = "S00001986";
				consol.Shipments.Add(shipment2);
				var testObjectCreator = new TestObjectCreator(factoryForConsolShipmentWithMutexJob);
				var apportionmentListing = new ApportionmentListingForTest(factoryForConsolShipmentWithMutexJob, consol);
				JobConsolCost cost = apportionmentListing.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
				cost.E6_OSCostAmount = 100m;
				cost.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
				factoryForConsolShipmentWithMutexJob.Save();
				consolPK = consol.PK;
			}

			ZGlobalMutex mutex = null;
			try
			{
				BusinessObjectFactory consolLoadingFactory = new BusinessObjectFactory();
				ForwardingConsol loadedConsol = consolLoadingFactory.Load<ForwardingConsol>(consolPK);
				var apportionmentListing = new ApportionmentListingForTest(consolLoadingFactory, loadedConsol);
				apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
				ForwardingShipment shipment3 = consolLoadingFactory.NewWithValidTestData<ForwardingShipment>();
				shipment3.JS_UniqueConsignRef = "S00001987";
				loadedConsol.Shipments.Add(shipment3);
				mutex = JobHeader.GetMutex_ForTestOnly(shipment3.PK);
				mutex.Lock();
				apportionmentListing.DoReleaseMutexesOnUnusedJobs();
			}
			finally
			{
				if (mutex != null && mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestReleaseMutexesOnUnusedJobs_ChecksGatewaySynchronisedConsolCostsForGatewayApportionments_ShouldNotDeleteJobs()
		{
			AssertGatewaySynchronisedConsolCostsShouldNotDeleteJob(true);
		}

		public void TestReleaseMutexesOnUnusedJobs_ChecksGatewaySynchronisedConsolCostsForStandardApportionments_ShouldNotDeleteJobs()
		{
			AssertGatewaySynchronisedConsolCostsShouldNotDeleteJob(false);
		}

		void AssertGatewaySynchronisedConsolCostsShouldNotDeleteJob(bool isGatewayApportionmentsContext)
		{
			// Arrange
			var gatewayConsol = GetGatewayConsolWithShipments();
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				var gatewayCharge = TestObjectCreator.CreateCharge(gatewayJob, debtor: GlbCompany.CurrentCompany.OrgProxy);
				gatewayCharge.SetContext(BusinessContext.AutoJobRevenueJournal);

				GatewaySellToCostSynchroniser.Synchronise(gatewayJob);

				var gatewayConsolCost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, gatewayConsol.PK));
				AssertNotNull("Expected synchronise to have created a corresponding gateway consol cost", gatewayConsolCost);
				AssertEquals("Consol Cost should be apportioned to two shipments", 0m, gatewayConsolCost.UnApportionedAmount);

				var apportionmentListing = gatewayConsol.GetApportionments(isGatewayApportionmentsContext);
				if (!isGatewayApportionmentsContext)
				{
					apportionmentListing.JobsWithMutexes_ForTestOnly.Add(gatewayConsol.Shipments[0].Job as Job);
					apportionmentListing.JobsWithMutexes_ForTestOnly.Add(gatewayConsol.Shipments[1].Job as Job);
				}

				var actualShipmentJobPKs = apportionmentListing.JobsWithMutexes_ForTestOnly.Select(x => x.PK);
				var expectedShipmentJobPKs = new[] { gatewayConsol.Shipments[0].Job.PK, gatewayConsol.Shipments[1].Job.PK };
				AssertContainsExactElementsInAnyOrder("Pre-condition", expectedShipmentJobPKs, actualShipmentJobPKs);
				AssertEquals("Pre-condition", isGatewayApportionmentsContext, apportionmentListing.IsGatewayApportionments);

				// Act
				apportionmentListing.ReleaseMutexesOnUnusedJobs();

				// Assert
				var message = "expected jobs not to be deleted as they're apportioned to gatewayConsolCost";
				actualShipmentJobPKs = apportionmentListing.JobsWithMutexes_ForTestOnly.Select(x => x.PK).ToArray();
				AssertContainsExactElementsInAnyOrder(message, expectedShipmentJobPKs, actualShipmentJobPKs);

				// Cleanup
				apportionmentListing.JobsWithMutexes_ForTestOnly[1].Delete();
				apportionmentListing.JobsWithMutexes_ForTestOnly[0].Delete();
			}
		}

		public void TestReleaseMutexesOnUnusedJobs_GatewayConsolHasInternalJobCharges_ShouldDeleteJobs()
		{
			// Arrange
			var gatewayConsol = GetGatewayConsolWithShipments();
			var gatewayJob = TestObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				var apportionmentListing = gatewayConsol.GetApportionments(true);
				apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();

				var expectedShipmentPKs = new[] { gatewayConsol.Shipments[0].PK, gatewayConsol.Shipments[1].PK };
				var actualShipmentPKs = apportionmentListing.JobsWithMutexes_ForTestOnly.Select(x => x.JH_ParentID);
				AssertContainsExactElementsInAnyOrder("Pre-condition", expectedShipmentPKs, actualShipmentPKs);

				var shipment2Job = apportionmentListing.JobsWithMutexes_ForTestOnly[1];

				var gatewayCharge = GatewaySellToCostSynchroniserTestHelper.GetValidGatewayCharge(gatewayJob, TestObjectCreator.CC1.PK, 100);
				gatewayCharge.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				gatewayCharge.JR_JH_InternalJob = shipment2Job.PK;

				Assert("Pre-condition", gatewayCharge.ShouldCreateSellJRJ);

				// Act
				apportionmentListing.ReleaseMutexesOnUnusedJobs();

				// Assert
				var message = "All newly created jobs should be deleted as they're not used.";
				var actualShipmentJobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix));
				AssertContainsExactElementsInAnyOrder(message, new[] { shipment2Job }, actualShipmentJobs);

				// Act
				gatewayConsol.GetApportionments().ReleaseMutexesOnUnusedJobs();

				// Assert
				message = "Should check ALL consol costs for charges, not just CostsCollection as we could be missing relevant charges";
				actualShipmentJobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix));
				AssertContainsExactElementsInAnyOrder(message, new[] { shipment2Job }, actualShipmentJobs);

				// Cleanup
				shipment2Job.Delete();
			}
		}

		public void TestReleaseMutexesOnUnusedJobs_GatewayConsolHasNoApplicableChargesForGatewayApportionments_ShouldDeleteJobs()
		{
			AssertHasNoApplicableChargesDeletesJobs(true);
		}

		public void TestReleaseMutexesOnUnusedJobs_GatewayConsolHasNoApplicableChargesForStandardApportionments_ShouldDeleteJobs()
		{
			AssertHasNoApplicableChargesDeletesJobs(false);
		}

		void AssertHasNoApplicableChargesDeletesJobs(bool isGateway)
		{
			// Arrange
			var gatewayConsol = GetGatewayConsolWithShipments();
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				var apportionmentListing = gatewayConsol.GetApportionments(isGateway);
				apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
				var actualJobs = apportionmentListing.JobsWithMutexes_ForTestOnly.Select(x => x.JH_ParentID);
				AssertContainsExactElementsInAnyOrder(new[] { gatewayConsol.Shipments[0].PK, gatewayConsol.Shipments[1].PK }, actualJobs);

				// Act
				apportionmentListing.ReleaseMutexesOnUnusedJobs();

				// Assert
				var message = "All newly created jobs should be deleted as they're not used.";
				AssertEquals(message, 0, apportionmentListing.JobsWithMutexes_ForTestOnly.Count);
			}
		}

		ForwardingConsol GetGatewayConsolWithShipments()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CompanyData.OB_IsDebtor = true;
			orgProxy.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertNull("Precondition : shipment1 job does not exist", new JobHeader.Loader(new BusinessObjectFactory(), shipment1).Load());
			AssertNull("Precondition : shipment2 job does not exist", new JobHeader.Loader(new BusinessObjectFactory(), shipment2).Load());

			return gatewayConsol;
		}

		public void TestReleaseMutexesOnUnusedJobs_InactiveJob()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001985");
			var shipment1 = TestObjectCreator.CreateShipment("S00001986", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001987", consol);
			var job = Job.CreateWithMutex_ForTestOnly(Factory, shipment1);
			var job2 = Job.CreateWithMutex_ForTestOnly(Factory, shipment2);
			Factory.Save();

			job.MarkAsInactive();
			Factory.Save();

			Assert(!job.JH_IsActive);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100m;
			cost.ApportionmentCharges.Cast<ApportionSplitCharge>().First(x => x.JR_JH == job.PK).JR_IsUsedForApportionment = false;

			Assert(job.JH_IsActive);

			consol.GetApportionments().ReleaseMutexesOnUnusedJobs();

			Assert(!job.JH_IsActive);
		}

		public void TestReleaseMutexesOnUnusedJobs_ConsolHasMasterShipment_ShouldDeleteSubShipmentJobs()
		{
			// Arrange
			var consol = TestObjectCreator.CreateConsol();
			var leadShipment = TestObjectCreator.CreateShipment("MASTER", consol);
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment1 = TestObjectCreator.CreateShipment("SUB1", consol);
			subShipment1.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment1.OuterPackLines.AddNew().JL_PackageCount = 10;
			var subShipment2 = TestObjectCreator.CreateShipment("SUB2", consol);
			subShipment2.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment2.OuterPackLines.AddNew().JL_PackageCount = 10;

			var apportionments = consol.GetApportionments();
			apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 240m, apportionmentListing: apportionments);

			AssertEquals("Pre-condition: creating cost creates jobs for all shipments", 3, apportionments.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("Pre-condition: but should only apportion to the master shipment", 1, cost.ApportionmentCharges.Count);

			// Act
			Factory.Save();

			// Assert
			var message = "Should only apportion to the AssemblyMaster shipment so subshipment jobs should be deleted.";
			var shipmentJobQuery = new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			var shipmentJobsInFactory = Factory.Load<JobHeader>(shipmentJobQuery).Select(x => x.Parent.JobNumber);
			AssertContainsExactElementsInAnyOrder(message, new[] { "MASTER" }, shipmentJobsInFactory);
		}

		#endregion

		public void TestLoadChildShipmentsAndAcquireMutexesWhereRequired_WhenServiceTaskHaveJobCreationException()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			var originalIsUserInteractive = Globals.IsUserInteractive;
			var originalIsWeb = Globals.IsWeb;
			var originalIsWebService = Globals.IsWebService;
			using (new DisposableAction(() =>
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
				Globals.IsWeb = originalIsWeb;
				Globals.IsWebService = originalIsWebService;
			}))
			using (new JobHeaderTestHelper(Factory).CreateJobInAnotherCW1(shipment2))
			{
				Globals.IsWeb = false;
				Globals.IsWebService = false;
				RunCases(gatewayConsol.GetApportionments());

				Globals.IsWeb = true;
				Globals.IsWebService = true;
				RunCases(gatewayConsol.GetApportionments());
			}

			return;

			void RunCases(ApportionmentListing apportionmentListing)
			{
				const string errorMsgForDefault = "User GS1 is in the process of creating the Job S2222. You cannot work on the job until he/she saves it or cancels the changes.";
				const string errorMsgForAutorate = @"Cost Autorating (either CAR, COS, CNC, NAR action) has been triggered to run on addition of an event to S2222’s consol.
The event has been added to the consol, but autorating cannot be run as S2222 is currently locked by GS1.
Autorating will need to be manually run on the consol once the GS1 closes S2222.";

				Globals.IsUserInteractive = true;
				Factory.NameForDebugging = "";
				AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(apportionmentListing,
					"Should not have issue when user run CW1, that IsUserInteractive == true",
					expectedExceptionType: typeof(JobCreationException),
					expectedErrorMsg: errorMsgForDefault,
					expectHaveIssue: false
				);

				Globals.IsUserInteractive = false;
				Factory.NameForDebugging = "Subscriber:" + "LWK Processor A";
				AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(apportionmentListing,
					"Should NOT have issue when running service task and has a valid mutex holding user.",
					expectedExceptionType: typeof(JobCreationException),
					expectedErrorMsg: errorMsgForDefault,
					expectHaveIssue: false
				);

				Globals.IsUserInteractive = true;
				Factory.NameForDebugging = "Subscriber:";
				AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(apportionmentListing,
					"Should not have issue when user run CW1, that IsUserInteractive == true",
					expectedExceptionType: typeof(JobCreationException),
					expectedErrorMsg: errorMsgForDefault,
					expectHaveIssue: false
				);

				Globals.IsUserInteractive = false;
				Factory.NameForDebugging = "";
				using (Factory.SetTempContext(BusinessContext.JobCreatedFromImporter))
				{
					AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(apportionmentListing,
						"Should just throw MessageProcessingBusinessFailureException when code was targeted by JobCreatedFromImporter context.",
						expectedExceptionType: typeof(MessageProcessingBusinessFailureException),
						expectedErrorMsg: errorMsgForDefault,
						expectHaveIssue: false
					);
				}

				Globals.IsUserInteractive = false;
				Factory.NameForDebugging = "Subscriber:";
				using (Factory.SetTempContext(BusinessContext.AutoRating))
				{
					AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(apportionmentListing,
						"When service task run in Cost Autorating context, if mutex error happen throw error message instead of error reporter",
						expectedExceptionType: typeof(JobCreationException),
						expectedErrorMsg: errorMsgForAutorate,
						expectHaveIssue: false
					);
				}
			}

			void AssertLoadChildShipmentsAndAcquireMutexesWhereRequired(ApportionmentListing apportionmentListing, string comment, Type expectedExceptionType, string expectedErrorMsg, bool expectHaveIssue)
			{
				const string issueKey = "TryLoadOrCreateJobWithMutexAndTrackingCore_1";

				CombineAssertions("PreCondition", () =>
				{
					var exp = AssertExceptionThrown<Exception>(apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired);

					AssertType(expectedExceptionType, exp);
					AssertEquals(expectedErrorMsg, exp.Message);
				});

				if (expectHaveIssue)
				{
					AssertEquals(comment, issueKey, ErrorReporter.LastKeyReported);

					var expectedMsg = $@"[MutexError]
Message:User GS1 is in the process of creating the Job S2222. You cannot work on the job until he/she saves it or cancels the changes.
ServiceTask:{Env.Instance.ServiceTaskCode}
IsWeb:{Env.Instance.IsWeb}
IsWebService:{Env.Instance.IsWebService}
Factory.NameForDebugging:{Factory.NameForDebugging}
MutexIsExistingBeforeReleasing:[Y][userCode:GS1]
Shipment Type: {typeof(ForwardingShipment).FullName}
[registry]EnableElectronicProcessingChargeFunctionality:False
ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob:False

stackTrace:   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)";
					AssertStartsWith(comment, expectedMsg, ErrorReporter.LastMessageReported);

					ErrorReporter.Clear();
				}
				else
				{
					AssertEquals(comment, false, ErrorReporter.HasBeenReported(issueKey));
				}
			}
		}

		public void TestLoadChildShipmentsAndAcquireMutexesWhereRequired_WhenServiceTaskHaveJobCreationException_WithInvalidMutexHoldingUser()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S1111", gatewayConsol);
			TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S2222", gatewayConsol);
			Factory.Save();

			var originalIsUserInteractive = Globals.IsUserInteractive;
			var originalIsWeb = Globals.IsWeb;
			var originalIsWebService = Globals.IsWebService;

			using (new DisposableAction(() => {
				Globals.IsUserInteractive = originalIsUserInteractive;
				Globals.IsWeb = originalIsWeb;
				Globals.IsWebService = originalIsWebService; }))
			using (new JobHeaderTestHelper(Factory).CreateJobInAnotherCW1(shipment2))
			using (JobHeader.SetTemporaryNullMutexHoldingUser_ForTestOnly())
			{
				Globals.IsWeb = false;
				Globals.IsWebService = false;
				AssertLoadChildShipmentsAndAcquireMutexCore(gatewayConsol.GetApportionments());

				Globals.IsWeb = true;
				Globals.IsWebService = true;
				AssertLoadChildShipmentsAndAcquireMutexCore(gatewayConsol.GetApportionments());
			}
		}

		void AssertLoadChildShipmentsAndAcquireMutexCore(ApportionmentListing apportionmentListing)
		{
			Globals.IsUserInteractive = false;
			Factory.NameForDebugging = "Subscriber:LWK Processor A";

			var ex = AssertExceptionThrown<JobCreationException>("Should report Job Mutex Error.", apportionmentListing.LoadChildShipmentsAndAcquireMutexesWhereRequired);
			AssertEquals("User (undefined) is in the process of creating the Job S2222. You cannot work on the job until he/she saves it or cancels the changes.", ex.Message);
			AssertEquals("TryLoadOrCreateJobWithMutexAndTrackingCore_1", ErrorReporter.LastKeyReported);

			var expectedMsg = $@"[MutexError]
Message:User (undefined) is in the process of creating the Job S2222. You cannot work on the job until he/she saves it or cancels the changes.
ServiceTask:{Env.Instance.ServiceTaskCode}
IsWeb:{Env.Instance.IsWeb}
IsWebService:{Env.Instance.IsWebService}
Factory.NameForDebugging:Subscriber:LWK Processor A
MutexIsExistingBeforeReleasing:[Y][userCode:GS1]
Shipment Type: {typeof(ForwardingShipment).FullName}
[registry]EnableElectronicProcessingChargeFunctionality:False
ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob:False";
			AssertEquals("Should report issue when running service task and has an invalid mutex holding user.", expectedMsg, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGetLockedChildShipmentsJobsErrorMessageWhenJobIsLocked()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001", true);
			var shipment1 = creator.CreateShipment("S001", consol);
			var shipment2 = creator.CreateShipment("S002", consol);
			Factory.Save();
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment1).Load());
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment2).Load());
			var apportionmentListing = new ApportionmentListingForTest(Factory, consol);
			using (var shipmentJob1 = new JobHeader.Loader(new BusinessObjectFactory(), shipment1).TryCreateWithMutex())
			using (var shipmentJob2 = new JobHeader.Loader(new BusinessObjectFactory(), shipment2).TryCreateWithMutex())
			{
				var lockedShipmentsJobsErrorMessage = apportionmentListing.GetLockedChildShipmentsJobsErrorMessage();
				Assert(!lockedShipmentsJobsErrorMessage.IsEmpty);
				AssertContains("You have created the job S001 on another form, but haven't saved it yet", lockedShipmentsJobsErrorMessage);
				AssertContains("You have created the job S002 on another form, but haven't saved it yet", lockedShipmentsJobsErrorMessage);
			}
		}

		public void TestGetLockedChildShipmentsJobsErrorMessageWhenJobIsNotLocked()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001", true);
			var shipment1 = creator.CreateShipment("S001", consol);
			var shipment2 = creator.CreateShipment("S002", consol);
			Factory.Save();
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment1).Load());
			AssertNull("Precondition: shipment job is not created", new JobHeader.Loader(shipment2).Load());
			var apportionmentListing = new ApportionmentListingForTest(Factory, consol);
			var lockedShipmentsJobsErrorMessage = apportionmentListing.GetLockedChildShipmentsJobsErrorMessage();
			Assert(lockedShipmentsJobsErrorMessage.IsEmpty);
			AssertNull("shipment job is not created", new JobHeader.Loader(shipment1).Load());
			AssertNull("shipment job is not created", new JobHeader.Loader(shipment2).Load());
		}

		public void TestGetLockedChildShipmentsJobsErrorMessageDoesNotUnlockExistingJobMutex()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001", true);
			var shipment = creator.CreateShipment("S001", consol);
			Factory.Save();

			var apportionmentListing = new ApportionmentListingForTest(Factory, consol);
			using (var shipmentJob = new JobHeader.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				Assert(mutex.IsLocked);
				var lockedShipmentsJobsErrorMessage = apportionmentListing.GetLockedChildShipmentsJobsErrorMessage();
				mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				Assert("Job mutex should still be locked after error message is created", mutex.IsLocked);
			}
		}

		[ExpectNoExceptions]
		public void TestDoesNotCreateUnusedJobsWhenInUniversalExportContext()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001985";
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001985";
			consol.Shipments.Add(shipment);
			var job = new Job.Loader(shipment).TryLoadOrCreateWithMutex();
			Factory.Save();
			var testObjectCreator = new TestObjectCreator(Factory);
			var apportionmentListing = new ApportionmentListingForTest(Factory, consol);
			JobConsolCost cost = apportionmentListing.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.ChargeableUnits;
			cost.E6_OSCostAmount = 100m;
			cost.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			Factory.Save();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00001986";
			consol.Shipments.Add(shipment2);
			Factory.Save();
			BusinessObjectFactory loadingFactory1 = new BusinessObjectFactory();
			ForwardingConsol loadedConsol1 = loadingFactory1.Load<ForwardingConsol>(consol.PK);
			var loadedApportionmentListing1 = new ApportionmentListingForTest(loadingFactory1, loadedConsol1);
			AssertEquals("One ConsolCost", 1, loadedApportionmentListing1.CostsCollection.Count);
			AssertEquals("Two Charges for both Shipments", 2, loadedApportionmentListing1.CostsCollection[0].ApportionmentCharges.Count);
			BusinessObjectFactory loadingFactory2 = new BusinessObjectFactory();
			loadingFactory2.SetContext(DataTransferContext.UniversalExport);
			try
			{
				ForwardingConsol loadedConsol2 = loadingFactory2.Load<ForwardingConsol>(consol.PK);
				var loadedApportionmentListing2 = new ApportionmentListingForTest(loadingFactory2, loadedConsol2);
				AssertEquals("One ConsolCost. There should not be Mutex exception", 1, loadedApportionmentListing2.CostsCollection.Count);
				AssertEquals("One Charge.  As we do not create Job and Charge for second Shipment", 1, loadedApportionmentListing2.CostsCollection[0].ApportionmentCharges.Count);
			}
			finally
			{
				loadingFactory2.RemoveContext(DataTransferContext.UniversalExport);
			}

			loadedApportionmentListing1.DoReleaseMutexesOnUnusedJobs();
		}

		public void TestDoesNotCreateUnusedJobsWhenDoNotCreateShipmentJob()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001985");
			var shipment = TestObjectCreator.CreateShipment("S00001985", consol);
			Factory.Save();

			AssertCreatingJobWhenLoadingCostsCollection(new ApportionmentListing(Factory, consol), shipment, expectedCreatingJob: true);

			var newFactory = new BusinessObjectFactory();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var apportionmentListingInNewFactory = new ApportionmentListing(newFactory, consolInNewFactory);
			using (apportionmentListingInNewFactory.DoNotCreateShipmentJob())
			{
				var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
				AssertCreatingJobWhenLoadingCostsCollection(apportionmentListingInNewFactory, shipmentInNewFactory, expectedCreatingJob: false);
			}

			void AssertCreatingJobWhenLoadingCostsCollection(ApportionmentListing apportionmentListing, ForwardingShipment assertingShipment, bool expectedCreatingJob)
			{
				AssertNull("PreCondition", assertingShipment.Job);
				AssertEquals("PreCondition, job creation logic only run once when loading CostsCollection.", false, apportionmentListing.CostsCollectionLoaded);

				AssertNotNull(apportionmentListing.CostsCollection);
				AssertEquals(true, apportionmentListing.CostsCollectionLoaded);
				if (expectedCreatingJob)
				{
					AssertNotNull(assertingShipment.Job);
				}
				else
				{
					AssertNull(assertingShipment.Job);
				}
				apportionmentListing.ReleaseMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestFactorySavedHandleMutexException()
		{
			BusinessObjectFactory factoryForConsolShipmentWithMutexJob = new BusinessObjectFactory();
			ForwardingShipment shipment = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001985";
			ForwardingConsol consol = factoryForConsolShipmentWithMutexJob.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001985";
			consol.Shipments.Add(shipment);
			factoryForConsolShipmentWithMutexJob.Save();
			//Creating shipment job with Mutex but without saving it
			using (JobHeader job = factoryForConsolShipmentWithMutexJob.NewJobWithValidTestDataForTesting<JobHeader>())
			{
				job.JH_ParentID = shipment.PK;
				job.JH_JobNum = shipment.JS_UniqueConsignRef;
				ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				try
				{
					mutex.Lock();
					BusinessObjectFactory consolLoadingFactory = new BusinessObjectFactory();
					ForwardingConsol loadedConsol = consolLoadingFactory.Load<ForwardingConsol>(consol.PK);
					ApportionmentListing apportionmentListing = new ApportionmentListing(consolLoadingFactory, loadedConsol);
					apportionmentListing.IsActivated = true;
					loadedConsol.JK_BookingReference = "S00001234";
					consolLoadingFactory.Save();
				}
				finally
				{
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}
			}
		}

		public void TestLoadChildShipmentJobsFromDBOnly_ConsolIsNull()
		{
			var apportionment = new ApportionmentListing(Factory, null);
			AssertNoExceptionThrown("Consol and Consol.CostSupporter should not be null", () =>
			{
				apportionment.LoadChildShipmentJobsFromDBOnly();
			}
			);
		}

		public void TestIApportionedChargesHeaderList_Headers()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCostCollection costs = apps.CostsCollection;
				JobConsolCost cost1 = costs.TryAddNew();
				JobConsolCost cost2 = costs.TryAddNew();
				var list = apps as IApportionedChargesHeaderList;
				AssertNotNull(list);
				AssertEquals("Headers.Count", 2, list.Headers.Length);
				AssertEquals("Headers[0]", cost1, list.Headers[0]);
				AssertEquals("Headers[1]", cost2, list.Headers[1]);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIsAllowOverrideBaseExchangeRate()
		{
			var apportionment = new ApportionmentListing(Factory, null);
			AssertEquals(true, apportionment.IsAllowOverrideBaseExchangeRate);

			apportionment.IsAllowOverrideBaseExchangeRate = false;
			AssertEquals(false, apportionment.IsAllowOverrideBaseExchangeRate);

			apportionment.IsAllowOverrideBaseExchangeRate = true;
			AssertEquals(true, apportionment.IsAllowOverrideBaseExchangeRate);
		}

		protected override void TearDown()
		{
			if (Apps != null)
			{
				Apps.ReleaseMutexes();
			}

			base.TearDown();
		}

		ApportionmentListing Apps;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
