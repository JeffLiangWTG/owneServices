using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using TaxDateDefaultingOption = Enterprise.MasterFiles.Business.TaxDateDefaultingOption;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	[TestExcludeWorkflowProviderHasTestCase]
	[DisableWorkflowSettingPropertiesAfterOnSaving]
	[CodeProperty(JobHeaderSchema.Constants.JH_JobNum), DescriptionProperty(JobHeaderSchema.Constants.JH_JobNum)]
	[ProvideMetaDataProperty("PropertiesReadOnlyState", MetaDataTypes.ReadOnly)]
	[RestrictedFilteredItem]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class Job :
		JobHeader,
		IDocManagerSupport,
		IAutoRatingAccountingInfo,
		IAutoRatingAccountingUtils,
		ITotalProvider,
		ILandedCostChargeHolder,
		ILandedCostExchangeRateProvider,
		IPostingJob,
		IPODCharge,
		ISecurityOverrideProviderSource,
		IJobWithRevenueTotal,
		IExchangeRateSource,
		IExchangeRateProvider,
		ISupportCriticalValidation,
		IAdditionalPropertyValuesProvider,
		IScreeningPartiesProvider,
		IEDocsParsingSupport
	{
		#region Events

		public event CancelEventHandler SpotQuoteChargesExist;

		public event EventHandler<UserQueryEventArgs> ShouldUseImmediateRevenueRecognisedDate;

		public event ErrorMessageHandler OnCloseJobError;

		public event EventHandler<UserQueryEventArgs> OnCloseJobYesNoQuestion;

		public event EventHandler<UserMessageEventArgs> OnCannotChangeStatusUserMessage = delegate { };

		public delegate void ErrorMessageHandler(Job job, string errorMessage);

		#region ShouldUseImmediateRevenueRecognisedDate Helpers

		public void ResetPreviouseRespose_ShouldUseImmediateRevenueRecognisedDate()
		{
			ShouldUseImmediateRevenueRecognisedDate_SilentRespose = null;
		}

		bool? ShouldUseImmediateRevenueRecognisedDate_SilentRespose
		{
			get;
			set;
		}

		public ZDateTime AskShouldUseImmediateRevenueRecognisedDate(AccChargeCode chargeCode)
		{
			return AskShouldUseImmediateRevenueRecognisedDate(chargeCode, null);
		}

		internal ZDateTime AskShouldUseImmediateRevenueRecognisedDate(AccChargeCode chargeCode, string revenueRecognitionOptionOnlyAccepted = null, bool useStubIfRecognitionOptionIsNotExist = false)
		{
			ZDateTime result = CalculateRevenueRecognitionDate(useStubIfRecognitionOptionIsNotExist ?
				GetRevenueRecognitionOption_CreateStubIfRecognitionOptionIsNotExist(chargeCode, revenueRecognitionOptionOnlyAccepted) :
				GetRevenueRecognitionOption(chargeCode, revenueRecognitionOptionOnlyAccepted));
			if (GetRevenueRecognitionDate(revenueRecognitionOptionOnlyAccepted ?? GetRevenueRecognitionType(chargeCode)).IsEmpty)
			{
				result = AskShouldUseImmediateRevenueRecognisedDate(ReturnCurrentDateWhenRegistryOn(result));
			}

			return result;
		}

		ZDateTime AskShouldUseImmediateRevenueRecognisedDate(ZDateTime revenueRecognitionDate)
		{
			ZDateTime result = revenueRecognitionDate;
			bool? shouldUseImmediateDate = null;
			string message = ((JobValidation)Validation).GetRevenueRecognitionDatePriorToExistingGLPeriodError(revenueRecognitionDate);

			if (!string.IsNullOrEmpty(message) &&
				(ShouldUseImmediateRevenueRecognisedDate != null || ShouldUseImmediateRevenueRecognisedDate_SilentRespose != null))
			{
				if (ShouldUseImmediateRevenueRecognisedDate_SilentRespose == null)
				{
					UserQueryEventArgs eventArgs = new UserQueryEventArgs(message, false);
					ShouldUseImmediateRevenueRecognisedDate(this, eventArgs);
					shouldUseImmediateDate = eventArgs.Response;
					ShouldUseImmediateRevenueRecognisedDate_SilentRespose = shouldUseImmediateDate;
				}
				else
				{
					shouldUseImmediateDate = ShouldUseImmediateRevenueRecognisedDate_SilentRespose.Value;
				}
			}

			if (shouldUseImmediateDate != null)
			{
				result = shouldUseImmediateDate.Value ? RevenueRecognitionDateConstants.Immediate : ZDateTime.Empty;
			}

			return result;
		}

		#endregion

		bool QuerySpotQuoteChargesExist()
		{
			if (SpotQuoteChargesExist != null)
			{
				CancelEventArgs args = new CancelEventArgs();
				SpotQuoteChargesExist(this, args);
				return !args.Cancel;
			}
			return true;
		}

		void CopyCompanyTariffLevelOverrideFromQuote(IJobHeaderParent jobHeaderParent)
		{
			if (jobHeaderParent is Quote quote)
			{
				if (Parent is QuotedBooking quotedBooking)
				{
					quotedBooking.CompanyTariffLevel = quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride.ToString();
				}
				else
				{
					if (CommonShipment != null)
					{
						CommonShipment.JS_CompanyTariffLevelOverride = quote.CurrentOneOffQuote.TT_CompanyTariffLevelOverride;
					}
				}
			}
		}

		#endregion

		#region Schema

		public new abstract class Schema : JobHeader.Schema
		{
			public const string JH_LocalCurrency = "JH_LocalCurrency";
			public const string JH_LocalCurrencyDecimals = "JH_LocalCurrencyDecimals";
			public const string JH_TotalRevenue = "JH_TotalRevenue";
			public const string JH_TotalCost = "JH_TotalCost";
			public const string JH_ProfitLoss = "JH_ProfitLoss";
			public const string JH_TotalCFX = "JH_TotalCFX";
			public const string JH_TotalWIP = "JH_TotalWIP";
			public const string JH_HouseBillNo = "JH_HouseBillNo";
			public const string JH_ConsolNo = "JH_ConsolNo";
			public const string JH_MasterBillNo = "JH_MasterBillNo";
			public const string JH_CoLoadMasterBill = "JH_CoLoadMasterBill";
			public const string JH_OSAmountForPeriodicBilling = "JH_OSAmountForPeriodicBilling";
			public const string JH_OSTaxAmountForPeriodicBilling = "JH_OSTaxAmountForPeriodicBilling";
			public const string JH_LocalAmountForPeriodicBilling = "JH_LocalAmountForPeriodicBilling";
			public const string JH_LocalTaxAmountForPeriodicBilling = "JH_LocalTaxAmountForPeriodicBilling";
			public const string JH_OSExtraTaxAmount = "JH_OSExtraTaxAmount";
			public const string JH_LocalExtraTaxAmount = "JH_LocalExtraTaxAmount";
			public const string JH_IsChargeCostReferenceFilterEnabled = "JH_IsChargeCostReferenceFilterEnabled";
			public const string JobDescription = "JobDescription";
			public const string IsJobDescriptionOverriden = "IsJobDescriptionOverriden";
			public const string JH_JS_OrderReferences = "JH_JS_OrderReferences";
			public const string AdditionalReferenceAsString = "AdditionalReferenceAsString";
			public const string JH_JS_JK_VoyageFlight = "JH_JS_JK_VoyageFlight";
			public const string JH_JS_JK_Vessel = "JH_JS_JK_Vessel";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message builder")]
		public Job(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
			if (!IsInDatabase && BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(factory))
			{
				var message = "UniversalCopy tries to create jobs via reflection using the default constructor which can cause many issues. " +
					"Please use the UniversalCopyIgnoreElement on the calling BusinessObject to exclude the Job property from being copied.";

				var moduleName = factory.ServiceContainer.GetService<IScheduleTaskModuleService>()?.ScheduleTaskModuleName ?? ZString.Empty;
				if (moduleName != ZString.Empty)
				{
					message += string.Format((NoResString)"\n\nDetail:\n Task Module Name: {0}", moduleName);
				}

				ErrorReporter.ReportOnce("DoNotCopyJobsWithUniversalCopy", message);
			}
			else if (!IsInDatabase && !factory.IsLoading && !factory.HasContext(BusinessContext.JobCreatedFromJobLoader) && !factory.IsConstructingNullBusinessObject)
			{
				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine("");
				errorMessageBuilder.AppendLine($"{nameof(factory.NameForDebugging)}={factory.NameForDebugging}");
				errorMessageBuilder.AppendLine($"{nameof(factory._Instance)}={factory._Instance}");
				errorMessageBuilder.AppendLine($"{nameof(factory.InstantiationTime)}={factory.InstantiationTime}");
				errorMessageBuilder.AppendLine($"{nameof(factory.IsOwnedByCurrentThread)}={factory.IsOwnedByCurrentThread.ToYesNoString()}");
				errorMessageBuilder.AppendLine("");

				ErrorReporter.ReportOnce("JobMustBeCreatedUsingJobLoader",
$@"Job need to be created using the Job.Loader (e.g. var job = new JobHeader.Loader(shipment).TryCreateWithMutex()),
Factory Details:
{errorMessageBuilder}");
			}
		}

		public void SetupDependency(IClosedJobReopener closedJobReopener)
		{
			ClosedJobReopener = closedJobReopener;
		}

		public IClosedJobReopener ClosedJobReopener { get; private set; }

		#region Loader

		public new class Loader : JobHeader.Loader
		{
			public Loader(IJobHeaderParent parent)
				: base(parent)
			{
			}

			public Loader(BusinessObjectFactory factory, IJobHeaderParent parent)
				: base(factory, parent)
			{
			}

			public new Job TryCreate() { return (Job)base.TryCreate(); }
			public new Job TryCreate(GlbBranch branch) { return (Job)base.TryCreate(branch); }
			public new Job TryCreateWithMutex(bool fallBackToCurrentBranch = true) { return (Job)base.TryCreateWithMutex(fallBackToCurrentBranch); }
			public new Job TryCreateWithMutex(GlbBranch branch) { return (Job)base.TryCreateWithMutex(branch); }
			public new Job Load() { return (Job)base.Load(); }
			public new Job Load(bool setParent, bool setJobDefaults = true) { return (Job)base.Load(setParent, setJobDefaults); }
			public new Job Load(bool setParent, GlbCompany company, bool setJobDefaults = true) { return (Job)base.Load(setParent, company, setJobDefaults); }
			public new Job TryLoadOrCreate() { return (Job)base.TryLoadOrCreate(); }
			public new Job TryLoadOrCreate(GlbBranch branch) { return (Job)base.TryLoadOrCreate(branch); }
			public new Job TryLoadOrCreateWithMutex() { return (Job)base.TryLoadOrCreateWithMutex(); }
			public new Job TryLoadOrCreateWithMutex(GlbBranch branch) { return (Job)base.TryLoadOrCreateWithMutex(branch); }
		}

#if DEBUG
		public static Job CreateWithMutex_ForTestOnly(BusinessObjectFactory factory, IJobHeaderParent parent)
		{
			return CreateWithMutex(factory, parent);
		}
#endif

		internal static Job CreateWithMutex(BusinessObjectFactory factory, IJobHeaderParent parent)
		{
			return new Loader(factory, parent).TryCreateWithMutex();
		}

		#endregion

		#region Pre-Fetch

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobFetchStrategy(this);
		}

		#endregion

		#region Actions

		public void Invoice()
		{
			JH_Status = JobHeaderStatus.JobInvoiced.Code;
		}

		public void ReOpen()
		{
			JH_Status = JobHeaderStatus.Working.Code;
		}

		ZString reopenLogInfo;

		public void ReOpenByImport(string jobReopenLogText)
		{
			ReOpen();
			if (IsReopened)
			{
				reopenLogInfo = jobReopenLogText;
			}
		}

		public void Close(ErrorMessageHandler job_OnCloseJobError, EventHandler<UserQueryEventArgs> job_OnCloseJobYesNoQuestion)
		{
			OnCloseJobError += job_OnCloseJobError;
			OnCloseJobYesNoQuestion += job_OnCloseJobYesNoQuestion;
			try
			{
#if DEBUG
				if (Globals.IsTest && IsForceToMakeCloseFail_ForTestOnly)
				{
					OnCloseJobError?.Invoke(this, "for test only fail message");
					return;
				}
#endif
				JH_Status = JobHeaderStatus.Closed.Code;
			}
			finally
			{
				OnCloseJobError -= job_OnCloseJobError;
				OnCloseJobYesNoQuestion -= job_OnCloseJobYesNoQuestion;
			}
		}

#if DEBUG
		[ThreadStatic]
		public static bool IsForceToMakeCloseFail_ForTestOnly;
#endif

		#region Clear AR Links

		public void ClearARLinks(IEnumerable<InvoicingBase> aRInvoicesAndCreditNotes)
		{
			foreach (Charge charge in Charges)
			{
				foreach (InvoicingBase invoiceOrCreditNote in aRInvoicesAndCreditNotes)
				{
					if (invoiceOrCreditNote.Lines.Contains(charge.JR_AL_ARLine))
					{
						charge.ClearRevenueLink();
						charge.ClearRevenueAmount();
					}
				}
			}
		}

		#endregion

		#region Are Transactions Created

		public bool AreTransactionsCreated()
		{
			bool result = false;
			foreach (Charge charge in Charges)
			{
				if (charge.ARLine != null || charge.APLine != null)
				{
					if (JH_IsDisbursement)
					{
						var electronicProcessingChargeCode = Factory.Load<AccChargeCode>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
						if (electronicProcessingChargeCode.ChildChargeCodes.Any(x => x.PK == charge.JR_AC))
						{
							continue;
						}
					}

					result = true;
					break;
				}
			}
			return result;
		}

		#endregion

		#region Clear Unposted Charges

		public void ClearUnpostedChargesAmounts()
		{
			foreach (Charge charge in Charges)
			{
				if (!charge.JR_IsAPCashAdvance)
				{
					if (!charge.JR_IsApportioned && !charge.IsCostPosted)
					{
						using (GetCalculationSuspender(charge))
						{
							charge.JR_OSCostAmt = 0;
							charge.JR_LocalCostAmt = 0;
							charge.JR_AgentDeclaredCostAmt = 0;
							charge.JR_AgentDeclaredCostAmtLocal = 0;
							charge.JR_CostRated = false;
						}
					}

					charge.JR_CostRatingOverride = false;
					charge.JR_CostRatingOverrideComment = ZString.Empty;
				}

				if (!charge.JR_IsARCashAdvance)
				{
					if (!charge.IsRevenuePosted)
					{
						using (GetCalculationSuspender(charge))
						{
							charge.JR_OSSellAmt = 0;
							charge.JR_LocalSellAmt = 0;
							charge.JR_AgentDeclaredSellAmt = 0;
							charge.JR_AgentDeclaredSellAmtLocal = 0;
							charge.JR_SellRated = false;
						}
					}

					charge.JR_SellRatingOverride = false;
					charge.JR_SellRatingOverrideComment = ZString.Empty;
				}
			}

			IDisposable GetCalculationSuspender(Charge c) => c.IsDisbursementCharge && (c.JR_IsAPCashAdvance || c.JR_IsARCashAdvance) ? c.Calculations.SuspendCalculations() : DisposableAction.NoAction;
		}

		#endregion

		#region Create WIPs and Accruals

		public void CreateWIPsForPostedCost()
		{
			Charge[] initialChargesAsArray = Charges.ToArray<Charge>();
			foreach (Charge charge in initialChargesAsArray)
			{
				if (!charge.HasARPaidOrInvoicedCashAdvanceRequestLine && (charge.JR_IsApportioned || charge.IsCostPosted) && !charge.IsRevenuePosted)
				{
					charge.JR_RX_NKSellCurrency = charge.JR_RX_NKCostCurrency;
					charge.JR_OSSellAmt = charge.GetRevenueAmountBasedOnCost();
					if (charge.IsDisbursementCharge)
					{
						charge.JR_LocalSellAmt = charge.JR_LocalCostAmt;
					}
				}
			}

			CriticalValidationHelpers.ReportArrayChanges(Charges.ToArray<Charge>(), initialChargesAsArray, "CreateWIPsForPostedCost",
				CriticalValidationHelpers.ArrayChangedWarningMessage, false, (charge) => charge.GetJobChargeInfo());
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Charges

		[ChildEditable(true)]
		public ChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ChargeCollection(this);
					RegisterEditableChildObject(fCharges);
					fCharges.DisplaySequenceChanged += Charges_OnDisplaySequenceChanged;
					fCharges.CountChanged += ChargeCollection_CountChanged;
				}

				LoadCharges();

				return fCharges;
			}
		}
		ChargeCollection fCharges;
		bool isChargesCollectionLoaded;

		protected override bool IsChargesCollectionLoaded
		{
			get { return isChargesCollectionLoaded; }
		}

		/// <summary>
		/// Charges in other companies for the same job parent that have a JR_OH_CostAccount that matches the org proxy of the current company or its branches.
		/// </summary>
		internal GroupCompanyChargesHelper GroupCompanyChargesForCreditor
			=> groupCompanyChargesForCreditor ?? (groupCompanyChargesForCreditor = GroupCompanyChargesHelper.Create(this, true));
		GroupCompanyChargesHelper groupCompanyChargesForCreditor;

		/// <summary>
		/// Charges in other companies for the same job parent that have a JR_OH_SellAccount that matches the org proxy of the current company or its branches.
		/// </summary>
		public GroupCompanyChargesHelper GroupCompanyChargesForDebtor
			=> groupCompanyChargesForDebtor ?? (groupCompanyChargesForDebtor = GroupCompanyChargesHelper.Create(this, false));
		GroupCompanyChargesHelper groupCompanyChargesForDebtor;

#if DEBUG
		public FunctionalitySuspender ChargesLoadSuspender_ExposedForTestOnly => ChargesLoadSuspender;
