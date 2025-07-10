using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	public partial class ARInvoice : Invoice, IARInvoice, IDocManagerSupport, IBadDebtWritingOff, IAmending, IRelatableActivity, IEDocsParsingSupport
	{
		public ARInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoAccTransactionHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ARInvoice LoadTop1NotReversed(ZGuid jobPK, ZGuid[] chargeCodePKs)
			{
				var result = LoadNotReversed(jobPK, chargeCodePKs);
				return result.Length > 0 ? result[0] : null;
			}

			/// <summary>
			/// Will return the first record matching the parameters which is not reversed
			/// </summary>
			public ARInvoice[] LoadNotReversed(ZGuid jobPK, ZGuid[] chargeCodePKs)
			{
				ZQuery linesFilter = new ZQuery(AccTransactionLinesSchema.AL_JH, jobPK);
				linesFilter.AddToFilter(AccTransactionLinesSchema.AL_AC, chargeCodePKs);
				linesFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
				AccTransactionLinesCollection collection = new AccTransactionLinesCollection(Factory, linesFilter);
				collection.Load();

				ZQuery filter = new ZQuery(new ZQuery(AccTransactionHeaderSchema.PK, collection.GetFieldValues(AccTransactionLinesSchema.AL_AH)));
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
				filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				return Factory.Load<ARInvoice>(filter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(ARInvoice);
			}
		}

		#region New Properties

		public ZString DeclarationEntryDetails
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsParentFromThisTable(JobDeclarationSchema.Constants.Prefix) || IsParentFromThisTable(JobShipmentSchema.Constants.Prefix))
				{
					BaseJobDeclaration declaration = null;
					if (IsParentFromThisTable(JobDeclarationSchema.Constants.Prefix))
					{
						declaration = Factory.Load<BaseJobDeclaration>(Job.JH_ParentID);
					}
					else
					{
						ForwardingShipment shipment = Factory.Load<ForwardingShipment>(Job.JH_ParentID);
						if (shipment != null)
						{
							foreach (BaseJobDeclaration oneDec in shipment.Declarations)
							{
								if (oneDec.Branch != null && oneDec.Branch.GB_GC == Job.JH_GC)
								{
									declaration = oneDec;
									break;
								}
							}
						}
					}

					result = declaration != null ? declaration.EntryDetailsInARInvoice : ZString.Empty;
				}
				return result;
			}
		}

		bool IsParentFromThisTable(string tablePrefix)
		{
			bool result = false;

			if (Job != null)
			{
				result = Job.JH_ParentTableCode == tablePrefix;
			}

			return result;
		}

		#endregion

		#region Overrides

		public override bool CheckLevelSecurityRights()
		{
			bool result = true;
			if (IsCreatingCreditNoteForReversal
				&& !IsCreatedFromApprovalRequest
				&& EnforceTwoApproversWhenPostingARCredit)
			{
				AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement = AuthorisationRequired;
				if (Level1AuthorisationRequired(authorisationRequirement))
				{
					result = SecurityOverrideProvider.SecurityCertificates[FirstApprovalCheckpoint].IsAllowed;
#if DEBUG
					SecurityOverrideProviderIsInvoked = true;
#endif
				}
				if (Level2AuthorisationRequired(authorisationRequirement))
				{
					result &= SecurityOverrideProvider.SecurityCertificates[SecondApprovalCheckpoint].IsAllowed;
#if DEBUG
					SecurityOverrideProviderIsInvoked = true;
#endif
				}
			}
			else
			{
				result = base.CheckLevelSecurityRights();
			}
			return result;
		}

#if DEBUG
		public bool SecurityOverrideProviderIsInvoked;
