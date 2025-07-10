using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class ARCreditNote : CreditNote, IDocManagerSupport, IBadDebtWritingOff, IAmending, IEDocsParsingSupport
	{
		public ARCreditNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new ARCreditNoteValidation(this);
		}

		protected override bool OriginalTransactionReference_ReadOnlyCore()
		{
			return (IsAmendingTransaction_StrongReference && hasBeenCreatedAsAmending) || AreOriginalReferenceFieldsReadOnly;
		}

		protected override bool AH_InvoiceTerm_ReadOnly
		{
			get
			{
				if (IsAmendingTransaction)
				{
					return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
						|| !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionWCreditNote);
				}
				else
				{
					return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
						|| !Env.Security.NewReceivablesCreditNoteTerm.IsAllowed;
				}
			}
		}

		protected override bool AH_InvoiceTermDays_ReadOnly
		{
			get
			{
				return AH_InvoiceTerm_ReadOnly || AH_InvoiceTerm == Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			}
		}

		protected override bool AH_InvoiceDate_ReadOnly
		{
			get
			{
				return base.AH_InvoiceDate_ReadOnly || !Env.Security.NewReceivablesCreditNoteInvoiceDate.IsAllowed;
			}
		}

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;

				if (AreOriginalTransactionReferenceFieldsMandatory &&
					!OriginalTransactionReference.IsValid && AH_ReceiptType.IsEmpty)
				{
					var invDate = AH_InvoiceDate.Date;
					AH_OriginalReferenceStartDate = invDate;
					AH_OriginalReferenceEndDate = invDate;
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("44c7c89a-16fd-4b8c-806e-666f253a8ce5", "Accounts Receivable Credit Note"); }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(ARCreditNoteLine); }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				if (AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice || AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.Value.Value)
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARInvoiceNo;
				}
				else
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARCreditNoteNo;
				}
			}
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(ARInvoice); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(ARCreditNote); }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			IBadDebtWritingOff badDebt = fReverseTransaction as IBadDebtWritingOff;
			if (badDebt != null)
			{
				badDebt.IsWritingOff = IsWritingOff;
				if (IsWritingOff)
				{
					fReverseTransaction.AH_JH = ZGuid.Empty;
				}
			}

			if (!IsWritingOff && !AH_ConsolidatedInvoiceRef.IsEmpty)
			{
				if (IsConsolInvoice)
				{
					fReverseTransaction.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, ConsolNumberFromConsolidatedInvoiceRef, fReverseTransaction.PK);
				}
				else
				{
					Job loadedJob = Factory.Load<Job>(AH_JH);
					if (loadedJob != null)
					{
						fReverseTransaction.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(this, loadedJob);
					}
				}
			}
		}

		protected override void CopyJobHeader(InvoicingLineBase sourceLine, InvoicingLineBase destinationLine)
		{
			IBadDebtWritingOff badDebt = fReverseTransaction as IBadDebtWritingOff;
			if (badDebt != null)
			{
				if (IsWritingOff)
				{
					destinationLine.SuspendAL_JHSettingDefaults();
					try
					{
						destinationLine.AL_JH = ZGuid.Empty;
					}
					finally
					{
						destinationLine.ResumeAL_JHSettingDefaults();
					}
				}
				else
				{
					base.CopyJobHeader(sourceLine, destinationLine);
				}
			}
		}

		protected override void CopyCharges(InvoicingLineBase sourceLine, InvoicingLineBase destinationLine)
		{
			IBadDebtWritingOff badDebt = fReverseTransaction as IBadDebtWritingOff;
			if (badDebt != null)
			{
				if (IsWritingOff)
				{
					destinationLine.AL_AC = ZGuid.Empty;
					destinationLine.AL_AG = ZGuid.Empty;
					destinationLine.AL_Desc = ZString.Empty;
					destinationLine.GenericCharge = (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				else
				{
					base.CopyCharges(sourceLine, destinationLine);
				}
			}
		}

		protected override void SetDescriptionCore(ZString descriptionToSet)
		{
			base.SetDescriptionCore(descriptionToSet);

			if (IsWritingOff)
			{
				using (GetValidationSuspender())
				{
					foreach (InvoicingLineBase invoiceLine in Lines)
					{
						invoiceLine.AL_Desc = descriptionToSet;
					}
				}
			}
		}

		public override ZString InvoiceBatchNumber
		{
			get { return AH_ReceiptBatchNo; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AddInvoiceApprovalLog();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (this.IsStampDutyApplicable())
			{
				if (this.ShouldAddStampDuty() && !this.IsStampDutyAmountGreaterThanCreditNoteTotal())
				{
					this.AddStampDutyLine(true);
				}

				Logs.AddNew(AutoEvents.StampDutyLiability);
			}

			base.OnFactorySavingBeforeTransactionCore();
			UpdateJobChargeOverrideAddressContact();
		}

		public override bool CheckLevelSecurityRights()
		{
			bool result = true;
			if (!IsCreatedFromApprovalRequest && EnforceTwoApproversWhenPostingARCredit)
			{
				AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement = AuthorisationRequired;
				if (Level1AuthorisationRequired(authorisationRequirement))
				{
					result = SecurityOverrideProvider.SecurityCertificates[FirstApprovalCheckpoint].IsAllowed;
				}
				if (Level2AuthorisationRequired(authorisationRequirement))
				{
					result &= SecurityOverrideProvider.SecurityCertificates[SecondApprovalCheckpoint].IsAllowed;
				}
			}
			else
			{
				result = base.CheckLevelSecurityRights();
			}
			return result;
		}

		protected override void AddInvoiceApprovalLog()
		{
			if (!IsReversing)
			{
				List<ZGuid> userPKs;
				if (SecurityOverrideProvider is ISecurityOverrideProviderSupportTwoApproverLogin twoApprovalProvider
					&& (twoApprovalProvider.RequiresTwoApprovers || twoApprovalProvider.RequiresSequentialApprovals)
					&& (twoApprovalProvider.UserPKsForTwoCredentialLogin?.Any() ?? false))
				{
					userPKs = twoApprovalProvider.UserPKsForTwoCredentialLogin;
					if (userPKs.Count > 2)
					{
						throw new InvalidOperationException(FormattableString.Invariant($"A maximum of 2 approvers are supported, you have supplied {userPKs.Count}."));
					}
				}
				else if (SecurityOverrideProvider?.UserSecurityOverride != null && SecurityOverrideProvider.UserSecurityOverride.UserPK != Guid.Empty)
				{
					userPKs = new List<ZGuid> { SecurityOverrideProvider.UserSecurityOverride.UserPK };
				}
				else
				{
					userPKs = ApprovingUserPKList;
					if (userPKs.Count > 6)
					{
						throw new InvalidOperationException(FormattableString.Invariant($"A maximum of 6 approvers are supported, you have supplied {userPKs.Count}."));
					}
				}

				if (userPKs != null)
				{
					var users = userPKs.Where(x => x.IsValid).Select(x => Factory.Load<GlbStaff>(x)).Where(x => x != null).ToArray();
					if (users.Any())
					{
						var userNames = users.Select(x => FormattableString.Invariant($"{x.GS_LoginName} ({x.GS_FullName})"));
						StmALog log = Logs.AddNew(Events.Authorised, FormattableString.Invariant($"Post authorized by {String.Join(", ", userNames)}"), ApprovalDate.ToOffset());
						log.SL_GS_NKUser = users.First().GS_Code;
					}
				}
			}
		}

		protected override GenApprovalRequest GetLatestTransactionRelatedApprovalRequest(BusinessObjectFactory factory, bool alwaysCheckInDb = false)
		{
			ARCreditNoteApprovalRequest result = null;
			var originalTransaction = ((IAmending)this).OriginalTransaction;

			if (originalTransaction != null)
			{
				var query = new ZQuery(new ARCreditNoteApprovalRequestCollection(factory, new ZQuery(GenApprovalRequestSchema.XP_ParentID, originalTransaction.PK)).CompleteFilter);
				query.OrderBy = GenApprovalRequestSchema.XP_SystemCreateTimeUtc.Name + " DESC";
				var transaction = originalTransaction as TransactionHeader;
				if (transaction != null)
				{
					query.FetchOnlyFromLocalCache = !transaction.IsInDatabase && !alwaysCheckInDb;
				}
				result = factory.LoadTop1<ARCreditNoteApprovalRequest>(query);
			}
			return result;
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting; }
		}

		public override bool IsAllowModifyAmendStatusCode
		{
			get
			{
				var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
				var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
				return (amendStatusCodeProvider?.ShouldShowAmendStatusCode() ?? false) && !OriginalTransactionReference.IsEmpty && !IsInDatabase;
			}
		}

		public override ZGuid OriginalTransactionReference
		{
			get => base.OriginalTransactionReference;
			set
			{
				base.OriginalTransactionReference = value;

				if (OriginalTransactionReference.IsEmpty)
				{
					AH_Calc_AmendStatusCode = ZString.Empty;
				}
				var shouldClearComplianceSubType = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeGUIProvider(Company.GC_RN_NKCountryCode)?.ShouldClearComplianceSubType(OriginalTransactionReference) ?? false;
				if (shouldClearComplianceSubType)
				{
					AH_ComplianceSubType = ZString.Empty;
				}

				if (OriginalTransactionReference.IsValid)
				{
					ClearReferenceDatesWhenOriginalRefMandatory();
				}
			}
		}

		public override ZString AH_ReceiptType
		{
			get => base.AH_ReceiptType;
			set
			{
				base.AH_ReceiptType = value;

				if (!AH_ReceiptType.IsEmpty)
				{
					ClearReferenceDatesWhenOriginalRefMandatory();
				}
			}
		}

		void ClearReferenceDatesWhenOriginalRefMandatory()
		{
			if (AreOriginalTransactionReferenceFieldsMandatory)
			{
				AH_OriginalReferenceStartDate = ZDate.Empty;
				AH_OriginalReferenceEndDate = ZDate.Empty;
			}
		}

		#region Invoice Amount Levels

		protected override (ZGuid branchPK, ZGuid departmentPK) AuthorizationModeAndSettingsRegistryFallback
		{
			get
			{
				var (departmentPK, branchPK) = (ZGuid.Empty, ZGuid.Empty);
				if (Branch != null && Branch.PK.IsValid && Department != null && Department.PK.IsValid)
				{
					if (IsAmendingTransaction_StrongReference)
					{
						branchPK = ((this as IAmending).OriginalTransaction as TransactionHeader).AH_GB;
						departmentPK = ((this as IAmending).OriginalTransaction as TransactionHeader).AH_GE;
					}
					else if (Job != null)
					{
						branchPK = Job.Branch.PK;
						departmentPK = Job.Department.PK;
					}
					else
					{
						branchPK = Branch.PK;
						departmentPK = Department.PK;
					}
				}

				return (branchPK, departmentPK);
			}
		}

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry => AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings;

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode)
		{
			return GetCheckPointForLevel(levelCode);
		}

		#endregion

		#region Receiving job properies

		public override ZString ReceivingOperator
		{
			get { return IntercompanyInvoiceHelper.GetReceivingOperator(Factory, AH_JH); }
		}

		public override ZGuid ReceivingBranch
		{
			get { return IntercompanyInvoiceHelper.GetReceivingBranch(Factory, AH_JH); }
		}

		public override ZGuid ReceivingDepartment
		{
			get { return IntercompanyInvoiceHelper.GetReceivingDepartment(Factory, AH_JH); }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return new InvoicingDocManagerInfo(this, Core.Constants.DocManagerCodes.ReceivableCreditNote);
		}

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region IBadDebtWritingOff Members

		public bool IsWritingOff
		{
			get
			{
				return fIsWritingOff;
			}
			set
			{
				fIsWritingOff = value;
			}
		}

		bool fIsWritingOff;

		#endregion

		#region IAmending Members

		ITransaction IAmending.OriginalTransaction
		{
			get { return Factory.Load<InvoicingBase>(AH_TransactionBelongsToGroup); }
		}

		IAmending IAmending.GenerateAmendingTransaction(string transactionType)
		{
			IAmending result = null;

			if (transactionType == TransactionTypes.CreditNote)
			{
				result = Factory.New<ARCreditNote>();
			}

			if (transactionType == TransactionTypes.Invoice)
			{
				result = Factory.New<ARInvoice>();
			}

			if (result != null)
			{
				result.FlagAsCreatedAmending();
				((InvoicingBase)result).AH_TransactionBelongsToGroup = this.PK;
				result.PopulateFromOriginalTransaction();
			}

			return result;
		}

		void IAmending.FlagAsCreatedAmending() => hasBeenCreatedAsAmending = true;

		bool IAmending.IsAmendingTransaction => this.CheckIsAmendingTransaction(hasBeenCreatedAsAmending);

		bool IAmending.IsOriginalTransaction => !AH_TransactionBelongsToGroup.IsValid && !IsAmendingTransaction_SoftReference;

		ZString IAmending.AmendingReason
		{
			get { return ReversingReason; }
			set { ReversingReason = value; }
		}

		ZString IAmending.AmendingReasonCode
		{
			get { return AH_ReceiptType; }
			set
			{
				if (AH_ReceiptType != value)
				{
					ReversingReason = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.GetDescriptionFromCode(value);
				}

				AH_ReceiptType = value;
			}
		}

		ZGuid[] IAmending.OriginalTransactionJobPKs => this.GetOriginalTransactionJobPKs();

		ZGuid IAmending.OriginalTransactionAccountPK => this.GetOriginalTransactionAccountPK();

		#endregion

		protected override void SetHeaderDetail(InvoicingBase transaction)
		{
			base.SetHeaderDetail(transaction);

			if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled
				&& AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value
				&& !string.IsNullOrEmpty(transaction.AH_TransactionReference)
				&& !string.IsNullOrEmpty(transaction.AH_ComplianceSubType))
			{
				AH_ComplianceSubType = transaction.AH_ComplianceSubType;
			}
		}
	}
}