#endif

		internal FunctionalitySuspender ChargesLoadSuspender
		{
			get { return chargesLoadSuspender ?? (chargesLoadSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender chargesLoadSuspender;

		void LoadCharges()
		{
			if (fCharges == null)
			{
				return;
			}

			if (!ChargesLoadSuspender.IsSuspended)
			{
				// initialize PlugInData so additional jobs below can be included!
				if (PlugInData == null)
				{
					InitializeParentFromGenericJobWithoutSettingDefaults();
				}

				var additionalJobsPlugInData = PlugInData as IJobInvoicingPlugInAdditionalJobs;
				if (additionalJobsPlugInData != null && !this.IsDeleted)
				{
					if (shouldReloadAdditionalJobs)
					{
						if (fCharges.IncludeChargesFromJobs(AdditionalJobsToShowChargesFor.ToArray()))
						{
							isChargesCollectionLoaded = false;
						}

						ToggleFlagForReLoadingAdditionalJobs(false);
					}
				}

				if (!isChargesCollectionLoaded && !IsDeleted)
				{
					isChargesCollectionLoaded = true;
					fCharges.DisplaySequenceChanged -= Charges_OnDisplaySequenceChanged;

					using (new DisposableAction(
						() => { Factory.SuspendValidation(); },
						() => { Factory.ResumeValidation(); }))
					{
						fCharges.Load();
					}

					UpdateSettingHasChangesOnAllChildrenSuspenders_Charges();

					fCharges.SetReadOnlyIncludingChildren(IsClosed);
					fCharges.DisplaySequenceChanged += Charges_OnDisplaySequenceChanged;
				}
			}
		}

		public void RefreshCharges()
		{
			Charges.Load();
			UpdateTotals();
		}

		void ResetAdditionalJobs()
		{
			if (fCharges != null)
			{
				fCharges.ResetAdditionalJobs();
			}
		}

		void ToggleFlagForReLoadingAdditionalJobs(bool performReload)
		{
			shouldReloadAdditionalJobs = performReload;
		}

		bool shouldReloadAdditionalJobs = true;

		#region CheckForChargesDisplaySequenceDuplicates

		void Charges_OnDisplaySequenceChanged(object sender, EventArgs e)
		{
			CheckForChargesDisplaySequenceDuplicates();
		}

		void CheckForChargesDisplaySequenceDuplicates()
		{
			if (CheckForChargesDisplaySequenceDuplicatesSuspender.IsSuspended || CheckForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck.IsSuspended ||
				Factory.HasContext(BusinessContext.ChargeReloader))
			{
				return;
			}

			using (CheckForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck.GetSuspender())
			{
				var warningMessage = Res.GetString("D8AB2B02-0285-46E8-B87D-6AE045143524", "Charges with duplicated sequence values will be displayed in the order that they were entered.");

				Charges.Cast<Charge>().Where(x => !x.IsValidationSuspended && x.HasRowWarnings)
					.ForEach(y => y.RemoveRowWarning(warningMessage));

				lock (Charges)
				{
					Charges.Cast<Charge>()
						.Where(x => !x.IsDeleted && !x.IsDeleting && !x.JR_IsRevenuePosted && !x.IsValidationSuspended)
						.GroupBy(item => item.JR_DisplaySequence)
						.Where(g => g.Count() > 1)
						.SelectMany(grp => grp)
						.ForEach(x => x.AddRowWarning(warningMessage));
				}
			}
		}

		internal FunctionalitySuspender CheckForChargesDisplaySequenceDuplicatesSuspender
		{
			get
			{
				return checkForChargesDisplaySequenceDuplicatesSuspender ?? (checkForChargesDisplaySequenceDuplicatesSuspender =
						new FunctionalitySuspender(() => CheckForChargesDisplaySequenceDuplicates(), true));
			}
		}
		FunctionalitySuspender checkForChargesDisplaySequenceDuplicatesSuspender;

		internal FunctionalitySuspender CheckForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck
		{
			get
			{
				return checkForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck ?? (checkForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck = new FunctionalitySuspender());
			}
		}
		FunctionalitySuspender checkForChargesDisplaySequenceDuplicatesSuspenderWithoutResumeCheck;

		#endregion

		#region ReceivableCharges

		public IReceivablesPostingChargeCollection ReceivableCharges
		{
			get
			{
				IReceivablesPostingChargeCollection rcvCharges = new IReceivablesPostingChargeCollection();
				rcvCharges.Job = this;
				foreach (IReceivablesPostingCharge charge in Charges)
				{
					rcvCharges.Add(charge);
				}

				return rcvCharges;
			}
		}

		#endregion

		#region ReceivableChargesNotPostedYet

		public IReceivablesPostingChargeCollection ReceivableChargesNotPostedYet
		{
			get
			{
				IReceivablesPostingChargeCollection rcvCharges = new IReceivablesPostingChargeCollection();
				rcvCharges.Job = this;
				foreach (IReceivablesPostingCharge charge in Charges)
				{
					if (!charge.IsRevenuePosted)
					{
						rcvCharges.Add(charge);
					}
				}

				return rcvCharges;
			}
		}

		#endregion

		#region ChargesFilteredByChargeViewingPermission

		public FilteredChargeCollectionView ChargesFilteredByChargeViewingPermission
		{
			get
			{
				if (chargesFilteredByChargeViewingPermission == null)
				{
					chargesFilteredByChargeViewingPermission = new FilteredChargeCollectionView(Charges);
				}

				return chargesFilteredByChargeViewingPermission;
			}
		}

		FilteredChargeCollectionView chargesFilteredByChargeViewingPermission;

		#endregion

		#region FilteredCharges

		public FilteredChargeCollection FilteredCharges
		{
			get
			{
				if (filteredCharges == null)
				{
					filteredCharges = new FilteredChargeCollection(ChargesFilteredByChargeViewingPermission);
					((IBindingList)filteredCharges).ListChanged += FilteredCharges_ListChanged;
					UpdateFilteredChargesFilter();
				}

				return filteredCharges;
			}
		}

		FilteredChargeCollection filteredCharges;

		void FilteredCharges_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (Factory.HasContext(BusinessContext.ChangingCurrentCellOnJobChargeBoundGrid)
				&& (e.ListChangedType == ListChangedType.ItemDeleted
					|| e.ListChangedType == ListChangedType.ItemAdded)
				)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK
					, CriticalValidationInfoCollectorServiceKeyType.ChangingCurrentCellOnJobChargeBoundGrid
					, () =>
					{
						var limitedTrace = System.Environment.StackTrace.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None).Skip(7).Take(10);
						return $@"[{e.ListChangedType},index:{e.NewIndex}] list is changed,top 10 trace below:
{string.Join(System.Environment.NewLine, limitedTrace)}
";
					}
					, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			}
		}

		void UpdateFilteredChargesFilter()
		{
			if (JH_IsChargeCostReferenceFilterEnabled)
			{
				FilteredCharges.EnableCostReferenceFilter(OperationalJobRef);
			}
			else
			{
				FilteredCharges.DisableCostReferenceFilter();
			}
		}

		#endregion

		[BusinessObjectTestExclude] //required as collection is generated by AutoRating which needs to store the result
		public AutoRateInfoCollection AutoRatedInfosForJobRevenue
		{
			get
			{
				if (autoRateInfosForJobRevenue == null)
				{
					var businessObjectToAutoRate = Parent as IBusiness;
					if (businessObjectToAutoRate != null)
					{
						var starter = new AutoRatingStarter(businessObjectToAutoRate, null);
						starter.ExecuteAutorating(
							new AutoRateOptions
							(
								autoRateRevenue: true,
								triggerSource: AutoRateTriggerSource.PrintInvoicing,
								billingType: BillingType.PrintInvoicing
							));
					}
				}

				return autoRateInfosForJobRevenue ?? new AutoRateInfoCollection(Factory);
			}
			internal set { autoRateInfosForJobRevenue = value; }
		}

		AutoRateInfoCollection autoRateInfosForJobRevenue;

		public AutoRateInfoCollection AutoRatedInfoGroupByChargeForJobRevenue
		{
			get
			{
				if (autoRatedInfoGroupByChargeForJobRevenue == null)
				{
					// trigger AutoRating that set AutoRatedInfoGroupByChargeForJobRevenue through AutoRateHLSPrintingStrategy
					_ = AutoRatedInfosForJobRevenue;
				}

				return autoRatedInfoGroupByChargeForJobRevenue ?? new AutoRateInfoCollection(Factory);
			}
			internal set { autoRatedInfoGroupByChargeForJobRevenue = value; }
		}

		AutoRateInfoCollection autoRatedInfoGroupByChargeForJobRevenue;

		#endregion

		#region Exchange Rates

		[ChildEditable(true)]
		public ExchangeRatesCollection ExchangeRates
		{
			get
			{
				if (exchangeRates == null)
				{
					exchangeRates = new ExchangeRatesCollection(this, Factory);
					RegisterEditableChildObject(exchangeRates);
				}

				LoadExchangeRates();

				return exchangeRates;
			}
		}

		void LoadExchangeRates()
		{
			if (exchangeRates == null)
			{
				return;
			}

			if (!ExchangeRatesLoadSuspender.IsSuspended && !isExchangeRatesLoaded)
			{
				isExchangeRatesLoaded = true;

				exchangeRates.Load();
				UpdateSettingHasChangesOnAllChildrenSuspenders_ExchangeRates();

				if (!IsDeleted)
				{
					exchangeRates.SetReadOnlyIncludingChildren(IsClosed);
				}
			}
		}

		ExchangeRatesCollection exchangeRates;
		bool isExchangeRatesLoaded;

		internal FunctionalitySuspender ExchangeRatesLoadSuspender
		{
			get { return exchangeRatesLoadSuspender ?? (exchangeRatesLoadSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender exchangeRatesLoadSuspender;

		#endregion

		#region Staff

		public GlbStaffCollection StaffCollection
		{
			get { return FindboxLookupCollections.GetStaffCollection(Factory); }
		}

		#endregion

		#region Branches

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory); }
		}

		#endregion

		#region One Off Quotes

		public BusinessObjectCollection Quotes
		{
			get
			{
				return FindboxLookupCollections.GetViewQuotedBookingCollection(Factory);
			}
		}

		#endregion

		#region Debtors

		public OrganisationsFindBoxCollection Debtors
		{
			get
			{
				if (fDebtors == null)
				{
					if (Parent != null && Parent.GetType().Name.ToUpper().StartsWith("WHS") && !Env.Security.WhsAllowedClients.IsAllowed)
					{
						fDebtors = Factory.GetCachedValue("Job_" + FindboxLookupCollections.CachingKey, () =>
						{
							var collection = new WarehouseClientCollectionWithSecurityCheck(Factory);
							collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));
							return collection;
						});
					}
					else
					{
						fDebtors = Factory.GetCachedValue("Job_" + FindboxLookupCollections.CachingKey, () =>
						{
							var collection = new OrganisationsFindBoxCollection(Factory);
							collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));
							return collection;
						});
					}

					fDebtors.OrganisationType = OrganisationTypes.Debtor;
				}
				return fDebtors;
			}
		}

		OrganisationsFindBoxCollection fDebtors;

		#endregion

		#region Job Status

		public CodeDescriptionPairList JobStatusList
		{
			get { return Factory.GetCachedValue<JobHeaderStatusList>(); }
		}

		#endregion

		#region One Off Quotation

		public Quote OneOffQuote
		{
			get
			{
				Quote result = null;
				if (!JH_TH_NKQuoteNumber.IsEmpty)
				{
					ZQuery filter = new ZQuery(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
					filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);
					filter.AddToFilter(RatingHeaderSchema.TH_QuoteNumber, JH_TH_NKQuoteNumber);
					filter.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, true);
					result = Factory.LoadTop1<Quote>(filter);
				}

				return result;
			}
		}

		#endregion

		#region Agent Relationship / Profit Share Agreement

		OrgAgentRelationship AgentRelationship
		{
			get
			{
				if (fAgentRelationship == null && PlugInData != null)
				{
					if (PlugInData.InvoicingSupporter.ReceivingAgent != null && PlugInData.InvoicingSupporter.SendingAgent != null)
					{
						fAgentRelationship = new OrgAgentRelationship.Loader(Factory).Load(PlugInData.InvoicingSupporter.SendingAgent, PlugInData.InvoicingSupporter.ReceivingAgent);
					}

					if (fAgentRelationship == null && PlugInData.InvoicingSupporter.ControllingAgent != null)
					{
						fAgentRelationship = new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(PlugInData.InvoicingSupporter.ControllingAgent);
					}

					if (fAgentRelationship == null && PlugInData.InvoicingSupporter.SendingAgent != null)
					{
						fAgentRelationship = new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(PlugInData.InvoicingSupporter.SendingAgent);
					}

					if (fAgentRelationship == null && PlugInData.InvoicingSupporter.ReceivingAgent != null)
					{
						fAgentRelationship = new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile(PlugInData.InvoicingSupporter.ReceivingAgent);
					}
				}

				return fAgentRelationship;
			}
		}

		OrgAgentRelationship fAgentRelationship;

		public OrgProfitShareDetails ProfitShareAgreement
		{
			get
			{
				if (profitShareAgreement == null && AgentRelationship != null)
				{
					var controllingCustomer = ControllingCustomerRetriever.GetControllingCustomer(PlugInData.InvoicingSupporter);
					profitShareAgreement = GetProfitShareDetails(AgentRelationship, controllingCustomer);
				}

				return profitShareAgreement;
			}
		}

		OrgProfitShareDetails profitShareAgreement;

		internal OrgProfitShareDetails GetProfitShareDetails(OrgAgentRelationship relationship, OrgHeader controllingCustomer)
		{
			if (PlugInData == null || PlugInData.InvoicingSupporter == null)
			{
				return null;
			}

			var consol = PlugInData as ForwardingConsol;
			var jobType = GetProfitShareAgreementJobTypeFromForwardingConsol(consol);
			var gatewayAgentType = GetProfitShareAgreementGatewayAgentTypeFromForwardingConsol(consol);

			var agreement = relationship.ProfitShareDetails.GetProfitShareAgreement(
				ZDateTime.Today,
				PlugInData.InvoicingSupporter.TransportMode,
				PlugInData.InvoicingSupporter.ContainerMode,
				(PlugInData.InvoicingSupporter.Origin != null ? PlugInData.InvoicingSupporter.Origin.RL_Code : ZString.Empty),
				(PlugInData.InvoicingSupporter.Destination != null ? PlugInData.InvoicingSupporter.Destination.RL_Code : ZString.Empty),
				new OrganisationsWithTypes(LocalCharges, PlugInData.InvoicingSupporter),
				controllingCustomer,
				jobType,
				gatewayAgentType,
				freightModeGroupagePriority: IsGroupageContainerModeForced(PlugInData as ForwardingShipment)
			);

			return agreement;
		}

		static bool IsGroupageContainerModeForced(ForwardingShipment shipment)
		{
			return
				shipment != null &&
				shipment.JS_PackingMode == Constants.ContainerModes.LCL &&
				shipment.Consols.OfType<ForwardingConsol>().Any(c => c.JK_ConsolMode == Constants.ContainerModes.Groupage);
		}

		static string GetProfitShareAgreementJobTypeFromForwardingConsol(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			return consol.IsGateway()
				? JobTypesList.Codes.GCN
				: JobTypesList.Codes.SHP;
		}

		static string GetProfitShareAgreementGatewayAgentTypeFromForwardingConsol(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			var isSendingAgentAGatewayAgent = consol.IsSendingAgentGTT() || consol.IsSendingAgentGTA();
			var isReceivingAgentAGatewayAgent = consol.IsReceivingAgentGTT() || consol.IsReceivingAgentGTA();

			switch (isSendingAgentAGatewayAgent)
			{
				case true when isReceivingAgentAGatewayAgent:
					return GatewayAgentTypesList.Codes.BGW;

				case true:
					return GatewayAgentTypesList.Codes.SGW;

				case false when isReceivingAgentAGatewayAgent:
					return GatewayAgentTypesList.Codes.RGW;

				default:
					return null;
			}
		}

		#endregion

		#region Profit and Loss

		public JobProfitLossCollection ProfitLoss
		{
			get
			{
				if (fProfitLoss == null)
				{
					fProfitLoss = new JobProfitLossCollection(Factory);
					JobProfitLoss item = new JobProfitLoss(Factory);
					List<ZGuid> jobs = new List<ZGuid>();
					jobs.Add(PK);
					if (Parent is IJobInvoicingPlugInAdditionalJobs)
					{
						jobs.AddRange(AdditionalJobsToShowChargesFor);
					}

					ZQuery childJobQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, jobs.ToArray());
					childJobQuery.AddToFilter(JobHeaderSchema.JH_GC, JH_GC);
					childJobQuery.FetchOnlyFromLocalCache = !IsInDatabase;
					Job[] childJobs = Factory.Load<Job>(childJobQuery);
					foreach (Job job in childJobs)
					{
						jobs.Add(job.PK);
					}

					item.SetJobPKs(jobs.ToArray());
					item.SetParent(Parent);
					fProfitLoss.Add(item);
				}
				return fProfitLoss;
			}
		}

		JobProfitLossCollection fProfitLoss;

		#endregion

		#region AR Invoices Printing Filter

		JobARInvoicePrintingFilter fPrintingFilter;
		public JobARInvoicePrintingFilter PrintingFilter
		{
			get { return fPrintingFilter ?? (fPrintingFilter = new JobARInvoicePrintingFilter((IBusiness)Parent, PK)); }
		}

		#endregion

		#region AP Invoices Printing Filter

		JobAPInvoicePrintingFilter fAPPrintingFilter;
		public JobAPInvoicePrintingFilter APPrintingFilter
		{
			get { return fAPPrintingFilter ?? (fAPPrintingFilter = new JobAPInvoicePrintingFilter((IBusiness)Parent, PK)); }
		}

		#endregion

		#region Cash Advance Requests

		public AccCashAdvanceRequestHeaderCollection CashAdvanceRequests
		{
			get
			{
				if (cashAdvanceRequests == null)
				{
					cashAdvanceRequests = new AccCashAdvanceRequestHeaderCollection(Factory);
					cashAdvanceRequests.Load(new ZQuery(AccCashAdvanceRequestHeaderSchema.CAH_JH_Job, PK));
				}
				return cashAdvanceRequests;
			}
		}
		AccCashAdvanceRequestHeaderCollection cashAdvanceRequests;

		public void ResetCashAdvanceRequests() => cashAdvanceRequests = null;

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!Factory.IsConstructingNullBusinessObject)
			{
				JH_Status = JobHeaderStatus.Working.Code;
				JH_GS_NKRepOps = GlbStaff.CurrentUser.GS_Code;
			}
		}

		public void Reset()
		{
			SetDefaultValues();
			Charges.RemoveAll();
		}

		public void SetDefaultValueForTaxBranch()
		{
			if (JH_GB_TaxBranch.IsEmpty)
			{
				JH_GB_TaxBranch = GetTaxBranchDefaultValue(true);
			}
		}

		ZGuid GetTaxBranchDefaultValue(bool otherCondition = true)
		{
			var result = ZGuid.Empty;

			if (AccountingConfigurationRegistry.Instance.CustomJobTaxBranchDefaultingRulesEngineConfiguration.Value)
			{
				var branchDefaultingManager = ObjectFactory.Get<IJobBillingTaxBranchDefaultingManager>();
				branchDefaultingManager.SetDefaultValue(PlugInData, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment);
				if (branchDefaultingManager.DefaultValue != null)
				{
					result = (ZGuid)branchDefaultingManager.DefaultValue;
				}
			}

			if (result.IsEmpty)
			{
				result = AccountingMasterFilesUtils.GetTaxBranchResetValue(otherCondition);
			}

			return result;
		}

		#region Job Defaults

		public IDisposable GetSetJobDefaultsSuspender()
		{
			return new SetJobDefaultsSuspender(this);
		}

		bool IsSetJobDefaultsSuspended;

		class SetJobDefaultsSuspender : IDisposable
		{
			public SetJobDefaultsSuspender(Job parentJob)
			{
				this.ParentJob = parentJob;
				parentJob.IsSetJobDefaultsSuspended = true;
			}

			readonly Job ParentJob;

			void IDisposable.Dispose()
			{
				ParentJob.IsSetJobDefaultsSuspended = false;
			}
		}

		public override void SetDefaultsForJob()
		{
			if (!IsSetJobDefaultsSuspended)
			{
				using (this.GetSetJobDefaultsSuspender())
				{
					if (PlugInData != null)
					{
						SetDefaultValuesForLocalClient();
						SetDefaultValuesForOverseasAgent();
						SetDefaultValuesForJobStatus();
						SetDefaultBranch(PlugInData);
						SetDefaultDepartment(PlugInData);

						GetControllingCustomerForSaleRepOrSubscribeToChange(true);
						if (JH_GS_NKRepSales.IsEmpty || CanOverrideDefaults(JH_GS_NKRepSalesInfo))
						{
							using (GetSetJobDefaultsInProgress())
							{
								SetDefaultValuesForSalesRep();
							}
						}

						SetDefaultValuesForOperator();
						SetDefaultValueForExcludeFromAutoRating();
						ToggleFlagForReLoadingAdditionalJobs(true);
						AddDefaultCurrency();
						SetDefaultValueForTaxBranch();
					}
					if (Parent != null && !(Parent is IJobInvoicingPlugIn) && JH_GE.IsEmpty)
					{
						JH_GE = Env.CurrentDepartment.PK;
					}
					UpdateJobReadOnlyStatus();
					RefreshBinding();
				}
			}
		}

		void ReadPivots()
		{
			if (Parent != null)
			{
				TryAssignClient(Parent.PK);
				TryAssignQuoteNumber();
			}
		}

		protected List<ZGuid> AdditionalJobsToShowChargesFor
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();
				var additionalJobsPlugInData = PlugInData as IJobInvoicingPlugInAdditionalJobs;
				if (additionalJobsPlugInData != null)
				{
					result.AddRange(JobAccessor.GetJobsFromShipment(additionalJobsPlugInData.AdditionalJobsToShowChargesFor));
				}

				return result;
			}
		}

		JobInvoicingDataAccessor JobAccessor
		{
			get
			{
				if (fJobAccessor == null)
				{
					fJobAccessor = new JobInvoicingDataAccessor(Factory);
				}
				return fJobAccessor;
			}
		}

		JobInvoicingDataAccessor fJobAccessor;

		public IEnumerable<ZGuid> ChildJobPKs
		{
			get
			{
				ZQuery childJobsQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, PK);
				childJobsQuery.AddToFilter(JobHeaderSchema.JH_GC, JH_GC);
				childJobsQuery.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.Load<Job>(childJobsQuery).Select(x =>
				{
					if (x.Parent == null)
					{
						x.InitializeParentFromGenericJobWithoutSettingDefaults();
					}
					return x.PK;
				});
			}
		}

		#region CanOverrideDefaults

		bool CanOverrideDefaults(ZPropertyInfo property)
		{
			if (IsInDatabase)
			{
				return false;
			}

			bool changedNotInSuspendedMode = false;
			if (ChangedNotInSuspendedMode.ContainsKey(property.Name))
			{
				changedNotInSuspendedMode = ChangedNotInSuspendedMode[property.Name];
			}
			else
			{
				if (property.Value.IsValid)
				{
					changedNotInSuspendedMode = true;
				}
			}
			return !changedNotInSuspendedMode && !HasNonZeroCharges;
		}

		void PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(ZPropertyInfo propertyInfo)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));
			if (!IsInDatabase)
			{
				if (!propertyInfo.Value.IsEmpty)
				{
					ChangedNotInSuspendedMode[propertyInfo.Name] = !IsSetJobDefaultsInProgress;
				}
			}
		}

		readonly Dictionary<string, bool> ChangedNotInSuspendedMode = new Dictionary<string, bool>();

		#endregion

		#region Local Client

		void SetDefaultValuesForLocalClient()
		{
			if (LocalChargesPK.IsEmpty)
			{
				JH_OA_LocalChargesAddr = GetClientSpecifiedBillingParty();
			}
			if (LocalChargesPK.IsEmpty || CanOverrideDefaults(JH_OA_LocalChargesAddrInfo))
			{
				ZGuid defaultBillTo = GetDefaultBillTo();
				if ((defaultBillTo.IsValid) && (defaultBillTo != AgentCollectPK) && !IsPartyMiscOrganization(defaultBillTo))
				{
					using (GetSetJobDefaultsInProgress())
					{
						LocalChargesPK = defaultBillTo;
					}
				}
			}
		}

		ZGuid GetDefaultBillTo()
		{
			ZGuid result = ZGuid.Empty;
			if (PlugInData != null)
			{
				if (PlugInData.InvoicingSupporter.OverriddenDefaultLocalClient != null)
				{
					result = PlugInData.InvoicingSupporter.OverriddenDefaultLocalClient.PK;
				}
				else if (JobType == JobInvoicingConsumerTypes.GatewayConsol)
				{
					if (PlugInData.InvoicingSupporter.SendingAgent != null)
					{
						result = PlugInData.InvoicingSupporter.SendingAgent.PK;
					}
				}
				else if (JobType == JobInvoicingConsumerTypes.LocalCartage && PlugInData.InvoicingSupporter.Consignor != null)
				{
					result = PlugInData.InvoicingSupporter.Consignor.PK;
				}
				else if ((JobType == JobInvoicingConsumerTypes.Shipment || JobType == JobInvoicingConsumerTypes.QuotedBooking) && IsDomestic)
				{
					result = GetBillToForDomesticShipment();
				}
				else if (CanCrossTradeDebtorDefaultingBeApplied && PlugInData.InvoicingSupporter.Consignor != null)
				{
					result = PlugInData.InvoicingSupporter.Consignor.GetRelatedBillToParty(JobType, false).PK;
				}
				else if ((IsImport || IsCrossTrade) && PlugInData.InvoicingSupporter.Consignee != null)
				{
					result = GetBillToForImportOrCrossTrade();
				}
				else if (IsExport)
				{
					result = GetBillToForExport();
				}
				else if (PlugInData.InvoicingSupporter.Consignee != null)
				{
					result = PlugInData.InvoicingSupporter.Consignee.PK;
				}
			}

			return result;
		}

		ZGuid GetClientSpecifiedBillingParty()
		{
			ZGuid result = ZGuid.Empty;
			var entity = PlugInData as IDocAddresses;
			if (entity != null)
			{
				var clientRequestedBillingPartyCollection = entity.DocAddresses.FindDocAddressesByType(DocAddressType.ClientRequestedBillingParty);
				if (clientRequestedBillingPartyCollection.Length > 0)
				{
					result = clientRequestedBillingPartyCollection[0].E2_OA_Address;
				}
			}
			return result;
		}

		ZGuid GetBillToForDomesticShipment()
		{
			var result = ZGuid.Empty;
			var entity = PlugInData;

			if (entity != null)
			{
				var supporter = entity.InvoicingSupporter;
				var prepaidCollect = PaymentTerm.GetPrepaidCollect(CostSell.Revenue);

				switch (prepaidCollect)
				{
					case Constants.PaymentType.Collect:
						if (!PaymentTerm.IsThirdParty(PaymentTermType.DomesticPaymentTerm))
						{
							var consignee = supporter.Consignee;
							if (consignee != null)
							{
								result = consignee.DeliveryFreightBillTo.PK;
							}
						}
						break;
					case Constants.PaymentType.Prepaid:
						var consignor = supporter.Consignor;
						if (consignor != null)
						{
							result = consignor.GetFreightBillTo(false, TransportMode, ContainerMode).PK;
						}
						break;
				}
			}

			return result;
		}

		ZGuid GetBillToForImportOrCrossTrade()
		{
			ZGuid result = ZGuid.Empty;

			if (PlugInData.InvoicingSupporter.Consignee != null)
			{
				result = PlugInData.InvoicingSupporter.Consignee.GetRelatedBillToParty(JobType, true, TransportMode, ContainerMode).PK;
			}

			return result;
		}

		ZGuid GetBillToForExport()
		{
			ZGuid result = ZGuid.Empty;

			if (PlugInData.InvoicingSupporter.Consignor != null)
			{
				result = PlugInData.InvoicingSupporter.Consignor.GetRelatedBillToParty(JobType, false, TransportMode, ContainerMode).PK;
			}

			if (JobType == JobInvoicingConsumerTypes.CFSShipment && PlugInData.InvoicingSupporter.SendingAgent != null)
			{
				result = PlugInData.InvoicingSupporter.SendingAgent.PK;
			}

			return result;
		}

		bool IsPartyMiscOrganization(ZGuid orgHeaderPK)
		{
			var orgHeader = Factory.GetCachedReadOnlyFactory().Load<OrgHeader>(orgHeaderPK);
			return orgHeader != null && orgHeader.IsMiscellaneous;
		}

		#endregion

		#region Overseas Agent

		void SetDefaultValuesForOverseasAgent()
		{
			if (AgentCollectPK.IsEmpty || CanOverrideDefaults(JH_OA_AgentCollectAddrInfo))
			{
				ZGuid defaultAgent = GetDefaultAgent();
				if ((defaultAgent.IsValid) && (defaultAgent != LocalChargesPK) && !IsPartyMiscOrganization(defaultAgent))
				{
					using (GetSetJobDefaultsInProgress())
					{
						AgentCollectPK = defaultAgent;
					}
				}
			}
		}

		ZGuid GetDefaultAgent()
		{
			ZGuid? result = null;
			var supporter = PlugInData.InvoicingSupporter;

			if (JobType == JobInvoicingConsumerTypes.GatewayConsol)
			{
				result = supporter.ReceivingAgent?.PK;
			}
			else if ((JobType == JobInvoicingConsumerTypes.Shipment || JobType == JobInvoicingConsumerTypes.QuotedBooking) && !IsDomestic)
			{
				bool isForwardingShipment = PlugInData is ForwardingShipment;
				if (supporter.IsExport)
				{
					if (supporter.Consignee != null &&
						(
							supporter.IsDirectShipment && supporter.Consignee.OH_IsDebtor ||
							isForwardingShipment && supporter.Consignee.CompanyData.OB_EXBillAgentChargesDirect
						))
					{
						result = supporter.Consignee.ARGrouping.PK;
					}
					else
					{
						result = supporter.LatestReceivingAgent?.ARGrouping?.PK;
					}
				}
				else if (CanCrossTradeDebtorDefaultingBeApplied && PlugInData.InvoicingSupporter.Consignee != null)
				{
					result = PlugInData.InvoicingSupporter.Consignee.GetRelatedBillToParty(JobType, true).PK;
				}
				else if (supporter.IsImport || IsCrossTrade)
				{
					if (isForwardingShipment && supporter.Consignor != null && supporter.Consignor.CompanyData.OB_IMBillAgentChargesDirect)
					{
						result = supporter.Consignor.ARGrouping.PK;
					}
					else
					{
						result = supporter.EarliestSendingAgent?.ARGrouping?.PK;
					}
				}
			}

			return result ?? ZGuid.Empty;
		}

		#endregion

		#region Job Status

		void SetDefaultValuesForJobStatus()
		{
			if (JH_Status.IsEmpty)
			{
				JH_Status = JobHeaderStatus.Working.Code;
			}
		}

		#endregion

		#region Sales Rep

		public override ZString JH_GS_NKRepSales
		{
			get { return base.JH_GS_NKRepSales; }
			set
			{
				var oldvalue = JH_GS_NKRepSales;
				base.JH_GS_NKRepSales = value;
				PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_GS_NKRepSalesInfo);

				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobJH_GS_NKRepSalesSetterCallStack,
					() => FormattableString.Invariant($@"JH_GS_NKRepSales: Old Value: {oldvalue}, New Value: {JH_GS_NKRepSales} , StackTrace ->\r\n {System.Environment.StackTrace}"));
			}
		}

		void SetDefaultValuesForSalesRep()
		{
			JH_GS_NKRepSales = GetDefaultSalesRep();
		}

		internal ZString GetDefaultSalesRep()
		{
			var salesRepOrganisation = GetControllingCustomerForSaleRep() ?? LocalCharges;
			var result = ZString.Empty;
			if (salesRepOrganisation != null)
			{
				result = salesRepOrganisation.StaffAssignments.OverallSalesRep;

				if (JobType == JobInvoicingConsumerTypes.WarehouseInwards || JobType == JobInvoicingConsumerTypes.WarehouseOutwards || JobType == JobInvoicingConsumerTypes.WarehouseStorage)
				{
					result = salesRepOrganisation.StaffAssignments.WarehousingRep.IsEmpty ? salesRepOrganisation.StaffAssignments.OverallSalesRep : salesRepOrganisation.StaffAssignments.WarehousingRep;
				}
				else
				{
					if (TransportMode == Core.Constants.TransportModes.Air)
					{
						if (MovementDirection == Directions.Domestic)
						{
							result = salesRepOrganisation.StaffAssignments.DomesticAirRep;
						}
						else if (MovementDirection == Directions.Import)
						{
							result = salesRepOrganisation.StaffAssignments.ImportAirRep;
						}
						else
						{
							result = salesRepOrganisation.StaffAssignments.ExportAirRep;
						}
					}
					else if (TransportMode == Core.Constants.TransportModes.Sea)
					{
						if (MovementDirection == Directions.Domestic)
						{
							result = salesRepOrganisation.StaffAssignments.DomesticSeaRep;
						}
						else if (MovementDirection == Directions.Import)
						{
							result = salesRepOrganisation.StaffAssignments.ImportSeaRep;
						}
						else
						{
							result = salesRepOrganisation.StaffAssignments.ExportSeaRep;
						}
					}
					else if (TransportMode == Core.Constants.TransportModes.Road)
					{
						if (MovementDirection == Directions.Domestic)
						{
							result = salesRepOrganisation.StaffAssignments.DomesticRoadRep;
						}
						else if (MovementDirection == Directions.Import)
						{
							result = salesRepOrganisation.StaffAssignments.ImportRoadRep;
						}
						else
						{
							result = salesRepOrganisation.StaffAssignments.ExportRoadRep;
						}
					}
					else if (TransportMode == Core.Constants.TransportModes.Rail)
					{
						if (MovementDirection == Directions.Domestic)
						{
							result = salesRepOrganisation.StaffAssignments.DomesticRailRep;
						}
						else if (MovementDirection == Directions.Import)
						{
							result = salesRepOrganisation.StaffAssignments.ImportRailRep;
						}
						else
						{
							result = salesRepOrganisation.StaffAssignments.ExportRailRep;
						}
					}
				}
			}
			return result;
		}

		internal OrgHeader GetControllingCustomerForSaleRep() => GetControllingCustomerForSaleRepOrSubscribeToChange(false);

		OrgHeader GetControllingCustomerForSaleRepOrSubscribeToChange(bool doSubscribeOnly)
		{
			var salesRepDefaultingSupporter = GetSalesRepDefaultingSupporterIsApplicable();
			if (salesRepDefaultingSupporter != null)
			{
				if (doSubscribeOnly && this.HasContext(BusinessContext.InvoicingPluginGUIExcludingConsol))
				{
					if (!this.HasContext(BusinessContext.InterCompanyJobOperation))
					{
						salesRepDefaultingSupporter.NotifyControllingCustomerChanged((sender, e) => SetDefaultValuesForSalesRep());
					}
				}
				else
				{
					return PlugInData.InvoicingSupporter.ControllingCustomer;
				}
			}
			return null;
		}

		ISalesRepDefaultingFromControllingCustomer GetSalesRepDefaultingSupporterIsApplicable()
		{
			if (PlugInData?.InvoicingSupporter is ISalesRepDefaultingFromControllingCustomer salesRepDefaultingSupporter && salesRepDefaultingSupporter.IsAllowedToDefaultSalesRepFromControllingCustomer)
			{
				return salesRepDefaultingSupporter;
			}
			return null;
		}

		#endregion

		#region Operator

		void SetDefaultValuesForOperator()
		{
			if (JH_GS_NKRepOps.IsEmpty)
			{
				JH_GS_NKRepOps = GlbStaff.CurrentUser.GS_Code;
			}
		}

		#endregion

		#region Currency

		void AddDefaultCurrency()
		{
			if (PlugInData.InvoicingSupporter.ConsumerType != null)
			{
				if (PlugInData.InvoicingSupporter.ConsumerType.Code == JobInvoicingConsumerTypes.AgencyBooking.Code
					|| PlugInData.InvoicingSupporter.ConsumerType.Code == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
					|| PlugInData.InvoicingSupporter.ConsumerType.Code == JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code)
				{
					IJobInvoicingExRateSourceProvider provider;
					IExchangeRateSource source;

					if ((provider = PlugInData as IJobInvoicingExRateSourceProvider) != null && (source = provider.GetExRateSource(ExRateSourceType.Voyage)) != null)
					{
						AddGenericExchangeRatesFromSource(source);
					}
				}
			}
		}

		#endregion

		#region Branch

		public void SetDefaultBranch(IJobInvoicingPlugIn plugin)
		{
			if (!IsDeleted && (JH_GB.IsEmpty || CanOverrideDefaults(JH_GBInfo)))
			{
				using (GetSetJobDefaultsInProgress())
				{
					JH_GB = new BranchChooser(Factory).GetBranch(LocalCharges, AgentCollect, plugin);
				}
			}
		}

		#endregion

		#region Department

		public void SetDefaultDepartment(IJobInvoicingPlugIn plugIn)
		{
			if (JH_GE.IsEmpty || CanOverrideDefaults(JH_GEInfo))
			{
				using (GetSetJobDefaultsInProgress())
				{
					JH_GE = DepartmentChooser.GetDepartment(plugIn);
				}
			}
		}

		DepartmentChooser fDepartmentChooser;
		DepartmentChooser DepartmentChooser
		{
			get { return fDepartmentChooser ?? (fDepartmentChooser = DepartmentChooser.New(Factory)); }
		}

		#endregion

		#region Exclude From AutoRating

		void SetDefaultValueForExcludeFromAutoRating()
		{
			if (!IsInDatabase)
			{
				JH_ExcludeFromPeriodicRating = JobType?.ShouldExcludeFromPeriodicBillingByDefault ?? false;
			}
		}

		#endregion

		#endregion

		#endregion

		#region PlugIn Consumer

		public IJobInvoicingPlugIn PlugInData
		{
			get { return Parent as IJobInvoicingPlugIn; }
			set { Parent = value; }
		}

		internal GenericJob.GenericJob GenericJobView
		{
			get
			{
				return !JH_ParentID.IsEmpty && !JH_ParentTableCode.IsEmpty ? this.LoadGenericJob<GenericJob.GenericJob>() : null;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			if (JH_GC == GlbCompany.CurrentCompany.PK)
			{
				base.RunPreSaveValidationCore();
			}
			else
			{
				this.SetContext(BusinessContext.HasBeenValidatedByDifferentCompany);

				if (IsChargesCollectionLoaded)
				{
					UnRegisterEditableChildObject(Charges);
				}

				return;
			}
		}

		protected override void SetParentCore(IJobHeaderParent value)
		{
			if (value != null)
			{
				var newParentSet = false;
				using (GetValidationSuspender())
				{
					base.SetParentCore(value);
					using (new DisposableAction(() => Factory.SetContext(BusinessContext.SetDefaultsForJob), () => Factory.RemoveContext(BusinessContext.SetDefaultsForJob)))
					using (SuspendSettingHasChangesOnAllChildren())
					{
						if (!JH_ParentID.IsValid || JH_ParentTableCode.IsEmpty)
						{
							ZString tableName = value.TableName;
							if (JH_ParentTableCode.IsEmpty && !tableName.IsEmpty)
							{
								JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
							}

							if (!JH_ParentID.IsValid && value.PK.IsValid)
							{
								JH_ParentID = value.PK;
							}

							if (JH_JobNum.IsEmpty && !string.IsNullOrEmpty(value.JobNumber))
							{
								JH_JobNum = value.JobNumber.Length > JH_JobNumInfo.MaxLength ? value.JobNumber.Substring(0, JH_JobNumInfo.MaxLength) : value.JobNumber;
							}

							ReadPivots();
							newParentSet = true;
						}

						SetDefaultsForJob();
					}
				}

				if (!Factory.HasContext(BusinessContext.IncompleteInvoiceDataAdapter))
				{
					if (!Factory.IsValidationSuspended && newParentSet)
					{
						Validation.ValidateJH_OA_LocalChargesAddr();
						// The line below is commented out because ValidateJH_OA_LocalChargesAddr calls ValidateJH_OA_AgentCollectAddr internaly
						//Validation.ValidateJH_OA_AgentCollectAddr();
					}
					Validation.ValidateJH_Status();
					Validation.ValidateJH_GB();
					Validation.ValidateJH_GE();
					Validation.ValidateJH_GS_NKRepSales();
					Validation.ValidateJH_GS_NKRepOps();
					((JobValidation)Validation).ValidateJH_ProfitLoss();
#if DEBUG
					ValidationCallCountInsideSetParentCore_ForTestOnly++;
#endif
				}

				SetHasChangesIfHasErrors();
			}
		}

#if DEBUG
		public int ValidationCallCountInsideSetParentCore_ForTestOnly;
		public bool AddSomeErrorNotValidatedInSetParentCore_ForTestOnly;
#endif
		public override ZString JH_ParentTableCode
		{
			get { return base.JH_ParentTableCode; }
			set
			{
				CheckFieldCannotChangeWhenJobIsAlreadyInDatabase(JH_ParentTableCodeInfo, value);

				base.JH_ParentTableCode = value;
				if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					using (ChargesLoadSuspender.GetSuspender())
					{
						Charges.MarkAsNeedingValidation();
					}
				}
			}
		}

		void CheckFieldCannotChangeWhenJobIsAlreadyInDatabase(ZPropertyInfo field, object value, Func<string> extraInfoCollector = null)
		{
			if (
#if DEBUG
			(!Globals.IsTest || Factory.HasAnyOfContexts(BusinessContext.EnableCheckFieldCannotChangeWhenJobIsAlreadyInDatabase_ForTestOnly)) &&
#endif
			IsInDatabase && !field.OriginalValue.Equals(value))
			{
				ErrorReporter.ReportOnce("JobHeaderChangedWhenAlreadyInDatabase_" + field.Name, FormattableString.Invariant($@"The jobHeader {field.Name} should not be changed after the job is already in the database. The {field.Name} value was changed from '{field.Value}' to '{value}'.
{GetJobDetails(this)}{extraInfoCollector?.Invoke()}"));
			}
		}

		string GetJobDetails(Job job)
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"Job Details: Job Number = {0}, Parent Table Code = {1}, Parent ID = {2}, Parent Type = {3}, Job Type = {4}, Job Status = {5}, Parent Job = ({6})",
job.JH_JobNum, job.JH_ParentTableCode, job.JH_ParentID, job.Parent?.GetType(), job.JobType, job.JH_Status, job.JH_JH_ParentJob.IsValid ? GetJobDetails(Factory.Load<Job>(job.JH_JH_ParentJob)) : string.Empty);
		}

		bool ShouldSkipCheckForChangedJobNumberField() => this.HasContext(BusinessContext.EnableJobHeaderNumberChange) && JH_ParentTableCode == JobConsolSchema.Constants.Prefix;

		public override ZString JH_JobNum
		{
			get { return base.JH_JobNum; }
			set
			{
				if (!ShouldSkipCheckForChangedJobNumberField())
				{
					CheckFieldCannotChangeWhenJobIsAlreadyInDatabase(JH_JobNumInfo, value, () => this.GetJobNumberInfo());
				}

				base.JH_JobNum = value;
				if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					using (ChargesLoadSuspender.GetSuspender())
					{
						Charges.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal IDisposable SuspendSettingHasChangesOnAllChildren()
		{
			JobDisposableCollection suspender = null;
			bool success = false;
			try
			{
				suspender = new JobDisposableCollection(this);
				if (settingHasChangesOnAllChildrenSuspendersList_CanBeNull == null)
				{
					settingHasChangesOnAllChildrenSuspendersList_CanBeNull = new List<JobDisposableCollection>();
				}
				settingHasChangesOnAllChildrenSuspendersList_CanBeNull.Add(suspender);

				success = true;
				return suspender;
			}
			finally
			{
				if (!success && suspender != null)
				{
					((IDisposable)suspender).Dispose();
				}
			}
		}
		List<JobDisposableCollection> settingHasChangesOnAllChildrenSuspendersList_CanBeNull;

		void UpdateSettingHasChangesOnAllChildrenSuspenders_Charges()
		{
			if (settingHasChangesOnAllChildrenSuspendersList_CanBeNull != null)
			{
				settingHasChangesOnAllChildrenSuspendersList_CanBeNull.ForEach(x => x.PopulateChargeDisposables());
			}
		}

		void UpdateSettingHasChangesOnAllChildrenSuspenders_ExchangeRates()
		{
			if (settingHasChangesOnAllChildrenSuspendersList_CanBeNull != null)
			{
				settingHasChangesOnAllChildrenSuspendersList_CanBeNull.ForEach(x => x.PopulateExchangeRatesDisposables());
			}
		}

		void AddSettingHasChangesOnAllChildrenSuspenders_Charge(Charge charge)
		{
			if (settingHasChangesOnAllChildrenSuspendersList_CanBeNull != null)
			{
				settingHasChangesOnAllChildrenSuspendersList_CanBeNull.ForEach(x => x.PopulateChargeDisposables(charge));
			}
		}

		void AddSettingHasChangesOnAllChildrenSuspenders_ExchangeRate(ExchangeRate exRate)
		{
			if (settingHasChangesOnAllChildrenSuspendersList_CanBeNull != null)
			{
				settingHasChangesOnAllChildrenSuspendersList_CanBeNull.ForEach(x => x.PopulateExchangeRateDisposables(exRate));
			}
		}

		class JobDisposableCollection : IDisposable
		{
			public JobDisposableCollection(Job job)
			{
				ParentJob = job;
				Disposables = new DisposableList(1);
				PopulateDisposables();
			}

			void PopulateDisposables()
			{
				Disposables.Add(ParentJob.SuspendSettingHasChangesKeepValidationAndSaving());
				PopulateChargeDisposables();
				PopulateExchangeRatesDisposables();
			}
			internal void PopulateChargeDisposables()
			{
				using (ParentJob.ChargesLoadSuspender.GetSuspender())
				{
					foreach (Charge charge in ParentJob.Charges)
					{
						Disposables.Add(charge.SuspendSettingHasChangesKeepValidationAndSaving());
					}
				}
			}

			internal void PopulateChargeDisposables(Charge charge)
			{
				Disposables.Add(charge.SuspendSettingHasChangesKeepValidationAndSaving());
			}

			internal void PopulateExchangeRatesDisposables()
			{
				using (ParentJob.ExchangeRatesLoadSuspender.GetSuspender())
				{
					foreach (ExchangeRate exRate in ParentJob.ExchangeRates)
					{
						Disposables.Add(exRate.SuspendSettingHasChangesKeepValidationAndSaving());
					}
				}
			}

			internal void PopulateExchangeRateDisposables(ExchangeRate exRate)
			{
				Disposables.Add(exRate.SuspendSettingHasChangesKeepValidationAndSaving());
			}

			void IDisposable.Dispose()
			{
				try
				{
					Disposables.Dispose();
				}
				finally
				{
					if (ParentJob.settingHasChangesOnAllChildrenSuspendersList_CanBeNull != null)
					{
						ParentJob.settingHasChangesOnAllChildrenSuspendersList_CanBeNull.Remove(this);
						if (!ParentJob.settingHasChangesOnAllChildrenSuspendersList_CanBeNull.Any())
						{
							ParentJob.settingHasChangesOnAllChildrenSuspendersList_CanBeNull = null;
						}
					}
					Disposables.Clear();
				}
			}

			readonly DisposableList Disposables;
			readonly Job ParentJob;
		}

		public PaymentTermInfos PaymentTerm
		{
			get
			{
				var plugInData = PlugInData;
				return plugInData != null ? plugInData.InvoicingSupporter.PaymentTerm : null;
			}
		}

		public Directions MovementDirection
		{
			get
			{
				Directions result = Directions.Unknown;

				InitializeParentFromGenericJobWithoutSettingDefaults();

				//Todo: replace with PlugInData.InvoicingSupporter.GetJobDirection() and then fix all the tests

				if (PlugInData != null && !IsPluginDataDeleted)
				{
					if (PlugInData.InvoicingSupporter.IsImport)
					{
						result = Directions.Import;
					}
					else if (PlugInData.InvoicingSupporter.IsDomestic)
					{
						result = Directions.Domestic;
					}
					else if (ImportExportHelper.IsCrossTrade(
						PlugInData.InvoicingSupporter.Origin?.Code ?? ZString.Empty,
						PlugInData.InvoicingSupporter.Destination?.Code ?? ZString.Empty))
					{
						result = Directions.CrossTrade;
					}
					else
					{
						result = Directions.Export;
					}
				}
				// When we copy an OOQ, CFX has to be calculated (coming from the Organisation), not copying from the original OOQ.
				// While the parent BusinessObject is Quote (not QuotedBooking), which doesn't implement IJobInvoicingPlugIn.
				// Therefore, we need to check the BusinessContext to let the CFX be updated during the copy.
				// Service Direction required for GetCFXPairFromOrganization() method to find the correct CFX.
				else if (this.HasContext(BusinessContext.CopyChargePersistentValues) && Parent is Quote quote && quote.CurrentOneOffQuote != null)
				{
					result = quote.CurrentOneOffQuote.JobDirection;
				}

				return result;
			}
		}

		internal bool IsPluginDataDeleted
		{
			get
			{
				BusinessObject bizO = PlugInData as BusinessObject;
				return bizO != null && bizO.IsDeleted;
			}
		}

		public ZString ServiceLevel
		{
			get
			{
				InitializeParentFromGenericJobWithoutSettingDefaults();
				return GetServiceLevel(PlugInData);
			}
		}

		internal static ZString GetServiceLevel(IJobInvoicingPlugIn plugin)
		{
			return plugin != null ? plugin.InvoicingSupporter.ServiceLevel : ZString.Empty;
		}

		public string ServiceDirection
		{
			get
			{
				string result = OrgConstants.ServiceDirection.Code.Unknown;

				InitializeParentFromGenericJobWithoutSettingDefaults();

				if (PlugInData != null && !IsPluginDataDeleted)
				{
					var serviceDirectionSupporter = PlugInData.InvoicingSupporter as IServiceDirection;
					if (serviceDirectionSupporter != null)
					{
						result = serviceDirectionSupporter.ServiceDirection;
					}
				}

				return result;
			}
		}

		internal ZString Direction
		{
			get
			{
				InitializeParentFromGenericJobWithoutSettingDefaults();
				return IsPluginDataDeleted ? ZString.Empty : GetDirection(PlugInData);
			}
		}

		internal static ZString GetDirection(IJobInvoicingPlugIn plugin)
		{
			ZString result = ZString.Empty;

			if (plugin != null)
			{
				if (plugin.InvoicingSupporter.IsDomestic)
				{
					result = Constants.FreightShipmentDirection.Code.Domestic;
				}
				else if (plugin.InvoicingSupporter.IsImport)
				{
					result = Constants.FreightShipmentDirection.Code.Import;
				}
				else if (plugin.InvoicingSupporter.IsExport)
				{
					result = Constants.FreightShipmentDirection.Code.Export;
				}
				else
				{
					result = Constants.FreightShipmentDirection.Code.Other;
				}
			}

			return result;
		}

		public ZString TransportMode
		{
			get
			{
				InitializeParentFromGenericJobWithoutSettingDefaults();

				if (PlugInData != null)
				{
					return GetTransportMode(PlugInData);
				}
				// When we copy an OOQ, CFX has to be calculated (coming from the Organisation), not copying from the original OOQ.
				// While the parent BusinessObject is Quote (not QuotedBooking), which doesn't implement IJobInvoicingPlugIn.
				// Therefore, we need to check the BusinessContext to let the CFX be updated during the copy.
				// Transport Mode required for GetCFXPairFromOrganization() method to find the correct CFX.
				else if (this.HasContext(BusinessContext.CopyChargePersistentValues) && Parent is Quote quote && quote.CurrentOneOffQuote != null)
				{
					return quote.CurrentOneOffQuote.TT_TransportMode;
				}

				return ZString.Empty;
			}
		}

		internal static ZString GetTransportMode(IJobInvoicingPlugIn plugin)
		{
			return plugin != null && !plugin.IsDeleted ? plugin.InvoicingSupporter.TransportMode : ZString.Empty;
		}

		public ZString ContainerMode
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.ContainerMode : ZString.Empty; }
		}

		public JobInvoicingConsumerType JobType
		{
			get
			{
				if (jobType == null)
				{
					if (PlugInData != null && PlugInData.InvoicingSupporter != null)
					{
						jobType = PlugInData.InvoicingSupporter.ConsumerType;
					}
					else if (GenericJobView != null)
					{
						jobType = GenericJobView.JobType;
					}
				}

				return jobType;
			}
		}

		JobInvoicingConsumerType jobType;

		public RefCountry Origin
		{
			get { return (PlugInData != null && PlugInData.InvoicingSupporter.Origin != null) ? PlugInData.InvoicingSupporter.Origin.Country : null; }
		}

		public ILocation FixedPlaceOfSupply
		{
			get
			{
				ILocation result = null;
				if (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value)
				{
					result = PlugInData != null ? PlugInData.InvoicingSupporter.FixedPlaceOfSupply : null;
				}
				return result;
			}
		}

		public RefCountry Destination
		{
			get { return (PlugInData != null && PlugInData.InvoicingSupporter.Destination != null) ? PlugInData.InvoicingSupporter.Destination.Country : null; }
		}

		public bool OverseasAgentIsApplicable
		{
			get { return JobType != null && JobType.OverseasAgentApplicable; }
		}

		internal BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (IsBrokerage)
				{
					return Factory.Load<BaseJobDeclaration>(JH_ParentID);
				}
				else
				{
					return BaseJobDeclaration.Load(Factory.Load<ForwardingShipment>(JH_ParentID));
				}
			}
		}

		public string ReasonNotToAllowPostCost
		{
			get
			{
				string result = "";

				if (Parent != null)
				{
					IJobInvoicingPlugIn parentAsInvoicingPlugIn = Parent as IJobInvoicingPlugIn;

					if (parentAsInvoicingPlugIn != null)
					{
						foreach (Charge charge in Charges)
						{
							if (charge.JR_AC.IsValid && charge.HasValidDataForCostPosting)
							{
								result = parentAsInvoicingPlugIn.InvoicingSupporter.GetReasonNotToAllowChangeOnCostDetails(charge.JR_AC);

								if (!string.IsNullOrEmpty(result))
								{
									break;
								}
							}
						}
					}
				}

				return result;
			}
		}

		public string ReasonNotToAllowPosting
		{
			get
			{
				string result = string.Empty;
				if (Parent != null)
				{
					IJobInvoicingPlugIn parentAsInvoicingPlugIn = Parent as IJobInvoicingPlugIn;
					if (parentAsInvoicingPlugIn != null)
					{
						result = parentAsInvoicingPlugIn.InvoicingSupporter.GetReasonNotToAllowPosting();
					}
				}
				return result;
			}
		}

		internal bool ConsumerTypeShouldCreateWIP(string invoiceType)
		{
			bool result = true;
			if (PlugInData != null && !IsPluginDataDeleted && PlugInData.InvoicingSupporter.ConsumerType != null)
			{
				result = PlugInData.InvoicingSupporter.ConsumerType.ShouldCreateWIPs(PlugInData, invoiceType);
			}
			else
			{
				if (JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix)
				{
					result = false;
				}
			}
			return result;
		}

		internal bool ConsumerTypeShouldCreateAccrual(string invoiceType)
		{
			bool result = true;
			if (PlugInData != null && !IsPluginDataDeleted && PlugInData.InvoicingSupporter.ConsumerType != null)
			{
				result = PlugInData.InvoicingSupporter.ConsumerType.ShouldCreateAccruals(PlugInData, invoiceType);
			}
			else
			{
				if (JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix)
				{
					result = false;
				}
			}
			return result;
		}

		internal bool IsConsumerTypeShouldCreateCostJRJ
		{
			get
			{
				if (PlugInData != null && !IsPluginDataDeleted && PlugInData.InvoicingSupporter.ConsumerType != null)
				{
					return PlugInData.InvoicingSupporter.ConsumerType.ShouldCreateCostJRJ(PlugInData);
				}
				else
				{
					return !(JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix);
				}
			}
		}

		internal bool IsConsumerTypeShouldCreateSellJRJ
		{
			get
			{
				if (PlugInData != null && !IsPluginDataDeleted && PlugInData.InvoicingSupporter.ConsumerType != null)
				{
					return PlugInData.InvoicingSupporter.ConsumerType.ShouldCreateSellJRJ(PlugInData);
				}
				else
				{
					return !(JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix);
				}
			}
		}

		#endregion

		#region Time Job Was Loaded

		public ZDateTime UtcTimeJobWasLoaded
		{
			get;
			set;
		}

		#endregion

		#region Job Closure Configuration

		public bool IsPastAllowedRestrictionDate()
		{
			var reopenRestirctionDate = TryToGetReOpenRestirctionDate();
			if (!reopenRestirctionDate.IsEmpty)
			{
				return Env.Time.CurrentLocalDate.Date >= reopenRestirctionDate.Date.ToDateTime();
			}

			return false;
		}

