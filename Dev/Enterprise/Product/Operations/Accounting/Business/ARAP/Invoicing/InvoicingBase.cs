using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;
using static Enterprise.Core.Constants;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using Constants = Enterprise.Core.Constants;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	#region Operations Invoice Types Enum

	public enum OperationsInvoiceTypes
	{
		Consol,
		Freight,
		Customs,
		Transport,
		Miscellaneous
	}

	#endregion

	[UniversalDataContext(DataContextType.AccountingInvoice)]
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	public abstract partial class InvoicingBase : TransactionHeaderWithLines,
		IDocumentSupportable,
		IPayablesAndReceivables,
		IReversing,
		IInvoiceTerms,
		ISupportMatchingOfMyLines,
		ISecurityOverrideProviderSource,
		IJobNumber,
		ICDArchive,
		IntercompanyTransactionImportHelper.IIntercompanyBranchDepartmentDeciderSource,
		ICommissionableTransaction,
		IInvoiceRemittance
	{
		public InvoicingBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			invoiceRoundingLineCreator_constructorInitializedOnly = new InvoiceRoundingLineCreator();
			chargeCreator_constructorInitializedOnly = new ChargeCreator();
			InvoiceTaxDateCacheProvider = new InvoiceTaxDateCacheProvider();
		}

		IInvoiceRoundingLineCreator InvoiceRoundingLineCreator => invoiceRoundingLineCreator_constructorInitializedOnly;
		IInvoiceRoundingLineCreator invoiceRoundingLineCreator_constructorInitializedOnly;

		IChargeCreator ChargeCreator => chargeCreator_constructorInitializedOnly;
		IChargeCreator chargeCreator_constructorInitializedOnly;
#if DEBUG
		public void SubstituteInvoiceRoundingLineCreator_ForTestOnly(IInvoiceRoundingLineCreator replacement) => invoiceRoundingLineCreator_constructorInitializedOnly = replacement;
		public IInvoiceRoundingLineCreator InvoiceRoundingLineCreator_ExposedForTestOnly => InvoiceRoundingLineCreator;

		public void SubstituteChargeCreator_ForTestOnly(IChargeCreator replacement) => chargeCreator_constructorInitializedOnly = replacement;
		public IChargeCreator ChargeCreator_ExposedForTestOnly => ChargeCreator;

		public void AddRelatedJobsForReversing_ForTestOnly(Job job)
		{
			if (fRelatedJobsForReversing == null)
			{
				fRelatedJobsForReversing = new List<Job>();
			}

			fRelatedJobsForReversing.Add(job);
		}
#endif

		public bool SupportMultiPeriodApportionment
		{
			get
			{
				var supports = !IsPosted;
				supports = supports && SupportHeaderReference;
				supports = supports && !IsReverseTransaction;
				return supports;
			}
		}

		public bool ExistLineWhichRelatedJobIsReadyForFinancialClosureWithoutPostSecurity => Lines.OfType<InvoicingLineBase>().Any(x => x.IsRelatedJobReadyForFinancialClosureWithoutPostSecurity);

		public override ZString AH_Desc
		{
			get => base.AH_Desc;
			set
			{
				using (Factory.SetTempContext(BusinessContext.MakingChangesNotAffectingTaxRecordParent))
				{
					base.AH_Desc = value;
				}
			}
		}

		protected bool AH_Desc_ReadOnly => ReadOnlyForAssociatedDraftInvoice;

		protected virtual bool AH_InvoiceTerm_ReadOnly
		{
			get { return Ledger == LedgerTypes.AccountsReceivable || !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		protected virtual bool AH_InvoiceTermDays_ReadOnly
		{
			get { return Ledger == LedgerTypes.AccountsReceivable || !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) || AH_InvoiceTerm == Constants.InvoiceTerms.CashOnDelivery; }
		}

		protected override bool AH_ChequeOrReference_ReadOnly
		{
			get => IsARInvoiceOrCreditNoteOrAdjustmentNote || base.AH_ChequeOrReference_ReadOnly;
			set => base.AH_ChequeOrReference_ReadOnly = value;
		}

		#region Tax Branch

		public override ZGuid AH_GB_TaxBranch
		{
			get
			{
				return base.AH_GB_TaxBranch;
			}
			set
			{
				base.AH_GB_TaxBranch = value;

				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (cost.E6_GB_CostTaxBranch != AH_GB_TaxBranch)
					{
						cost.E6_GB_CostTaxBranch = AH_GB_TaxBranch;
					}
				}
			}
		}

		protected override bool AH_GB_TaxBranch_ReadOnly => base.AH_GB_TaxBranch_ReadOnly || !IsMiscServTaxApplicable;

		public override ZBool CanApplyTaxBranch => IsMiscServTaxApplicable && AccountingMasterFilesUtils.IsTaxBranchApplicable;

		#endregion

		#region AH_PostedToEFT

		public bool UseJobExchangeRate
		{
			get { return AH_PostedToEFT; }
			set { AH_PostedToEFT = value; }
		}

		bool UseJobExchangeRateDisregardingCheckBox => this.GetExRateLedger() == ExchangeRateValidLedgerEnum.AR;
		internal ZBool ShouldUseJobExRateConfigWhenJobIsNull => (UseJobExchangeRate || UseJobExchangeRateDisregardingCheckBox) && Job == null;

		protected virtual bool IsUseJobExchangeRateApplicable => Ledger != LedgerTypes.AccountsReceivable;

		public override ZBool AH_PostedToEFT
		{
			get
			{
				return base.AH_PostedToEFT;
			}
			set
			{
				IsProxyingHeaderValues = true;
				try
				{
					bool originalValue = base.AH_PostedToEFT;
					base.AH_PostedToEFT = value;
					if (value != originalValue && !IsReverseTransaction && !SuspendRateChangesInAH_PostedToEFT.IsSuspended)
					{
						if (!value) //set the ex. rate back
						{
							SetExchangeRate(true);
						}
						else
						{
							ApplyLogicForAH_PostedToEFTIsTrue(null);
						}
					}

					SetExchangeRateAdditionalRateReadOnlyCondition();
				}
				finally
				{
					IsProxyingHeaderValues = false;
				}
				RefreshBinding();
				foreach (InvoicingLineBase line in Lines)
				{
					line.AL_ExchangeRateInfo.RefreshBinding();
				}
			}
		}

		FunctionalitySuspender SuspendRateChangesInAH_PostedToEFT => suspendRateChangesInAH_PostedToEFT ?? (suspendRateChangesInAH_PostedToEFT = new FunctionalitySuspender());
		FunctionalitySuspender suspendRateChangesInAH_PostedToEFT;

		void ApplyLogicForAH_PostedToEFTIsTrue(HashSet<ZGuid> changedConsolCostPKs)
		{
			var isOriginalRateSet = false;
			foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
			{
				var originalCost = cost.RelatedConsolCostFromDatabase;
				if (originalCost != null && cost.E6_ExchangeRate != originalCost.E6_ExchangeRate)
				{
					cost.E6_ExchangeRate = cost.E6_RX_NKCurrency == originalCost.E6_RX_NKCurrency ? originalCost.E6_ExchangeRate : cost.GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(TransactionCurrency);
					cost.SplitApportionAmount();
					isOriginalRateSet = true;
				}
				if (!isOriginalRateSet && changedConsolCostPKs != null && changedConsolCostPKs.Contains(cost.PK))
				{
					isOriginalRateSet = true;
				}
			}

			if (isOriginalRateSet)
			{
				ImportAllApportionmentsFromCosting();
				AdjustLocalRoundedValuesForImportedConsolCosts();
			}

			SetTransactionLinesExchangeRate(AH_ExchangeRate);
			UpdateHeaderAmounts();
			RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
		}

		public bool AH_PostedToEFT_ReadOnly
		{
			get
			{
				bool readOnly = AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				if (!readOnly)
				{
					if (this is APInvoice)
					{
						readOnly = !Env.Security.AllowAPInvoiceChangeDefaultUseJobExchangeRate.IsAllowed;
					}
					else if (this is APCreditNote)
					{
						readOnly = !Env.Security.AllowAPCreditNoteChangeDefaultUseJobExchangeRate.IsAllowed;
					}
				}
				return readOnly;
			}
		}

		#endregion

		protected override void SetExchangeRateAdditionalRateReadOnlyCondition()
		{
			ExchangeRate.AdditionalRateReadOnlyCondition = AH_PostedToEFT;
		}

		protected override void SetTransactionLinesExchangeRate(ZDecimal exchangeRate)
		{
			using (Lines.SuspendListChanged())
			{
				foreach (InvoicingLineBase line in Lines)
				{
					line.SetExchangeRate();
				}
			}
		}

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				var additionalReference = ZString.Empty;

				if (IsReceivableOrPayable
					&& IsInvoiceOrCreditNoteOrAdjustmentNote)
				{
					if (!IsReversed)
					{
						if (!IsInDatabase && !AH_FullyPaidDate.IsEmpty)
						{
							additionalReference = AccountingConstants.InvoiceAdditionalReference.PostedAndFullyMatched;
						}
						else if (IsPosting)
						{
							additionalReference = AccountingConstants.InvoiceAdditionalReference.Posted;
						}
						else if (IsInDatabase)
						{
							if (AH_FullyPaidDateInfo.OriginalValue.IsEmpty && !AH_FullyPaidDate.IsEmpty)
							{
								additionalReference = AccountingConstants.InvoiceAdditionalReference.FullyMatched;
							}
							else if (!AH_FullyPaidDateInfo.OriginalValue.IsEmpty && AH_FullyPaidDate.IsEmpty)
							{
								additionalReference = AccountingConstants.InvoiceAdditionalReference.UndoFullyMatched;
							}
						}
					}
					else
					{
						if ((AH_IsCancelledInfo.HasChanges && !(ZBool)AH_IsCancelledInfo.OriginalValue && AH_IsCancelled) || !IsInDatabase)
						{
							additionalReference = AccountingConstants.InvoiceAdditionalReference.Reversed;
						}
					}
				}
				else if (AH_Ledger == LedgerTypes.IncompleteTransactions || AH_Ledger == LedgerTypes.TransactionsPendingAllocation)
				{
					if (Factory.HasContext(BusinessContext.SavingAsIncomplete))
					{
						additionalReference = (NoResString)"Saved as Incomplete";
					}
					else
					{
						var approvalRequest = GetLatestTransactionRelatedApprovalRequest(new BusinessObjectFactory(), true);
						if (approvalRequest != null && approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested)
						{
							additionalReference = (NoResString)"Saved as Pending Approval";
						}
					}
				}

				return GetLogReference(additionalReference);
			}
		}

		string GetLogReference(string additionalReference)
		{
			return string.IsNullOrWhiteSpace(additionalReference) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "{0}{3}{1}{3}{2}", AH_Ledger, AH_TransactionType, additionalReference, AccountingConstants.InvoiceAdditionalReference.Separator);
		}

		public List<InvoicingBase> GetLineLevelTransactionsGroupedByBranchAndDept()
		{
			var factory = new BusinessObjectFactory();
			var lineLevelTransactions = new List<InvoicingBase>();

			foreach (var linesGroupByBranchAndDept in Lines.Cast<InvoicingLineBase>().GroupBy(x => new { BranchPK = x.AL_GB, DeptPK = x.AL_GE }))
			{
				var groupedLinesLocalExTaxAmountTotal = linesGroupByBranchAndDept.Sum(x => x.AL_LocalExTaxAmount);
				if (groupedLinesLocalExTaxAmountTotal * (this is ARAdjustmentNote && !IsCreatingCreditNoteForReversal ? -1 : 1) > 0)
				{
					var subInvoice = (InvoicingBase)factory.New(TypeOfTransaction);
					subInvoice.AH_LocalExTaxAmount = groupedLinesLocalExTaxAmountTotal;
					subInvoice.AH_LocalTaxAmount = linesGroupByBranchAndDept.Sum(x => x.AL_LocalTaxAmount);
					subInvoice.AH_GB = linesGroupByBranchAndDept.Key.BranchPK;
					subInvoice.AH_GE = linesGroupByBranchAndDept.Key.DeptPK;
					subInvoice.IsCreatingCreditNoteForReversal = this.IsCreatingCreditNoteForReversal;

					lineLevelTransactions.Add(subInvoice);
				}
			}

			return lineLevelTransactions;
		}

		#region Schema

		public new abstract partial class Schema : TransactionHeaderWithLines.Schema
		{
			public const string LastRequestedViaWeb = "LastRequestedViaWeb";
			public const string OriginalTransactionReference = "OriginalTransactionReference";
			public const string IsPostedToCASSOrSaved = "IsPostedToCASSOrSaved";
			public const string DisplayInvoiceAddressOverrideForAddressControl = "DisplayInvoiceAddressOverrideForAddressControl";
			public const string ApprovalRequestStatus = "ApprovalRequestStatus";
		}

		#endregion

		#region Type Decider

		public new static readonly TypeDecider TypeDecider = new TransactionHeaderTypeDecider();

		#endregion

		#region Events

		#region Negative Compliance Lines Event

		public event EventHandler OnNegativeCompliancesFailedToCreate;

		void RaiseOnNegativeComplianceSequenceFailedToCreate(object sender)
		{
			OnNegativeCompliancesFailedToCreate?.Invoke(sender, null);
		}

		#endregion

		#region Compliance Sequence Failure Event

		public event EventHandler OnComplianceSequenceFailedToAssign;

		void RaiseOnComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			OnComplianceSequenceFailedToAssign?.Invoke(sender, e);
		}

		#endregion

		#region Digital Signature Signing Failure Event

		public event EventHandler<UserMessageEventArgs> OnDigitalSignatureFailedToSign;

		void RaiseOnDigitalSignatureFailedToSign(object sender, UserMessageEventArgs failureReason)
		{
			OnDigitalSignatureFailedToSign?.Invoke(sender, failureReason);
		}

		#endregion

		#region Mutex Error Event

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event MutexErrorEventHandler MutexError;

		void RaiseMutexError(InvoicingLineBase invoiceLine, MutexErrorEventArgs args)
		{
			if (MutexError != null)
			{
				MutexError(invoiceLine, args);
			}
		}

		protected override void CreateAndLoadLines()
		{
			base.CreateAndLoadLines();
			Lines.MutexError += new MutexErrorEventHandler(Lines_MutexError);
			Lines.ApportionedLineRemoved += new ApportionedLineRemovedEventHander(APInvoice_ApportionedLineRemoved);
		}

		protected override void UnmanageRegisteredEditableChildObjectForDataRefresh()
		{
			base.UnmanageRegisteredEditableChildObjectForDataRefresh();

			if (Lines != null)
			{
				Lines.IsManagedForDataRefresh = false;
			}
		}

		void Lines_MutexError(InvoicingLineBase invoiceLine, MutexErrorEventArgs e)
		{
			RaiseMutexError(invoiceLine, e);
		}

		#endregion

		#region OnJobChanged

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<ChangedBizoEventArgs> OnJobChanged;

		internal void RaiseOnJobChanged(object sender, Job newJob)
		{
			if (OnJobChangedIsSubscribed && newJob != null)
			{
				OnJobChanged(sender, new ChangedBizoEventArgs(newJob));
			}
		}

		internal bool OnJobChangedIsSubscribed
		{
			get { return OnJobChanged != null; }
		}

		#endregion

		#endregion

		#region New Unbound Properties

		#region Related Transactions

		public override ZString RelatedTransactionDebtorsAsString
		{
			get
			{
				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					TransactionHeaderCollection collection = parentCollection as TransactionHeaderCollection;
					if (collection != null)
					{
						return collection.RelatedTransactionDebtors(PK);
					}
				}
				TransactionHeaderCollection col = new TransactionHeaderCollection(Factory);
				return col.RelatedTransactionDebtors(PK);
			}
		}

		public string[] RelatedTransactionDebtorsCodes
		{
			get
			{
				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					TransactionHeaderCollection collection = parentCollection as TransactionHeaderCollection;
					if (collection != null)
					{
						return collection.RelatedTransactionDebtorsCodes(PK);
					}
				}
				TransactionHeaderCollection col = new TransactionHeaderCollection(Factory);
				return col.RelatedTransactionDebtorsCodes(PK);
			}
		}

		public void ResetRelatedTransactionsCollection()
		{
			foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				TransactionHeaderCollection collection = parentCollection as TransactionHeaderCollection;
				if (collection != null)
				{
					collection.ResetRelatedTransactionsCollection();
				}
			}
		}

		#endregion

		public bool SubmittedFromInvoicingForm
		{
			get { return fSubmittedFromInvoicingForm; }
			set
			{
				fSubmittedFromInvoicingForm = value;

				if (fSubmittedFromInvoicingForm && !IsValidationSuspended && ShouldUseDefaultARInvoiceAndPostDate())
				{
					if (InvoicingValidation != null)
					{
						InvoicingValidation.ValidateAH_InvoiceDate();
						InvoicingValidation.ValidateAH_PostDate();
					}
				}
			}
		}
		bool fSubmittedFromInvoicingForm;

		public bool IsPartiallyOrFullyPaid
		{
			get
			{
				return (AH_LocalOutstandingAmount != AH_LocalTotalAmount) && (AH_LocalExTaxAmount != 0);
			}
		}

		public bool IsBelongToMultipleJobs
		{
			get
			{
				if (Lines.Count > 0)
				{
					ZGuid jobPK = Lines[0].AL_JH;
					foreach (InvoicingLineBase line in Lines)
					{
						if (line.AL_JH != jobPK)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsJobRelated
		{
			get
			{
				return AH_JH.IsValid || Lines.Cast<InvoicingLineBase>().Any(x => x.AL_JH.IsValid);
			}
		}

		public bool HasDisbursementCharges(Job parentJob)
		{
			bool result = false;
			foreach (InvoicingLineBase line in Lines)
			{
				var chargeCode = line.ChargeCode;
				if (chargeCode != null)
				{
					var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(chargeCode, parentJob);
					if (chargeTypeOverride != null && chargeTypeOverride.IsDisbursement)
					{
						result = true;
						break;
					}
					chargeTypeOverride = null;
				}
			}
			return result;
		}

		public bool HasCustomsDisbursementCharge
		{
			get
			{
				foreach (InvoicingLineBase line in Lines)
				{
					if (line.ChargeCode != null
						&& line.ChargeCode.AC_ChargeType == Constants.ChargeType.Disbursement
						&& line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool LinesHaveSameJobHeader
		{
			get
			{
				if (Lines.Count > 0)
				{
					ZGuid jobHeaderPK = Lines[0].AL_JH;
					foreach (InvoicingLineBase line in Lines)
					{
						if (jobHeaderPK != line.AL_JH)
						{
							return false;
						}
					}
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool LinesHaveSameDepartment
		{
			get
			{
				if (Lines.Count > 0)
				{
					ZGuid departmentPK = Lines[0].AL_GE;

					foreach (InvoicingLineBase line in Lines)
					{
						if (departmentPK != line.AL_GE)
						{
							return false;
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual ZString HeaderDefaultCurrency
		{
			get
			{
				ZString currencyNK = ZString.Empty;
				if (Header != null)
				{
					if (AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						if (Header.CompanyData.ARDDefltCurrency != null)
						{
							currencyNK = Header.CompanyData.ARDDefltCurrency.RX_Code;
						}
					}
					else
					{
						if (Header.CompanyData.APDefltCurrency != null)
						{
							currencyNK = Header.CompanyData.APDefltCurrency.RX_Code;
						}
					}
				}
				return (currencyNK == ZString.Empty) ? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency : currencyNK;
			}
		}

		JobInvoicingSecurityHelper securityHelper;

		protected JobInvoicingSecurityHelper SecurityHelper
		{
			get
			{
				if (securityHelper == null)
				{
					securityHelper = new JobInvoicingSecurityHelper(InvoicingJob?.PlugInData?.InvoicingSupporter?.JobInvoicingSecurity ?? Env.Security.None);
				}
				return securityHelper;
			}
		}

		public ZString MessageForInvoiceTermOverriding { get; set; }

		#endregion

		#region Properties

		public ZGuid DraftInvoiceHeaderPK { get; set; }

		public ZBool IsContainCashAdvanceCurrencyMismatch { get; set; }

		public ZBool IsCreatingCreditNoteForReversal { get; set; }

		#region ExpectedInvoiceTotal

		protected virtual bool ValidateExpectedInvoiceTotalSecurityIsAllowed
		{
			get
			{
				return true;
			}
		}

		public ZBool ValidateExpectedInvoiceTotal
		{
			get
			{
				return fValidateExpectedInvoiceTotal;
			}
			set
			{
				if (fValidateExpectedInvoiceTotal != value)
				{
					fValidateExpectedInvoiceTotal = value;
					if (!value)
					{
						using (ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
						{
							ExpectedInvoiceTotal = ZDecimal.Zero;
							ExpectedInvoiceTaxTotal = ZDecimal.Zero;
							ExpectedInvoiceExclTaxTotal = ZDecimal.Zero;
						}
					}
					ValidateExpectedInvoiceTotalInfo.RefreshBinding();
				}
				ExpectedInvoiceTotalInfo.RefreshBinding();

				if (InvoicingValidation != null)
				{
					InvoicingValidation.ValidateValidateExpectedInvoiceTotal();
					InvoicingValidation.ValidateExpectedInvoiceTotal();

					if (IsExpectedTaxTotalVisible)
					{
						InvoicingValidation.ValidateExpectedInvoiceTaxTotal();
						InvoicingValidation.ValidateExpectedInvoiceExclTaxTotal();
					}
				}
			}
		}

		protected bool ValidateExpectedInvoiceTotal_ReadOnly => IsInDatabase && !ValidateExpectedInvoiceTotalSecurityIsAllowed || ReadOnlyForAssociatedDraftInvoice;

		public ZPropertyInfo ValidateExpectedInvoiceTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ValidateExpectedInvoiceTotal)); }
		}

		public bool IsSuspendingExpectedInvoiceTotalValidation => ExpectedInvoiceTotalValidationSuspender.IsSuspended;

		public IDisposable SuspendExpectedInvoiceTotalValidation => ExpectedInvoiceTotalValidationSuspender.GetSuspender();

		FunctionalitySuspender ExpectedInvoiceTotalValidationSuspender => expectedInvoiceTotalValidationSuspender ?? (expectedInvoiceTotalValidationSuspender = new FunctionalitySuspender());
		FunctionalitySuspender expectedInvoiceTotalValidationSuspender;

		public void SetExpectedOSAmountFromAccDraftInvoice(AccDraftInvoiceHeader draftInvoice)
		{
			ValidateExpectedInvoiceTotal = true;
			IsValidationOfValidateExpectedInvoiceTotalEnabled = true;
			IsSetFromDraftInvoice = true;

			using (ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
			{
				ExpectedInvoiceTaxTotal = draftInvoice.AIH_ExpectedOSTaxAmount;
				ExpectedInvoiceExclTaxTotal = draftInvoice.AIH_ExpectedOSExTaxAmount;
				ExpectedInvoiceTotal = draftInvoice.AIH_ExpectedOSTotalAmount;
			}
		}

		public void EnableValidationOfValidateExpectedInvoiceTotal()
		{
			if (!IsSetFromDraftInvoice)
			{
				ValidateExpectedInvoiceTotal = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
				IsValidationOfValidateExpectedInvoiceTotalEnabled = true;
			}
		}

		public void UpdateExpectedAmountFromOSAmount()
		{
			if (ValidateExpectedInvoiceTotal && !IsAllocatingInvoice && AH_Ledger != LedgerTypes.IncompleteTransactions && !IsSetFromDraftInvoice)
			{
				ExpectedInvoiceTotal = AH_OSExTaxAmount + AH_OSTaxAmount;
				if (IsExpectedTaxTotalVisible)
				{
					ExpectedInvoiceTaxTotal = AH_OSTaxAmount;
					ExpectedInvoiceExclTaxTotal = AH_OSExTaxAmount;
					InvoicingValidation?.ValidateExpectedInvoiceTaxTotal();
					InvoicingValidation?.ValidateExpectedInvoiceExclTaxTotal();
				}
				InvoicingValidation?.ValidateExpectedInvoiceTotal();
			}
		}

		public FunctionalitySuspender ExpectedInvoiceTotalAdjustmentSuspender => expectedInvoiceTotalAdjustmentSuspender ?? (expectedInvoiceTotalAdjustmentSuspender = new FunctionalitySuspender());
		FunctionalitySuspender expectedInvoiceTotalAdjustmentSuspender;

		internal bool IsValidationOfValidateExpectedInvoiceTotalEnabled { get; private set; }

		bool IsSetFromDraftInvoice { get; set; }

		internal bool IsPrintingProformaInvoice { get; set; }

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal ExpectedInvoiceTotal
		{
			get
			{
				return fExpectedInvoiceTotal;
			}
			set
			{
				if (fExpectedInvoiceTotal != value)
				{
					fExpectedInvoiceTotal = value;
					ExpectedInvoiceTotalInfo.RefreshBinding();

					if (ValidateExpectedInvoiceTotal && IsExpectedTaxTotalVisible && !ExpectedInvoiceTotalAdjustmentSuspender.IsSuspended)
					{
						using (ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
						{
							if (!ExpectedInvoiceTaxTotal.IsEmpty && ExpectedInvoiceExclTaxTotal.IsEmpty)
							{
								ExpectedInvoiceExclTaxTotal = ExpectedInvoiceTotal - ExpectedInvoiceTaxTotal;
							}
							else if (ExpectedInvoiceTaxTotal.IsEmpty && !ExpectedInvoiceExclTaxTotal.IsEmpty)
							{
								ExpectedInvoiceTaxTotal = ExpectedInvoiceTotal - ExpectedInvoiceExclTaxTotal;
							}
						}

						if (InvoicingValidation != null)
						{
							InvoicingValidation.ValidateExpectedInvoiceTaxTotal();
							InvoicingValidation.ValidateExpectedInvoiceExclTaxTotal();
						}
					}
				}
			}
		}

		public ZPropertyInfo ExpectedInvoiceTotalInfo => GetZPropertyInfo(nameof(ExpectedInvoiceTotal));

		protected bool ExpectedInvoiceTotal_ReadOnly => !fValidateExpectedInvoiceTotal || ReadOnlyForAssociatedDraftInvoice;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal UnallocatedInvoiceTotal => ExpectedInvoiceTotal - AH_OSTotalAmount;

		public ZPropertyInfo UnallocatedInvoiceTotalInfo => GetZPropertyInfo(nameof(UnallocatedInvoiceTotal));

		ZDecimal fExpectedInvoiceTotal;
		ZBool fValidateExpectedInvoiceTotal;
		ZDecimal expectedInvoiceTaxTotal;
		ZDecimal expectedInvoiceExclTaxTotal;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal ExpectedInvoiceTaxTotal
		{
			get
			{
				return expectedInvoiceTaxTotal;
			}
			set
			{
				if (expectedInvoiceTaxTotal != value)
				{
					expectedInvoiceTaxTotal = value;
					ExpectedInvoiceTaxTotalInfo.RefreshBinding();

					if (ValidateExpectedInvoiceTotal && IsExpectedTaxTotalVisible && !ExpectedInvoiceTotalAdjustmentSuspender.IsSuspended)
					{
						using (ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
						{
							if (!ExpectedInvoiceTotal.IsEmpty && ExpectedInvoiceExclTaxTotal.IsEmpty)
							{
								ExpectedInvoiceExclTaxTotal = ExpectedInvoiceTotal - ExpectedInvoiceTaxTotal;
							}
							else if (ExpectedInvoiceTotal.IsEmpty && !ExpectedInvoiceExclTaxTotal.IsEmpty)
							{
								ExpectedInvoiceTotal = ExpectedInvoiceExclTaxTotal + ExpectedInvoiceTaxTotal;
							}
						}

						if (InvoicingValidation != null)
						{
							InvoicingValidation.ValidateExpectedInvoiceTotal();
							InvoicingValidation.ValidateExpectedInvoiceExclTaxTotal();
						}
					}
				}
			}
		}

		public ZPropertyInfo ExpectedInvoiceTaxTotalInfo => GetZPropertyInfo(nameof(ExpectedInvoiceTaxTotal));

		protected bool ExpectedInvoiceTaxTotal_ReadOnly => !fValidateExpectedInvoiceTotal || ReadOnlyForAssociatedDraftInvoice;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal UnallocatedInvoiceTaxTotal => ExpectedInvoiceTaxTotal - AH_OSTaxAmount;

		public ZPropertyInfo UnallocatedInvoiceTaxTotalInfo => GetZPropertyInfo(nameof(UnallocatedInvoiceTaxTotal));

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal ExpectedInvoiceExclTaxTotal
		{
			get
			{
				return expectedInvoiceExclTaxTotal;
			}
			set
			{
				if (expectedInvoiceExclTaxTotal != value)
				{
					expectedInvoiceExclTaxTotal = value;
					ExpectedInvoiceExclTaxTotalInfo.RefreshBinding();

					if (ValidateExpectedInvoiceTotal && IsExpectedTaxTotalVisible && !ExpectedInvoiceTotalAdjustmentSuspender.IsSuspended)
					{
						using (ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
						{
							if (!ExpectedInvoiceTotal.IsEmpty && ExpectedInvoiceTaxTotal.IsEmpty)
							{
								ExpectedInvoiceTaxTotal = ExpectedInvoiceTotal - ExpectedInvoiceExclTaxTotal;
							}
							else if (ExpectedInvoiceTotal.IsEmpty && !ExpectedInvoiceTaxTotal.IsEmpty)
							{
								ExpectedInvoiceTotal = ExpectedInvoiceExclTaxTotal + ExpectedInvoiceTaxTotal;
							}
						}

						if (InvoicingValidation != null)
						{
							InvoicingValidation.ValidateExpectedInvoiceTotal();
							InvoicingValidation.ValidateExpectedInvoiceTaxTotal();
						}
					}
				}
			}
		}

		public ZPropertyInfo ExpectedInvoiceExclTaxTotalInfo => GetZPropertyInfo(nameof(ExpectedInvoiceExclTaxTotal));

		protected bool ExpectedInvoiceExclTaxTotal_ReadOnly => !fValidateExpectedInvoiceTotal || ReadOnlyForAssociatedDraftInvoice;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal UnallocatedInvoiceExclTaxTotal => ExpectedInvoiceExclTaxTotal - AH_OSExTaxAmount;

		public ZPropertyInfo UnallocatedInvoiceExclTaxTotalInfo => GetZPropertyInfo(nameof(UnallocatedInvoiceExclTaxTotal));

		public bool IsExpectedTaxTotalVisible => (this is APInvoice || this is APCreditNote) && Header != null && Header.CompanyData.IsAPTaxApplicable && Company.GC_IsGSTRegistered;

		#endregion

		#region OriginalTransactionReference

		[List(nameof(OriginalTransactionList))]
		public virtual ZGuid OriginalTransactionReference
		{
			get
			{
				if (IsAmendingTransaction_StrongReference)
				{
					return ((IAmending)this).OriginalTransaction.PK;
				}

				return ZGuid.Empty;
			}
			set
			{
				// setter is only available on Credit Notes and Invoices
				// for ARInvoice value is set via AH_TransactionBelongsToGroup during amendment generation
			}
		}

		ITransaction GetOriginalTransactionForAmending() => IsAmendingTransaction_StrongReference ? ((IAmending)this).OriginalTransaction : null;

		public InvoicingBase OriginalReferenceTransaction => Factory.Load(TypeOfReverseTransaction, OriginalTransactionReference) as InvoicingBase;

		public ZPropertyInfo OriginalTransactionReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.OriginalTransactionReference); }
		}

		protected bool OriginalTransactionReference_ReadOnly
		{
			get { return OriginalTransactionReference_ReadOnlyCore(); }
		}

		protected virtual bool OriginalTransactionReference_ReadOnlyCore()
		{
			return IsAmendingTransaction_StrongReference || AreOriginalReferenceFieldsReadOnly;
		}

		protected internal bool IsAmendingTransaction_StrongReference
		{
			get
			{
				var amending = this as IAmending;
				return amending != null && amending.IsAmendingTransaction && amending.OriginalTransaction != null;
			}
		}

		public bool IsAmendingTransaction_SoftReference => !AH_OriginalTransactionNum.IsEmpty || !AH_OriginalInvoiceDate.IsEmpty;

#if DEBUG
		// this should be removed once AP CRD implements the GenerateAmendingTransaction in another WI
		public virtual bool HasImplementedGenerateAmendingTransaction => true;
#endif
		protected virtual void SetHeaderDetail(InvoicingBase transaction)
		{
			AH_OH = transaction.AH_OH;
			AH_JH = transaction.AH_JH;
			AH_OA_InvoiceAddressOverride = transaction.AH_OA_InvoiceAddressOverride;
			AH_OC_InvoiceContactOverride = transaction.AH_OC_InvoiceContactOverride;
			ExchangeRate.Currency = transaction.AH_RX_NKTransactionCurrency;
			if (CanCopyHeaderExRateForAmending)
			{
				AH_ExchangeRate = transaction.AH_ExchangeRate;
			}
			IsDisbursementOrFinal = transaction.IsDisbursementOrFinal;
			AH_AgreedPaymentMethodOverride = transaction.AH_AgreedPaymentMethodOverride;
			AH_GB_TaxBranch = transaction.AH_GB_TaxBranch;
			AH_PlaceOfSupply = transaction.AH_PlaceOfSupply;
		}

		bool CanCopyHeaderExRateForAmending => IsCopyExRateForAmendingAllowedByRegistry() && (IsAmendingTransaction || CanCopyExRateForAmendingCore);

		bool CanCopyLineExRateForAmending => IsCopyExRateForAmendingAllowedByRegistry() && CanCopyExRateForAmendingCore;

		protected virtual bool CanCopyExRateForAmendingCore
			=> IsLocalCurrencyTransaction || !ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC);

		internal bool IsCopyExRateForAmendingAllowedByRegistry()
		{
			var result = true;
			if (AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				result = AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransaction.GetFallBackValueAtAllLevels(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			}
			else if (AH_Ledger == LedgerTypes.AccountsPayable)
			{
				result = AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.GetFallBackValueAtAllLevels(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			}
			return result;
		}

		internal void PopulateFromOriginalTransaction(InvoicingBase originalTransaction, bool populateAmount = true)
		{
			if (originalTransaction == null)
			{
				return;
			}

			SetHeaderDetail(originalTransaction);

			if (Factory.HasContext(BusinessContext.InterCompanyInvoiceImport) || Factory.HasContext(BusinessContext.IntercompanyInvoiceAutoImport))
			{
				return;
			}

			using (GetReportingDeletedApportionmentChargesSuspender())
			{
				Lines.RemoveAndDeleteAll();
			}

			using (Lines.SuspendListChanged())
			{
				foreach (InvoicingLineBase fromLine in originalTransaction.Lines)
				{
					if (!fromLine.IsStampDutyChargeLine() && !InvoiceRoundingLineCreator.IsRoundingLine(fromLine))
					{
						InvoicingLineBase toLine = (InvoicingLineBase)Lines.AddNew();
						CopyTransactionLine(fromLine, toLine, populateAmount);
					}
				}
			}
		}

		#endregion

		#region Invoice Number

		[MaxLength(AccTransactionHeader.Schema.AH_ConsolidatedInvoiceRefMaxLength)]
		public ZString InvoiceNumber
		{
			get
			{
				if (IsUsingJobNumberBasedInvoiceNumber)
				{
					return AH_ConsolidatedInvoiceRef;
				}
				else
				{
					return AH_TransactionNum;
				}
			}
		}

		public ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceNumber)); }
		}

		public ZString InvoiceNumberPrefixed
		{
			get
			{
				if (IsUsingJobNumberBasedInvoiceNumber)
				{
					return InvoiceNumber;
				}
				else
				{
					return TransactionNumberPrefixed;
				}
			}
		}

		bool IsUsingJobNumberBasedInvoiceNumber
		{
			get
			{
				return AH_Ledger == LedgerTypes.AccountsReceivable
					&& !AH_ConsolidatedInvoiceRef.IsEmpty
					&& AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.GetFallBackValueAtAllLevels(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}
		}

		#endregion

		public bool IsLevel1AuthorisationRequired => Level1AuthorisationRequired(AuthorisationRequired);
		public bool IsLevel2AuthorisationRequired => Level2AuthorisationRequired(AuthorisationRequired);
		public bool IsLevel3AuthorisationRequired => Level3AuthorisationRequired(AuthorisationRequired);
		public bool IsLevel4AuthorisationRequired => Level4AuthorisationRequired(AuthorisationRequired);
		public bool IsLevel5AuthorisationRequired => Level5AuthorisationRequired(AuthorisationRequired);
		public bool IsLevel6AuthorisationRequired => Level6AuthorisationRequired(AuthorisationRequired);

		public int RequiredAuthorisationLevel => IsLevel1AuthorisationRequired ? 1 :
													IsLevel2AuthorisationRequired ? 2 :
													IsLevel3AuthorisationRequired ? 3 :
													IsLevel4AuthorisationRequired ? 4 :
													IsLevel5AuthorisationRequired ? 5 :
													IsLevel6AuthorisationRequired ? 6 :
													0;

		public bool CheckCurrentLoginUserHasSecurityRightsForAllTransactionsForAuthorisationCalculation()
		{
			bool result = true;
			var loginController = new UserLoginController();
			foreach (var transaction in TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation)
			{
				var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, transaction.AH_GB.ToGuid(), transaction.AH_GE.ToGuid());
				if (transaction.Level1AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed
					|| transaction.Level2AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed
					|| transaction.Level3AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalThirdLevelApproval.IsAllowed
					|| transaction.Level4AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalFourthLevelApproval.IsAllowed
					|| transaction.Level5AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalFifthLevelApproval.IsAllowed
					|| transaction.Level6AuthorisationRequired(transaction.AuthorisationRequired) && !userSecurity.CreditAdjustmentNotePostingApprovalSixthLevelApproval.IsAllowed
					)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public bool IsBadDebtWritingOff => this is IBadDebtWritingOff badDebt && badDebt.IsWritingOff;

		readonly IComplianceDocumentNumberProvider complianceDocumentNumberProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceDocumentNumberProvider>)?.Get();

		public override ZString AH_TransactionReference
		{
			set { base.AH_TransactionReference = value; }
			get
			{
				if (complianceDocumentNumberProvider != null && base.AH_TransactionReference.IsEmpty)
				{
					var complianceDocumentNumbers = complianceDocumentNumberProvider.GetComplianceDocumentNumbers(Lines);
					return string.Join(",", complianceDocumentNumbers);
				}
				else
				{
					return base.AH_TransactionReference;
				}
			}
		}

		public override ZString AH_TransactionNum
		{
			get
			{
				return base.AH_TransactionNum;
			}
			set
			{
				if (AH_TransactionNum != value)
				{
					ResetSameNumberTransactionDetails();
				}
				base.AH_TransactionNum = value;

				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (value != cost.E6_InvoiceNum)
					{
						cost.E6_InvoiceNum = value;
					}
				}
			}
		}

		public void AddTransactionNumLog()
		{
			if (this.HasContext(BusinessContext.OverrideInvoiceReference) &&
				AH_Ledger == LedgerTypes.AccountsPayable &&
				(ZString)AH_TransactionNumInfo.OriginalValue != AH_TransactionNum)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Transaction Number changed from '{0}' to '{1}'",
					IsInDatabase ?
					AH_TransactionNumInfo.OriginalValue : ZString.Empty,
					AH_TransactionNum));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get
			{
				return base.AH_TransactionNum_ReadOnly || AH_Ledger == LedgerTypes.AccountsReceivable ||
						((AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || AH_Ledger == LedgerTypes.IncompleteTransactions) &&
						IsSelfBillingInvoice) || ReadOnlyForAssociatedDraftInvoice;
			}
		}

		public override ZString AH_ChequeOrReference
		{
			get
			{
				return base.AH_ChequeOrReference;
			}
			set
			{
				base.AH_ChequeOrReference = value;
				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (value != cost.E6_CostReference)
					{
						cost.E6_CostReference = value;
					}
				}
			}
		}

		protected bool AH_ConsolidatedInvoiceRef_ReadOnly
		{
			get { return AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.IncompleteTransactions || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions; }
		}

		#region Include in the Batch

		public event EventHandler<InvoiceBatchEventArgs> UpdateInvoiceBatchTotal;

		ZBool fIncludeInTheBatch;
		public ZBool IncludeInTheBatch
		{
			get { return fIncludeInTheBatch; }
			set
			{
				if (fIncludeInTheBatch != value)
				{
					SetNonPersistentPropertyValue(IncludeInTheBatchInfo, ref fIncludeInTheBatch, value);
					UpdateInvoiceBatchTotal?.Invoke(null, new InvoiceBatchEventArgs(this));
				}
			}
		}

		public ZPropertyInfo IncludeInTheBatchInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInTheBatch)); }
		}

		#endregion

		#region Consol

		public bool IsConsolInvoice
		{
			get { return !AH_ConsolidatedInvoiceRef.IsEmpty && AH_JH.IsEmpty; }
		}

		public IJobCostingPlugIn Consol
		{
			get
			{
				IJobCostingPlugIn result = null;
				var consoleNumber = ConsolNumberFromConsolidatedInvoiceRef;
				if (!consoleNumber.IsEmpty)
				{
					var consolId = Lines.Cast<InvoicingLineBase>().Select(x => x.GetConsolID()).FirstOrDefault(x => x != null);
					if (consolId != null)
					{
						result = GenericConsol.GenericConsol.GetIJobCostingPlugInByPK(Factory, consolId.Item1, consolId.Item2);
					}
					if (result == null || result.JK_UniqueConsignRef != consoleNumber)
					{
						result = GenericConsol.GenericConsol.GetIJobCostingPlugInByPrimaryCode(Factory, consoleNumber);
					}
				}
				return result;
			}
		}

		public ZString ConsolNumberFromConsolidatedInvoiceRef
		{
			get
			{
				return AH_Ledger == LedgerTypes.AccountsReceivable
						|| this.HasContext(BusinessContext.InterCompanyInvoiceImportedFromGatewayConsol)
						? GetConsolNumberFromConsolidatedInvoiceRef(AH_ConsolidatedInvoiceRef) : ZString.Empty;
			}
		}

		public static ZString GetConsolNumberFromConsolidatedInvoiceRef(ZString consolidatedInvoiceRef)
		{
			return consolidatedInvoiceRef.Split('/')[0];
		}

		public IJobInvoicingPlugIn SingleConsolConsumer
		{
			get
			{
				IJobInvoicingPlugIn consumer = null;
				if (AH_Ledger == LedgerTypes.AccountsPayable && OriginalTransaction != null)
				{
					InvoicingBase invoice = OriginalTransaction as InvoicingBase;
					if (invoice != null)
					{
						APInvoiceConsolCostCollection consolCosts = new APInvoiceConsolCostCollection(Factory, invoice);
						consolCosts.Load();
						var consolPKs = (from JobConsolCost consolCost in consolCosts
										 where consolCost.E6_ParentTableCode == JobConsolSchema.Constants.Prefix
										 select consolCost.E6_ParentID).Distinct();

						if (consolPKs.Count() == 1)
						{
							var genericJob = Factory.LoadGenericJob<GenericJob.GenericJob>(consolPKs.First(), JobConsolSchema.Constants.Prefix);

							if (genericJob != null)
							{
								consumer = genericJob.Consumer;
							}
						}
					}
				}
				else if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ZQuery query = new ZQuery(ViewGenericJobSchema.VJ_JobNumber, ConsolNumberFromConsolidatedInvoiceRef);
					query.AddToFilter(ViewGenericJobSchema.VJ_TableName, JobConsolSchema.Constants.TableName);
					GenericJob.GenericJob[] genericJobs = Factory.Load<GenericJob.GenericJob>(query);

					if (genericJobs.Length == 1)
					{
						consumer = genericJobs[0].Consumer;
					}
				}
				return consumer;
			}
		}

		#endregion

		public ZGuid SingleJobInvoicePK
		{
			get
			{
				//returns single job pk, if AH_JH set or at least 1 AL_JH exists (not empty) and all AL_JH are the same
				//otherwise empty

				ZGuid result = ZGuid.Empty;

				if (AH_JH.IsValid)
				{
					result = AH_JH;
				}
				else
				{
					var lines = Lines.ToArray<InvoicingLineBase>();
					var aL_JHFilter = (from InvoicingLineBase line in lines
									   where line.AL_JH.IsValid
									   select line.AL_JH).Distinct();
					var aL_JHs = aL_JHFilter.Take(2);
					if (aL_JHs.Count() == 1)
					{
						result = aL_JHs.FirstOrDefault();
					}
				}
				return result;
			}
		}

		#region GSTInclusiveAmounts

		public ZBool GSTInclusiveAmounts
		{
			get
			{
				return fGSTInclusiveAmounts;
			}
			set
			{
				if (fGSTInclusiveAmounts != value)
				{
					fGSTInclusiveAmounts = value;
					GSTInclusiveAmountsInfo.RefreshBinding();

					foreach (InvoicingLineBase line in Lines)
					{
						line.GSTInclusiveAmountInfo.RefreshBinding();
					}
				}
			}
		}

		public ZPropertyInfo GSTInclusiveAmountsInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(GSTInclusiveAmounts));
			}
		}

		ZBool fGSTInclusiveAmounts;

		internal ZBool GSTInclusiveAmountNeedUpdate
		{
			get { return fGSTInclusiveAmountNeedUpdate; }
			set { fGSTInclusiveAmountNeedUpdate = value; }
		}

		ZBool fGSTInclusiveAmountNeedUpdate;

		#endregion

		#region IsDisbursementOrFinal

		public ZBool IsDisbursementOrFinal
		{
			get
			{
				return AH_IsDisbursementCalc;
			}
			set
			{
				ZString adaptedValue = GetTransactionCategoryForMiscInvoice(value, IsLocalCurrencyTransaction);
				if (AH_TransactionCategory != adaptedValue)
				{
					AH_TransactionCategory = adaptedValue;
					IsDisbursementOrFinalInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsDisbursementOrFinalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(IsDisbursementOrFinal));
			}
		}

		ZString GetTransactionCategoryForMiscInvoice(bool isDisbursementInvoice, bool isLocalCurrency)
		{
			if (isDisbursementInvoice)
			{
				return isLocalCurrency ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			}
			else
			{
				return isLocalCurrency ? InvoiceTypesList.Codes.FinalInvoice : InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			}
		}

		#endregion

		#region SelfBillingInvoice

		public override ZBool IsSelfBillingInvoice
		{
			get
			{
				return base.IsSelfBillingInvoice;
			}
			set
			{
				var oldValue = IsSelfBillingInvoice;

				if ((IsSelfBillingInvoice != value || AH_TransactionCategory.IsEmpty) &&
					(AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || AH_Ledger == LedgerTypes.IncompleteTransactions))
				{
					AH_TransactionCategory = value ? Constants.TransactionCategory.Codes.SelfBilling : Constants.TransactionCategory.Codes.Standard;

					if (value && !IsInDatabase)
					{
						AH_TransactionNum = ZString.Empty;
					}

					if (!IsValidationSuspended && InvoicingValidation != null)
					{
						InvoicingValidation.ValidateIsSelfBillingInvoice();
					}
				}

				if (oldValue != value && !value)
				{
					foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
					{
						cost.E6_InvoiceNum = AH_TransactionNum;
						cost.E6_InvoiceDate = AH_InvoiceDate;
						cost.E6_DocumentReceivedDate = AH_DocumentReceivedDate;
						cost.E6_PaymentDate = AH_DueDate;
					}
				}

				IsSelfBillingInvoiceInfo.RefreshBinding();
			}
		}

		protected bool IsSelfBillingInvoice_ReadOnly => ReadOnlyForAssociatedDraftInvoice;

		#endregion

		public bool IsAH_PostDate_ReadOnly
		{
			get
			{
				return AH_PostDate_ReadOnly;
			}
		}

		[ReadOnly(true)]
		public ZBool IsPostedToCASSOrSaved
		{
			get { return IsInDatabase || fIsPostedToCASS; }
			set
			{
				SetNonPersistentPropertyValue(IsPostedToCASSOrSavedInfo, ref fIsPostedToCASS, value);
				IsPostedToCASSOrSavedInfo.RefreshBinding();
			}
		}
		ZBool fIsPostedToCASS;

		public ZPropertyInfo IsPostedToCASSOrSavedInfo
		{
			get { return GetZPropertyInfo(Schema.IsPostedToCASSOrSaved); }
		}

		#region Approval Request

		public bool HasApprovalRequestAndNotPosted
		{
			get { return IsIncompleteInvoice && HasApprovalRequest; } //checking invoice state is more stable then relaying on correctly updated 'Posted' status of approval request
		}

		public virtual bool HasApprovalRequest
		{
			get { return false; }
		}

		internal bool IsNewUnapprovedOrWithRequest
		{
			get { return this.HasContext(APInvoiceChargesApprovalRequest.Context.Editing) || HasApprovalRequestAndNotPosted; }
		}

		#region Transaction Related Approval Request

		public GenApprovalRequest TransactionRelatedApprovalRequest
		{
			get { return GetLatestTransactionRelatedApprovalRequest(Factory); }
		}

		public APInvoiceChargesApprovalRequest APInvoiceTransactionRelatedApprovalRequest
		{
			get { return (APInvoiceChargesApprovalRequest)TransactionRelatedApprovalRequest; }
		}

		protected virtual GenApprovalRequest GetLatestTransactionRelatedApprovalRequest(BusinessObjectFactory factory, bool alwaysCheckInDb = false)
		{
			var query = new ZQuery(new APInvoiceChargesApprovalRequestCollection(factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, PK)).CompleteFilter);
			query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";
			query.FetchOnlyFromLocalCache = !IsInDatabase && !alwaysCheckInDb;

			return factory.LoadTop1<APInvoiceChargesApprovalRequest>(query);
		}

		[ResourceStringData("InvoicingBase|ApprovalRequestStatus", Caption = "Approval Request Status", MediumCaption = "Request Status")]
		[List("Lookups.ApprovalStatusList")]
		public ZString ApprovalRequestStatus
		{
			get
			{
				var request = TransactionRelatedApprovalRequest;
				return request != null ? request.XP_ApprovalStatus : ZString.Empty;
			}
		}

		public ZPropertyInfo ApprovalRequestStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ApprovalRequestStatus); }
		}

		internal ZGuid ApprovingUserPK
		{
			get => ApprovingUserPKList.FirstOrDefault();
			set => ApprovingUserPKList = new List<ZGuid> { value };
		}
		public List<ZGuid> ApprovingUserPKList = new List<ZGuid>();
		public ZDateTime ApprovalDate { get; set; }
		internal APInvoiceChargesApprovalRequest ApprovingRequestForJobPosting { get; set; }

#if DEBUG

		public ZGuid ApprovingUserPKForTest => ApprovingUserPK;

#endif

		protected virtual void AddInvoiceApprovalLog()
		{
			if (AH_Ledger == LedgerTypes.AccountsPayable)
			{
				var approvalRequest = ApprovingRequestForJobPosting; //this request is saved after this invoice factory
				if (approvalRequest == null)
				{
					approvalRequest = APInvoiceTransactionRelatedApprovalRequest; //this request is saved before this invoice factory
					if (approvalRequest != null && approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
					{
						approvalRequest.Reload();
					}
				}

				if (approvalRequest != null)
				{
					if (!approvalRequest.XP_GS_NKApprovingUser1.IsEmpty && approvalRequest.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted)
					{
						AddInvoiceApprovalLog(approvalRequest.XP_GS_NKApprovingUser1, approvalRequest.XP_ApprovalDate);
					}
				}
				else if (!ApprovingUserPK.IsEmpty)
				{
					var approvingUser = Factory.Load<GlbStaff>(ApprovingUserPK);
					if (approvingUser != null)
					{
						AddInvoiceApprovalLog(approvingUser.GS_Code, ZDateTime.Now);
					}
				}
			}
		}

		void AddInvoiceApprovalLog(ZString staffCode, ZDateTime eventTime)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, InvoiceApprovedLogReference);
			if (!Logs.Find(query).Any())
			{
				var log = Logs.AddNew(Events.TransactionApprovalActioned, InvoiceApprovedLogReference, eventTime.ToOffset());
				log.SL_GS_NKUser = staffCode;
			}
		}

		protected virtual ZString InvoiceApprovedLogReference
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ApprovalRequestForAP
		public GenApprovalRequest ApprovalRequestForAP
		{
			get { return GetApprovalRequestForAP(); }
		}

		GenApprovalRequest GetApprovalRequestForAP()
		{
			GenApprovalRequest result = null;
			if (this is APInvoice || this is APCreditNote)
			{
				// 1. we can get the request immediately if it's invoice related
				result = TransactionRelatedApprovalRequest;
				if (result == null)
				{
					if (!AH_JH.IsEmpty)  // note: consol invoice with multiple jobs have AH_JH null.
					{
						// 2. try to get request if it's job related
						result = FindMatchingRequestBasedOnParentID(AH_JH);
					}

					// 3. try to get request if it's consol related
					if (result == null)
					{
						if (IsInDatabase)
						{
							var collection = new DynamicBusinessObjectCollection(Factory);
							ZSqlParameterCollection parameters = new ZSqlParameterCollection();
							string sQL = @"SELECT Distinct E6_ParentID FROM dbo.JobConsolCost
WHERE E6_AH_APInvoice = @invoicePK
AND E6_ParentTableCode = 'JK'";
							parameters.Add("@invoicePK", this.PK, AccTransactionHeaderSchema.PK);
							collection.Load(sQL, parameters);

							if (collection.Count == 1)
							{
								result = FindMatchingRequestBasedOnParentID((ZGuid)collection[0][JobConsolCostSchema.Constants.E6_ParentID]);
							}
						}
						else
						{
							var consolCostFilter = new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, this.PK);
							consolCostFilter.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
							consolCostFilter.FetchOnlyFromLocalCache = true;
							var consolCosts = Factory.Load<JobConsolCost>(consolCostFilter);
							if (consolCosts != null)
							{
								var consolPKs = consolCosts.Select(x => x.E6_ParentID).Distinct();
								if (consolPKs.Count() == 1)
								{
									result = FindMatchingRequestBasedOnParentID(consolPKs.First());
								}
							}
						}
					}
				}
			}
			return result;
		}

		GenApprovalRequest FindMatchingRequestBasedOnParentID(ZGuid parentID)
		{
			GenApprovalRequest result = null;
			var query = new ZQuery(new APInvoiceChargesApprovalRequestCollection(Factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, parentID)).CompleteFilter);
			query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";
			var requests = Factory.Load<APInvoiceChargesApprovalRequest>(query);
			foreach (var request in requests)
			{
				if (request.PostingDetails.TransactionNumber == AH_TransactionNum)
				{
					result = request;
					break;
				}
			}
			return result;
		}

		#endregion

		#endregion

		public HashSet<Job> LineJobsWithMutex => lineJobsWithMutex ?? (lineJobsWithMutex = new HashSet<Job>());
		HashSet<Job> lineJobsWithMutex;

		#endregion

		#region Overridden Properties

		#region AH_ExchangeRate

		internal IAccExchangeRateConfigurationRateConsumer GetExchangeRateConfigurationRateConsumer(Job transactionJob) => transactionJob?.ExchangeRateConfigurationRateConsumer ?? ExchangeRateConfigurationRateConsumerCreator.CreateExchangeRateConfigurationRateConsumerForNonJob(Company);

		internal ZDecimal GetExchangeRateFromJobExRateConfig(Job transactionJob)
		{
			var rate = ZDecimal.Zero;
			var invoiceCurrencyType = InvoiceCurrencyType;
			if (UseJobExchangeRate || Ledger == LedgerTypes.AccountsReceivable)
			{
				if (!IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC))
				{
					rate = AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(GetExchangeRateConfigurationRateConsumer(transactionJob), TransactionCurrency, IsLocalCurrencyTransaction, AH_GC, Header, this.GetExRateLedger(), AH_InvoiceDate, AH_PostDate, InvoiceTaxDate);
				}
				else if (IsLocalCurrencyTransaction)
				{
					rate = 1.0;
				}
				else
				{
					rate = (transactionJob as IExchangeRateProvider)?.GetExchangeRate(AH_RX_NKTransactionCurrency, AH_OH, ExchangeRateEnumsExtensions.GetLedgerFromCode(Ledger), invoiceCurrencyType: invoiceCurrencyType)?.Rate ?? ZDecimal.Zero;
				}
			}

			if (rate.IsEmpty)
			{
				rate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(GetExchangeRateConfigurationRateConsumer(transactionJob), TransactionCurrency, Header, ExchangeRateEnumsExtensions.GetLedgerFromCode(Ledger), invoiceCurrencyType);
				if (rate.IsEmpty)
				{
					rate = AH_ExchangeRate;
				}
			}

			return rate;
		}

		public InvoiceCurrencyType InvoiceCurrencyType => AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(Company, ExchangeRateEnumsExtensions.GetLedgerFromCode(Ledger), IsLocalCurrencyTransaction ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign);

		internal ZDecimal GetExchangeRateFromInvoicingPlugIn(IAccExchangeRateConfigurationRateConsumer rateConsumer)
		{
			ZDecimal rate = ZDecimal.Zero;

			if (ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				rate = Env.CurrentCompany.ExchangeRate.GetRate(currencyCode: AH_RX_NKTransactionCurrency, rateType: ExchangeRateType.Buy, valuationDateTime: AH_InvoiceDate.ToDateTime());
			}
			else if (AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && !AH_ConsolidatedInvoiceRef.IsEmpty)
			{
				rate = AccExchangeRateConfigurationRateFinder.GetExchangeRate(rateConsumer, TransactionCurrency, Header, ExchangeRateEnumsExtensions.GetLedgerFromCode(Ledger), InvoiceCurrencyType.Foreign, false);
			}

			if (rate.IsEmpty)
			{
				if (AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					rate = 1M;
				}
				else
				{
					rate = Env.CurrentCompany.ExchangeRate.TodaysRate(AH_RX_NKTransactionCurrency, ExchangeRateType.Buy);
				}
			}

			if (rate.IsEmpty)
			{
				rate = AH_ExchangeRate;
			}

			return rate;
		}

		public override ZDecimal AH_ExchangeRate
		{
			get { return base.AH_ExchangeRate; }
			set
			{
				IsProxyingHeaderValues = true;
				try
				{
					IsSettingAH_ExchangeRate = true;
					base.AH_ExchangeRate = value;

					if (ShouldProxyHeaderExchangeRate)
					{
						foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
						{
							if (cost.E6_ExchangeRate != AH_ExchangeRate)
							{
								cost.E6_ExchangeRate = AH_ExchangeRate;
								cost.SplitApportionAmount();
							}
						}
						ImportAllApportionmentsFromCostingCore();
						SetTransactionLinesExchangeRate(value);
						AdjustLocalRoundedValuesForImportedConsolCosts();
						LoadInvoiceLineTaxSummaries();
						LoadInvoiceDependentJobs();
					}
				}
				finally
				{
					IsProxyingHeaderValues = false;
					IsSettingAH_ExchangeRate = false;
				}
				RefreshBinding();
			}
		}

		bool IsSettingAH_ExchangeRate;

		public virtual ZBool AH_ExchangeRate_ReadOnly
		{
			get { return !AH_PostedToEFT && AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && !IsInDatabase; }
		}

		bool ShouldProxyHeaderExchangeRate = true;

		IDisposable GetProxyHeaderExchangeRateSuspender()
		{
			return new SuspendProxyingHeaderExchangeRate(this);
		}

		class SuspendProxyingHeaderExchangeRate : IDisposable
		{
			public SuspendProxyingHeaderExchangeRate(InvoicingBase invoice)
			{
				this.invoice = invoice;
				invoice.ShouldProxyHeaderExchangeRate = false;
			}

			readonly InvoicingBase invoice;

			#region IDisposable Members

			public void Dispose()
			{
				invoice.ShouldProxyHeaderExchangeRate = true;
			}

			#endregion
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				using (GetHeaderAmountsUpdateSuspender())
				{
					var originalValue = base.AH_RX_NKTransactionCurrency;
					base.AH_RX_NKTransactionCurrency = value;
					try
					{
						IsProxyingHeaderValues = true;
						if (IsUseJobExchangeRateApplicable)
						{
							using (SuspendRateChangesInAH_PostedToEFT.GetSuspender())
							{
								if (value == Env.CurrentCompany.LocalCurrency.Code && originalValue != Env.CurrentCompany.LocalCurrency.Code)
								{
									UseJobExchangeRate = false;
								}
								if (value != Env.CurrentCompany.LocalCurrency.Code && originalValue == Env.CurrentCompany.LocalCurrency.Code)
								{
									UseJobExchangeRate = AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.Value;
								}
							}
						}
						if (!UseJobExchangeRate)
						{
							SetExchangeRate();
						}
						UpdateAH_OSExTaxAmount();
						SetTransactionLinesCurrency(value);
						if (originalValue != value)
						{
							SetTransactionCategoryForMiscARInvoices();

							if (!originalValue.IsEmpty)
							{
								UpdateAH_OSTaxAmount();
							}
						}

						var changedConsolCostPKs = new HashSet<ZGuid>();
						foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
						{
							if (cost.E6_RX_NKCurrency != AH_RX_NKTransactionCurrency)
							{
								changedConsolCostPKs.Add(cost.PK);
								cost.E6_RX_NKCurrency = AH_RX_NKTransactionCurrency;
							}
						}

						if (UseJobExchangeRate)
						{
							ApplyLogicForAH_PostedToEFTIsTrue(changedConsolCostPKs);
						}
						else
						{
							foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
							{
								if (changedConsolCostPKs.Contains(cost.PK))
								{
									cost.E6_ExchangeRate = AH_ExchangeRate; //Reset Ex Rate - gets overwritten by currency
									cost.SplitApportionAmount();
								}
							}
							ImportAllApportionmentsFromCosting();
							SetTransactionLinesExchangeRate(AH_ExchangeRate);
							AdjustLocalRoundedValuesForImportedConsolCosts();
						}

						RefreshBinding();
						foreach (InvoicingLineBase line in Lines)
						{
							line.GSTInclusiveAmountInfo.RefreshBinding();
						}
					}
					finally
					{
						IsProxyingHeaderValues = false;
					}
				}
			}
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get => base.AH_RX_NKTransactionCurrency_ReadOnly || ReadOnlyForAssociatedDraftInvoice;
			set => base.AH_RX_NKTransactionCurrency_ReadOnly = value;
		}

		void SetTransactionCategoryForMiscARInvoices()
		{
			if (AH_Ledger == LedgerTypes.AccountsReceivable && !IsJobRelated)
			{
				AH_TransactionCategory = GetTransactionCategoryForMiscInvoice(IsDisbursementOrFinal, IsLocalCurrencyTransaction);
			}
		}

		#endregion

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				IsProxyingHeaderValues = true;
				try
				{
					if (AH_OHCore != value)
					{
						ResetSameNumberTransactionDetails();
					}
					var oldValue = AH_OHCore;
					base.AH_OHCore = value;
					bool hasChanged = false;

					if (AH_OHCore != oldValue)
					{
						hasChanged = true;
						SetLinesAL_GSTVATBasis();
					}

					if (Header != null && !IsReversing && SubmittedFromInvoicingForm && !(Factory.HasContext(BusinessContext.InterCompanyInvoiceImport) || Factory.HasContext(BusinessContext.IntercompanyInvoiceAutoImport)))
					{
						ExchangeRate.Currency = HeaderDefaultCurrency;
					}
					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
					Lines.SetGSTReadOnlyStateForAllLines();

					if (!IsSetGSTOnLinesForIntercompanyImportSuspended)
					{
						Lines.CalculateGSTAndWHTForAllLines();
					}

					AH_GB_TaxBranch = AccountingMasterFilesUtils.GetTaxBranchResetValue(CanApplyTaxBranch);

					if (Header != null &&
						(AH_Ledger == LedgerTypes.AccountsPayable ||
						AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
						AH_Ledger == LedgerTypes.IncompleteTransactions))
					{
						IsSelfBillingInvoice = Header.CompanyData.OB_APCostsSelfBilled;
					}

					SetOrganizationForImportedXMLMatchingRules(false);
					SetAH_AgreedPaymentMethodOverride();

					//We recreate lines from consol cost when creditor is changed.
					if (hasChanged)
					{
						foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
						{
							cost.E6_OH_Creditor = value;
						}

						ImportAllApportionmentsFromCosting();

						if (!IsEnabledComplianceDocumentModuleAndHasPCDSetting)
						{
							foreach (InvoicingLineBase line in Lines)
							{
								line.CreateComplianceDocumentRecordOnPosting = false;
							}
						}

						Lines.Cast<InvoicingLineBase>().ForEach(x => x.PeriodApportionment.DefaultClearingAccount());
					}
				}
				finally
				{
					IsProxyingHeaderValues = false;
				}
			}
		}

		protected override bool AH_OH_ReadOnly
		{
			get { return base.AH_OH_ReadOnly || (this is IAmending && ((IAmending)this).IsAmendingTransaction) || ReadOnlyForAssociatedDraftInvoice; }
		}

		int fIsProxyingHeaderValues;
		protected internal bool IsProxyingHeaderValues
		{
			get { return fIsProxyingHeaderValues > 0; }
			private set
			{
				fIsProxyingHeaderValues += value ? 1 : -1;
				if (fIsProxyingHeaderValues < 0)
				{
					fIsProxyingHeaderValues = 0;
				}
			}
		}

		#endregion

#if DEBUG
		internal void SetIsProxyingHeaderValuesForTestOnly(bool value)
		{
			IsProxyingHeaderValues = value;
		}
#endif

		#region Suspend Auto GST Calculations

		bool fIsSettingGSTOnLinesSuspended;
		public bool IsSettingGSTOnLinesSuspended
		{
			get { return fIsSettingGSTOnLinesSuspended; }
			private set { fIsSettingGSTOnLinesSuspended = value; }
		}

		public bool IsSetGSTOnLinesForIntercompanyImportSuspended { get; private set; }

		public IDisposable GetSetGSTOnLinesSuspender()
		{
			return new SetGSTOnLinesSuspender(this);
		}

		public IDisposable GetSetGSTOnLinesForIntercompanyImportSuspender()
		{
			return new SetGSTOnLinesForIntercompanyImportSuspender(this);
		}

		class SetGSTOnLinesSuspender : IDisposable
		{
			public SetGSTOnLinesSuspender(InvoicingBase invoice)
			{
				this.invoice = invoice;
				invoice.IsSettingGSTOnLinesSuspended = true;
			}

			readonly InvoicingBase invoice;

			public void Dispose()
			{
				invoice.IsSettingGSTOnLinesSuspended = false;
			}
		}

		class SetGSTOnLinesForIntercompanyImportSuspender : IDisposable
		{
			public SetGSTOnLinesForIntercompanyImportSuspender(InvoicingBase invoice)
			{
				this.invoice = invoice;
				invoice.IsSetGSTOnLinesForIntercompanyImportSuspended = true;
			}

			readonly InvoicingBase invoice;

			public void Dispose()
			{
				invoice.IsSetGSTOnLinesForIntercompanyImportSuspended = false;
			}
		}

		#endregion

		#region AH_TransactionCategory

		public override ZString AH_TransactionCategory
		{
			get { return base.AH_TransactionCategory; }
			set
			{
				base.AH_TransactionCategory = value;
				TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
				SetAH_AgreedPaymentMethodOverride();
			}
		}

		public ZString TransactionCategoryDescription
		{
			get { return InvoiceTypes.GetDescriptionFromCode(AH_TransactionCategory); }
		}

		public ZPropertyInfo TransactionCategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionCategoryDescription)); }
		}

		#endregion

		#region AH_InvoiceDate

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				var calValue = Factory.HasContext(BusinessContext.UseCurrentDateAsTransactionDateWhenAutoPosting)
					? ZDateTime.Today
					: value;

				var shouldUseDefaultDate = ShouldUseDefaultARInvoiceAndPostDate();
				var defaultDate = shouldUseDefaultDate ? ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(value) : calValue;
				if (AH_InvoiceDate != defaultDate)
				{
					ResetSameNumberTransactionDetails();
				}
				base.AH_InvoiceDate = defaultDate;
				invComplSeq = null;

				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (AH_InvoiceDate != cost.E6_InvoiceDate)
					{
						cost.E6_InvoiceDate = AH_InvoiceDate;
					}
				}

				if (!this.HasContext(BusinessContext.OverrideInvoiceReference))
				{
					CalculateDueDate();
				}

				if (!IsPosted && ((Factory.HasContext(BusinessContext.ReverseDateForm) && AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate)
					|| shouldUseDefaultDate))
				{
					AH_PostDate = AH_InvoiceDate;
				}

				if (IsDocumentReceivedDateApplicable)
				{
					SetDefaultDocumentReceivedDate();
				}

				if (!IsInDatabase || ShouldSetExchangeRateWhenSetInvoiceDate)
				{
					SetExchangeRateForInvoicePostingExchangeRateOption(
						AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code,
						AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code);
				}
			}
		}

		public virtual bool ShouldSetExchangeRateWhenSetInvoiceDate => AH_Ledger == LedgerTypes.IncompleteTransactions;

		void AddInvoiceDateLog()
		{
			if (this.HasContext(BusinessContext.OverrideInvoiceReference) &&
				AH_Ledger == LedgerTypes.AccountsPayable &&
				(ZDateTime)AH_InvoiceDateInfo.OriginalValue != AH_InvoiceDate)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture,
					"Invoice Date changed from '{0}' to '{1}'",
					IsInDatabase ? AH_InvoiceDateInfo.OriginalValue : ZString.Empty,
					AH_InvoiceDate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void AddDueDateLog()
		{
			if (this.HasContext(BusinessContext.OverrideTransactionAgreedPaymentMethod) &&
				IsReceivableOrPayable &&
				(ZDateTime)AH_DueDateInfo.OriginalValue != AH_DueDate)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture,
					"Due Date changed from '{0}' to '{1}'",
					IsInDatabase ? AH_DueDateInfo.OriginalValue : ZString.Empty,
					AH_DueDate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void AddGovernmentAllocatedNumberLog()
		{
			if (IsPropertyLoggable(AH_GovernmentAllocatedIDInfo))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Government Allocated Number changed from '{0}' to '{1}'", AH_GovernmentAllocatedIDInfo.OriginalValue, AH_GovernmentAllocatedID));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		bool IsPropertyLoggable(ZPropertyInfo info)
		{
			return info.OriginalValue != null && !info.OriginalValue.IsEmpty && info.HasChanges;
		}

		public override ZDateTime DefaultInvoiceDate => ShouldUseDefaultBlankInvoiceDateForAP ? ZDateTime.Empty : ZDateTime.Now;

		protected bool ShouldUseDefaultBlankInvoiceDateForAP => IsAPInvoiceOrCreditNoteOrAdjustmentNote &&
					 AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.Value == AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank;

		public bool IsAPInvoiceOrCreditNoteOrAdjustmentNote => AH_Ledger == LedgerTypes.AccountsPayable &&
					IsInvoiceOrCreditNoteOrAdjustmentNote;

		public bool IsARInvoiceOrCreditNoteOrAdjustmentNote => AH_Ledger == LedgerTypes.AccountsReceivable &&
					IsInvoiceOrCreditNoteOrAdjustmentNote;

		public bool IsAPTransaction => this is APInvoice || this is APCreditNote || this is APAdjustmentNote;

		protected override bool AH_InvoiceDate_ReadOnly
		{
			get { return base.AH_InvoiceDate_ReadOnly || ShouldUseDefaultARInvoiceAndPostDate() || ReadOnlyForAssociatedDraftInvoice; }
		}

		#endregion

		#region AH_DocumentReceivedDate

		public override ZDateTime AH_DocumentReceivedDate
		{
			get => base.AH_DocumentReceivedDate;
			set
			{
				base.AH_DocumentReceivedDate = value;
				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (AH_DocumentReceivedDate != cost.E6_DocumentReceivedDate)
					{
						cost.E6_DocumentReceivedDate = AH_DocumentReceivedDate;
					}
				}

				CalculateDueDate();
			}
		}

		protected bool AH_DocumentReceivedDate_ReadOnly => ReadOnlyForAssociatedDraftInvoice;

		internal void SetDefaultDocumentReceivedDate()
		{
			var defaultLogic = AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.Value;
			if (AH_DocumentReceivedDate.IsEmpty && defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate)
			{
				AH_DocumentReceivedDate = ZDateTime.Now;
			}
			else if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate)
			{
				AH_DocumentReceivedDate = AH_InvoiceDate;
			}
		}

		#endregion

		public override ZDateTime AH_DueDate
		{
			get
			{
				return base.AH_DueDate;
			}
			set
			{
				base.AH_DueDate = value;
				foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
				{
					if (value != cost.E6_PaymentDate)
					{
						cost.E6_PaymentDate = value;
					}
				}
			}
		}

		protected override bool AH_DueDate_ReadOnly => base.AH_DueDate_ReadOnly || (AH_Ledger == LedgerTypes.AccountsReceivable && !IsSourceReferenceUsed) || ReadOnlyForAssociatedDraftInvoice;

		protected virtual bool AH_OSTotalAmount_ReadOnly => ReadOnly || !IsSourceReferenceUsed;

		public void CalculateDueDate()
		{
			if (OriginalTransaction != null)
			{
				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					string registryValue = AccountingConfigurationRegistry.Instance.AllowARReversalDueDateCalculation.Value;
					if (registryValue == AccountingConstants.ReversalDueDateCalculation.DebtorsTerms)
					{
						TermsAndDueDateCalculationProvider.CalculateDueDate();
					}
					else if (registryValue == AccountingConstants.ReversalDueDateCalculation.OriginalInvoiceDueDate)
					{
						AH_DueDate = OriginalTransaction.AH_DueDate;
					}
				}
				else if (AH_Ledger == LedgerTypes.AccountsPayable)
				{
					string registryValue = AccountingConfigurationRegistry.Instance.AllowAPReversalDueDateCalculation.Value;
					if (registryValue == AccountingConstants.ReversalDueDateCalculation.CreditorsTerms)
					{
						TermsAndDueDateCalculationProvider.CalculateDueDate();
					}
					else if (registryValue == AccountingConstants.ReversalDueDateCalculation.OriginalInvoiceDueDate)
					{
						AH_DueDate = OriginalTransaction.AH_DueDate;
					}
				}
			}
			else
			{
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}

		public void SetExchangeRateForInvoicePostingExchangeRateOption(params string[] invoicePostingExchangeRateOptions) => SetExchangeRateForInvoicePostingExchangeRateOption(false, invoicePostingExchangeRateOptions);

		public void SetExchangeRateForInvoicePostingExchangeRateOption(bool shouldForceUpdateLineExRateForLocalCurrencyTransaction, params string[] invoicePostingExchangeRateOptions)
		{
			if (ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC, invoicePostingExchangeRateOptions))
			{
				if (IsLocalCurrencyTransaction)
				{
					SetExchangeRate(shouldForceUpdateLineExRateForLocalCurrencyTransaction);
				}
				else
				{
					SetExchangeRate();
				}
			}
		}

		public override ZDateTime AH_PostDate
		{
			get
			{
				return base.AH_PostDate;
			}
			set
			{
				var shouldUseDefaultDate = ShouldUseDefaultARInvoiceAndPostDate();
				var defaultDate = shouldUseDefaultDate ? ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(value) : value;

				var originalDate = AH_PostDate;
				base.AH_PostDate = defaultDate;
				invComplSeq = null;

				SetExchangeRateForInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

				if (originalDate != AH_PostDate)
				{
					if (IsReverseTransaction && OriginalTransaction != null)
					{
						ObjectFactory.Get<ITaxProcessor>().UpdatePostDateOnParentReversing(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(OriginalTransaction as InvoicingBase),
																						   TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(this),
																						   AH_PostDate.Date);
					}
					AdjustPostDateForRelatedReversalJournals();
				}
			}
		}

		void AdjustPostDateForRelatedReversalJournals()
		{
			if (IsReverseTransaction && RelatedApportionmentReversings.Any() && OriginalTransaction != null)
			{
				RelatedApportionmentReversings.ForEach(x => x.AdjustPostDateForReversalJournal());
			}
		}

		public List<PeriodApportionmentGLJournalReversing> RelatedApportionmentReversings = new List<PeriodApportionmentGLJournalReversing>();

		protected override bool AH_PostDate_ReadOnly
		{
			get
			{
				var ledgersWithAllowedInvoiceEditing = new[] { LedgerTypes.IncompleteTransactions, LedgerTypes.TransactionsPendingAllocation };

				return (!ledgersWithAllowedInvoiceEditing.Contains(AH_Ledger.ToString()) && !IsCompletingInvoice && base.AH_PostDate_ReadOnly)
					|| (AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && IsReversing)
					|| ShouldUseDefaultARInvoiceAndPostDate()
					|| ReadOnlyForAssociatedDraftInvoice;
			}
		}

		#region AH_InvoiceTerm

		[List("InvoiceTerms_List")]
		public override ZString AH_InvoiceTerm
		{
			get { return base.AH_InvoiceTerm; }
			set
			{
				base.AH_InvoiceTerm = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
				AH_InvoiceTermInfo.RefreshBinding();
				AH_InvoiceTermDaysInfo.RefreshBinding();
				if (value == Constants.InvoiceTerms.CashOnDelivery || value == Constants.InvoiceTerms.PaymentInAdvance)
				{
					AH_InvoiceTermDays = ZByte.Zero;
				}
			}
		}

		#endregion

		#region AH_InvoiceTermDays

		public override ZByte AH_InvoiceTermDays
		{
			get { return base.AH_InvoiceTermDays; }
			set
			{
				base.AH_InvoiceTermDays = value;
				TermsAndDueDateCalculationProvider.CalculateDueDate();
			}
		}

		#endregion

		#region SetOutstandingLocalAmountAfterLocalExTaxAmountSet

		protected override void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
			SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore();
		}

		#endregion

		public override ZDecimal AH_OutstandingAmount
		{
			get { return base.AH_OutstandingAmount; }
			set
			{
				base.AH_OutstandingAmount = value;
				if (IsCreditLimitCheckApplicable)
				{
					Validation.ValidateAH_OH();
				}
			}
		}

		public override ZDecimal AH_LocalTaxAmount
		{
			get { return base.AH_LocalTaxAmount; }
			set
			{
				base.AH_LocalTaxAmount = value;
				RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
				UpdateLocalOutstandingAmount();
			}
		}

		public override ZDecimal AH_OSTaxAmountOtherTaxes
		{
			get => base.AH_OSTaxAmountOtherTaxes;
			set
			{
				base.AH_OSTaxAmountOtherTaxes = value;
				RecalculateExchangeRateFromAmountsOnlyWhenItAllowedToBeModified();
			}
		}

		public override ZDecimal AH_LocalTaxAmountOtherTaxes
		{
			get => base.AH_LocalTaxAmountOtherTaxes;
			set
			{
				base.AH_LocalTaxAmountOtherTaxes = value;
				RecalculateExchangeRateFromAmountsOnlyWhenItAllowedToBeModified();
				UpdateLocalOutstandingAmount();
			}
		}

		//This is actually AH_OSExTaxAmount + AH_OSTaxAmount, but we can't calculate that way as these values are restored from OS Total using ex rate and so not so precise
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSSubTotalAmountWithGSTOnly => AH_OSTotalAmount - AH_OSTaxAmountOtherTaxes_ForDisplay;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalSubTotalAmountWithGSTOnly => AH_LocalExTaxAmount + AH_LocalTaxAmount;

		public override JobHeader Job
		{
			get
			{
				Job job = Factory.Load<Job>(AH_JH);
				if (job != null)
				{
					job.InitializeParentFromGenericJobWithoutSettingDefaults();
				}
				return job;
			}
		}

		public override Directions JobDirection
		{
			get
			{
				if (InvoicingJob != null)
				{
					return InvoicingJob.MovementDirection;
				}
				else if (IsConsolInvoice & AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var consolJob = Factory.LoadFromUniqueKey<CommonConsol>(JobConsolSchema.JK_UniqueConsignRef, ConsolNumberFromConsolidatedInvoiceRef);
					return consolJob != null ? consolJob.JobDirection : Directions.Unknown;
				}
				else
				{
					return base.JobDirection;
				}
			}
		}

		public Job InvoicingJob
		{
			get { return Job as Job; }
		}

		public override ZString AH_Ledger
		{
			get { return base.AH_Ledger; }
			set
			{
				var oldValue = AH_Ledger;
				base.AH_Ledger = value;
				if (AH_Ledger != oldValue)
				{
					SetLinesAL_GSTVATBasis();
				}
			}
		}

		public JobInvoicingConsumerType JobType
		{
			get { return this.InvoicingJob != null ? this.InvoicingJob.JobType : null; }
		}

		public ZString Direction
		{
			get { return this.InvoicingJob != null ? this.InvoicingJob.Direction : ZString.Empty; }
		}

		public ZString TransportMode
		{
			get { return this.InvoicingJob != null ? this.InvoicingJob.TransportMode : ZString.Empty; }
		}

		public ZString ShortUniqueDescription
		{
			get
			{
				var result = AH_Ledger + " " + AH_TransactionType + " " + AH_TransactionNum;
				if (AH_Ledger == LedgerTypes.AccountsPayable)
				{
					result += " (" + Header?.OH_Code + ") [" + AH_InvoiceDate.ToShortDateString() + "]";
				}
				return result;
			}
		}

		protected override ZString GetPostedByCore()
		{
			var postedLog = Logs.Find(x => x.SL_Reference == GetLogReference(AccountingConstants.InvoiceAdditionalReference.Posted)).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
			return postedLog?.SL_UserNameAndInitials ?? base.GetPostedByCore();
		}

		protected override ZDecimal AH_NotionalWHTTaxCore => Factory.LoadNotionalWHT(PK);

		protected override ZDecimal AH_RealizedWHTTaxCore => Factory.LoadRealizedWHT(PK);

		public override ZString AH_ComplianceSubType
		{
			get => base.AH_ComplianceSubType;
			set
			{
				base.AH_ComplianceSubType = value;
				invComplSeq = null;
				Lines.UpdateTaxAndTotalReadOnlyStateForAllLines();
			}
		}

		public virtual bool AH_ComplianceSubType_ReadOnly
		{
			get
			{
				return ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeGUIProvider(Company.GC_RN_NKCountryCode)?.ComplianceSubTypeIsReadOnly(hasBeenCreatedAsAmending, AH_Ledger, AH_TransactionType, OriginalTransactionReference.IsEmpty) ?? false;
			}
		}

		public override ZGuid AH_XD_ComplianceBook
		{
			get
			{
				return base.AH_XD_ComplianceBook;
			}
			set
			{
				if (Company.Country.Code == CountryCodes.VietNam
					&& AH_Ledger == LedgerTypes.AccountsReceivable
					&& IsPeriodicInvoice
					&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(Company.PK.ToGuid(), Guid.Empty, Guid.Empty)
					&& !AH_TransactionReference.IsEmpty
					&& value.IsEmpty)
				{
					ErrorReporter.ReportOnce("SetAH_XD_ComplianceBookEmpty", "Set AH_XD_ComplianceBook to empty for VN AR Periodic Invoice which EnableEInvoicingFunctionality is Yes and AH_TransactionReference is not empty." + System.Environment.NewLine + System.Environment.NewLine + new StackTrace().ToString());
				}

				base.AH_XD_ComplianceBook = value;
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsSelfBillingInvoice = false;

			if (AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				SetDefaultComplianceSubTypeBasedOnTransactionTypeAndCountry();
			}
		}

		internal void SetDefaultComplianceSubTypeBasedOnTransactionTypeAndCountry()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Peru)
			{
				if (AH_TransactionType == TransactionTypes.Invoice)
				{
					base.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				}
				else if (AH_TransactionType == TransactionTypes.CreditNote)
				{
					base.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCR;
				}
			}
		}

		#endregion

		#region Document Printed

		protected override void OnDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			base.OnDocumentPrinted(sender, e);
			// Precondition = AH_InvoicePrinted flag is set to 'N'.
			// Transaction is already posted.
			UpdatePrintedFlag();
		}

		#endregion

		#region Document Requested via Web

		public void AddRequestedViaWebLog()
		{
			Logs.AddNew(Events.DocumentDelivered, InvoiceRequestedViaWebReference);
		}

		public ZDateTime LastRequestedViaWeb
		{
			get
			{
				ZQuery eventFilter = new ZQuery(StmALogSchema.SL_Parent, this.PK);
				eventFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DocumentDelivered.Code);
				eventFilter.OrderBy = StmALogSchema.SL_EventTime.Name + " DESC";

				StmALog lastEvent = Factory.LoadTop1<StmALog>(eventFilter);

				return lastEvent != null ? lastEvent.SL_EventTime : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo LastRequestedViaWebInfo
		{
			get { return GetZPropertyInfo(Schema.LastRequestedViaWeb); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Standard description")]
		const string InvoiceRequestedViaWebReference = "Requested via Web";

		#endregion

		#region Related Business Objects

		#region Lines

		public ZGuid DefaultChargeCodeForLines { get; set; }

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new InvoicingLineBaseCollection(this);
		}

		bool IsLinesLoaded;
		[ChildEditable(true)]
		public new InvoicingLineBaseCollection Lines
		{
			get
			{
				InvoicingLineBaseCollection result = (InvoicingLineBaseCollection)base.Lines;
				if (!IsLinesLoaded)
				{
					result.CountChanged += (_, _) =>
					{
						InvoiceTaxDateCacheProvider.StaleCache();
					};
				}

				IsLinesLoaded = true;
				return result;
			}
		}

		public new InvoicingLineBaseCollection GetComplianceRelatedLines(AccComplianceDocumentHeader compliance)
		{
			return (InvoicingLineBaseCollection)base.GetComplianceRelatedLines(compliance);
		}

		public new InvoicingLineBaseCollection GetComplianceRelatedLines(AccTransactionLinesCollection lines)
		{
			return (InvoicingLineBaseCollection)base.GetComplianceRelatedLines(lines);
		}

		public FilteredInvoicingLineBaseCollectionView FilteredLines
		{
			get
			{
				if (filteredLines == null && Lines != null)
				{
					filteredLines = new FilteredInvoicingLineBaseCollectionView(Lines);
				}
				return filteredLines;
			}
		}

		FilteredInvoicingLineBaseCollectionView filteredLines;

		public ZDecimal GetAmountOfLinesWithSpecificCurrency(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (InvoicingLineBase line in Lines)
			{
				if (line.AL_RX_NKTransactionCurrency == currencyCode)
				{
					result += line.AL_OSAmount;
				}
			}

			return result;
		}

		public ZDecimal GetOSAmountOfMatchedLinesWithSpecificCurrency(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (ILineMatching line in Lines)
			{
				if (!line.PaidAmount.IsEmpty && line.ChargeCurrency == currencyCode)
				{
					result += line.ChargeAmount;
				}
			}

			return result;
		}

		public ZDecimal GetLocalAmountOfMatchedLinesWithSpecificCurrency(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (ILineMatching line in Lines)
			{
				if (!line.PaidAmount.IsEmpty && line.ChargeCurrency == currencyCode)
				{
					result += line.LocalPaidAmount;
				}
			}

			return result;
		}

		public ZDecimal GetWeightedExchangeRate(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;
			ZDecimal totalLocal = ZDecimal.Zero;
			ZDecimal totalOS = ZDecimal.Zero;

			foreach (InvoicingLineBase line in Lines)
			{
				if (line.AL_RX_NKTransactionCurrency == currencyCode)
				{
					totalLocal += line.AL_LocalTotalAmount;
					totalOS += line.AL_OSAmount;
				}
			}

			if (totalLocal != 0m)
			{
				result = Math.Abs(totalOS / totalLocal);
			}
			return result;
		}

		public ZDecimal GetWeightedExchangeRateOfMatchedLines(ZString currencyCode)
		{
			ZDecimal result = ZDecimal.Zero;
			ZDecimal totalLocal = ZDecimal.Zero;
			ZDecimal totalOS = ZDecimal.Zero;

			foreach (ILineMatching line in Lines)
			{
				if (!line.PaidAmount.IsEmpty && line.ChargeCurrency == currencyCode)
				{
					totalLocal += line.LocalPaidAmount;
					totalOS += line.ChargeAmount;
				}
			}

			if (totalLocal != 0m)
			{
				result = Math.Abs(totalOS / totalLocal);
			}
			return result;
		}

		public bool ContainsMatchedLinesInSpecificCurrency(string currencyCode)
		{
			foreach (ILineMatching line in Lines)
			{
				if (!line.PaidAmount.IsEmpty && currencyCode == line.ChargeCurrency)
				{
					return true;
				}
			}
			return false;
		}

		public override ZDateTime InvoiceTaxDate => InvoiceTaxDateCacheProvider.GetEarliestInvoiceTaxDate(Lines.Cast<InvoicingLineBase>(), AH_InvoiceDate);

		#endregion

		#region InvoiceDependentJobs

		InvoiceDependentJobCollection fInvoiceDependentJob;
		public InvoiceDependentJobCollection InvoiceDependentJobs
		{
			get
			{
				if (fInvoiceDependentJob == null)
				{
					fInvoiceDependentJob = new InvoiceDependentJobCollection(this, Factory);
				}
				fInvoiceDependentJob.RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab();
				return fInvoiceDependentJob;
			}
		}

		public void LoadInvoiceDependentJobs()
		{
			InvoiceDependentJobs.RefreshBinding();
		}

		#endregion

		#region InvoiceLineTaxSummaries

		public InvoicingLineTaxSummaryCollection InvoiceLineTaxSummaries
		{
			get
			{
				if (fInvoiceLineTaxSummaries == null)
				{
					fInvoiceLineTaxSummaries = new InvoicingLineTaxSummaryCollection();
				}
				return fInvoiceLineTaxSummaries;
			}
		}
		InvoicingLineTaxSummaryCollection fInvoiceLineTaxSummaries;

		public bool IsTaxSummaryTabSelected { get; set; }

		public void LoadInvoiceLineTaxSummaries()
		{
			if (IsTaxSummaryTabSelected)
			{
				InvoiceLineTaxSummaries.Load(Lines);
			}
		}

		#endregion

		#endregion

		#region RelatedJobsForReversing

		internal void ResetRelatedJobsForReversing()
		{
			fRelatedJobsForReversing = null;
		}

		public IEnumerable<Job> RelatedJobsForReversing
		{
			get
			{
				if (fRelatedJobsForReversing == null)
				{
					fRelatedJobsForReversing = new List<Job>();

					if (TransactionType == TransactionTypes.Invoice || TransactionType == TransactionTypes.CreditNote ||
						TransactionType == TransactionTypes.UAInvoice || TransactionType == TransactionTypes.UACreditNote)
					{
						foreach (InvoicingLineBase invoiceLine in Lines)
						{
							if (invoiceLine.InvoicingJob != null)
							{
								fRelatedJobsForReversing.Add(invoiceLine.InvoicingJob);
							}
						}

						if (fRelatedJobsForReversing.Count == 0 && Ledger == LedgerTypes.AccountsReceivable && !AH_TransactionBelongsToGroup.IsEmpty)
						{
							ZDBOnlyQuery jobsQuery = new ZDBOnlyQuery(typeof(JobHeader));
							ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(TransactionLine), AccTransactionLinesSchema.AL_JH);
							ZDBOnlySubQuery transactionHeaderSubQuery = new ZDBOnlySubQuery(typeof(TransactionHeader), AccTransactionHeaderSchema.PK);
							ZDBOnlySubQuery jobConsolCostSubQuery = new ZDBOnlySubQuery(typeof(JobConsolCost), JobConsolCostSchema.E6_AH_APInvoice);

							jobConsolCostSubQuery.AddToFilter(JobConsolCostSchema.E6_AH_ARInvoice, AH_TransactionBelongsToGroup);
							transactionHeaderSubQuery.AddSubQuery(jobConsolCostSubQuery, JoinCondition.And);
							transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_AH, transactionHeaderSubQuery, JoinCondition.And);
							jobsQuery.AddSubQuery(transactionLinesSubQuery, JoinCondition.And);
							jobsQuery.AddToFilter(JobHeaderSchema.JH_GC, AH_GC);

							Job[] jobs = Factory.Load<Job>(jobsQuery);
							if (jobs.Length > 0)
							{
								fRelatedJobsForReversing.AddRange(jobs);
							}
						}
					}
				}

				return fRelatedJobsForReversing.Where(x => !x.IsDeleted);
			}
		}
		List<Job> fRelatedJobsForReversing;

		public bool HasClosedJob
		{
			get
			{
				bool result = false;

				if (Factory.ServiceContainer.GetService<JobHeaderReloader>() == null)
				{
					Factory.ServiceContainer.AddService(new JobHeaderReloader(Factory));
				}

				foreach (Job job in RelatedJobsForReversing)
				{
					if (job.JH_Status == JobHeaderStatus.Closed.Code)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public bool ReOpenClosedJob()
		{
			bool result = true;

			foreach (Job job in RelatedJobsForReversing)
			{
				if (job.JH_Status == JobHeaderStatus.Closed.Code)
				{
					if (JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(this, job))
					{
						ISecurityOverrideProvider originalProvider = job.SecurityOverrideProvider;
						job.SecurityOverrideProvider = SecurityOverrideProvider;
						try
						{
							job.ReOpenByImport(string.Format(CultureInfo.InvariantCulture, " - {0} {1} {2} ({3})", Ledger, TransactionType, AH_TransactionNum, Header?.OH_Code));
						}
						finally
						{
							job.SecurityOverrideProvider = originalProvider;
						}
					}
					else
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Invoice Transaction Reference

		public override ZString InvoiceTransactionReference
		{
			get
			{
				if (SupportHeaderReference)
				{
					var reference = GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR);
					return reference != null ? reference.AH1_Reference : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		bool SupportHeaderReference => TransactionType == TransactionTypes.Invoice || TransactionType == TransactionTypes.CreditNote || TransactionType == TransactionTypes.AdjustmentNote;

		bool IsInvoiceOrCreditNoteOrAdjustmentNote => AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote || AH_TransactionType == TransactionTypes.AdjustmentNote;

		public void SetInvoiceTransactionReference()
		{
			AccTransactionHeaderReference headerReference = null;
			if (IsInDatabase)
			{
				headerReference = this.GetTransactionHeaderReference(AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR);
			}
			if (headerReference == null)
			{
				headerReference = Factory.New<AccTransactionHeaderReference>();

				headerReference.AH1_AH = this.PK;
				headerReference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR;
				headerReference.AH1_Reference = Env.NumberFountains.InvoiceTransactionReference(AH_GC.ToGuid()).GetNextFormatted(Factory);
			}
		}

		#endregion

		#region Invoice Remittance Reference

		protected bool InvoiceRemittanceReference_ReadOnly => Ledger == LedgerTypes.AccountsReceivable;

		InvoiceRemittanceConfiguration InvoiceRemittanceConfiguration
		{
			get
			{
				if (invoiceRemittanceConfiguration == null && !AH_InvoicePaymentReferenceCode.IsEmpty)
				{
					invoiceRemittanceConfiguration = GetInvoiceRemittanceConfigurationCollection().FirstOrDefault(c => c.Code == AH_InvoicePaymentReferenceCode);
				}
				return invoiceRemittanceConfiguration;
			}
			set { invoiceRemittanceConfiguration = value; }
		}
		InvoiceRemittanceConfiguration invoiceRemittanceConfiguration;

		void SetARInvoiceRemittance()
		{
			if (InvoiceRemittanceConfiguration == null)
			{
				var paymentReferenceCodeCollection = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.GetValueWithoutFallback(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (paymentReferenceCodeCollection != null && paymentReferenceCodeCollection.Count > 0)
				{
					InvoiceRemittanceConfiguration = paymentReferenceCodeCollection.GetBestMatchInvoiceRemittanceConfiguration(Header);
				}
			}

			if (InvoiceRemittanceConfiguration != null)
			{
				if (AH_InvoicePaymentReferenceCode.IsEmpty)
				{
					AH_InvoicePaymentReferenceCode = InvoiceRemittanceConfiguration.Code;
				}

				InvoiceRemittanceConfiguration = null;

				var calculatedRemittanceReference = InvoiceRemittanceConfiguration.GetInvoiceRemittanceReference(this);
				if (calculatedRemittanceReference.Length > InvoiceRemittanceReferenceInfo.MaxLength)
				{
					this.SetContext(BusinessContext.RemittanceReferenceNumberExceedMaxLength);

					var dictionary = Factory.GetCachedValue("RemittanceReferenceNumber", () => { return new Dictionary<ZGuid, ZString>(); });
					dictionary[PK] = calculatedRemittanceReference;
				}
				else
				{
					if (this.HasContext(BusinessContext.RemittanceReferenceNumberExceedMaxLength))
					{
						this.RemoveContext(BusinessContext.RemittanceReferenceNumberExceedMaxLength);
					}
					InvoiceRemittanceReference = calculatedRemittanceReference;
					AddHeaderReferenceLog();
				}
			}
		}

		public void AddHeaderReferenceLog()
		{
			Action addReferenceLog = () =>
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Remittance Reference changed from '{0}' to '{1}'", TransactionHeaderReferenceIRR.IsInDatabase ? TransactionHeaderReferenceIRR.AH1_ReferenceInfo.OriginalValue : ZString.Empty, TransactionHeaderReferenceIRR.AH1_Reference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			};
			if (this.HasContext(BusinessContext.OverrideInvoiceRemittanceType))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Invoice Remittance Type changed from '{0}' to '{1}'", AH_InvoicePaymentReferenceCodeInfo.OriginalValue, AH_InvoicePaymentReferenceCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				addReferenceLog();
			}
			if (this.HasContext(BusinessContext.OverrideInvoiceReference))
			{
				addReferenceLog();
			}
		}

		#endregion

		#region IInvoiceRemittance

		ZString IInvoiceRemittance.InvoiceNumber => InvoiceNumber;

		ZString IInvoiceRemittance.InvoiceNumberInNumeric => GetInvoiceNumberInNumeric(InvoiceNumber);

		ZString GetInvoiceNumberInNumeric(string invoiceNumber)
		{
			var result = new ZStringBuilder();
			if (!string.IsNullOrEmpty(invoiceNumber))
			{
				foreach (char item in invoiceNumber.ToUpper(CultureInfo.InvariantCulture).ToCharArray())
				{
					if (Regex.IsMatch(item.ToString(), "^[A-Z]$"))
					{
						var letterValue = Convert.ToInt32(item) - 64;
						result.Append(letterValue > 9 ? letterValue.ToString(CultureInfo.InvariantCulture) : "0" + letterValue.ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						result.Append(item.ToString());
					}
				}
			}
			return result.ToString();
		}

		ZString IInvoiceRemittance.InvoiceTotalInLocalCurrency
		{
			get
			{
				var amount = Utilities.Round(AH_LocalTotalAmount, AH_LocalTotalAmount.DecimalPlaces).ToString(CultureInfo.InvariantCulture).Replace(".", "");
				if (InvoiceRemittanceConfiguration != null)
				{
					return InvoiceRemittanceConfiguration.GetInvoiceTotalInLocalCurrencyDigitCode(amount);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZString IInvoiceRemittance.InvoiceTotalInInvoiceCurrency
		{
			get
			{
				var amount = Utilities.Round(AH_OSTotalAmount, AH_OSTotalAmount.DecimalPlaces).ToString(CultureInfo.InvariantCulture).Replace(".", "");
				if (InvoiceRemittanceConfiguration != null)
				{
					return InvoiceRemittanceConfiguration.GetInvoiceTotalInInvoiceCurrencyDigitCode(amount);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZString IInvoiceRemittance.Message => InvoiceRemittanceConfiguration != null ? InvoiceRemittanceConfiguration.Message : ZString.Empty;

		ZString IInvoiceRemittance.BillerCode => InvoiceRemittanceConfiguration != null ? InvoiceRemittanceConfiguration.BillerCode : ZString.Empty;

		ZString IInvoiceRemittance.BillerAccountNumber => InvoiceRemittanceConfiguration != null ? InvoiceRemittanceConfiguration.BillerAccountNumber : ZString.Empty;

		ZString IInvoiceRemittance.DebtorOrganizationCode => Header.OH_Code;

		ZString IInvoiceRemittance.DebtorClientNumber => Header.CompanyData.OB_ARClientNumber;

		ZString IInvoiceRemittance.InvoiceTransactionReference => InvoiceTransactionReference;

		#endregion

		#region Receivable Disbursement Invoice

		public ZGuid ReceivableDisbursementInvoicePK;

		void CreateReceivableDisbursementInvoiceReference()
		{
			if (ReceivableDisbursementInvoicePK.IsValid)
			{
				var headerReference = Factory.New<AccTransactionHeaderReference>();

				headerReference.AH1_AH = ReceivableDisbursementInvoicePK;
				headerReference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice;
				headerReference.AH1_Reference = AH_TransactionNum;
			}
		}

		#endregion

		#region Saving

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event SaveEventHandler OnSavedHander;
		public delegate void SaveEventHandler(bool saveSuccessful);

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
			using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
			using (GetValidationSuspender())
			using (GetValidationSuspenderForLines())
			using (GetValidationSuspenderForJobs())
			{
				if (IsPosting)
				{
					if (Factory.RefreshEnabled && Lines.Count > AccountingUtils.ObjectCountThresholdToDisableDataRefreshBus)
					{
						Factory.RefreshEnabled = false;
					}
				}

				IsProxyingHeaderValues = true;

				CreateTasksAndMilestonesFromTemplate();

				RemoveNotRequiredJobsAndChargesFromCostsAndLines();

				ClearValidatedConsolCostPKList();

				base.OnFactorySavingBeforeTransactionCore();

				if (!IsInMatchingContext && Factory.ServiceContainer.GetService<JobHeaderReloader>() == null)
				{
					Factory.ServiceContainer.AddService(new JobHeaderReloader(Factory));
				}

				if (IsReversed && ReverseInvoice != null)
				{
					ReverseInvoicingTransformer transformer = new ReverseInvoicingTransformer(Factory);
					if (MustTransform)
					{
						transformer.Transform(this);
					}
					ReverseInvoice.ClearedJobCharges = transformer.Charges;
				}

				if (IsPosting)
				{
					foreach (InvoicingLineBase line in Lines)
					{
						line.AL_PostDate = AH_PostDate;
						SetJobRevenueRecognitionDate(line);
					}

					if (ShouldRunJobChargeTransformer)
					{
						if (!HasJobChargeTransformerBeenRun)
						{
							TransactionLineJobChargeTransformer.Transform(this);
							HasJobChargeTransformerBeenRun = true;
						}
					}

					if (!AH_OverrideExchangeRate && !UseJobExchangeRate)
					{
						foreach (InvoicingLineBase line in Lines)
						{
							line.CalculateHighPrecisionExchangeRate();
						}
					}

					SetJobChargesWIPAccrualCreationDate();
				}

				if (IsPosting || IsCreatingUAInvoiceOrCRD)
				{
					RecalculateHeaderAmounts();
					RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
					if (AH_Ledger != LedgerTypes.IncompleteTransactions && AH_Ledger != LedgerTypes.TransactionsPendingAllocation)
					{
						RecalculateAH_JH();
					}
				}

				if (!IsInDatabase)
				{
					Factory.AddFetchHint(AccTransactionHeaderSchema.Instance, GetQueryForMatchingJournal(Constants.TransactionCategory.Codes.TransactionNotFound));
				}

				UpdateImportedXMLMatchingRules();
				SaveReasonAddOnColumn();

				CreateComplianceDocument();
				CreateJobDocAddress();

				if (this.HasContext(BusinessContext.OverrideInvoiceReference)
					&& (TransactionHeaderReferenceIRR != null && (TransactionHeaderReferenceIRR.AH1_ReferenceInfo.HasChanges || !TransactionHeaderReferenceIRR.IsInDatabase))
					&& IsInDatabase && Ledger == LedgerTypes.AccountsPayable
					&& SupportHeaderReference)
				{
					AddHeaderReferenceLog();
				}

				AddTransactionNumLog();
				AddInvoiceDateLog();
				AddChequeOrReferenceLog();
				AddDueDateLog();
				AddGovernmentAllocatedNumberLog();
			}
		}

		public bool HasJobChargeTransformerBeenRun { get; private set; }

		public bool ShouldRunJobChargeTransformer
		{
			get
			{
				return (IsCreatingAPInvoiceOrCRD || IsCreatingUAInvoiceOrCRD)
					&& (SubmittedFromInvoicingForm || IsAmendingTransaction || this.HasContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer))
					&& (!IsInDatabase || IsConvertedUAInvoiceOrCRD || IsAllocatingInvoice || IsCompletingInvoice)
					&& !(IsCreatedByENett && IsUAInvoiceOrCreditNote);
			}
		}

		TransactionLineJobChargeTransformer TransactionLineJobChargeTransformer
		{
			get
			{
				if (Factory.ServiceContainer.GetService<TransactionLineJobChargeTransformer>() == null)
				{
					Factory.ServiceContainer.AddService(new TransactionLineJobChargeTransformer(Factory));
				}

				return Factory.ServiceContainer.GetService<TransactionLineJobChargeTransformer>();
			}
		}

		//Class created to avoid code analysis issue: CA1001: Types that own disposable fields should be disposable
		class InternalSuspenders
		{
			internal IDisposable onFactorySavingIgnoreValidationSuspender;
			internal IDisposable onFactorySavingValidationSuspender;
			internal IDisposable onFactorySavingLinesValidationSuspender;
			internal IDisposable onFactorySavingJobsValidationSuspender;
		}

		InternalSuspenders onFactorySavingSuspenders;

		protected override void OnFactorySaving()
		{
			onFactorySavingSuspenders = new InternalSuspenders();
			if (onFactorySavingSuspenders.onFactorySavingIgnoreValidationSuspender == null)
			{
				var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
				onFactorySavingSuspenders.onFactorySavingIgnoreValidationSuspender = new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended);
			}
			if (onFactorySavingSuspenders.onFactorySavingValidationSuspender == null)
			{
				onFactorySavingSuspenders.onFactorySavingValidationSuspender = GetValidationSuspender();
			}
			if (onFactorySavingSuspenders.onFactorySavingLinesValidationSuspender == null)
			{
				onFactorySavingSuspenders.onFactorySavingLinesValidationSuspender = GetValidationSuspenderForLines();
			}
			if (onFactorySavingSuspenders.onFactorySavingJobsValidationSuspender == null)
			{
				onFactorySavingSuspenders.onFactorySavingJobsValidationSuspender = GetValidationSuspenderForJobs();
			}
			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (onFactorySavingSuspenders != null)
			{
				onFactorySavingSuspenders.onFactorySavingIgnoreValidationSuspender?.Dispose();
				onFactorySavingSuspenders.onFactorySavingIgnoreValidationSuspender = null;
				onFactorySavingSuspenders.onFactorySavingValidationSuspender?.Dispose();
				onFactorySavingSuspenders.onFactorySavingValidationSuspender = null;
				onFactorySavingSuspenders.onFactorySavingLinesValidationSuspender?.Dispose();
				onFactorySavingSuspenders.onFactorySavingLinesValidationSuspender = null;
				onFactorySavingSuspenders.onFactorySavingJobsValidationSuspender?.Dispose();
				onFactorySavingSuspenders.onFactorySavingJobsValidationSuspender = null;
				onFactorySavingSuspenders = null;
			}

			IsProxyingHeaderValues = false;

			base.OnFactorySaved(saveSucceeded);
			if (Factory.ServiceContainer.GetService<JobHeaderReloader>() != null)
			{
				Factory.ServiceContainer.RemoveService<JobHeaderReloader>();
			}
			if (Factory.ServiceContainer.GetService<LineTotalPaidAmountPostedCalculator>() != null)
			{
				Factory.ServiceContainer.RemoveService<LineTotalPaidAmountPostedCalculator>();
			}
			if (this.HasContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer))
			{
				this.RemoveContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);
			}
		}

		public override ZDecimal AH_InvoiceAmount
		{
			get { return base.AH_InvoiceAmount; }
			set
			{
				base.AH_InvoiceAmount = value;

				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateAH_OSTotalAmount();
					HeaderValidation.ValidateAH_LocalTotalAmount();
				}
			}
		}

		public override ZDecimal AH_GSTAmount
		{
			get { return base.AH_GSTAmount; }
			set
			{
				base.AH_GSTAmount = value;

				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateAH_OSTotalAmount();
					HeaderValidation.ValidateAH_LocalTotalAmount();
				}
			}
		}

		public override ZDecimal AH_OSTotal
		{
			get { return base.AH_OSTotal; }
			set
			{
				base.AH_OSTotal = value;

				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateAH_OSTotalAmount();
				}
			}
		}

		public override ZDecimal AH_OSExTaxAmount
		{
			get
			{
				return base.AH_OSExTaxAmount;
			}
			set
			{
				base.AH_OSExTaxAmount = value;
				RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
			}
		}

		public override ZDecimal AH_OSTaxAmount
		{
			get
			{
				return base.AH_OSTaxAmount;
			}
			set
			{
				base.AH_OSTaxAmount = value;
				RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
			}
		}

		public override ZDecimal AH_LocalExTaxAmount
		{
			get
			{
				return base.AH_LocalExTaxAmount;
			}
			set
			{
				base.AH_LocalExTaxAmount = value;
				RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed();
			}
		}

		void RecalculateExchangeRateFromAmountsOnlyWhenJobExchangeRateIsUsed()
		{
			if (AH_PostedToEFT)
			{
				RecalculateExchangeRateFromAmounts();
			}
		}

		void RecalculateExchangeRateFromAmountsOnlyWhenItAllowedToBeModified()
		{
			if (AH_PostedToEFT || IsLocalCurrencyTransaction || !ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC))
			{
				RecalculateExchangeRateFromAmounts();
			}
		}

		void RecalculateExchangeRateFromAmounts()
		{
			if (!SetExchangeRateSuspender.IsSuspended && !IsReverseTransaction && AH_RX_NKTransactionCurrency != Company.GC_RX_NKLocalCurrency)
			{
				using (GetProxyHeaderExchangeRateSuspender())
				{
					AH_ExchangeRate = Company.GetExchangeRate().GetRate(AH_LocalTotalAmount, AH_OSTotalAmount);
				}
			}
		}

		void UpdateLocalOutstandingAmount()
		{
			if (!AmountsCalculationsSuspender.IsSuspended)
			{
				AH_LocalOutstandingAmount = AH_LocalTotalAmount;
			}
		}

		public override ExchangeRateType RateType
		{
			get
			{
				AccExchangeRateConfigurationWrapper config = null;

				if (UseJobExchangeRate || UseJobExchangeRateDisregardingCheckBox)
				{
					var consumer = GetExchangeRateConfigurationRateConsumer(InvoicingJob);
					config = consumer?.GetExchangeRateConfiguration(Header, this.GetExRateLedger(), InvoiceCurrencyType, AH_RX_NKTransactionCurrency);
				}

				return config?.ExchangeRateType ?? base.RateType;
			}
		}

		void SetExchangeRate(bool setAlways = false)
		{
			if (!SetExchangeRateSuspender.IsSuspended && !IsReverseTransaction && !IsSettingAH_ExchangeRate && !Factory.HasContext(BusinessContext.IncompleteInvoiceDataAdapter))
			{
				ZDecimal? newExRate = null;

				if (IsLocalCurrencyTransaction)
				{
					newExRate = 1;
				}
				else if (ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC))
				{
					newExRate = ExchangeRateCalculator.GetOverrideExchangeRate(AH_RX_NKTransactionCurrency, IsLocalCurrencyTransaction, AH_GC, RateType, this.GetExRateLedger(), AH_InvoiceDate, AH_PostDate, InvoiceTaxDate);
				}

				if (newExRate.HasValue && AH_ExchangeRate != newExRate)
				{
					AH_ExchangeRate = newExRate.Value;
					ExchangeRate.DontSetTodaysRateOnCurrencyChange = true; //do not recalculate rate by ExchangeRate class internally
				}
				else if (setAlways)
				{
					AH_ExchangeRate = AH_ExchangeRate;
				}
			}
		}

		#region Suspend SetExchangeRate
		public FunctionalitySuspender SetExchangeRateSuspender
		{
			get { return setExchangeRateSuspender ?? (setExchangeRateSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setExchangeRateSuspender;
		#endregion

		void RecalculateHeaderAmounts()
		{
			if (HeaderAmountsRecalculationSuspendedCount == 0
				&& AH_Ledger != LedgerTypes.IncompleteTransactions
				&& AH_Ledger != LedgerTypes.TransactionsPendingAllocation
				&& !IsSourceReferenceUsed
				&& (!IsInDatabase
					|| AH_InvoiceAmountInfo.HasChanges
					|| AH_GSTAmountInfo.HasChanges
					|| AH_OSTotalInfo.HasChanges
					|| AH_IsCancelledInfo.HasChanges))
			{
				ZDecimal oldAH_InvoiceAmount = AH_InvoiceAmount;
				ZDecimal oldAH_LocalExTaxAmount = AH_LocalExTaxAmount;
				ZDecimal oldAH_LocalTaxAmount = AH_LocalTaxAmount;
				using (AmountsCalculationsSuspender.GetSuspender())
				using (ValidateAH_OSTotalAmountSuspender.GetSuspender())
				{
					UpdateHeaderAmounts();
				}

				if (oldAH_InvoiceAmount != AH_InvoiceAmount || oldAH_LocalExTaxAmount != AH_LocalExTaxAmount || oldAH_LocalTaxAmount != AH_LocalTaxAmount)
				{
					AH_OutstandingAmount -= (oldAH_LocalExTaxAmount - AH_LocalExTaxAmount + oldAH_LocalTaxAmount - AH_LocalTaxAmount) * Multiplier;
					if (AH_OutstandingAmount != 0M && !AH_FullyPaidDate.IsEmpty)
					{
						AH_FullyPaidDate = ZDateTime.Empty;
					}
				}
			}
		}

		void UpdateHeaderAmounts()
		{
			UpdateAH_LocalExTaxAmount();
			UpdateAH_LocalTaxAmount();
			UpdateAH_OSTotalAmount();
		}

		public IDisposable SuspendHeaderAmountsRecalculation()
		{
			return new HeaderAmountsRecalculationSuspender(this);
		}

		#region HeaderAmountsRecalculationSuspender

		class HeaderAmountsRecalculationSuspender : Disposable
		{
			public HeaderAmountsRecalculationSuspender(InvoicingBase parent)
			{
				this.parent = parent;
				parent.HeaderAmountsRecalculationSuspendedCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					parent.HeaderAmountsRecalculationSuspendedCount--;
					if (parent.HeaderAmountsRecalculationSuspendedCount < 0)
					{
						ErrorReporter.ReportOnce("InvoicingBase.BalanceCalculationSuspender.Dispose", "Suspend Count is below zero.");
					}
				}
			}

			readonly InvoicingBase parent;
		}

		int HeaderAmountsRecalculationSuspendedCount;

		#endregion

		public IDisposable SuspendCreditLimitCheck()
		{
			return new CreditLimitCheckSuspender(this);
		}

		public bool IsCreditLimitCheckSuspended
		{
			get { return CreditLimitCheckSuspendedCount > 0; }
		}

		internal bool IsCreditLimitCheckApplicable => !IsCreditLimitCheckSuspended && IsARInvoiceOrCreditNoteOrAdjustmentNote;

		public void SuspendCreditLimitCheckOnSaved()
		{
			IsCreditLimitCheckOnSavedSuspended = true;
		}

		bool IsCreditLimitCheckOnSavedSuspended;

		#region CreditLimitCheckSuspender

		class CreditLimitCheckSuspender : Disposable
		{
			public CreditLimitCheckSuspender(InvoicingBase parent)
			{
				this.parent = parent;
				parent.CreditLimitCheckSuspendedCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					parent.CreditLimitCheckSuspendedCount--;
					if (parent.CreditLimitCheckSuspendedCount < 0)
					{
						ErrorReporter.ReportOnce("InvoicingBase.CreditLimitCheckSuspender.Dispose", "Suspend Count is below zero.");
					}
				}
			}

			readonly InvoicingBase parent;
		}

		int CreditLimitCheckSuspendedCount;

		#endregion

		void SetJobChargesWIPAccrualCreationDate()
		{
			if (IsARAP && IsInvoiceOrCreditNote && ClearedJobCharges != null)
			{
				foreach (Charge aCharge in ClearedJobCharges)
				{
					aCharge.WIPAccrualCreationDate = AH_PostDate;
				}
			}
		}

		void SetJobRevenueRecognitionDate(InvoicingLineBase invoicingLine)
		{
			Job job = Factory.Load<Job>(invoicingLine.AL_JH);
			if (job != null)
			{
				job.ApplyRevenueRecognitionDate(invoicingLine);
			}
			else
			{
				invoicingLine.UpdateAL_ReverseDate();
			}
		}

		bool IsPosting
		{
			get { return !IsInDatabase || IsConvertedUAInvoiceOrCRD || IsAllocatingInvoice || IsCompletingInvoice; }
		}

		public bool IsPosted
		{
			get { return IsInDatabase && AH_Ledger != LedgerTypes.UnapprovedPayableTransactions && AH_Ledger != LedgerTypes.TransactionsPendingAllocation && AH_Ledger != LedgerTypes.IncompleteTransactions && !AH_LedgerInfo.HasChanges; }
		}

		public bool IsBeingCreatedPostedAllocatedApprovedOrIncomplete
		{
			get { return !IsInDatabase || IsApprovingInvoice || IsAllocatingInvoice || IsIncompleteInvoice; }
		}

		bool IsBeingCreatedOrPayablesWithLedgerChanged
		{
			get { return !IsInDatabase || IsApprovingInvoice || IsAllocatingInvoice || IsCompletingInvoice; }
		}

		public bool ShouldReopenJob
		{
			get { return (!IsInDatabase || IsApprovingInvoice || IsCompletingInvoice || IsAllocatingInvoice || IsAmendingTransaction); }
		}

		public bool IsBeingCreatedAllocatedOrCompletedAndNotConvertedFromAR
		{
			get { return !IsConvertedFromARInvoice && (!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice); }
		}

		public bool IsAllocatingInvoice
		{
			get
			{
				return IsInDatabase &&
					(ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.TransactionsPendingAllocation &&
						(AH_Ledger == LedgerTypes.AccountsPayable ||
						(AH_Ledger == LedgerTypes.AccountsReceivable && AH_TransactionType == TransactionTypes.CreditNote) ||
						AH_Ledger == LedgerTypes.UnapprovedPayableTransactions);
			}
		}

		public bool IsAwaitingApprovalFromGovt => IsARInvoiceOrCreditNoteOrAdjustmentNote &&
			!EInvoicingStatus.IsEmpty && EInvoicingStatus != EInvoicingPivotState.Succeed;

		public bool IsApprovedByGovt => IsARInvoiceOrCreditNoteOrAdjustmentNote &&
			!EInvoicingStatus.IsEmpty && EInvoicingStatus == EInvoicingPivotState.Succeed;

		public bool IsApprovingInvoice
		{
			get { return IsInDatabase && (ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.UnapprovedPayableTransactions && AH_Ledger == LedgerTypes.AccountsPayable; }
		}
		/// <summary>
		/// Currently incomplete and not transitioning to AP
		/// </summary>
		public bool IsIncompleteInvoice
		{
			get { return IsInDatabase && (ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions; }
		}
		/// <summary>
		/// Currently incomplete and transitioning to AP
		/// </summary>
		public bool IsCompletingInvoice
		{
			get { return IsInDatabase && (ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions && AH_Ledger == LedgerTypes.AccountsPayable; }
		}

		public bool IsImportedFromFile { get; set; }

		public sealed override bool NeedPlaceOfSupplyAtHeaderLevel => PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Company) && IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled;

		public sealed override bool NeedPlaceOfSupplyAtLineLevel => PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Company);

		protected abstract bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled { get; }

		void RecalculateAH_JH(bool isIncomplete = false)
		{
			if (IsAPInvoiceOrCreditNote)
			{
				PAToAPTransactionLineMonitor.GetInstance(this)?.RecordLineCount("RecalculateAH_JH", Lines.Count);

				ZGuid newAH_JH = ZGuid.Empty;
				if (Lines.Count > 0)
				{
					newAH_JH = Lines[0].AL_JH;
					foreach (InvoicingLineBase line in Lines)
					{
						if (line.AL_JH != newAH_JH)
						{
							newAH_JH = ZGuid.Empty;
							break;
						}
					}

					if (isIncomplete && !newAH_JH.Equals(ZGuid.Empty))
					{
						var jobHeader = Factory.Load<JobHeader>(newAH_JH);
						if (jobHeader == null || !jobHeader.IsInDatabase)
						{
							newAH_JH = ZGuid.Empty;
						}
					}
				}
				AH_JH = newAH_JH;
			}
		}

		internal int FactoryCountUsedToGenerateTransactionReference
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactoryCountUsedToGenerateTransactionReferenceName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactoryCountUsedToGenerateTransactionReferenceName, value);
			}
		}
		const string FactoryCountUsedToGenerateTransactionReferenceName = "FactoryCountUsedToGenerateTransactionReferenceName";

		int FactoryCountUsedToGenerateConsolidatedInvoiceRef
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactoryCountUsedToGenerateConsolidatedInvoiceRefName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactoryCountUsedToGenerateConsolidatedInvoiceRefName, value);
			}
		}
		const string FactoryCountUsedToGenerateConsolidatedInvoiceRefName = "FactoryCountUsedToGenerateConsolidatedInvoiceRefName";

		int FactoryCountUsedToGenerateTransactionNumForSelfBilling
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactoryCountUsedToGenerateTransactionNumForSelfBillingName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactoryCountUsedToGenerateTransactionNumForSelfBillingName, value);
			}
		}
		const string FactoryCountUsedToGenerateTransactionNumForSelfBillingName = "FactoryCountUsedToGenerateTransactionNumForSelfBillingName";

		bool shouldInvoiceTermOverriddenEmailBeSent;

		protected override void RunPreSaveValidationCore()
		{
			if (AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value)
			{
				using (this.DoNotValidateEmptyComplianceSubTypeSuspender.GetSuspender())
				{
					runPresavevalidationCore();
				}
			}
			else
			{
				runPresavevalidationCore();
			}

			void runPresavevalidationCore()
			{
				base.RunPreSaveValidationCore();
				ValidateINVDocTypesIfAny();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode")]
		protected override void OnSavingCore()
		{
			if (!TransactionImportJobChargeMappingProvider.Validate(Factory))
			{
				Logs.AddNew(Events.NoteAdded, "MatchingCriteria not found");
			}

			if (!IsInDatabase)
			{
				MatchWithJournal(Constants.TransactionCategory.Codes.TransactionNotFound);
				SetTransactionCategoryIfEmptyWithoutOverridingTerms();
				shouldInvoiceTermOverriddenEmailBeSent = true;
			}

			noteCleanupRequired = IsCompletingInvoice;
			attributeCleanupRequired = IsCompletingInvoice || (IsIncompleteInvoice && IsCancelled);

			if (IsCompletingInvoice)
			{
				ITransaction thisAsITransaction = this;
				thisAsITransaction.TransactionNumber = thisAsITransaction.TransactionNumber;
			}

			if (IsReceivableOrPayable &&
				(!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice || IsApprovingInvoice))
			{
				if (!IsTransactionNumSetFromNumberFountainAtTransactionHeader)
				{
					if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
					{
						UpdateComplianceSubTypeAndSequenceNumberTogether(); //set AH_TransactionReference from compliance number sequence which is kind of number fountain.
					}
				}

				if (!IsAmendingOrReversal
					&& !((IsCompletingInvoice || IsApprovingInvoice)
					&& Logs.Find(x => x.ReferenceFreeText.Contains((NoResString)"Transaction Branch Edited: From '", StringComparison.CurrentCulture)).Any()))
				{
					ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(this, InvoiceProcessingLevelIsAllowingToResetBranch.Saving);
				}

				CreateTransactionHeaderReferenceIVA();
			}

			base.OnSavingCore();

			this.QueueForComplianceReports();

			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(this);
			var amending = this as IAmending;
			if (!IsInDatabase
				&& !Factory.HasContext(BusinessContext.SavingAsIncomplete)
				&& amending != null && amending.IsAmendingTransaction
				&& !HasJobChargeTransformerBeenRun /* when amend AP INV with AP CRD, the lines have been transformed to charges already*/)
			{
				foreach (InvoicingLineBase line in Lines)
				{
					if (!taxParent.IsTaxRecoveryLine(line))
					{
						ChargeCreator.CreateChargeFromJobRelatedRevenueLine(line, this, false);
					}
				}
			}

			taxParent.RunOnSavingOperations();

			if (!Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateConsolidatedInvoiceRef))
			{
				if ((!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice) && NumberFountainForInternalRef != null && AH_Ledger != LedgerTypes.IncompleteTransactions)
				{
					AH_ConsolidatedInvoiceRef = NumberFountainForInternalRef.Generate(this);
					FactoryCountUsedToGenerateConsolidatedInvoiceRef = Factory.SaveCount;
				}
			}

			if (IsSelfBillingInvoice && (!IsInDatabase || IsCompletingInvoice))
			{
				if (AH_Ledger == LedgerTypes.AccountsPayable || Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					if (!Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateTransactionNumForSelfBilling))
					{
						AH_TransactionNum = NumberFountainForSelfBillingInvoice.Generate(this);
						FactoryCountUsedToGenerateTransactionNumForSelfBilling = Factory.SaveCount;

						if (Factory.IsEqualToCurrentSaveCount(FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader))
						{
							var message = string.Format(
(NoResString)@"Transaction Number got numbers from 2 number fountains (NumberFountain and SelfBillingInvoiceNumberFountain) in the same time. Transaction details:
{0}",
							this.GetTransactionHeaderWithLinesInfo());
							ErrorReporter.ReportOnce("TransactionNumberWasSetFrom2NumberFountains", message);
						}

						Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
						try
						{
							UpdateChargeAndRelatedCost();
						}
						finally
						{
							Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
						}
					}
				}
				else if (!IsInDatabase && AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					AH_TransactionCount = Convert.ToByte(GetNextIncompleteSelfBillingInvoiceTransactionCount());
				}
			}

			if ((AH_Ledger == LedgerTypes.AccountsPayable || Ledger == LedgerTypes.UnapprovedPayableTransactions)
				&& (this.HasContext(BusinessContext.OverrideInvoiceReference) ||
					this.HasContext(BusinessContext.OverrideTransactionAgreedPaymentMethod))
				&& (AH_TransactionNumInfo.HasChanges ||
					AH_InvoiceDateInfo.HasChanges ||
					AH_DueDateInfo.HasChanges ||
					AH_ChequeOrReferenceInfo.HasChanges))
			{
				{
					Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
					try
					{
						UpdateChargeAndRelatedCost(AH_ChequeOrReferenceInfo.HasChanges);
					}
					finally
					{
						Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
					}
				}
			}

			if (!IsReverseTransaction)
			{
				if (!IsInDatabase)
				{
					CreateJournalsForMultipleInstalments();

					SetFullyPaidDateIfNotOutstanding(false);
					if (IsTransactionInDatabaseReadOnly)
					{
						Lines.SetReadOnlyIncludingChildren(false);
					}
				}
				else
				{
					if (IsTransitioningTransaction)
					{
						SetFullyPaidDateIfNotOutstanding(true);
					}
				}
			}

			ShouldCheckCreditLimitExceeded = (AH_Ledger == LedgerTypes.AccountsReceivable && !IsInDatabase) ||
				(AH_Ledger == LedgerTypes.AccountsPayable && !(this is UAInvoice) && (!IsInDatabase || IsCompletingInvoice || IsApprovingInvoice || IsAllocatingInvoice));

			LogDescriptionChanged();

			LogTransactionHeaderExchangeRateChanged();

			LogTransactionLineExchangeRateChanged();

			if (!IsInDatabase && SetConsolidatedInvoiceRefForAmendedTransactionStrategy != null)
			{
				SetConsolidatedInvoiceRefForAmendedTransactionStrategy.SetConsolidatedInvoiceRef(this);
			}

			if (!IsInDatabase && IsCommissionable)
			{
				CreateOrQueueCommissions();
			}

			CalculateAndAddUSSalesTaxLineItemIfRequired();

			var duplicateTransactionNumberDetails = GetPreviousSameNumberTransactionDetails();
			if (duplicateTransactionNumberDetails.HasValue && duplicateTransactionNumberDetails.Value.NotificationType != CargoWise.ComponentModel.NotificationType.Error)
			{
				AH_TransactionCount = duplicateTransactionNumberDetails.Value.Category == AccountingUtils.TransactionNumberUseCategory.APTransaction
									  || duplicateTransactionNumberDetails.Value.Category == AccountingUtils.TransactionNumberUseCategory.PATransaction
									  ? Convert.ToByte(duplicateTransactionNumberDetails.Value.PreviousTransactionCount)
									  : Convert.ToByte(0);
				AH_TransactionCount++;

				foreach (StmALog log in Logs.GetAllLogs())
				{
					if (log.SL_SE_NKEvent == Events.AddedARecordToTheSystem.Code)
					{
						using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
						{
							log.ReferenceFreeText = string.Format(CultureInfo.InvariantCulture, (NoResString)"Transaction Number is already in used. Previous Invoice Date is {0:dd-MMM-yy}. Transaction posted as user has ‘Allow duplicate AP Invoice Number’ security right at point of posting.", duplicateTransactionNumberDetails.Value.PreviousInvoiceDate.Date);
						}
					}
				}
			}

			AddServiceInvoicePostedLog();

			if (SupportHeaderReference)
			{
				if (Ledger == LedgerTypes.AccountsReceivable)
				{
					if (Header != null)
					{
						Header.CompanyData.GenerateARClientNumber();
					}
					SetInvoiceTransactionReference();
					SetARInvoiceRemittance();
				}
			}

			if (!IsInDatabase && Ledger == LedgerTypes.AccountsReceivable && AH_TransactionType == TransactionTypes.Invoice)
			{
				CreateReceivableDisbursementInvoiceReference();
			}

			if (IsReversing && IsInDatabase)
			{
				Logs.AddNew(AutoEvents.TransactionReversed, CustomLogReferenceSuffix);
			}

			if (IsSourceReferenceUsed)
			{
				this.SetContext(BusinessContext.CheckTransactionLineTotalsMatchTransactionHeaderAmountsIsSuspended);
			}

			if (SupportMultiPeriodApportionment)
			{
				UpdateJournalDescriptions();
			}
		}

		#region Create Multi Period Apportionment Journals

		public void ResetMultiPeriodApportionmentJournals()
		{
			if (fMultiPeriodApportionmentJournals != null)
			{
				fMultiPeriodApportionmentJournals.ForEach(x => x.Delete());
			}
			fMultiPeriodApportionmentJournals = null;
		}

		#region Journal Descriptions

		public void UpdateJournalDescriptions(object sender = null, EventArgs e = null)
		{
			if (fMultiPeriodApportionmentJournals == null)
			{
				return;
			}

			foreach (var journal in fMultiPeriodApportionmentJournals)
			{
				journal.UpdatePeriodApportionmentJournalHeaderDescription();

				foreach (GLJournalLine journalLine in journal.GLJournalLines)
				{
					journalLine.UpdatePeriodApportionmentJournalLineDescription();
				}
			}
		}

		#endregion

		public List<GLJournal> MultiPeriodApportionmentJournals
		{
			get
			{
				if (fMultiPeriodApportionmentJournals == null)
				{
					fMultiPeriodApportionmentJournals = new List<GLJournal>();
					if (SupportMultiPeriodApportionment)
					{
						foreach (InvoicingLineBase invoiceLine in Lines)
						{
							if (!invoiceLine.PeriodApportionmentMethod.IsEmpty && invoiceLine.PeriodApportionmentMethod != PeriodApportionmentMethods.Codes.Default)
							{
								invoiceLine.PeriodApportionment.Recalculate();

								var isAmountNegativeInDB = invoiceLine.AL_OSAmount < 0;
								AccGLHeader crLineAccount, drLineAccount;

								//create the master journal first
								crLineAccount = isAmountNegativeInDB ? invoiceLine.GLHeader : invoiceLine.PeriodClearingGLAccount;
								drLineAccount = isAmountNegativeInDB ? invoiceLine.PeriodClearingGLAccount : invoiceLine.GLHeader;
								var journal = CreatePeriodApportionmentGLJournal(invoiceLine.AL_GB, invoiceLine.AL_GE, PeriodCalculator.GetPeriodFromDate(AH_PostDate), invoiceLine.AL_RX_NKTransactionCurrency, invoiceLine.AL_ExchangeRate,
									invoiceLine.AL_OSExTaxAmount + invoiceLine.AL_OSTaxAmount_NotRecoverable, invoiceLine.AL_LocalExTaxAmount + invoiceLine.AL_LocalTaxAmount_NotRecoverable, crLineAccount, drLineAccount, invoiceLine.GenericChargeBizO, isMasterJournal: true);

								CopyToGLJournalFromInvoiceLine(journal, invoiceLine);
								fMultiPeriodApportionmentJournals.Add(journal);

								//create a list of sub journals
								var osAmountClearedToDate = 0m;
								var localAmountClearedToDate = 0m;
								var osNonRecoverableTaxAmountClearedToDate = 0m;
								var localNonRecoverableTaxAmountClearedToDate = 0m;
								var osAmountYetToBeCleared = Math.Abs(invoiceLine.AL_OSExTaxAmount) + Math.Abs(invoiceLine.AL_OSTaxAmount_NotRecoverable);
								var localAmountYetToBeCleared = Math.Abs(invoiceLine.AL_LocalExTaxAmount) + Math.Abs(invoiceLine.AL_LocalTaxAmount_NotRecoverable);
								var osNonRecoverableTaxAmountYetToBeCleared = Math.Abs(invoiceLine.AL_OSTaxAmount_NotRecoverable);
								var localNonRecoverableTaxAmountYetToBeCleared = Math.Abs(invoiceLine.AL_LocalTaxAmount_NotRecoverable);
								foreach (PeriodApportionmentLine apportionmentLine in invoiceLine.PeriodApportionment.Lines)
								{
									var totLineOSAmount = Math.Abs(apportionmentLine.OSAmount) + Math.Abs(apportionmentLine.OSTaxNotRecoverable);
									var totLineLocalAmount = Math.Abs(apportionmentLine.LocalAmount) + Math.Abs(apportionmentLine.LocalTaxNotRecoverable);
									osAmountClearedToDate += totLineOSAmount;
									localAmountClearedToDate += totLineLocalAmount;
									osAmountYetToBeCleared -= totLineOSAmount;
									localAmountYetToBeCleared -= totLineLocalAmount;

									osNonRecoverableTaxAmountClearedToDate += Math.Abs(apportionmentLine.OSTaxNotRecoverable);
									localNonRecoverableTaxAmountClearedToDate += Math.Abs(apportionmentLine.LocalTaxNotRecoverable);
									osNonRecoverableTaxAmountYetToBeCleared -= Math.Abs(apportionmentLine.OSTaxNotRecoverable);
									localNonRecoverableTaxAmountYetToBeCleared -= Math.Abs(apportionmentLine.LocalTaxNotRecoverable);

									crLineAccount = isAmountNegativeInDB ? invoiceLine.PeriodClearingGLAccount : invoiceLine.GLHeader;
									drLineAccount = isAmountNegativeInDB ? invoiceLine.GLHeader : invoiceLine.PeriodClearingGLAccount;
									journal = CreatePeriodApportionmentGLJournal(invoiceLine.AL_GB, invoiceLine.AL_GE, apportionmentLine.Period, invoiceLine.AL_RX_NKTransactionCurrency, apportionmentLine.ExchangeRate,
										totLineOSAmount, totLineLocalAmount, crLineAccount, drLineAccount, invoiceLine.GenericChargeBizO,
										true, osAmountClearedToDate, localAmountClearedToDate, osAmountYetToBeCleared, localAmountYetToBeCleared, invoiceLine.AL_OSTaxAmount_NotRecoverable != 0,
										osNonRecoverableTaxAmountClearedToDate, localNonRecoverableTaxAmountClearedToDate, osNonRecoverableTaxAmountYetToBeCleared, localNonRecoverableTaxAmountYetToBeCleared);
									CopyToGLJournalFromInvoiceLine(journal, invoiceLine);
									fMultiPeriodApportionmentJournals.Add(journal);
								}
							}
						}
					}
				}
				return fMultiPeriodApportionmentJournals;
			}
		}
		List<GLJournal> fMultiPeriodApportionmentJournals;

		GLJournal CreatePeriodApportionmentGLJournal(ZGuid branchPK, ZGuid departmentPK, ZInt postPeriod, ZString currency, ZDecimal exchangeRate, ZDecimal osTotalAmount, ZDecimal localTotalAmount,
												AccGLHeader crLineAccount, AccGLHeader drLineAccount, AccGenericCharge genericChargeBizO,
												bool createLineDescription = false, decimal osAmountClearedToDate = 0m, decimal localAmountClearedToDate = 0m, decimal osAmountYetToBeCleared = 0m, decimal localAmountYetToBeCleared = 0m,
												bool isPresentTaxAmountNotRecoverable = false, decimal osNonRecoverableTaxAmountClearedToDate = 0m, decimal localNonRecoverableTaxAmountClearedToDate = 0m,
												decimal osNonRecoverableTaxAmountYetToBeCleared = 0m, decimal localNonRecoverableTaxAmountYetToBeCleared = 0m, bool isMasterJournal = false)
		{
			var journal = Factory.New<GLJournal>();
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.PostPeriod = postPeriod;
			journal.AH_GB = branchPK;
			journal.AH_GE = departmentPK;
			journal.AH_GC = AH_GC;
			journal.AH_InvoiceDate = AH_InvoiceDate;
			journal.AH_TransactionBelongsToGroup = PK;
			journal.IsMasterJournal = isMasterJournal;

			journal.PeriodApportionmentJournalDescBuilder.InvoicingBase = this;
			journal.PeriodApportionmentJournalDescBuilder.PostPeriod = postPeriod;

			CreatePeriodApportionmentGLJournalLine(journal, DebitCredit.CR, crLineAccount, postPeriod, currency, exchangeRate, osTotalAmount, localTotalAmount, genericChargeBizO,
													createLineDescription, osAmountClearedToDate, localAmountClearedToDate, osAmountYetToBeCleared, localAmountYetToBeCleared,
													isPresentTaxAmountNotRecoverable, osNonRecoverableTaxAmountClearedToDate, localNonRecoverableTaxAmountClearedToDate, osNonRecoverableTaxAmountYetToBeCleared, localNonRecoverableTaxAmountYetToBeCleared);
			CreatePeriodApportionmentGLJournalLine(journal, DebitCredit.DR, drLineAccount, postPeriod, currency, exchangeRate, osTotalAmount, localTotalAmount, genericChargeBizO,
													createLineDescription, osAmountClearedToDate, localAmountClearedToDate, osAmountYetToBeCleared, localAmountYetToBeCleared,
													isPresentTaxAmountNotRecoverable, osNonRecoverableTaxAmountClearedToDate, localNonRecoverableTaxAmountClearedToDate, osNonRecoverableTaxAmountYetToBeCleared, localNonRecoverableTaxAmountYetToBeCleared);
			return journal;
		}

		void CreatePeriodApportionmentGLJournalLine(GLJournal journal, DebitCredit debitOrCredit, AccGLHeader lineAccount,
												ZInt postPeriod, ZString currency, ZDecimal exchangeRate, ZDecimal osTotalAmount, ZDecimal localTotalAmount, AccGenericCharge genericChargeBizO,
												bool createLineDescription = false, decimal osAmountClearedToDate = 0m, decimal localAmountClearedToDate = 0m, decimal osAmountYetToBeCleared = 0m, decimal localAmountYetToBeCleared = 0m,
												bool isPresentTaxAmountNotRecoverable = false, decimal osNonRecoverableTaxAmountClearedToDate = 0m, decimal localNonRecoverableTaxAmountClearedToDate = 0m, decimal osNonRecoverableTaxAmountYetToBeCleared = 0m, decimal localNonRecoverableTaxAmountYetToBeCleared = 0m)
		{
			var journalLine = journal.GLJournalLines.AddNew();

			journalLine.IsMasterJournalLine = journal.IsMasterJournal;

			journalLine.PeriodApportionmentJournalDescBuilder.InvoicingBase = this;
			journalLine.PeriodApportionmentJournalDescBuilder.LineAccount = lineAccount;
			journalLine.PeriodApportionmentJournalDescBuilder.Charge = genericChargeBizO;
			journalLine.PeriodApportionmentJournalDescBuilder.Currency = currency;
			journalLine.PeriodApportionmentJournalDescBuilder.RefCurrencyCode = GlbCompany.CurrentCompany.LocalCurrency.Code;
			journalLine.PeriodApportionmentJournalDescBuilder.PostPeriod = postPeriod;
			journalLine.PeriodApportionmentJournalDescBuilder.OSAmountClearedToDate = osAmountClearedToDate;
			journalLine.PeriodApportionmentJournalDescBuilder.OSAmountYetToBeCleared = osAmountYetToBeCleared;
			journalLine.PeriodApportionmentJournalDescBuilder.LocalAmountClearedToDate = localAmountClearedToDate;
			journalLine.PeriodApportionmentJournalDescBuilder.LocalAmountYetToBeCleared = localAmountYetToBeCleared;
			journalLine.PeriodApportionmentJournalDescBuilder.PresentTaxAmountNotRecoverable = isPresentTaxAmountNotRecoverable;
			journalLine.PeriodApportionmentJournalDescBuilder.OSNonRecoverableTaxAmountClearedToDate = osNonRecoverableTaxAmountClearedToDate;
			journalLine.PeriodApportionmentJournalDescBuilder.OSNonRecoverableTaxAmountYetToBeCleared = osNonRecoverableTaxAmountYetToBeCleared;
			journalLine.PeriodApportionmentJournalDescBuilder.LocalNonRecoverableTaxAmountClearedToDate = localNonRecoverableTaxAmountClearedToDate;
			journalLine.PeriodApportionmentJournalDescBuilder.LocalNonRecoverableTaxAmountYetToBeCleared = localNonRecoverableTaxAmountYetToBeCleared;

			try
			{
				((AccountingSuspenders.IRunMethodSuspending)journalLine).RunMethodSuspended = true;
				journalLine.AL_RX_NKTransactionCurrency = currency;
				journalLine.AL_ExchangeRate = exchangeRate;
				journalLine.DebitCreditSign = debitOrCredit.ToString();
				journalLine.UnsignedOSLineAmount = osTotalAmount;
				journalLine.UnsignedLocalLineAmount = localTotalAmount;
				journalLine.AL_AG = lineAccount.PK;
				journalLine.AL_GB = journal.AH_GB;
				journalLine.AL_GE = journal.AH_GE;
			}
			finally
			{
				((AccountingSuspenders.IRunMethodSuspending)journalLine).RunMethodSuspended = false;
			}
		}

		void CopyToGLJournalFromInvoiceLine(GLJournal journal, InvoicingLineBase invoiceLine)
		{
			SubAccountHelper.CopySubAccounts(journal.GLJournalLines.Cast<GLJournalLine>().FirstOrDefault(x => x.AL_AG == invoiceLine.GLHeader.PK), invoiceLine);
		}

		#endregion

		void AddServiceInvoicePostedLog()
		{
			if (!IsInDatabase && AH_Ledger == LedgerTypes.AccountsReceivable && IsInvoiceOrCreditNote)
			{
				if (IsPeriodicInvoice)
				{
					var jobs = (from InvoicingLineBase line in Lines where line.Job != null select line.Job).Distinct().ToArray();
					foreach (var job in jobs)
					{
						((Job)job).Logs.AddNew(Events.ServiceInvoicePosted, job.JH_JobNum);
					}
				}
				else if (Job != null)
				{
					var reference = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", AH_TransactionCategory, AH_TransactionType, Job.JH_JobNum);
					Job.Logs.AddNew(Events.ServiceInvoicePosted, reference);
				}
				else if (IsConsolInvoice && Consol != null)
				{
					var reference = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", AH_TransactionCategory, AH_TransactionType, Consol.JK_UniqueConsignRef);
					Consol.AddNewToLogs(Events.ServiceInvoicePosted, reference);
				}
			}
		}

		void UpdateChargeAndRelatedCost(bool isOverrideCostReference = false)
		{
			var linePKs = Lines.GetPKs();
			var processedCostPKs = new HashSet<ZGuid>();

			foreach (var partialChargePKs in AccountingUtils.ChunksOf(linePKs, BatchSize))
			{
				var chargeQuery = new ZQuery();
				chargeQuery.AddToFilter(JobChargeSchema.JR_AL_APLine, partialChargePKs.ToArray());
				var chargesToUpdate = Factory.Load<JobCharge>(chargeQuery);

				var costPKs = chargesToUpdate.Where(c => c.JR_E6.IsValid && !processedCostPKs.Contains(c.JR_E6)).Select(p => p.JR_E6).Distinct();

				if (costPKs.Any())
				{
					var costQuery = new ZQuery();
					costQuery.AddToFilter(JobConsolCostSchema.PK, costPKs.ToArray());
					var costs = Factory.Load<JobConsolCost>(costQuery);

					foreach (var cost in costs)
					{
						using (cost.GetValidationSuspender())
						{
							cost.E6_InvoiceNum = AH_TransactionNum;
							cost.E6_InvoiceDate = AH_InvoiceDate;
							cost.E6_DocumentReceivedDate = AH_DocumentReceivedDate;
							cost.E6_PaymentDate = AH_DueDate;

							if (isOverrideCostReference)
							{
								cost.E6_CostReference = AH_ChequeOrReference;
							}
						}

						processedCostPKs.Add(cost.PK);
					}
				}

				foreach (var charge in chargesToUpdate)
				{
					using (charge.GetValidationSuspender())
					{
						charge.JR_APInvoiceNum = AH_TransactionNum;
						charge.JR_APInvoiceDate = AH_InvoiceDate;
						charge.JR_APDocumentReceivedDate = AH_DocumentReceivedDate;
						charge.JR_PaymentDate = AH_DueDate;

						if (isOverrideCostReference)
						{
							charge.JR_CostReference = AH_ChequeOrReference;
						}
					}
				}
			}
		}

		void SetTransactionCategoryIfEmptyWithoutOverridingTerms()
		{
			if (AH_Ledger == LedgerTypes.AccountsReceivable && AH_TransactionCategory.IsEmpty && !IsJobRelated)
			{
				base.AH_TransactionCategory = GetTransactionCategoryForMiscInvoice(IsDisbursementOrFinal, IsLocalCurrencyTransaction);
			}
		}

		void SetFullyPaidDateIfNotOutstanding(bool createCashVATAlways)
		{
			if (AH_OutstandingAmount == 0m && AH_Ledger != LedgerTypes.IncompleteTransactions && AH_Ledger != LedgerTypes.TransactionsPendingAllocation)
			{
				Func<AccCashBasisVAT[]> createCashVAT = () => CashBasisVATManager.CreateRecordsForTransactionFullyPaidWithoutMatching(this);
				if (AH_FullyPaidDate.IsEmpty)
				{
					AH_FullyPaidDate = AH_InvoiceDate;
					isAH_FullyPaidDateSetForZeroValueInvoice = true;
				}

				if ((isAH_FullyPaidDateSetForZeroValueInvoice || createCashVATAlways) && cashBasisVATRecordsForZeroValueInvoice.Count == 0)
				{
					cashBasisVATRecordsForZeroValueInvoice.AddRange(createCashVAT());
				}

				ObjectFactory.Get<ITaxProcessor>().ProcessTaxesOnMatching(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(this), AH_PostDate.Date);
			}
		}

		void SetAH_AgreedPaymentMethodOverride()
		{
			if (!IsAmendingOrReversal && !OriginalTransactionReference.IsValid &&
				(!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice || IsApprovingInvoice) &&
				Header != null && Header.CompanyData != null)
			{
				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var arTerms = Header.CompanyData.GetBestMatchedARterms(JobType, Direction, TransportMode, AH_GB, AH_GE, AH_TransactionCategory);
					if (arTerms != null && !string.IsNullOrWhiteSpace(arTerms.PY_AgreedPaymentMethod))
					{
						AH_AgreedPaymentMethodOverride = arTerms.PY_AgreedPaymentMethod;
					}

					if (string.IsNullOrWhiteSpace(AH_AgreedPaymentMethodOverride))
					{
						AH_AgreedPaymentMethodOverride = Header.CompanyData.OB_ARCreditAgreedPaymentMethod;
					}
				}
				else
				{
					AH_AgreedPaymentMethodOverride = Header.CompanyData.OB_APCreditAgreedPaymentMethod;
				}
			}
		}

		readonly List<AccCashBasisVAT> cashBasisVATRecordsForZeroValueInvoice = new List<AccCashBasisVAT>();
		bool isAH_FullyPaidDateSetForZeroValueInvoice;

		internal bool IsTransitioningTransaction
		{
			get
			{
				return ((ZString)AH_LedgerInfo.OriginalValue) != AH_Ledger && IsReceivableOrPayable ||
					((ZString)AH_TransactionTypeInfo.OriginalValue) != AH_TransactionType && IsInvoiceOrCreditNoteOrAdjustmentNote;
			}
		}

		protected bool IsReceivableOrPayable => AH_Ledger == LedgerTypes.AccountsReceivable || AH_Ledger == LedgerTypes.AccountsPayable;

		void LogDescriptionChanged()
		{
			if (IsReceivableOrPayable && (ZString)AH_DescInfo.OriginalValue != AH_Desc)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format("Description Edited. New Description: '{0}'", AH_Desc));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void LogTransactionHeaderExchangeRateChanged()
		{
			if (AH_OverrideExchangeRate && !UseJobExchangeRate)
			{
				var exRateFromRegistrySetting = IsLocalCurrencyTransaction ? 1 : ExchangeRateCalculator.GetOverrideExchangeRate(AH_RX_NKTransactionCurrency, IsLocalCurrencyTransaction, AH_GC, RateType, this.GetExRateLedger(), AH_InvoiceDate, AH_PostDate, InvoiceTaxDate);
				var rateDecimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				if (AH_Ledger == LedgerTypes.AccountsPayable && AH_ExchangeRate != exRateFromRegistrySetting)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Transaction Exchange Rate Updated From: '{0}' to '{1}'", exRateFromRegistrySetting.ToString(rateDecimals), AH_ExchangeRate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		void LogTransactionLineExchangeRateChanged()
		{
			if ((IsLocalCurrencyTransaction || UseJobExchangeRate) && AH_OverrideExchangeRate)
			{
				foreach (InvoicingLineBase line in Lines)
				{
					if (IsLocalCurrencyTransaction && line.AL_RX_NKTransactionCurrency == AH_RX_NKTransactionCurrency)
					{
						continue;
					}
					if (TransactionLinesDefaultExRates.TryGetValue(line.PK, out var value) && value != line.AL_ExchangeRate)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						Logs.AddNew(Events.EditedARecord, string.Format("Line Exchange Rates Updated"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						break;
					}
				}
			}
		}

		ZBool ShouldCheckCreditLimitExceeded;

		void CreateJournalsForMultipleInstalments()
		{
			if (!IsARInvoiceOrCreditNoteOrAdjustmentNote || AH_InvoiceTerm != Constants.InvoiceTerms.FromInvoiceDate)
			{
				return;
			}
			var arTermMLI = Header.CompanyData.GetBestMatchedARterms(JobType, Direction, TransportMode, AH_GB, AH_GE, AH_TransactionCategory, AH_InvoiceTerm);
			if (arTermMLI == null || !arTermMLI.InvoiceTermIsMultipleInstallments)
			{
				return;
			}

			//strictly in this order, else DueDate is not set correctly
			AH_InvoiceTerm = Constants.InvoiceTerms.MultipleInstallments;
			AH_DueDate = new ZDateTime(Math.Max(AH_InvoiceDate.Ticks, AH_PostDate.Ticks));

			var debitCredit = AH_OSTotal > 0 ? "CR" : "DR";
			var complianceNumDesc = AH_TransactionReference.IsEmpty ? string.Empty : $" / {AH_TransactionReference}";

			var journal = Factory.New<ARJournal>();
			journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClearingJournal;
			journal.AH_TransactionBelongsToGroup = PK;
			journal.AH_ChequeOrReference = AH_TransactionNum;
			journal.AH_OH = AH_OH;
			journal.AH_InvoiceDate = AH_InvoiceDate;
			journal.AH_PostDate = AH_PostDate;
			journal.AH_DueDate = AH_DueDate;
			journal.AH_AgreedPaymentMethodOverride = AH_AgreedPaymentMethodOverride;
			journal.AH_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
			journal.DebitCreditSign = journal.GetOppositeDebitCreditSign(debitCredit);
			//journal.AH_ExchangeRate = AH_ExchangeRate; SUBSTITUTED with LocalAmount set, to keep consistency with original Invoice
			decimal osTotal, localTotal;
			journal.AH_OSExTaxAmount = osTotal = Math.Abs(AH_OSTotalAmount);
			journal.AH_LocalExTaxAmount = localTotal = Math.Abs(AH_LocalTotalAmount);
			journal.AH_GC = AH_GC;
			journal.AH_GB = AH_GB;
			journal.AH_GE = AH_GE;
			journal.AH_Desc = Res.GetString("AB5B3F30-2B3E-457E-8E88-1B99458579C3", "Multi-installment clearing Journal for {0} {1} {2}{3} - {4}",
				AH_Ledger, AH_TransactionType, AH_TransactionNum, complianceNumDesc, AH_Desc);

			var arTermInstallsOrdered = arTermMLI.ARTermsInstallments.OrderBy(x => x.ML_SequenceNumber);
			var installsCount = arTermInstallsOrdered.Count();
			var roundingScale = TransactionCurrency?.Decimals ?? GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			ZDecimal subtotAmount = 0, subtotLocalAmount = 0;
			foreach (var termInst in arTermInstallsOrdered)
			{
				var instalment = Factory.New<ARJournal>();
				instalment.AH_TransactionCategory = Constants.TransactionCategory.Codes.InstalmentJournal;
				instalment.AH_TransactionBelongsToGroup = PK;
				instalment.AH_OH = AH_OH;
				instalment.AH_InvoiceDate = AH_InvoiceDate;
				instalment.AH_PostDate = AH_PostDate;
				instalment.AH_DueDate = AH_InvoiceDate.AddDays(termInst.ML_DaysFromInvoiceDate);
				instalment.AH_AgreedPaymentMethodOverride = termInst.ML_AgreedPaymentMethod;
				instalment.AH_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				instalment.DebitCreditSign = debitCredit;
				//instalment.AH_ExchangeRate = AH_ExchangeRate; SUBSTITUTED with LocalAmount set, to keep consistency with original Invoice
				if (termInst.ML_SequenceNumber < installsCount)
				{
					subtotAmount += instalment.AH_OSExTaxAmount = Utilities.Round(osTotal * termInst.ML_SplitPercentage / 100, roundingScale);
					subtotLocalAmount += instalment.AH_LocalExTaxAmount = Utilities.Round(localTotal * termInst.ML_SplitPercentage / 100, roundingScale);
				}
				else
				{
					instalment.AH_OSExTaxAmount = osTotal - subtotAmount;
					instalment.AH_LocalExTaxAmount = localTotal - subtotLocalAmount;
				}
				instalment.AH_GC = AH_GC;
				instalment.AH_GB = AH_GB;
				instalment.AH_GE = AH_GE;
				instalment.AH_Desc = Res.GetString("A3A92420-1C14-4444-BAEA-3F1B419DAD8D", "Instalment #{0} for {1} {2} {3}{4} - {5}",
					termInst.ML_SequenceNumber, AH_Ledger, AH_TransactionType, AH_TransactionNum, complianceNumDesc, AH_Desc);
			}

			MatchWithJournal(Constants.TransactionCategory.Codes.ClearingJournal);
		}

		void MatchWithJournal(string category)
		{
			var journal = GetJournalForMatching(category);
			if (journal != null)
			{
				var matching = AH_Ledger == LedgerTypes.AccountsPayable ? new APMatchingBase(Factory) : (MatchingBase)new ARMatchingBase(Factory);
				matching.PrimaryOrganization = AH_OH;
				var transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();

				if (Math.Abs(AH_OSTotal) > Math.Abs(journal.AH_OSTotal))
				{
					transactionsToMatch.Add(this, -journal.AH_OSTotal);
					transactionsToMatch.Add(journal, journal.AH_OSTotal);
				}
				else if (Math.Abs(AH_OSTotal) < Math.Abs(journal.AH_OSTotal))
				{
					transactionsToMatch.Add(this, AH_OSTotal);
					transactionsToMatch.Add(journal, -AH_OSTotal);
				}
				else
				{
					transactionsToMatch.Add(this, AH_OSTotal);
					transactionsToMatch.Add(journal, journal.AH_OSTotal);
				}

				matching.MoveFromUnmatchToMatch(transactionsToMatch);
				if (category == Constants.TransactionCategory.Codes.ClearingJournal)
				{
					matching.MatchDate = AH_PostDate;
				}

				TransactionHeader exchangeDifference = null;
				if (matching.Balance != 0 && ((IMatching)this).OSPartialPaymentAmount + ((IMatching)journal).OSPartialPaymentAmount == 0)
				{
					exchangeDifference = matching.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
					matching.AddMiscellaneousTransaction(exchangeDifference);
				}

				var matchingResult = false;
				if (matching.Balance == 0m && !matching.HasErrors)
				{
					matching.DoNotSaveFactoryOnMatching = true;
					matchingResult = matching.MatchAndClearTransactions();
				}
				if (exchangeDifference != null && !matchingResult)
				{
					matching.DeleteMiscTransaction(exchangeDifference);
				}
			}
		}

		Journal.Journal GetJournalForMatching(string category)
		{
			if (AH_Ledger == LedgerTypes.AccountsPayable)
			{
				return Factory.LoadTop1<APJournal>(GetQueryForMatchingJournal(category));
			}
			if (AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				return Factory.LoadTop1<ARJournal>(GetQueryForMatchingJournal(category));
			}
			return null;
		}

		ZQuery GetQueryForMatchingJournal(string category)
		{
			var referenceFilter = new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, AH_TransactionNum);
			referenceFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ChequeOrReference, AH_ConsolidatedInvoiceRef);
			referenceFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ChequeOrReference, AH_ChequeOrReference);

			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, AH_Ledger);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, AH_OH);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, AH_RX_NKTransactionCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, category);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);
			if (AH_OSTotal > 0)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.LessThan, 0);
			}
			else
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThan, 0);
			}
			query.AddToFilter(referenceFilter);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);

			return query;
		}

		public ARJournal[] GetMultipleInstallmentsJournals(string category = null)
		{
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, PK);
			if (category != null)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, category);
			}
			else
			{
				ZQuery categoryFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.ClearingJournal);
				categoryFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.InstalmentJournal);
				query.AddToFilter(categoryFilter);
			}
			var journals = Factory.Load<ARJournal>(query);
			return journals;
		}

		public int GetNextIncompleteSelfBillingInvoiceTransactionCount()
		{
			var transactionCount = new DynamicBusinessObjectCollection(Factory);
			var newValueColumn = "NewValue";
			transactionCount.Load($@"
SELECT TOP 1
	CONVERT(int, {newValueColumn}) AS {newValueColumn}
FROM
	(SELECT
		{AccTransactionHeaderSchema.Constants.AH_TransactionCount},
		ROW_NUMBER() OVER (ORDER BY {AccTransactionHeaderSchema.Constants.AH_TransactionCount}) AS {newValueColumn}
	FROM
		{AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
	WHERE
		{AccTransactionHeaderSchema.Constants.AH_GC} = @Company
		AND {AccTransactionHeaderSchema.Constants.AH_Ledger} = @Ledger
		AND {AccTransactionHeaderSchema.Constants.AH_OH} = @Creditor
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = @TransactionType
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionNum} = @TransactionNum
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionCategory} = @TransactionCategory

	UNION ALL

	SELECT
		null,
		ISNULL(MAX({AccTransactionHeaderSchema.Constants.AH_TransactionCount}), 0) + 1
	FROM
		{AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName}
	WHERE
		{AccTransactionHeaderSchema.Constants.AH_GC} = @Company
		AND {AccTransactionHeaderSchema.Constants.AH_Ledger} = @Ledger
		AND {AccTransactionHeaderSchema.Constants.AH_OH} = @Creditor
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = @TransactionType
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionNum} = @TransactionNum
		AND {AccTransactionHeaderSchema.Constants.AH_TransactionCategory} = @TransactionCategory
	) AS SubQuery
WHERE
	{AccTransactionHeaderSchema.Constants.AH_TransactionCount} IS NULL OR {AccTransactionHeaderSchema.Constants.AH_TransactionCount} != {newValueColumn}
ORDER BY
	{newValueColumn}",
			new[]
			{
				ZSqlParameter.New("@Company", AH_GC, AccTransactionHeaderSchema.AH_GC),
				ZSqlParameter.New("@Ledger", LedgerTypes.IncompleteTransactions, AccTransactionHeaderSchema.AH_Ledger),
				ZSqlParameter.New("@Creditor", AH_OH, AccTransactionHeaderSchema.AH_OH),
				ZSqlParameter.New("@TransactionType", AccountingUtils.ConvertTransactionTypeFromAPToINSafe(AH_TransactionType), AccTransactionHeaderSchema.AH_TransactionType),
				ZSqlParameter.New("@TransactionNum", AH_TransactionNum, AccTransactionHeaderSchema.AH_TransactionNum), //even if it will be always empty, without this parameter we can't guaranty (in case of any bugs) that query returns us NewValue less then int. Because ROW_NUMBER() is bigint, we then need to convert it to int and be sure we don't have overflow.
				ZSqlParameter.New("@TransactionCategory", AH_TransactionCategory, AccTransactionHeaderSchema.AH_TransactionCategory),
			});

			return (ZInt)transactionCount[0][newValueColumn];
		}

		protected AccountingNumberFountainWrapper NumberFountainForSelfBillingInvoice
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.SelfBillingInvoiceNo; }
		}

		internal ZBool HasZeroLocalButNonZeroOSTaxAmount
		{
			get
			{
				var isHeaderTaxAmountIncorrect = AH_LocalTaxAmount.IsEmpty && !AH_OSTaxAmount.IsEmpty;
				var isLinesTaxAmountIncorrect = Lines.OfType<InvoicingLineBase>().Any(x => x.AL_LocalTaxAmount.IsEmpty && !x.AL_OSTaxAmount.IsEmpty);
				return isHeaderTaxAmountIncorrect || isLinesTaxAmountIncorrect;
			}
		}

		void CheckNonZeroOSTaxAmountAndZeroLocalTaxAmount()
		{
			var isARInvoiceOrCreditNote = (Ledger == LedgerTypes.AccountsReceivable) && (this is ARInvoice || this is ARCreditNote);

			if (Factory.HasContext(BusinessContext.PostingReceivableChargesForTaxCalculation) && isARInvoiceOrCreditNote)
			{
				if (HasZeroLocalButNonZeroOSTaxAmount)
				{
					var stringBuilder = new ZStringBuilder();

					stringBuilder.AppendLine((NoResString)"OS Tax Amount is NOT Zero while Local Tax Amount is Zero");
					stringBuilder.AppendLine(FormattableString.Invariant($"UseLocalExTaxAmountToCalculateLocalTax: {TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(Company.PK)}"));
					stringBuilder.AppendLine(FormattableString.Invariant($"ShouldFixTaxAmountOnHeader: {ShouldFixTaxAmountOnHeader}"));
					stringBuilder.AppendLine(FormattableString.Invariant($"HasWarehousePeriodicBillingJob: {HasWarehousePeriodicBillingJob}"));
					stringBuilder.AppendLine((NoResString)"Invoice tax amount details before adjustment");
					stringBuilder.AppendLine(CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(PK, CriticalValidationInfoCollectorServiceKeyType.OSTaxAmountIsNotZeroWhileLocalTaxAmoutIsZero));
					stringBuilder.AppendLine((NoResString)"Invoice tax amount details after adjustment");
					stringBuilder.AppendLine(this.GetTransactionHeaderTaxAmountWithLinesTaxAmountInfo());
					stringBuilder.AppendLine((NoResString)"Invoice details");
					stringBuilder.AppendLine(this.GetTransactionHeaderWithLinesInfo());
					ErrorReporter.ReportOnce("OS Tax Amount is NOT Zero while Local Tax Amount is Zero", stringBuilder.ToString());
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (!Factory.HasContext(BusinessContext.IncompleteInvoiceSaving))
				{
					ReleaseAllMutexOnInvoice();
				}

				if (ShouldCheckCreditLimitExceeded)
				{
					CheckCreditLimitExceeded();
				}
				if (IsComplianceSequenceFailedToAssign &&
					!AccountingConfigurationRegistry.Instance.SuppressWarningWhenThereIsNoComplianceBookSetups.Value)
				{
					RaiseOnComplianceSequenceFailedToAssign(this, EventArgsForCompliance);
					IsComplianceSequenceFailedToAssign = false;
				}

				if (IsDigitialSignatureFailedDueToPreviousInvoiceNotFound)
				{
					RaiseOnDigitalSignatureFailedToSign(this, new UserMessageEventArgs(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToPreviousInvoiceNotFound));
					IsDigitialSignatureFailedDueToPreviousInvoiceNotFound = false;
				}
				else if (IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice)
				{
					RaiseOnDigitalSignatureFailedToSign(this, new UserMessageEventArgs(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice));
					IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice = false;
				}
				else if (IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime)
				{
					RaiseOnDigitalSignatureFailedToSign(this, new UserMessageEventArgs(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToNotBeingAbleToGetInvoiceCreatedLogTime));
					IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime = false;
				}

				if (shouldInvoiceTermOverriddenEmailBeSent)
				{
					CheckInvoiceTerms();
					shouldInvoiceTermOverriddenEmailBeSent = false;
				}

				if (noteCleanupRequired || attributeCleanupRequired)
				{
					var factoryForIncompleteData = new BusinessObjectFactory();
					if (noteCleanupRequired)
					{
						DeleteIncompleteData(factoryForIncompleteData);
					}
					if (attributeCleanupRequired)
					{
						DeleteINVAttributeOfChargesAndCostsImportedToIncompleteInvoice(factoryForIncompleteData);
					}
					factoryForIncompleteData.Save();
				}

				CheckNonZeroOSTaxAmountAndZeroLocalTaxAmount();

				if (HasNegativeComplianceLinesWhenCreating)
				{
					RaiseOnNegativeComplianceSequenceFailedToCreate(this);
				}

				SubmitUSSalesTaxIfRequired();
			}
			else
			{
				if (!IsInDatabase && Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateTransactionReference))
				{
					AH_TransactionReference = ZString.Empty;
				}

				if (!IsInDatabase && Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateConsolidatedInvoiceRef))
				{
					AH_ConsolidatedInvoiceRef = ZString.Empty;
				}

				if (!IsInDatabase && Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateTransactionNumForSelfBilling))
				{
					AH_TransactionNum = ZString.Empty;
				}
			}

			if (!saveSucceeded)
			{
				if (isAH_FullyPaidDateSetForZeroValueInvoice)
				{
					AH_FullyPaidDate = ZDateTime.Empty;
				}
				cashBasisVATRecordsForZeroValueInvoice.ForEach(item => item.Delete());
			}
			isAH_FullyPaidDateSetForZeroValueInvoice = false;
			cashBasisVATRecordsForZeroValueInvoice.Clear();

			if (isComplianceDocumentRelatedPropertiesHooked)
			{
				UnhookComplianceDocumentRelatedProperties();
			}

			if (OnSavedHander != null)
			{
				OnSavedHander(saveSucceeded);
			}
		}

		void CheckInvoiceTerms()
		{
			if (Header != null && AH_Ledger == LedgerTypes.AccountsReceivable && SubmittedFromInvoicingForm)
			{
				var arTerm = Header.CompanyData.GetARTerm(JobType, Direction, TransportMode, AH_GB, AH_GE, AH_TransactionCategory);
				if (AH_InvoiceTerm != arTerm.Term || AH_InvoiceTermDays != arTerm.Days)
				{
					new InvoiceTermsChangedEmail(this).Send();
				}
			}
		}

		void CheckCreditLimitExceeded()
		{
			if (!Factory.HasContext(BusinessContext.BulkTransactionSaving))
			{
				CheckCreditLimitExceeded(new[] { this });
			}
		}

		public static void CheckCreditLimitExceeded(IEnumerable<InvoicingBase> invoices)
		{
			if (invoices != null && invoices.Any())
			{
				var groupedInvoices = from invoice in invoices
									  where invoice.Header != null && !invoice.IsCreditLimitCheckOnSavedSuspended && !invoice.IsCreditLimitCheckSuspended && !(invoice.IsInMatchingContext || invoice.IsInInvoiceBatchContext) && !invoice.AH_IsCancelled
									  group invoice by new { ledger = invoice.AH_Ledger, orgPK = invoice.AH_OH } into g
									  select g;

				foreach (var groupedInvoice in groupedInvoices)
				{
					var factory = groupedInvoice.First().Factory;
					var emailSent = false;
					var ledger = groupedInvoice.Key.ledger == LedgerTypes.UnapprovedPayableTransactions ? LedgerTypes.AccountsPayable : (string)groupedInvoice.Key.ledger;
					var orgHeader = factory.Load<OrgHeader>(groupedInvoice.Key.orgPK);

					orgHeader.CreditChecker.ResetCache(inAsyncCall: false);
					var details = orgHeader.CreditChecker.GetCreditDetails(ledger);
					var creditLimit = AccountingUtils.Round(details.CreditLimit, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					var overBalance = details.TotalOutstandingAmount - creditLimit;
					var globalCreditLimit = AccountingUtils.Round(details.GlobalCreditLimit, details.GlobalCreditCurrency);
					var overGlobalBalance = details.GlobalTotalOutstandingAmount - globalCreditLimit;
					var invoicesToEmail = new List<InvoicingBase>();
					var invoicesToGlobalEmail = new List<InvoicingBase>();
					var creditThreshold = AccountingConfigurationRegistry.Instance.CreditLimitWarningThreshold.Value;
					var globalCreditThreshold = AccountingConfigurationRegistry.Instance.GlobalCreditLimitWarningThreshold.Value;
					RefCurrency globalCreditCurrency = null;

					if (creditLimit != 0)
					{
						invoicesToEmail = (details.OnCreditHold ? groupedInvoice : groupedInvoice.Where(x => x.InvoiceTerms != Constants.InvoiceTerms.CashOnDelivery)).ToList();
						if (invoicesToEmail.Count > 0 && overBalance > 0)
						{
							new CreditLimitExceededEmail(invoicesToEmail, creditLimit, overBalance).Send();
							emailSent = true;
						}
					}
					if (!emailSent && globalCreditLimit != 0)
					{
						invoicesToGlobalEmail = (details.IsOnGlobalCreditHold ? groupedInvoice : groupedInvoice.Where(x => x.InvoiceTerms != Constants.InvoiceTerms.CashOnDelivery)).ToList();
						if (invoicesToGlobalEmail.Count > 0 && overGlobalBalance > 0)
						{
							globalCreditCurrency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, details.GlobalCreditCurrency));
							new CreditLimitExceededEmail(invoicesToGlobalEmail, globalCreditLimit, overGlobalBalance, false, globalCreditCurrency).Send();
							emailSent = true;
						}
					}
					if (!emailSent && invoicesToEmail.Count > 0 && IsCreditLimitWarning(creditLimit, details.TotalOutstandingAmount, creditThreshold))
					{
						new CreditLimitWarningEmail(invoicesToEmail, creditLimit, creditThreshold).Send();
						emailSent = true;
					}
					if (!emailSent && invoicesToGlobalEmail.Count > 0 && IsCreditLimitWarning(globalCreditLimit, details.GlobalTotalOutstandingAmount, globalCreditThreshold))
					{
						globalCreditCurrency = globalCreditCurrency ?? factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, details.GlobalCreditCurrency));
						new CreditLimitWarningEmail(invoicesToGlobalEmail, globalCreditLimit, globalCreditThreshold, false, globalCreditCurrency).Send();
					}
				}
			}
		}

		static bool IsCreditLimitWarning(decimal creditLimit, decimal outstandingBalanceOrganisation, decimal creditThreshold)
		{
			decimal result;
			result = creditLimit * creditThreshold * 0.01m;

			return creditThreshold != 0 && outstandingBalanceOrganisation > result;
		}

		string InvoiceTerms
		{
			get
			{
				if (Header == null)
				{
					return null;
				}

				string invoiceTerms = Header.CompanyData.GetARTerm(JobType, Direction, TransportMode, AH_GE, AH_GE, AH_TransactionCategory).Term;
				if (AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					invoiceTerms = Header.CompanyData.GetAPTerm().Term;
				}

				return invoiceTerms;
			}
		}

		protected void UpdateJobChargeOverrideAddressContact()
		{
			if (IsInDatabase && this.HasContext(BusinessContext.OverrideInvoiceAddressContact)
						 && IsJobRelated && (AH_OA_InvoiceAddressOverrideInfo.HasChanges || AH_OC_InvoiceContactOverrideInfo.HasChanges))
			{
				int linesProcessed = 0;
				List<Charge> jobCharges = new List<Charge>();
				while (linesProcessed < Lines.Count)
				{
					int batchCount = Math.Min(Lines.Count - linesProcessed, BatchSize);
					var linePKs = Lines.Skip(linesProcessed).Take(batchCount).Select(item => item.PK);
					var filter = new ZQuery(JobChargeSchema.JR_AL_ARLine, linePKs);
					jobCharges.AddRange(Factory.Load<Charge>(filter));

					linesProcessed += batchCount;
				}

				foreach (var jobCharge in jobCharges)
				{
					using (jobCharge.GetValidationSuspender())
					{
						if (AH_OA_InvoiceAddressOverrideInfo.HasChanges)
						{
							jobCharge.JR_OA_SellInvoiceAddress = AH_OA_InvoiceAddressOverride;
						}

						if (AH_OC_InvoiceContactOverrideInfo.HasChanges)
						{
							jobCharge.JR_OC_SellInvoiceContact = AH_OC_InvoiceContactOverride;
						}
					}
				}
			}
		}

		int BatchSize
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return 3;
				}
#endif
				return _batchSize;
			}
		}

		const int _batchSize = 1000;

		#region Commission

		public virtual bool IsCommissionable
		{
			get { return false; }
		}

		void CreateOrQueueCommissions()
		{
			if (AH_PostDate.IsEmpty)
			{
				return;
			}

			var invoicePostedCreateCommissions = ObjectFactory.Get<IInvoicePostedCreateCommissions>("IInvoicePostedCreateCommissions", this);
			invoicePostedCreateCommissions.CommissionCreatorOverride = CommissionCreatorOverride;
			invoicePostedCreateCommissions.PostQueueItemOrCreateCommissions();
		}

		public void RegenerateCommissions(BusinessObjectFactory factoryForRegeneration, ILogger logger = null)
		{
			var commissionRegenerator = ObjectFactory.Get<ICommissionCreatorProvider>().GetCommissionRegenerator(this, factoryForRegeneration, logger);
			commissionRegenerator.RegenerateCommissions();
		}

		public ICommissionCreator CommissionCreatorOverride;

		ZString ICommissionableTransaction.AH_Calc_LocalRXCode
		{
			get
			{
				return AH_Calc_LocalRXCode;
			}
		}

		bool ICommissionableTransaction.IsJobRelated
		{
			get
			{
				return IsJobRelated;
			}
		}

		BusinessObjectCollection ICommissionableTransaction.Lines
		{
			get
			{
				return Lines;
			}
		}

		#endregion

		#region US Sales Tax

		public IUSSalesTaxCalculator USSalesTaxCalculator
		{
			get
			{
				if (usSalesTaxCalculatorValue == null)
				{
					usSalesTaxCalculatorValue = ObjectFactory.Get<IUSSalesTaxCalculator>();
					isUSSalesTaxCalculatorOwnedByThis = true;
				}
				return usSalesTaxCalculatorValue;
			}
			set
			{
				usSalesTaxCalculatorValue = value;
				isUSSalesTaxCalculatorOwnedByThis = false;
			}
		}

		void CalculateAndAddUSSalesTaxLineItemIfRequired()
		{
			if (!IsInDatabase
				&& USSalesTaxCalculator.IsEnabled(Branch)
				&& USSalesTaxCalculator.ShouldSetSalesTaxOnPost(this))
			{
				var (salesTaxResult, salesTaxError) = USSalesTaxCalculator.CalculateSalesTax(this);
				if (salesTaxError != null)
				{
					ErrorReporter.ReportOnce("USSalesTaxCalculation.OnPosting", "Error when calculating US Sales Tax.\r\n" + USSalesTaxCalculator.GetTransactionDetailsForIssueManager(this), salesTaxError);
				}
				else
				{
					USSalesTaxCalculator.SetSalesTaxLineItem(this, salesTaxResult.TotalSalesTaxAmount);
					usSalesTaxWasAddedOnPost = true;
				}
			}
		}

		void SubmitUSSalesTaxIfRequired()
		{
			if (usSalesTaxWasAddedOnPost)
			{
				var (_, salesTaxError) = USSalesTaxCalculator.SubmitSalesTax(this);
				if (salesTaxError != null)
				{
					ErrorReporter.ReportOnce("USSalesTaxCalculation.AfterPosting", "Error when submitting US Sales Tax.\r\n" + USSalesTaxCalculator.GetTransactionDetailsForIssueManager(this), salesTaxError);
				}
				else
				{
					usSalesTaxWasAddedOnPost = false;
				}

				if (isUSSalesTaxCalculatorOwnedByThis)
				{
					USSalesTaxCalculator.Dispose();
					USSalesTaxCalculator = null;
				}
			}
		}

		IUSSalesTaxCalculator usSalesTaxCalculatorValue;
		bool isUSSalesTaxCalculatorOwnedByThis;
		bool usSalesTaxWasAddedOnPost;

		#endregion

		#endregion

		#region Deleting

		protected override bool AllowDelete
		{
			get { return base.AllowDelete || AH_Ledger == LedgerTypes.IncompleteTransactions; }
		}

		protected override void DeleteCore()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();

			if (IsIncompleteInvoice)
			{
				DeleteIncompleteData();
				TransactionHeaderReferenceIRR?.Delete();
				DeleteINVAttributeOfChargesAndCostsImportedToIncompleteInvoice(Factory);
				RemoveLinkFromDraftInvoice();
			}

			DeleteCountrySpecificReferences();

			using (IsIncompleteInvoice ? ConsolCosting.ConsolSummary.UpdateSuspender.GetSuspender() : null)
			using (IsIncompleteInvoice ? GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().GetSuspender() : null)
			{
#if DEBUG
				if (GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().IsSuspended)
				{
					SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly++;
				}
#endif
				base.DeleteCore();
			}
		}

		public override void DeleteFromDB()
		{
			using (GetReportingDeletedApportionmentChargesSuspender())
			{
				base.DeleteFromDB();
			}
		}

		#endregion

		#region ReportingDeletedApportionmentChargesSuspender

		public IDisposable GetReportingDeletedApportionmentChargesSuspender()
		{
			return new ReportingDeletedApportionmentChargesSuspender(this);
		}

		public bool IsReportingDeletedApportionmentChargesSuspended
		{
			get { return ReportingDeletedApportionmentChargesSuspenderCount > 0; }
		}

		class ReportingDeletedApportionmentChargesSuspender : IDisposable
		{
			internal ReportingDeletedApportionmentChargesSuspender(InvoicingBase parent)
			{
				Parent = parent;
				Parent.ReportingDeletedApportionmentChargesSuspenderCount++;
			}

			void IDisposable.Dispose()
			{
				Parent.ReportingDeletedApportionmentChargesSuspenderCount--;

				if (Parent.ReportingDeletedApportionmentChargesSuspenderCount == 0)
				{
					if (Parent.Lines.Cast<InvoicingLineBase>().Any(line => !line.ImportedApportionmentID.IsEmpty && !line.IsDeleted && line.ApportionmentChargeImportedFrom == null))
					{
						ErrorReporter.ReportOnce("ApportionmentChargeImportedFromIsDeleted_4", "Apportion charge linked to invoice line has been deleted unexpectedly with no call stack collected");
					}
				}
			}

			readonly InvoicingBase Parent;
		}

		int ReportingDeletedApportionmentChargesSuspenderCount;

		#endregion

		#region Handling Incomplete Transactions

		HiddenStmNote LoadOrCreateIncompleteTransactionDataNote()
		{
			HiddenStmNote note = GetDetailsNoteIfItExists();
			if (note == null)
			{
				note = Factory.New<HiddenStmNoteNotAutoLogged>();
				note.ST_Description = IncompleteTransactionDataDescription;
				note.ST_Table = AccTransactionHeaderSchema.Constants.TableName;
				note.ST_ParentID = PK;
			}
			return note;
		}

		HiddenStmNote GetDetailsNoteIfItExists(BusinessObjectFactory factory = null)
		{
			ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, IncompleteTransactionDataDescription);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, PK);
			return (factory ?? Factory).LoadTop1<HiddenStmNote>(filter);
		}

		IValueObjectDataAdapter GetDataAdapter()
		{
			if (IsAPTransaction)
			{
				var type = Type.GetType("Enterprise.Accounting.DataTransfer.IncompleteTransactionDataAdapter`1, Enterprise.Accounting.DataTransfer");
				var generic = type.MakeGenericType(GetType());
				return (IValueObjectDataAdapter)Activator.CreateInstance(generic, this);
			}
			return null;
		}

		const string IncompleteTransactionDataDescription = "IncompleteTransactionData";

		public string SaveAsIncomplete()
		{
			MakeAsIncomplete(out var subTypeMessage);

			using (new DisposableAction(
				() => Factory.SetContext(BusinessContext.SavingAsIncomplete),
				() => Factory.RemoveContext(BusinessContext.SavingAsIncomplete)))
			{
				PrepareAttributesOfImportedChargesAndCostsForSavingIncompleteInvoice();
				Factory.Save();
			}
			return subTypeMessage;
		}

		internal void MakeAsIncomplete(out string subTypeMessage)
		{
			DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);

			subTypeMessage = ResetComplianceSubtypeIfApplicable();

			RecalculateAH_JH(true);
			MoveToIncompleteLedger();
			RegisterInvoiceToSaveOnlyInvoiceHeader();
			LoadOrCreateIncompleteTransactionDataNote().ST_NoteDataAsText = Serialize();
			Factory.Saved += IncompleteInvoiceFactory_Saved;
		}

		string ResetComplianceSubtypeIfApplicable()
		{
			if (!AH_ComplianceSubType.IsEmpty && IsAPTransaction && IsComplianceNumberAllocationMandatory)
			{
				AH_ComplianceSubType = ZString.Empty;
				return Res.GetString("94C1A989-5C88-41B4-9CDA-9E14B9DCF1E0", $@"Note: Incomplete Invoice is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated");
			}
			return null;
		}

		void IncompleteInvoiceFactory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= IncompleteInvoiceFactory_Saved;

			if (savedSuccessfully)
			{
				CacheChargesAndCostsOriginallyImportedToIncompleteInvoice();
				((IBusinessObjectState)this).ClearHasChangesIncludingChildren();
			}
		}

		internal void MoveToIncompleteLedger()
		{ //GetNextIncompleteSelfBillingInvoiceTransactionCount uses this logic as well
			AH_Ledger = LedgerTypes.IncompleteTransactions;
			AH_TransactionType = AccountingUtils.ConvertTransactionTypeFromAPToINSafe(AH_TransactionType);
		}

		protected override bool IsTransactionInDatabaseReadOnlyCore
		{
			get { return base.IsTransactionInDatabaseReadOnlyCore && AH_Ledger != LedgerTypes.IncompleteTransactions && !IsAllocatingInvoice; }
		}

		internal string Serialize()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				IValueObjectDataAdapter dataAdapter = GetDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				serializer.ExportXmlData(stream, dataAdapter, new[] { this }, new ValueObjectExportContext(new NotificationBuffer()));

				stream.Flush();

				string result = StreamConverter.StreamToString(stream);
				return result;
			}
		}
		public class RestoreSavedDataResult
		{
			public RestoreSavedDataResult(ResultType result, ZString error)
			{
				if (result == ResultType.Success && !error.IsEmpty)
				{
					throw new ArgumentException("Error should be empty when 'Result = Success'");
				}
				else if (result != ResultType.Success && error.IsEmpty)
				{
					throw new ArgumentException("Error cannot be empty when 'Result != Success'");
				}

				Result = result;
				Error = error;
			}

			public ResultType Result { get; }
			public ZString Error { get; }

			public enum ResultType
			{
				Success,
				SuccessWithErrors,
				Failed
			}
		}

		public RestoreSavedDataResult RestoreSavedData(bool runPreSaveValidation = true)
		{
			RestoreSavedDataResult result = null;

			var detailsNote = GetDetailsNoteIfItExists();
			if (detailsNote == null)
			{
				return new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Failed, Res.GetString("5DC32C16-C707-4418-B4D5-001F6101B0DD", "There is a problem with the transaction and this transaction can no longer be used. You will need to delete this transaction from your system"));
			}
			else
			{
				try
				{
					var error = Deserialize(detailsNote.ST_NoteDataAsText);
					if (!error.IsEmpty)
					{
						result = new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.SuccessWithErrors, error);
					}

					CacheChargesAndCostsOriginallyImportedToIncompleteInvoice();
					RegisterInvoiceToSaveOnlyInvoiceHeader();
				}
				catch (JobCreationException ex)
				{
					return new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Failed, ex.Message);
				}
			}

			ValidateAndFixTaxBranch();

			ImportAllApportionmentsFromCosting();

			if (IsIncompleteInvoice)
			{
				foreach (JobConsolCost enteredCost in ConsolCosting.ConsolCosts)
				{
					ValidateAndFixConsolCostsMarkedAsImported(enteredCost);
				}

				var apInvoice = this as APInvoice;
				apInvoice?.SetDefaultFinalFlags();
			}

			RestoreTaxTransactionInIncompleteInvoiceHelper.RestoreTaxTransactionFromTaxRecordData(this);

			((IBusinessObjectState)this).ClearHasChangesIncludingChildren();

			if (runPreSaveValidation)
			{
				MarkAsNeedingValidationIncludingChildren();
				RunPreSaveValidation();
			}

			return result ?? new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Success, ZString.Empty);
		}

		public void ValidateAndFixTaxBranch()
		{
			if (!AH_GB_TaxBranch.IsEmpty && !CanApplyTaxBranch)
			{
				AH_GB_TaxBranch = ZGuid.Empty;
			}
		}

		bool noteCleanupRequired;
		bool attributeCleanupRequired;
		void DeleteIncompleteData(BusinessObjectFactory factory = null)
		{
			HiddenStmNote note = GetDetailsNoteIfItExists(factory);
			if (note == null)
			{
				return;
			}
			note.Delete();
		}

		void RemoveLinkFromDraftInvoice()
		{
			var draftInvoiceLinkedToInvoice = Factory.LoadTop1<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, PK));
			if (draftInvoiceLinkedToInvoice != null)
			{
				draftInvoiceLinkedToInvoice.AIH_AH_PostedTransactionHeader = ZGuid.Empty;
			}
		}

		public delegate void ShowErrorHandler(string message, string caption);
		public ShowErrorHandler ShowError;

		public void RaiseShowError(string message, string caption)
		{
			if (ShowError != null)
			{
				ShowError(message, caption);
			}
			else
			{
				throw new ZException(Res.GetString("a1ea9664-beee-4b10-9073-8a575bc17890", "GUI message handler is not hooked"));
			}
		}

		internal ZString Deserialize(string serializedInvoiceData)
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.INTransactionSerializedData, () => serializedInvoiceData);

			ZString error = ZString.Empty;

			using (Stream stream = StreamConverter.StringToStream(serializedInvoiceData, Encoding.UTF8))
			{
				IValueObjectDataAdapter dataAdapter = GetDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				NotificationBuffer notifications = new NotificationBuffer();
				InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory);
				try
				{
					serializer.ImportXmlData(stream, dataAdapter, collection, null, notifications);
				}
				finally
				{
					collection.RemoveAll();
				}
				Factory.ClearQueryCache();

				var errors = notifications.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error);
				if (errors.Count() > 0)
				{
					error = errors.ToUniqueMessageListString();
				}
			}

			return error;
		}

		public void MoveFromIncompleteToPayableLedger()
		{
			AH_Ledger = LedgerTypes.AccountsPayable;
			AH_TransactionType = AccountingUtils.ConvertTransactionTypeFromINToAPSafe(AH_TransactionType);
			SubmittedFromInvoicingForm = true;
			DeregisterInvoiceFromSaveOnlyInvoiceHeader();
		}

		void CacheChargesAndCostsOriginallyImportedToIncompleteInvoice()
		{
			ChargesOriginallyImportedToIncompleteInvoice = GetImportedChargeDetailsFromIncompleteInvoice();
			ConsolCostsOriginallyImportedToIncompleteInvoice = GetImportedConsolCostDetailsFromIncompleteInvoice();
		}

		void RegisterInvoiceToSaveOnlyInvoiceHeader()
		{
			IncompleteInvoiceBOIsSavedByFactoryServiceProvider.RegisterInvoiceToSaveOnlyInvoiceHeader(this);
		}

		void DeregisterInvoiceFromSaveOnlyInvoiceHeader()
		{
			IncompleteInvoiceBOIsSavedByFactoryServiceProvider.DeregisterInvoiceFromSaveOnlyInvoiceHeader(this);
		}

		#endregion

		#region Calculation Providers

		internal TermsAndDueDateCalculationProvider TermsAndDueDateCalculationProvider
		{
			get
			{
				if (fTermsAndDueDateCalculationProvider == null)
				{
					if (AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						fTermsAndDueDateCalculationProvider = new ARTermsAndDueDateCalculationProvider(this);
					}
					else
					{
						fTermsAndDueDateCalculationProvider = new APTermsAndDueDateCalculationProvider(this);
					}
				}
				return fTermsAndDueDateCalculationProvider;
			}
		}

		TermsAndDueDateCalculationProvider fTermsAndDueDateCalculationProvider;

		#endregion

		#region Invoice Document Types

		public OperationsInvoiceTypes OperationsInvoiceType
		{
			get
			{
				if (IsConsolInvoiceType)
				{
					return OperationsInvoiceTypes.Consol;
				}
				else if (IsCustomsInvoice)
				{
					return OperationsInvoiceTypes.Customs;
				}
				else if (IsFreightInvoice)
				{
					return OperationsInvoiceTypes.Freight;
				}
				else if (IsTransportInvoice)
				{
					return OperationsInvoiceTypes.Transport;
				}
				else
				{
					return OperationsInvoiceTypes.Miscellaneous;
				}
			}
		}

		protected bool IsConsolInvoiceType
		{
			get
			{
				var jobs = GetAllJobs();
				return jobs.Count() > 1 && DoAllJobsHaveCorrectForeignKeyLinks(jobs, OperationsInvoiceTypes.Freight);
			}
		}

		protected bool IsCustomsInvoice
		{
			get
			{
				var jobs = GetAllJobs();
				return jobs.Count() == 1 && DoAllJobsHaveCorrectForeignKeyLinks(jobs, OperationsInvoiceTypes.Customs);
			}
		}

		protected bool IsTransportInvoice
		{
			get
			{
				var jobs = GetAllJobs();
				return jobs.Count() == 1 && DoAllJobsHaveCorrectForeignKeyLinks(jobs, OperationsInvoiceTypes.Transport);
			}
		}

		protected bool IsFreightInvoice
		{
			get
			{
				var jobs = GetAllJobs();
				return jobs.Count() == 1 && DoAllJobsHaveCorrectForeignKeyLinks(jobs, OperationsInvoiceTypes.Freight);
			}
		}

		protected bool DoAllJobsHaveCorrectForeignKeyLinks(IEnumerable<Job> jobs, OperationsInvoiceTypes invoiceType)
		{
			foreach (Job job in jobs)
			{
				if (job.JH_ParentTableCode != GetParentTableCodeFromInvoiceType(invoiceType))
				{
					return false;
				}
			}
			return true;
		}

		IEnumerable<Job> GetAllJobs()
		{
			var result = new HashSet<Job>();
			if (AH_JH.IsValid)
			{
				result.Add(Factory.Load<Job>(AH_JH));
			}

			foreach (InvoicingLineBase line in Lines)
			{
				if (line.AL_JH.IsValid)
				{
					result.Add(Factory.Load<Job>(line.AL_JH));
				}
			}
			return result;
		}

		protected string GetParentTableCodeFromInvoiceType(OperationsInvoiceTypes invoiceType)
		{
			switch (invoiceType)
			{
				case OperationsInvoiceTypes.Consol:
				case OperationsInvoiceTypes.Freight:
					return JobShipmentSchema.Constants.Prefix;

				case OperationsInvoiceTypes.Customs:
					return JobDeclarationSchema.Constants.Prefix;

				case OperationsInvoiceTypes.Transport:
					return JobCartageSchema.Constants.Prefix;

				default:
					return "";
			}
		}

		#endregion

		#region Lookups

		public new InvoicingBaseLookups Lookups => (InvoicingBaseLookups)base.Lookups;

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new InvoicingBaseLookups(this);
		}

		public TransactionHeaderCollection OriginalTransactionList
		{
			get
			{
				if (fOriginalTransactionReferenceList == null || PreviousAH_OH != AH_OH)
				{
					fOriginalTransactionReferenceList = GetOriginalTransactionReferenceList();
					PreviousAH_OH = AH_OH;
				}
				return fOriginalTransactionReferenceList;
			}
		}

		TransactionHeaderCollection fOriginalTransactionReferenceList;

		protected TransactionHeaderCollection GetOriginalTransactionReferenceList()
		{
			ZQuery filter = new ZQuery();
			if (IsAmendingTransaction_StrongReference)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.PK, ((IAmending)this).OriginalTransaction.PK);
			}
			else
			{
				if (IsARInvoiceOrCreditNote)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
				}

				if (IsAPInvoiceOrCreditNote)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				}

				filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, AH_OH);

				if (AH_TransactionType == TransactionTypes.CreditNote)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				}

				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);
				filter.AddToFilter(AddTransactionLineFilters(PK));
			}

			var result = new TransactionHeaderCollection(Factory, filter);

			if (IsAmendingTransaction_StrongReference)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction #", "Property", ((IAmending)this).OriginalTransaction.TransactionNumber));
			}
			else
			{
				if (IsARInvoiceOrCreditNote)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AccountingUtils.OrganisationFilterTypes.Debtor, "Property", AH_OH, false));
				}

				if (IsAPInvoiceOrCreditNote)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AccountingUtils.OrganisationFilterTypes.Creditor, "Property", AH_OH, false));
				}

				if (AH_TransactionType == TransactionTypes.CreditNote)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transaction Type", "Property", new ZString(TransactionTypes.Invoice), false));
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery AddTransactionLineFilters(ZGuid transactionHeaderPK)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery linesFilter = new ZQuery(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.Equal, transactionHeaderPK);
			linesFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, AH_GC);
			AccTransactionLines[] lines = newFactory.Load(typeof(AccTransactionLines), linesFilter) as AccTransactionLines[];

			ZQuery transactionHeadersToExcludeFilter = new ZQuery();
			foreach (AccTransactionLines line in lines)
			{
				if (line.AL_AH.IsValid && line.AL_JH.IsValid)
				{
					transactionHeadersToExcludeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, line.AL_AH);
				}
			}

			return transactionHeadersToExcludeFilter;
		}

		ZGuid PreviousAH_OH;

		InvoiceTypesList InvoiceTypes
		{
			get { return invoiceTypes ?? (invoiceTypes = new InvoiceTypesList()); }
		}
		InvoiceTypesList invoiceTypes;

		#region Lists

		public CodeDescriptionPairList InvoiceTerms_List
		{
			get
			{
				return Ledger == LedgerTypes.AccountsReceivable ? (InvoiceTermsList)FindboxLookupCollections.GetARInvoiceTermsList(Factory) :
									FindboxLookupCollections.GetAPInvoiceTermsList(Factory);
			}
		}

		#endregion

		#endregion

		#region Public Methods

		public void SetWHTReadOnlyState()
		{
			foreach (InvoicingLineBase line in Lines)
			{
				line.UpdateWHTReadOnlyState();
			}
		}

		public bool IsSettingLineExchangeRateSupported
		{
			get { return IsAPInvoiceOrCreditNote; }
		}

		public bool IsEditingLocalAmountsSupported
		{
			get { return IsAPInvoiceOrCreditNote && AH_RX_NKTransactionCurrency == AH_Calc_LocalRXCode; }
		}

		public bool IsMiscServWHTApplicable
		{
			get
			{
				if (Header != null && Header.MiscServ != null)
				{
					return (AH_Ledger == LedgerTypes.AccountsReceivable ?
						Header.MiscServ.OM_ARWHTApplicable :
						Header.MiscServ.OM_APWHTApplicable);
				}
				else
				{
					return false;
				}
			}
		}

		public virtual void ReleaseAllMutexOnInvoice()
		{
			if (IsBeingCreatedPostedAllocatedApprovedOrIncomplete || LinesHaveBeenLoaded)
			{
				foreach (InvoicingLineBase line in Lines)
				{
					line.ReleaseMutex();
				}
			}

			LineJobsWithMutex.ForEach(x => x.Dispose());
		}

		public bool HasInvalidPostingGroups
		{
			get
			{
				if (!IsReversalTransaction && !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value && AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					var postingGroupsCount = Lines.Cast<InvoicingLineBase>()
						.Where(x => !x.AL_AT.IsEmpty)
						.Select(x => x.PostingGroupID).Distinct().Count();

					if (postingGroupsCount > 1)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region Implementation

		bool ReadOnlyForAssociatedDraftInvoice
		{
			get
			{
				if (!Globals.IsUserInteractive || !(IsLoadedFromAPTransaction() || IsLoadedFromAPIncompleteTransaction()))
				{
					return false;
				}

				return IsSetFromDraftInvoice || (readOnlyForAssociatedDraftInvoice ??= Factory.Exists(typeof(AccDraftInvoiceHeader), new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, PK)));

				bool IsLoadedFromAPTransaction()
				{
					var types = new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote };
					if (AH_Ledger == LedgerTypes.AccountsPayable && types.Contains((string)AH_TransactionType))
					{
						return true;
					}
					return false;
				}

				bool IsLoadedFromAPIncompleteTransaction()
				{
					var types = new[] { TransactionTypes.IncompleteInvoice, TransactionTypes.IncompleteCreditNote, TransactionTypes.IncompleteAdjustmentNote };
					if (AH_Ledger == LedgerTypes.IncompleteTransactions && types.Contains((string)AH_TransactionType))
					{
						return true;
					}
					return false;
				}
			}
		}

		bool? readOnlyForAssociatedDraftInvoice;

		protected override List<string> GetWritableProperties()
		{
			List<string> result = base.GetWritableProperties();
			result.Add("IncludeInTheBatch");
			result.Add("IncludeInThePeriodicInvoice");

			if (AH_Ledger == LedgerTypes.TransactionsPendingAllocation)
			{
				result.Add(AutoAccTransactionHeader.Schema.AH_ComplianceSubType);
			}

			return result;
		}

		Charge[] ClearedJobCharges;

		protected ZGuid GetChargeCodeTaxPK(ZGuid lineChargeCode)
		{
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, SQLComparisonOperator.Equal, lineChargeCode);
			AccGenericCharge chargeCode = Factory.LoadTop1<AccGenericCharge>(filter);

			return (chargeCode != null) ? chargeCode.VC_GSTRate : ZGuid.Empty;
		}

		protected virtual void CopyTransactionLine(InvoicingLineBase fromLine, InvoicingLineBase toLine, bool populateAmount = true)
		{
			toLine.AL_LineType = fromLine.AL_LineType;
			toLine.AL_UnitQty = fromLine.AL_UnitQty;
			toLine.AL_UnitPrice = fromLine.AL_UnitPrice;
			toLine.AL_OSUnitPrice = fromLine.AL_OSUnitPrice;
			toLine.AL_PreventInvoicePrintGrouping = fromLine.AL_PreventInvoicePrintGrouping;

			toLine.AL_OH = fromLine.AL_OH;
			toLine.AL_JH = fromLine.AL_JH;

			toLine.GenericCharge = !fromLine.AL_AC.IsValid ? fromLine.AL_AG : fromLine.AL_AC;
			toLine.AL_AC = fromLine.AL_AC;
			toLine.AL_AG = fromLine.AL_AG;

			toLine.AL_GB = fromLine.AL_GB;
			toLine.AL_GE = fromLine.AL_GE;

			toLine.AL_Desc = fromLine.AL_Desc;

			toLine.AL_AG_PercentOf = fromLine.AL_AG_PercentOf;
			toLine.AL_PercentageOfPeriod = fromLine.AL_PercentageOfPeriod;
			toLine.AL_WithholdingTax = fromLine.AL_WithholdingTax;

			toLine.AL_PostPeriod = 0;
			toLine.AL_PostToGL = "N";
			toLine.AL_ReversePeriod = 0;
			toLine.AL_ReverseToGL = "N";

			toLine.AL_RX_NKTransactionCurrency = fromLine.AL_RX_NKTransactionCurrency;

			toLine.AL_GB_TaxBranch = fromLine.AL_GB_TaxBranch;

			// Tax Id and Tax Rate

			toLine.AL_SupplyType = fromLine.AL_SupplyType;
			toLine.AL_AT = fromLine.AL_AT;
			toLine.SetTaxDateSafe(fromLine.AL_TaxDate);
			toLine.AL_A9_VATClass = fromLine.AL_A9_VATClass;
			toLine.AL_AW = fromLine.AL_AW;

			if (populateAmount)
			{
				if (CanCopyLineExRateForAmending)
				{
					toLine.AL_ExchangeRate = fromLine.AL_ExchangeRate;
				}
				else
				{
					toLine.SetExchangeRate();
				}

				toLine.AL_LineAmount = fromLine.AL_LineAmount;
				toLine.AL_OSAmount = fromLine.AL_OSAmount;
				toLine.AL_OSExTaxAmount = fromLine.AL_OSExTaxAmount;
				using (var osAmountRecalculationSuspender = new TransactionLine.OSAmountRecalculationSuspender(toLine))
				{
					toLine.AL_LocalExTaxAmount = fromLine.AL_LocalExTaxAmount;
				}
				toLine.AL_LocalTaxAmount = fromLine.AL_LocalTaxAmount;
			}
			toLine.AL_GovtChargeCode = fromLine.AL_GovtChargeCode;

			toLine.AL_RevRecognitionType = fromLine.AL_RevRecognitionType;

			toLine.CopiedFromPK = fromLine.PK;
			toLine.AL_PlaceOfSupply = fromLine.AL_PlaceOfSupply;
		}

		protected bool IsARAP => IsReceivableOrPayable || AH_Ledger == LedgerTypes.UnapprovedPayableTransactions;

		bool IsInvoiceOrCreditNote => AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote;

		internal bool IsUAInvoiceOrCreditNote => AH_TransactionType == TransactionTypes.UAInvoice || AH_TransactionType == TransactionTypes.UACreditNote;

		public bool IsIncompletInvoiceOrCreditNote => AH_TransactionType == TransactionTypes.IncompleteInvoice || AH_TransactionType == TransactionTypes.IncompleteCreditNote;

		bool IsCreatingAPInvoiceOrCRD => AH_Ledger == LedgerTypes.AccountsPayable && !AH_IsCancelled && IsInvoiceOrCreditNote;

		bool IsCreatingUAInvoiceOrCRD => AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && !AH_IsCancelled && IsUAInvoiceOrCreditNote;

		public bool IsConvertedUAInvoiceOrCRD => Ledger == LedgerTypes.UnapprovedPayableTransactions && AH_Ledger == LedgerTypes.AccountsPayable &&
				!AH_IsCancelled && IsInvoiceOrCreditNote;

		public bool IsConvertedFromARInvoice { get; set; }

		protected virtual AccountingNumberFountainWrapper NumberFountainForInternalRef => null;

		public bool IsInvoiceApproving => IsConvertedUAInvoiceOrCRD && IsInDatabase;

		public bool IsARInvoiceOrCreditNote => AH_Ledger == LedgerTypes.AccountsReceivable && IsInvoiceOrCreditNote;

		public bool IsARAPInvoiceOrCreditNote => (AH_Ledger == LedgerTypes.AccountsReceivable || AH_Ledger == LedgerTypes.AccountsPayable) && IsInvoiceOrCreditNote;

		public bool IsAPInvoiceOrCreditNote => AH_Ledger == LedgerTypes.AccountsPayable && IsInvoiceOrCreditNote
			|| AH_TransactionType == TransactionTypes.UAInvoice
			|| AH_TransactionType == TransactionTypes.UACreditNote
			|| AH_TransactionType == TransactionTypes.IncompleteInvoice
			|| AH_TransactionType == TransactionTypes.IncompleteCreditNote;

		internal void SetLinesAL_GSTVATBasis(InvoicingLineBase lineToSet = null)
		{
			if (SetLinesAL_GSTVATBasisSuspender.IsSuspended)
			{
				return;
			}

			var lines = lineToSet != null ? new[] { lineToSet } : Lines.ToArray<InvoicingLineBase>();
			lines.ForEach(x => x.SetDefaultPlaceOfSupply());

			var ledger = AH_Ledger;
			if (ledger == LedgerTypes.IncompleteTransactions)
			{
				ledger = LedgerTypes.AccountsPayable;
			}
			var organisation = Header;
			var isCashBasisGSTAllowed = (!IsInDatabase || HasChanges) && GlbCompany.CurrentCompany.GC_IsGSTCashBasis &&
										(ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.AccountsReceivable) &&
										organisation != null;
			TaxRecognitionDefaultingRules registryItem = null;
			var orgConfig = ZString.Empty;
			if (isCashBasisGSTAllowed)
			{
				orgConfig = ledger == LedgerTypes.AccountsReceivable ? organisation.CompanyData.OB_ARVATConfig : organisation.CompanyData.OB_APVATConfig;
			}

			foreach (var line in lines)
			{
				var lineValue = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
				if (isCashBasisGSTAllowed)
				{
					var chargeCode = line.ChargeCode;
					var glHeader = line.GLHeader;
					if (line.AL_AT.IsValid && (chargeCode != null || glHeader != null))
					{
						registryItem = registryItem ?? AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.Value;
						if (orgConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code ||
							orgConfig == AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code && registryItem.IsCashBasisRecognition(ledger, chargeCode))
						{
							lineValue = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
						}
					}
				}
				if (line.AL_GSTVATBasis != lineValue)
				{
					line.AL_GSTVATBasis = lineValue;
				}
			}
		}

		internal FunctionalitySuspender SetLinesAL_GSTVATBasisSuspender
		{
			get { return setLinesAL_GSTVATBasisSuspender ?? (setLinesAL_GSTVATBasisSuspender = new FunctionalitySuspender(() => SetLinesAL_GSTVATBasis(), true)); }
		}
		FunctionalitySuspender setLinesAL_GSTVATBasisSuspender;

		#endregion

		#region IMatching Members

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

		ZString IMatching.PaymentCriticality
		{
			get { return AH_RequisitionStatus; }
		}

		ZDateTime IMatching.PaymentRequestedDate
		{
			get { return AH_RequisitionDate; }
		}

		bool IMatching.IsMatched
		{
			get { return AH_LocalOutstandingAmount != AH_LocalTotalAmount; }
		}

		void IMatching.FullyPay(ZDateTime fullyPaidDate)
		{
			MatchingMonitor.FullyPay(fullyPaidDate);

			if (!ThisAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty)
			{
				foreach (ILineMatching line in Lines)
				{
					line.IsFullyPay = true;
				}
			}
		}

		void IMatching.PartiallyPay()
		{
			MatchingMonitor.PartiallyPay();
			AH_FullyPaidDate = ZDateTime.Empty;
		}

		void IMatching.GenerateMatchLinksCore()
		{
			MatchingMonitor.GenerateMatchLinks();
		}

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			MatchingMonitor.GeneratePaymentApprovalItems(approval);
		}

		PaymentApprovalItemCollection IMatching.PaymentApprovalItems
		{
			get
			{
				if (fPaymentApprovalItems == null)
				{
					fPaymentApprovalItems = new PaymentApprovalItemCollection(Factory);
				}

				return fPaymentApprovalItems;
			}
		}

		PaymentApprovalItemCollection fPaymentApprovalItems;

		UnmatchingResult IMatching.CanUnmatch(ZDecimal matchLinkAmount)
		{
			return CanUnmatchMatchLink(matchLinkAmount);
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			UnmatchCore(matchLinkAmount, matchLinkOSAmount);
		}

		protected virtual void UnmatchCore(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			AH_FullyPaidDate = ZDateTime.Empty;
			TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(this, matchLinkAmount, matchLinkOSAmount, true);
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
		}

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get { return fCurrentMatchGroup ?? (fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory)); }
		}
		protected TransactionMatchLinkGroup fCurrentMatchGroup;

		TransactionMatchLinkCollection IMatching.Matchlinks
		{
			get
			{
				return matchlinks ?? (matchlinks = new TransactionMatchLinkCollection(Factory, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK)));
			}
		}
		TransactionMatchLinkCollection matchlinks;

		public InvoicingBase CorrespondingReversedTransaction
		{
			get
			{
				ZGuid reversedGuid = ZGuid.Empty;
				if (AH_IsCancelled)
				{
					return !AH_TransactionBelongsToGroup.IsValid ? Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC)) : Factory.Load<InvoicingBase>(AH_TransactionBelongsToGroup);
				}
				return this;
			}
		}

		ZDecimal IMatching.OSOutstandingAmount
		{
			get { return OSOutstandingAmountMatching; }
		}

		ZPropertyInfo IMatching.OSOutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OSOutstandingAmount"); }
		}

		ZDecimal IMatching.OutstandingAmount
		{
			get { return OutstandingAmountMatching; }
		}

		ZPropertyInfo IMatching.OutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OutstandingAmount"); }
		}

		ZDecimal IMatching.OriginalOutstandingAmount
		{
			get { return AH_OutstandingAmount; }
		}

		[List("Organisations")]
		ZGuid IMatching.Organisation
		{
			get { return AH_OH; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return GetZPropertyInfo("Organisation"); }
		}

		ZString IMatching.Ledger
		{
			get { return AH_Ledger; }
		}

		ZPropertyInfo IMatching.LedgerInfo
		{
			get { return GetZPropertyInfo("Ledger"); }
		}

		ZString IMatching.TransactionNumber
		{
			get { return AH_TransactionNum; }
		}

		ZPropertyInfo IMatching.TransactionNumberInfo
		{
			get { return GetZPropertyInfo("TransactionNumber"); }
		}

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty ? currencyNK == AH_RX_NKTransactionCurrency : ThisAsISupportMatchingOfMyLines.IsAllPaidLinesInTheSameCurrency(currencyNK);
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			ZDecimal result = 0;
			if (ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty)
			{
				result = currencyNK == AH_RX_NKTransactionCurrency ? ((IMatching)this).OSPartialPaymentAmount : 0;
			}
			else
			{
				result = ThisAsISupportMatchingOfMyLines.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(currencyNK);
			}
			return result;
		}

		ZDecimal IMatching.OSPartialPaymentAmount
		{
			get
			{
				return ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty ? fOSPartialPaymentAmount : ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount;
			}
			set
			{
				fOSPartialPaymentAmount = value;

				ThisAsISupportMatchingOfMyLines.ApportionPaidAmountToLines();
				ThisAsIMatching.OSPartialPaymentAmountInfo.RefreshBinding();
				ThisAsIMatching.LocalPartialPaymentAmountInfo.RefreshBinding();

				if (!IsValidationSuspended && Validation is MatchingValidation)
				{
					((MatchingValidation)Validation).ValidateOSPartialPaymentAmount();
				}
			}
		}

		ZDecimal fOSPartialPaymentAmount;

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("OSPartialPaymentAmount"); }
		}

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get
			{
				ZDecimal localPartialPaymentAmtReturn = OutstandingAmountMatching;
				if (ThisAsIMatching.OSPartialPaymentAmount != OSOutstandingAmountMatching)
				{
					localPartialPaymentAmtReturn = (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(ThisAsIMatching.OSPartialPaymentAmount, AH_ExchangeRate);
				}
				return (Math.Abs(localPartialPaymentAmtReturn) > Math.Abs(OutstandingAmountMatching) && OutstandingAmountMatching != 0) ? OutstandingAmountMatching : localPartialPaymentAmtReturn;
			}
		}

		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("LocalPartialPaymentAmount"); }
		}

		ZString IMatching.TransactionType
		{
			get { return AH_TransactionType; }
		}

		ZPropertyInfo IMatching.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZGuid IMatching.BranchGuid
		{
			get { return AH_GB; }
		}

		ZPropertyInfo IMatching.BranchGuidInfo
		{
			get { return GetZPropertyInfo("BranchGuid"); }
		}

		ZGuid IMatching.DepartmentGuid
		{
			get { return AH_GE; }
		}

		ZPropertyInfo IMatching.DepartmentGuidInfo
		{
			get { return GetZPropertyInfo("DepartmentGuid"); }
		}

		[MaxLength(3)]
		ZString IMatching.CurrencyCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return GetZPropertyInfo("CurrencyCode"); }
		}

		ZInt IMatching.CurrencyDecimals
		{
			get { return AH_Calc_RXDecimals; }
		}

		ZPropertyInfo IMatching.CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("CurrencyDecimals"); }
		}

		ZInt IMatching.LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("LoginCompanyCurrencyDecimals"); }
		}

		ZString IMatching.Description
		{
			get { return AH_Desc; }
		}

		ZPropertyInfo IMatching.DescriptionInfo
		{
			get { return GetZPropertyInfo("Description"); }
		}

		ZDateTime IMatching.PostDate
		{
			get { return AH_PostDate; }
			set { AH_PostDate = value; }
		}

		ZPropertyInfo IMatching.PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostDate)); }
		}

		bool IMatching.PostDate_ReadOnly
		{
			get { return true; }
		}

		public ZString ChequeOrReference
		{
			get { return AH_ChequeOrReference; }
			set { AH_ChequeOrReference = value; }
		}

		public void AddChequeOrReferenceLog()
		{
			if (this.HasContext(BusinessContext.OverrideInvoiceReference) &&
				AH_Ledger == LedgerTypes.AccountsPayable &&
				(ZString)AH_ChequeOrReferenceInfo.OriginalValue != AH_ChequeOrReference)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture,
					"Cost reference changed from '{0}' to '{1}'",
					IsInDatabase ?
					AH_ChequeOrReferenceInfo.OriginalValue : ZString.Empty,
					AH_ChequeOrReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		ZString IMatching.ChequeOrReference
		{
			get { return ChequeOrReference; }
			set { ChequeOrReference = value; }
		}

		public ZPropertyInfo ChequeOrReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ChequeOrReference), x => AH_ChequeOrReferenceInfo); }
		}

		ZPropertyInfo IMatching.ChequeOrReferenceInfo
		{
			get { return ChequeOrReferenceInfo; }
		}

		ZDecimal IMatching.ExchangeRateAmount
		{
			get { return AH_ExchangeRate; }
		}

		ZPropertyInfo IMatching.ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo("ExchangeRateAmount"); }
		}

		ZString IMatching.TransactionReference
		{
			get { return AH_TransactionReference; }
		}

		ZPropertyInfo IMatching.TransactionReferenceInfo
		{
			get { return GetZPropertyInfo("TransactionReference"); }
		}

		ZString IMatching.ConsolidatedRef
		{
			get { return AH_ConsolidatedInvoiceRef; }
		}

		ZPropertyInfo IMatching.ConsolidatedRefInfo
		{
			get { return GetZPropertyInfo("ConsolidatedRef"); }
		}

		ZDateTime IMatching.InvoiceDate
		{
			get { return AH_InvoiceDate; }
		}

		ZPropertyInfo IMatching.InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceDate)); }
		}

		ZDateTime IMatching.DueDate
		{
			get { return AH_DueDate; }
		}

		ZPropertyInfo IMatching.DueDateInfo
		{
			get { return GetZPropertyInfo("DueDate"); }
		}

		ZString IMatching.VoyageVesselOrFlightDate
		{
			get
			{
				var result = ZString.Empty;
				var readonlyFactory = Factory.GetCachedReadOnlyFactory();

				if (AH_JH.IsValid)
				{
					var readonlyJob = readonlyFactory.Load<Job>(AH_JH);
					if (readonlyJob != null)
					{
						if (readonlyJob.Parent == null)
						{
							readonlyJob.InitializeParentFromGenericJobWithSettingDefaults();
						}
						var pluginData = readonlyJob.Parent as IJobInvoicingPlugIn;
						result = pluginData != null ? pluginData.InvoicingSupporter.VoyageVesselOrFlightDate : ZString.Empty;
					}
				}
				else if (IsConsolInvoice)
				{
					var consol = readonlyFactory.LoadFromUniqueKey<CommonConsol>(JobConsolSchema.JK_UniqueConsignRef, ConsolNumberFromConsolidatedInvoiceRef);
					var pluginData = consol as IJobInvoicingPlugIn;
					if (pluginData != null)
					{
						result = pluginData.InvoicingSupporter.VoyageVesselOrFlightDate;
					}
				}

				return result;
			}
		}

		ZString IMatching.ShipmentHouseBill
		{
			get
			{
				ZString result = ZString.Empty;
				if (InvoicingJob != null && InvoicingJob.Parent != null)
				{
					result = InvoicingJob.JH_HouseBillNo;
				}

				return result;
			}
		}

		ZString IMatching.ShipmentMasterBill
		{
			get
			{
				ZString result = ZString.Empty;
				if (InvoicingJob != null && InvoicingJob.Parent != null)
				{
					result = InvoicingJob.JH_MasterBillNo;
				}
				else if (AH_JH.IsEmpty && !AH_ConsolidatedInvoiceRef.IsEmpty)
				{
					ZString uniqueConsignRef = AH_ConsolidatedInvoiceRef.Left(9);
					CommonConsol consol = Factory.LoadTop1<CommonConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, uniqueConsignRef));
					if (consol != null)
					{
						result = consol.JK_MasterBillNum;
					}
				}
				return result;
			}
		}

		ZString IMatching.MatchStatus
		{
			get { return AH_MatchStatus; }
			set { AH_MatchStatus = value; }
		}

		ZPropertyInfo IMatching.MatchStatusInfo => GetZPropertyInfo("MatchStatus");

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList => AccountingUtils.GetMatchStatusList();

		ZString IMatching.MatchStatusReasonCode
		{
			get { return AH_MatchStatusReasonCode; }
			set { AH_MatchStatusReasonCode = value; }
		}

		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo => GetZPropertyInfo("MatchStatusReasonCode");

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList => AccountingUtils.GetMatchStatusReasonCodeList();

		ZDateTime IMatching.MatchDate
		{
			get
			{
				if (LatestMatchLink != null)
				{
					return LatestMatchLink.AP_MatchDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		ZPropertyInfo IMatching.MatchDateInfo
		{
			get { return GetZPropertyInfo("MatchDate"); }
		}

		ZString IMatching.InvoiceRemittanceReference => this.InvoiceRemittanceReference;

		OrgHeaderCollection IMatching.Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}

		ZString IMatching.RelatedDisbursementTransactions => base.RelatedDisbursementTransactions;

		public override bool OSPartialPaymentAmount_ReadOnly
		{
			get { return base.OSPartialPaymentAmount_ReadOnly || !ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount.IsEmpty && ThisAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty; }
		}

		OrgHeaderCollection fOrganisations;

		GlbBranchCollection IMatching.BranchCollection
		{
			get { return Lookups.Branches; }
		}

		GlbDepartmentCollection IMatching.DepartmentCollection
		{
			get { return Lookups.Departments; }
		}

		RefCurrencyCollection IMatching.Currencies
		{
			get { return Lookups.TransactionCurrencies; }
		}

		IMatching ThisAsIMatching
		{
			get { return this; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.NotionalWHTTax => AH_NotionalWHTTax;
		ZPropertyInfo IMatching.NotionalWHTTaxInfo => AH_NotionalWHTTaxInfo;
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.RealizedWHTTax => AH_RealizedWHTTax;
		ZPropertyInfo IMatching.RealizedWTHTaxInfo => AH_RealizedWHTTaxInfo;
		#endregion

		#region IReversing Members

		protected override bool InvertSignsOfOriginalTransactionOnReversing
		{
			get { return false; }
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			using (ReverseInvoice.GetSetGSTOnLinesSuspender())
			using (ReverseInvoice.Lines.SuspendListChanged())
			using (GetValidationSuspender())
			using (ReverseInvoice.GetValidationSuspender())
			{
				int reverseTransactionMultiplier = InvertSignsOfOriginalTransactionOnReversing ? -1 : 1;

				foreach (InvoicingLineBase line in Lines)
				{
					using (line.GetValidationSuspender())
					{
						InvoicingLineBase fReverseInvoiceLine = (InvoicingLineBase)ReverseInvoice.Lines.AddNew();
						try
						{
							using (fReverseInvoiceLine.GetValidationSuspender())
							{
								if (line.AL_RX_NKTransactionCurrency != AH_RX_NKTransactionCurrency)
								{
									fReverseInvoiceLine.AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
								}
								fReverseInvoiceLine.AL_PlaceOfSupply = line.AL_PlaceOfSupply;
								fReverseInvoiceLine.AL_ExchangeRate = line.AL_ExchangeRate;
								fReverseInvoiceLine.AL_GE = line.AL_GE;

								CopyJobHeader(line, fReverseInvoiceLine);
								CopyCharges(line, fReverseInvoiceLine);

								using (fReverseInvoiceLine.OnRateChangedSuspender.GetSuspender())
								{
									fReverseInvoiceLine.AL_AT = line.AL_AT;
									fReverseInvoiceLine.AL_TaxDate = line.AL_TaxDate;
									fReverseInvoiceLine.AL_TaxRateNumerator = line.AL_TaxRateNumerator;
									fReverseInvoiceLine.AL_TaxRateDenominator = line.AL_TaxRateDenominator;
									fReverseInvoiceLine.AL_TaxExtraRateNumerator = line.AL_TaxExtraRateNumerator;
									fReverseInvoiceLine.AL_TaxExtraRateDenominator = line.AL_TaxExtraRateDenominator;
								}
								fReverseInvoiceLine.AL_A9_VATClass = line.AL_A9_VATClass;
								fReverseInvoiceLine.AL_AW = line.AL_AW;
								fReverseInvoiceLine.AL_GB = line.AL_GB;
								fReverseInvoiceLine.AL_Sequence = line.AL_Sequence;

								fReverseInvoiceLine.AL_OSExTaxAmount = line.AL_OSExTaxAmount * reverseTransactionMultiplier;
								((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = true;
								try
								{
									fReverseInvoiceLine.AL_LocalExTaxAmount = line.AL_LocalExTaxAmount * reverseTransactionMultiplier;
								}
								finally
								{
									((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = false;
								}
								fReverseInvoiceLine.AL_OSTaxAmount = line.AL_OSTaxAmount * reverseTransactionMultiplier;

								((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = true;
								try
								{
									fReverseInvoiceLine.AL_LocalTaxAmount = line.AL_LocalTaxAmount * reverseTransactionMultiplier;
								}
								finally
								{
									((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = false;
								}

								fReverseInvoiceLine.AL_LocalExtraTaxAmount = line.AL_LocalExtraTaxAmount * reverseTransactionMultiplier;

								fReverseInvoiceLine.AL_OSWHTAmount = line.AL_OSWHTAmount * reverseTransactionMultiplier;

								((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = true;
								try
								{
									fReverseInvoiceLine.AL_LocalWHTAmount = line.AL_LocalWHTAmount * reverseTransactionMultiplier;
								}
								finally
								{
									((AccountingSuspenders.IRunMethodSuspending)fReverseInvoiceLine).RunMethodSuspended = false;
								}
								fReverseInvoiceLine.AL_RevRecognitionType = line.AL_RevRecognitionType;

								SubAccountHelper.CopySubAccounts(fReverseInvoiceLine, line, true);

								fReverseInvoiceLine.AL_GovtChargeCode = line.AL_GovtChargeCode;
								fReverseInvoiceLine.CopiedFromPK = line.PK;
								fReverseInvoiceLine.AL_SupplyType = line.AL_SupplyType;
								fReverseInvoiceLine.AL_GB_TaxBranch = line.AL_GB_TaxBranch;
							}
						}
						finally
						{
							fReverseInvoiceLine.ExchangeRate.SuspendValidation();
						}
					}
				}

				ReverseInvoice.AH_OSTaxAmountOtherTaxes = AH_OSTaxAmountOtherTaxes_ForDisplay * reverseTransactionMultiplier * ReverseInvoice.Multiplier;
				ReverseInvoice.AH_LocalTaxAmountOtherTaxes = AH_LocalTaxAmountOtherTaxes_ForDisplay * reverseTransactionMultiplier * ReverseInvoice.Multiplier;

				MustTransform = mustTransform;
			}
		}

		protected override void ApplyWorkflowTemplatesOnReverseTransactionCore()
		{
			base.ApplyWorkflowTemplatesOnReverseTransactionCore();
			ReverseInvoice.CreateTasksAndMilestonesFromTemplate();
		}

		protected bool MustTransform { get; private set; }

		public InvoicingBase ReverseInvoice
		{
			get { return fReverseTransaction as InvoicingBase; }
		}

		protected virtual void CopyJobHeader(InvoicingLineBase sourceLine, InvoicingLineBase destinationLine)
		{
			destinationLine.SuspendAL_JHSettingDefaults();

			try
			{
				destinationLine.AL_JH = sourceLine.AL_JH;
			}
			finally
			{
				destinationLine.ResumeAL_JHSettingDefaults();
			}
		}

		protected virtual void CopyCharges(InvoicingLineBase sourceLine, InvoicingLineBase destinationLine)
		{
			destinationLine.SuspendGenericChargeSettingDefaults();
			try
			{
				destinationLine.GenericCharge = sourceLine.GenericCharge;
			}
			finally
			{
				destinationLine.ResumeGenericChargeSettingDefaults();
			}

			using (GetSetGSTOnLinesSuspender())
			{
				destinationLine.AL_AC = sourceLine.AL_AC;
			}
			destinationLine.AL_AG = sourceLine.AL_AG;
			destinationLine.AL_Desc = sourceLine.AL_Desc;
			destinationLine.AL_InputGSTVATRecoverable = sourceLine.AL_InputGSTVATRecoverable;
		}

		#endregion

		#region IDocManagerSupport Members

		InvoicingDocManagerInfo docManagerInfo;

		public sealed override DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = GetNewDocManagerInfo();
					docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				}
				return docManagerInfo;
			}
		}

		protected virtual InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return new InvoicingDocManagerInfo(this, AH_Ledger);
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return InvoicingBaseDocumentSupporter.New(this); }
		}

		#endregion

		internal MultilingualString CustomWatermarkText { get; set; }

		#region Invoice Document Properties

		protected override AccBankAccount ReceiptBankAccountCore
		{
			get
			{
				ZGuid headerPK = Header != null ? Header.PK : ZGuid.Empty;

				return AccBankAccount.GetDefaultReceiptBankAccountForDebtor(headerPK, AH_RX_NKTransactionCurrency, Branch, Factory);
			}
		}

		#endregion

		#region Apportionment

		#region New Consol Costing

		APInvoiceConsolCosting fConsolCosting;
		public APInvoiceConsolCosting ConsolCosting
		{
			get
			{
				if (fConsolCosting == null)
				{
					fConsolCosting = new APInvoiceConsolCosting(Factory, this);
				}
				return fConsolCosting;
			}
		}

		public IDisposable GetConsolCostImportPopupSuspender()
		{
			return new ConsolCostImportPopupSuspender(this);
		}

		class ConsolCostImportPopupSuspender : IDisposable
		{
			public ConsolCostImportPopupSuspender(InvoicingBase parentInvoice)
			{
				this.parentInvoice = parentInvoice;
				parentInvoice.ConsolCostImportPopupSuspenderCount++;
			}

			readonly InvoicingBase parentInvoice;

			public void Dispose()
			{
				parentInvoice.ConsolCostImportPopupSuspenderCount--;
			}
		}

		public bool IsConsolCostImportPopupSuspended
		{
			get { return ConsolCostImportPopupSuspenderCount > 0; }
		}
		int ConsolCostImportPopupSuspenderCount;

		#region HeaderAmountsValidationSuspender

		internal FunctionalitySuspender ValidateAH_OSTotalAmountSuspenderForBatchLineChanges
		{
			get
			{
				return validateAH_OSTotalAmountSuspenderForBatchLineChanges ?? (validateAH_OSTotalAmountSuspenderForBatchLineChanges =
					new FunctionalitySuspender(() =>
					{
						if (InvoicingValidation != null)
						{
							InvoicingValidation.ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChanges();
						}
					},
					true));
			}
		}
		FunctionalitySuspender validateAH_OSTotalAmountSuspenderForBatchLineChanges;

		#endregion

		readonly List<JobConsolCost> ImportedConsolCosts = new List<JobConsolCost>();

#if DEBUG
		public int SetFinalFlagWhenImportingFromSplitChargeSuspender_IsSuspendedCountForTestOnly;
#endif

		public FunctionalitySuspender GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(bool needResumeAction = false)
		{
			return needResumeAction ? SetFinalFlagWhenImportingFromLinesSuspenderWithResumeAction : SetFinalFlagWhenImportingFromSplitChargeSuspenderWithoutResumeAction;
		}

		FunctionalitySuspender SetFinalFlagWhenImportingFromSplitChargeSuspenderWithoutResumeAction
		{
			get { return setFinalFlagWhenImportingFromSplitChargeSuspenderWithoutResumeAction ?? (setFinalFlagWhenImportingFromSplitChargeSuspenderWithoutResumeAction = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setFinalFlagWhenImportingFromSplitChargeSuspenderWithoutResumeAction;

		FunctionalitySuspender SetFinalFlagWhenImportingFromLinesSuspenderWithResumeAction
		{
			get { return setFinalFlagWhenImportingFromLinesSuspenderWithResumeAction ?? (setFinalFlagWhenImportingFromLinesSuspenderWithResumeAction = new FunctionalitySuspender(onResumeAction: () => AutoTickFinalFlagAfterImportCompleted())); }
		}
		FunctionalitySuspender setFinalFlagWhenImportingFromLinesSuspenderWithResumeAction;

		public FunctionalitySuspender SetDefaultFinalWhenAutoTickingFinalFlagSuspender
		{
			get { return setDefaultFinalWhenAutoTickingFinalFlagSuspender ?? (setDefaultFinalWhenAutoTickingFinalFlagSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setDefaultFinalWhenAutoTickingFinalFlagSuspender;

		void AutoTickFinalFlagAfterImportCompleted()
		{
			if (this is APInvoice apInvoice && Lines.Count > 0)
			{
				if (!apInvoice.CostVarianceApprovalHelper.AutoTickFinalFlag)
				{
					return;
				}

				apInvoice.CostVarianceApprovalHelper.CalculateAuthorisationByLines();

				using (SetDefaultFinalWhenAutoTickingFinalFlagSuspender.GetSuspender())
				{
					if (apInvoice.CostVarianceApprovalHelper.MonitorTotalInvoiceVariance)
					{
						AutoTickFinalFlagForLinesIfMonitorTotalInvoiceVariance();
					}
					else
					{
						foreach (APInvoiceLine line in Lines)
						{
							if (line.IsValidLineForCalculatingCostVarianceApproval && !IsDeleting)
							{
								try
								{
									using (line.StopChangingConsolRelatedLinesSuspender.GetSuspender())
									{
										line.IsSetAL_IsFinalChargeByGroup = true;
										line.AutoTickFinalFlagForEachLine();
									}
								}
								finally
								{
									line.IsSetAL_IsFinalChargeByGroup = false;
								}
							}
						}
					}
				}
			}
		}

		internal void AutoTickFinalFlagForLinesIfMonitorTotalInvoiceVariance()
		{
			if (this is APInvoice apInvoice)
			{
				var totalRequirement = apInvoice.CostVarianceApprovalHelper.GetTotalAuthorisationRequirement();
				var totalAppropvalLevel = totalRequirement != null ? totalRequirement.AuthorisationRequirement.ToString() : AuthorisationCodes.NoApprovalRequired;
				var untickAll = (totalAppropvalLevel != AuthorisationCodes.NoApprovalRequired);

				foreach (APInvoiceLine line in Lines)
				{
					if (line.IsValidLineForCalculatingCostVarianceApproval)
					{
						using (line.StopChangingConsolRelatedLinesSuspender.GetSuspender())
						{
							if (untickAll)
							{
								line.AL_IsFinalCharge = false;
								if (line.IsPopulatedFromImportedApportionment)
								{
									line.ApportionmentChargeImportedFrom.IsFinal = false;
								}
								continue;
							}

							line.AutoTickFinalFlagForEachLine();
						}
					}
				}
			}
		}

		public void ImportAllApportionmentsFromCosting()
		{
#if DEBUG
			IDisposable reportSuspender = null;
			if (Globals.IsTest)
			{
				reportSuspender = GetReportingDeletedApportionmentChargesSuspender();
			}
#endif
			ImportAllApportionmentsFromCostingCore();
			LoadInvoiceLineTaxSummaries();
#if DEBUG
			if (Globals.IsTest)
			{
				reportSuspender?.Dispose();
			}
#endif
		}

		public void ImportAllApportionmentsFromCostingCore()
		{
			try
			{
				if (ConsolCosting.ConsolCosts.Count >= 0)
				{
					IsImportingConsolApportionment = true;

					using (Lines.SuspendListChanged())
					using (GetValidationSuspender())
					using (GetValidationSuspenderForLines())
					using (GetConsolCostImportPopupSuspender())
					using (GetSetGSTOnLinesSuspender())
					using (GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().GetSuspender())
					{
						foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
						{
							ImportSingleCostCore(cost, null);
						}

						for (int index = ImportedConsolCosts.Count - 1; index >= 0; index--)
						{
							JobConsolCost cost = ImportedConsolCosts[index];
							if (!ConsolCosting.ConsolCosts.Contains(cost))
							{
								RemoveAllLinesRelatingToConsolCost(cost, new List<InvoicingLineBase>());
							}
						}
					}
				}
			}
			finally
			{
				IsImportingConsolApportionment = false;

				AdjustLocalRoundedValuesForImportedConsolCosts();
			}
		}

		/// <summary>
		/// Removes invoice/credit note lines that are imported from apportioned charges for a consol cost.
		/// </summary>
		/// <param name="cost">Cost PK to remove lines for</param>
		/// <param name="linesToExclude">Exclude lines, eg when user deletes a line that triggers this behaviour, standard delete behaviour on grid should handle this</param>
		public void RemoveAllLinesRelatingToConsolCost(JobConsolCost cost, List<InvoicingLineBase> linesToExclude)
		{
			using (GetReportingDeletedApportionmentChargesSuspender())
			using (Lines.SuspendListChanged())
			{
				var consolLines = GetLinesLinkedToConsolCost(cost);
				foreach (var line in consolLines)
				{
					if (!linesToExclude.Contains(line))
					{
						Lines.RemoveAndDelete(line);
					}
				}
			}
		}

		public HashSet<InvoicingLineBase> GetLinesLinkedToConsolCost(JobConsolCost cost)
		{
			var consolLines = new HashSet<InvoicingLineBase>();
			if (cost != null && Lines.Count > 0)
			{
				int lineCount = Lines.Count;
				for (int index = lineCount - 1; index >= 0; index--)
				{
					var line = Lines[index];
					if (cost.PK == line.ImportedApportionmentID)
					{
						consolLines.Add(line);
					}
				}
			}
			return consolLines;
		}

		public void RemoveAllLinesRelatingToConsolCost(ZGuid consolCostPK, List<InvoicingLineBase> linesToExclude)
		{
			RemoveAllLinesRelatingToConsolCost(ConsolCosting.ConsolCosts.FindByPK(consolCostPK) as JobConsolCost, linesToExclude);
		}

		public void ImportSingleCostAndRevalidateLines(JobConsolCost costToImport, InvoicingLineBase originatingLine)
		{
			ImportSingleCost(costToImport, originatingLine);

			if (!IsValidationSuspended)
			{
				Lines.RunPreSaveValidation();
			}
		}

		public void ImportSingleCost(JobConsolCost costToImport, InvoicingLineBase originatingLine)
		{
			try
			{
				IsImportingConsolApportionment = true;

				using (Lines.SuspendListChanged())
				using (GetValidationSuspender())
				using (GetValidationSuspenderForLines())
				using (GetConsolCostImportPopupSuspender())
				using (GetSetGSTOnLinesSuspender())
				using (GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender().GetSuspender())
				{
					ImportSingleCostCore(costToImport, originatingLine);
				}
			}
			finally
			{
				IsImportingConsolApportionment = false;

				AdjustLocalRoundedValuesForImportedConsolCosts();
			}
		}

		public IDisposable BeginImportingManyConsoleCosts() => SuspendAdjustLocalRoundedValuesForImportedConsolCosts.GetSuspender();

		void ImportSingleCostCore(JobConsolCost costToImport, InvoicingLineBase originatingLine)
		{
			if (costToImport.ApportionmentCharges.Count > 0)
			{
				AdjustLocalRoundedValuesForImportedConsolCost(costToImport);
				GSTInclusiveAmountNeedUpdate = true;
				try
				{
					List<ApportionSplitCharge> chargesToImport = new List<ApportionSplitCharge>();
					foreach (ApportionSplitCharge charge in costToImport.ApportionmentCharges)
					{
						if (charge.JR_OSCostAmt != 0)
						{
							chargesToImport.Add(charge);
						}
					}

					if (chargesToImport.Count > 0)
					{
						if (IsInvoiceApproving)
						{
							foreach (InvoicingLineBase line in Lines)
							{
								line.ImportFromApportionSplitCharge(line.ApportionmentChargeImportedFrom);
							}
						}
						else
						{
							if (ImportedConsolCosts.Contains(costToImport))
							{
								CacheIndexOfImportedUniversalTransactionLineValues();
								RemoveAllLinesRelatingToConsolCost(costToImport, new List<InvoicingLineBase>());
								ImportSingleCostCore(costToImport, null);
							}
							else
							{
								ImportedConsolCosts.Add(costToImport);
								int startIndex = 0;
								ApportionSplitCharge splitChargeToImport = null;
								if (originatingLine != null)
								{
									splitChargeToImport = chargesToImport[0];
									originatingLine.ImportFromApportionSplitCharge(splitChargeToImport);
									startIndex = 1;
								}

								for (int index = startIndex; index < chargesToImport.Count; index++)
								{
									InvoicingLineBase newLine = (InvoicingLineBase)Lines.AddNew();
									using (newLine.GetValidationSuspender())
									{
										splitChargeToImport = chargesToImport[index];
										newLine.ImportFromApportionSplitCharge(splitChargeToImport);
									}
								}
								RestoreIndexOfImportedUniversalTransactionLineValuesAndClearCache();
							}
						}
					}
				}
				finally
				{
					GSTInclusiveAmountNeedUpdate = false;
				}
			}
		}

		public void ValidateAndFixConsolCostsMarkedAsImported(JobConsolCost consolCost)
		{
			var listApportionCharge = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Where(x => x.JR_IsUsedForApportionment).ToList();
			if (consolCost.RelatedConsolCostPK.IsValid)
			{
				var relatedCost = Factory.Load<JobConsolCost>(consolCost.RelatedConsolCostPK);
				if (relatedCost == null || relatedCost.ApportionmentCharges.Count != listApportionCharge.Count || listApportionCharge.Any(x => x.RelatedApportionChargeFromDB == null))
				{
					consolCost.RelatedConsolCostPK = ZGuid.Empty;
				}
			}

			var apportionChargesWithRelatedCharge = listApportionCharge.Where(x => x.RelatedApportionChargeFromDB != null);
			if (!consolCost.RelatedConsolCostPK.IsValid || apportionChargesWithRelatedCharge.Any(x => x.RelatedApportionChargeFromDB.JR_E6 != consolCost.RelatedConsolCostPK))
			{
				consolCost.RelatedConsolCostPK = ZGuid.Empty;
				foreach (ApportionSplitCharge apportionChargeWithRelatedCharge in apportionChargesWithRelatedCharge)
				{
					var line = Lines.Cast<InvoicingLineBase>()
									.FirstOrDefault(l => l.OriginalJobCharge != null &&
														l.OriginalJobCharge.PK == apportionChargeWithRelatedCharge.RelatedApportionChargeFromDB.PK);
					if (line != null)
					{
						line.OriginalJobCharge = null;
					}
					apportionChargeWithRelatedCharge.RelatedApportionChargeFromDB = null;
				}
			}
		}

		void RestoreIndexOfImportedUniversalTransactionLineValuesAndClearCache()
		{
			if (indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob != null)
			{
				foreach (InvoicingLineBase line in Lines)
				{
					int indexOfImportedUniversalTransactionLine;
					if (indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob.TryGetValue(Tuple.Create(line.ImportedApportionmentID, line.AL_JH), out indexOfImportedUniversalTransactionLine))
					{
						line.IndexOfImportedUniversalTransactionLine = indexOfImportedUniversalTransactionLine;
					}
				}

				indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob = null;
			}
		}

		void CacheIndexOfImportedUniversalTransactionLineValues()
		{
			indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob = new Dictionary<Tuple<ZGuid, ZGuid>, int>();
			foreach (InvoicingLineBase line in Lines)
			{
				if (!line.ImportedApportionmentID.IsEmpty && !line.AL_JH.IsEmpty)
				{
					var key = Tuple.Create(line.ImportedApportionmentID, line.AL_JH);
					indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob[key] = line.IndexOfImportedUniversalTransactionLine;
				}
			}

			if (indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob.Count == 0)
			{
				indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob = null;
			}
		}

		Dictionary<Tuple<ZGuid, ZGuid>, int> indexOfImportedUniversalTransactionLineValuesPerConsolCostAndJob;

		#endregion

		bool fIsImportingJobCharges;
		bool fIsImportingAccruals;

		protected void AdjustLocalRoundedValuesForImportedConsolCosts()
			 => AdjustLocalRoundedValuesForImportedConsolCosts(ConsolCosting.ConsolCosts, GetLineToImportedChargeMapper());

		IReadOnlyDictionary<ZGuid, ApportionSplitCharge> GetLineToImportedChargeMapper()
			=> Lines
				.Cast<InvoicingLineBase>()
				.ToDictionary(x => x.PK, x => x.ApportionmentChargeImportedFrom);

		public void AdjustLocalRoundedValuesForImportedConsolCosts(BusinessObjectCollection<JobConsolCost> consolCosts, IReadOnlyDictionary<ZGuid, ApportionSplitCharge> mapperLineToApportionmentCharge)
		{
			if (SuspendAdjustLocalRoundedValuesForImportedConsolCosts.IsSuspended)
			{
				return;
			}

#if DEBUG
			CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly++;
#endif

			foreach (JobConsolCost cost in consolCosts)
			{
				InvoicingLineBase maxGSTLine = null;
				InvoicingLineBase maxLocalLine = null;
				ZDecimal gSTTotal = 0m;
				ZDecimal localTotal = 0m;
				foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
				{
					foreach (InvoicingLineBase line1 in Lines)
					{
						if (mapperLineToApportionmentCharge.TryGetValue(line1.PK, out ApportionSplitCharge targetCharge) && targetCharge != null && charge.PK == targetCharge.PK)
						{
							if ((maxGSTLine == null) || maxGSTLine.AL_LocalTaxAmount < line1.AL_LocalTaxAmount ||
								(maxGSTLine.AL_LocalExTaxAmount == line1.AL_LocalExTaxAmount && maxGSTLine.AL_OSExTaxAmount < line1.AL_OSExTaxAmount))
							{
								maxGSTLine = line1;
							}
							if ((maxLocalLine == null) || maxLocalLine.AL_LocalExTaxAmount < line1.AL_LocalExTaxAmount ||
								(maxLocalLine.AL_LocalExTaxAmount == line1.AL_LocalExTaxAmount && maxLocalLine.AL_OSExTaxAmount < line1.AL_OSExTaxAmount))
							{
								maxLocalLine = line1;
							}
							gSTTotal += line1.AL_LocalTaxAmount;
							localTotal += line1.AL_LocalExTaxAmount;
						}
					}
				}
				var localConsolCostTax = cost.E6_Calc_LocalGSTAmount;
				if (maxGSTLine != null && gSTTotal != localConsolCostTax)
				{
					try
					{
						((AccountingSuspenders.IRunMethodSuspending)maxGSTLine).RunMethodSuspended = true;
						maxGSTLine.AL_LocalTaxAmount -= (gSTTotal - localConsolCostTax);
					}
					finally
					{
						((AccountingSuspenders.IRunMethodSuspending)maxGSTLine).RunMethodSuspended = false;
					}
				}
				if (maxLocalLine != null && localTotal != cost.E6_LocalCostAmount && cost.E6_LocalCostAmount != ZDecimal.Zero)
				{
					try
					{
						((AccountingSuspenders.IRunMethodSuspending)maxLocalLine).RunMethodSuspended = true;
						maxLocalLine.AL_LocalExTaxAmount -= (localTotal - cost.E6_LocalCostAmount);
					}
					finally
					{
						((AccountingSuspenders.IRunMethodSuspending)maxLocalLine).RunMethodSuspended = false;
					}
				}
				if (cost.E6_LocalCostAmount == ZDecimal.Zero)
				{
					using (Lines.SuspendListChanged())
					{
						foreach (InvoicingLineBase line in Lines)
						{
							if (mapperLineToApportionmentCharge.TryGetValue(line.PK, out ApportionSplitCharge targetCharge) &&
								targetCharge != null &&
								targetCharge.JR_E6 == cost.PK &&
								line.AL_LocalExTaxAmount != ZDecimal.Zero)
							{
								line.AL_LocalExTaxAmount = ZDecimal.Zero;
							}
						}
					}
				}
			}
		}

		FunctionalitySuspender SuspendAdjustLocalRoundedValuesForImportedConsolCosts
			=> SuspendAdjustLocalRoundedValuesForImportedConsolCostsField
				?? (SuspendAdjustLocalRoundedValuesForImportedConsolCostsField = new FunctionalitySuspender(onResumeAction: () => this.AdjustLocalRoundedValuesForImportedConsolCosts()));
		FunctionalitySuspender SuspendAdjustLocalRoundedValuesForImportedConsolCostsField;

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Test only field used to assert specific performance characteristic in specific test cases")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211", Justification = "Test only field. Turning this into a property won't improve readability.")]
		public static int CallsToAdjustLocalRoundedValuesForImportedConsolCosts_TestOnly;

#endif

		protected void AdjustLocalRoundedValuesForImportedConsolCost(JobConsolCost cost)
		{
			BaseCharge maxLocalCharge = null;
			ZDecimal localTotal = 0m;

			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				if ((maxLocalCharge == null) || maxLocalCharge.JR_LocalCostAmt < charge.JR_LocalCostAmt ||
					(maxLocalCharge.JR_LocalCostAmt == charge.JR_LocalCostAmt && maxLocalCharge.JR_OSCostAmt < charge.JR_OSCostAmt))
				{
					maxLocalCharge = charge;
				}
				localTotal += charge.JR_LocalCostAmt;
			}

			if (maxLocalCharge != null && localTotal != cost.E6_LocalCostAmount && cost.E6_LocalCostAmount != ZDecimal.Zero)
			{
				maxLocalCharge.JR_LocalCostAmt -= (localTotal - cost.E6_LocalCostAmount);
			}
		}

		#region LineTotalPaidAmountPostedCalculator

		internal class LineTotalPaidAmountPostedCalculator : IService
		{
			internal LineTotalPaidAmountPostedCalculator()
			{
				Values = new Dictionary<ZGuid, ZDecimal>();
			}

			readonly Dictionary<ZGuid, ZDecimal> Values;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			public void AddNewElements(IEnumerable<ZGuid> pks)
			{
				var newPks = pks.Where(pk => !Values.ContainsKey(pk)).ToArray();
				if (newPks.Any())
				{
					const string SQL =
								@"SELECT
									AH_PK AS PK,
									SUM(A7_Amount) AS PaidAmount
								FROM dbo.AccTransactionHeader
								LEFT JOIN dbo.AccTransactionLines on AL_AH = AH_PK
								LEFT JOIN dbo.AccTransLinePay on A7_AL = AL_PK
								WHERE AH_PK IN (select value from @p)
								GROUP BY AH_PK";
					using (var cmd = Db.Connection.Command(SQL))
					{
						cmd.AddTableValuedParameter((NoResString)"@p", AccTransactionHeaderSchema.PK, newPks);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								Values.Add(reader.GetGuid(0), reader.IsDBNull(1) ? 0 : reader.GetDecimal(1));
							}
						}
					}
				}
			}

			public ZDecimal GetValueFromElement(ZGuid pk)
			{
				if (!Values.ContainsKey(pk))
				{
					AddNewElements(new ZGuid[] { pk });
				}

				ZDecimal result = 0;
				Values.TryGetValue(pk, out result);
				return result;
			}

			public static LineTotalPaidAmountPostedCalculator GetOrCreateNewInstance(BusinessObjectFactory factory)
			{
				LineTotalPaidAmountPostedCalculator lineTotalPaidAmountPostedCalculator = factory.ServiceContainer.GetService<LineTotalPaidAmountPostedCalculator>();
				if (lineTotalPaidAmountPostedCalculator == null)
				{
					factory.ServiceContainer.AddService(new LineTotalPaidAmountPostedCalculator());
					lineTotalPaidAmountPostedCalculator = factory.ServiceContainer.GetService<LineTotalPaidAmountPostedCalculator>();
				}
				return lineTotalPaidAmountPostedCalculator;
			}
		}

		#endregion

		#region JobHeaderReloader

#if DEBUG
		public static readonly Overridable<bool> UseSmallBatchSizeInTest = new Overridable<bool>(true);
#endif

		///// <summary>
		///// Helper class implementing group reload of JobHeaders that are in Database and not changed in the current factory
		///// Should do reload only once but for all JobHeaders requiring reload
		///// </summary>
		internal class JobHeaderReloader : IService
		{
			internal JobHeaderReloader(BusinessObjectFactory factory)
			{
				ReloadJobHeaders(factory);
			}

			int batchSize
			{
				get
				{
					return
#if DEBUG
	Globals.IsTest && UseSmallBatchSizeInTest.Value ? 5 :
#endif
	500;
				}
			}

			void ReloadJobHeaders(BusinessObjectFactory factory)
			{
				ZQuery cacheOnlyFilter = new ZQuery();
				cacheOnlyFilter.FetchOnlyFromLocalCache = true;
				JobHeader[] jobsInFactory = factory.Load<JobHeader>(cacheOnlyFilter);

				if (jobsInFactory.Any())
				{
					var reloadJobPKs = from JobHeader job in jobsInFactory where job.IsInDatabase && !job.HasChanges select job.PK;

					List<ZGuid> pks = reloadJobPKs.Distinct().ToList();
					for (int i = 0; i < pks.Count; i = i + batchSize)
					{
						ZGuid[] batch = new ZGuid[i <= pks.Count - batchSize ? batchSize : pks.Count % batchSize];
						pks.CopyTo(i, batch, 0, batch.Length);
						ZDBOnlyQuery reloadFilter = new ZDBOnlyQuery(typeof(JobHeader));
						reloadFilter.AddToFilter(JobHeaderSchema.PK, batch.ToArray());
						reloadFilter.ReLoadExistingRows = true;
						factory.Load<JobHeader>(reloadFilter);
					}
				}
			}
		}

		#endregion

		public void RemoveNotRequiredJobsAndChargesFromCostsAndLines()
		{
			HashSet<Job> jobsNotRequiredForPosting = new HashSet<Job>();

			foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
			{
				cost.RemoveNonApplicableCharges();
				cost.JobsWithMutexes.ForEach(j => jobsNotRequiredForPosting.Add(j));
			}

			LineJobsWithMutex.ForEach(j => jobsNotRequiredForPosting.Add(j));
			RemoveJobsCreatedButNotRequiredForPosting(jobsNotRequiredForPosting.ToList());
		}

		void RemoveJobsCreatedButNotRequiredForPosting(List<Job> jobsNotRequiredForPosting)
		{
			if (jobsNotRequiredForPosting.Count > 0)
			{
				for (int index = jobsNotRequiredForPosting.Count - 1; index >= 0; index--)
				{
					Job job = jobsNotRequiredForPosting[index];
					if (!job.IsInDatabase || job.IsJobActivating)
					{
						InvoicingLineBase[] jobRelatedLines = (InvoicingLineBase[])Lines.Find(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK));
						if (jobRelatedLines.Length == 0)
						{
							for (int chargeIndex = job.Charges.Count - 1; chargeIndex >= 0; chargeIndex--)
							{
								Charge charge = job.Charges[chargeIndex];
								if (charge != null && !charge.IsDeleted && charge.JR_OSCostAmt == ZDecimal.Zero && charge.JR_JH == job.PK)
								{
									charge.Delete();
								}
							}
							for (int exRateIndex = job.ExchangeRates.Count - 1; exRateIndex >= 0; exRateIndex--)
							{
								ExchangeRate rate = job.ExchangeRates[exRateIndex];
								if (rate != null)
								{
									rate.Delete();
								}
							}

							if (job.IsJobActivating)
							{
								job.MarkAsInactive();
							}
							else
							{
								job.Delete();
							}
						}
					}
				}
			}
		}

		void APInvoice_ApportionedLineRemoved(InvoicingLineBaseCollection sender, ApportionedLineRemovedEventArgs e)
		{
			InvoicingLineBase removedLine = e.RemovedLine;
			if (!removedLine.ImportedApportionmentID.IsEmpty)
			{
				JobConsolCost costBeingDeleted = (JobConsolCost)ConsolCosting.ConsolCosts.FindByPK(removedLine.ImportedApportionmentID);
				if (costBeingDeleted != null)
				{
					ImportedConsolCosts.Remove(costBeingDeleted);
					if (!IsImportingConsolApportionment)
					{
						if (ConsolCosting.ConsolCosts.Contains(costBeingDeleted))
						{
							ConsolCosting.ConsolCosts.RemoveAndDelete(costBeingDeleted);
						}
					}
				}
			}
		}

		public bool IsImportingConsolApportionment
		{
			get { return isImportingConsolApportionmentCounter > 0; }
			private set
			{
				isImportingConsolApportionmentCounter += value ? 1 : -1;
				if (isImportingConsolApportionmentCounter < 0)
				{
					isImportingConsolApportionmentCounter = 0;
				}
			}
		}
		int isImportingConsolApportionmentCounter;

		public void ImportAccrualsIntoInvoice(IEnumerable<BaseWIPAccrual> accrualsToImport, InvoicingLineBase originatingLine)
		{
			if (accrualsToImport != null && accrualsToImport.Any())
			{
				fIsImportingAccruals = true;
				try
				{
					using (GetValidationSuspender())
					{
						GSTInclusiveAmountNeedUpdate = true;
						try
						{
							using (Lines.SuspendListChanged())
							{
								// Pre-load related Charges
								var accrualPKs = from Accrual acr in accrualsToImport select acr.PK;
								int batchSize =
#if DEBUG
		Globals.IsTest ? 5 :
#endif
		500;

								List<Charge> relatedCharges = new List<Charge>();

								if (accrualPKs.Any())
								{
									List<ZGuid> pks = accrualPKs.ToList();
									for (int i = 0; i < pks.Count; i = i + batchSize)
									{
										ZGuid[] batch = new ZGuid[i <= pks.Count - batchSize ? batchSize : pks.Count % batchSize];
										pks.CopyTo(i, batch, 0, batch.Length);
										relatedCharges.AddRange(Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, batch)));
									}

									var shipmentPKs = (from Charge charge in relatedCharges where charge.Job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix select charge.Job.JH_ParentID).Distinct();

									if (shipmentPKs.Any())
									{
										pks = shipmentPKs.ToList();
										for (int i = 0; i < pks.Count; i = i + batchSize)
										{
											ZGuid[] batch = new ZGuid[i <= pks.Count - batchSize ? batchSize : pks.Count % batchSize];
											pks.CopyTo(i, batch, 0, batch.Length);
											Factory.Load<CommonShipment>(new ZQuery(JobShipmentSchema.PK, batch));
										}
									}
								}

								InvoicingLineBase line = originatingLine;
								bool isFirstAccrual = true;

								foreach (Accrual accrualToImport in accrualsToImport)
								{
									if (!isFirstAccrual)
									{
										line = Lines.AddNew(LineType) as InvoicingLineBase;
									}
									else
									{
										isFirstAccrual = false;
									}

									using (line.GetValidationSuspender())
									{
										line.AL_JH = accrualToImport.AL_JH;
										line.GenericCharge = accrualToImport.AL_AC;

										line.AL_GB = accrualToImport.AL_GB;
										line.AL_GE = accrualToImport.AL_GE;

										ImportAccrualAmountIntoLine(accrualToImport, line);

										line.OriginalAccrual = accrualToImport;
										line.CalculateGST(IsApportionmentGSTMandatory);
										SetGSTAmountFromChargeGSTAmount(accrualToImport, line);
									}
								}
							}
						}
						finally
						{
							GSTInclusiveAmountNeedUpdate = false;
						}
					}
				}
				finally
				{
					fIsImportingAccruals = false;
				}
			}
		}

		protected void ImportAccrualAmountIntoLine(Accrual accrl, InvoicingLineBase line)
		{
			if (!AH_RX_NKTransactionCurrency.IsEmpty &&
				AH_ExchangeRate != accrl.AL_ExchangeRate && AH_ExchangeRate != 0M)
			{
				line.AL_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				line.SetExchangeRate();
				line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(accrl.AL_OSExTaxAmount, line.AL_ExchangeRate, AH_RX_NKTransactionCurrency);
			}
			else // use local currency
			{
				line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(accrl.AL_OSExTaxAmount, 1M, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		void SetGSTAmountFromChargeGSTAmount(Accrual accrual, InvoicingLineBase invoiceLine)
		{
			Charge relatedCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, accrual.PK));
			if (relatedCharge != null && ShouldUseChargesGSTAmount(relatedCharge))
			{
				invoiceLine.AL_AT = relatedCharge.JR_AT_CostGSTRate;
				invoiceLine.SetTaxDateSafe(relatedCharge.JR_CostTaxDate);
				invoiceLine.AL_A9_VATClass = relatedCharge.JR_A9_CostVATClass;
				ZDecimal localGSTAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(relatedCharge.JR_OSCostGSTAmt_Calc, relatedCharge.JR_OSCostExRate);
				invoiceLine.AL_OSTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(localGSTAmount, AH_ExchangeRate, invoiceLine.AL_RX_NKTransactionCurrency);
			}
		}

		public void ImportJobChargesIntoInvoice(IEnumerable<Charge> jobChargesToImport, InvoicingLineBase originatingLine)
		{
			ImportJobChargesIntoInvoice(jobChargesToImport, originatingLine, true);
		}

		public void ImportJobChargesIntoInvoice(IEnumerable<Charge> jobChargesToImport, InvoicingLineBase originatingLine, bool removeFirstJobCharge)
		{
			if (jobChargesToImport != null && jobChargesToImport.Any())
			{
				fIsImportingJobCharges = true;
				using (Lines.SuspendListChanged())
				{
					try
					{
						using (GetValidationSuspender())
						{
							PreImportJobChargesValidation(jobChargesToImport);
							GSTInclusiveAmountNeedUpdate = true;
							try
							{
								var jobChargesToImportList = jobChargesToImport.ToList();
								Charge firstJobCharge = jobChargesToImportList[0];

								if (removeFirstJobCharge)
								{
									jobChargesToImportList.Remove(firstJobCharge);
								}
								else
								{
									originatingLine.AL_JH = firstJobCharge.JR_JH;
								}

								originatingLine.GenericCharge = firstJobCharge.JR_AC;
								originatingLine.UpdateAPTaxDateForOperationalJob();
								originatingLine.AppendCostReferenceToTheLineDescription(firstJobCharge);

								originatingLine.AL_GB = firstJobCharge.JR_GB;
								originatingLine.AL_GE = firstJobCharge.JR_GE;
								originatingLine.AL_PlaceOfSupply = firstJobCharge.JR_CostPlaceOfSupply;
								originatingLine.AL_SupplyType = firstJobCharge.JR_CostSupplyType;
								if (CanApplyTaxBranch)
								{
									originatingLine.AL_GB_TaxBranch = AH_GB_TaxBranch.IsValid ? AH_GB_TaxBranch : firstJobCharge.JR_GB_CostTaxBranch;
								}

								ImportJobChargeAmountIntoLine(firstJobCharge, originatingLine);

								originatingLine.OriginalJobCharge = firstJobCharge;
								originatingLine.CalculateGST(IsApportionmentGSTMandatory);
								SetGSTAmountFromChargeGSTAmount(firstJobCharge, originatingLine);

								ImportJobChargeGovtChargeCodeIntoLine(firstJobCharge, originatingLine);
								ImportWithholdingTax(firstJobCharge, originatingLine);

								originatingLine.OriginalJobCharge = firstJobCharge;

								foreach (Charge jobChargeToImport in jobChargesToImportList)
								{
									if (jobChargeToImport != firstJobCharge)
									{
										InvoicingLineBase line = Lines.AddNew(LineType) as InvoicingLineBase;
										using (line.GetValidationSuspender())
										{
											line.AL_JH = jobChargeToImport.JR_JH;
											line.GenericCharge = jobChargeToImport.JR_AC;
											line.UpdateAPTaxDateForOperationalJob();
											line.AppendCostReferenceToTheLineDescription(jobChargeToImport);

											line.AL_GB = jobChargeToImport.JR_GB;
											line.AL_GE = jobChargeToImport.JR_GE;
											line.AL_PlaceOfSupply = jobChargeToImport.JR_CostPlaceOfSupply;
											line.AL_SupplyType = jobChargeToImport.JR_CostSupplyType;
											if (CanApplyTaxBranch)
											{
												line.AL_GB_TaxBranch = AH_GB_TaxBranch.IsValid ? AH_GB_TaxBranch : jobChargeToImport.JR_GB_CostTaxBranch;
											}

											ImportJobChargeAmountIntoLine(jobChargeToImport, line);

											line.OriginalJobCharge = jobChargeToImport;
											line.CalculateGST(IsApportionmentGSTMandatory);
											SetGSTAmountFromChargeGSTAmount(jobChargeToImport, line);

											ImportJobChargeGovtChargeCodeIntoLine(jobChargeToImport, line);
											ImportWithholdingTax(jobChargeToImport, line);
										}
									}
								}
							}
							finally
							{
								GSTInclusiveAmountNeedUpdate = false;
							}
						}
					}
					finally
					{
						fIsImportingJobCharges = false;
					}
				}

				if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<InvoicingBaseLineImporter.FunctionalitySuspender>.IsSuspended(Factory))
				{
					originatingLine.Validation.ValidateAll();
				}

				LoadInvoiceDependentJobs();
				LoadInvoiceLineTaxSummaries();
			}
		}

		public void PreImportJobChargesValidation(IEnumerable<Charge> jobChargesToImport)
		{
			CheckForCashAdvanceCurrency(jobChargesToImport);
		}

		void CheckForCashAdvanceCurrency(IEnumerable<Charge> jobChargesToImport)
		{
			var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled && this is APInvoice apInvoice)
			{
				var errorMessages = new ZStringBuilder();
				var errorMessageList = new Dictionary<string, List<string>>();
				var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessageList);
				validator.Visit(apInvoice, jobChargesToImport);
				foreach (var errKvp in errorMessageList)
				{
					errorMessages.Append(errKvp.Key);
					errorMessages.Append(string.Join(System.Environment.NewLine, string.Join(System.Environment.NewLine, errKvp.Value)));
				}

				var fullErrorMessage = errorMessages.ToStringWithNewLineBetweenAppends();
				if (!string.IsNullOrEmpty(fullErrorMessage))
				{
					throw new CannotGenerateCashAdvanceJournalException(null, fullErrorMessage);
				}
			}
		}

		void SetGSTAmountFromChargeGSTAmount(Charge jobCharge, InvoicingLineBase invoiceLine)
		{
			if (jobCharge != null && ShouldUseChargesGSTAmount(jobCharge))
			{
				invoiceLine.AL_AT = jobCharge.JR_AT_CostGSTRate;
				if (jobCharge.JR_CostTaxDate.IsValid)
				{
					invoiceLine.SetTaxDateSafe(jobCharge.JR_CostTaxDate);
				}
				invoiceLine.AL_A9_VATClass = jobCharge.JR_A9_CostVATClass;
				invoiceLine.AL_OSTaxAmount = invoiceLine.CalculateExpectedOSTaxAmount();
			}
		}

		bool ShouldUseChargesGSTAmount(Charge relatedCharge)
		{
			return Header != null && Header.CompanyData.IsAPTaxApplicable &&
				relatedCharge != null && relatedCharge.JR_OH_CostAccount == AH_OH;
		}

		Type LineType
		{
			get
			{
				if (Ledger == LedgerTypes.AccountsPayable)
				{
					if (TransactionType == TransactionTypes.Invoice)
					{
						return typeof(APInvoiceLine);
					}
					else if (TransactionType == TransactionTypes.CreditNote)
					{
						return typeof(APCreditNoteLine);
					}
					else if (TransactionType == TransactionTypes.AdjustmentNote)
					{
						return typeof(APAdjustmentNoteLine);
					}
				}
				else if (Ledger == LedgerTypes.AccountsReceivable)
				{
					if (TransactionType == TransactionTypes.Invoice)
					{
						return typeof(ARInvoiceLine);
					}
					else if (TransactionType == TransactionTypes.CreditNote)
					{
						return typeof(ARCreditNoteLine);
					}
					else if (TransactionType == TransactionTypes.AdjustmentNote)
					{
						return typeof(ARAdjustmentNoteLine);
					}
				}
				else if (Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					if (TransactionType == TransactionTypes.UAInvoice)
					{
						return typeof(UAInvoiceLine);
					}
					else if (TransactionType == TransactionTypes.UACreditNote)
					{
						return typeof(UACreditNoteLine);
					}
				}

				return typeof(AccTransactionLines);
			}
		}

		public bool DoOtherLinesExistForImportedApportionment(ZGuid apportionmentID, IEnumerable<BusinessObject> selectedLines)
		{
			JobConsolCost cost = (JobConsolCost)ConsolCosting.ConsolCosts.FindByPK(apportionmentID);
			var selectedInvoiceLines = new HashSet<BusinessObject>(selectedLines);
			if (cost != null)
			{
				foreach (InvoicingLineBase line in Lines)
				{
					if (line.ApportionmentChargeImportedFrom != null && line.ApportionmentChargeImportedFrom.ParentConsolCost != null)
					{
						if (cost.ApportionmentCharges.Contains(line.ApportionmentChargeImportedFrom))
						{
							if (!selectedInvoiceLines.Contains(line))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		protected void ImportJobChargeAmountIntoLine(Charge charge, InvoicingLineBase line)
		{
			if (!AH_RX_NKTransactionCurrency.IsEmpty && AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && (!AH_ExchangeRate.IsEmpty || UseJobExchangeRate))
			{
				line.AL_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				line.SetExchangeRate();

				if (UseJobExchangeRate)
				{
					if (AH_RX_NKTransactionCurrency == charge.JR_RX_NKCostCurrency)
					{
						line.AL_OSExTaxAmount = charge.JR_OSCostAmt * -Multiplier;
					}
					else
					{
						ZDecimal localUnRoundedAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(charge.JR_OSCostAmt, charge.JR_OSCostExRate);
						line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(localUnRoundedAmount, line.AL_ExchangeRate, AH_RX_NKTransactionCurrency) * -Multiplier;
					}

					if (line.AL_ExchangeRate == charge.JR_OSCostExRate) // Do not override Local Amount if Exchange Rates are different
					{
						try
						{
							((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = true;
							line.AL_LocalExTaxAmount = charge.JR_LocalCostAmt * -Multiplier;
						}
						finally
						{
							((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = false;
						}
					}
				}
				else if (AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Value && charge.JR_RX_NKCostCurrency == line.AL_RX_NKTransactionCurrency)
				{
					line.AL_OSExTaxAmount = charge.JR_OSCostAmt * -Multiplier;
				}
				else
				{
					line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(charge.JR_LocalCostAmt, line.AL_ExchangeRate, AH_RX_NKTransactionCurrency) * -Multiplier;
				}
			}
			else if (!AH_RX_NKTransactionCurrency.IsEmpty && AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && AH_ExchangeRate.IsEmpty)
			{
				if (AH_RX_NKTransactionCurrency != charge.JR_RX_NKCostCurrency)
				{
					line.AL_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				}
			}
			else if (!AH_RX_NKTransactionCurrency.IsEmpty && AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				line.AL_RX_NKTransactionCurrency = charge.JR_RX_NKCostCurrency;

				var isCreditorInvoicedExRateApplied = line.Factory.ServiceContainer.GetService<CreditorInvoicedExchangeRateProvider>()?.TrySetCreditorInvoicedExchangeRate(line) ?? false;

				if (!isCreditorInvoicedExRateApplied && (
					line.AL_RX_NKTransactionCurrency == line.Company.GC_RX_NKLocalCurrency || !ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC)))
				{
					line.AL_ExchangeRate = charge.JR_OSCostExRate;
				}

				line.AL_OSExTaxAmount = charge.JR_OSCostAmt * -Multiplier;
			}
			else // use local currency
			{
				line.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(charge.JR_LocalCostAmt, 1M, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency) * -Multiplier;
				line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		void ImportJobChargeGovtChargeCodeIntoLine(Charge charge, InvoicingLineBase line)
		{
			if (Ledger == LedgerTypes.AccountsPayable
				|| Ledger == LedgerTypes.IncompleteTransactions
				|| Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				line.AL_GovtChargeCode = charge.JR_CostGovtChargeCode;
			}
			else if (Ledger == LedgerTypes.AccountsReceivable)
			{
				line.AL_GovtChargeCode = charge.JR_SellGovtChargeCode;
			}
		}

		protected ZBool IsApportionmentGSTMandatory
		{
			get { return (Header?.CompanyData?.IsAPTaxApplicable ?? false) && GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		void ImportWithholdingTax(Charge charge, InvoicingLineBase line)
		{
			if (charge.JR_OH_CostAccount == AH_OH
				&& GlbCompany.CurrentCompany.GC_IsWHTRegistered
					&& IsMiscServWHTApplicable
						&& !charge.JR_AW_CostWHTRate.IsEmpty)
			{
				line.AL_AW = charge.JR_AW_CostWHTRate;
			}
		}

		public bool IsImportingAccruals
		{
			get { return fIsImportingAccruals; }
		}

		public bool IsImportingJobCharges
		{
			get { return fIsImportingJobCharges; }
		}

		public void ClearApportionmentJobMutexes()
		{
			foreach (JobConsolCost cost in ConsolCosting.ConsolCosts)
			{
				cost.CalculationStrategy.ReleaseMutexes();
			}
		}

		#endregion

		#region Validation

		public InvoiceBaseValidation InvoicingValidation
		{
			get { return Validation as InvoiceBaseValidation; }
		}

		public InvoicingBaseReversalValidation TransactionReversalValidation
		{
			get { return Validation as InvoicingBaseReversalValidation; }
		}

		#region Duplicate Lines Sequence Validation

		public void InitializeDuplicateLinesSequenceLookup()
		{
			duplicateLineSequenceLookup = new Dictionary<Tuple<ZGuid, ZShort>, List<ZGuid>>();  // <JobPK, Sequence#> => List<InvoiceLinePK>
			foreach (InvoicingLineBase line in Lines)
			{
				var key = new Tuple<ZGuid, ZShort>(line.AL_JH, line.AL_Sequence);
				if (!duplicateLineSequenceLookup.ContainsKey(key)) // key not found
				{
					duplicateLineSequenceLookup.Add(key, new List<ZGuid>() { line.PK });
				}
				else if (!duplicateLineSequenceLookup[key].Contains(line.PK)) // key found, but value list does not contain line PK
				{
					duplicateLineSequenceLookup[key].Add(line.PK);
				}
			}
		}

		public bool IsDuplicateLineSequenceDetected(Tuple<ZGuid, ZShort> key)
		{
			bool result = false;
			if (duplicateLineSequenceLookup != null && duplicateLineSequenceLookup.ContainsKey(key) && duplicateLineSequenceLookup[key].Count > 1)
			{
				result = true;
			}
			return result;
		}

		Dictionary<Tuple<ZGuid, ZShort>, List<ZGuid>> duplicateLineSequenceLookup;

		#endregion

		protected override AccTransactionHeaderCriticalValidation GetCriticalValidation()
		{
			return new InvoicingBaseCriticalValidation(this);
		}

		internal IDisposable GetValidationSuspenderForLines() => IsLinesLoaded ? new DisposableAction(() => Lines.SuspendValidation(), () => Lines.ResumeValidation()) : DisposableAction.NoAction;
		internal IDisposable GetValidationSuspenderForJobs()
		{
			if (IsLinesLoaded)
			{
				return new DisposableList(Lines.Cast<InvoicingLineBase>().Where(x => x.Job != null).Select(x => x.Job.GetValidationSuspender()));
			}

			return DisposableAction.NoAction;
		}

		protected override AccTransactionHeaderValidation GetNewEmptyValidation() => new InvoicingBaseEmptyValidation(this);

		#endregion

		#region Tax Amount Apportionment

		protected override void UpdateAH_OSTaxAmountCore(ZDecimal defaultNewAmount)
		{
			if (ShouldFixTaxAmountOnHeader)
			{
				using (GetHeaderTaxAmountCalculationSuspender())
				{
					if (!IsReverseTransaction)
					{
						ReCalculateAndAdjustLineTaxAmountsToCorrectHeadersTaxAmount();
					}
					base.UpdateAH_OSTaxAmountCore(0m);
					AH_OSTaxAmountInfo.RefreshBinding();
				}
			}
			else
			{
				base.UpdateAH_OSTaxAmountCore(defaultNewAmount);
			}
		}

		public override void UpdateAH_OSExtraTaxAmount()
		{
			if (ShouldFixTaxAmountOnHeader)
			{
				using (GetHeaderTaxAmountCalculationSuspender())
				{
					if (!IsReverseTransaction)
					{
						ReCalculateAndAdjustLineTaxAmountsToCorrectHeadersTaxAmount();
					}
					base.UpdateAH_OSExtraTaxAmount();
					AH_OSTaxAmountInfo.RefreshBinding();
				}
			}
			else
			{
				base.UpdateAH_OSExtraTaxAmount();
			}
		}

		public bool ShouldFixTaxAmountOnHeader
		{
			get
			{
				return GlbCompany.CurrentCompany.LocalCurrency != null
					&& AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable
					&& ShouldFixTaxAmountOnHeaderCore && !IsHeaderTaxAmountCalculationSuspended;
			}
		}

		bool ShouldFixTaxAmountOnHeaderCore
		{
			get { return RegistryValueForCalculateTaxAtHeaderLevel || IsIcelandicCurrencyAndCountry || HasWarehousePeriodicBillingJob; }
		}

		bool HasWarehousePeriodicBillingJob
		{
			get { return InvoicingJob != null && InvoicingJob.IsWarehousePeriodicBilling; }
		}

		bool IsIcelandicCurrencyAndCountry
		{
			get { return (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland) && (AH_RX_NKTransactionCurrency == Core.Constants.CurrencyCodes.Iceland); }
		}

		public bool RegistryValueForCalculateTaxAtHeaderLevel
		{
			get
			{
				Guid companyPK = (Branch != null && Branch.Company != null) ? Branch.Company.PK.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid();
				return AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			}
		}

		void ReCalculateAndAdjustLineTaxAmountsToCorrectHeadersTaxAmount()
		{
			if (!IsInDatabase && TransactionCurrency != null)
			{
				List<IReceivablesTaxAmountCalculation> charges = new List<IReceivablesTaxAmountCalculation>();

				foreach (InvoicingLineBase line in Lines)
				{
					charges.Add(line);
				}

				int roundingPrecision = IsIcelandicCurrencyAndCountry ? 0 : TransactionCurrency.Decimals;
				HeaderTaxAmountCalculator headerTaxAmountCalc = new HeaderTaxAmountCalculator(charges, roundingPrecision);
				if (HasWarehousePeriodicBillingJob)
				{
					headerTaxAmountCalc.AdjustAgainstAllCharges(AH_GC);
				}
				else
				{
					headerTaxAmountCalc.AdjustAgainstLargestCharge(AH_GC);
				}
			}
		}

		#region Header Tax Amount Calculation Suspender

		protected bool IsHeaderTaxAmountCalculationSuspended;

		public HeaderTaxAmountCalculationSuspender GetHeaderTaxAmountCalculationSuspender()
		{
			return new HeaderTaxAmountCalculationSuspender(this);
		}

		public class HeaderTaxAmountCalculationSuspender : IDisposable
		{
			public HeaderTaxAmountCalculationSuspender(InvoicingBase parent)
			{
				parent.IsHeaderTaxAmountCalculationSuspended = true;
				this.Parent = parent;
			}

			readonly InvoicingBase Parent;

			void IDisposable.Dispose()
			{
				Parent.IsHeaderTaxAmountCalculationSuspended = false;
			}
		}

		#endregion

		#endregion

		#region Invoice Amount Levels

		public bool LevelAuthorizationRequired
		{
			get
			{
				var result = GetLevelAuthorizationRequired(AuthorisationRequired);
				if (!result)
				{
					foreach (var transaction in TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation)
					{
						if (IsCreatingCreditNoteForReversal)
						{
							transaction.IsCreatingCreditNoteForReversal = true;
						}

						if (transaction.EnforceTwoApproversWhenPostingARCredit || transaction.EnforceSequentialApproversWhenPostingARCredit)
						{
							result = true;
							break;
						}

						if (transaction.GetLevelAuthorizationRequired(transaction.AuthorisationRequired))
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public bool GetLevelAuthorizationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			var levels = new[]
			{
				AuthorisationCodes.FirstApprovalRequiredOnly,
				AuthorisationCodes.SecondApprovalRequiredOnly,
				AuthorisationCodes.ThirdApprovalRequiredOnly,
				AuthorisationCodes.FourthApprovalRequiredOnly,
				AuthorisationCodes.FifthApprovalRequiredOnly,
				AuthorisationCodes.SixthApprovalRequiredOnly,
			};

			if (authorisationRequirement == null || !levels.Contains<string>(authorisationRequirement.AuthorisationRequirement))
			{
				return false;
			}

			//if (EnforceSequentialApproversWhenPostingARCredit)
			//{
			//	return true;
			//}
			//else
			//{
			var requiredLevel = authorisationRequirement.AuthorisationRequirement;
			var requiredCheckPoint = RetrieveLevelApprovalCheckPoint(requiredLevel);
			return requiredCheckPoint != null && !requiredCheckPoint.IsAllowed;
			//}
		}

		public List<InvoicingBase> TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation
		{
			get
			{
				return RemoveDuplicateBranchDepartment(TransactionsForAuthorisationCalculation);
			}
		}

		public List<InvoicingBase> TransactionsForAuthorisationCalculation
		{
			get
			{
				if (transactionsForAuthorisationCalculation == null)
				{
					transactionsForAuthorisationCalculation = new List<InvoicingBase>();
				}

				return transactionsForAuthorisationCalculation;
			}
			set
			{
				transactionsForAuthorisationCalculation = value;
			}
		}
		List<InvoicingBase> transactionsForAuthorisationCalculation;

		public virtual bool CheckLevelSecurityRights()
		{
			bool result = true;
			if (!UserHasAuthoriseLevel1Security || !UserHasAuthoriseLevel2Security)
			{
				AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement = AuthorisationRequired;
				if (!UserHasAuthoriseLevel1Security && Level1AuthorisationRequired(authorisationRequirement))
				{
					result = SecurityOverrideProvider.SecurityCertificates[FirstApprovalCheckpoint].IsAllowed;
				}
				if (!UserHasAuthoriseLevel2Security && Level2AuthorisationRequired(authorisationRequirement))
				{
					result &= SecurityOverrideProvider.SecurityCertificates[SecondApprovalCheckpoint].IsAllowed;
				}
			}
			return result;
		}

		public AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequired
		{
			get { return AuthorisationRequiredCore(); }
		}

		protected virtual AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore()
		{
			return AuthorisationRequiredCore(AH_LocalTotalAmount);
		}

		protected virtual AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore(ZDecimal localAmount)
		{
			AmountBasedMultiLevelAuthorisationRequirement result = null;

			if (localAmount != 0)
			{
				var registryValue = AuthorizationSettingsCollection;
				if (registryValue != null)
				{
					result = registryValue.Cast<AmountBasedMultiLevelAuthorisationRequirement>().GetAuthorisationRequired(localAmount);
				}
			}

			return result;
		}

		protected AuthorizationModeAndSettings AuthorizationModeAndSettings
		{
			get
			{
				var (branchPK, departmentPK) = AuthorizationModeAndSettingsRegistryFallback;
				return GetAuthorizationModeAndSettings(branchPK, departmentPK);
			}
		}

		public AuthorizationModeAndSettings GetAuthorizationModeAndSettings(ZGuid branchPK, ZGuid departmentPK)
		{
			Guid toGuid(ZGuid pk) => pk.IsValid ? pk.ToGuid() : Guid.Empty;
			return AuthorizationModeAndSettingsRegistry?.GetFallBackValueAtAllLevels(Guid.Empty, toGuid(branchPK), toGuid(departmentPK));
		}

		protected virtual (ZGuid branchPK, ZGuid departmentPK) AuthorizationModeAndSettingsRegistryFallback => (AH_GB, AH_GE);
		protected abstract AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry { get; }

		public bool EnforceTwoApproversWhenPostingARCredit => (AuthorizationModeAndSettings?.AuthorizationMode ?? ZString.Empty) == AuthorizationMode.Codes.TwoApprovers;
		public bool EnforceSequentialApproversWhenPostingARCredit => (AuthorizationModeAndSettings?.AuthorizationMode ?? ZString.Empty) == AuthorizationMode.Codes.SequentialApprovers;
		protected virtual AmountBasedAuthorisationRequirementCollection AuthorizationSettingsCollection => AuthorizationModeAndSettings?.AuthorisationSettings;

		protected SecurityCheckpoint GetCheckPointForLevel(string levelCode)
		{
			if (Branch != null && Branch.PK.IsValid && Department != null && Department.PK.IsValid &&
				(Branch.PK != GlbBranch.CurrentBranch.PK || Department.PK != GlbDepartment.CurrentDepartment.PK))
			{
				var loginController = new UserLoginController();
				var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, Branch.PK.ToGuid(), Department.PK.ToGuid());
				return GetCheckPointForLevelFromSecurityCore(userSecurity, levelCode);
			}
			else
			{
				return GetCheckPointForLevelFromSecurityCore(Env.Security, levelCode);
			}
		}

		protected SecurityCheckpoint GetCheckPointForLevelFromSecurityCore(SecurityCore securityCore, string levelCode)
		{
			switch (levelCode)
			{
				case AuthorisationCodes.FirstApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalFirstLevelApproval;
				case AuthorisationCodes.SecondApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalSecondLevelApproval;
				case AuthorisationCodes.ThirdApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalThirdLevelApproval;
				case AuthorisationCodes.FourthApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalFourthLevelApproval;
				case AuthorisationCodes.FifthApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalFifthLevelApproval;
				case AuthorisationCodes.SixthApprovalRequiredOnly:
					return securityCore.CreditAdjustmentNotePostingApprovalSixthLevelApproval;
				default:
					return null;
			}
		}

		#region Level 1

		protected bool Level1AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			ZString authorisationType = authorisationRequirement != null ? authorisationRequirement.AuthorisationRequirement : ZString.Empty;
			return authorisationType == AuthorisationCodes.FirstApprovalRequiredOnly;
		}

		protected bool UserHasAuthoriseLevel1Security
		{
			get { return FirstApprovalCheckpoint?.IsAllowed ?? false; }
		}

		protected SecurityCheckpoint FirstApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.FirstApprovalRequiredOnly);

		#endregion

		#region Level 2

		protected bool Level2AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			ZString authorisationType = authorisationRequirement != null ? authorisationRequirement.AuthorisationRequirement : ZString.Empty;
			return authorisationType == AuthorisationCodes.SecondApprovalRequiredOnly;
		}

		protected bool UserHasAuthoriseLevel2Security
		{
			get { return SecondApprovalCheckpoint?.IsAllowed ?? false; }
		}

		protected SecurityCheckpoint SecondApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.SecondApprovalRequiredOnly);

		#endregion

		#region Level 3

		protected bool Level3AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return GetAuthorisationCodeFromRequirement(authorisationRequirement) == AuthorisationCodes.ThirdApprovalRequiredOnly;
		}

		protected SecurityCheckpoint ThirdApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.ThirdApprovalRequiredOnly);

		#endregion

		#region Level 4

		protected bool Level4AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return GetAuthorisationCodeFromRequirement(authorisationRequirement) == AuthorisationCodes.FourthApprovalRequiredOnly;
		}

		protected SecurityCheckpoint FourthApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.FourthApprovalRequiredOnly);

		#endregion

		#region Level 5

		protected bool Level5AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return GetAuthorisationCodeFromRequirement(authorisationRequirement) == AuthorisationCodes.FifthApprovalRequiredOnly;
		}

		protected SecurityCheckpoint FifthApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.FifthApprovalRequiredOnly);

		#endregion

		#region Level 6

		protected bool Level6AuthorisationRequired(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return GetAuthorisationCodeFromRequirement(authorisationRequirement) == AuthorisationCodes.SixthApprovalRequiredOnly;
		}

		protected SecurityCheckpoint SixthApprovalCheckpoint => RetrieveLevelApprovalCheckPoint(AuthorisationCodes.SixthApprovalRequiredOnly);

		#endregion

		protected abstract SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode);

		ZString GetAuthorisationCodeFromRequirement(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return authorisationRequirement != null ? authorisationRequirement.AuthorisationRequirement : ZString.Empty;
		}

		#endregion

		#region Claim Creation

		public virtual AccQueryClaimBase CreateClaim(bool withDefaultLines)
		{
			return null;
		}

		public virtual bool InitialiseApprovingWithClaim(InvoicingBase invoice)
		{
			return false;
		}

		public virtual bool WasApprovingWithClaimInitialized
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ISupportMatchingOfMyLines Members

		ISupportMatchingOfMyLines ThisAsISupportMatchingOfMyLines
		{
			get { return this; }
		}

		ZDecimal ISupportMatchingOfMyLines.LineTotalPaidAmount
		{
			get { return lineTotalPaidAmount; }
			set
			{
				if (lineTotalPaidAmount != value)
				{
					lineTotalPaidAmount = value;
					ThisAsIMatching.OSPartialPaymentAmountInfo.RefreshBinding();
				}
			}
		}
		ZDecimal lineTotalPaidAmount;

		ZDecimal ISupportMatchingOfMyLines.LineTotalLocalPaidAmount { get; set; }

		ZDecimal ISupportMatchingOfMyLines.LineTotalPaidAmountPosted
		{
			get
			{
				LineTotalPaidAmountPostedCalculator lineTotalPaidAmountPostedCalculator = LineTotalPaidAmountPostedCalculator.GetOrCreateNewInstance(Factory);
				return lineTotalPaidAmountPostedCalculator.GetValueFromElement(PK);
			}
		}

		void ISupportMatchingOfMyLines.GenerateTransLinePayRecords(ZGuid matchLinkPK)
		{
			foreach (InvoicingLineBase line in Lines)
			{
				ILineMatching lineAsILineMatching = line;
				if (!lineAsILineMatching.PaidAmount.IsEmpty)
				{
					AccTransLinePay transLinePay = line.TransLinePays.AddNew();
					transLinePay.A7_AP = matchLinkPK;

					var isFullyPay = lineAsILineMatching.LocalOutstandingAmount == lineAsILineMatching.LocalPaidAmount;
					var osPaidAmount = isFullyPay
						? lineAsILineMatching.OutstandingAmount
						: lineAsILineMatching.PaidAmount;
					TransactionLinePayOSOutstandingAmountProvider.SetPaymentAmounts(this, transLinePay, lineAsILineMatching.LocalPaidAmount, osPaidAmount);
				}
			}
		}

		void ISupportMatchingOfMyLines.ResetAmounts()
		{
			if (IsLinesLoaded)
			{
				foreach (ILineMatching line in Lines)
				{
					line.ResetAmounts();
				}

				ThisAsISupportMatchingOfMyLines.LineTotalPaidAmount = 0m;
				ThisAsISupportMatchingOfMyLines.LineTotalLocalPaidAmount = 0m;

				if (Validation is MatchingValidation)
				{
					((MatchingValidation)Validation).ValidateOSPartialPaymentAmount();
				}
			}
		}

		bool ISupportMatchingOfMyLines.IsPaidAmountApportionedToLines { get; set; }

		void ISupportMatchingOfMyLines.ApportionPaidAmountToLines()
		{
			if (!ThisAsISupportMatchingOfMyLines.LineTotalPaidAmountPosted.IsEmpty && ThisAsIMatching.OSOutstandingAmount == ThisAsIMatching.OSPartialPaymentAmount)
			{
				ThisAsISupportMatchingOfMyLines.IsPaidAmountApportionedToLines = true;

				foreach (ILineMatching line in Lines)
				{
					line.SetDefaultValues();
					line.IsFullyPay = true;
				}
			}
		}

		bool ISupportMatchingOfMyLines.IsAllPaidLinesInTheSameCurrency(ZString currencyNK)
		{
			foreach (ILineMatching line in Lines)
			{
				if (line.PaidAmountInChargeCurrency != 0 && line.ChargeCurrency != currencyNK)
				{
					return false;
				}
			}
			return true;
		}

		ZDecimal ISupportMatchingOfMyLines.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			ZDecimal result = 0;
			foreach (ILineMatching line in Lines)
			{
				if (line.ChargeCurrency == currencyNK)
				{
					result += line.PaidAmountInChargeCurrency;
				}
				else if (line.ChargeCurrency.IsEmpty && line.AL_RX_NKTransactionCurrency == currencyNK)
				{
					result += line.PaidAmount;
				}
			}
			return result;
		}

		#endregion

		#region Related Amending transactions

		public InvoicingBaseCollection GetRelatedAmendingTransactions()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, PK);
			if (AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			}
			else
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			}
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
			query.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);

			var relatedAmendingTransactions = new InvoicingBaseCollection(Factory, query);
			relatedAmendingTransactions.Load();
			return relatedAmendingTransactions;
		}

		#endregion

		#region Related Invoices

		InvoicingBaseCollection fRelatedInvoices;
		public InvoicingBaseCollection RelatedInvoices
		{
			get
			{
				if (fRelatedInvoices == null)
				{
					fRelatedInvoices = new InvoicingBaseCollection(Factory, RelatedInvoicesQuery);
					fRelatedInvoices.SetReadOnlyIncludingChildren(true);
					fRelatedInvoices.Load();
				}
				return fRelatedInvoices;
			}
		}

		ZQuery RelatedInvoicesQuery
		{
			get
			{
				var result = ZQuery.NoResultQuery;
				var relatedInvoicesPKs = GetRelatedInvoicesPK();

				if (relatedInvoicesPKs.Any())
				{
					result = new ZQuery(AccTransactionHeaderSchema.PK, relatedInvoicesPKs);
				}
				return result;
			}
		}

		List<ZGuid> GetRelatedInvoicesPK()
		{
			var rawQuery = @"
DECLARE @PrimaryLines TABLE (
	AL_JH uniqueidentifier NOT NULL,
	AL_AC uniqueidentifier NOT NULL,
	AL_GB uniqueidentifier NOT NULL,
	AL_GE uniqueidentifier NOT NULL
)

INSERT INTO @PrimaryLines
(
	AL_AC, AL_JH, AL_GB, AL_GE
)
SELECT DISTINCT AL_AC, AL_JH, AL_GB, AL_GE
FROM dbo.AccTransactionLines
WHERE AL_AH = @TransactionPK
	AND AL_AC IS NOT NULL
	AND AL_JH IS NOT NULL

;WITH SecondaryLines AS (
	SELECT DISTINCT AL_AH
	FROM dbo.AccTransactionLines AS SL
	WHERE SL.AL_LineType IN ('REV', 'CST')
	AND EXISTS (
		SELECT 1
		FROM @PrimaryLines AS PL
		WHERE
			SL.AL_JH = PL.AL_JH
			AND SL.AL_GB = PL.AL_GB
			AND SL.AL_GE = PL.AL_GE
			AND SL.AL_AC = PL.AL_AC
	)
)

SELECT AH_PK AS relatedInvoicePK
FROM dbo.AccTransactionHeader
	INNER JOIN SecondaryLines ON AH_PK = SecondaryLines.AL_AH
WHERE AH_Ledger = @OppositeLedger
	AND AH_TransactionType IN ('INV', 'CRD')";

			var oppositeLedger = this.AH_Ledger == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			var parameters = new[]
			{
				ZSqlParameter.New("@TransactionPK", this.PK, AccTransactionHeaderSchema.PK),
				ZSqlParameter.New("@OppositeLedger", oppositeLedger, AccTransactionHeaderSchema.AH_Ledger)
			};

			var queryResult = new DynamicBusinessObjectCollection(Factory);
			queryResult.Load(rawQuery, parameters);

			var relatedInvoicesPK = new List<ZGuid>();
			for (var i = 0; i < queryResult.Count; i++)
			{
				relatedInvoicesPK.Add((ZGuid)queryResult[i]["relatedInvoicePK"]);
			}
			return relatedInvoicesPK;
		}

		public void GeneratePaidRelatedInvoicesSecurityCertificate(SecurityCheckpoint checkPoint = null)
		{
			if (IsSelfBillingInvoice && PaidRelatedSelfBilledInvoicesSecurityCertificate == null)
			{
				PaidRelatedSelfBilledInvoicesSecurityCertificate = SecurityOverrideProvider.SecurityCertificates[checkPoint ?? Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid];
			}
			else if (!IsSelfBillingInvoice && PaidRelatedInvoicesSecurityCertificate == null)
			{
				PaidRelatedInvoicesSecurityCertificate = SecurityOverrideProvider.SecurityCertificates[checkPoint ?? Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid];
			}
		}

		public SecurityCertificate PaidRelatedInvoicesSecurityCertificate
		{
			get;
			set;
		}

		public SecurityCertificate PaidRelatedSelfBilledInvoicesSecurityCertificate
		{
			get;
			set;
		}

		#endregion

		public bool ShouldConfirmComplianceSubTypeCanReverse =>
				IsReceivableOrPayable
				&& IsInvoiceOrCreditNoteOrAdjustmentNote
				&& !AH_TransactionReference.IsEmpty
				&& !AH_ComplianceSubType.IsEmpty
				&& !HasGeneratedComplianceDocument(PK);

		protected List<InvoicingBase> RemoveDuplicateBranchDepartment(List<InvoicingBase> transactions)
		{
			var branchDepartmentLevelRequired = new Dictionary<InvoicingBase, int>();

			foreach (InvoicingBase transaction in transactions)
			{
				var levelRequired = transaction.Level6AuthorisationRequired(transaction.AuthorisationRequired) ? 6 :
									transaction.Level5AuthorisationRequired(transaction.AuthorisationRequired) ? 5 :
									transaction.Level4AuthorisationRequired(transaction.AuthorisationRequired) ? 4 :
									transaction.Level3AuthorisationRequired(transaction.AuthorisationRequired) ? 3 :
									transaction.Level2AuthorisationRequired(transaction.AuthorisationRequired) ? 2 :
									transaction.Level1AuthorisationRequired(transaction.AuthorisationRequired) ? 1 :
									0;
				var savedTransaction = branchDepartmentLevelRequired.FirstOrDefault(x => x.Key.AH_GB == transaction.AH_GB && x.Key.AH_GE == transaction.AH_GE).Key;

				if (savedTransaction != null)
				{
					var savedTransactionLevelRequired = branchDepartmentLevelRequired.First(x => x.Key.AH_GB == transaction.AH_GB && x.Key.AH_GE == transaction.AH_GE).Value;
					if (savedTransactionLevelRequired < levelRequired)
					{
						branchDepartmentLevelRequired.Remove(savedTransaction);
						branchDepartmentLevelRequired.Add(transaction, levelRequired);
					}
				}
				else
				{
					branchDepartmentLevelRequired.Add(transaction, levelRequired);
				}
			}

			return branchDepartmentLevelRequired.Keys.ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool CheckLevelSecurityRightsForARCreditNote(out Dictionary<Guid, Guid> transactionApprovedByUserMapping, bool isMiscInvoice = true, bool isFromApprovalModule = false)
		{
			bool result = true;
			bool isLevelOneRequired, isLevelTwoRequired, isLevelThreeRequired, isLevelFourRequired, isLevelFiveRequired, isLevelSixRequired;
			isLevelOneRequired = isLevelTwoRequired = isLevelThreeRequired = isLevelFourRequired = isLevelFiveRequired = isLevelSixRequired = false;
			transactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level0TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level1TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level2TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level3TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level4TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level5TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level6TransactionApprovedByUserMapping = new Dictionary<Guid, Guid>();
			var level0ApprovingUserPK = Env.CurrentUser.PK;

			if (TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation != null)
			{
				int level1SecurityCheckNeedPromptIndex, level2SecurityCheckNeedPromptIndex, level3SecurityCheckNeedPromptIndex, level4SecurityCheckNeedPromptIndex, level5SecurityCheckNeedPromptIndex, level6SecurityCheckNeedPromptIndex;
				level1SecurityCheckNeedPromptIndex = level2SecurityCheckNeedPromptIndex = level3SecurityCheckNeedPromptIndex = level4SecurityCheckNeedPromptIndex = level5SecurityCheckNeedPromptIndex = level6SecurityCheckNeedPromptIndex = 0;
				var loginController = new UserLoginController();
				int index = 0;
				var needToRespectEnforceMultipleApproverSetting = isMiscInvoice || !isFromApprovalModule;

				foreach (var transaction in TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation)
				{
					var userSecurity = Env.Security;

					if (transaction.Level1AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelOneRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.FirstApprovalRequiredOnly, level1TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level1SecurityCheckNeedPromptIndex);
					}
					else if (transaction.Level2AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelTwoRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.SecondApprovalRequiredOnly, level2TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level2SecurityCheckNeedPromptIndex);
					}
					else if (transaction.Level3AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelThreeRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.ThirdApprovalRequiredOnly, level3TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level3SecurityCheckNeedPromptIndex);
					}
					else if (transaction.Level4AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelFourRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.FourthApprovalRequiredOnly, level4TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level4SecurityCheckNeedPromptIndex);
					}
					else if (transaction.Level5AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelFiveRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.FifthApprovalRequiredOnly, level5TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level5SecurityCheckNeedPromptIndex);
					}
					else if (transaction.Level6AuthorisationRequired(transaction.AuthorisationRequired))
					{
						isLevelSixRequired = true;
						BuildSecurityNeedPromptIndex(transaction, AuthorisationCodes.SixthApprovalRequiredOnly, level6TransactionApprovedByUserMapping, loginController, index, needToRespectEnforceMultipleApproverSetting, ref userSecurity, ref level6SecurityCheckNeedPromptIndex);
					}
					else
					{
						level0TransactionApprovedByUserMapping.Add(transaction.PK.ToGuid(), level0ApprovingUserPK);
					}

					index++;
				}

				bool levelOneCheckedSuccessfully = false;
				bool levelTwoCheckedSuccessfully = false;
				bool levelThreeCheckedSuccessfully = false;
				bool levelFourCheckedSuccessfully = false;
				bool levelFiveCheckedSuccessfully = false;
				bool levelSixCheckedSuccessfully = false;

				if (isLevelOneRequired)
				{
					levelOneCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.FirstApprovalRequiredOnly, level1TransactionApprovedByUserMapping, level1SecurityCheckNeedPromptIndex);
					result = levelOneCheckedSuccessfully;
				}

				if (result && isLevelTwoRequired)
				{
					levelTwoCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.SecondApprovalRequiredOnly, level2TransactionApprovedByUserMapping, level2SecurityCheckNeedPromptIndex, levelOneCheckedSuccessfully);
					result = levelTwoCheckedSuccessfully;
				}

				if (result && isLevelThreeRequired)
				{
					levelThreeCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.ThirdApprovalRequiredOnly, level3TransactionApprovedByUserMapping, level3SecurityCheckNeedPromptIndex, levelTwoCheckedSuccessfully);
					result = levelThreeCheckedSuccessfully;
				}

				if (result && isLevelFourRequired)
				{
					levelFourCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.FourthApprovalRequiredOnly, level4TransactionApprovedByUserMapping, level4SecurityCheckNeedPromptIndex, levelThreeCheckedSuccessfully);
					result = levelFourCheckedSuccessfully;
				}

				if (result && isLevelFiveRequired)
				{
					levelFiveCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.FifthApprovalRequiredOnly, level5TransactionApprovedByUserMapping, level5SecurityCheckNeedPromptIndex, levelFourCheckedSuccessfully);
					result = levelFiveCheckedSuccessfully;
				}

				if (result && isLevelSixRequired)
				{
					levelSixCheckedSuccessfully = PromptLoginAndUpdateMapping(AuthorisationCodes.SixthApprovalRequiredOnly, level6TransactionApprovedByUserMapping, level6SecurityCheckNeedPromptIndex, levelFiveCheckedSuccessfully);
					result = levelSixCheckedSuccessfully;
				}

				transactionApprovedByUserMapping = level1TransactionApprovedByUserMapping
					.Concat(level2TransactionApprovedByUserMapping)
					.Concat(level3TransactionApprovedByUserMapping)
					.Concat(level4TransactionApprovedByUserMapping)
					.Concat(level5TransactionApprovedByUserMapping)
					.Concat(level6TransactionApprovedByUserMapping)
					.Concat(level0TransactionApprovedByUserMapping)
					.ToDictionary(pair => pair.Key, pair => pair.Value);
			}

			return result;
		}

		bool PromptLoginAndUpdateMapping(string levelCode, Dictionary<Guid, Guid> levelTransactionApprovedByUserMapping,
			int levelSecurityCheckNeedPromptIndex, bool previousLevelCheckedSuccesfully = false)
		{
			Guid levelApprovingUserPK = Guid.Empty;
			bool levelCheckedSuccessfully = false;

			if (SecurityOverrideProvider is ISupportMixedLevelAuthorization && previousLevelCheckedSuccesfully)
			{
				((ISupportMixedLevelAuthorization)SecurityOverrideProvider).EnforceToCheckNextLevelOfAuthorization = true;
			}

			if (levelTransactionApprovedByUserMapping.Values.Contains(Guid.Empty))
			{
				var firstCreditNoteNeedPrompt = TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation[levelSecurityCheckNeedPromptIndex];
				var checkPoint = firstCreditNoteNeedPrompt.RetrieveLevelApprovalCheckPoint(levelCode);
				levelCheckedSuccessfully = SecurityOverrideProvider.SecurityCertificates[checkPoint].IsAllowed;

				if (SecurityOverrideProvider.UserSecurityOverride != null)
				{
					levelApprovingUserPK = SecurityOverrideProvider.UserSecurityOverride.UserPK;
				}

				if (levelCheckedSuccessfully)
				{
					foreach (var kvp in levelTransactionApprovedByUserMapping.ToArray())
					{
						levelTransactionApprovedByUserMapping[kvp.Key] = levelApprovingUserPK;
					}
				}
			}
			else
			{
				levelCheckedSuccessfully = true;
			}
			return levelCheckedSuccessfully;
		}

		void BuildSecurityNeedPromptIndex(InvoicingBase transaction, string levelCode, Dictionary<Guid, Guid> transactionApprovedByUserMapping, UserLoginController loginController, int index, bool needToRespectEnforceMultipleApproverSetting, ref SecurityCore userSecurity, ref int securityCheckNeedPromptIndex)
		{
			if (needToRespectEnforceMultipleApproverSetting && (transaction.EnforceTwoApproversWhenPostingARCredit || transaction.EnforceSequentialApproversWhenPostingARCredit))
			{
				transactionApprovedByUserMapping.Add(transaction.PK.ToGuid(), Guid.Empty);
				securityCheckNeedPromptIndex = index;
			}
			else
			{
				if (transaction.AH_GB.ToGuid() != Env.CurrentBranch.PK || transaction.AH_GE.ToGuid() != Env.CurrentDepartment.PK)
				{
					userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName, transaction.AH_GB.ToGuid(), transaction.AH_GE.ToGuid());
				}

				if (!transactionApprovedByUserMapping.Keys.Contains(transaction.PK.ToGuid()))
				{
					var checkPoint = GetCheckPointForLevelFromSecurityCore(userSecurity, levelCode);
					if (checkPoint != null)
					{
						transactionApprovedByUserMapping.Add(transaction.PK.ToGuid(), checkPoint.IsAllowed ? Env.CurrentUser.PK : Guid.Empty);
						if (!checkPoint.IsAllowed)
						{
							securityCheckNeedPromptIndex = index;
						}
					}
				}
			}
		}

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
				if (provider == null)
				{
					provider = new DefaultAccessSecurityProvider();
				}
				return provider;
			}
			set
			{
				provider = value;
			}
		}

		#endregion

		#region Is Paid By Webservice

		internal bool IsInvoicePaidByPaymentWebservice()
		{
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			filter.AddToFilter(StmALogSchema.SL_Table, AccTransactionHeader.Schema.TableName);
			filter.AddToFilter(StmALogSchema.SL_Parent, this.PK);
			filter.AddToFilter(StmALogSchema.SL_GS_NKUser, "ZZ");
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "by Transaction Payment Web Service");

			var log = Factory.LoadTop1<StmALog>(filter);
			return log != null;
		}

		#endregion

		public ZBool IsPeriodicInvoice
		{
			get { return InvoiceTypeCalculationProvider.IsDeferredInvoiceType(AH_TransactionCategory); }
		}

		public bool IsTaxCoreInvoice => Company?.IsInTaxCoreSupportedCountry() ?? false;

		public override ZBool IsTaxed
		{
			get
			{
				if (!IsInDatabase || !IsPosted)
				{
					return LinesContainTax;
				}
				else
				{
					return Factory.GetCachedValue("IsTaxed:" + PK.ToStringKey(), () =>
					{
						return LinesContainTax;
					});
				}
			}
		}

		internal bool IsForeignCurrencyInvoice
		{
			get { return ExchangeRate.Currency != ZString.Empty && ExchangeRate.Currency != Company.GC_RX_NKLocalCurrency; }
		}

		ZBool LinesContainTax
		{
			get
			{
				return Lines.Cast<InvoicingLineBase>().Any(x => x.TaxRate != null);
			}
		}

		#region IncludeInThePeriodicInvoice

		public ZBool IncludeInThePeriodicInvoice
		{
			get
			{
				return fIncludeInThePeriodicInvoice;
			}
			set
			{
				if (fIncludeInThePeriodicInvoice != value)
				{
					fIncludeInThePeriodicInvoice = value;

					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						PeriodicInvoiceMiscInvoiceCollection periodicInvoiceLineCollection = parentCollection as PeriodicInvoiceMiscInvoiceCollection;
						if (periodicInvoiceLineCollection != null)
						{
							periodicInvoiceLineCollection.RaiseOnIncludingInThePeriodicInvoiceChanged(this);
						}
					}

					IncludeInThePeriodicInvoiceInfo.RefreshBinding();
				}
			}
		}
		ZBool fIncludeInThePeriodicInvoice;

		public ZPropertyInfo IncludeInThePeriodicInvoiceInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInThePeriodicInvoice)); }
		}

		#endregion

		string IJobNumber.JobNumber
		{
			get
			{
				if (!JobNumber.IsEmpty)
				{
					return JobNumber;
				}
				else
				{
					return AH_TransactionNum;
				}
			}
		}

		ChargeCodeToGlobalChargeCodeMap chargeCodeToGlobalChargeCodeMap;

		public IEnumerable<GlobalChargeCodeMap> GetGlobalChargeCodes(ZGuid chargeCodePK)
		{
			if (chargeCodeToGlobalChargeCodeMap == null
				|| !chargeCodeToGlobalChargeCodeMap.WasIncludedInQuery(chargeCodePK))
			{
				if (Header == null)
				{
					return Array.Empty<GlobalChargeCodeMap>();
				}

				HashSet<ZGuid> invoiceChargeCodePKs = new HashSet<ZGuid>();

				foreach (InvoicingLineBase line in Lines)
				{
					invoiceChargeCodePKs.Add(line.AL_AC);
				}

				chargeCodeToGlobalChargeCodeMap = ChargeCodeToGlobalChargeCodeMap.GetARMap(Factory, Header.PK, invoiceChargeCodePKs);
			}

			return chargeCodeToGlobalChargeCodeMap[chargeCodePK];
		}

		#region ICDArchive Members

		public CDArchiveInfo CDArchiveInfo
		{
			get { return new InvoiceCDArchiveInfo(this); }
		}

		public class InvoiceCDArchiveInfo : CDArchiveInfo
		{
			public InvoiceCDArchiveInfo(InvoicingBase invoice)
				: base(invoice)
			{
			}

			ICDArchive CDArchiveJob
			{
				get
				{
					if (cdArchiveJob == null)
					{
						InvoicingBase invoice = (InvoicingBase)BusinessEntity;
						if (invoice != null && invoice.Ledger == LedgerTypes.AccountsReceivable && invoice.Job != null) // currently we are interested in AR only
						{
							var job = Factory.LoadGenericJob<GenericJob.GenericJob>(invoice.Job.JH_ParentID, invoice.Job.JH_ParentTableCode);
							if (job != null)
							{
								Type consumerType = job.GetConsumerType();
								if (consumerType != null)
								{
									var consumer = Factory.Load(consumerType, job.PK);
									if (consumer != null)
									{
										cdArchiveJob = consumer as ICDArchive;
									}
								}
							}
						}
					}
					return cdArchiveJob;
				}
			}
			ICDArchive cdArchiveJob;

			public override ZString ConsigneeCode
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.ConsigneeCode : ZString.Empty; }
			}

			public override ZString ConsignorCode
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.ConsignorCode : ZString.Empty; }
			}

			public override ZString[] ContainerNumbersList
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.ContainerNumbersList : Array.Empty<ZString>(); }
			}

			public override ZString Destination
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.Destination : ZString.Empty; }
			}

			public override ZDateTime ETA
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.ETA : ZDateTime.Empty; }
			}

			public override ZDateTime ETD
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.ETD : ZDateTime.Empty; }
			}

			public override ZString[] EntryNumbersList
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.EntryNumbersList : Array.Empty<ZString>(); }
			}

			public override ZString HouseBill
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.HouseBill : ZString.Empty; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.InvoiceNumbersList : Array.Empty<ZString>(); }
			}

			public override ZString JobNumber
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.JobNumber : ZString.Empty; }
			}

			public override ZString MasterBill
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.MasterBill : ZString.Empty; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.OrderNumbersList : Array.Empty<ZString>(); }
			}

			public override ZString Origin
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.Origin : ZString.Empty; }
			}

			public override ZString Vessel
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.Vessel : ZString.Empty; }
			}

			public override ZString VoyageFlight
			{
				get { return (CDArchiveJob != null && CDArchiveJob.CDArchiveInfo != null) ? CDArchiveJob.CDArchiveInfo.VoyageFlight : ZString.Empty; }
			}
		}

		#endregion

		#region Invoice Address GUI Support

		[List("Lookups.Headers")]
		public ZGuid DisplayInvoiceAddressOverrideForAddressControl
		{
			get
			{
				return DisplayInvoiceAddressOverride;
			}
			set
			{
				if (value.IsValid)
				{
					var address = Factory.Load<OrgAddress>(value);
					if (address != null && address.OA_OH != AH_OH)
					{
						AH_OH = address.OA_OH;
					}
				}

				DisplayInvoiceAddressOverride = value;
			}
		}

		public ZPropertyInfo DisplayInvoiceAddressOverrideForAddressControlInfo
		{
			get { return GetWrappedZPropertyInfo(InvoicingBase.Schema.DisplayInvoiceAddressOverrideForAddressControl, x => AH_OA_InvoiceAddressOverrideInfo); }
		}

		public ZAddressWithContact OrganisationAddressWithContact
		{
			get
			{
				if (fOrganisationAddressWithContact == null)
				{
					fOrganisationAddressWithContact = GetOrganisationAddressWithContact();
				}
				return fOrganisationAddressWithContact;
			}
		}
		ZAddressWithContact fOrganisationAddressWithContact;

		public bool DisplayInvoiceAddressOverrideForAddressControl_ReadOnly
		{
			get { return AH_OHInfo.ReadOnly; }
		}

		ZAddressWithContact GetOrganisationAddressWithContact()
		{
			var result = new ZAddressWithContact(DisplayInvoiceContactOverrideInfo, DisplayInvoiceAddressOverrideForAddressControlInfo);
			result.GetDefaultAddress = GetDefaultAddress;
			return result;
		}

		#endregion

		SetConsolidatedInvoiceRefForAmendedTransactionStrategy setConsolidatedInvoiceRefForAmendedTransactionStrategy;

		internal SetConsolidatedInvoiceRefForAmendedTransactionStrategy SetConsolidatedInvoiceRefForAmendedTransactionStrategy
		{
			get { return setConsolidatedInvoiceRefForAmendedTransactionStrategy; }
			set { setConsolidatedInvoiceRefForAmendedTransactionStrategy = value; }
		}

		#region Duplicate Transaction Number Details Provider

		internal void SetPreviousSameNumberTransactionDetails(AccountingUtils.PreviousSameNumberTransactionDetails value)
		{
			var service = this.Factory.ServiceContainer.GetService<DuplicateTransactionNumberDetailsProvider>();
			if (service == null)
			{
				service = new DuplicateTransactionNumberDetailsProvider();
				this.Factory.ServiceContainer.AddService(service);
			}

			service.SetDataRowRelatedValue(this, value);
		}

		internal void ResetSameNumberTransactionDetails()
		{
			var service = this.Factory.ServiceContainer.GetService<DuplicateTransactionNumberDetailsProvider>();
			if (service != null)
			{
				service.RemoveDataRowRelatedValue(this);
			}
		}

		internal AccountingUtils.PreviousSameNumberTransactionDetails? GetPreviousSameNumberTransactionDetails()
		{
			var service = this.Factory.ServiceContainer.GetService<DuplicateTransactionNumberDetailsProvider>();

			AccountingUtils.PreviousSameNumberTransactionDetails value;
			if (service != null && service.TryGetDataRowRelatedValue(this, out value))
			{
				return value;
			}

			return null;
		}

		class DuplicateTransactionNumberDetailsProvider : BizoDataRowRelatedValue<AccountingUtils.PreviousSameNumberTransactionDetails>
		{
			public new bool TryGetDataRowRelatedValue(BusinessObject bizoForDataRow, out AccountingUtils.PreviousSameNumberTransactionDetails value)
			{
				return base.TryGetDataRowRelatedValue(bizoForDataRow, out value);
			}

			public new void SetDataRowRelatedValue(BusinessObject bizoForDataRow, AccountingUtils.PreviousSameNumberTransactionDetails value)
			{
				base.SetDataRowRelatedValue(bizoForDataRow, value);
			}

			public new bool RemoveDataRowRelatedValue(BusinessObject bizoForDataRow)
			{
				return base.RemoveDataRowRelatedValue(bizoForDataRow);
			}
		}

		#endregion

		#region ConsolCostValidationOptimization

#if DEBUG
		internal int CheckConsolIDFromApportionedChargeCallCount_ForTestOnly;
#endif

#if DEBUG
		public
#endif
		List<ZGuid> ConsolCostValidatedPKList => consolCostValidatedPKList ?? (consolCostValidatedPKList = new List<ZGuid>());
		List<ZGuid> consolCostValidatedPKList;

		internal void ClearValidatedConsolCostPKList()
		{
			consolCostValidatedPKList = null;
		}

		internal void ValidateConsolCostIfRequired(JobConsolCost parentConsolCost)
		{
			if (!ConsolCostValidatedPKList.Contains(parentConsolCost.PK))
			{
#if DEBUG
				if (Globals.IsTest)
				{
					CheckConsolIDFromApportionedChargeCallCount_ForTestOnly++;
				}
#endif
				if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					parentConsolCost.MarkAsNeedingValidationIncludingChildren();
				}
				parentConsolCost.RunPreSaveValidation();
				ConsolCostValidatedPKList.Add(parentConsolCost.PK);
			}
		}

		#endregion

		#region Should print IT Autofattura Document

		public bool SupportPrintAutofatturaDocument()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy &&
				AH_ComplianceSubType == ItalyComplianceInfo.ComplianceSubTypeCodes.APS &&
				!string.IsNullOrEmpty(AH_TransactionReference);
		}

		#endregion

		#region Invoice Date Defaulting Behaviour

		public bool ShouldUseDefaultARInvoiceAndPostDate() => IsARInvoiceOrCreditNoteOrAdjustmentNote
			&& ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate();

#if DEBUG
		public bool AH_InvoiceDate_ReadOnly_Exposed => AH_InvoiceDate_ReadOnly;
		public bool AH_PostDate_ReadOnly_Exposed => AH_PostDate_ReadOnly;
#endif
		#endregion

		Dictionary<ZGuid, ZDecimal> transactionLinesDefaultExRates;
		public Dictionary<ZGuid, ZDecimal> TransactionLinesDefaultExRates
		{
			get
			{
				if (transactionLinesDefaultExRates == null)
				{
					transactionLinesDefaultExRates = new Dictionary<ZGuid, ZDecimal>();
				}
				return transactionLinesDefaultExRates;
			}
		}

		internal readonly InvoiceTaxDateCacheProvider InvoiceTaxDateCacheProvider;

		public bool IsAmendInFull { get; set; }

		protected bool hasBeenCreatedAsAmending;
	}

	#region Document Supporter

	public class InvoicingBaseDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		protected InvoicingBaseDocumentSupporter(InvoicingBase invoice)
			: base(invoice)
		{
		}

		#region Construction

		public static InvoicingBaseDocumentSupporter New(InvoicingBase invoice)
		{
			InvoicingBaseDocumentSupporter result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoice);
			}
			else if (invoice != null)
			{
				result = new InvoicingBaseDocumentSupporter(invoice);
			}

			return result;
		}

		protected delegate InvoicingBaseDocumentSupporter NewDelegate(InvoicingBase invoice);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		protected InvoicingBase Invoice
		{
			get { return (InvoicingBase)BusinessObject; }
		}

		public override IOrgContact GetAdditionalDeliveryContact()
		{
			return Invoice.InvoiceContactOverride;
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return Invoice.InvoiceAddressOverride;
		}

		#region Contexts and Doc Supporter Logic

		protected override ZString GetPDFPasswordCore(DeliverableInfo deliverableInfo)
		{
			return new InvoicingBasePDFPasswordProvider(Invoice, deliverableInfo).GetPassword();
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			if (result.IsValid && commandAboutToBeRun != null)
			{
				switch (commandAboutToBeRun.SU_MenuName)
				{
					case JobInvoicingEDocsProviderSupporter.ITAutofattura:
						if (!((Invoice.AH_TransactionType == TransactionTypes.Invoice ||
							Invoice.AH_TransactionType == TransactionTypes.CreditNote ||
							Invoice.AH_TransactionType == TransactionTypes.AdjustmentNote) &&
							Invoice.AH_ComplianceSubType == ItalyComplianceInfo.ComplianceSubTypeCodes.APS &&
							!string.IsNullOrEmpty(Invoice.AH_TransactionReference)))
						{
							var errorMessage = Res.GetString("7E1F84CC-87C8-49B8-A7BD-41590FB2E94E", "Autofattura (IT) can only be printed for Invoice, Credit Note and Adjustment transactions that have a Compliance Sub Type = APS and allocated Compliance Number");
							result = new DocumentSupporterDataState(false, errorMessage);
						}
						break;
					case EInvoicingKoreaSouthConstants.KRElectronicInvoice:
					case EInvoicingKoreaSouthConstants.KRElectronicExemptInvoice:
						result = DocumentSupporterDataCheckerForKRElectronicInvoice.CheckerForKRElectronicInvoiceDocument(Invoice);
						break;
				}
			}
			return result;
		}

		public override CargoWise.Definitions.BusinessContext BusinessContext
		{
			get
			{
				CargoWise.Definitions.BusinessContext context;
				if (Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable
					|| Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions)
				{
					context = CargoWise.Definitions.BusinessContext.APInvoice;
				}
				else if (Invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
				{
					context = CargoWise.Definitions.BusinessContext.ARInvoice;
				}
				else
				{
					context = CargoWise.Definitions.BusinessContext.INVALID;
				}

				return context;
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.ARInvoice:
					return DocumentWrapperFactory.CreateWrappers(Constants.DataContext.ARInvoice, Invoice, GetInvoiceCopiesInformation());
				case Constants.DataContext.GenericFreightJob:
				case Constants.DataContext.GenericFreightJobInvoice:
					if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						return DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, Invoice, GetInvoiceCopiesInformation());
					}
					else
					{
						return DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, Invoice, (DocWrapperCopyInfo[])null);
					}
				case Constants.DataContext.APInvoice:
				case Constants.DataContext.WhsInvoiceDetail:
				case Constants.DataContext.WhsOrdersInvoiceJobHistory:
				case Constants.DataContext.WhsReceiveInvoiceJobHistory:
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, Invoice) };

				default:
					return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded document title.")]
		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			TitleCopyCountPair value = null;
			if ((parentDocumentMenuName.EqualsIgnoringCase("Invoice") || parentDocumentMenuName.EqualsIgnoringCase("DocBuilder Invoice")) && pivot.SI_DocumentTitle.EqualsIgnoringCase("Invoice"))
			{
				InvoiceCopyCollection registryDefinedCopies = AccountingConfigurationRegistry.Instance.InvoiceCopies.Value;
				foreach (InvoiceCopy entry in registryDefinedCopies)
				{
					if (entry.IsOriginal && !entry.Name.IsEmpty)
					{
						value = new TitleCopyCountPair(entry.Name);
					}
				}

				if (value == null && Invoice.IsPrintingProformaInvoice)
				{
					value = new TitleCopyCountPair(Res.GetString("570c633c-0f2f-45df-bb62-ada921ef05de", "Pro Forma Invoice"));
				}
			}
			return value;
		}

		ARInvoiceDocWrapperCopyInfo[] GetInvoiceCopiesInformation()
		{
			var result = new List<ARInvoiceDocWrapperCopyInfo>();

			if (!Factory.HasContext(Enterprise.Integration.Accounting.BusinessContext.SuspendInvoiceCopies))
			{
				var registryInvoiceCopies = AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Cast<InvoiceCopy>().Where(x => !x.IsOriginal).OrderBy(y => y.Order);
				registryInvoiceCopies.ForEach(copy => result.Add(new ARInvoiceDocWrapperCopyInfo(copy)));
			}

			return result.ToArray();
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			List<DataContext> result = new List<DataContext>
			{
				Constants.DataContext.ARInvoice,
				Constants.DataContext.GenericFreightJob,
				Constants.DataContext.JobInvoicingJob,
				Constants.DataContext.Statement,
				Constants.DataContext.TransactionHeader,
				Constants.DataContext.StatementSummary
			};

			if (Invoice != null)
			{
				if (Invoice.AH_Ledger == LedgerTypes.AccountsPayable || Invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					result = new List<DataContext> { Constants.DataContext.APInvoice, Constants.DataContext.GenericFreightJob };
				}
				else if (Invoice.Job != null)
				{
					if (Invoice.Job.JH_ParentTableCode == JobStorageSchema.Constants.Prefix)
					{
						result = new List<DataContext>
						{
							Constants.DataContext.ARInvoice,
							Constants.DataContext.GenericFreightJob,
							Constants.DataContext.JobInvoicingJob,
							Constants.DataContext.Statement,
							Constants.DataContext.TransactionHeader,
							Constants.DataContext.WhsInvoiceDetail,
							Constants.DataContext.WhsOrdersInvoiceJobHistory,
							Constants.DataContext.WhsReceiveInvoiceJobHistory,
							Constants.DataContext.StatementSummary
						};
					}
				}
			}
			if (ShouldSupportAccountingVoucher())
			{
				result.Add(Constants.DataContext.AccountingVoucher);
			}
			return result.ToArray();
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeader orgHeader = Factory.Load<OrgHeader>(Invoice.AH_OH);
			return new OrgHeaderContact(orgHeader, null);
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandardInvoice:
					{
						return ZBool.True.ToString();
					}
				case MenuTemplateFilterType.PrintClientSpecificInvoice:
					{
						return ZBool.False.ToString();
					}
				case MenuTemplateFilterType.PrintClientSpecificTrailingPage:
					{
						return ZBool.False.ToString();
					}
			}
			return null;
		}

		#endregion

		#region Ports and Transport Mode

		public override string ForeignPort(IContactType contactType, DocumentDirection direction)
		{
			RefUNLOCO loco = null;
			CommonConsol consol = ConsolData;
			IJobInvoicingPlugIn plugin = PluginData;

			if (consol != null)
			{
				loco = consol.IsImport() ? consol.LoadPort : consol.DischargePort;
			}
			else if (plugin != null)
			{
				loco = plugin.InvoicingSupporter.IsImport ? plugin.InvoicingSupporter.Origin : plugin.InvoicingSupporter.Destination;
			}

			return loco != null ? loco.Code : ZString.Empty;
		}

		public override string LocalPort(IContactType contactType, DocumentDirection direction)
		{
			RefUNLOCO loco = null;
			CommonConsol consol = ConsolData;
			IJobInvoicingPlugIn plugin = PluginData;

			if (consol != null)
			{
				loco = consol.IsImport() ? consol.DischargePort : consol.LoadPort;
			}
			else if (plugin != null)
			{
				loco = plugin.InvoicingSupporter.IsImport ? plugin.InvoicingSupporter.Destination : plugin.InvoicingSupporter.Origin;
			}

			return loco != null ? loco.Code : ZString.Empty;
		}

		public override string TransportMode
		{
			get
			{
				string mode = "";
				CommonConsol consol = ConsolData;
				IJobInvoicingPlugIn plugin = PluginData;

				if (consol != null)
				{
					mode = consol.JK_TransportMode;
				}
				else if (plugin != null)
				{
					mode = plugin.InvoicingSupporter.TransportMode;
				}

				return mode;
			}
		}

		public override string ContainerMode
		{
			get
			{
				var containerMode = "";

				if (ConsolData != null)
				{
					containerMode = ConsolData.JK_ConsolMode;
				}
				else if (PluginData != null)
				{
					containerMode = PluginData.InvoicingSupporter.ContainerMode;
				}

				return containerMode;
			}
		}

		public override bool IsImport
		{
			get
			{
				bool result = false;
				CommonConsol consol = ConsolData;
				IJobInvoicingPlugIn plugin = PluginData;

				if (consol != null)
				{
					result = consol.IsImport();
				}
				else if (plugin != null)
				{
					result = plugin.InvoicingSupporter.IsImport;
				}

				return result;
			}
		}

		IJobInvoicingPlugIn PluginData
		{
			get
			{
				GenericJob.GenericJob result = null;
				if (Invoice.Job != null)
				{
					result = Invoice.Job.LoadGenericJob<GenericJob.GenericJob>();
				}
				return result != null ? result.Consumer : null;
			}
		}

		CommonConsol ConsolData
		{
			get { return Invoice.IsConsolInvoice ? Factory.LoadFromUniqueKey<CommonConsol>(JobConsolSchema.JK_UniqueConsignRef, Invoice.ConsolNumberFromConsolidatedInvoiceRef) : null; }
		}

		#endregion

		public override MultilingualString CustomWatermarkText => Invoice.CustomWatermarkText;
	}

	#region ARInvoiceDocWrapperCopyInfo

	public class ARInvoiceDocWrapperCopyInfo : DocWrapperCopyInfo
	{
		public ARInvoiceDocWrapperCopyInfo(InvoiceCopy copy)
			: base()
		{
			fIncludeTradingTerms = copy.IncludeTradingTerms;
			Name = copy.Name;
			DeliveryMethod = GetPrintCopyType(copy.DeliveryMethod);
			Message = copy.Message;
		}

		public bool IncludeTradingTerms
		{
			get { return fIncludeTradingTerms; }
		}

		readonly bool fIncludeTradingTerms;

		public readonly string Message;

		PrintCopyType GetPrintCopyType(string type)
		{
			PrintCopyType printCopyType = PrintCopyType.ALL;

			try
			{
				printCopyType = (PrintCopyType)Enum.Parse(typeof(PrintCopyType), type, true);
			}
			catch (ArgumentException)
			{
				printCopyType = PrintCopyType.ALL;
			}

			return printCopyType;
		}
	}

	#endregion

	#endregion
}
