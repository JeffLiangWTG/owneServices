using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public class PeriodicInvoice : PeriodicInvoiceBase, IInvoiceTerms
	{
		public PeriodicInvoice(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Initialization(PeriodicInvoiceBase periodicInvoiceCopy, ZGuid debtorPK, string invoiceType, bool isPartOfPeriodicInvoiceBulk)
		{
			using (ClearJobsSuspender.GetSuspender())
			using (ClearMiscInvoicesSuspender.GetSuspender())
			{
				DebtorPK = debtorPK;
				InvoiceType = invoiceType;
				CurrencyNK = periodicInvoiceCopy.CurrencyNK;
				InvoiceDate = periodicInvoiceCopy.InvoiceDate;
				PostDate = periodicInvoiceCopy.PostDate;
				IsPartOfPeriodicInvoiceBulk = isPartOfPeriodicInvoiceBulk;

				using (JobTypeList_OnPairChangedSuspender.GetSuspender())
				{
					for (int i = 0; i < periodicInvoiceCopy.JobTypeList.Count; i++)
					{
						ZString desc = periodicInvoiceCopy.JobTypeList[i].Description;
						if (JobTypeList[desc] != null)
						{
							JobTypeList[desc].Value = periodicInvoiceCopy.JobTypeList[i].Value;
						}
					}
				}
			}
		}

		public string[] PreviewPeriodicInvoiceAndReturnErrors(bool previewOnly = false, EventHandler<CriticalPostingErrorEventArgs> eventHandler = null)
		{
			var newFactory = new BusinessObjectFactory();
			var invoiceCopy = new PeriodicInvoiceLightForBulkPosting(newFactory);

			var errors = invoiceCopy.InitalizeAndValidate(new PeriodicInvoiceBulkPoster.InvoiceInfo(this));
			if (errors.Length == 0)
			{
				var previewPostManager = new PeriodicInvoicePostManager(invoiceCopy);

				using (new DisposableAction(
				() => { if (eventHandler != null) { previewPostManager.OnCriticalPostError += eventHandler; } },
				() => { if (eventHandler != null) { previewPostManager.OnCriticalPostError -= eventHandler; } }))
				{
					previewPostManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
				}

				foreach (InvoicingBase postedInvoice in previewPostManager.Poster.PostedInvoices)
				{
					using (var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(postedInvoice)))
					{
						printTask.Run(!previewOnly);
					}
				}
#if DEBUG
				if (Globals.IsTest)
				{
					previewPostManager_ForTestOnly = previewPostManager;
				}
#endif
			}

			return errors;
		}

#if DEBUG
		public PeriodicInvoicePostManager previewPostManager_ForTestOnly;
#endif

		PeriodicInvoicePostManager fPostManager;
		public PeriodicInvoicePostManager PostManager
		{
			get { return fPostManager ?? (fPostManager = new PeriodicInvoicePostManager(this)); }
		}

		public void ResetPostManager()
		{
			fPostManager = null;
		}

		public TransactionHeader[] ARTransactionsCreatedForPosting
		{
			get { return PostManager.Poster.PostedInvoices.ToArray<TransactionHeader>(); }
		}

		public new PeriodicInvoiceJobsFilterBusinessObject JobsFilter
		{
			get { return (PeriodicInvoiceJobsFilterBusinessObject)base.JobsFilter; }
		}

		protected override bool ShouldRegisterEditableChildObject
		{
			get { return true; }
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			InvoiceType = InvoiceTypeList[0].Code;
		}

		protected override ZQuery AdditionalChargeFilter
		{
			get
			{
				ZQuery filter = base.AdditionalChargeFilter;
				filter.AddToFilter(DebtorPK.IsValid ? new ZQuery(JobChargeSchema.JR_OH_SellAccount, DebtorPK) : new ZQuery());
				filter.AddToFilter(InvoiceTypeInfo.HasErrors() ? new ZQuery() : new ZQuery(JobChargeSchema.JR_InvoiceType, InvoiceType));
				return filter;
			}
		}

		protected override ZQuery AdditionalMiscTransactionFilter
		{
			get
			{
				ZQuery filter = base.AdditionalMiscTransactionFilter;
				filter.AddToFilter(DebtorPK.IsValid ? new ZQuery(AccTransactionHeaderSchema.AH_OH, DebtorPK) : new ZQuery());

				string invoiceTypeForMiscInvoices = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(InvoiceType);
				if (invoiceTypeForMiscInvoices == InvoiceTypesList.Codes.DisbursementInForeignCurrency)
				{
					invoiceTypeForMiscInvoices = InvoiceTypesList.Codes.DisbursementInvoice;
				}
				else if (invoiceTypeForMiscInvoices == InvoiceTypesList.Codes.ForeignCurrencyInvoice)
				{
					invoiceTypeForMiscInvoices = InvoiceTypesList.Codes.FinalInvoice;
				}
				filter.AddToFilter(InvoiceTypeInfo.HasErrors() ? new ZQuery() :
					new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, invoiceTypeForMiscInvoices));

				return filter;
			}
		}

		protected override void JobTypeList_OnPairChangedCore(ZBoolDescriptionPairChangedEventArgs e)
		{
			base.JobTypeList_OnPairChangedCore(e);
			TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();

			if (!IsValidationSuspended)
			{
				ValidateDebtorPK();
			}
		}

		public override void ValidateTotalAmount()
		{
			if (IncludeInThePeriodicInvoice || !IsPartOfPeriodicInvoiceBulk)
			{
				base.ValidateTotalAmount();
			}
		}

		public bool CreateTransactions()
		{
			Jobs.Reload();  //for bulk posting need to reload job after previous posting.
			PostManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			return !PostManager.CancelPosting;
		}

		public PeriodicInvoice PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck()
		{
			var newFactory = new BusinessObjectFactory() { NameForDebugging = "Periodic Invoice Factory for Authorization Check" };
			if (ValidationHasRun && !HasErrors)
			{
				// Validation has already run, just in a different factory.
				// No point running the same checks all over again in a new factory, so we disable all validation in the new factory.
				newFactory.SuspendValidation();
				newFactory.SetContext(BusinessContext.PeriodicInvoiceHasAlreadyBeenValidatedInAnotherFactory);
			}

			var copiedInvoiceInNewFactory = new PeriodicInvoiceLightForCheckSecurityRights(newFactory);
			copiedInvoiceInNewFactory.InitalizeFromInvoiceInfo(new PeriodicInvoiceBulkPoster.InvoiceInfo(this));
			return copiedInvoiceInNewFactory;
		}

		protected override void Jobs_IncludeInThePeriodicInvoiceChangedCore(object sender, EventArgs e)
		{
			base.Jobs_IncludeInThePeriodicInvoiceChangedCore(sender, e);

			using (IncludeInThePeriodicInvoiceJobUpdateSuspender.GetSuspender())
			{
				IncludeInThePeriodicInvoice = SelectedJobs.Any();
			}
		}

		protected override IEnumerable<ZString> GetJobTypesWhichHaveConfiguration()
		{
			if (Debtor != null && Debtor.CompanyData.InvoiceTypes != null)
			{
				var jobTypes = Debtor.CompanyData.InvoiceTypes.Cast<OrgInvoiceType>().Select(x => x.PI_Module).Distinct().ToList();
				if (jobTypes.Contains(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code))
				{
					jobTypes = Debtor.CompanyData.InvoiceTypes[0].Lookups.JobTypeList.ToArray().Select(x => (ZString)x.Code).ToList();
				}
				return jobTypes;
			}
			else
			{
				return null;
			}
		}

		protected override void LoadJobsCore()
		{
			base.LoadJobsCore();

			TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
			foreach (PeriodicInvoiceSelectableJob job in Jobs)
			{
				job.Parent.LayoutWhenPrintedInPeriodicInvoice = GetLayout(Debtor, job.Parent);
				job.Parent.SecondaryLayoutWhenPrintedInPeriodicInvoice = GetSecondaryLayout(Debtor, job.Parent);
			}
		}

		protected override PeriodicInvoiceBaseJobFilterBusinessObject GetJobFilterBusinessObject()
		{
			return new PeriodicInvoiceJobsFilterBusinessObject();
		}
		#endregion

		#region Lookups

		#region Debtors

		public DebtorCollection Debtors
		{
			get { return FindboxLookupCollections.GetDebtorCollection(Factory); }
		}

		#endregion

		#region Invoice Terms

		public CodeDescriptionPairList InvoiceTerms_List
		{
			get { return FindboxLookupCollections.GetARInvoiceTermsList(Factory); }
		}

		#endregion

		#region Invoice Type List

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (fInvoiceTypeList == null)
				{
					fInvoiceTypeList = new CodeDescriptionPairList();

					var allInvoiceTypes = new InvoiceTypesList();
					allInvoiceTypes.AddRange(new AgencyInvoiceTypesList());

					foreach (CodeDescriptionPair invoiceType in allInvoiceTypes)
					{
						if (Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, invoiceType.Code) > -1)
						{
							fInvoiceTypeList.Add(invoiceType);
						}
					}
				}
				return fInvoiceTypeList;
			}
		}

		CodeDescriptionPairList fInvoiceTypeList;

		#endregion

		#endregion

		#region Properties

		#region ReadOnly

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;
			RefreshBinding();
		}

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#region Sell Reference

		[MaxLength(35)]
		public ZString SellReference
		{
			get { return fSellReference; }
			set
			{
				if (fSellReference != value)
				{
					CheckMaximumLength(SellReferenceInfo, value);
					SetNonPersistentPropertyValue(SellReferenceInfo, ref fSellReference, value);
					SellReferenceInfo.RefreshBinding();
				}
			}
		}

		ZString fSellReference;

		public ZPropertyInfo SellReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(SellReference)); }
		}

		#endregion

		#region IncludeInThePeriodicInvoice

		public ZBool IncludeInThePeriodicInvoice
		{
			get { return fIncludeInThePeriodicInvoice; }
			set
			{
				if (fIncludeInThePeriodicInvoice != value)
				{
					using (Jobs.IncludeInThePeriodicInvoiceChangedSuspender.GetSuspender())
					{
						if (!IncludeInThePeriodicInvoiceJobUpdateSuspender.IsSuspended)
						{
							if (value)
							{
								foreach (PeriodicInvoiceSelectableJob job in Jobs)
								{
									job.SetDefaultForIncludeInThePeriodicInvoice();
								}
							}
							else
							{
								foreach (PeriodicInvoiceSelectableJob job in Jobs)
								{
									job.IncludeInThePeriodicInvoice = false;
								}
								ClearRowNotifications();
							}
						}
						SetNonPersistentPropertyValue(IncludeInThePeriodicInvoiceInfo, ref fIncludeInThePeriodicInvoice, value);
					}

					Jobs.OnParentIncludeInThePeriodicInvoiceChanged(value);
				}
			}
		}
		ZBool fIncludeInThePeriodicInvoice;

		public ZPropertyInfo IncludeInThePeriodicInvoiceInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInThePeriodicInvoice)); }
		}

		protected FunctionalitySuspender IncludeInThePeriodicInvoiceJobUpdateSuspender
		{
			get { return includeInThePeriodicInvoiceJobUpdateSuspender ?? (includeInThePeriodicInvoiceJobUpdateSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender includeInThePeriodicInvoiceJobUpdateSuspender;

		#endregion

		#region Debtor

		[RelatedBusinessObject("Debtor")]
		[List("Debtors")]
		public ZGuid DebtorPK
		{
			get { return DebtorPK_internal; }
			set
			{
				if (DebtorPK != value)
				{
					ClearJobs();
					ClearMiscInvoices();

					SetNonPersistentPropertyValue(DebtorPKInfo, ref DebtorPK_internal, value);
					CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					ResetJobTypeList();
					SetDefaultTaxBranch();

					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();

					if (!IsValidationSuspended)
					{
						ValidateDebtorPK();
					}
				}
			}
		}
		ZGuid DebtorPK_internal;

		public ZPropertyInfo DebtorPKInfo
		{
			get { return GetZPropertyInfo(nameof(DebtorPK)); }
		}

		public OrgHeader Debtor
		{
			get { return Factory.Load<OrgHeader>(DebtorPK); }
		}

		#endregion

		#region Tax Branch

		[List("Branches")]
		public ZGuid TaxBranch
		{
			get
			{
				return fTaxBranch;
			}
			set
			{
				if (fTaxBranch != value)
				{
					SetNonPersistentPropertyValue(TaxBranchInfo, ref fTaxBranch, value);
					if (IsPartOfPeriodicInvoiceBulk)
					{
						AfterTaxBranchChanged();
						if (TaxBranchAndJobsChanged != null)
						{
							TaxBranchAndJobsChanged(null, EventArgs.Empty);
						}
					}
					else
					{
						ClearJobs();
					}

					if (!IsValidationSuspended)
					{
						ValidateTaxBranch();
					}
				}
			}
		}
		ZGuid fTaxBranch;

		public event EventHandler TaxBranchAndJobsChanged;

		public ZPropertyInfo TaxBranchInfo
		{
			get { return GetZPropertyInfo(nameof(TaxBranch)); }
		}

		public GlbBranchCollection Branches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		public bool TaxBranch_ReadOnly => !IsDebtorTaxApplicable || !Env.Security.NewReceivablesOverrideTaxBranchAllows.IsAllowedWithConstraint();

		bool CanAppllyTaxBranch => AccountingMasterFilesUtils.IsTaxBranchApplicable && IsDebtorTaxApplicable;

		bool IsDebtorTaxApplicable => Debtor?.CompanyData?.IsARTaxApplicable ?? false;

		void SetDefaultTaxBranch() => TaxBranch = AccountingMasterFilesUtils.GetTaxBranchResetValue(IsDebtorTaxApplicable);

		void AfterTaxBranchChanged()
		{
			if (InitializedCharges.Any())
			{
				var filteredCharges = TaxBranch.IsEmpty ? InitializedCharges : InitializedCharges.Where(x => x.JR_GB_SellTaxBranch == TaxBranch);

				Charges.Clear();
				Charges.AddRange(filteredCharges);
				ClearTotals();

				Jobs.RemoveAll();

				foreach (var charge in Charges)
				{
					AddJobFromCharge(charge);
				}
			}
		}

		public PeriodicInvoiceSelectableJob AddJobFromCharge(Charge charge)
		{
			var currentJob = charge.InvoicingJob;
			var selectableJob = Jobs.FindJob(currentJob);
			if (selectableJob == null)
			{
				currentJob.LayoutWhenPrintedInPeriodicInvoice = GetLayout(Debtor, currentJob);
				currentJob.SecondaryLayoutWhenPrintedInPeriodicInvoice = GetSecondaryLayout(Debtor, currentJob);
				using (ReloadChargesSuspenderWithoutReloadOnResume.GetSuspender())
				{
					selectableJob = Jobs.Add(currentJob);
				}
			}

			return selectableJob;
		}

		public List<Charge> InitializedCharges
		{
			get { return fInitializedCharges ?? (fInitializedCharges = new List<Charge>()); }
		}
		List<Charge> fInitializedCharges;

		#endregion

		#region Invoice Date

		public override ZDateTime InvoiceDate
		{
			get { return base.InvoiceDate; }
			set
			{
				base.InvoiceDate = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}

		protected override bool InvoiceDate_ReadOnly => base.InvoiceDate_ReadOnly || !Env.Security.NewReceivablesPeriodicInvoiceDate.IsAllowed;

		#endregion

		#region Due Date

		public ZDateTime DueDate
		{
			get { return DueDate_internal; }
			set
			{
				SetNonPersistentPropertyValue(DueDateInfo, ref DueDate_internal, value);
			}
		}
		ZDateTime DueDate_internal;

		public ZPropertyInfo DueDateInfo
		{
			get { return GetZPropertyInfo(nameof(DueDate)); }
		}

		protected bool DueDate_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Invoice Terms

		[MaxLength(3)]
		[List("InvoiceTerms_List")]
		public ZString InvoiceTerm
		{
			get { return InvoiceTerm_internal; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTermInfo, ref InvoiceTerm_internal, value);
				TermsAndDueDateCalculationProvider.CalculateDueDate();
				if (InvoiceTerm == Constants.InvoiceTerms.CashOnDelivery)
				{
					InvoiceTermDays = ZByte.Zero;
				}
			}
		}
		ZString InvoiceTerm_internal;

		public ZPropertyInfo InvoiceTermInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceTerm)); }
		}

		protected bool InvoiceTerm_ReadOnly
		{
			get
			{
				return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
					|| !Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed;
			}
		}

		#endregion

		#region Invoice Term Days

		public ZByte InvoiceTermDays
		{
			get { return InvoiceTermDays_internal; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceTermDaysInfo, ref InvoiceTermDays_internal, value);
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}
		ZByte InvoiceTermDays_internal;

		public ZPropertyInfo InvoiceTermDaysInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceTermDays)); }
		}

		protected bool InvoiceTermDays_ReadOnly
		{
			get
			{
				return InvoiceTerm_ReadOnly || InvoiceTerm == Constants.InvoiceTerms.CashOnDelivery;
			}
		}

		#endregion

		#region Invoice Type

		[MaxLength(3)]
		[List("InvoiceTypeList")]
		public ZString InvoiceType
		{
			get { return InvoiceType_internal; }
			set
			{
				if (InvoiceType != value)
				{
					ClearJobs();
					ClearMiscInvoices();

					SetNonPersistentPropertyValue(InvoiceTypeInfo, ref InvoiceType_internal, value);
					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();

					if (InvoiceTypeCalculationProvider.BillInLocalCurrency(InvoiceType))
					{
						CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					}
					else if (CurrencyNK == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						CurrencyNK = ZString.Empty;
					}
				}
			}
		}
		ZString InvoiceType_internal;

		public ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceType)); }
		}

		#endregion

		public string InvoiceID
		{
			get { return string.Format("{0}, {1}, {2}", Debtor.OH_Code, CurrencyNK, InvoiceType); }
		}

		protected TermsAndDueDateCalculationProvider TermsAndDueDateCalculationProvider
		{
			get
			{
				fTermsAndDueDateCalculationProvider = new ARTermsAndDueDateCalculationProvider(this);
				return fTermsAndDueDateCalculationProvider;
			}
		}
		TermsAndDueDateCalculationProvider fTermsAndDueDateCalculationProvider;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateDebtorPK();
			ValidateInvoiceTerm();
			ValidateInvoiceType();
			ValidateDueDate();
			ValidateBranchDepartmentCombination();
			ValidateTaxBranch();

			foreach (PeriodicInvoiceSelectableJob selectableJob in this.Jobs)
			{
				selectableJob.ClearRowNotifications();
				var job = selectableJob.Parent;

				if (selectableJob.IncludeInThePeriodicInvoice)
				{
					((IBusinessObjectState)job).ClearHasChangesIncludingChildren();
					var charges = Charges.Where(c => c.JR_JH == job.PK);
					PeriodicInvoicePostManagerValidation postValidation = new PeriodicInvoicePostManagerValidation(job, charges.ToArray(), this);
					INotification notification = postValidation.Validate();

					if (notification != null)
					{
						selectableJob.AddRowNotification(notification);
					}
				}
			}
		}

		#region ValidateDebtorPK

		public void ValidateDebtorPK()
		{
			DebtorPKInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(CurrencyNKInfo);
			MandatoryValidation.CheckEntered(DebtorPKInfo);
			ListValidation.ErrorIfInvalidPK(DebtorPKInfo, Debtors);

			if (IsPartOfPeriodicInvoiceBulk)
			{
				if (DebtorDoesNotHaveModuleConfigurationsForAllSelectedJobs)
				{
					DebtorPKInfo.AddError(Res.GetString("b61b24f5-d556-439f-b2e5-0e87075e589f", "This debtor does not have module configurations for all selected jobs. Review the 'Periodic Invoicing' configuration for this debtor"));
				}
			}
			else if (!DebtorHasConfigurationsForAllSelectedLayouts)
			{
				DebtorPKInfo.AddError(Res.GetString("88d536e1-af9a-48ad-95f2-9a92c77bd063", "This debtor does not have configurations for all the selected job types. Review the 'Periodic Invoicing' configuration for this debtor"));
			}
		}

		bool DebtorHasConfigurationsForAllSelectedLayouts
		{
			get
			{
				bool result = true;

				if (Debtor != null && SelectedJobTypeCodes.Count > 0 && Debtor.CompanyData != null)
				{
					var jobTypesWithALL = SelectedJobTypeCodes;
					jobTypesWithALL.Add(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

					OrgInvoiceType[] invoiceTypes = Debtor.CompanyData.InvoiceTypes.Find(new ZQuery(OrgInvoiceTypeSchema.PI_Module, jobTypesWithALL)) as OrgInvoiceType[];
					if (invoiceTypes == null)
					{
						result = false;
					}
					else
					{
						var distinctJobTypes = invoiceTypes.Select(x => x.PI_Module).Distinct();
						result = distinctJobTypes.Contains(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code) || SelectedJobTypeCodes.All(x => distinctJobTypes.Contains(x));
					}
				}

				return result;
			}
		}

		bool DebtorDoesNotHaveModuleConfigurationsForAllSelectedJobs
		{
			get
			{
				bool result = false;

				if (IncludeInThePeriodicInvoice && Debtor != null && Debtor.CompanyData != null)
				{
					var jobGroups = SelectedJobs.Select(x => new { x.JobType, x.TransportMode, x.ServiceDirection, x.ServiceLevel }).Distinct().ToList();

					foreach (var jobGroup in jobGroups)
					{
						var invoiceTypes = Debtor.CompanyData.GetApplicableInvoiceTypes(jobGroup.TransportMode, jobGroup.ServiceDirection, jobGroup.ServiceLevel, true, jobGroup.JobType.Code);
						if (invoiceTypes == null || !invoiceTypes.Keys.Contains(jobGroup.JobType.Code))
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		#endregion

		public void ValidateInvoiceTerm()
		{
			InvoiceTermInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(InvoiceTermInfo, InvoiceTerms_List);

			string error = TermsAndDueDateCalculationProvider.CanInvoiceTermBeSelectedError;
			if (!string.IsNullOrEmpty(error))
			{
				InvoiceTermInfo.AddError(error);
			}
		}

		public void ValidateBranchDepartmentCombination()
		{
			var error = GlbBranchCombinationValidation.CheckBranchDepartmentCombination(GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
			if (!string.IsNullOrEmpty(error))
			{
				AddRowError(error);
			}
		}

		public void ValidateInvoiceType()
		{
			InvoiceTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InvoiceTypeInfo);
			ListValidation.ErrorIfInvalidCode(InvoiceTypeInfo, InvoiceTypeList);

			if (!CurrencyNK.IsEmpty)
			{
				if (!InvoiceTypeCalculationProvider.BillInLocalCurrency(InvoiceType) && InvoiceType != InvoiceTypesList.Codes.SelfBillingInvoice_Batching)
				{
					if (CurrencyNK == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						InvoiceTypeInfo.AddError(Res.GetString("8afae891-68e6-4e8e-977f-735ea6a7cee7", "This Invoice Type can be selected only for foreign currency."));
					}
				}
			}
		}

		public override void ValidateCurrencyNK()
		{
			base.ValidateCurrencyNK();

			ValidateInvoiceType();
		}

		public void ValidateDueDate()
		{
			DueDateInfo.ClearAllNotifications();

			TypeValidation.CheckValidZDateTimeRange(DueDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(DueDateInfo);
		}

		public void ValidateTaxBranch()
		{
			TaxBranchInfo.ClearAllNotifications();

			if (CanAppllyTaxBranch && (!IsPartOfPeriodicInvoiceBulk || IncludeInThePeriodicInvoice))
			{
				MandatoryValidation.CheckEntered(TaxBranchInfo);
				ListValidation.ErrorIfInvalidPK(TaxBranchInfo);
			}
		}

		public override void ValidateBeforeFindingJobs()
		{
			base.ValidateBeforeFindingJobs();
			ValidateDebtorPK();
			ValidateTaxBranch();
		}

		public override void ValidateBeforeFindingMiscInvoices()
		{
			base.ValidateBeforeFindingMiscInvoices();
			ValidateDebtorPK();
		}

		#endregion

		#region IInvoiceTerms Members

		OrgHeader IInvoiceTerms.Header
		{
			get { return Debtor; }
		}

		JobInvoicingConsumerType IInvoiceTerms.JobType
		{
			get { return (SelectedJobTypeCodes != null && SelectedJobTypeCodes.Count == 1) ? JobInvoicingConsumerTypes.New()[SelectedJobTypeCodes[0]] : null; }
		}

		ZString IInvoiceTerms.Direction
		{
			get
			{
				var result = ZString.Empty;
				var directions = Jobs?.Cast<PeriodicInvoiceSelectableJob>().Where(x => x.IncludeInThePeriodicInvoice || IsPartOfPeriodicInvoiceBulk).Select(x => x.ServiceDirection).Distinct();
				if (directions.Any())
				{
					result = (directions.Count() == 1 && directions.First() != OrgConstants.ServiceDirection.Code.Unknown) ? directions.First() : ZString.Empty;
				}
				return result;
			}
		}

		ZString IInvoiceTerms.TransportMode
		{
			get
			{
				var result = ZString.Empty;
				var transportMode = Jobs?.Cast<PeriodicInvoiceSelectableJob>().Where(x => x.IncludeInThePeriodicInvoice || IsPartOfPeriodicInvoiceBulk).Select(x => x.TransportMode).Distinct();
				if (transportMode.Any())
				{
					result = transportMode.Count() == 1 ? transportMode.First() : ZString.Empty;
				}
				return result;
			}
		}

		JobHeader IInvoiceTerms.Job
		{
			get
			{
				var jobs = Jobs?.Cast<PeriodicInvoiceSelectableJob>().Where(x => x.IncludeInThePeriodicInvoice || IsPartOfPeriodicInvoiceBulk).Select(x => x.Parent).Distinct();
				if (jobs.Any() && jobs.Count() == 1)
				{
					return jobs.First();
				}

				return null;
			}
		}

		ZGuid IInvoiceTerms.AH_GE
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IInvoiceTerms.AH_GB
		{
			get { return ZGuid.Empty; }
		}

		ZString IInvoiceTerms.AH_TransactionCategory
		{
			get { return InvoiceType; }
		}

		ZByte IInvoiceTerms.AH_InvoiceTermDays
		{
			get { return InvoiceTermDays; }
			set { InvoiceTermDays = value; }
		}

		ZString IInvoiceTerms.AH_InvoiceTerm
		{
			get { return InvoiceTerm; }
			set { InvoiceTerm = value; }
		}

		ZPropertyInfo IInvoiceTerms.AH_InvoiceDateInfo
		{
			get { return InvoiceDateInfo; }
		}

		ZPropertyInfo IInvoiceTerms.AH_InvoiceTermInfo
		{
			get { return InvoiceTermInfo; }
		}

		ZPropertyInfo IInvoiceTerms.AH_InvoiceTermDaysInfo
		{
			get { return InvoiceTermDaysInfo; }
		}

		ZDateTime IInvoiceTerms.AH_DueDate
		{
			get
			{
				return DueDate;
			}
			set
			{
				DueDate = value;
			}
		}

		ZDateTime IInvoiceTerms.AH_InvoiceDate
		{
			get { return InvoiceDate; }
		}

		ZDateTime IInvoiceTerms.AH_DocumentReceivedDate
		{
			get { return InvoiceDate; }
		}

		ZPropertyInfo IInvoiceTerms.AH_DocumentReceivedDateInfo
		{
			get { return InvoiceDateInfo; }
		}

		#endregion

		#region Implementation

#if DEBUG
		internal
#endif
 bool IsPartOfPeriodicInvoiceBulk;

		public void UpdateDataAfterLinesChanges()
		{
			InitializeFilterForJobs();
			InitializeFilterForMiscInvoices();

			ClearTotals();

			foreach (PeriodicInvoiceSelectableJob job in Jobs)
			{
				Factory.AddFetchHint(typeof(ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers), job.JH_ParentID);
			}

			((IBindingListView)Jobs).ApplySort(((IBindingListView)Jobs).SortDescriptions);
			((IBindingListView)MiscInvoices).ApplySort(((IBindingListView)MiscInvoices).SortDescriptions);
		}

		#endregion
	}
}