#if DEBUG
		public int TryToGetReOpenRestirctionDateCount_ForTestOnly;
#endif

		ZDateTime TryToGetReOpenRestirctionDate()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				TryToGetReOpenRestirctionDateCount_ForTestOnly++;
			}
#endif

			var reopenRestrictionOffset = ZDateTime.Empty;
			if (AutoJobClosureConfiguration != null && AutoJobClosureConfiguration.ReopenRestrictionOffset > 0)
			{
				var significantDate = AutoJobClosureHelper.TryToGetJobSignificantDate(this, AutoJobClosureConfiguration);
				reopenRestrictionOffset = AutoJobClosureHelper.GetThresholdDate(significantDate, AutoJobClosureConfiguration.ReopenRestrictionOffsetType, AutoJobClosureConfiguration.ReopenRestrictionOffset, 1);
			}
			return reopenRestrictionOffset;
		}

		public JobClosureConfiguration AutoJobClosureConfiguration =>
			jobClosureConfiguration ?? (jobClosureConfiguration = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(JH_GC, JobType?.Code ?? ZString.Empty, Direction, TransportMode, JH_GE, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close));
		JobClosureConfiguration jobClosureConfiguration;

		#endregion

		#region Helpers

		public bool HasActiveUnInvoicedCashAdvanceRequests
		{
			get
			{
				ResetCashAdvanceRequests();
				return CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>().Any(x => x.CAH_Status != CashAdvanceStatusCodes.RequestHeader.Cancelled && x.CAH_Status != CashAdvanceStatusCodes.RequestHeader.Invoiced);
			}
		}

		public bool ContainsUnpostedApportionment
		{
			get
			{
				foreach (Charge charge in Charges)
				{
					if (charge.JR_IsApportioned && !charge.IsCostPosted)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Charge UnpostedApportionmentForSpecificChargeCode(ZGuid chargeCodePK)
		{
			foreach (Charge chargeToTest in Charges)
			{
				if (chargeToTest.JR_AC.IsValid && chargeCodePK == chargeToTest.JR_AC &&
					chargeToTest.JR_E6.IsValid && !chargeToTest.JR_IsCostPosted)
				{
					return chargeToTest;
				}
			}
			return null;
		}

		public bool IsInvoiceOnHold
		{
			get { return JH_Status == JobHeaderStatus.InvoiceOnHold.Code; }
		}

		public bool IsWorkOnHold
		{
			get { return JH_Status == JobHeaderStatus.WorkOnHold.Code; }
		}

		public bool IsFreight
		{
			get { return JH_ParentTableCode == JobShipmentSchema.Constants.Prefix; }
		}

		public bool IsBrokerage
		{
			get { return JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix; }
		}

		public bool IsWarehousePeriodicBilling
		{
			get { return JobType != null && JobType.Code == JobInvoicingConsumerTypes.WarehouseStorage.Code; }
		}

		public bool HasDisbursementCharges
		{
			get
			{
				foreach (Charge charge in Charges)
				{
					if (charge.IsDisbursementCharge)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ZDecimal GetRevenueForChargeCode(AccChargeCode code)
		{
			ZDecimal jobRevenue = 0;
			if (code != null)
			{
				foreach (Charge aCharge in Charges)
				{
					if (aCharge.JR_AC == code.PK)
					{
						jobRevenue += aCharge.JR_LocalSellAmt;
					}
				}
			}
			return jobRevenue;
		}

		bool HasNonZeroCharges
		{
			get
			{
				foreach (Charge charge in Charges)
				{
					if (charge.JR_OSSellAmt != 0 || charge.JR_OSCostAmt != 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected JobInvoicingSecurityHelper SecurityHelper
		{
			get
			{
				if (fSecurityHelper == null && PlugInData != null)
				{
					fSecurityHelper = new JobInvoicingSecurityHelper(PlugInData.InvoicingSupporter.JobInvoicingSecurity);
				}
				return fSecurityHelper;
			}
		}
		JobInvoicingSecurityHelper fSecurityHelper;

		internal bool ChargesProfitShareAmountsReadOnly
		{
			get
			{
				return (!SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyProfitShareAmounts) ?? false) ||
					(IsReadyForRevenuePosting && IsReadyForCostPosting);
			}
		}

		internal
#if DEBUG
		virtual
#endif
		bool InvoicingAllowOverrideofCFX
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCFX) ?? false; }
		}

		internal bool InvoicingAllowOverrideSalesRep
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideSalesRep) ?? false; }
		}

		internal bool InvoicingAllowOverrideSellTaxId
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideSellTaxId) ?? false; }
		}

		internal bool InvoicingAllowOverrideCostTaxId
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostTaxId) ?? false; }
		}

		internal bool InvoicingAllowOverrideSellTaxMessage
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideSellTaxMsg) ?? false; }
		}

		internal bool InvoicingAllowOverrideCostTaxMessage
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostTaxMsg) ?? false; }
		}

		bool InvoicingAllowOverrideJobTaxBranch
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideJobTaxBranch) ?? false; }
		}

		internal bool InvoicingAllowOverrideCostTaxBranch
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideCostTaxBranch) ?? false; }
		}

		internal bool InvoicingAllowOverrideSellTaxBranch
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideSellTaxBranch) ?? false; }
		}

		string InvoicingAllowOverrideSellGovtChargeCodeSecurityCheckpointName => SecurityCore.AllowOverrideSellGovtCrgCode;

		public string InvoicingAllowOverrideSellGovtChargeCodeSecurityDisplayTextPath
		{
			get { return SecurityHelper?.GetInvSecurity(InvoicingAllowOverrideSellGovtChargeCodeSecurityCheckpointName)?.DisplayTextPathToSecurityRight ?? string.Empty; }
		}

		internal bool InvoicingAllowOverrideSellGovtChargeCode
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(InvoicingAllowOverrideSellGovtChargeCodeSecurityCheckpointName) ?? false; }
		}

		string InvoicingAllowOverrideCostGovtChargeCodeSecurityCheckpointName => SecurityCore.AllowOverrideCostGovtCrgCode;

		public string InvoicingAllowOverrideCostGovtChargeCodeSecurityDisplayTextPath
		{
			get { return SecurityHelper?.GetInvSecurity(InvoicingAllowOverrideCostGovtChargeCodeSecurityCheckpointName)?.DisplayTextPathToSecurityRight ?? string.Empty; }
		}

		internal bool InvoicingAllowOverrideCostGovtChargeCode
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(InvoicingAllowOverrideCostGovtChargeCodeSecurityCheckpointName) ?? false; }
		}

		public bool AllowOverridePostedTransactionDescriptionForJob
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.OverrideTransactionLineDescriptionForJob) ?? false; }
		}

		public string AllowOverridePostedTransactionDescriptionForJobErrorText
		{
			get { return SecurityHelper == null ? string.Empty : SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.OverrideTransactionLineDescriptionForJob); }
		}

		internal bool AllowOverrideEstimatedCost
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideEstimatedCost) ?? false; }
		}

		public bool AllowAppendToUnpostedTransactionDescriptionForJob
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob) ?? false; }
		}

		public string AllowAppendToUnpostedTransactionDescriptionForJobErrorText
		{
			get { return SecurityHelper == null ? string.Empty : SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob); }
		}

		internal bool AllowSaveNonZeroBalanceDisbursements
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.SaveNonZeroBalanceDisbursements) ?? false; }
		}

		internal bool AllowModifyDefaultChargeCodeDescription
		{
			get { return SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ModifyDefaultChargeCodeDescription) ?? false; }
		}

		internal ZString GetInvoiceTypeModule()
		{
			ZString result = ZString.Empty;
			JobInvoicingConsumerType jobType = JobType;
			if (jobType != null)
			{
				if (jobType == JobInvoicingConsumerTypes.Shipment
					|| jobType == JobInvoicingConsumerTypes.QuotedBooking
					|| jobType == JobInvoicingConsumerTypes.OneOffQuotation)
				{
					result = InvoiceTypeModuleList.Codes.FWD;
				}
				else if (jobType == JobInvoicingConsumerTypes.Brokerage)
				{
					result = InvoiceTypeModuleList.Codes.CUS;
				}
				else if (jobType == JobInvoicingConsumerTypes.CFSShipment ||
						 jobType == JobInvoicingConsumerTypes.CFSLoadList)
				{
					result = InvoiceTypeModuleList.Codes.CFS;
				}
				else if (jobType == JobInvoicingConsumerTypes.TransportBookingConsignment)
				{
					result = InvoiceTypeModuleList.Codes.TCN;
				}
				else if (jobType == JobInvoicingConsumerTypes.LocalCartage)
				{
					result = InvoiceTypeModuleList.Codes.TPT;
				}
				else if (jobType == JobInvoicingConsumerTypes.ImporterSecurityFiling)
				{
					result = InvoiceTypeModuleList.Codes.ISF;
				}
			}
			return result;
		}

		#endregion

		#region Load and Save

		public override void OnLoaded()
		{
			base.OnLoaded();
			fPreviousDepartment = JH_GE;
			UtcTimeJobWasLoaded = ZDateTime.UtcNow;
		}

		public bool IsPluginDeleted
		{
			get
			{
				BusinessObject pluginBizo = PlugInData as BusinessObject;
				return (pluginBizo != null && pluginBizo.IsDeleted);
			}
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || PlugInData == null || !IsPluginDeleted); }
		}

		protected override void SetDefaultBranchAndDepartmentIfNull()
		{
			//We first default to what the Registry set
			if (JH_GB.IsEmpty)
			{
				SetDefaultBranch(PlugInData);
			}

			if (JH_GE.IsEmpty)
			{
				SetDefaultDepartment(PlugInData);
			}

			// if it is still null (because the Registry set so), we then call the base's set default function
			if ((JH_GB.IsEmpty) || (JH_GE.IsEmpty))
			{
				base.SetDefaultBranchAndDepartmentIfNull();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public override void OnSaving()
		{
			base.OnSaving();

			if (Parent != null)
			{
				Parent.SetJobNumberFieldOnSaving();
				if (!(JH_JobNum == Parent.JobNumber || JH_JobNum == Parent.JobNumber + Core.Constants.GatewaySuffixForJobHeaderDeprecated))
				{
					JH_JobNum = Parent.JobNumber;
				}
			}

			if (JH_JobNum.IsEmpty)
			{
				var errorMessage = GenerateErrorReportStringForEmptyJobNumber();
				ErrorReporter.ReportOnce("JobHasNoJobNumberGenerated", errorMessage);
				// Adding a new job consumer type and see unit test failures here? Make sure IJobHeaderParent.SetJobNumberFieldOnSaving is implemented properly.
			}

			if (!JH_GE.IsValid)
			{
				ErrorReporter.ReportOnce("JobHasNoValidDepartment", "Job does not have a department.");
			}

			if (IsClosed)
			{
				ClearAllUnpostedCharges();
				ReverseAllAssociatedWIPsAndAccruals();
			}

			if (IsClosed && JH_A_JCLInfo.HasChanges && !JH_A_JCL.IsEmpty)
			{
				CreateOrQueueCommissions();
			}

			// Mark the one off quote that is used as "consumed" so it can't be used elsewhere
			Quote consumedQuote = OneOffQuote;
			if (consumedQuote != null && !consumedQuote.TH_IsOneOffQuoteConsumed)
			{
				consumedQuote.TH_IsOneOffQuoteConsumed = true;
			}

			HandleJobActiveInactiveLogs();
			HandleJobBranchChangeLogs();
			HandleJobOpenCloseLogs();
			HandleJobSalesRepLogs();

			if (!IsInDatabase)
			{
				UtcTimeJobWasLoaded = ZDateTime.UtcNow;

				ObjectFactory.Get<IElectronicProcessingChargeProvider>().CreateElectronicProcessingCharge(this);
			}

			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();

			var dsbJobClosingRestriction = JH_StatusInfo.HasChanges && JH_Status == JobHeaderStatus.Closed.Code
				&& !Factory.HasContext(BusinessContext.SavingDsbJobBatch)
				&& GetShouldJobBeClosedByDsbBatch();
			if (dsbJobClosingRestriction)
			{
				throw new ZCannotSaveException(Res.GetString("B2790110-1DE4-47CB-BA04-BDB9B2A60A12", @"Job {0} contains disbursement clearing charges and can only be closed via the Auto Job Closure process.", JH_JobNum), Res.GetString("1C0B93BE-59E6-4352-9CF4-89FCABDD76A1", "Cannot Save Job"));
			}
		}

		string GenerateErrorReportStringForEmptyJobNumber()
		{
			var errorMessageBuilder = new StringBuilder();

			try
			{
				errorMessageBuilder.AppendLine((NoResString)"Job does not have a job number. Job Info:");
				errorMessageBuilder.AppendLine($"\tPK = {PK}");
				errorMessageBuilder.AppendLine($"\tIsInDatabase = {IsInDatabase}");
				errorMessageBuilder.AppendLine($"\tIsDeleted = {IsDeleted}");
				errorMessageBuilder.AppendLine($"\tJH_ParentTableCode = {JH_ParentTableCode}");
				errorMessageBuilder.AppendLine($"\tJH_ParentID = {JH_ParentID}");

				if (Parent != null && !Parent.IsDeleted)
				{
					errorMessageBuilder.AppendLine((NoResString)"Job Parent Info:");
					errorMessageBuilder.AppendLine($"\tType = {Parent.GetType()}");
					errorMessageBuilder.AppendLine($"\tJobNumber = {Parent.JobNumber}");
					errorMessageBuilder.AppendLine($"\tIsInDatabase = {Parent.IsInDatabase}");
					errorMessageBuilder.AppendLine($"\tIsDeleted = {Parent.IsDeleted}");
				}

				errorMessageBuilder.AppendLine((NoResString)"Registry Key JobsNumberSequenceCustomisation:");
				var includedSequenceCustomisations = AccountingConfigurationRegistry.Instance.JobsNumberSequenceCustomisation.Value
					.Cast<TransactionNumberSequenceCustomisation>()
					.Where(x => x.Include == true)
					.ToArray();

				foreach (var customisation in includedSequenceCustomisations)
				{
					errorMessageBuilder.AppendLine($"\tElementName:\t{customisation.ElementName}");
					errorMessageBuilder.AppendLine($"\tOrder:\t{customisation.Order}");
					errorMessageBuilder.AppendLine($"\tCode:\t{customisation.Code}");
					errorMessageBuilder.AppendLine($"\tLength:\t{customisation.Length}");
					errorMessageBuilder.AppendLine($"\tFountain:\t{customisation.Fountain}");
					errorMessageBuilder.AppendLine($"\tDescription:\t{customisation.Description}");
				}

				var nextFountainValue = Env.NumberFountains.GetLocalJobRefNumberGeneratorFountain("JHJLR").GetNextFormatted(Factory);
				errorMessageBuilder.AppendLine($"Next Fountain Value for GetLocalJobRefNumberGeneratorFountain is:\t{nextFountainValue}");
			}
			catch (Exception ex)
			{
				errorMessageBuilder.AppendLine($"Unexpected Exception Occurred: {ex}");
			}

			return errorMessageBuilder.ToString();
		}

		public bool GetShouldJobBeClosedByDsbBatch()
		{
			var jobPkParam = ZSqlParameter.New("@JobPK", PK, JobHeaderSchema.PK);
			return AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value && Db.Connection.Exists((NoResString)"FROM ShouldJobBeClosedByDsbBatch (@JobPK)", (x) => x.AddParameter(jobPkParam));
		}

		protected override void OnFactorySavingCore()
		{
			base.OnFactorySavingCore();

			var enableDeferredRevenueRecognition = AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.GetFallBackValueAtAllLevels(JH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (enableDeferredRevenueRecognition && !JH_StatusInfo.HasChanges && (JH_Status == JobHeaderStatus.Complete.Code || JH_Status == JobHeaderStatus.Closed.Code || JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code))
			{
				string optionToExclude = (JH_Status == JobHeaderStatus.Complete.Code || JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code) ? RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure : null;
				if ((GetChargesToRecognizeCost(optionToExclude).Any() || GetChargesToRecognizeSell(optionToExclude).Any() || GetUnrecognizedTransactionLines(optionToExclude).Any()))
				{
					RecognizeRevenue(optionToExclude);
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore2()
		{
			base.OnFactorySavingBeforeTransactionCore2();

			if (!IsDeleted && JH_IsActive)
			{
				if (IsReopened)
				{
					SendJobReopenedEmailOnSaved = true;
				}

				if (JH_GE.IsEmpty)
				{
					JH_GE = GlbDepartment.CurrentDepartment.PK;
				}

				var chargesHaveChanged = HasChanges && Parent != null && fCharges != null && fCharges.IsLoaded && fCharges.HasChanges;

				if (!Factory.HasContext(BusinessContext.PostingChargesFromConsol) && chargesHaveChanged)
				{
					UpdateShipmentLevelProfitShare();
				}

				if (JH_Status != JobHeaderStatus.Closed.Code && chargesHaveChanged && revenueRecognition != null)
				{
					var nonDeletedCharges = fCharges.Where(x => !x.IsDeleted);
					var hasJobChargePosted = nonDeletedCharges.Any() && nonDeletedCharges.Any(x => x.IsRevenuePosted || x.IsCostPosted);

					if (hasJobChargePosted && IsChargePossiblyCreateNewWIPACR(nonDeletedCharges))
					{
						shouldRefreshRevenueRecognitionCollection = true;
					}
				}

				GatewaySellToCostSynchroniser.Synchronise(this);
			}
		}

		bool IsChargePossiblyCreateNewWIPACR(IEnumerable<Charge> charges)
		{
			return charges
				.Where(x => x.HasChanges && !x.IsRevenuePosted && !x.IsCostPosted && (x.ShouldCreateWIP || x.ShouldCreateAccrual))
				.Any(x =>
				{
					var isACRPossiblyBeEmpty = x.ShouldReverseAccrual || !x.JR_AL_APLine.IsValid;
					var isWIPPossiblyBeEmpty = x.ShouldReverseWIP || !x.JR_AL_ARLine.IsValid;

					return (isACRPossiblyBeEmpty && GetRevenueRecognitionDate(x.CostRecognition).IsEmpty)
					|| (isWIPPossiblyBeEmpty && GetRevenueRecognitionDate(x.SellRecognition).IsEmpty);
				});
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				RevenueRecognitionStatusSet = null;

				if (SendJobReopenedEmailOnSaved)
				{
					this.RunSingleActionPerTransaction(typeof(EmailNotification.JobReopenedEmail).ToString(), () => new EmailNotification.JobReopenedEmail(this, ReopenedUserPK, JH_GB).Send());
					SendJobReopenedEmailOnSaved = false;
				}

				if (Factory.HasContext(BusinessContext.InvoicingPlugInGUI))
				{
					Validation.ValidateJH_OA_AgentCollectAddr();
					Validation.ValidateJH_GS_NKRepSales();
				}
			}
		}

		protected override void OnFactorySavedCore(bool saveSucceeded)
		{
			base.OnFactorySavedCore(saveSucceeded);
			if (saveSucceeded)
			{
				ShouldRaiseErrorForNonZeroBalanceDisbursementChargesEvenIfNoChanges = false;
			}
		}

		public bool ShouldRaiseErrorForNonZeroBalanceDisbursementChargesEvenIfNoChanges { get; set; }

		void CreateOrQueueCommissions()
		{
			using (Factory.SetTempContext(BusinessContext.CalculateCommissionOnJobClosure))
			{
				var commissionCreator = ObjectFactory.Get<ICommissionCreatorProvider>().GetJobClosedCommissionCreator(this, JH_A_JCL);
				commissionCreator.PostQueueItemOrCreateCommissionsOnJobClosed();
			}
		}

		public void RegenerateCommissions(BusinessObjectFactory factoryForRegeneration, ILogger logger = null)
		{
			var commissionRegenerator = ObjectFactory.Get<ICommissionCreatorProvider>().GetCommissionRegenerator(this, factoryForRegeneration, logger);
			commissionRegenerator.RegenerateCommissions();
		}

		protected override void OnDeletedByDataRefresh()
		{
			base.OnDeletedByDataRefresh();

			OnJobDeactivatedByDataRefresh?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler OnJobDeactivatedByDataRefresh;

		/// <summary>
		/// Reverses all WIPs and Accruals related to the Job
		/// </summary>
		void ReverseAllAssociatedWIPsAndAccruals()
		{
			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, PK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);
			var collection = Factory.Load<BaseWIPAccrual>(new WIPAccrualCollection(Factory, filter).CompleteFilter);
			foreach (BaseWIPAccrual line in collection)
			{
				line.Reverse();
			}
		}

		/// <summary>
		/// Clears Revenue and Cost columns and clears links to WIPs and ACRs
		/// </summary>
		void ClearAllUnpostedCharges()
		{
			foreach (Charge charge in Charges)
			{
				if (charge.JR_JH == PK) // To skip charges from related jobs shown in the Charges collection
				{
					if (!charge.IsCostPosted)
					{
						charge.JR_OSCostAmt = 0;
						charge.JR_LocalCostAmt = 0;
						charge.ReverseAccrual(ZDateTime.Now);
					}
					if (!charge.IsRevenuePosted)
					{
						charge.JR_OSSellAmt = 0;
						charge.JR_LocalSellAmt = 0;
						charge.ReverseWIP(ZDateTime.Now);
					}
				}
			}
		}

		void HandleJobBranchChangeLogs()
		{
			if ((ZGuid)JH_GBInfo.OriginalValue != JH_GB)
			{
				GlbBranch originalBranch = Factory.Load<GlbBranch>((ZGuid)JH_GBInfo.OriginalValue);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format("Branch Changed: From '{0}' To '{1}'", originalBranch.GB_Code, Branch.GB_Code));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void HandleJobOpenCloseLogs()
		{
			var originalJobStatus = (ZString)JH_StatusInfo.OriginalValue;
			if (JH_Status != originalJobStatus || !IsInDatabase || IsJobActivating)
			{
				if (JH_Status == JobHeaderStatus.Closed.Code)
				{
					Logs.AddNew(Events.JobClose);
				}
				else if (IsJobActivating || !IsInDatabase || originalJobStatus == JobHeaderStatus.Closed.Code)
				{
					Logs.AddNew(Events.JobOpen);
				}
			}
		}

		void HandleJobSalesRepLogs()
		{
			if ((ZString)JH_GS_NKRepSalesInfo.OriginalValue != JH_GS_NKRepSales)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format("Sales Rep Changed: From '{0}' To '{1}'",
					(ZString)JH_GS_NKRepSalesInfo.OriginalValue, JH_GS_NKRepSales));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void HandleJobActiveInactiveLogs()
		{
			if ((ZBool)JH_IsActiveInfo.OriginalValue != JH_IsActive)
			{
				if (JH_IsActive)
				{
					Logs.AddNew(Events.BillingJobEdit, string.Format(CultureInfo.InvariantCulture, "Job marked as Active"));
				}
				else
				{
					Logs.AddNew(Events.BillingJobEdit, string.Format(CultureInfo.InvariantCulture, "Job marked as Inactive"));
				}
			}
		}

		GlbStaff GetUserFromSecurity()
		{
			var userPk = SecurityOverrideProvider != null && SecurityOverrideProvider.UserSecurityOverride != null
				? SecurityOverrideProvider.UserSecurityOverride.UserPK
				: ZGuid.Empty;
			return userPk.IsValid ? Factory.Load<GlbStaff>(userPk) : null;
		}

		protected virtual void SetReopenedUser()
		{
			if (!IsReopened)
			{
				return;
			}

			var staff = GetUserFromSecurity();
			if (staff == null)
			{
				return;
			}

			ReopenedUserPK = staff.PK;
			Logs.AddNew(Events.Authorised, $"Job reopening authorized by {staff.GS_LoginName} ({staff.GS_FullName})");
		}

		ZGuid ReopenedUserPK { get; set; }

		bool IsReopened
		{
			get
			{
				var originalJobStatus = (ZString)JH_StatusInfo.OriginalValue;
				return JH_Status != originalJobStatus && originalJobStatus == JobHeaderStatus.Closed.Code;
			}
		}

		bool SendJobReopenedEmailOnSaved { get; set; }

		#region Profit Share Adjustments

		public void ClearProfitShareReferences()
		{
			if (JH_IsProfitSharePosted)
			{
				JH_IsProfitSharePosted = false;
				JH_ProfitShareInvoice = ZGuid.Empty;
			}
		}

		void UpdateShipmentLevelProfitShare()
		{
			if (AccountingConfigurationRegistry.Instance.ProfitShareUpdateOnSave.Value && ProfitShareSuspendOnSaveCount <= 0 && fCharges != null)
			{
				ZQuery query = new ZQuery(JobChargeSchema.JR_AC, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
				query.AddToFilter(JobChargeSchema.JR_E6, null);
				bool shipmentLevelProfitShareChargeExists = Charges.Find(query).Length > 0;

				if (shipmentLevelProfitShareChargeExists)
				{
					using (SuspendCreatingProfitShareOnSave())
					{
						var profitSharesCalculator = new ProfitShareCalculator(Factory, PlugInData);
						var profitShares = profitSharesCalculator.CreateProfitShares();

						new ProfitShareShipmentChargeCreator(profitShares, this).CreateCharges();
					}
				}
			}
		}

		internal IDisposable SuspendCreatingProfitShareOnSave()
		{
			return new CreateProfitShareOnSaveSuspender(this);
		}

		class CreateProfitShareOnSaveSuspender : IDisposable
		{
			public CreateProfitShareOnSaveSuspender(Job job)
			{
				this.Job = job;
				job.ProfitShareSuspendOnSaveCount++;
			}

			readonly Job Job;

			public void Dispose()
			{
				Job.ProfitShareSuspendOnSaveCount--;
			}
		}

		int ProfitShareSuspendOnSaveCount;

		#endregion

		public void DeleteUnpostedLines()
		{
			ArrayList toBeRemoved = new ArrayList();

			foreach (Charge aCharge in Charges)
			{
				if (!aCharge.IsCostPosted && !aCharge.IsRevenuePosted)
				{
					toBeRemoved.Add(aCharge);
				}
			}

			using (Charges.SuspendListChanged())
			{
				foreach (Charge aCharge in toBeRemoved)
				{
					Charges.RemoveAndDelete(aCharge);
				}
			}
		}

		public void ResetUnpostedLinesInvoiceType()
		{
			foreach (Charge aCharge in Charges)
			{
				if (!aCharge.IsRevenuePosted)
				{
					InvoiceTypeCalculator calc = new InvoiceTypeCalculator(aCharge);
					calc.UpdateInvoiceType();
				}
			}
		}

		#region Reset Default Debtor On Unposted Lines

		public void ResetDefaultDebtorOnUnpostedLines()
		{
			foreach (Charge charge in Charges)
			{
				if (!charge.IsRevenuePosted)
				{
					charge.ResetChargeDebtor();
				}
			}
		}

		#endregion

		void SetBranchIfEmptyForChargesAndLines()
		{
			bool hasNonZeroCharges = HasNonZeroCharges;
			foreach (Charge charge in Charges)
			{
				if (charge.JR_GB.IsEmpty || !hasNonZeroCharges)
				{
					using (IsSettingHasChangesSuspended ? charge.SuspendSettingHasChanges() : null)
					{
						charge.JR_GB = JH_GB;
					}
				}
			}
		}

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				if (JH_Status != (ZString)JH_StatusInfo.OriginalValue && (JH_Status == JobHeaderStatus.Closed.Code || JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code) && CustomLogReferenceSuffixExtraDetails != null)
				{
					var extraDetailsAsSingleLine = string.Join(" ", CustomLogReferenceSuffixExtraDetails.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
					var customLogReferenceSuffixJoinedText = string.Join(" ", new[] { DefaultCustomLogReferenceSuffix, extraDetailsAsSingleLine }.Where(x => !string.IsNullOrEmpty(x)));
					var customLogReferenceSuffixTruncatedText = customLogReferenceSuffixJoinedText.Substring(0, Math.Min(customLogReferenceSuffixJoinedText.Length, StmALog.Schema.SL_ReferenceMaxLength));
					return customLogReferenceSuffixTruncatedText;
				}
				else
				{
					return DefaultCustomLogReferenceSuffix;
				}
			}
		}

		string DefaultCustomLogReferenceSuffix
		{
			get
			{
				var originalJobStatus = (ZString)JH_StatusInfo.OriginalValue;
				return JH_Status != originalJobStatus
					? Res.GetString(
						"0201441B-16B0-4F4C-8D3B-E085684CB209",
						"Job status changed from {0} to {1}{2}.",
						originalJobStatus,
						JH_Status,
						reopenLogInfo
					)
					: string.Empty;
			}
		}

		public string CustomLogReferenceSuffixExtraDetails { get; set; }

		#endregion

		#region Delete

		public ZString CheckIfCanDeleteJobInCurrentCompany()
		{
			var tableName = ZString.Empty;

			if (HasSavedNonZeroJobCharges)
			{
				tableName = JobChargeSchema.Constants.TableName;
			}
			else if (HasSavedTransactionLines)
			{
				tableName = AccTransactionLinesSchema.Constants.TableName;
			}
			else if (HasSavedTransactionHeaders)
			{
				tableName = AccTransactionHeaderSchema.Constants.TableName;
			}
			else if (HasSavedHotCheques)
			{
				tableName = AccHotChequeSchema.Constants.TableName;
			}
			else if (HasSavedChildJobHeader)
			{
				tableName = JobHeaderSchema.Constants.TableName;
			}

			return !tableName.IsEmpty
				? JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(tableName, Res.GetString("56c134f6-06cb-4665-8caa-12fc7ee5da58", "This job"))
				: string.Empty;
		}

		public bool HasTransactions
		{
			get { return HasSavedNonZeroJobCharges || HasSavedTransactionLines || HasSavedTransactionHeaders || HasSavedHotCheques; }
		}

		bool HasSavedNonZeroJobCharges
		{
			get
			{
				var filter = GetSavedNonZeroJobChargesQuery();

				return Factory.LoadTop1<JobCharge>(filter) != null;
			}
		}

		ZQuery GetSavedNonZeroJobChargesQuery()
		{
			var filter = new ZQuery(JobChargeSchema.JR_JH, PK);

			var nonZeroAmountsFilter = new ZQuery(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.DefaultJoinCondition = JoinCondition.Or;
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_LocalCostAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_EstimatedCost, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_AgentDeclaredCostAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_OSSellAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_LocalSellAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_EstimatedRevenue, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			nonZeroAmountsFilter.AddToFilter(JobChargeSchema.JR_AgentDeclaredSellAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);

			filter.AddToFilter(nonZeroAmountsFilter, JoinCondition.And);

			return filter;
		}

		bool HasSavedTransactionLines
		{
			get
			{
				ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_JH, PK);
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);
				return Factory.LoadTop1<AccTransactionLines>(filter) != null;
			}
		}

		bool HasSavedTransactionHeaders
		{
			get
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_JH, PK);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, JH_GC);
				return Factory.LoadTop1<AccTransactionHeader>(filter) != null;
			}
		}

		bool HasSavedHotCheques
		{
			get
			{
				ZQuery filter = new ZQuery(AccHotChequeSchema.AQ_JH, PK);
				return Factory.LoadTop1<AccHotCheque>(filter) != null;
			}
		}

		bool HasSavedChildJobHeader
		{
			get
			{
				ZQuery filter = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, PK);
				return Factory.LoadTop1<JobHeader>(filter) != null;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				DeleteRelatedData();
			}
			base.Delete();
			if (OnJobDeleted != null)
			{
				OnJobDeleted(this, EventArgs.Empty);
			}
		}

		public event EventHandler OnJobDeleted;

		void DeleteRelatedData()
		{
			Charges.IsManagedForDataRefresh = false;
			DeleteUnpostedLines();
			ExchangeRates.RemoveAndDeleteAll();
			foreach (ProcessTask task in Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, PK)))
			{
				task.Delete();
			}
			if (!IsDeleted && JH_IsActive)
			{
				GatewaySellToCostSynchroniser.Synchronise(this);
			}

			if (!this.HasContext(BusinessContext.InterCompanyJobOperation))
			{
				GetSalesRepDefaultingSupporterIsApplicable()?.NotifyControllingCustomerChanged(null);
			}
		}

		#endregion

		#region Mark Job as Inactive

		public override void MarkAsInactive()
		{
			if (!IsDeleted && !base.IsCancelled)
			{
				DeleteRelatedData();
			}
			base.MarkAsInactive();
			if (!IsDeleted && base.IsCancelled)
			{
				DeleteUnpostedLines();
			}
			OnJobDeactivated?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler OnJobDeactivated;

		public ZString CheckIfCanDeactiveJobInCurrentCompany(string jobNumber = "")
		{
			var businessObjectReadableName = string.IsNullOrEmpty(jobNumber) ? Res.GetString("FC1D76F7-ADD7-4045-A77A-1FA1ABAC6D1D", "This job") : Res.GetString("69608FA5-ECFF-423B-A03F-8A419D36EFE8", "This job: {0}", jobNumber);
			if (AccountingConfigurationRegistry.Instance.JobDeactivationConfiguration.Value == JobDeactivationConfigurations.NoActiveWIPACR.Code)
			{
				var category = GetCategoryForCannotDeactiveJob();

				return category != JobHeaderParentDeletionHelper.CannotDeactiveJobReason.EmptyReason
					? JobHeaderParentDeletionHelper.GetJobNoDeactiveErrorMessage(category, businessObjectReadableName, JH_GC.ToGuid())
					: string.Empty;
			}
			else
			{
				var tableName = GetTableNameForCannotDeactiveJob();

				return !tableName.IsEmpty
					? JobHeaderParentDeletionHelper.GetJobNoDeactiveErrorMessage(tableName, businessObjectReadableName)
					: string.Empty;
			}
		}

		ZString GetTableNameForCannotDeactiveJob()
		{
			var tableName = ZString.Empty;

			if (HasSavedNonZeroJobCharges)
			{
				tableName = JobChargeSchema.Constants.TableName;
			}
			else if (HasSavedTransactionLines)
			{
				tableName = AccTransactionLinesSchema.Constants.TableName;
			}
			else if (HasSavedTransactionHeaders)
			{
				tableName = AccTransactionHeaderSchema.Constants.TableName;
			}
			else if (HasSavedHotCheques)
			{
				tableName = AccHotChequeSchema.Constants.TableName;
			}

			return tableName;
		}

		JobHeaderParentDeletionHelper.CannotDeactiveJobReason GetCategoryForCannotDeactiveJob()
		{
			var globalCharge = Factory.Load<AccChargeCode>(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value);
			var chargeInCurrentCompany = globalCharge?.ChildChargeCodes?.FirstOrDefault(c => c.AC_GC == JH_GC);

			if (HasPostedTransactionForNPL(chargeInCurrentCompany))
			{
				return JobHeaderParentDeletionHelper.CannotDeactiveJobReason.HasPostedTransactions;
			}

			if (HasSavedNonZeroJobChargesForNPL(chargeInCurrentCompany))
			{
				return JobHeaderParentDeletionHelper.CannotDeactiveJobReason.HasSavedNonZeroJobCharges;
			}

			if (HasNotReversedWIPACRForNPL)
			{
				return JobHeaderParentDeletionHelper.CannotDeactiveJobReason.HasNotReversedWIPACR;
			}

			if (HasSavedHotCheques)
			{
				return JobHeaderParentDeletionHelper.CannotDeactiveJobReason.HasSavedHotCheques;
			}

			return JobHeaderParentDeletionHelper.CannotDeactiveJobReason.EmptyReason;
		}

		bool HasPostedTransactionForNPL(AccChargeCode electronicProcessingChargeCode)
		{
			var headerFilter = new ZQuery(AccTransactionHeaderSchema.AH_JH, PK);
			headerFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, JH_GC);
			headerFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.JobRevenueJournal);

			var hasNoneJRJHeader = Factory.LoadTop1<AccTransactionHeader>(headerFilter) != null;

			var lineFilter = new ZQuery(AccTransactionLinesSchema.AL_JH, PK);
			lineFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);
			lineFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.Accrual);
			lineFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.WIP);
			var lines = Factory.Load<AccTransactionLines>(lineFilter);

			var hasNoneWIPACRLines = lines?.Any(x => !((x.ChargeCode?.PK ?? ZGuid.Empty) == electronicProcessingChargeCode?.PK && (x.TransactionHeader?.AH_TransactionType ?? ZString.Empty) == TransactionTypes.JobRevenueJournal)) ?? false;

			return hasNoneJRJHeader || hasNoneWIPACRLines;
		}

		bool HasSavedNonZeroJobChargesForNPL(AccChargeCode electronicProcessingChargeCode)
		{
			var filter = GetSavedNonZeroJobChargesQuery();
			filter.AddToFilter(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, electronicProcessingChargeCode?.PK ?? ZGuid.Empty);

			return Factory.LoadTop1<JobCharge>(filter) != null;
		}

		bool HasNotReversedWIPACRForNPL
		{
			get
			{
				var filter = new ZQuery(AccTransactionLinesSchema.AL_JH, PK);
				filter.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);
				filter.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);

				var noneWIPACRFiter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);
				noneWIPACRFiter.DefaultJoinCondition = JoinCondition.Or;
				noneWIPACRFiter.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
				filter.AddToFilter(noneWIPACRFiter, JoinCondition.And);

				return Factory.LoadTop1<AccTransactionLines>(filter) != null;
			}
		}

		#endregion

		#region Rating Override Suppressor

		class RatingOverrideSuppressor : IDisposable
		{
			public RatingOverrideSuppressor(Job job)
			{
				this.job = job;
				job.ratingOverrideSuppressorIncursionCount++;
			}

			readonly Job job;

			void IDisposable.Dispose()
			{
				job.ratingOverrideSuppressorIncursionCount--;
			}
		}

		public IDisposable SuppressAutoRatingOverride()
		{
			return new RatingOverrideSuppressor(this);
		}

		internal ZBool AutoRatingOverrideSuppressed
		{
			get { return ratingOverrideSuppressorIncursionCount > 0; }
		}
		int ratingOverrideSuppressorIncursionCount;

		#endregion

		#region Properties

		public IEnumerable<AccTransactionLines> GetPostedARAPLines(ZGuid orgPK = default(ZGuid))
		{
			var chargeCostQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_APLine);

			var chargeSellQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_ARLine);
			chargeCostQuery.AddAsUnionQuery(chargeSellQuery);

			var postedFilter = new ZDBOnlyQuery(typeof(AccTransactionLines));
			postedFilter.AddToFilter(JoinCondition.Or, AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, AccTransactionLines.CostLineTypes);
			postedFilter.AddToFilter(JoinCondition.Or, AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, AccTransactionLines.RevenueLineTypes);

			var postedLinesQuery = new ZDBOnlyQuery(typeof(AccTransactionLines));
			postedLinesQuery.AddSubQuery(chargeCostQuery, JoinCondition.And);
			postedLinesQuery.AddToFilter(postedFilter);
			postedLinesQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, PK);
			postedLinesQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);

			if (orgPK.IsValid)
			{
				postedLinesQuery.AddToFilter(AccTransactionLinesSchema.AL_OH, orgPK);
			}

			return Factory.Load<AccTransactionLines>(postedLinesQuery);
		}

		public int WeightDecimals => DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits;

		public int UnitDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits;

		public bool IsAnyCostOrRevenuePosted(ZGuid orgPK = default(ZGuid))
		{
			return GetPostedARAPLines(orgPK).Any();
		}

		public bool IsAnyRevenuePosted(ZGuid orgPK = default(ZGuid))
		{
			var lines = GetPostedARAPLines(orgPK);
			if (lines == null)
			{
				return false;
			}

			return lines.Any(x => x.AL_LineType == TransactionLineTypes.Revenue);
		}

		public bool IsImport
		{
			get { return MovementDirection == Directions.Import; }
		}

		public bool IsExport
		{
			get { return MovementDirection == Directions.Export; }
		}

		public bool IsDomestic
		{
			get { return MovementDirection == Directions.Domestic; }
		}

		public bool IsCrossTrade
		{
			get { return MovementDirection == Directions.CrossTrade; }
		}

		public bool HasOrphanWIPsorAccruals
		{
			get { return Job.HasOrphanWIPsorAccrualsForJobs(Factory, this.PK); }
		}

		public static bool HasOrphanWIPsorAccrualsForJobs(BusinessObjectFactory factory, params ZGuid[] jobPks)
		{
			return Job.GetOrphanWIPsOrAccruals(factory, jobPks);
		}

		static bool GetOrphanWIPsOrAccruals(BusinessObjectFactory factory, params ZGuid[] jobPks)
		{
			ZDBOnlyQuery query = AccountingUtils.GetOrphanWIPsOrAccrualsFilterQuery();
			query.AddToFilter(AccTransactionLinesSchema.AL_JH, jobPks);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);

			return factory.LoadTop1<AccTransactionLines>(query) != null;
		}

		public bool JobHasParent()
		{
			var plugIn = PlugInData;
			if (plugIn == null && !JH_ParentID.IsEmpty && !JH_ParentTableCode.IsEmpty)
			{
				plugIn = LoadJobInvoicingPlugIn();
			}
			return plugIn != null && !plugIn.IsDeleted;
		}

		public static string GetJobDoesNotHaveParentMessage()
		{
			return Res.GetString("22FFD022-2F17-4F67-AE05-495A6BEAD684", "Cannot find corresponding operation job.\r\nYou can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent", Core.Constants.ProductName);
		}

		#region ReadPivots

		void TryAssignClient(ZGuid bookingPK)
		{
			if (Parent.TableName == JobShipmentSchema.Constants.TableName)
			{
				var client = Factory.Load<OrgHeader>(ClientShipmentTmpLink.Consume(bookingPK, Factory));
				if (client != null)
				{
					JH_OA_LocalChargesAddr = client.MainAddress.PK;
				}
			}
		}

		void TryAssignQuoteNumber()
		{
			if (Parent.TableName == JobShipmentSchema.Constants.TableName)
			{
				var shipment = Parent as CommonShipment;
				if (shipment != null && !shipment.JS_TH_OneTimeQuote.IsEmpty)
				{
					var quoteQuery = new ZQuery(RatingHeaderSchema.PK, shipment.JS_TH_OneTimeQuote);
					quoteQuery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
					var quote = Factory.LoadTop1<Quote>(quoteQuery);

					if (quote != null)
					{
						JH_TH_NKQuoteNumber = quote.TH_QuoteNumber;
					}
				}
			}
		}

		#endregion

		public ZDateTime JH_JS_HouseBillIssueDate => CommonShipment?.JS_HouseBillIssueDate ?? ZDateTime.Invalid;

		CommonShipment CommonShipment
		{
			get
			{
				if (fCommonShipment == null)
				{
					if (Parent is CommonShipment shipment)
					{
						fCommonShipment = shipment;
					}
					else if (Parent is QuotedBooking quotedBooking)
					{
						fCommonShipment = quotedBooking.Booking;
					}
				}

				return fCommonShipment;
			}
		}
		CommonShipment fCommonShipment;

		#region JH_JS_OrderReferences

		public ZString JH_JS_OrderReferences => CommonShipment != null ? CommonShipment.JS_OrderReferences : ZString.Empty;

		public ZPropertyInfo JH_JS_OrderReferencesInfo
		{
			get { return GetZPropertyInfo(Schema.JH_JS_OrderReferences); }
		}

		#endregion

		#region Vessel

		public ZString JH_JS_JK_Vessel
		{
			get
			{
				var shipment = Parent as CommonShipment;
				return shipment != null ? shipment.JS_JK_Vessel : ZString.Empty;
			}
		}

		public ZPropertyInfo JH_JS_JK_VesselInfo
		{
			get { return GetZPropertyInfo(Schema.JH_JS_JK_Vessel); }
		}

		#endregion

		#region Voyage

		public ZString JH_JS_JK_VoyageFlight
		{
			get
			{
				var shipment = Parent as CommonShipment;
				return shipment != null ? shipment.JS_JK_VoyageFlight : ZString.Empty;
			}
		}

		public ZPropertyInfo JH_JS_JK_VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JH_JS_JK_VoyageFlight); }
		}

		#endregion

		#region Additional Reference

		public ZString AdditionalReferenceAsString
		{
			get
			{
				var result = ZString.Empty;

				if (CommonShipment != null)
				{
					result = CommonShipment.NumbersAsString;
				}
				else if (JobDeclaration != null)
				{
					result = JobDeclaration.AdditionalReferenceNumbers.AllNumbersAsString;
				}

				return result;
			}
		}

		public ZPropertyInfo AdditionalReferenceAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.AdditionalReferenceAsString); }
		}

		public CusEntryNumAdditionalReferenceCollection AdditionalReferences
		{
			get
			{
				CusEntryNumAdditionalReferenceCollection result = null;
				if (CommonShipment != null)
				{
					result = CommonShipment.Numbers;
				}
				else if (JobDeclaration != null)
				{
					result = JobDeclaration.AdditionalReferenceNumbers;
				}
				else
				{
					result = new CusEntryNumAdditionalReferenceCollection(this);
				}
				return result;
			}
		}

		#endregion

		#region JH_TH_NKQuoteNumber

		//[List("Quotes")]		// until "[CodeProperty("QuoteNumber")" will be fixed in ViewQuotedBooking.
		public override ZString JH_TH_NKQuoteNumber
		{
			get { return base.JH_TH_NKQuoteNumber; }
			set
			{
				if (JH_TH_NKQuoteNumber != value)
				{
					base.JH_TH_NKQuoteNumber = value;
					CopyChargesFromSpotQuote();
				}
			}
		}

		protected override void CopyChargesFromSpotQuoteCore()
		{
			var filter = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, JH_TH_NKQuoteNumber);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);
			filter.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);

			var quote = Factory.LoadTop1<Quote>(filter);
			if (quote != null && !JH_TH_NKQuoteNumberInfo.HasErrors())
			{
				CopyJobBillingInformation(
					jobHeaderParent: quote,
					skipTransactionInfo: false,
					overrideComment: false,
					resetGSTTaxDefault: true,
					copyExchangeRates: true,
					copyRateAudit: true);

				UpdateExchangeRates(false);
			}
		}

		public override void CopyJobBillingInformation(IJobHeaderParent jobHeaderParent, bool skipTransactionInfo, bool overrideComment, bool resetGSTTaxDefault, bool copyExchangeRates, bool copyRateAudit)
		{
			if (jobHeaderParent == null)
			{
				return;
			}

			var job = new Loader(jobHeaderParent).Load();

			if (job == null || job == this)
			{
				return;
			}

			Factory.SetContext(BusinessContext.CopyChargePersistentValues);
			try
			{
				JH_OA_LocalChargesAddr = job.JH_OA_LocalChargesAddr;
				if (RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.Value)
				{
					JH_OA_AgentCollectAddr = job.JH_OA_AgentCollectAddr;
				}

				if (job.Charges.Count > 0)
				{
					if (QuerySpotQuoteChargesExist())
					{
						if (copyExchangeRates)
						{
							foreach (ExchangeRate exRate in job.ExchangeRates)
							{
								var newExRate = ExchangeRates.AddNew();
								AddSettingHasChangesOnAllChildrenSuspenders_ExchangeRate(newExRate);
								newExRate.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(exRate);
								newExRate.JF_IsTransformed = true;
								newExRate.RunPreSaveValidation();
							}
						}

						CopyCompanyTariffLevelOverrideFromQuote(jobHeaderParent);

						CopyJobCharges(jobHeaderParent, job, skipTransactionInfo, overrideComment, resetGSTTaxDefault, copyRateAudit);
					}
				}
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.CopyChargePersistentValues);
			}

			JH_GB = job.JH_GB;
			JH_GE = job.JH_GE;
		}

		void CopyJobCharges(IJobHeaderParent jobHeaderParent, Job job, bool skipTransactionInfo, bool overrideComment, bool resetGSTTaxDefault, bool copyRateAudit)
		{
			foreach (Charge charge in job.Charges)
			{
				if (jobHeaderParent is IChargeApplicableForCopy chargeParent && !chargeParent.IsApplicable(charge))
				{
					continue;
				}

				var newCharge = Charges.AddNew();
				AddSettingHasChangesOnAllChildrenSuspenders_Charge(newCharge);

				_Rating.MarkAsBeingCopiedFromQuote(newCharge);
				newCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(charge);

				if (skipTransactionInfo)
				{
					newCharge.JR_APInvoiceNum = ZString.Empty;
					newCharge.JR_APInvoiceDate = ZDateTime.Empty;
					newCharge.JR_APDocumentReceivedDate = ZDateTime.Empty;
					newCharge.JR_PaymentDate = ZDateTime.Empty;
					newCharge.JR_PaymentType = ZString.Empty;
					newCharge.JR_AB = ZGuid.Empty;
					newCharge.JR_AK = ZGuid.Empty;
					newCharge.JR_ChequeNo = ZString.Empty;
					newCharge.JR_CostReference = ZString.Empty;

					newCharge.JR_CostTaxDate = ZDate.Empty;
					newCharge.JR_SellTaxDate = ZDate.Empty;

					newCharge.JR_CostRated = false;
					newCharge.JR_SellRated = false;
				}

				newCharge.JR_OrderReference = JH_JobNum;

				if (resetGSTTaxDefault)
				{
					newCharge.ResetSellGSTTaxDefault();
					newCharge.ResetCostGSTTaxDefault();
				}

				newCharge.UpdateOsSellWHTAmount();
				newCharge.UpdateOsCostGSTAmount();
				newCharge.UpdateOsCostWHTAmount();
				newCharge.JR_SellRatingOverride = DataRegistryRating.Instance.PreserveQuoteRevenueRatingBehaviour.Value ? charge.JR_SellRatingOverride : true;
				newCharge.JR_CostRatingOverride = charge.JR_CostRatingOverride;

				if (overrideComment)
				{
					string ratingOverrideComment = Res.GetString("f4909626-70b8-4edb-89d7-909003be2679", "Copied from (Quote {0})", jobHeaderParent.JobNumber);

					if (string.IsNullOrEmpty(newCharge.JR_CostRatingOverrideComment))
					{
						newCharge.JR_CostRatingOverrideComment = ratingOverrideComment;
					}

					if (string.IsNullOrEmpty(newCharge.JR_SellRatingOverrideComment))
					{
						newCharge.JR_SellRatingOverrideComment = ratingOverrideComment;
					}
				}

				if (copyRateAudit)
				{
					newCharge.RevenueCalculationDescription = charge.RevenueCalculationDescription;
					newCharge.CostCalculationDescription = charge.CostCalculationDescription;
				}

				newCharge.RunPreSaveValidation();
				if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value &&
					AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value && newCharge.ShouldCreateWIP)
				{
					newCharge.MarkAsNeedingValidation();
				}

				newCharge.SetProFormaRevenueAndCost();
			}
		}

		#endregion

		#region JH_GC

		public override ZGuid JH_GC
		{
			get { return base.JH_GC; }
			set
			{
				if (base.JH_GC != value)
				{
					base.JH_GC = value;
					if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
					{
						using (ChargesLoadSuspender.GetSuspender())
						{
							Charges.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		#endregion //JH_GC

		#region JH_GB

		[List("Branches")]
		public override ZGuid JH_GB
		{
			get { return base.JH_GB; }
			set
			{
				bool hasChanged = JH_GB != value;

				base.JH_GB = value;
				PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_GBInfo);
				if (hasChanged)
				{
					SetBranchIfEmptyForChargesAndLines();
				}
			}
		}

		#endregion

		#region JH_GE

		[List("Departments")]
		public override ZGuid JH_GE
		{
			get { return base.JH_GE; }
			set
			{
				if (JH_GE != value)
				{
					base.JH_GE = value;
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_GEInfo);
					if (Department != null)
					{
						if (fPreviousDepartment.IsEmpty || Charges.Count == 0)
						{
							fPreviousDepartment = Department.PK;
						}
						SetDefaultsForJob();
						AddDepartmentCharges();
					}
				}
				else
				{
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_GEInfo);
				}
			}
		}

		ZGuid fPreviousDepartment;

		public ZGuid PreviousDeptPK
		{
			get { return fPreviousDepartment; }
		}

		GlbDepartment PreviousDepartmentBizO
		{
			get
			{
				return Factory.Load<GlbDepartment>(fPreviousDepartment);
			}
		}

		public ZString PreviousDeptCode
		{
			get { return PreviousDepartmentBizO != null ? PreviousDepartmentBizO.GE_Code : ZString.Empty; }
		}

		public void AddDepartmentCharges()
		{
			GlbDepartment dept = Factory.Load<GlbDepartment>(JH_GE);

			if (dept != null && !Factory.HasContext(BusinessContext.CopyChargePersistentValues))
			{
				using (Charges.SuspendListChanged())
				{
					foreach (GlbDeptCharges departmentCharge in dept.DeptCharges)
					{
						if (Charges.ContainsChargeCode(departmentCharge.ChargeCode) == null)
						{
							Charge newCharge = Charges.AddNew();
							using (IsSettingHasChangesSuspended ? newCharge.SuspendSettingHasChanges() : null)
							{
								newCharge.JR_AC = departmentCharge.GD_AC;
							}
						}
					}
				}
			}
		}

		#endregion

		#region JH_OA_LocalCharges

		[List("Debtors")]
		public override ZGuid LocalChargesPK
		{
			get { return base.LocalChargesPK; }
			set { base.LocalChargesPK = value; }
		}

		[List("Debtors")]
		public override ZGuid JH_OA_LocalChargesAddr
		{
			get { return base.JH_OA_LocalChargesAddr; }
			set
			{
				if (JH_OA_LocalChargesAddr != value)
				{
					var oldValue = LocalChargesPK;
					base.JH_OA_LocalChargesAddr = value;
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_OA_LocalChargesAddrInfo);
					if (oldValue != LocalChargesPK)
					{
						ResetDebtorOnCharges(oldValue, LocalChargesPK);
						SetDefaultBranch(PlugInData);
						SetDefaultValuesForSalesRep();
					}

					Charges.Where(charge => charge.JR_OH_SellAccount == LocalChargesPK).ToList().ForEach(charge => charge.JR_OA_SellInvoiceAddressInfo.RefreshBinding());
				}
				else
				{
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_OA_LocalChargesAddrInfo);
				}
			}
		}

		public ResourceStringData JH_OA_LocalChargesAddrCaption => IsGatewayBillingJob()
			? Res.GetData("f0537ef1-77af-4c43-8f59-2edb36a3ce69", "Prepaid Agent", "Agent who collects revenue for this gateway billing job and/or handles the consol at origin.")
			: Res.GetData("JobChargeUserControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Local Client");

		public ResourceStringData PrepaidBillToPartyCaption => Res.GetData("F9213B3B-2330-427A-A241-3A21C59C8990", "Prepaid Bill-To Party", "This field defaults the job's Consignor. If Cross Trade jobs are not configured to bill the job's Controlling Party, then the Prepaid Bill-To Party (or their IFT party if they have one) will default as the charge line debtor for prepaid charges on this job.");

		#endregion

		#region JH_GB_TaxBranch

		protected bool JH_GB_TaxBranch_ReadOnly => !InvoicingAllowOverrideJobTaxBranch;

		#endregion

		public override ZGuid JH_OC_LocalBillingContact
		{
			get { return base.JH_OC_LocalBillingContact; }
			set
			{
				var oldValue = JH_OC_LocalBillingContact;
				base.JH_OC_LocalBillingContact = value;
				if (oldValue != JH_OC_LocalBillingContact)
				{
					Charges.Where(charge => charge.JR_OH_SellAccount == LocalChargesPK).ToList().ForEach(charge => charge.JR_OC_SellInvoiceContactInfo.RefreshBinding());
				}
			}
		}

		public CreditStatusBusinessObject CreditStatusBizObject
		{
			get
			{
				if (creditStatusBizObject == null)
				{
					creditStatusBizObject = new CreditStatusBusinessObject(Factory);
				}
				return creditStatusBizObject;
			}
		}
		CreditStatusBusinessObject creditStatusBizObject;

		#region JH_OA_AgentCollectAddr

		[List("Debtors")]
		public override ZGuid AgentCollectPK
		{
			get { return base.AgentCollectPK; }
			set { base.AgentCollectPK = value; }
		}

		[List("Debtors")]
		public override ZGuid JH_OA_AgentCollectAddr
		{
			get { return base.JH_OA_AgentCollectAddr; }
			set
			{
				if (JH_OA_AgentCollectAddr != value)
				{
					var oldValue = AgentCollectPK;
					base.JH_OA_AgentCollectAddr = value;
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_OA_AgentCollectAddrInfo);
					if (oldValue != AgentCollectPK)
					{
						ResetDebtorOnCharges(oldValue, AgentCollectPK);
						SetDefaultBranch(PlugInData);
					}

					Charges.Where(charge => charge.JR_OH_SellAccount == AgentCollectPK).ToList().ForEach(charge => charge.JR_OA_SellInvoiceAddressInfo.RefreshBinding());
				}
				else
				{
					PropertyMonitoredForChangesNotInSuspendedMode_ValueChanged(JH_OA_AgentCollectAddrInfo);
				}
			}
		}

		public ResourceStringData JH_OA_AgentCollectAddrCaption => IsGatewayBillingJob()
		? Res.GetData("cd89948d-09b2-4cc3-ab03-72958a9524bc", "Collect Agent", "Agent who collects revenue for this gateway billing job and/or handles the consol at destination.")
		: Res.GetData("JobChargeUserControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f77", "Overseas Agent");

		public ResourceStringData CollectBillToPartyCaption => Res.GetData("07CED490-75CC-438B-AA36-0654602C66F3", "Collect Bill-To Party", "This field defaults the job's Consignee. If Cross Trade jobs are not configured to bill the job's Controlling Party, then the Collect Bill-To Party (or their IFT party if they have one) will default as the charge line debtor for collect charges on this job.");

		#endregion

		#region JH_HoldReason

		public override ZString JH_HoldReason
		{
			get { return base.JH_HoldReason; }
			set
			{
				if (base.JH_HoldReason != value)
				{
					base.JH_HoldReason = value;
					if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
					{
						using (ChargesLoadSuspender.GetSuspender())
						{
							Charges.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		#endregion

		#region JH_JH_ParentJob

		public override ZGuid JH_JH_ParentJob
		{
			get { return base.JH_JH_ParentJob; }
			set
			{
				if (base.JH_JH_ParentJob != value)
				{
					base.JH_JH_ParentJob = value;
					Charges.Load();
				}
			}
		}

		#endregion

		#region CFX

		#region CFX Change Event

		public delegate bool LocalChargeCFXChanged(ZDecimal newRate);
		public LocalChargeCFXChanged LocalChargeCFXChangedEvent;

		bool OnLocalChargeCFXChanged(ZDecimal newRate)
		{
			bool result = true;

			if (LocalChargeCFXChangedEvent != null)
			{
				result = LocalChargeCFXChangedEvent(newRate);
			}

			return result;
		}

		#endregion
		internal const string JobTypeALL = "ALL";

		public void GetCFXPairFromOrganization(OrgHeader org, ZString currencyCode, ZDate? date, out ZDecimal cfxPercent, out ZDecimal cfxMin)
		{
			var cfxConfigurations = org?.CompanyData?.AccCFXConfigurations ?? GlbBranch.CurrentBranch.AccCFXConfigurations;

			var cfxRecord = cfxConfigurations.GetRecord(
				JobType?.Code ?? JobTypeALL,
				GetServiceDirectionCodeForCFX(),
				TransportMode,
				Origin?.Code.ToString(),
				Destination?.Code.ToString(),
				currencyCode,
				date);

			cfxPercent = cfxRecord?.JCF_CFXPercentage ?? ZDecimal.Zero;
			cfxMin = cfxRecord?.JCF_CFXMinimum ?? ZDecimal.Zero;
		}

		string GetServiceDirectionCodeForCFX()
		{
			if (IsCrossTrade)
			{
				return Constants.FreightShipmentDirection.Code.Other;
			}

			if (IsDomestic)
			{
				return OrgConstants.ServiceDirection.Code.Domestic;
			}

			if (IsExport)
			{
				return OrgConstants.ServiceDirection.Code.Export;
			}

			if (IsImport)
			{
				return OrgConstants.ServiceDirection.Code.Import;
			}

			if (ServiceDirection != OrgConstants.ServiceDirection.Code.Unknown)
			{
				return ServiceDirection;
			}

			return OrgConstants.ServiceDirection.Code.All;
		}

		void ResetDebtorOnCharges(ZGuid oldDebtorPK, ZGuid newDebtorPK)
		{
			InitializeParentFromGenericJobWithSettingDefaults();
			foreach (Charge charge in Charges.ToArray())
			{
#if DEBUG
				SetJobParent_ForTestOnly();
#endif
				if ((charge.JR_OH_SellAccount == oldDebtorPK || charge.JR_OH_SellAccount.IsEmpty && !charge.IsValidDebtorForDefaulting(oldDebtorPK)) && !charge.IsRevenuePosted)
				{
					if (!newDebtorPK.IsEmpty)
					{
						charge.ResetChargeDebtor();
					}
					else
					{
						charge.JR_OH_SellAccount = newDebtorPK;
					}
				}
			}
		}

#if DEBUG
		internal ZGuid jobParent_ForTestOnly;

		void SetJobParent_ForTestOnly()
		{
			if (Globals.IsTest && !jobParent_ForTestOnly.IsEmpty)
			{
				JH_ParentID = jobParent_ForTestOnly;
			}
		}
#endif

		#endregion

		#region ReOpenJobStatus

		public bool ReOpenJobStatus()
		{
			bool result = true;

			if (IsClosed)
			{
				if (JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(this, this))
				{
					JH_Status = JobHeaderStatus.Working.Code;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		#endregion

		#region ValidateReopenClosedJobSecurity

		public bool CanReOpenClosedJobSecurity(ZString newJobStatusValue)
			=> !IsReopeningJob(newJobStatusValue) || IsAllowedToReopenJob();

		bool IsReopeningJob(ZString newJobStatusValue) =>
			!JH_Status.IsEmpty &&
			newJobStatusValue != JobHeaderStatus.Closed.Code &&
			JH_StatusInfo.OriginalValue.ToString() == JobHeaderStatus.Closed.Code &&
			IsInDatabase;

		bool IsAllowedToReopenJob()
			=> JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(this, this);

		#endregion

		#region CanChangeStatusOfCompleteJobs

		public bool CanChangeStatusOfCompleteJobs(ZString newJobStatusValue)
		{
			bool result = true;
			if (!JH_Status.IsEmpty &&
				newJobStatusValue != JobHeaderStatus.Complete.Code &&
				JH_StatusInfo.OriginalValue.ToString() == JobHeaderStatus.Complete.Code &&
				IsInDatabase &&
				!Env.Security.ChangeStatusOfCompleteJobs.IsAllowed)
			{
				result = false;
			}
			return result;
		}

		#endregion

		#region JH_Status

		internal bool DoesChangingJobStatusTriggerRevenueRecognition
		{
			get { return !string.IsNullOrEmpty(RevenueRecognitionStatusSet); }
		}

		[List("JobStatusList")]
		public override ZString JH_Status
		{
			get { return base.JH_Status; }
			set
			{
				if (JH_Status != value)
				{
					// Status cannot be changed if previous change in the same session triggered revenue recognition
					if (!string.IsNullOrEmpty(RevenueRecognitionStatusSet) && RevenueRecognitionStatusSet != value)
					{
						if (OnCannotChangeStatusUserMessage != null && !Factory.IsInTransaction)
						{
							var eventArgs = new UserMessageEventArgs(Res.GetString("4D9FAF40-CA3F-48B1-AC2D-4DCC4DC3B4DF", "You have changed the status of this job to '{0}' before saving. This will trigger revenue recognition.\r\nIf you did not intend to recognize revenue, please close the job without saving.\r\nIf you intended to recognize revenue, please change the status back to '{0}' and save.", RevenueRecognitionStatusSet));
							OnCannotChangeStatusUserMessage(this, eventArgs);

							JH_StatusInfo.RefreshBinding();
						}
					}
					else
					{
						bool setNewValue = true;
						string optionToExclude = (value == JobHeaderStatus.Complete.Code || value == JobHeaderStatus.JobReadyForFinancialClosure.Code) ? RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure : null;
						var attemptingToReopenJob = IsReopeningJob(value);
						var jobExpectedToBeReopened = attemptingToReopenJob;

						if (attemptingToReopenJob && !IsAllowedToReopenJob())
						{
							if (!Factory.IsInTransaction)
							{
								OnCannotChangeStatusUserMessage(this, new UserMessageEventArgs(Env.Security.ReopenJob.ErrorMessageForNotAllowed));
							}
							jobExpectedToBeReopened = false;
							setNewValue = false;
						}
						else
						{
							if (value == JobHeaderStatus.Closed.Code || value == JobHeaderStatus.Complete.Code || value == JobHeaderStatus.JobReadyForFinancialClosure.Code)
							{
								JobValidation jobValidation = Validation as JobValidation;
								if (jobValidation != null)
								{
									string errors = jobValidation.GetRevenueRecognitionDateValidationErrors();
									if (!string.IsNullOrEmpty(errors))
									{
										if (jobValidation.IsErrorsCanBeFixedBySettingNowDate(errors))
										{
											if (OnCloseJobYesNoQuestion != null && !Factory.IsInTransaction)
											{
												string message = errors + System.Environment.NewLine +
													Res.GetString("9CFE3A95-0AB9-405B-866C-4A61BDC1039B", "The job {0} contains unrecognized revenue and cannot be closed or completed.\r\nDo you want to recognize revenue with Today's date?", JH_JobNum);
												var eventArgs = new UserQueryEventArgs(message, true);
												OnCloseJobYesNoQuestion(this, eventArgs);
												if (eventArgs.Response)
												{
													errors = string.Empty;
												}
											}
										}

										if (!string.IsNullOrEmpty(errors))
										{
											if (OnCloseJobError != null && !Factory.IsInTransaction)
											{
												OnCloseJobError(this, Res.GetString("657435EB-8731-4907-864E-FCDC9C1BBFB2", "The job {0} contains unrecognized revenue and cannot be closed or completed.\r\nPlease recognize revenue first and try again.", JH_JobNum)
													+ System.Environment.NewLine + errors);
											}
											setNewValue = false;
										}
									}
								}
								// Ask confirmation for changing to status which will cause Revenue Recognition run
								if (setNewValue && (GetChargesToRecognizeCost(optionToExclude).Any() || GetChargesToRecognizeSell(optionToExclude).Any() || GetUnrecognizedTransactionLines(optionToExclude).Any()))
								{
									RevenueRecognitionStatusSet = value;
								}
							}

							if (setNewValue && value == JobHeaderStatus.JobInvoiced.Code && !IsValidationSuspended && (GetChargesToRecognizeCost(optionToExclude).Any() || GetChargesToRecognizeSell(optionToExclude).Any() || GetUnrecognizedTransactionLines(optionToExclude).Any()))
							{
								RevenueRecognitionStatusSet = value;
							}

							if (setNewValue)
							{
								if (value == JobHeaderStatus.Closed.Code)
								{
									var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();

									if (ContainsUnpostedApportionment)
									{
										if (OnCloseJobError != null && !Factory.IsInTransaction)
										{
											OnCloseJobError(this, JobValidation.GetClosingJobApportionedChargesPresenceErrorMessage(JH_JobNum));
										}
										setNewValue = false;
									}
									else if (((JobValidation)Validation).CheckHasRelatedUnapprovedTransactions())
									{
										if (OnCloseJobError != null && !Factory.IsInTransaction)
										{
											OnCloseJobError(this, JobValidation.GetClosingJobUnapprovedTransactionErrorMessage(JH_JobNum));
										}
										setNewValue = false;
									}
									else if ((checker.IsPayablesCashAdvanceFunctionalityEnabled || checker.IsReceivablesCashAdvanceFunctionalityEnabled) && HasActiveUnInvoicedCashAdvanceRequests)
									{
										if (OnCloseJobError != null && !Factory.IsInTransaction)
										{
											OnCloseJobError(this, Res.GetString("bf8ce758-6e00-47fa-9c2e-509b89d541fc", "Unable to close job with active Advance Payment."));
										}
										setNewValue = false;
									}
								}
							}
						}

						if (setNewValue)
						{
							base.JH_Status = value;

							if (IsReopened)
							{
								SendJobReopenedEmailOnSaved = true;
							}

							UpdateJobReadOnlyStatus();
							RecognizeRevenue(optionToExclude);
						}
						else
						{
							base.JH_Status = JH_StatusInfo.OriginalValue.ToString();
						}

						UpdateHoldReason();

						if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
						{
							using (ChargesLoadSuspender.GetSuspender())
							{
								Charges.MarkAsNeedingValidation();
							}
						}

						if (!IsValidationSuspended)
						{
							Validation.ValidateJH_ProfitLossReasonCode();
						}

						SetReopenedUser();

						if (jobExpectedToBeReopened)
						{
							CriticalValidationInfoCollectorService
								.GetOrCreateService(Factory)
								.AddInfoWhenAllowed(
									PK,
									CriticalValidationInfoCollectorServiceKeyType.ReopeningJobStackTrace,
									() => FormattableString.Invariant($@"Attempted to re-open job to status: {value}
IsReopened: {IsReopened}
Authorized by user: {GetUserFromSecurity()?.GS_Code}
Current user: {GlbStaff.CurrentUser.GS_Code}
StackTrace:
{System.Environment.StackTrace}"));
						}
					}
				}
			}
		}

		void UpdateHoldReason()
		{
			var jobStatusesUsingHoldReason = new[] { JobHeaderStatus.WorkOnHold.Code, JobHeaderStatus.InvoiceOnHold.Code };

			if (jobStatusesUsingHoldReason.Contains(JH_StatusInfo.OriginalValue.ToString()) && !jobStatusesUsingHoldReason.Contains(JH_Status.ToString()))
			{
				JH_HoldReason = string.Empty;
			}
		}

		void RecognizeRevenue(string optionToExclude)
		{
			bool needToCallUpdateReverseDate = false;
			bool dummyBoolValue = false;
			if (JH_Status == JobHeaderStatus.Closed.Code || JH_Status == JobHeaderStatus.Complete.Code || JH_Status == JobHeaderStatus.JobInvoiced.Code || JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code)
			{
				var charges = (Charge[])Charges.ToArray(typeof(Charge));
				needToCallUpdateReverseDate = charges.Length > 0;

				var recognitionTypeFAR = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
				var recognitionDateFAR = CalculateFARRevenueRecognitionDate();
				if (recognitionDateFAR == RevenueRecognitionDateConstants.PostDateOfFirstARTransaction)
				{
					recognitionDateFAR = ZDateTime.Now;
				}

				if (charges.Any())
				{
					foreach (Charge charge in charges)
					{
						ApplyRevenueRecognitionDate(charge, recognitionTypeFAR, recognitionDateFAR, false, charge.CostRecognition == recognitionTypeFAR || charge.SellRecognition == recognitionTypeFAR, false, null, out dummyBoolValue);
					}
				}
				else if (JH_Status == JobHeaderStatus.JobInvoiced.Code)
				{
					//The second 'false' (useStubIfRecognitionOptionIsNotExist parameter) is responsible for not recording FAR date when the registry for this shipment is not set to FAR.
					ApplyRevenueRecognitionDate(null, recognitionTypeFAR, recognitionDateFAR, false, false, false, null, out dummyBoolValue);
				}

				if (JH_Status == JobHeaderStatus.Closed.Code)
				{
					var recognitionTypeJCL = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
					foreach (Charge charge in charges)
					{
						ApplyRevenueRecognitionDate(charge, recognitionTypeJCL, JH_A_JCL, false, charge.CostRecognition == recognitionTypeJCL || charge.SellRecognition == recognitionTypeJCL, false, null, out dummyBoolValue);
					}
				}

				if (JH_Status == JobHeaderStatus.Closed.Code || JH_Status == JobHeaderStatus.Complete.Code || JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code)
				{
					ApplyRevenueRecognitionDateForWholeJob(out dummyBoolValue);
					ApplyRevenueRecognitionDateForWholeJob(ZDateTime.Now, optionToExclude, out dummyBoolValue);
				}

				if (charges.Length != Charges.Count)
				{
					string message = string.Format((NoResString)"Charges collection was modified while changing Job Status to {0} for the Job PK '{1}'. Charges Count changed from {2} to {3}.", base.JH_Status, this.PK, charges.Length, Charges.Count);
					ErrorReporter.ReportOnce(message);
				}
			}
			if (needToCallUpdateReverseDate)
			{
				UpdateReverseDateForREVandCSTLines(null, out dummyBoolValue);
			}
		}

		string RevenueRecognitionStatusSet;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get => base.ReadOnly;
			set
			{
				if (base.ReadOnly != value)
				{
					base.ReadOnly = value;
					UpdateJobReadOnlyStatus();
				}
			}
		}

		void UpdateJobReadOnlyStatus()
		{
			bool isReadOnly = ReadOnly || IsClosed || (JH_Status == JobHeaderStatus.Complete.Code && !Env.Security.ModifyChargesOfCompleteJobs.IsAllowed) || (JH_Status == JobHeaderStatus.JobReadyForFinancialClosure.Code && !Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed);

			using (ExchangeRatesLoadSuspender.GetSuspender())
			{
				ExchangeRates.SetReadOnlyIncludingChildren(isReadOnly);
				ExchangeRates.RefreshBinding();
			}

			using (ChargesLoadSuspender.GetSuspender())
			{
				Charges.SetReadOnlyIncludingChildren(isReadOnly);
				Charges.RefreshBinding();
			}

			LocalZAddressWithContact?.SetReadOnlyIncludingChildren(isReadOnly);
			JH_OA_AgentCollectAddr_ZAddress?.SetReadOnlyIncludingChildren(isReadOnly);
		}

		protected bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			return GetPropertiesReadOnlyStateCore(property);
		}

		protected virtual bool GetPropertiesReadOnlyStateCore(PropertyDescriptor property)
		{
			bool result = (JH_Status == JobHeaderStatus.Complete.Code && !Env.Security.ModifyChargesOfCompleteJobs.IsAllowed);
			if (!result)
			{
				switch (property.Name)
				{
					case Schema.JH_Status:
						result = false;
						break;
					case Schema.JH_JobNum:
					case Schema.JH_A_JOP:
					case Schema.JH_A_JCL:
						result = true;
						break;
					case Schema.JH_HoldReason:
						result = (JH_Status != JobHeaderStatus.WorkOnHold.Code && JH_Status != JobHeaderStatus.InvoiceOnHold.Code);
						break;
					default:
						result = IsClosed;
						break;
				}
			}
			result = result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region PlugInData Values

		public ZPropertyInfo ChargeableWgtVolInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeableWgtVol)); }
		}

		[DecimalPlaces(nameof(WeightDecimals))]
		public ZDecimal ChargeableWgtVol
		{
			get { return (PlugInData != null) ? PlugInData.InvoicingSupporter.ActualChargeable : 0; }
		}

		public ZPropertyInfo ChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeableUnit)); }
		}

		public ZString ChargeableUnit
		{
			get { return (PlugInData != null) ? PlugInData.InvoicingSupporter.ActualChargeableUnit : new ZString("M3"); }
		}

		public ZInt ContainerCount
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.ContainerCount : 0; }
		}

		[DecimalPlaces(nameof(UnitDecimals))]
		public ZDecimal TEUCount
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.TEUCount : (ZDecimal)0m; }
		}

		public ZInt OuterPackTotal
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.OuterPackTotal : 0; }
		}

		[DecimalPlaces(nameof(WeightDecimals))]
		public ZDecimal ExcessActualVolumeWeight
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.ExcessActualVolumeWeight : 0; }
		}

		[DecimalPlaces(nameof(WeightDecimals))]
		public ZDecimal ExcessChargeableVolumeWeight
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.ExcessChargeableVolumeWeight : 0; }
		}

		#endregion

		public bool IsAutoratingInProcess
		{
			get;
			set;
		}

		public bool IsReversingInProcess
		{
			get { return fIsReversingInProcess; }
			set { fIsReversingInProcess = value; }
		}
		bool fIsReversingInProcess;

		#region JH_LocalCurrency
		[MaxLength(3)]
		public ZString JH_LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo JH_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Job.Schema.JH_LocalCurrency); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region JH_LocalCurrencyDecimals

		public ZInt JH_LocalCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		public ZPropertyInfo JH_LocalCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(Schema.JH_LocalCurrencyDecimals); }
		}

		public virtual int LocalDecimals => JH_LocalCurrencyDecimals;

		#endregion

		#region Job Totals

		#region JH_TotalRevenue

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalRevenue
		{
			get { return GetFilteredChargeTotal(c => c.JR_TotalLocalRevenue); }
		}

		public ZPropertyInfo JH_TotalRevenueInfo
		{
			get { return GetZPropertyInfo(Schema.JH_TotalRevenue); }
		}

		ZDecimal JH_TotalRevenueExcludingDSB
		{
			get { return GetFilteredChargeTotal(c => c.JR_TotalLocalRevenue, c => !((Charge)c).IsDisbursementCharge); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalRevenuePosted
		{
			get
			{
				decimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					if (aCharge.IsRevenuePosted)
					{
						result += aCharge.JR_TotalLocalRevenue;
					}
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalRevenuePostedExcludingDSB
		{
			get
			{
				return Charges.Where(c => c.IsRevenuePosted && !c.IsDisbursementCharge).Sum(c => c.JR_TotalLocalRevenue);
			}
		}

		#endregion

		#region JH_TotalWIP

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalWIP
		{
			get
			{
				decimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					if (!aCharge.IsRevenuePosted && aCharge.ARLine != null)
					{
						result += aCharge.JR_LocalSellAmt - aCharge.JR_CFXAmt;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JH_TotalWIPInfo
		{
			get { return GetZPropertyInfo(Schema.JH_TotalWIP); }
		}

		#endregion

		#region JH_ProfitLoss

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_ProfitLoss
		{
			get { return GetProfitLoss(JH_TotalRevenue, JH_TotalCost, JH_TotalCFX, JH_TotalTaxExpense); }
		}

		public ZPropertyInfo JH_ProfitLossInfo
		{
			get { return GetZPropertyInfo(Schema.JH_ProfitLoss); }
		}

		ZDecimal JH_ProfitLossExcludingDSB
		{
			get { return GetProfitLoss(JH_TotalRevenueExcludingDSB, JH_TotalCostExcludingDSB, JH_TotalCFXExcludingDSB, JH_TotalTaxExpenseExcludingDSB); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_ProfitLossPosted
		{
			get { return GetProfitLoss(JH_TotalRevenuePosted, JH_TotalCostPosted, JH_TotalCFXPosted, JH_TotalTaxExpensePosted); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_ProfitLossPostedExcludingDSB
		{
			get { return GetProfitLoss(JH_TotalRevenuePostedExcludingDSB, JH_TotalCostPostedExcludingDSB, JH_TotalCFXPostedExcludingDSB, JH_TotalTaxExpensePostedExcludingDSB); }
		}
		#endregion

		#region JH_TotalCost

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalCost
		{
			get { return GetFilteredChargeTotal((Charge c) => { return c.JR_LocalCostAmt; }); }
		}

		public ZPropertyInfo JH_TotalCostInfo
		{
			get { return GetZPropertyInfo(Schema.JH_TotalCost); }
		}

		ZDecimal JH_TotalCostExcludingDSB
		{
			get { return GetFilteredChargeTotal((Charge c) => { return c.JR_LocalCostAmt; }, (JobCharge c) => { return !((Charge)c).IsDisbursementCharge; }); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalCostPosted
		{
			get
			{
				ZDecimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					if (aCharge.IsCostPosted)
					{
						result += aCharge.JR_LocalCostAmt;
					}
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalCostPostedExcludingDSB
		{
			get
			{
				return Charges.Where(c => c.IsCostPosted && !c.IsDisbursementCharge).Sum(c => c.JR_LocalCostAmt);
			}
		}

		#endregion

		#region JH_TotalCFX

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal JH_TotalCFX
		{
			get
			{
				ZDecimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					result += aCharge.JR_CFXAmtReverseSign;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_TotalCFXInfo
		{
			get { return GetZPropertyInfo(Schema.JH_TotalCFX); }
		}

		ZDecimal JH_TotalCFXExcludingDSB
		{
			get
			{
				ZDecimal result = 0;
				foreach (Charge charge in Charges)
				{
					if (!charge.IsDisbursementCharge)
					{
						result += charge.JR_CFXAmtReverseSign;
					}
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal JH_TotalCFXPosted
		{
			get
			{
				ZDecimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					if (aCharge.IsCFXPosted)
					{
						result += aCharge.JR_CFXAmtReverseSign;
					}
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal JH_TotalCFXPostedExcludingDSB
		{
			get
			{
				return Charges.Where(c => c.IsCFXPosted && !c.IsDisbursementCharge).Sum(c => c.JR_CFXAmtReverseSign);
			}
		}

		#endregion

		#region Tax Expense

		[DecimalPlaces(nameof(LocalDecimals))]
		ZDecimal JH_TotalTaxExpense => JH_TotalTaxExpenseRevenue + JH_TotalTaxExpenseCost;

		[DecimalPlaces(nameof(LocalDecimals))]
		ZDecimal JH_TotalTaxExpensePosted
		{
			get
			{
				decimal result = 0;
				foreach (Charge aCharge in Charges)
				{
					result += aCharge.JR_TotalTaxExpenseRevenue + aCharge.JR_TotalTaxExpenseCost;
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		ZDecimal JH_TotalTaxExpenseExcludingDSB => GetFilteredChargeTotal((Charge c) => { return c.JR_TotalTaxExpenseRevenue + c.JR_TotalTaxExpenseCost; }, c => !((Charge)c).IsDisbursementCharge);

		[DecimalPlaces(nameof(LocalDecimals))]
		ZDecimal JH_TotalTaxExpensePostedExcludingDSB => Charges.Where(c => !c.IsDisbursementCharge).Sum(c => c.JR_TotalTaxExpenseRevenue + c.JR_TotalTaxExpenseCost);

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalTaxExpenseRevenue => GetFilteredChargeTotal(c => c.JR_TotalTaxExpenseRevenue, addFetchHints: charges =>
		{
			charges.Cast<Charge>().Where(c => c.IsInDatabase).ForEach(c => Factory.AddFetchHint(AccTaxRecordTransactionLinePivotSchema.ATP_AL_TransactionLine, c.JR_AL_ARLine));
		});

		public ZPropertyInfo JH_TotalTaxExpenseRevenueInfo => GetZPropertyInfo(nameof(JH_TotalTaxExpenseRevenue));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal JH_TotalTaxExpenseCost => GetFilteredChargeTotal((Charge c) => { return c.JR_TotalTaxExpenseCost; });

		public ZPropertyInfo JH_TotalTaxExpenseCostInfo => GetZPropertyInfo(nameof(JH_TotalTaxExpenseCost));

		#endregion

		public override Money GetTotalProfitAndLossLocal(Func<JobCharge, bool> isApplicableForTotal)
		{
			ZDecimal GetApplicableRevenue(JobCharge charge)
			{
				return charge.JR_AgentDeclaredSellAmtLocal.IsEmpty ? charge.JR_LocalSellAmt : charge.JR_AgentDeclaredSellAmtLocal;
			}

			ZDecimal GetApplicableCost(JobCharge charge)
			{
				return charge.JR_AgentDeclaredCostAmtLocal.IsEmpty ? charge.JR_LocalCostAmt : charge.JR_AgentDeclaredCostAmtLocal;
			}

			var totalApplicableRevenue = GetFilteredChargeTotal(GetApplicableRevenue, isApplicableForTotal);
			var totalApplicableCost = GetFilteredChargeTotal(GetApplicableCost, isApplicableForTotal);
			var totalApplicableCFX = GetFilteredChargeTotal((Charge c) => { return c.JR_CFXAmtReverseSign; }, isApplicableForTotal);
			var totalApplicableTaxExpense = GetFilteredChargeTotal((Charge c) => { return c.JR_TotalTaxExpenseRevenue + c.JR_TotalTaxExpenseCost; }, isApplicableForTotal);
			var profitLoss = GetProfitLoss(totalApplicableRevenue, totalApplicableCost, totalApplicableCFX, totalApplicableTaxExpense);

			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			return new Money(profitLoss, localCurrency, Res.GetString("BAC02E30-BFAC-4C0A-975E-868EB91AFDFB", "profit"));
		}

		ZDecimal GetFilteredChargeTotal(Func<Charge, ZDecimal> getAmountToAdd, Func<JobCharge, bool> isApplicableForTotal = null, Action<FilteredChargeCollectionView> addFetchHints = null)
		{
			ZDecimal result = 0;

			addFetchHints?.Invoke(ChargesFilteredByChargeViewingPermission);

			foreach (Charge charge in ChargesFilteredByChargeViewingPermission)
			{
				if (isApplicableForTotal == null || isApplicableForTotal(charge))
				{
					result += getAmountToAdd(charge);
				}
			}

			return result;
		}

		ZDecimal GetProfitLoss(ZDecimal revenue, ZDecimal cost, ZDecimal cfx, ZDecimal taxExpense)
		{
			return revenue - cost + cfx + taxExpense;
		}

		#endregion

		#region JH_ParentID

		public override ZGuid JH_ParentID
		{
			get
			{
				return base.JH_ParentID;
			}
			set
			{
				CheckFieldCannotChangeWhenJobIsAlreadyInDatabase(JH_ParentIDInfo, value);

				var oldValue = base.JH_ParentID;
				base.JH_ParentID = value;
				if (oldValue != base.JH_ParentID && AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					using (ChargesLoadSuspender.GetSuspender())
					{
						Charges.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region JH_ConsolNo

		public ZString JH_ConsolNo
		{
			get
			{
				ZString result = ZString.Empty;
				JobInvoicingConsumerType jobType = JobType;
				if (jobType != null && jobType.Code == JobInvoicingConsumerTypes.Shipment.Code)
				{
					ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers numbers = ConsolAndMasterBillNumbers;
					result = numbers == null ? ZString.Empty : numbers.VV_ConsolNumbers;
				}
				else
				{
					result = PlugInData == null ? ZString.Empty : PlugInData.InvoicingSupporter.ConsolNumber;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_ConsolNoInfo
		{
			get { return GetZPropertyInfo(Schema.JH_ConsolNo); }
		}

		#endregion

		#region JH_MasterBillNo

		public ZString JH_MasterBillNo
		{
			get
			{
				ZString result = ZString.Empty;
				JobInvoicingConsumerType jobType = JobType;
				if (jobType != null && jobType.Code == JobInvoicingConsumerTypes.Shipment.Code)
				{
					ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers numbers = ConsolAndMasterBillNumbers;
					result = numbers == null ? ZString.Empty : numbers.VV_MasterBillNumbers;
				}
				else
				{
					result = PlugInData == null ? ZString.Empty : PlugInData.InvoicingSupporter.MasterBillNumber;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_MasterBillNoInfo
		{
			get { return GetZPropertyInfo(Schema.JH_MasterBillNo); }
		}

		#endregion

		#region JH_CoLoadMasterBill

		public ZString JH_CoLoadMasterBill
		{
			get
			{
				ZString result = ZString.Empty;
				JobInvoicingConsumerType jobInvoicingType = JobType;
				if (jobInvoicingType != null && jobInvoicingType.Code == JobInvoicingConsumerTypes.Shipment.Code)
				{
					var numbers = ConsolAndMasterBillNumbers;
					result = numbers?.VV_CoLoadMasterBillNumbers ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_CoLoadMasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.JH_CoLoadMasterBill); }
		}

		#endregion

		#region JH_HouseBillNo

		public ZString JH_HouseBillNo
		{
			get
			{
				ZString result = ZString.Empty;
				JobInvoicingConsumerType jobType = JobType;
				if (jobType != null && jobType.Code == JobInvoicingConsumerTypes.Shipment.Code)
				{
					ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers numbers = ConsolAndMasterBillNumbers;
					result = numbers == null ? ZString.Empty : numbers.VV_HouseBill;
				}
				else
				{
					if (PlugInData == null)
					{
						this.InitializeParentFromGenericJobWithoutSettingDefaults();
					}
					result = PlugInData == null ? ZString.Empty : PlugInData.InvoicingSupporter.HouseBillNumber;
				}
				return result;
			}
		}

		public ZPropertyInfo JH_HouseBillNoInfo
		{
			get { return GetZPropertyInfo(Schema.JH_HouseBillNo); }
		}

		#endregion

		#region ShipmentConsolAndMasterBillNumbers

		ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers ConsolAndMasterBillNumbers
		{
			get
			{
				return Factory.Load<ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers>(JH_ParentID);
			}
		}

		#endregion

		#region JH_IsChargeCostReferenceFilterEnabled

		public ZBool JH_IsChargeCostReferenceFilterEnabled
		{
			get
			{
				if (!isChargeCostReferenceFilterEnabled.HasValue)
				{
					isChargeCostReferenceFilterEnabled = ShowOperationalJobRefFilter && !OperationalJobRef.IsEmpty;
				}

				return isChargeCostReferenceFilterEnabled.Value;
			}
			set
			{
				isChargeCostReferenceFilterEnabled = value;
				UpdateFilteredChargesFilter();

				JH_IsChargeCostReferenceFilterEnabledInfo.RefreshBinding();
			}
		}

		ZBool? isChargeCostReferenceFilterEnabled;

		public ZPropertyInfo JH_IsChargeCostReferenceFilterEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.JH_IsChargeCostReferenceFilterEnabled); }
		}

		ZString OperationalJobRef
		{
			get { return PlugInData != null ? PlugInData.InvoicingSupporter.OperationalJobRef : ZString.Empty; }
		}

		ZBool ShowOperationalJobRefFilter
		{
			get { return PlugInData != null && PlugInData.InvoicingSupporter.ShowOperationalJobRefFilter; }
		}

		#endregion

		#region JobDescription

		[MaxLength(Schema.JH_DescriptionMaxLength)]
		public ZString JobDescription
		{
			get
			{
				return IsJobDescriptionOverriden ? JH_Description : DefaultJobDescription;
			}
			set
			{
				if (JH_Description != value)
				{
					CheckMaximumLength(JobDescriptionInfo, value);
					SetPropertyValue(JH_DescriptionInfo, value);
					JobDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JobDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JobDescription); }
		}

		protected bool JobDescription_ReadOnly
		{
			get { return !IsJobDescriptionOverriden || !Env.Security.ModifyJobDescription.IsAllowed; }
		}

		ZString DefaultJobDescription
		{
			get
			{
				if (cached_DefaultJobDescription == null)
				{
					cached_DefaultJobDescription = new CachedProperty<ZString>(Factory, GetDefaultJobDescription);
				}
				return cached_DefaultJobDescription.Value;
			}
		}
		CachedProperty<ZString> cached_DefaultJobDescription;

		ZString GetDefaultJobDescription()
		{
			var result = ZString.Empty;

			if (PlugInData != null)
			{
				result = PlugInData.GetDefaultJobDescription();
			}

			return result;
		}

		#endregion

		#region IsJobDescriptionOverriden

		public ZBool IsJobDescriptionOverriden
		{
			get
			{
				return fIsJobDescriptionOverriden || !JH_Description.IsEmpty;
			}
			set
			{
				SetNonPersistentPropertyValue(IsJobDescriptionOverridenInfo, ref fIsJobDescriptionOverriden, value);
				JH_Description = value ? DefaultJobDescription : ZString.Empty;
				JH_DescriptionInfo.RefreshBinding();
			}
		}
		ZBool fIsJobDescriptionOverriden;

		public ZPropertyInfo IsJobDescriptionOverridenInfo
		{
			get { return GetZPropertyInfo(Schema.IsJobDescriptionOverriden); }
		}

		protected bool IsJobDescriptionOverriden_ReadOnly
		{
			get { return !Env.Security.ModifyJobDescription.IsAllowed; }
		}

		#endregion

		#region IScreeningPartiesProvider

		ScreeningParty[] IScreeningPartiesProvider.ScreeningParties => ObjectFactory
			.Get<IScreeningPartiesProviderHelper>()
			.GetJobInvoicingScreeningParties(Parent, Charges.ToArray<JobCharge>());

		#endregion

		public IEnumerable<JobConsolCost> UnpostedConsolCostsLinkedToThisJob
		{
			get
			{
				return from Charge charge in this.Charges
					   where charge.JR_IsApportioned && !charge.IsCostPosted
					   select charge.ParentConsolCost;
			}
		}

		public IEnumerable<Job> GetAllPeerJobsThatHaveUnpostedConsolCosts()
		{
			var allPeerJobs = new Dictionary<ZGuid, Job>();
			var visitedConsolCosts = new HashSet<ZGuid>();

			AddAllPeerJobsRecursively(this);
			allPeerJobs.Remove(PK);
			return allPeerJobs.Values.ToArray();

			void AddAllPeerJobsRecursively(Job job)
			{
				var peersOfThisJob = GetImmediatePeerJobsThatHaveUnpostedConsolCosts(job);
				foreach (var peerJob in peersOfThisJob)
				{
					if (!allPeerJobs.ContainsKey(peerJob.PK))
					{
						allPeerJobs.Add(peerJob.PK, peerJob);
						AddAllPeerJobsRecursively(peerJob);
					}
				}
			}

			HashSet<Job> GetImmediatePeerJobsThatHaveUnpostedConsolCosts(Job job)
			{
				var peersJobs = new HashSet<Job>();
				foreach (JobConsolCost cost in job.UnpostedConsolCostsLinkedToThisJob)
				{
					if (!visitedConsolCosts.Contains(cost.PK))
					{
						visitedConsolCosts.Add(cost.PK);
						foreach (ApportionSplitCharge apcharge in cost.ApportionmentCharges)
						{
							if (!apcharge.IsDeleted &&
								apcharge.JR_IsApportioned &&
								!apcharge.IsCostPosted &&
								apcharge.JR_LocalCostAmt != 0 &&
								apcharge.InvoicingJob != null &&
								apcharge.InvoicingJob != job)
							{
								peersJobs.Add(apcharge.InvoicingJob);
							}
						}
					}
				}
				return peersJobs;
			}
		}

		public ZString LayoutWhenPrintedInPeriodicInvoice { get; set; }
		public ZString SecondaryLayoutWhenPrintedInPeriodicInvoice { get; set; }

		#endregion

		#region Validation

		protected override JobHeaderValidation GetNewValidation()
		{
			return new JobValidation(this);
		}

		#region Posting Validation

		public string CanBePosted
		{
			get { return (Charges.IsNegativePayment) ? Res.GetString("4f8f58ff-d19d-43aa-a566-0cded5d63d77", "Negative Payment is not acceptable.") : ""; }
		}

		#endregion

		#region Group Validation

		ChargeGroupValidation ChargeGroupValidation
		{
			get
			{
				if (chargeGroupValidation == null)
				{
					ProcessGroupValidation();
				}

				return chargeGroupValidation;
			}
		}
		ChargeGroupValidation chargeGroupValidation;

		internal void InvalidateGroupValidation()
		{
			chargeGroupValidation = null;
		}

		void ChargeCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			InvalidateGroupValidation();
		}

		void ProcessGroupValidation()
		{
			var chargesToProcess = Charges;
			chargeGroupValidation = new ChargeGroupValidation();
			foreach (Charge charge in chargesToProcess)
			{
				chargeGroupValidation.AddItem(charge);
			}
		}

		public bool HasSameRelatedJobNumberAndSellReferenceNumberMoreThanOnce(ChargeWithCost charge) => ChargeGroupValidation.HasSameRelatedJobNumberAndSellReferenceNumberMoreThanOnce(charge);

		public bool HasMultipleGatewayChargesUsingSameChargeCode(Charge charge, (IOrgHeader sendingAgent, IOrgHeader receivingAgent) gatewayAgents) => ChargeGroupValidation.HasMultipleGatewayChargesUsingSameChargeCode(charge, gatewayAgents);

		#endregion

		#endregion

		#region AP Auto Population

		ZGuid creditor;
		[ReadOnly(true)]
		[List("Creditors")]
		public ZGuid Creditor
		{
			get { return creditor; }
			set { SetNonPersistentPropertyValue(CreditorInfo, ref creditor, value); }
		}

		public ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(nameof(Creditor)); }
		}

		public CreditorCollection Creditors
		{
			get { return FindboxLookupCollections.GetCreditorCollection(Factory); }
		}

		ZString invoiceNum;
		[MaxLength(JobCharge.Schema.JR_APInvoiceNumMaxLength)]
		public ZString InvoiceNum
		{
			get { return invoiceNum; }
			set
			{
				if (invoiceNum != value)
				{
					CheckMaximumLength(InvoiceNumInfo, value);
					SetNonPersistentPropertyValue(InvoiceNumInfo, ref invoiceNum, value);
					if (ChargesForAutoPopulate != null)
					{
						using (GetValidationSuspender())
						{
							foreach (Charge ch in ChargesForAutoPopulate)
							{
								if (ch.IsSelectedForAutoPopulation)
								{
									ch.JR_APInvoiceNum = InvoiceNum;
								}
							}
						}
						if (!IsValidationSuspended)
						{
							((JobValidation)Validation).ValidateInvoiceNum();
						}
					}
				}
			}
		}

		public ZPropertyInfo InvoiceNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(InvoiceNum));
			}
		}

		ZDateTime invoiceDate;
		public ZDateTime InvoiceDate
		{
			get { return invoiceDate; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceDateInfo, ref invoiceDate, value);
				SetDefaultDocumentReceivedDate();
				if (value.IsValid)
				{
					CalculateDueDate();
				}
				if (ChargesForAutoPopulate != null)
				{
					using (GetValidationSuspender())
					{
						foreach (Charge ch in ChargesForAutoPopulate)
						{
							if (ch.IsSelectedForAutoPopulation)
							{
								ch.JR_APInvoiceDate = InvoiceDate;
							}
						}
					}
					if (!IsValidationSuspended)
					{
						((JobValidation)Validation).ValidateInvoiceDate();
					}
				}
			}
		}

		public ZPropertyInfo InvoiceDateInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(InvoiceDate));
				return info;
			}
		}

		#region CalculateDueDate

		void CalculateDueDate()
		{
			if (InvoiceDueDate.IsEmpty)
			{
				Charge selectedCharge = ChargesForAutoPopulate.Count > 0 ? ChargesForAutoPopulate[0] : null;
				if (selectedCharge != null && selectedCharge.CostAccount != null && selectedCharge.CostAccount.CompanyData != null)
				{
					var calculateDate = DueDateCalculation.GetCalculateDate(selectedCharge.CostAccount.CompanyData.GetAPTerm(), InvoiceDate, DocumentReceivedDate);
					InvoiceDueDate = new InvoiceAndDueDateCalculator(PlugInData, calculateDate, selectedCharge.CostAccount).DueDate;
				}
			}
		}

		#endregion

		ZDateTime documentReceivedDate;
		public ZDateTime DocumentReceivedDate
		{
			get { return documentReceivedDate; }
			set
			{
				SetNonPersistentPropertyValue(DocumentReceivedDateInfo, ref documentReceivedDate, value);
				if (value.IsValid)
				{
					CalculateDueDate();
				}
				if (ChargesForAutoPopulate != null)
				{
					using (GetValidationSuspender())
					{
						foreach (Charge ch in ChargesForAutoPopulate)
						{
							if (ch.IsSelectedForAutoPopulation)
							{
								ch.JR_APDocumentReceivedDate = DocumentReceivedDate;
							}
						}
					}
					if (!IsValidationSuspended)
					{
						((JobValidation)Validation).ValidateDocumentReceivedDate();
					}
				}
			}
		}

		public ZPropertyInfo DocumentReceivedDateInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(DocumentReceivedDate));
				return info;
			}
		}

		void SetDefaultDocumentReceivedDate()
		{
			if (DocumentReceivedDate.IsEmpty)
			{
				var defaultLogic = AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.Value;
				if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate)
				{
					DocumentReceivedDate = ZDateTime.Now;
				}
				else if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate)
				{
					DocumentReceivedDate = InvoiceDate;
				}
			}
		}

		ZDateTime invoiceDueDate;
		public ZDateTime InvoiceDueDate
		{
			get { return invoiceDueDate; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceDueDateInfo, ref invoiceDueDate, value);
				if (ChargesForAutoPopulate != null)
				{
					using (GetValidationSuspender())
					{
						foreach (Charge ch in ChargesForAutoPopulate)
						{
							if (ch.IsSelectedForAutoPopulation)
							{
								ch.JR_PaymentDate = InvoiceDueDate;
							}
						}
					}
					if (!IsValidationSuspended)
					{
						((JobValidation)Validation).ValidateInvoiceDueDate();
					}
				}
			}
		}

		public ZPropertyInfo InvoiceDueDateInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(InvoiceDueDate));
				return info;
			}
		}

		ZString supplierCostReference;
		[MaxLength(JobCharge.Schema.JR_CostReferenceMaxLength)]
		public ZString SupplierCostReference
		{
			get { return supplierCostReference; }
			set
			{
				if (supplierCostReference != value)
				{
					SetNonPersistentPropertyValue(SupplierCostReferenceInfo, ref supplierCostReference, value);
					if (ChargesForAutoPopulate != null)
					{
						using (GetValidationSuspender())
						{
							foreach (Charge ch in ChargesForAutoPopulate)
							{
								if (ch.IsSelectedForAutoPopulation)
								{
									ch.JR_CostReference = SupplierCostReference;
								}
							}
						}
						if (!IsValidationSuspended)
						{
							((JobValidation)Validation).ValidateSupplierCostReference();
						}
					}
				}
			}
		}

		public ZPropertyInfo SupplierCostReferenceInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(SupplierCostReference));
			}
		}

		ChargeCollection fChargesForAutoPopulate;
		[BusinessObjectTestExclude]
		public ChargeCollection ChargesForAutoPopulate
		{
			get
			{
				if (fChargesForAutoPopulate == null)
				{
					fChargesForAutoPopulate = new ChargeCollection(this);
				}
				return fChargesForAutoPopulate;
			}
			set
			{
				fChargesForAutoPopulate = value;
			}
		}

		ZQuery FilterForAutoPopulate
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, Creditor);
				filter.AddToFilter(JobChargeSchema.JR_E6, null);
				return filter;
			}
		}

		public void LoadForAutoPopulate()
		{
			this.ChargesForAutoPopulate.Load(FilterForAutoPopulate);
			this.ChargesForAutoPopulate.LoadedFromAutopopulate = true;
			foreach (Charge ch in ChargesForAutoPopulate)
			{
				using (ch.SuspendSettingHasChanges())
				{
					if (ch.JR_APInvoiceNum.IsEmpty)
					{
						ch.IsSelectedForAutoPopulation = true;
					}
					else
					{
						ch.IsSelectedForAutoPopulation = false;
					}
				}
			}
			RegisterEditableChildObject(ChargesForAutoPopulate);
		}

		public void PostAutoPopulation()
		{
			this.Factory.Save();
		}

		#endregion

		#region ITotalProvider Members

		public void UpdateTotals()
		{
			if (!(IsReversingInProcess || IsAutoratingInProcess))
			{
				RefreshBindingTotalFields();
			}
		}

		protected virtual void RefreshBindingTotalFields()
		{
#if DEBUG
			RefreshBindingTotalFieldsIsCalledForTest = true;
#endif
			JH_TotalCostInfo.RefreshBinding();
			JH_ProfitLossInfo.RefreshBinding();
			JH_TotalRevenueInfo.RefreshBinding();
		}

		#endregion

		#region Currency and Exchange Rates

		public IExchangeRateJobBilling GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger, bool forceExchangeRateCreation = false, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			if (currencyCode.IsEmpty || currencyCode == this.Company.GC_RX_NKLocalCurrency)
			{
				return null;
			}

			var orgType = this.GetJobInvoicingOrgType(ledger);

			var exRate = ExchangeRates.GetExchangeRate(currencyCode, orgPk, orgType, invoiceCurrencyType);

			if (exRate == null)
			{
				if (!forceExchangeRateCreation)
				{
					return null;
				}
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				if (currency == null)
				{
					return null;
				}
				exRate = AddCurrency(currency, orgPk, orgType.ToLedger(), invoiceCurrencyType);
			}

			if (exRate == null)
			{
				return null;
			}

			var org = orgPk == ZGuid.Empty ? null : Factory.Load<OrgHeader>(orgPk);

			return new ExchangeRateWrapper(exRate, org);
		}

		public decimal GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateOrgTypeEnum orgType, ExchangeRateType rateType, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			var rate = ((IExchangeRateProvider)this).GetExchangeRate(currencyCode, orgPk, orgType.ToLedger(), invoiceCurrencyType: invoiceCurrencyType);

			if (rate == null)
			{
				return 0;
			}

			if (rateType == ExchangeRateType.Buy)
			{
				return rate.Rate;
			}
			if (rateType == ExchangeRateType.Sell)
			{
				return rate.SellRate;
			}

			return 0;
		}

		public override void UpdateExchangeRates(bool updateEmptyExchangeRates)
		{
			InitializeParentFromGenericJobWithSettingDefaults();
			foreach (ExchangeRate exRate in ExchangeRates.ToArray())
			{
				UpdateExchangeRate(exRate, updateEmptyExchangeRates);
			}
		}

#if DEBUG
		protected virtual
#endif
		void UpdateExchangeRate(ExchangeRate exchangeRate, bool updateEmptyExchangeRates)
		{
			var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(exchangeRate.EffectiveInvoiceCurrencyType);
			ZDecimal defaultExRate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ExchangeRateConfigurationRateConsumer, exchangeRate.RateCurrency, exchangeRate.Org, exchangeRate.OrgType.ToLedger(), invoiceCurrencyType);
			if (updateEmptyExchangeRates || !defaultExRate.IsEmpty)
			{
				exchangeRate.JF_BaseRate = defaultExRate;
			}
		}

		public override bool HasEmptyExchangeRates()
		{
			return ExchangeRates.Cast<ExchangeRate>().Any(exRate =>
				{
					var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(exRate.EffectiveInvoiceCurrencyType);
					return AccExchangeRateConfigurationRateFinder.GetExchangeRate(ExchangeRateConfigurationRateConsumer, exRate.RateCurrency, exRate.Org, exRate.OrgType.ToLedger(), invoiceCurrencyType).IsEmpty;
				});
		}

		public ExchangeRateOrgTypeEnum GetJobInvoicingOrgType(ExchangeRateValidLedgerEnum ledger)
		{
			if (ledger == ExchangeRateValidLedgerEnum.AP || ledger == ExchangeRateValidLedgerEnum.UA)
			{
				return ExchangeRateOrgTypeEnum.Creditor;
			}

			if (ledger == ExchangeRateValidLedgerEnum.AR)
			{
				return ExchangeRateOrgTypeEnum.Debtor;
			}

			return ExchangeRateOrgTypeEnum.None;
		}

		#region Exchange Rate Markups

		public ExchangeRate AddCurrency(RefCurrency currency, ExchangeRateValidLedgerEnum ledger, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			return AddCurrency(currency, ZGuid.Empty, ledger, invoiceCurrencyType);
		}

		public ExchangeRate AddCurrency(RefCurrency currency, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			if (currency == null)
			{
				return null;
			}

			var org = Factory.Load<OrgHeader>(orgPk);
			var exRate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ExchangeRateConfigurationRateConsumer, currency, org, ledger, invoiceCurrencyType);
			return AddCurrency(currency, exRate, orgPk, ledger, invoiceCurrencyType);
		}

		//Add blank line with the currency
		public ExchangeRate AddCurrency(RefCurrency currency, ZDecimal rate, ExchangeRateValidLedgerEnum ledger, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			return AddCurrency(currency, rate, ZGuid.Empty, ledger, invoiceCurrencyType);
		}

		public ExchangeRate AddCurrency(RefCurrency currency, ZDecimal rate, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			if (currency == null)
			{
				return null;
			}

			var orgType = GetJobInvoicingOrgType(ledger);
			var exRate = ExchangeRates.AddRate(currency, rate, orgPk, orgType, invoiceCurrencyType);

			if (exRate == null)
			{
				return null;
			}

			//now we need to refresh charge lines associated with the same organization:
			RefreshChargeLinesExchangeRateBinding(currency.RX_Code, orgPk, invoiceCurrencyType);

			return exRate;
		}

		void RefreshChargeLinesExchangeRateBinding(ZString currencyCode, ZGuid orgPk, InvoiceCurrencyType invoiceCurrencyType)
		{
			IEnumerable<BaseCharge> chargesToUpdate;
			if (IsChargesCollectionLoaded)
			{
				chargesToUpdate = Charges.Cast<BaseCharge>()
					.Where(c => (c.JR_OH_SellAccount == orgPk || orgPk.IsEmpty) && (c.JR_SellCurrency == currencyCode || c.JR_RX_NKSellInvoiceCurrency == currencyCode));
			}
			else
			{
				var queryOrg = new ZQuery(JobChargeSchema.JR_OH_SellAccount, orgPk);
				queryOrg.AddToFilter(new ZQuery(JobChargeSchema.JR_OH_SellAccount, ZGuid.Empty), JoinCondition.Or);

				var queryJob = new ZQuery(JobChargeSchema.JR_JH, this.PK);
				queryJob.AddToFilter(queryOrg);

				var queryCurrency = new ZQuery(JobChargeSchema.JR_RX_NKSellCurrency, currencyCode);
				queryCurrency.AddToFilter(new ZQuery(JobChargeSchema.JR_RX_NKSellInvoiceCurrency, currencyCode), JoinCondition.Or);

				var query = new ZQuery(queryJob, queryCurrency);

				chargesToUpdate = Factory.Load<BaseCharge>(query);
			}

			foreach (var c in chargesToUpdate.Where(x => x.InvoiceCurrencyTypeForAR == invoiceCurrencyType))
			{
				if (c.JR_SellCurrency == currencyCode)
				{
					c.UpdateRevenueExchangeRate();
				}
				if (c.JR_RX_NKSellInvoiceCurrency == currencyCode)
				{
					c.UpdateSellInvoiceExchangeRate();
				}
			}

			if (IsChargesCollectionLoaded)
			{
				chargesToUpdate = Charges.Cast<BaseCharge>().Where(c => (c.JR_OH_CostAccount == orgPk || orgPk.IsEmpty) && c.JR_RX_NKCostCurrency == currencyCode);
			}
			else
			{
				var queryOrg = new ZQuery(JobChargeSchema.JR_OH_CostAccount, orgPk);
				queryOrg.AddToFilter(new ZQuery(JobChargeSchema.JR_OH_CostAccount, ZGuid.Empty), JoinCondition.Or);

				var queryJob = new ZQuery(JobChargeSchema.JR_JH, this.PK);
				queryJob.AddToFilter(queryOrg);

				var queryCurrency = new ZQuery(JobChargeSchema.JR_RX_NKCostCurrency, currencyCode);

				var query = new ZQuery(queryJob, queryCurrency);

				chargesToUpdate = Factory.Load<BaseCharge>(query);
			}

			foreach (var c in chargesToUpdate)
			{
				c.UpdateCostExchangeRate();
			}
		}

		public
#if DEBUG
			virtual
#endif
			void RefreshChargeLinesExchangeRateBinding()
		{
			RefreshChargeLinesExchangeRateBinding(Charges.Cast<BaseCharge>());
		}

		internal static void RefreshChargeLinesExchangeRateBinding(IEnumerable<BaseCharge> charges)
		{
			foreach (var c in charges.Where(c => c.JR_SellCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency || c.JR_RX_NKSellInvoiceCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency))
			{
				c.UpdateRevenueExchangeRate();
				c.UpdateSellInvoiceExchangeRate();
			}

			foreach (var c in charges.Where(c => c.JR_RX_NKCostCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency))
			{
				c.UpdateCostExchangeRate();
			}
		}

		void AddGenericExchangeRatesFromSource(IExchangeRateSource source)
		{
			if (source != null)
			{
				foreach (IExchangeRate exRate in source.ToArray())
				{
					var currency = RefCurrency.LoadFromCurrencyCode(Factory, exRate.CurrencyCode);
					var rate = AddCurrency(currency, exRate.Rate, ExchangeRateValidLedgerEnum.None);
					if (rate != null)
					{
						rate.JF_IsTransformed = true; //to prevent it from being automatically deleted
					}
				}
			}
		}

		#endregion

		#endregion

		#region Reverse Commission Transactions

		List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> nonReversedCommissions;
		List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> NonReversedCommissions
		{
			get
			{
				if (nonReversedCommissions == null)
				{
					nonReversedCommissions = GetNonReversedCommissionHeaders();
				}

				return nonReversedCommissions;
			}
		}

		public override bool HasNonReversedCommissionHeaders()
		{
			return HasNonReversedCommissionHeaders(ZGuid.Empty);
		}

		public override bool HasNonReversedCommissionHeaders(ZGuid orgPk)
		{
			return NonReversedCommissions.Any(i => orgPk.IsEmpty || i.Item1.AH_OH == orgPk);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		List<Tuple<ICommissionableTransaction, IAccCommissionHeader[]>> GetNonReversedCommissionHeaders() => ObjectFactory.Get<ICommissionHelper>().GetNonReversedCommissionHeaders(Factory, this);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public override void ReverseTransactionCommissions()
		{
			if (IsDeleted)
			{
				return;
			}

			foreach (var grouping in NonReversedCommissions)
			{
				var transaction = grouping.Item1;
				var commissions = grouping.Item2;

				if (!commissionsReversalOrgPk.IsEmpty && transaction.AH_OH != commissionsReversalOrgPk && !(transaction is APInvoice))
				{
					continue;
				}

				foreach (var commission in commissions)
				{
					var commissionCreator = ObjectFactory.Get<ICommissionCreatorProvider>().GetReversalTransactionCommissionCreator(transaction);
					commissionCreator.CreateReversalCommissions(commission);
				}
			}

			nonReversedCommissions = null;
			commissionsReversalOrgPk = ZGuid.Empty;
		}

		#endregion

		#region GSTID

		protected override ZGuid GetGSTIDCore(OrgHeader org, AccChargeCode chargeCode, GlbBranch branch, CostSell costOrSell, ILocation chargeFixedPlaceOfSupplyLocation, ZString supplyType, out ZGuid overrideInvTaxMsg)
		{
			overrideInvTaxMsg = ZGuid.Empty;
			ZGuid result = ZGuid.Empty;

			if (org == null || chargeCode == null)
			{
				return result;
			}

			var parameters = this.GetTaxCalculationParameters();
			parameters.Organisation = org;
			parameters.Branch = branch;
			parameters.CostOrSell = costOrSell;
			var invoicingSupporter = GetInvoicingSupporter();
			parameters.Consignor = invoicingSupporter?.Consignor;
			parameters.Consignee = invoicingSupporter?.Consignee;
			if (chargeFixedPlaceOfSupplyLocation != null)
			{
				parameters.FixedPlaceOfSupply = chargeFixedPlaceOfSupplyLocation;
			}
			parameters.SupplyType = supplyType;

			var taxRate = chargeCode.GetGSTRate(parameters, out overrideInvTaxMsg);

			if (taxRate != null)
			{
				result = taxRate.PK;
			}

			return result;
		}

		public override ZGuid GetDebtorPK(AccChargeCode chargeCode, ZString relatedJobNumber)
		{
			var result = ZGuid.Empty;

			var supporter = GetInvoicingSupporter();
			var defaultDebtor = supporter?.GetDefaultDebtor(chargeCode, this, relatedJobNumber);
			if (defaultDebtor != null)
			{
				result = defaultDebtor.PK;
			}
			else if (supporter == null || AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.GetValueWithoutFallback(JH_GC.ToGuid(), Guid.Empty, Guid.Empty))
			{
				result = base.GetDebtorPK(chargeCode, relatedJobNumber);
			}

			return result;
		}

		public override ZGuid GetCreditorPK(AccChargeCode chargeCode, ZString chargeType, ZString invoiceType, ZGuid rateProviderOrgPK)
		{
			IJobInvoicingSupporter supporter = GetInvoicingSupporter();

			if (supporter != null && (chargeType == Constants.ChargeType.Margin || chargeType == Constants.ChargeType.Disbursement || chargeType == Constants.ChargeType.ManualJobAccrual))
			{
				OrgHeader defaultCreditor = supporter.GetDefaultCreditor(new DefaultCreditorSetting(chargeCode, invoiceType, rateProviderOrgPK));

				if (defaultCreditor != null)
				{
					return defaultCreditor.PK;
				}
			}

			return ZGuid.Empty;
		}

		public IJobInvoicingSupporter GetInvoicingSupporter()
		{
			var plugIn = PlugInData;
			if (plugIn == null && !JH_ParentID.IsEmpty && !JH_ParentTableCode.IsEmpty)
			{
				plugIn = LoadJobInvoicingPlugIn();
			}
			return plugIn?.InvoicingSupporter;
		}

		IJobInvoicingPlugIn LoadJobInvoicingPlugIn()
		{
			var genericJob = Factory.LoadGenericJob<GenericJob.GenericJob>(JH_ParentID, JH_ParentTableCode);
			return genericJob?.Consumer;
		}

		protected override ZString GetChargeType(AccChargeCode chargeCode)
		{
			var chargeOverride = GetChargeTypeOverrideInformation(chargeCode);
			return chargeOverride != null ? chargeOverride.AN_ChargeType : base.GetChargeType(chargeCode);
		}

		IAccChargeTypeOverride GetChargeTypeOverrideInformation(AccChargeCode chargeCode)
		{
			IAccChargeTypeOverride chargeTypeOverrideInformation = null;
			string chargeTypeCacheKey = GetChargeTypeCacheKey(chargeCode);

			if (!string.IsNullOrEmpty(chargeTypeCacheKey))
			{
				chargeTypeOverrideInformation = ChargeTypeCache.GetOrAdd(chargeTypeCacheKey, (k) => chargeCode.GetChargeType(JobType, MovementDirection));
			}

			return chargeTypeOverrideInformation;
		}

		public static IAccChargeTypeOverride GetChargeTypeInformation(AccChargeCode chargeCode, Job job = null)
		{
			return job != null ? job.GetChargeTypeOverrideInformation(chargeCode) : chargeCode.GetChargeType(null, Directions.Unknown);
		}

		string GetChargeTypeCacheKey(AccChargeCode chargeCode)
		{
			if (chargeCode == null)
			{
				return string.Empty;
			}

			var jobTypeCode = JobType != null ? JobType.Code : string.Empty;
			return string.Concat(chargeCode.PK, chargeCode.AC_ChargeType, MovementDirection, jobTypeCode);  //Allow user to change Charge Type on the Charge Code itself.
		}

		ConcurrentDictionary<string, IAccChargeTypeOverride> ChargeTypeCache
		{
			get
			{
				return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + "JobAccChargeTypeOverrideDictionary",
					() => new ConcurrentDictionary<string, IAccChargeTypeOverride>(),
					CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

#if DEBUG

		internal void ClearChargeTypeCache_ForTestOnly()
		{
			ChargeTypeCache.Clear();
		}

		internal void ForceChargesRefreshExRates_ForTestsOnly()
		{
			foreach (ExchangeRate exRate in ExchangeRates)
			{
				exRate.InvokeChanged_ForTestsOnly();
			}
		}

#endif

		#endregion

		#region ExchangeRateConfigurationRateConsumer

		public IAccExchangeRateConfigurationRateConsumer ExchangeRateConfigurationRateConsumer => ExchangeRateConfigurationRateConsumerCreator.CreateExchangeRateConfigurationRateConsumerForJob(this);

		#endregion

		#region IAutoRatingAccountingInfo Members

		ZString IAutoRatingAccountingInfo.QuoteNumber
		{
			get { return OneOffQuote != null ? JH_TH_NKQuoteNumber : ZString.Empty; }
		}

		public override CurrencyConverter CurrencyConverter
		{
			get { return new JobExRateCurrencyConverter(ExchangeRates, ZDateTime.Today, ExchangeRateType.Sell); }
		}

		IAutoRatingChargeInfo[] IAutoRatingAccountingInfo.GetExistingCharges(bool fromAllCompanies)
		{
			// Job always has charges only for the company it was created for.
			// So, fromAllCompanies is ignored, it can't provide charges for all companies by design.
			return Charges.Cast<IAutoRatingChargeInfo>().ToArray();
		}

		#endregion

		#region IAutoRatingAccountingUtils Members

		void IAutoRatingAccountingUtils.ReloadChargesFromAdditionalJobs()
		{
			ToggleFlagForReLoadingAdditionalJobs(true);
			ResetAdditionalJobs();
			LoadCharges();
		}

		void IAutoRatingAccountingUtils.UpdateChargesDescription()
		{
			var charges = Charges.Cast<ChargeWithCost>();
			foreach (var charge in charges)
			{
				charge.SetChargeDescription();
			}
		}

		#endregion

		#region ILandedCostChargeHolder Members

		public IEnumerable<IDefaultLandedCostInput> ChargesToImportForLandedCosting
		{
			get
			{
				foreach (Charge charge in Charges)
				{
					if (((IDefaultLandedCostInput)charge).IsValidToImport)
					{
						yield return charge;
					}
				}
			}
		}

		ZDecimal ILandedCostExchangeRateProvider.GetExRateFor(RefCurrency currency)
		{
			ZDecimal result = 0m;
			if (currency != null)
			{
				ExchangeRate jobExRate = ExchangeRates.FindByRefCurrency(currency);
				if (jobExRate != null)
				{
					result = jobExRate.JF_SellRate;
				}
			}
			return result;
		}

		#endregion

		#region IPostingJob Members

		ZGuid IPostingJob.Branch
		{
			get { return JH_GB; }
		}

		ZGuid IPostingJob.Department
		{
			get { return JH_GE; }
		}

		ZString IPostingJob.JobNumber
		{
			get { return JH_JobNum; }
		}

		ZGuid IPostingJob.PK
		{
			get { return PK; }
		}

		IJobInvoicingPlugIn IPostingJob.Consumer
		{
			get { return PlugInData; }
		}

		ZShort IPostingJob.UniqueJobInvoiceNumber
		{
			get { return JH_UniqueJobInvoiceNumber; }
		}

		void IPostingJob.IncrementUniqueJobInvoiceNumber()
		{
			JH_UniqueJobInvoiceNumber += 1;
		}

		void IPostingJob.DecrementUniqueJobInvoiceNumber()
		{
			JH_UniqueJobInvoiceNumber = JH_UniqueJobInvoiceNumber - 1;
		}

		TaxDateDefaultingOption IPostingJob.GetTaxDateDefaultingOptionForJob(BusinessObjectFactory factory, ZString ledger)
		{
			var cachedKey = string.Format("JobInvoicingJob_GetTaxDateDefaultingOptionForJob_{0}_{1}", ledger, this.PK);
			return factory.GetCachedValue(cachedKey, () =>
			{
				TaxDateDefaultingOption result = null;
				var plugIn = PlugInData?.InvoicingSupporter;
				if (plugIn != null)
				{
					result = AccountingUtils.GetTaxDateDefaultingOptionForInvoicingSupporter(plugIn, ledger);
				}
				return result;
			});
		}

		Tuple<ZDate, ZString> IPostingJob.GetTaxDateBasedOnRegistryDefaultingOption(IJobInvoicingSupporter invoicingSupporter, ZString taxDateOption, ZDate invoiceDate)
		{
			var result = new Tuple<ZDate, ZString>(ZDate.Today, TaxDateDefaultingOption.Description.Today);
			switch (taxDateOption)
			{
				case TaxDateDefaultingOption.Code.InvoiceDate:
					result = new Tuple<ZDate, ZString>(invoiceDate, TaxDateDefaultingOption.Description.InvoiceDate);
					break;
				case TaxDateDefaultingOption.Code.ArrivalDate:
				case TaxDateDefaultingOption.Code.VesselArrivalDate:
					result = new Tuple<ZDate, ZString>(invoicingSupporter?.ATA.Date ?? ZDate.Empty, TaxDateDefaultingOption.Description.ArrivalDate);
					break;
				case TaxDateDefaultingOption.Code.DepartureDate:
				case TaxDateDefaultingOption.Code.VesselDepartureDate:
					result = new Tuple<ZDate, ZString>(invoicingSupporter?.ATD.Date ?? ZDate.Empty, TaxDateDefaultingOption.Description.DepartureDate);
					break;
				case TaxDateDefaultingOption.Code.EstimatedArrivalDate:
					result = new Tuple<ZDate, ZString>(invoicingSupporter?.ETA.Date ?? ZDate.Empty, TaxDateDefaultingOption.Description.EstimatedArrivalDate);
					break;
				case TaxDateDefaultingOption.Code.EstimatedDepartureDate:
					result = new Tuple<ZDate, ZString>(invoicingSupporter?.ETD.Date ?? ZDate.Empty, TaxDateDefaultingOption.Description.EstimatedDepartureDate);
					break;
				case TaxDateDefaultingOption.Code.CustomClearanceDate:
					result = new Tuple<ZDate, ZString>(invoicingSupporter?.GetCustomsClearanceDate().Date ?? ZDate.Empty, TaxDateDefaultingOption.Description.CustomClearanceDate);
					break;
				case TaxDateDefaultingOption.Code.PickupDate:
					var pickupDate = invoicingSupporter == null
						? ZDate.Empty
						: invoicingSupporter.ActualPickupDate.IsEmpty
							? invoicingSupporter.ESP.Date
							: invoicingSupporter.ActualPickupDate.Date;
					var pickupDateDescription = invoicingSupporter != null && invoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.LocalCartage
						? TaxDateDefaultingOption.Description.EstimatePickupDate
						: TaxDateDefaultingOption.Description.ActualEstimatePickupDate;

					result = new Tuple<ZDate, ZString>(pickupDate, pickupDateDescription);
					break;
				case TaxDateDefaultingOption.Code.DeliveryDate:
					var deliveryDate = invoicingSupporter == null
						? ZDate.Empty
						: invoicingSupporter.ActualDeliveryDate.IsEmpty
							? invoicingSupporter.ESD.Date
							: invoicingSupporter.ActualDeliveryDate.Date;
					var deliveryDateDescription = invoicingSupporter != null && invoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.LocalCartage
						? TaxDateDefaultingOption.Description.EstimateDeliveryDate
						: TaxDateDefaultingOption.Description.ActualEstimateDeliveryDate;

					result = new Tuple<ZDate, ZString>(deliveryDate, deliveryDateDescription);
					break;
				default:
					break;
			}
			return result;
		}

		public void UpdateBaseExchangeRate(ZString currency, ExchangeRateValidLedgerEnum ledger, ZDecimal newExchangeRate, ZGuid? orgPK = null, bool ignoreNoLedgerExRates = false, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			var exRates = ExchangeRates.Cast<ExchangeRate>().Where(
				x => x.JF_RX_NKRateCurrency == currency &&
				(x.OrgType.ToLedger() == ledger || (!ignoreNoLedgerExRates && x.OrgType == ExchangeRateOrgTypeEnum.None)) &&
				(orgPK == null || x.JF_OH_Org == orgPK.Value) &&
				ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(x.EffectiveInvoiceCurrencyType) == invoiceCurrencyType
			).ToList();

			foreach (var rate in exRates)
			{
				rate.JF_BaseRate = newExchangeRate;

#if DEBUG
				if (Globals.IsTest && SimulateExceptionCondition)
				{
					ExchangeRates.AddRate(RefCurrency.LoadFromCurrencyCode(Factory, currency), newExchangeRate, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);
					SimulateExceptionCondition = false;
				}
#endif
			}
		}

		public void UpdateBaseExchangeRate(ZString currency, ExchangeRateValidLedgerEnum ledger, ZDecimal newExchangeRate, IEnumerable<Charge> charges)
		{
			if (ledger == ExchangeRateValidLedgerEnum.AR)
			{
				foreach (var charge in charges.Where(c => c.JR_RX_NKSellCurrency == currency))
				{
					charge.UpdateSellExRateWithBaseRate(newExchangeRate);
				}

				foreach (var charge in charges.Where(c => c.JR_RX_NKSellInvoiceCurrency == currency && c.BillInInvoiceCurrency))
				{
					charge.OverrideOSSellInvoiceExRateForPosting(newExchangeRate);
				}
			}
			else
			{
				foreach (var charge in charges.Where(c => c.JR_RX_NKCostCurrency == currency))
				{
					charge.JR_OSCostExRate = newExchangeRate;
				}
			}
		}

#if DEBUG
		public bool SimulateExceptionCondition { get; set; }
#endif

		#endregion

		#region IPODCharge Members

		JobCharge IPODCharge.CreateCharge
		{
			get { return Charges.AddNew(); }
		}

		#endregion

		#region Enabler
		public abstract class ActionPermissionSemaphore
		{
			public ActionPermissionSemaphore(bool defaultState)
			{
				State = defaultState;
			}

			protected IDisposable SwitchActionState()
			{
				State = !State;
				return new DisposableAction(delegate
				{ State = !State; });
			}

			public bool IsEnabled
			{
				get { return State; }
			}

			bool State;
		}

		public class ActionEnabler : ActionPermissionSemaphore
		{
			public ActionEnabler()
				: base(false)
			{ }

			public IDisposable Enable()
			{
				return SwitchActionState();
			}
		}
		#endregion

		JobProfitLossCalculation JobProfitLossCalculation => jobProfitLossCalculation ??= new JobProfitLossCalculation(this);
		JobProfitLossCalculation jobProfitLossCalculation;

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal JH_TotalProfitRevenueMargin => JobProfitLossCalculation.TotalMargin;

		#region Profit/Loss Reason

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal JH_ProfitRevenueMargin
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return JH_TotalRevenue == 0 ? 0M : Utilities.Round(JH_ProfitLoss / JH_TotalRevenue * 100M, JH_ProfitRevenueMarginDecimals);
				}
				else
				{
					var totalRevenueExcludingDSB = JH_TotalRevenueExcludingDSB;
					return totalRevenueExcludingDSB == 0 ? 0M : Utilities.Round(JH_ProfitLossExcludingDSB / totalRevenueExcludingDSB * 100M, JH_ProfitRevenueMarginDecimals);
				}
			}
		}

		public ZPropertyInfo JH_ProfitRevenueMarginInfo
		{
			get { return GetZPropertyInfo(nameof(JH_ProfitRevenueMargin)); }
		}

		public ZInt JH_ProfitRevenueMarginDecimals
		{
			get { return 2; }
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal JH_ProfitRevenueMarginPosted
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return JH_TotalRevenuePosted == 0 ? 100.000M : Utilities.Round(JH_ProfitLossPosted / JH_TotalRevenuePosted * 100M, JH_ProfitRevenueMarginDecimals);
				}
				else
				{
					return JH_TotalRevenuePostedExcludingDSB == 0 ? 100.000M : Utilities.Round(JH_ProfitLossPostedExcludingDSB / JH_TotalRevenuePostedExcludingDSB * 100M, JH_ProfitRevenueMarginDecimals);
				}
			}
		}

		public CodeDescriptionPairList ProfitLossReasonCodes
		{
			get { return AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.GetCodeDescriptionPairList(); }
		}

		[List("ProfitLossReasonCodes")]
		public override ZString JH_ProfitLossReasonCode
		{
			get
			{
				return base.JH_ProfitLossReasonCode;
			}
			set
			{
				ZString oldValue = base.JH_ProfitLossReasonCode;
				base.JH_ProfitLossReasonCode = value;
				if (oldValue != base.JH_ProfitLossReasonCode)
				{
					Logs.AddNew(Events.ProfitLossReasonCodeChanged, string.Format("Old: '{0}', New: '{1}'", oldValue, base.JH_ProfitLossReasonCode));
				}
			}
		}

		protected bool JH_ProfitLossReasonCode_ReadOnly
		{
			get { return SecurityHelper != null && !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ProfitLossReasonCode); }
		}

		public bool IsAllowedToModifyChargesFromOtherBranchesOrDepartments
		{
			get { return SecurityHelper != null && SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowEnterModifyCharges); }
		}

		public bool IsAllowedToViewChargesFromOtherBranchesOrDepartments
		{
			get { return SecurityHelper != null && SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowViewCharges); }
		}

		public bool IsAllowedToViewConsolCostsFromOtherBranchesOrDepartments
		{
			get
			{
				var result = false;
				if (PlugInData == null)
				{
					InitializeParentFromGenericJobWithoutSettingDefaults();
				}
				if (PlugInData == null)
				{
					ErrorReporter.ReportOnce("PlugInDataIsNull", CreateDeveloperErrorMessage());
				}
				else if (PlugInData.InvoicingSupporter != null)
				{
					result = PlugInData.InvoicingSupporter.ViewConsolCostFromOtherBRNorDEPSecurityCheckpoint.IsAllowed;
				}
				return result;
			}
		}

		ZString CreateDeveloperErrorMessage()
		{
			var errorMessageBuilder = new ZStringBuilder();
			errorMessageBuilder.Append(ZString.Format((NoResString)"Parent of Job {0} is null.", JH_JobNum));
			errorMessageBuilder.Append(ZString.Format((NoResString)"Job Details: PK = {0}, Job Number = {1}, Parent ID = {2}, Parent Table Code = {3}, Parent Job = {4}, Job Status = {5}, IsInDatabase = {6}, BranchPK = {7}, DepartmentPK = {8}, CompanyPK = {9}",
				PK, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_JH_ParentJob, JH_Status, IsInDatabase.ToYesNoString(), JH_GB, JH_GE, JH_GC));
			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.InvoicingJob);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;
		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region ISecurityOverrideProviderSource Members

		public ISecurityOverrideProvider SecurityOverrideProvider
		{
			get { return ((ISecurityOverrideProviderSource)this).Provider; }
			set { ((ISecurityOverrideProviderSource)this).Provider = value; }
		}

		ISecurityOverrideProvider provider;
		ISecurityOverrideProvider ISecurityOverrideProviderSource.Provider
		{
			get
			{
				if (Factory.IsInTransaction)
				{
					return new DefaultAccessSecurityProvider();
				}
				else
				{
					if (provider == null)
					{
						provider = new DefaultAccessSecurityProvider();
					}
					return provider;
				}
			}
			set
			{
				provider = value;
			}
		}

		#endregion

		#region RevenueRecognition

		public IRevenueRecognition GetRevenueRecognitionOption(AccChargeCode chargeCode)
		{
			return GetRevenueRecognitionOption(chargeCode, null);
		}

		IRevenueRecognition GetRevenueRecognitionOption(AccChargeCode chargeCode, string revenueRecognitionOptionOnlyAccepted = null)
		{
#pragma warning disable CA1820 // Null is treated differently to an empty string by design, so checking IsNullOrEmpty here will break existing functionality.
			if (revenueRecognitionOptionOnlyAccepted == string.Empty)
			{
				return null;
			}
#pragma warning restore CA1820

			IRevenueRecognition result = null;

			string consumerTypeCode = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			string directionCode = Constants.FreightShipmentDirection.Code.All;
			string transportMode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			string broker = RevenueRecognitionLookups.BrokerCodes.All;

			InitializeParentFromGenericJobWithSettingDefaults();
			if (PlugInData != null && !IsPluginDataDeleted)
			{
				if (PlugInData.InvoicingSupporter.ConsumerType != null)
				{
					consumerTypeCode = PlugInData.InvoicingSupporter.ConsumerType.Code;
				}

				if (!Direction.IsEmpty)
				{
					directionCode = Direction;
				}

				if (!PlugInData.InvoicingSupporter.TransportMode.IsEmpty)
				{
					transportMode = (string)PlugInData.InvoicingSupporter.TransportMode;
				}

				if (PlugInData.InvoicingSupporter.Broker != null)
				{
					broker = PlugInData.InvoicingSupporter.Broker.IsProxyOrg(GlbCompany.CurrentCompany) ?
						RevenueRecognitionLookups.BrokerCodes.Internal :
						RevenueRecognitionLookups.BrokerCodes.External;
				}
			}

			if (chargeCode != null)
			{
				result = FindRevenueRecognition(chargeCode.RevenueRecOverrides, consumerTypeCode, directionCode, transportMode, broker);
				if (result == null)
				{
					RevenueRecognitionByChargeGroup revenueRecognitionByChargeGroup = null;
					foreach (RevenueRecognitionByChargeGroup revenueRecognitionByChargeGroupItem in RevenueRecognitionByChargeGroupSetup)
					{
						if (revenueRecognitionByChargeGroupItem.ChargeGroup == chargeCode.AC_ChargeGroup)
						{
							revenueRecognitionByChargeGroup = revenueRecognitionByChargeGroupItem;
							break;
						}
					}
					if (revenueRecognitionByChargeGroup != null)
					{
						result = FindRevenueRecognition(revenueRecognitionByChargeGroup.ChargeGroupSettings, consumerTypeCode, directionCode, transportMode, broker);
					}
				}
			}
			IRevenueRecognition baseRegistryValue = FindRevenueRecognition(RevenueRecognitionSetup, consumerTypeCode, directionCode, transportMode, broker);
			if (result == null)
			{
				result = baseRegistryValue;
			}

			if (result != null)
			{
				bool shouldRevenueRecognitionBeOverridenForQuotedBookings =
					(consumerTypeCode == JobInvoicingConsumerTypes.QuotedBooking.Code || PlugInData?.InvoicingSupporter is BookingInvoicingSupporter)
					&& result.RecognitionDateOptionCode != RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;

				if (shouldRevenueRecognitionBeOverridenForQuotedBookings)
				{
					result = RevenueRecognitionForQSH;
				}
				if (baseRegistryValue != null)
				{
					result.Offset = baseRegistryValue.Offset;
					result.OffsetType = baseRegistryValue.OffsetType;
				}
				else
				{
					result.Offset = 0;
					result.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
				}

				bool revenueRecognitionIsNotOptionOnlyAccepted = revenueRecognitionOptionOnlyAccepted != null && result.RecognitionDateOptionCode != revenueRecognitionOptionOnlyAccepted;
				if (revenueRecognitionIsNotOptionOnlyAccepted)
				{
					result = null;
				}
			}

			return result;
		}

		IRevenueRecognition GetRevenueRecognitionOption_CreateStubIfRecognitionOptionIsNotExist(AccChargeCode chargeCode, ZString revenueRecognitionOptionOnlyAccepted)
		{
			if (revenueRecognitionOptionOnlyAccepted.IsEmpty)
			{
				return null;
			}

			ZDateTime revenueRecognitionDate = GetRevenueRecognitionDate(revenueRecognitionOptionOnlyAccepted);
			IRevenueRecognition revenueRecognition = GetRevenueRecognitionOption(chargeCode, revenueRecognitionOptionOnlyAccepted);
			if (revenueRecognition == null && revenueRecognitionDate.IsEmpty)
			{
				revenueRecognition = new RevenueRecognition();
				revenueRecognition.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
				revenueRecognition.RecognitionDateOptionCode = revenueRecognitionOptionOnlyAccepted;
				revenueRecognition.Offset = 0;
				revenueRecognition.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			}

			return revenueRecognition;
		}

		RevenueRecognitionByChargeGroupCollection RevenueRecognitionByChargeGroupSetup
		{
			get { return Factory.GetCachedValue(FindboxLookupCollections.CachingKey, () => AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.Value); }
		}

		RevenueRecognitionCollection RevenueRecognitionSetup
		{
			get { return Factory.GetCachedValue(FindboxLookupCollections.CachingKey, () => AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value); }
		}

		RevenueRecognition RevenueRecognitionForQSH
		{
			get { return Factory.GetCachedValue(FindboxLookupCollections.CachingKey, () => RevenueRecognition.CreateRevenueRecognitionForQSH()); }
		}

