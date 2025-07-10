using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.JobHeader.Loader;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public enum ApportionmentListingStates
	{
		NotLoaded,
		Loaded,
		Cleaned
	}
	public partial class ApportionmentListing : NonPersistentBusinessObject, IApportionmentListing, IApportionedChargesHeaderList, IObsoleteValidation, ISecurityOverrideProviderSource
	{
		public ApportionmentListing(BusinessObjectFactory factory, IGenericJobCostPlugIn consol, bool isGatewayApportionments = false, IJobCreationErrorHandler jobCreationErrorHandler = null)
			: base(factory)
		{
			this.Consol = consol;
			this.IsGatewayApportionments = isGatewayApportionments;
			this.JobsWithMutexes = new List<Job>();
			this.JobCreationErrorHandler = jobCreationErrorHandler ?? new DefaultJobCreationErrorHandler();
		}

		public ApportionmentListingStates State { get; private set; }

		readonly IGenericJobCostPlugIn Consol;
		public bool IsGatewayApportionments;

		IJobCreationErrorHandler JobCreationErrorHandler { get; }

		ZBool IsConsolValid
		{
			get
			{
				return Consol != null && Consol.CostSupporter != null;
			}
		}

		public ZString ConsolType
		{
			get
			{
				return IsConsolValid ? Consol.CostSupporter.Type : ZString.Empty;
			}
		}

		public ZGuid ConsolPK
		{
			get
			{
				return IsConsolValid ? Consol.CostSupporter.PK : ZGuid.Empty;
			}
		}

		public bool IsPosting
		{
			get { return fIsPosting; }
			set
			{
				fIsPosting = value;
				if (fCostsCollection != null)
				{
					fCostsCollection.IsPosting = value;
				}
			}
		}
		bool fIsPosting;

		public bool IsActivated { get; set; }

		public bool IsAllowOverrideBaseExchangeRate { get; set; } = true;

		public void PrepareForConsolCosting()
		{
			IsActivated = true;
			if (ConsolJob != null && !ConsolJob.IsDeleted && ConsolJob.IsGatewayBillingJob() && IsGatewayApportionments)
			{
				GatewaySellToCostSynchroniser.Synchronise(ConsolJob);
			}
			else
			{
				LoadChildShipmentsAndAcquireMutexesWhereRequired();
			}
			CostsCollection.UpdateAllConsolCostApportionmentCharges();
		}

		Job ConsolJob
		{
			get
			{
				if (consolJob == null)
				{
					var query = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(JobHeaderSchema.JH_ParentID, Consol.CostSupporter.PK);
					consolJob = Consol.Factory.LoadTop1<Job>(query);
				}
				return consolJob;
			}
		}
		Job consolJob;

		bool FactoryWithValidContext
		{
			get { return !Factory.HasContext(BusinessContext.IncompleteInvoiceSaving) && !Factory.HasContext(BusinessContext.PreviewInvoice); }
		}

		protected sealed override void OnFactorySavingBeforeTransactionCore()
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySavingBeforeTransactionCore();
				if (!IsPosting)
				{
					ReleaseMutexesOnUnusedJobs();
				}

				OnFactorySavingBeforeTransactionCore2();
			}
		}

		protected virtual void OnFactorySavingBeforeTransactionCore2()
		{
		}

		internal protected virtual void ReleaseMutexesOnUnusedJobs()
		{
			State = ApportionmentListingStates.Cleaned;

			try
			{
				isReleasingMutexesOnUnusedJobs = true;
				var jobsLinkedToConsol = GetJobsLinkedToConsolChargesAndCosts();
				var unusedJobsWithMutexes = new List<Job>();
				foreach (var newJob in JobsWithMutexes)
				{
					var canDeactivateNewJob = newJob.IsJobActivating
						|| Factory.HasContext(BusinessContext.JobIsManuallyDeactivating)
						|| (!newJob.IsInDatabase && !newJob.IsDeleted && !newJob.IsManuallyCreated);

					if (canDeactivateNewJob && !jobsLinkedToConsol.Contains(newJob.PK))
					{
						unusedJobsWithMutexes.Add(newJob);
					}
				}

				int count = unusedJobsWithMutexes.Count;
				for (int index = count - 1; index >= 0; index--)
				{
					Job unusedJob = unusedJobsWithMutexes[index];
					unusedJobsWithMutexes.Remove(unusedJob);
					JobsWithMutexes.Remove(unusedJob);

					if (unusedJob.IsInDatabase)
					{
						unusedJob.MarkAsInactive();
					}
					else
					{
						unusedJob.Dispose();
						unusedJob.Delete();
					}
				}
			}
			finally
			{
				isReleasingMutexesOnUnusedJobs = false;
			}
		}

		internal HashSet<ZGuid> GetJobsLinkedToConsolChargesAndCosts()
		{
			var jobsWithLinkedChargesPKs = new HashSet<ZGuid>();

			if (ConsolJob != null && !ConsolJob.IsDeleted && ConsolJob.IsGatewayBillingJob())
			{
				foreach (Charge charge in ConsolJob.Charges)
				{
					if (!charge.IsDeleted && charge.ShouldCreateJRJ)
					{
						jobsWithLinkedChargesPKs.Add(charge.JR_JH_InternalJob);
					}
				}
			}

			//Cannot use CostCollection as we have two separate instances for Gateway
			// but when deciding to delete jobs we need to check everything.
			var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, ConsolPK);
			consolCostQuery.FetchOnlyFromLocalCache = true;
			foreach (var cost in Factory.Load<JobConsolCost>(consolCostQuery))
			{
				foreach (var apportionedCharge in cost.ApportionmentCharges.Cast<ApportionSplitCharge>())
				{
					if (!apportionedCharge.IsDeleted && !apportionedCharge.JR_OSCostAmt.IsEmpty)
					{
						jobsWithLinkedChargesPKs.Add(apportionedCharge.JR_JH);
					}
				}
			}

			//Apportionment may create jobs that should be linked internally.
			// Once JH_JH_ParentJob is set, deleting it will violiate a db constraint.
			var internalJobQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, JobsWithMutexes.Select(x => x.PK));
			internalJobQuery.FetchOnlyFromLocalCache = true;
			foreach (var internalJobHeader in Factory.Load<JobHeader>(internalJobQuery))
			{
				jobsWithLinkedChargesPKs.Add(internalJobHeader.JH_JH_ParentJob);
			}

			return jobsWithLinkedChargesPKs;
		}

		protected sealed override void OnFactorySaving()
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaving();

				OnFactorySavingCore();
			}
		}

		protected virtual void OnFactorySavingCore()
		{
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaved(saveSucceeded);

				try
				{
					if (saveSucceeded)
					{
						if (State == ApportionmentListingStates.Loaded)
						{
							ErrorReporter.ReportOnce("ApportionmentListing_FactorySavedButUnusedJobsWereNotCleanedUp", "Factory was saved successfully, but unused job clean up code did not run. Unused jobs might have been saved to database.");
						}

						ReleaseMutexes();
						if (!IsPosting && IsActivated)
						{
							SuspendSetJobHasChanges = true;
							LoadChildShipmentsAndAcquireMutexesWhereRequired();
							CostsCollection.UpdateAllConsolCostApportionmentCharges();
						}
					}
				}
				catch (JobCreationException)
				{
				}
				finally
				{
					SuspendSetJobHasChanges = false;
				}

				OnFactorySavedCore(saveSucceeded);
			}
		}

		bool SuspendSetJobHasChanges;

		protected virtual void OnFactorySavedCore(bool saveSucceeded)
		{
		}

		bool isReleasingMutexesOnUnusedJobs;

		public JobConsolCostCollection CostsCollection
		{
			get
			{
				if (fCostsCollection == null)
				{
					var onlyLoadExistingJobs = IsPosting ||
						Factory.HasContext(DataTransferContext.UniversalExport) ||
						Factory.HasContext(BusinessContext.WarningOnlyValidation) ||
						!shouldCreateShipmentJob;

					if (!onlyLoadExistingJobs && !IsActivated && !isReleasingMutexesOnUnusedJobs)
					{
						LoadChildShipmentsAndAcquireMutexesWhereRequired();
					}
					else if (onlyLoadExistingJobs)
					{
						LoadChildShipmentJobsFromDBOnly();
					}
					fCostsCollection = new JobConsolCostCollection(Factory, Consol, IsGatewayApportionments);
					fCostsCollection.Load();
					fCostsCollection.IsPosting = IsPosting;
					fCostsCollection.IsManagedForDataRefresh = true;
					fCostsCollection.OnReplaceShipmentExchangeRate += new EventHandler<ReplaceShipmentExchangeRateEventArgs>(RaiseOnReplaceShipmentExchangeRate);
					RegisterEditableChildObject(fCostsCollection);
				}

				return fCostsCollection;
			}
		}

		internal JobConsolCostCollection fCostsCollection;

		internal IDisposable DoNotCreateShipmentJob()
		{
			var createShipmentJob_current = shouldCreateShipmentJob;
			shouldCreateShipmentJob = false;
			return new DisposableAction(() => shouldCreateShipmentJob = createShipmentJob_current);
		}

		bool shouldCreateShipmentJob = true;

		public JobConsolCostFilteredCollection CostsFilteredCollection
		{
			get
			{
				if (fCostsFilteredCollection == null)
				{
					fCostsFilteredCollection = new JobConsolCostFilteredCollection(CostsCollection);
				}
				return fCostsFilteredCollection;
			}
		}

		JobConsolCostFilteredCollection fCostsFilteredCollection;

		public bool CostsCollectionLoaded
		{
			get { return fCostsCollection != null; }
		}

		public void LoadChildShipmentJobsFromDBOnly()
		{
			if (IsConsolValid)
			{
				foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
				{
					var loader = new Job.Loader(Factory, shipment);

					var job = loader.Load();
					if (job != null)
					{
						using (job.SuspendSettingHasChanges())
						{
							if (job.IsInDatabase)
							{
								using (job.GetSetJobDefaultsSuspender())
								{
									job.PlugInData = shipment;
								}
							}
							else
							{
								job.PlugInData = shipment;
							}

							if (job.JH_GE.IsEmpty)
							{
								job.JH_GE = GlbDepartment.CurrentDepartment.PK;
							}

							if (job.JH_GB.IsEmpty)
							{
								job.JH_GB = GlbBranch.CurrentBranch.PK;
							}
						}
					}
				}
			}
		}

		public ZString GetLockedChildShipmentsJobsErrorMessage()
		{
			var errorMessage = new StringBuilder();
			if (IsConsolValid)
			{
				foreach (var shipment in Consol.CostSupporter.ShipmentsList)
				{
					var existingJob = new Job.Loader(Factory, shipment).Load();
					if (existingJob == null)
					{
						var tempLoader = new Job.Loader(new BusinessObjectFactory(), shipment);
						using (var newJob = tempLoader.TryCreateWithMutex())
						{
							if (newJob == null)
							{
								errorMessage.AppendLine(tempLoader.GetJobCreationError());
							}
						}
					}
				}
			}
			return errorMessage.ToString();
		}

		public Job TryLoadOrCreateJobWithMutexAndTracking(IJobInvoicingPlugIn jobHeaderParent)
		{
			try
			{
				return TryLoadOrCreateJobWithMutexAndTrackingCore(jobHeaderParent);
			}
			catch (JobCreationException e)
			{
				if (OnJobCreationError != null)
				{
					OnJobCreationError?.Invoke(this, new MutexErrorEventArgs(e.Message));
					return null;
				}

				throw;
			}
		}

		public event EventHandler<MutexErrorEventArgs> OnJobCreationError;

		public void LoadChildShipmentsAndAcquireMutexesWhereRequired()
		{
			foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter?.ShipmentsList ?? Enumerable.Empty<IJobInvoicingPlugIn>())
			{
				TryLoadOrCreateJobWithMutexAndTrackingCore(shipment);
			}
		}

		Job TryLoadOrCreateJobWithMutexAndTrackingCore(IJobInvoicingPlugIn shipment)
		{
			var loader = new Job.Loader(Factory, shipment);
			var wasJobCreatedByApportionmentListing = false;

			var job = loader.Load();
			if (job != null && job.Parent == null)
			{
				job.Parent = shipment;
			}

			if (job == null)
			{
				using (SuspendSetJobHasChanges ? loader.SetJobHasChangesSuspender.GetSuspender() : null)
				{
					job = loader.TryCreateWithMutex();
				}
				wasJobCreatedByApportionmentListing = true;
			}

			if (job == null)
			{
				LockInfo lockInfoBeforeReleasing;
				using (var tempMutex = JobHeader.GetCreateOrActivateJobMutex(shipment.PK))
				{
					lockInfoBeforeReleasing = tempMutex.GetLockInfo();
				}

				JobCreationErrorHandler.HandleJobCreationError(this);

				var error = loader.GetJobCreationError();

				if (!Globals.IsUserInteractive)
				{
					if (Factory.HasContext(BusinessContext.AutoRating))
					{
						var costAutoratingErrorMessage = Res.GetString("4CA79D75-4CF3-4E14-891E-0F8297ED6607", "Cost Autorating (either CAR, COS, CNC, NAR action) has been triggered to run on addition of an event to {0}’s consol.\r\nThe event has been added to the consol, but autorating cannot be run as {0} is currently locked by {1}.\r\nAutorating will need to be manually run on the consol once the {1} closes {0}.", shipment.JobNumber, lockInfoBeforeReleasing?.UserWithLock?.GS_LoginName);

						throw new JobCreationException(costAutoratingErrorMessage);
					}
					else if (Factory.HasContext(BusinessContext.JobCreatedFromImporter))
					{
						throw new MessageProcessingBusinessFailureException(error.Message, Enum.GetName(typeof(JobCreationErrorType), error.Type), true);
					}
					else if (!error.IsValidMutexError)
					{
						var issueMsg = new ZStringBuilder($@"[{error.Type}]
Message:{error.Message}
ServiceTask:{Env.Instance.ServiceTaskCode}
IsWeb:{Env.Instance.IsWeb}
IsWebService:{Env.Instance.IsWebService}
Factory.NameForDebugging:{Factory.NameForDebugging}
MutexIsExistingBeforeReleasing:[{(lockInfoBeforeReleasing == null ? "N" : "Y")}][userCode:{lockInfoBeforeReleasing?.UserWithLock?.GS_Code}]
Shipment Type: {shipment.GetType().FullName}
[registry]EnableElectronicProcessingChargeFunctionality:{AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value}
ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob:{AccountingConfigurationRegistry.Instance.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment)}");

						ErrorReporter.ReportOnce("TryLoadOrCreateJobWithMutexAndTrackingCore_1", issueMsg.ToString());
					}
				}

				throw new JobCreationException(error);
			}
			else if (wasJobCreatedByApportionmentListing)
			{
				AddJobToJobsWithMutexes(job);
			}

			using (job.SuspendSettingHasChanges())
			{
				if (job.JH_GE.IsEmpty)
				{
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				}

				if (job.JH_GB.IsEmpty)
				{
					job.JH_GB = GlbBranch.CurrentBranch.PK;
				}
			}

			return job;
		}

		protected void AddJobToJobsWithMutexes(Job job)
		{
			if (!JobsWithMutexes.Contains(job))
			{
				JobsWithMutexes.Add(job);
				State = ApportionmentListingStates.Loaded;
				if (job.Factory.IsInSaveTransaction)
				{
					ErrorReporter.ReportOnce("ApportionmentListing_NewJobCreatedWhenFactoryIsInSaveTransaction", "Should not create new jobs when factory is in save transaction. Unused jobs will not get cleaned up because clean up is done in OnFactorySavingBeforeTransaction.");
				}
			}
		}

		List<Job> JobsWithMutexes { get; }

		public void DeleteAllCreatedJobs()
		{
			foreach (Job job in JobsWithMutexes)
			{
				if (!job.IsInDatabase)
				{
					job.DisposeAndDeleteNew();
				}
			}
		}

		public void ReleaseMutexes()
		{
			foreach (Job job in JobsWithMutexes)
			{
				job.Dispose();
			}
		}

		#region OnReplaceShipmentExchangeRate

		public void RaiseOnReplaceShipmentExchangeRate(object sender, ReplaceShipmentExchangeRateEventArgs args)
		{
			if (OnReplaceShipmentExchangeRate != null)
			{
				OnReplaceShipmentExchangeRate(sender, args);
			}
		}

		public bool IsReplaceShipmentExchangeRateHooked
		{
			get { return OnReplaceShipmentExchangeRate != null; }
		}

		public event EventHandler<ReplaceShipmentExchangeRateEventArgs> OnReplaceShipmentExchangeRate;

		#endregion

		#region IApportionmentListing Members

		void IApportionmentListing.UpdateOrCreateCost(AccChargeCode chargeCode, OrgHeader creditor, decimal amount)
		{
			CostsCollection.Load();

			if (chargeCode != null && creditor != null)
			{
				ZQuery consolCostQuery = new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, chargeCode.PK);
				consolCostQuery.AddToFilter(JobConsolCostSchema.E6_OH_Creditor, creditor.PK);
				consolCostQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);
				JobConsolCost[] costs = (JobConsolCost[])CostsCollection.Find(consolCostQuery);

				JobConsolCost cost = null;
				if (costs.Length > 0)
				{
					cost = costs[0];
				}
				else
				{
					cost = CostsCollection.TryAddNew();
					if (cost != null)
					{
						cost.E6_AC_ChargeCode = chargeCode.PK;
						cost.E6_OH_Creditor = creditor.PK;
					}
				}

				if (cost != null)
				{
					cost.E6_LocalCostAmount = amount;
					cost.E6_OSCostAmount = amount;
				}
			}
		}

		#endregion

		#region IApportionedChargesHeaderList members

		IApportionedChargesHeader[] IApportionedChargesHeaderList.Headers
		{
			get { return CostsCollection.Cast<IApportionedChargesHeader>().ToArray(); }
		}

		#endregion

		#region ISecurityOverrideProviderSource Members

		ISecurityOverrideProvider _provider;
		ISecurityOverrideProvider ISecurityOverrideProviderSource.Provider
		{
			get
			{
				if (_provider == null)
				{
					_provider = new DefaultAccessSecurityProvider();
				}
				return _provider;
			}
			set
			{
				_provider = value;
			}
		}

		#endregion
	}
}