#endif

		protected override bool AH_InvoiceTerm_ReadOnly
		{
			get
			{
				if (IsAmendingTransaction)
				{
					return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
						|| !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AllowOverrideARInvoiceTermWAmendTransactionInvoice);
				}
				else
				{
					return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
						|| !Env.Security.NewReceivablesInvoiceTerm.IsAllowed;
				}
			}
		}

		protected override bool AH_InvoiceTermDays_ReadOnly
		{
			get
			{
				return AH_InvoiceTerm_ReadOnly || AH_InvoiceTerm == Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery || AH_InvoiceTerm == Enterprise.Core.Constants.InvoiceTerms.PaymentInAdvance;
			}
		}

		protected override bool AH_InvoiceDate_ReadOnly
		{
			get
			{
				return base.AH_InvoiceDate_ReadOnly || !Env.Security.NewReceivablesInvoiceInvoiceDate.IsAllowed;
			}
		}

		public override bool AH_ComplianceSubType_ReadOnly
		{
			get
			{
				var countryComplianceInfoExtension = CountryComplianceEInvoicingExtensionFactory.GetCountryComplianceInfoExtension(Company.GC_RN_NKCountryCode) as IComplianceSubTypeGUIProvider;
				if (countryComplianceInfoExtension != null)
				{
					return countryComplianceInfoExtension.ComplianceSubTypeIsReadOnly(hasBeenCreatedAsAmending, AH_Ledger, AH_TransactionType, OriginalTransactionReference.IsEmpty);
				}

				return base.AH_ComplianceSubType_ReadOnly;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("95792ded-a4a8-4b0c-a2f8-4e4ab6faed08", "Accounts Receivable Invoice"); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(ARInvoiceLine); }
		}

		public override bool IsAllowModifyAmendStatusCode
		{
			get
			{
				var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
				var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
				return (amendStatusCodeProvider?.ShouldShowAmendStatusCode() ?? false) && IsAmendingTransaction && !IsInDatabase;
			}
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				if (AH_TransactionType == TransactionTypes.CreditNote && !AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.Value.Value)
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARCreditNoteNo;
				}
				else
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARInvoiceNo;
				}
			}
		}

		protected override ReceiptPaymentBase NewReceiptPayment
		{
			get { return Factory.New<ARReceipt>(); }
		}

		[List("ReceiptMethods")]
		public override ZString ReceiptPaymentAH_ReceiptType
		{
			get { return base.ReceiptPaymentAH_ReceiptType; }
			set
			{
				if (base.ReceiptPaymentAH_ReceiptType != value)
				{
					base.ReceiptPaymentAH_ReceiptType = value;
					ResetChequeDetails(value);
					RefreshChequePropertiesReadOnly();
				}
			}
		}

		/// <summary>
		/// Get bank account marked as default, whose currency matches the Debtor's default currency
		/// </summary>
		protected override ZGuid DefaultBankAccount
		{
			get
			{
				ZQuery filter = new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True);
				filter.AddToFilter(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);

				if (Header != null && Header.CompanyData.ARDDefltCurrency != null)
				{
					filter.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, Header.CompanyData.ARDDefltCurrency.RX_Code);
				}
				else
				{
					filter.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, AH_RX_NKTransactionCurrency);
				}
				AccBankAccount[] accounts = (AccBankAccount[])Factory.Load(typeof(AccBankAccount), filter);

				AccBankAccount nonBranchAccount = null;
				if (accounts.Length == 0)
				{
					return ZGuid.Empty;
				}
				else if (accounts.Length == 1)
				{
					return accounts[0].PK;
				}
				else //Find one that matches current branch or has no branch
				{
					foreach (AccBankAccount account in accounts)
					{
						if (account.AB_GB == GlbBranch.CurrentBranch.PK)
						{
							return account.PK;
						}
						else if (!account.AB_GB.IsValid && nonBranchAccount == null)
						{
							nonBranchAccount = account;
						}
					}
					return nonBranchAccount != null ? nonBranchAccount.PK : ZGuid.Empty;
				}
			}
		}

		protected bool ReceiptPaymentAH_ChequeDrawer_ReadOnly
		{
			get { return ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cheque; }
		}

		protected bool ReceiptPaymentAH_DrawerBranch_ReadOnly
		{
			get { return ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cheque; }
		}

		protected bool ReceiptPaymentAH_DrawerBank_ReadOnly
		{
			get { return ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cheque; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected void RefreshChequePropertiesReadOnly()
		{
			ArrayList propertiesToRefreshBinding = new ArrayList();
			propertiesToRefreshBinding.Add(ReceiptPaymentAH_ChequeDrawerInfo);
			propertiesToRefreshBinding.Add(ReceiptPaymentAH_DrawerBranchInfo);
			propertiesToRefreshBinding.Add(ReceiptPaymentAH_DrawerBankInfo);
			RefreshPropertyBinding((ZPropertyInfo[])propertiesToRefreshBinding.ToArray(typeof(ZPropertyInfo)));
		}

		protected void RefreshPropertyBinding(ZPropertyInfo[] refreshProperties)
		{
			foreach (ZPropertyInfo property in refreshProperties)
			{
				property.RefreshBinding();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (this.IsStampDutyApplicable())
			{
				if (this.ShouldAddStampDuty())
				{
					this.AddStampDutyLine();
				}

				Logs.AddNew(AutoEvents.StampDutyLiability);
			}

			base.OnFactorySavingBeforeTransactionCore();

			var jobStatusesRegistry = AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.GetFallBackValueAtAllLevels(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var statusesWhenARInvoicePostingCausesStatusChange = jobStatusesRegistry.Cast<CodeDescriptionBool>().Where(x => !x.Bool).Select(x => x.Code).ToArray();
			if (!IsInDatabase && !IsConsolInvoice && statusesWhenARInvoicePostingCausesStatusChange.Any())
			{
				HashSet<ZGuid> invoiceJobs = new HashSet<ZGuid>();
				if (InvoicingJob != null)
				{
					invoiceJobs.Add(InvoicingJob.PK);
				}
				else
				{
					foreach (InvoicingLineBase line in Lines)
					{
						if (line.Job != null)
						{
							invoiceJobs.Add(line.InvoicingJob.PK);
						}
					}
				}

				ZGuid[] invoiceJobsArray = invoiceJobs.ToArray();
				ZDBOnlyQuery jobFilter = new ZDBOnlyQuery(typeof(Job));
				jobFilter.AddToFilter(JobHeaderSchema.PK, invoiceJobsArray);
				jobFilter.AddToFilter(JobHeaderSchema.JH_Status, statusesWhenARInvoicePostingCausesStatusChange);

				Job[] jobsToUpdate = Factory.Load<Job>(jobFilter);

				foreach (Job job in jobsToUpdate)
				{
					using (job.GetValidationSuspender())
					{
						job.JH_Status = JobHeaderStatus.JobInvoiced.Code;
					}
				}
			}

			UpdateJobChargeOverrideAddressContact();
		}

		protected override void OnSavingCore()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			if (ReceiptPaymentAH_ReceiptType != ReceiptTypes.Cheque)
			{
				ReceiptPaymentAH_ChequeDrawer = ZString.Empty;
				ReceiptPaymentAH_DrawerBranch = ZString.Empty;
				ReceiptPaymentAH_DrawerBank = ZString.Empty;
			}

			base.OnSavingCore();
		}

		protected override void JobRelatedLogicOnCopiedLine(InvoiceLine oldLine, InvoiceLine newLine)
		{
			newLine.InternalAL_JH_ReadOnly = true;
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(ARCreditNote); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(ARInvoice); }
		}

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
				if (IsWritingOff && (sourceLine.ChargeCode == null || sourceLine.ChargeCode.AC_ChargeType != Constants.ChargeType.Comment))
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

		#region Invoice Amount Levels

		protected override ZString AuthorisationLevelCore()
		{
			ZString resultAuthorisationSettings = null;
			if (IsInUnapprovedTransactionContext)
			{
				if (AuthorisationLevelForIntercompanyInvoice == null)
				{
					var converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
					try
					{
						var invoice = converter.ConvertToAP(this, true, generateInvoicePdf: false);
						if (invoice != null)
						{
							AuthorisationLevelForIntercompanyInvoice = invoice.AuthorisationLevel;
							#region Test
#if DEBUG
							if (Globals.IsTest)
							{
								ConvertedAPInvoiceForTest = invoice;
							}
#endif
							#endregion
						}
					}
					catch (JobCreationException)
					{
						return resultAuthorisationSettings;
					}
				}

				return AuthorisationLevelForIntercompanyInvoice;
			}

			resultAuthorisationSettings = base.AuthorisationLevelCore();

			return resultAuthorisationSettings;
		}
		string AuthorisationLevelForIntercompanyInvoice;

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry =>
			IsCreatingCreditNoteForReversal
				? AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings
				: null;

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode)
		{
			if (IsCreatingCreditNoteForReversal)
			{
				return GetCheckPointForLevel(levelCode);
			}
			else
			{
				return null;
			}
		}

		protected override ZString MaxAuthorisationLevelCore()
		{
			ZString result = ZString.Empty;
			if (IsInUnapprovedTransactionContext && Company != null)
			{
				result = GetMaxCompanyAuthorizationLevel(Company.GC_Code);
			}
			return result.IsEmpty ? new ZString(Res.GetString("b203fb2a-a8db-495c-b6dd-8f1464ca3051", "Not Defined")) : result;
		}

		#endregion

		protected override void SetChequeDetails()
		{
			ReceiptPaymentAH_ChequeDrawer = base.DefaultChequeDrawer;
			ReceiptPaymentAH_DrawerBank = base.DefaultDrawerBank;
			ReceiptPaymentAH_DrawerBranch = base.DefaultDrawerBranch;
		}

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

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting; }
		}

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new InvoiceValidation(this);
		}

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return ARInvoiceDocManagerInfo.New(this, Core.Constants.DocManagerCodes.ReceivableInvoice);
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

		protected override bool OriginalTransactionReference_ReadOnlyCore()
		{
			return (IsAmendingTransaction_StrongReference && hasBeenCreatedAsAmending) || AreOriginalReferenceFieldsReadOnly;
		}

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

		bool IAmending.IsOriginalTransaction
		{
			get { return !AH_TransactionBelongsToGroup.IsValid; }
		}

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

		ZGuid[] IAmending.OriginalTransactionJobPKs
		{
			get { return this.GetOriginalTransactionJobPKs(); }
		}

		ZGuid IAmending.OriginalTransactionAccountPK
		{
			get { return this.GetOriginalTransactionAccountPK(); }
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.ArInvoice; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new ZString[] { AH_TransactionType, AH_Desc, AH_InvoiceDate.ToShortDateString(), AH_DueDate.ToShortDateString() }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}

		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		protected override void DeleteCore()
		{
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();

			base.DeleteCore();
		}

		void ResetChequeDetails(ZString receiptType)
		{
			if (receiptType == ReceiptTypes.Cheque)
			{
				SetChequeDetails();
			}
			else
			{
				ReceiptPaymentAH_ChequeDrawer = ZString.Empty;
				ReceiptPaymentAH_DrawerBank = ZString.Empty;
				ReceiptPaymentAH_DrawerBranch = ZString.Empty;
			}
		}

		#region Cash Advance

		protected internal override Journal.Journal[] LoadOverpaymentCAIJournals()
		{
			return Factory.Load<Journal.ARJournal>(GetOverpaymentCAIJournalsQuery());
		}

		protected override void AcceptCore(ICashAdvanceRequestProcessingByInvoiceVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected internal override bool IsCashAdvanceFunctionalityEnabled => ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled;

		protected internal override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed => ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;

		#endregion

		#region Vietnam Specific

		protected override void SetHeaderDetail(InvoicingBase transaction)
		{
			base.SetHeaderDetail(transaction);

			if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled
				&& AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Value
				&& !string.IsNullOrEmpty(transaction.AH_TransactionReference)
				&& !string.IsNullOrEmpty(transaction.AH_ComplianceSubType))
			{
				AH_ComplianceSubType = transaction.AH_ComplianceSubType;
			}
		}

		#endregion

		#region Test
#if DEBUG

		#region Authorisation Level Not Generate Invoice Pdf Test

		internal InvoicingBase ConvertedAPInvoiceForTest;

		#endregion

#endif
		#endregion
	}
}