#if DEBUG
		public void ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly()
		{
			Factory.ClearCachedValue<RevenueRecognitionByChargeGroupCollection>(FindboxLookupCollections.CachingKey);
			Factory.ClearCachedValue<RevenueRecognitionCollection>(FindboxLookupCollections.CachingKey);
		}

		public void ClearCacheOfContainsFARCharge_ForTestsOnly(ZGuid jobPk)
		{
			Factory.ClearCachedValue<bool>($"ContainsFARCharge{jobPk}");
		}

#endif

		public ActiveBusinessObjectCollection<JobChargeRevRecognition> RevenueRecognitionCollection
		{
			get
			{
				if (revenueRecognition == null)
				{
					revenueRecognition = new ActiveBusinessObjectCollection<JobChargeRevRecognition>(this);
				}
				else if (shouldRefreshRevenueRecognitionCollection)
				{
					revenueRecognition.RefreshFromDb();
				}
				shouldRefreshRevenueRecognitionCollection = false;
				return revenueRecognition;
			}
		}
		ActiveBusinessObjectCollection<JobChargeRevRecognition> revenueRecognition;

		bool shouldRefreshRevenueRecognitionCollection;

		public ZDateTime GetRevenueRecognitionDate(ZString revenueRecognitionType)
		{
			return GetRevenueRecognitionDate(revenueRecognitionType, false);
		}

		ZDateTime GetRevenueRecognitionDate(ZString revenueRecognitionType, bool getOldJobDatesOnly = false)
		{
			if (revenueRecognitionType.IsEmpty && !getOldJobDatesOnly)
			{
				return ZDateTime.Empty;
			}

			foreach (JobChargeRevRecognition item in RevenueRecognitionCollection)
			{
				if (item.D3_RecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob)
				{
					return item.D3_RecognitionDate;
				}
			}

			if (!getOldJobDatesOnly)
			{
				foreach (JobChargeRevRecognition item in RevenueRecognitionCollection)
				{
					if (item.D3_RecognitionType == revenueRecognitionType)
					{
						return item.D3_RecognitionDate;
					}
				}
			}

			return ZDateTime.Empty;
		}

		public string GetRevenueRecognitionType(AccChargeCode chargeCode)
		{
			string resultRevenueRecognitionType = "";

			ZDateTime oldJobRecognitionDate = GetRevenueRecognitionDate("", true);
			if (!oldJobRecognitionDate.IsEmpty)
			{
				resultRevenueRecognitionType = RevenueRecognitionLookups.CompleteRecognitionDateOptionList.GetCodeFromDescription(
													GetRevenueRecognitionOptionName(oldJobRecognitionDate));
				if (string.IsNullOrEmpty(resultRevenueRecognitionType))
				{
					resultRevenueRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob;
				}
			}
			else
			{
				IRevenueRecognition revenueRecognition = GetRevenueRecognitionOption(chargeCode);
				if (revenueRecognition != null)
				{
					resultRevenueRecognitionType = revenueRecognition.RecognitionDateOptionCode;
				}
			}

			return resultRevenueRecognitionType;
		}

		public override string GetRevenueRecognitionDetails(AccChargeCode chargeCode)
		{
			var broker = PlugInData?.InvoicingSupporter?.Broker;
			var brokerCode = broker == null ? "" :
				broker.IsProxyOrg(GlbCompany.CurrentCompany) ? RevenueRecognitionLookups.BrokerCodes.Internal : RevenueRecognitionLookups.BrokerCodes.External;

			return FormattableString.Invariant(
$@"Revenue Recognition Type: {GetRevenueRecognitionType(chargeCode)} 
JobType: {PlugInData?.InvoicingSupporter?.ConsumerType?.Code}
Direction: {Direction}
Mode: {PlugInData?.InvoicingSupporter?.TransportMode}
Broker: {brokerCode}");
		}

		ZDateTime CalculateRevenueRecognitionDate(IRevenueRecognition revenueRecognition)
		{
			return CalculateRevenueRecognitionDate(revenueRecognition, ZDateTime.Empty);
		}

		ZDateTime CalculateRevenueRecognitionDate(IRevenueRecognition revenueRecognition, ZDateTime dateForRevenueRecognitionOptionOnlyAccepted)
		{
			ZDateTime resultRevenueRecognitionDate = ZDateTime.Empty;

			if (revenueRecognition != null)
			{
				if (!dateForRevenueRecognitionOptionOnlyAccepted.IsEmpty)
				{
					resultRevenueRecognitionDate = dateForRevenueRecognitionOptionOnlyAccepted;
				}
				else
				{
					switch (revenueRecognition.RecognitionDateOptionCode)
					{
						case RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate:
							resultRevenueRecognitionDate = RevenueRecognitionDateConstants.Immediate;
							break;
						case RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate:
							resultRevenueRecognitionDate = JH_A_JOP;
							break;
						case RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure:
							resultRevenueRecognitionDate = RevenueRecognitionDateConstants.JobClosure;
							break;
						case RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction:
							resultRevenueRecognitionDate = CalculateFARRevenueRecognitionDate();
							break;
					}

					if (PlugInData == null)
					{
						InitializeParentFromGenericJobWithSettingDefaults();
					}

					if (PlugInData != null && !IsPluginDataDeleted && resultRevenueRecognitionDate.IsEmpty)
					{
						ZDateTime pluginRevenueRecognitionDate = PlugInData.InvoicingSupporter.GetOperationsSignificantDateByDirection(revenueRecognition.RecognitionDateOptionCode, Direction);
						if (!pluginRevenueRecognitionDate.IsEmpty)
						{
							resultRevenueRecognitionDate = pluginRevenueRecognitionDate;
						}
					}
				}

				if (resultRevenueRecognitionDate > RevenueRecognitionDateConstants.Immediate &&
					resultRevenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate)
				{
					resultRevenueRecognitionDate = OffsetRevenueRecognitionDate(resultRevenueRecognitionDate, revenueRecognition);
				}
			}

			return resultRevenueRecognitionDate;
		}

		ZDateTime ReturnCurrentDateWhenRegistryOn(ZDateTime revenueRecognitionDate)
		{
			var shouldUseCurrentDate = !revenueRecognitionDate.IsEmpty
				&& AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.Value
				&& revenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate
				&& revenueRecognitionDate.Date > ZDateTime.Today.Date;

			return shouldUseCurrentDate ? ZDateTime.Today.Date : revenueRecognitionDate;
		}

		public ZDateTime CalculateFARRevenueRecognitionDate()
		{
			var cacheKey = "Job." + nameof(CalculateFARRevenueRecognitionDate) + "." + PK;
			return Factory.GetCachedValue(cacheKey, CalculateFARRevenueRecognitionDateInternal, CacheStalenessPolicy.StaleBeforeFactorySavingTransaction);

			ZDateTime CalculateFARRevenueRecognitionDateInternal()
			{
				string sqlText = @" SELECT MIN(AH_PostDate) AS PostDate
									FROM
										(SELECT	AH.AH_PostDate AS AH_PostDate
										FROM	dbo.AccTransactionHeader AH
												INNER JOIN	dbo.AccTransactionLines AL ON AL.AL_AH = AH.AH_PK
										WHERE	AH.AH_Ledger = 'AR'
												AND AH.AH_TransactionType in ('INV', 'CRD')
												AND AL.AL_JH is Not NULL
												AND AL.AL_JH = @JobPK
												AND AL.AL_LineType = 'REV'

										UNION ALL

										SELECT	AH.AH_PostDate
										FROM	dbo.AccTransactionHeader AH
										WHERE 	AH.AH_Ledger = 'AR'
												AND AH.AH_TransactionType in ('INV', 'CRD')
												AND AH.AH_JH is Not NULL
												AND AH.AH_JH = @JobPK
										) AllJobs";

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@JobPK", PK, JobHeaderSchema.PK);

				var readOnlyFactory = Factory.GetCachedReadOnlyFactory();
				var collection = new DynamicBusinessObjectCollection(readOnlyFactory);
				collection.Load(sqlText, parameters);
				if (collection.Count > 0 && collection[0]["PostDate"] is ZDateTime dt && !dt.IsEmpty)
				{
					return dt;
				}
				else
				{
					return RevenueRecognitionDateConstants.PostDateOfFirstARTransaction;
				}
			}
		}
		public void ApplyRevenueRecognitionDate(TransactionLine invoicingLine)
		{
			if (invoicingLine.AL_JH != PK)
			{
#if DEBUG
				throw new InvalidOperationException("The InvoicingLine doesn't belong to the Job.");
#else
				return;
#endif
			}

			IRevenueRecognition revenueRecognitionOption = null;
			ZDateTime revenueRecognitionDate = GetRevenueRecognitionDate(invoicingLine.AL_RevRecognitionType);
			bool updateOldStyleCustomClearanceJobChargeRevRecognition = revenueRecognitionDate == RevenueRecognitionDateConstants.CustomsClearanceDate;
			bool shouldUpdateReverseDateForREVandCSTLines = false;

			if (revenueRecognitionDate.IsEmpty || updateOldStyleCustomClearanceJobChargeRevRecognition)
			{
				if (revenueRecognitionDate.IsEmpty)
				{
					revenueRecognitionOption = GetRevenueRecognitionOption(invoicingLine.ChargeCode, invoicingLine.AL_RevRecognitionType);
					revenueRecognitionDate = CalculateRevenueRecognitionDate(revenueRecognitionOption);
				}
				else if (updateOldStyleCustomClearanceJobChargeRevRecognition)
				{
					revenueRecognitionOption = GetRevenueRecognitionOption(invoicingLine.ChargeCode, RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate);
					ZDateTime pluginRevenueRecognitionDate = CalculateRevenueRecognitionDate(revenueRecognitionOption);
					if (!pluginRevenueRecognitionDate.IsEmpty)
					{
						revenueRecognitionDate = pluginRevenueRecognitionDate;
					}
				}

				if (invoicingLine is InvoicingLineBase line && line.InvoiceBase != null && line.InvoiceBase.IsARInvoiceOrCreditNote &&
					revenueRecognitionDate == RevenueRecognitionDateConstants.PostDateOfFirstARTransaction)
				{
					revenueRecognitionDate = line.AL_PostDate;
					if (revenueRecognitionOption != null && revenueRecognitionOption.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction)
					{
						revenueRecognitionDate = OffsetRevenueRecognitionDate(revenueRecognitionDate, revenueRecognitionOption);
					}
				}

				revenueRecognitionDate = ReturnCurrentDateWhenRegistryOn(revenueRecognitionDate);
				revenueRecognitionDate = AskShouldUseImmediateRevenueRecognisedDate(revenueRecognitionDate);

				if (revenueRecognitionDate >= RevenueRecognitionDateConstants.Immediate &&
					revenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate)
				{
					if (revenueRecognitionOption != null)
					{
						bool shouldDeferRecognition = false;
						if (updateOldStyleCustomClearanceJobChargeRevRecognition)
						{
							UpdateJobChargeRevRecognition(RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, revenueRecognitionDate);
						}
						else if (IsRevenueRecognitionShouldBeDeferred(revenueRecognitionOption.RecognitionDateOptionCode))
						{
							shouldDeferRecognition = true;
						}
						else
						{
							GetOrCreateJobChargeRevRecognition(revenueRecognitionOption.RecognitionDateOptionCode, revenueRecognitionDate);
						}

						if (shouldDeferRecognition)
						{
							revenueRecognitionDate = ZDateTime.Empty;
						}
						else
						{
							bool dummyBoolValue = false;
							foreach (Charge charge in Charges)
							{
								ApplyRevenueRecognitionDate(charge, revenueRecognitionOption.RecognitionDateOptionCode, ZDateTime.Empty, false, false, true, null, out dummyBoolValue);
							}

							shouldUpdateReverseDateForREVandCSTLines = true;
						}
					}
				}
			}

			var isFARRecognitionApplied = ApplyFARRevenueRecognitionDate(invoicingLine);
			shouldUpdateReverseDateForREVandCSTLines |= isFARRecognitionApplied;

			if (shouldUpdateReverseDateForREVandCSTLines)
			{
				bool dummyBoolValue = false;
				UpdateReverseDateForREVandCSTLines(invoicingLine, out dummyBoolValue);
			}

			if (!revenueRecognitionDate.IsEmpty)
			{
				invoicingLine.UpdateAL_ReverseDate();
			}
		}

		bool ApplyFARRevenueRecognitionDate(TransactionLine invoicingLine)
		{
			var isRecognitionApplied = false;

			if (invoicingLine is InvoicingLineBase line && line.InvoiceBase != null && line.InvoiceBase.IsARInvoiceOrCreditNote)
			{
				var revenueRecognitionDate = GetRevenueRecognitionDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction);

				if (revenueRecognitionDate.IsEmpty)
				{
					var cacheKey = $"ContainsFARCharge{PK}";
					var containsFARCharge = Factory.GetCachedValue(cacheKey, ContainsFARCharge, CacheStalenessPolicy.StaleOnFactorySave);

					if (containsFARCharge)
					{
						revenueRecognitionDate = CalculateFARRevenueRecognitionDate();

						if (revenueRecognitionDate == RevenueRecognitionDateConstants.PostDateOfFirstARTransaction)
						{
							revenueRecognitionDate = line.AL_PostDate;
						}

#if DEBUG
						if (Globals.IsTest && RecognitionDateOffset_NeedToUpdateFAR_ForTestOnly.HasValue)
						{
							revenueRecognitionDate = revenueRecognitionDate.AddDays(RecognitionDateOffset_NeedToUpdateFAR_ForTestOnly.Value);
						}
#endif

						revenueRecognitionDate = ReturnCurrentDateWhenRegistryOn(revenueRecognitionDate);
						revenueRecognitionDate = AskShouldUseImmediateRevenueRecognisedDate(revenueRecognitionDate);

						if (revenueRecognitionDate >= RevenueRecognitionDateConstants.Immediate &&
							revenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate)
						{
							GetOrCreateJobChargeRevRecognition(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction, revenueRecognitionDate);
							isRecognitionApplied = true;
						}
					}
				}
			}

			return isRecognitionApplied;

			bool ContainsFARCharge() => Charges.Cast<Charge>().Any(x => x.CostRecognition == RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction || x.SellRecognition == RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction);
		}

#if DEBUG
		internal int? RecognitionDateOffset_NeedToUpdateFAR_ForTestOnly;
#endif

		bool IsRevenueRecognitionShouldBeDeferred(string optionCode)
		{
			string[] optionsInvalidForDeferredRecognition =
			{
				RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate,
				RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure,
				RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate,
				RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob,
				RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction,
			};

			return JH_Status != JobHeaderStatus.Complete.Code && JH_Status != JobHeaderStatus.JobReadyForFinancialClosure.Code && (string.IsNullOrEmpty(optionCode) ||
				AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.Value && !optionsInvalidForDeferredRecognition.Contains(optionCode));
		}

		public void ApplyRevenueRecognitionDateForWholeJob(out bool areREVandCSTLinesUpdated)
		{
			areREVandCSTLinesUpdated = false;
			ApplyRevenueRecognitionDateForWholeJob(ZDateTime.Empty, null, out areREVandCSTLinesUpdated);
		}

		void ApplyRevenueRecognitionDateForWholeJob(ZDateTime dateForRevenueRecognitionOptionOnlyAccepted, string revenueRecognitionOptionToExclude, out bool areREVandCSTLinesUpdated)
		{
			areREVandCSTLinesUpdated = false;
			bool lineUpdated = false;
			foreach (var charge in GetChargesToRecognizeCost(revenueRecognitionOptionToExclude))
			{
				ApplyRevenueRecognitionDate(charge, charge.CostRecognition, dateForRevenueRecognitionOptionOnlyAccepted, true, charge.IsCostPosted, false, null, out lineUpdated);
				areREVandCSTLinesUpdated |= lineUpdated;
			}

			foreach (var charge in GetChargesToRecognizeSell(revenueRecognitionOptionToExclude))
			{
				ApplyRevenueRecognitionDate(charge, charge.SellRecognition, dateForRevenueRecognitionOptionOnlyAccepted, true, charge.IsRevenuePosted, false, null, out lineUpdated);
				areREVandCSTLinesUpdated |= lineUpdated;
			}

			foreach (var line in GetUnrecognizedTransactionLines(revenueRecognitionOptionToExclude))
			{
				ApplyRevenueRecognitionDate(null, line.AL_RevRecognitionType, dateForRevenueRecognitionOptionOnlyAccepted, true, true, false, line.ChargeCode, out lineUpdated);
				areREVandCSTLinesUpdated |= lineUpdated;
			}
		}

		public void ApplyRevenueRecognitionDate(BaseCharge charge)
		{
			ApplyRevenueRecognitionDateForCostPart(charge);
			ApplyRevenueRecognitionDateForSellPart(charge);
		}

		public void ApplyRevenueRecognitionDateForCostPart(BaseCharge charge)
		{
			if (ShouldRecognizeChargeCost(charge))
			{
				ApplyRevenueRecognitionDate(charge, charge.CostRecognition, ZDateTime.Empty, true, charge.IsCostPosted, true, null, out bool dummyBoolValue);
			}
		}

		public void ApplyRevenueRecognitionDateForSellPart(BaseCharge charge)
		{
			if (ShouldRecognizeChargeSell(charge))
			{
				ApplyRevenueRecognitionDate(charge, charge.SellRecognition, ZDateTime.Empty, true, charge.IsRevenuePosted, true, null, out bool dummyBoolValue);
			}
		}

		Charge[] GetChargesToRecognizeCost(string revenueRecognitionOptionToExclude = null)
		{
			return Charges.Where(x => ShouldRecognizeChargeCost(x, revenueRecognitionOptionToExclude)).ToArray();
		}

		Charge[] GetChargesToRecognizeSell(string revenueRecognitionOptionToExclude = null)
		{
			return Charges.Where(x => ShouldRecognizeChargeSell(x, revenueRecognitionOptionToExclude)).ToArray();
		}

		internal bool ShouldRecognizeChargeCost(BaseCharge charge, string revenueRecognitionOptionToExclude = null)
		{
			return !charge.IsCostRecognized && !charge.CostRecognition.IsEmpty &&
				(string.IsNullOrEmpty(revenueRecognitionOptionToExclude) || charge.CostRecognition != revenueRecognitionOptionToExclude);
		}

		internal bool ShouldRecognizeChargeSell(BaseCharge charge, string revenueRecognitionOptionToExclude = null)
		{
			return !charge.IsSellRecognized && !charge.SellRecognition.IsEmpty &&
				(string.IsNullOrEmpty(revenueRecognitionOptionToExclude) || charge.SellRecognition != revenueRecognitionOptionToExclude);
		}

		internal TransactionLine[] GetUnrecognizedTransactionLines(string revenueRecognitionOptionToExclude = null)
		{
			var result = Factory.Load<TransactionLine>(new TransactionLinesCollection(Factory, GetUnrecognisedLinesQuery(null, null)).CompleteFilter);
			return result.Where(x => !x.AL_RevRecognitionType.IsEmpty && (string.IsNullOrEmpty(revenueRecognitionOptionToExclude) || x.AL_RevRecognitionType != revenueRecognitionOptionToExclude)).ToArray();
		}

		void ApplyRevenueRecognitionDate(
			BaseCharge charge,
			ZString revenueRecognitionOptionOnlyAccepted,
			ZDateTime dateForRevenueRecognitionOptionOnlyAccepted,
			bool updateREVandCSTReverseDate,
			bool useStubIfRecognitionOptionIsNotExist,
			bool canDeferRecognition,
			AccChargeCode chargeCode,
			out bool areREVandCSTLinesUpdated)
		{
			areREVandCSTLinesUpdated = false;

			if (charge != null && charge.JR_JH != PK)
			{
				return;  // charge collection may contain charges from the child Jobs which we should skip
			}

			ZDateTime revenueRecognitionDate = GetRevenueRecognitionDate(revenueRecognitionOptionOnlyAccepted);
			bool updateOldStypeJobClosureJobChargeRevRecognition =
					revenueRecognitionDate == RevenueRecognitionDateConstants.JobClosure &&
					revenueRecognitionOptionOnlyAccepted == RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;

			AccChargeCode chargeCodeForRecognition = charge != null ? charge.ChargeCode : chargeCode;
			IRevenueRecognition revenueRecognition = useStubIfRecognitionOptionIsNotExist ?
				GetRevenueRecognitionOption_CreateStubIfRecognitionOptionIsNotExist(chargeCodeForRecognition, revenueRecognitionOptionOnlyAccepted) :
				GetRevenueRecognitionOption(chargeCodeForRecognition, revenueRecognitionOptionOnlyAccepted);

			if (revenueRecognition != null || updateOldStypeJobClosureJobChargeRevRecognition)
			{
				if (revenueRecognitionDate.IsEmpty || updateOldStypeJobClosureJobChargeRevRecognition)
				{
					if (revenueRecognitionDate.IsEmpty)
					{
						revenueRecognitionDate = CalculateRevenueRecognitionDate(revenueRecognition, dateForRevenueRecognitionOptionOnlyAccepted);
					}
					else if (updateOldStypeJobClosureJobChargeRevRecognition)
					{
						revenueRecognitionDate = dateForRevenueRecognitionOptionOnlyAccepted;
					}

					revenueRecognitionDate = ReturnCurrentDateWhenRegistryOn(revenueRecognitionDate);
					revenueRecognitionDate = AskShouldUseImmediateRevenueRecognisedDate(revenueRecognitionDate);

					if (revenueRecognitionDate >= RevenueRecognitionDateConstants.Immediate &&
						revenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate)
					{
						JobValidation jobValidation = Validation as JobValidation;
						if (jobValidation != null)
						{
							string error = jobValidation.GetRevenueRecognitionDateNotInGLPeriodError(revenueRecognitionDate);
							if (string.IsNullOrEmpty(error))
							{
								bool shouldDeferRecognition = false;
								if (updateOldStypeJobClosureJobChargeRevRecognition)
								{
									UpdateJobChargeRevRecognition(RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, revenueRecognitionDate);
									GetOrCreateJobChargeRevRecognition(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure, revenueRecognitionDate);
								}
								else if (canDeferRecognition && IsRevenueRecognitionShouldBeDeferred(revenueRecognition.RecognitionDateOptionCode))
								{
									shouldDeferRecognition = true;
								}
								else
								{
									GetOrCreateJobChargeRevRecognition(revenueRecognition.RecognitionDateOptionCode, revenueRecognitionDate);
								}

								if (shouldDeferRecognition)
								{
									revenueRecognitionDate = ZDateTime.Empty;
								}
								else if (updateREVandCSTReverseDate)
								{
									UpdateReverseDateForREVandCSTLines(null, out areREVandCSTLinesUpdated);
									if (charge != null && AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
									{
										charge.MarkAsNeedingValidation();
									}
								}
							}
						}
					}
				}
				else if (updateREVandCSTReverseDate)
				{
					UpdateReverseDateForREVandCSTLines(null, out areREVandCSTLinesUpdated);
				}
			}
		}

		void UpdateReverseDateForREVandCSTLines(TransactionLine excludedLine, out bool areREVandCSTLinesUpdated)
		{
			areREVandCSTLinesUpdated = false;
			ZString[] recognitionTypes = RevenueRecognitionCollection.Select(revenueRecognition => revenueRecognition.D3_RecognitionType).ToArray();
			if (recognitionTypes.Length > 0)
			{
				ZQuery filter = GetUnrecognisedLinesQuery(recognitionTypes, excludedLine);
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
				var linesCollection = Factory.Load<TransactionLine>(new TransactionLinesCollection(Factory, filter).CompleteFilter);
				foreach (TransactionLine line in linesCollection)
				{
					line.UpdateAL_ReverseDate();
					if (line.HasChanges)
					{
						areREVandCSTLinesUpdated = true;
					}
				}
			}
		}

		internal ZQuery GetUnrecognisedLinesQuery(ZString[] recognitionTypes, TransactionLine excludedLine)
		{
			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_JH, PK);
			if (excludedLine != null)
			{
				filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, excludedLine.PK);
			}
			filter.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, ZDateTime.Empty);
			filter.AddToFilter(AccTransactionLinesSchema.AL_LineType, new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue });

			if (recognitionTypes != null && recognitionTypes.Length > 0)
			{
				filter.AddToFilter(AccTransactionLinesSchema.AL_RevRecognitionType, recognitionTypes);
			}
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, JH_GC);

			return filter;
		}

		JobChargeRevRecognition GetOrCreateJobChargeRevRecognition(ZString recognitionType, ZDateTime recognitionDate)
		{
			var revenueRecognition = RevenueRecognitionCollection.FirstOrDefault(x => x.D3_RecognitionType == recognitionType && x.D3_RecognitionDate == recognitionDate);
			if (revenueRecognition != null)
			{
				return revenueRecognition;
			}

			revenueRecognition = RevenueRecognitionCollection.AddNew();
			revenueRecognition.D3_JH = PK;
			revenueRecognition.D3_RecognitionType = recognitionType;
			revenueRecognition.D3_RecognitionDate = recognitionDate;
			RegisterEditableChildObject(revenueRecognition);
			return revenueRecognition;
		}

		JobChargeRevRecognition UpdateJobChargeRevRecognition(ZString recognitionType, ZDateTime recognitionDate)
		{
			var revenueRecognition = RevenueRecognitionCollection.FirstOrDefault(x => x.D3_RecognitionType == recognitionType);
			if (revenueRecognition != null)
			{
				revenueRecognition.D3_RecognitionDate = recognitionDate;
			}
			RegisterEditableChildObject(revenueRecognition);
			return revenueRecognition;
		}

		ZDateTime OffsetRevenueRecognitionDate(ZDateTime revenueRecognitionDate, IRevenueRecognition revenueRecognition)
		{
			if (revenueRecognitionDate > RevenueRecognitionDateConstants.Immediate &&
				revenueRecognitionDate < RevenueRecognitionDateConstants.MinSpecialDate)
			{
				if (revenueRecognition.OffsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Days)
				{
					revenueRecognitionDate = revenueRecognitionDate.AddDays(revenueRecognition.Offset);
				}
				else if (revenueRecognition.OffsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Periods)
				{
					AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
					AccPeriodManagement period = periodCalculator.GetPeriodManagementByOffset(
						periodCalculator.GetPeriodManagementFromDate(revenueRecognitionDate), (sbyte)revenueRecognition.Offset);
					revenueRecognitionDate = period != null ? period.AM_EndDate : RevenueRecognitionDateConstants.DateAfterLastPeriod;
				}
			}

			return revenueRecognitionDate;
		}

		IRevenueRecognition FindRevenueRecognition(IEnumerable<BusinessObject> revenueRecognitions, string jobType, string direction, string mode, string broker)
		{
			List<IRevenueRecognition> result = new List<IRevenueRecognition>();
			foreach (IRevenueRecognition revRecognition in revenueRecognitions)
			{
				if ((jobType == null ||
					 string.Equals(revRecognition.JobType, jobType, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(revRecognition.JobType, RevenueRecognitionLookups.JobTypeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase))
					&&
					(direction == null ||
					 string.Equals(revRecognition.DirectionCode, Constants.FreightShipmentDirection.Code.All, StringComparison.OrdinalIgnoreCase) ||
					 revRecognition.DirectionCode == "" ||
					 string.Equals(revRecognition.DirectionCode, direction, StringComparison.OrdinalIgnoreCase))
					&&
					(mode == null ||
					 string.Equals(revRecognition.Mode, mode, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(revRecognition.Mode, RevenueRecognitionLookups.ModeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 revRecognition.Mode == "")
					 &&
					(broker == null ||
					 string.Equals(revRecognition.BrokerCode, broker, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(revRecognition.BrokerCode, RevenueRecognitionLookups.BrokerCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 revRecognition.BrokerCode == ""))
				{
					result.Add(revRecognition);
				}
			}

			return result.Count > 0 ? result[0] : null;
		}

		public static string GetRevenueRecognitionOptionName(ZDateTime revenueRecognitionDate)
		{
			string revenueRecognitionOptionName = string.Empty;
			if (revenueRecognitionDate == RevenueRecognitionDateConstants.Immediate)
			{
				revenueRecognitionOptionName = RevenueRecognitionLookups.RecognitionDateOptionDescriptions.Immediate;
			}
			else if (revenueRecognitionDate == RevenueRecognitionDateConstants.JobClosure)
			{
				revenueRecognitionOptionName = RevenueRecognitionLookups.RecognitionDateOptionDescriptions.JobClosure;
			}
			else if (revenueRecognitionDate == RevenueRecognitionDateConstants.PostDateOfFirstARTransaction)
			{
				revenueRecognitionOptionName = RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction;
			}
			else if (revenueRecognitionDate == RevenueRecognitionDateConstants.CustomsClearanceDate)
			{
				revenueRecognitionOptionName = RevenueRecognitionLookups.RecognitionDateOptionDescriptions.CustomsClearanceDate;
			}
			return revenueRecognitionOptionName;
		}

		public void FixRelatedLinesWithEmptyRecognitionType(BusinessObjectFactory factoryForFixedLines)
		{
			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_JH, PK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_RevRecognitionType, string.Empty);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			var linesCollection = factoryForFixedLines.Load<TransactionLine>(new ZQuery(new TransactionLinesCollection(factoryForFixedLines, filter).CompleteFilter));

			foreach (TransactionLine line in linesCollection)
			{
				if (line.AL_LineType == TransactionLineTypes.Accrual || line.AL_LineType == TransactionLineTypes.WIP)
				{
					var existedRecognitionTypes = from revRecognition in RevenueRecognitionCollection
												  where revRecognition.D3_RecognitionDate == line.AL_PostDate
												  select revRecognition.D3_RecognitionType;
					ZString existedRecognitionType = existedRecognitionTypes.FirstOrDefault();
					if (!existedRecognitionType.IsEmpty)
					{
						line.AL_RevRecognitionType = existedRecognitionType;
					}
				}
				else
				{
					if (line.AL_PostDate == line.AL_ReverseDate)
					{
						line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
					}
					if (line.AL_RevRecognitionType.IsEmpty && !line.AL_ReverseDate.IsEmpty)
					{
						var existedRecognitionTypes = from revRecognition in RevenueRecognitionCollection
													  where revRecognition.D3_RecognitionDate == line.AL_ReverseDate
													  select revRecognition.D3_RecognitionType;
						ZString existedRecognitionType = existedRecognitionTypes.FirstOrDefault();
						if (!existedRecognitionType.IsEmpty)
						{
							line.AL_RevRecognitionType = existedRecognitionType;
						}
					}
				}
				if (line.AL_RevRecognitionType.IsEmpty)
				{
					var recognitionOption = GetRevenueRecognitionOption(line.ChargeCode);
					if (recognitionOption != null)
					{
						line.AL_RevRecognitionType = recognitionOption.RecognitionDateOptionCode;
					}
				}
			}
		}

		#region RevenueRecognitionDates

		public ZString RevenueRecognitionDates
		{
			get { return GetRevenueRecognitionDatesAsString(RevenueRecognitionCollection); }
		}

		public ZPropertyInfo RevenueRecognitionDatesInfo
		{
			get { return GetZPropertyInfo(nameof(RevenueRecognitionDates)); }
		}

		internal static string GetRevenueRecognitionDatesAsString(IEnumerable<JobChargeRevRecognition> revenueRecognitionCollection)
		{
			ZString result = ZString.Empty;

			if (revenueRecognitionCollection != null)
			{
				string delimeter = ", ";
				foreach (JobChargeRevRecognition revRec in revenueRecognitionCollection.OrderBy(x => x.D3_RecognitionType).ThenBy(x => x.D3_RecognitionDate))
				{
					string revenueRecognitionDate = string.Empty;

					if (revRec.D3_RecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate)
					{
						revenueRecognitionDate = revRec.D3_RecognitionType;
					}
					else if (revRec.D3_RecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob)
					{
						string recognitionType = RevenueRecognitionLookups.CompleteRecognitionDateOptionList.GetCodeFromDescription(
													Job.GetRevenueRecognitionOptionName(revRec.D3_RecognitionDate));
						if (!string.IsNullOrEmpty(recognitionType))
						{
							revenueRecognitionDate = revRec.D3_RecognitionType + ' ' + recognitionType;
						}
					}

					if (string.IsNullOrEmpty(revenueRecognitionDate))
					{
						revenueRecognitionDate = revRec.D3_RecognitionType + ' ' + revRec.D3_RecognitionDate.ToShortDateString();
					}

					result += (result.IsEmpty ? "" : delimeter) + revenueRecognitionDate;
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region ParentOperationalJobRefChanged

		public void ParentOperationalJobRefChanged(ZString originalCostReference, ZString newCostReference, bool isParentInDatabase)
		{
			foreach (Charge charge in Charges)
			{
				if (charge.JR_CostReference == originalCostReference && charge.JR_E6.IsEmpty && (!charge.IsInDatabase || isParentInDatabase))
				{
					charge.JR_CostReference = newCostReference;
				}
			}

			UpdateFilteredChargesFilter();
		}

		#endregion

		#region Test
#if DEBUG
		public ZBool RefreshBindingTotalFieldsIsCalledForTest;

		public override void LoadCharges_ForTestOnly()
		{
			Charges.Load();
		}

#endif
		#endregion

		#region IExchangeRateSource Members

		public ZString Description
		{
			get { return Res.GetString("f47084ae-3119-452a-a11c-f3153da2a4e4", "Job Billing Exchange Rate Configuration"); }
		}

		public ZDecimal? GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);

			var org = Factory.Load<OrgHeader>(orgPk);
			var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(Company, ledger, currencyCode);
			var result = AccExchangeRateConfigurationRateFinder.GetExchangeRate(ExchangeRateConfigurationRateConsumer, currency, org, ledger, invoiceCurrencyType);
			if (result == 0M)
			{
				return null;
			}

			return result;
		}

		public ControllerID SourceController
		{
			get { return null; }
		}

		public ZGuid SourcePK
		{
			get { return ZGuid.Empty; }
		}

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new JobCriticalValidation(this); }
		}
		public IEnumerator<IExchangeRate> GetEnumerator()
		{
			return ExchangeRates.Cast<IExchangeRate>().GetEnumerator();
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}
		#endregion

		#region IAdditionalProperyValuesProvider

		ZString IAdditionalPropertyValuesProvider.GetAdditionalProperyValues()
		{
			return $"Job Type = {JobType}";
		}

		#endregion

		#region Cross Trade Debtor Defaulting

		/* please do not replace this with job.IsCrossTrade or Job.GetInvoicingSupporter().IsCrossTrade, as it will produce different result.
		 Job.GetInvoicingSupporter()?.GetJobDirection() has fallback logic (IsImport -> IsExport -> IsCrossTrade -> IsDomestic) which handles a Singapore Customs Declaration job that can be both Import and CrossTrade  */
		public override bool CanCrossTradeDebtorDefaultingBeApplied => IsCrossTradeDebtorDefaultingFunctionalityEnabled() && (GetInvoicingSupporter()?.GetJobDirection() ?? Directions.Unknown) == Directions.CrossTrade;

		public override CrossTradeDebtorDefaultingParam GetParamForCrossTradeDebtorDefaulting()
		{
			return new CrossTradeDebtorDefaultingParam
			{
				JobType = JobType,
				TransportMode = TransportMode
			};
		}

		#endregion

		#region NotifyRegisteredChildEditable

		protected override void NotifyRegisteredChildEditable()
		{
			var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			infoCollector.AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobRegisteredAsEditableChild,
				() => FormattableString.Invariant($@"JH_GC: {JH_GC}, JH_Parent_ID: {JH_ParentID}, JH_ParentTableCode: {JH_ParentTableCode}, CurrentCompany: {GlbCompany.CurrentCompany.PK}({GlbCompany.CurrentCompany.GC_Code}), job.IsInDatabase: {IsInDatabase}, StackTrace ->\r\n {System.Environment.StackTrace}"),
				useNeverClearedInfo: true);
		}

		#endregion
	}

#if DEBUG
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public sealed class JobReferenceNumberGenerator : IJobReferenceNumberGenerator
	{
		void IJobReferenceNumberGenerator.AddJobDeletedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory)
		{
			EnterpriseBusinessObject bO = null;

			var genericJob = parentJobPK.IsValid && !parentTableCode.IsEmpty ? factory.LoadGenericJob<GenericJob.GenericJob>(parentJobPK, parentTableCode) : null;
			if (genericJob != null)
			{
				bO = genericJob.Consumer as EnterpriseBusinessObject;
			}
			if (bO != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				bO.Logs.AddNew(Events.EditedARecord, string.Format("Deleted Job Record - {0}", jobLocalReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void IJobReferenceNumberGenerator.AddJobDeactivatedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory)
		{
			EnterpriseBusinessObject bO = null;

			var genericJob = parentJobPK.IsValid && !parentTableCode.IsEmpty ? factory.LoadGenericJob<GenericJob.GenericJob>(parentJobPK, parentTableCode) : null;
			if (genericJob != null)
			{
				bO = genericJob.Consumer as EnterpriseBusinessObject;
			}
			if (bO != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				bO.Logs.AddNew(Events.EditedARecord, string.Format("Job marked as Inactive - {0}", jobLocalReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void IJobReferenceNumberGenerator.AddJobActivatedRecordToParentJobLog(ZGuid parentJobPK, ZString parentTableCode, ZString jobLocalReference, BusinessObjectFactory factory, ZDateTime eventTime)
		{
			EnterpriseBusinessObject bO = null;

			var genericJob = parentJobPK.IsValid && !parentTableCode.IsEmpty ? factory.LoadGenericJob<GenericJob.GenericJob>(parentJobPK, parentTableCode) : null;
			if (genericJob != null)
			{
				bO = genericJob.Consumer as EnterpriseBusinessObject;
			}
			if (bO != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				bO.Logs.AddNew(Events.EditedARecord, string.Format("Job marked as Active - {0}", jobLocalReference), eventTime.ToOffset());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		ZString IJobReferenceNumberGenerator.GenerateLocalJobReferenceNumber(ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK, BusinessObjectFactory factory)
		{
			ZString result = ZString.Empty;
			GlbBranch branch = factory.Load<GlbBranch>(branchPK);
			GlbDepartment department = factory.Load<GlbDepartment>(departmentPK);

			var customisationList = AccountingConfigurationRegistry.Instance.JobsNumberSequenceCustomisation.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty)
				.Cast<TransactionNumberSequenceCustomisation>()
				.OrderBy(x => x.Order)
				.ToArray();

			ZString nonConfigurableKey = "JHJLR";

			if (nonConfigurableKey == GetKey(customisationList, nonConfigurableKey, false, branch, department))
			{
				result = Env.NumberFountains.GetLocalJobRefNumberGeneratorFountain(nonConfigurableKey).GetNextFormatted(factory);
			}
			else
			{
				string fountainKey = GetKey(customisationList, nonConfigurableKey, true, branch, department);
				long seed = Env.NumberFountains.GetLocalJobRefNumberGeneratorFountain(fountainKey).GetNext(factory);
				result = GenerateNumber(seed, customisationList, branch, department);
			}
			return result;
		}

		string GenerateNumber(long seed, IEnumerable<TransactionNumberSequenceCustomisation> customisationList, GlbBranch branch, GlbDepartment department)
		{
			StringBuilder builder = new StringBuilder();
			foreach (TransactionNumberSequenceCustomisation element in customisationList)
			{
				if (element.Include)
				{
					string value = GetValue(element, seed, branch, department);
					builder.Append(value);
				}
			}

			return builder.ToString();
		}

		string GetKey(IEnumerable<TransactionNumberSequenceCustomisation> customisationList, ZString nonUserConfigurableKey, bool fountainOnly, GlbBranch branch, GlbDepartment department)
		{
			StringBuilder builder = new StringBuilder(nonUserConfigurableKey);

			foreach (TransactionNumberSequenceCustomisation element in customisationList)
			{
				if (element.Include && (!fountainOnly || element.Fountain) && element.ElementName != TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber)
				{
					builder.Append(GetValue(element, 0, branch, department));
				}
			}

			return builder.ToString();
		}

		string GetValue(TransactionNumberSequenceCustomisation element, long seed, GlbBranch branch, GlbDepartment department)
		{
			string result = string.Empty;
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			switch (element.ElementName)
			{
				case TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber:
					result = seed.ToString("D" + element.Length, CultureInfo.InvariantCulture);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.JobHeaderBranchCode:
					result = branch.GB_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.JobHeaderDepartmentCode:
					result = department.GE_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement1:
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement2:
					result = element.Code;
					break;
			}

			return result;
		}
	}
}
