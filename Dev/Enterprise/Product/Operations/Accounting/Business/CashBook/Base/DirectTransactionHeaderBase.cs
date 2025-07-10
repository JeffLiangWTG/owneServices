using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract partial class DirectTransactionHeaderBase : TransactionHeaderWithLines, IDocumentSupportable
	{
		#region Schema

		public abstract class DirectTransactionSchema
		{
			public const string AH_Calc_ReceiptTypeLabel = "AH_Calc_ReceiptTypeLabel";
		}

		#endregion

		protected DirectTransactionHeaderBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new DirectTransactionHeaderBaseLookups Lookups
		{
			get { return GetNewLookups() as DirectTransactionHeaderBaseLookups; }
		}

		#region Override

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;
				AH_DueDate = value;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!(this is BankTransferCharge))
			{
				AH_ReceiptType = TransactionType == TransactionTypes.DirectPayment ? AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.Value : AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.Value;
			}

			AH_GB_TaxBranch = AccountingMasterFilesUtils.GetTaxBranchResetValue();
		}

		public override ZExchangeRate ExchangeRate
		{
			get
			{
				if (fAH_ExchangeRate == null)
				{
					fAH_ExchangeRate = new ZAccExchangeRate(this, AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment ? ExchangeRateType.Buy : ExchangeRateType.Sell, AH_ExchangeRateInfo, (ZPropertyInfoString)AH_RX_NKTransactionCurrencyInfo, AH_GCInfo);
					AH_RX_NKTransactionCurrency_ReadOnly = true;
				}
				return fAH_ExchangeRate;
			}
		}

		[List("BankAccounts")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				base.AH_AB = value;
				if (BankAccount != null)
				{
					ExchangeRate.Currency = BankAccount.AB_RX_NKAccountCurrency;
					ChequeBookPK = ZGuid.Empty;
					if (IsCashAccountType)
					{
						AH_ReceiptType = ReceiptTypes.Cash;
					}
				}
			}
		}

		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				if (base.AH_RX_NKTransactionCurrency != value)
				{
					base.AH_RX_NKTransactionCurrency = value;
					SetTransactionLinesCurrency(value);
					RefreshBinding();
				}
			}
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get
			{
				if (ReadonlyConfigForBankStatement)
				{
					return true;
				}

				bool result = false;
				if (!inAV_RX_ReadOnly)
				{
					inAV_RX_ReadOnly = true;
					result = ExchangeRate.Currency_ReadOnly;
					inAV_RX_ReadOnly = false;
				}
				return result;
			}
			set
			{
				if (!inAV_RX_ReadOnly)
				{
					inAV_RX_ReadOnly = true;
					ExchangeRate.Currency_ReadOnly = value;
					inAV_RX_ReadOnly = false;
				}
			}
		}
		bool inAV_RX_ReadOnly;

		public override ZDecimal AH_ExchangeRate
		{
			get { return base.AH_ExchangeRate; }
			set
			{
				base.AH_ExchangeRate = value;
				SetTransactionLinesExchangeRate(value);
				RefreshBinding();
			}
		}

		protected bool AH_ExchangeRate_ReadOnly
		{
			get
			{
				if (ReadonlyConfigForBankStatement)
				{
					return true;
				}

				return CheckCurrencyOnEqualToCurrectCompany
					&& AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public bool CheckCurrencyOnEqualToCurrectCompany { get; set; }

		protected virtual bool AH_ChequeDrawer_ReadOnly => ReadonlyConfigForBankStatement;

		[List("PaymentReceiptMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				if (base.AH_ReceiptType != value)
				{
					AH_ChequeOrReference = "";
				}

				base.AH_ReceiptType = value;
				base.AH_ChequeDrawer = "";
				base.AH_DrawerBank = "";
				base.AH_DrawerBranch = "";

				var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(value);
				if (defaultReferenceNumber.IsEmpty)
				{
					if (value == ZArchitecture.Core.ReceiptTypes.Cash)
					{
						base.AH_ChequeOrReference = "CASH";
					}
				}
				else
				{
					base.AH_ChequeOrReference = defaultReferenceNumber;
				}

				CheckNumberIsAutoAllocated();
			}
		}

		protected void SetReceiptTypeOnly(ZString receiptType)
		{
			base.AH_ReceiptType = receiptType;
		}

		//[ReadOnly(true)]
		//public override ZString AH_TransactionNum
		//{
		//    get { return base.AH_TransactionNum; }
		//    set { base.AH_TransactionNum = value; }
		//}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSTotalAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_ChequeOrReference_ReadOnly
		{
			get { return ReadonlyConfigForBankStatement || IsChequeNumberAutoAllocated || base.AH_ChequeOrReference_ReadOnly; }
			set { base.AH_ChequeOrReference_ReadOnly = value; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsInDatabase && !IsReversed)
			{
				ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(this, InvoiceProcessingLevelIsAllowingToResetBranch.Saving);
			}
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();

			this.QueueForComplianceReports();
		}

		protected override bool AH_PostDate_ReadOnly => ReadonlyConfigForBankStatement;

		protected override bool AH_InvoiceDate_ReadOnly => ReadonlyConfigForBankStatement || base.AH_InvoiceDate_ReadOnly;

		protected virtual bool AH_ReceiptType_ReadOnly => ReadonlyConfigForBankStatement;

		protected virtual bool AH_DrawerBranch_ReadOnly => ReadonlyConfigForBankStatement;

		protected virtual bool AH_DrawerBank_ReadOnly => ReadonlyConfigForBankStatement;

		protected override bool AH_TransactionType_ReadOnly => base.AH_TransactionType_ReadOnly || ReadonlyConfigForBankStatement;

		#endregion

		#region IReversing Override

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			using (((DirectTransactionHeaderBase)fReverseTransaction).Lines.SuspendListChanged())
			using (GetValidationSuspender())
			using (fReverseTransaction.GetValidationSuspender())
			{
				fReverseTransaction.AH_ChequeDrawer = AH_ChequeDrawer;
				fReverseTransaction.AH_DrawerBank = AH_DrawerBank;
				fReverseTransaction.AH_DrawerBranch = AH_DrawerBranch;

				foreach (DirectTransactionLineBase line in Lines)
				{
					using (line.GetValidationSuspender())
					{
						ZDecimal exTaxAmount = -line.AL_OSExTaxAmount;
						ZDecimal taxAmount = -line.AL_OSTaxAmount;

						DirectTransactionLineBase newLine = (DirectTransactionLineBase)((DirectTransactionHeaderBase)fReverseTransaction).Lines.AddNew();
						newLine.AL_AG = line.AL_AG;
						newLine.AL_InputGSTVATRecoverable = line.AL_InputGSTVATRecoverable;
						newLine.AL_GB = line.AL_GB;
						newLine.AL_GE = line.AL_GE;
						newLine.AL_Desc = line.AL_Desc;

						using (newLine.OnRateChangedSuspender.GetSuspender())
						{
							newLine.AL_AT = line.AL_AT;
							newLine.AL_TaxDate = line.AL_TaxDate;
							newLine.AL_TaxRateNumerator = line.AL_TaxRateNumerator;
							newLine.AL_TaxRateDenominator = line.AL_TaxRateDenominator;
							newLine.AL_TaxExtraRateNumerator = line.AL_TaxExtraRateNumerator;
							newLine.AL_TaxExtraRateDenominator = line.AL_TaxExtraRateDenominator;
						}

						newLine.AL_A9_VATClass = line.AL_A9_VATClass;
						newLine.AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
						newLine.AL_ExchangeRate = line.AL_ExchangeRate;

						newLine.AL_OSExTaxAmount = exTaxAmount;
						newLine.AL_OSTaxAmount = taxAmount;

						newLine.AL_LineAmount = -line.AL_LineAmount;
						newLine.AL_GSTVAT = -line.AL_GSTVAT;
						newLine.AL_OSAmount = -line.AL_OSAmount;

						SubAccountHelper.CopySubAccounts(newLine, line, true);

						newLine.AL_GovtChargeCode = line.AL_GovtChargeCode;
						newLine.AL_SupplyType = line.AL_SupplyType;
						newLine.AL_GB_TaxBranch = line.AL_GB_TaxBranch;
					}
				}

				fReverseTransaction.AH_OSExTaxAmount = -AH_OSExTaxAmount;
				fReverseTransaction.AH_OSTaxAmount = -AH_OSTaxAmount;

				fReverseTransaction.AH_InvoiceAmount = -AH_InvoiceAmount;
				fReverseTransaction.AH_GSTAmount = -AH_GSTAmount;
				fReverseTransaction.AH_OSTotal = -AH_OSTotal;

				fReverseTransaction.AH_LocalOutstandingAmount = 0m;
			}
		}

		#endregion

		#region Collections

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
					fBankAccounts.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("cc831d19-be6c-4cc1-98c2-0060ebafe261", "This bank account cannot be chosen because it belongs to another branch. Please choose another bank account"));
				}
				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		public AccChequeBookCollection ChequeBooks
		{
			get
			{
				ZQuery filter = new ZQuery(AccChequeBookSchema.AK_GB, GlbBranch.CurrentBranch.PK);

				if (AH_AB.IsValid)
				{
					filter.AddToFilter(AccChequeBookSchema.AK_AB, AH_AB);
				}

				AccChequeBookCollection result = new ActiveChequeBookCollection(Factory, filter);
				result.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("c4bfbd87-77e0-417c-9df1-a61e8f8fcfd0", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
				return result;
			}
		}

		#endregion

		#region Calculated Properties

		[List("ChequeBooks")]
		public virtual ZGuid ChequeBookPK
		{
			get
			{
				if (IsInDatabase
					&& AH_TransactionType == TransactionTypes.DirectPayment
					&& AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque
					&& !fChequeBookPK.IsValid)
				{
					SetChequeBookFromBankAndChequeNumber();
				}

				return fChequeBookPK;
			}
			set
			{
				SetNonPersistentPropertyValue(ChequeBookPKInfo, ref fChequeBookPK, value);
				if (ChequeBook != null && !IsChequeNumberAutoAllocated)
				{
					AH_ChequeOrReference = ChequeBook.AK_CurrentNo.ToString();
				}
				DirectTransactionHeaderBaseValidation directTransactionValidation = Validation as DirectTransactionHeaderBaseValidation;
				if (directTransactionValidation != null && !IsValidationSuspended)
				{
					directTransactionValidation.ValidateChequeBookPK();
				}
				CheckNumberIsAutoAllocated();
			}
		}
		ZGuid fChequeBookPK;

		public virtual ZPropertyInfo ChequeBookPKInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeBookPK)); }
		}

		public AccChequeBook ChequeBook
		{
			get
			{
				if (fChequeBook == null || fChequeBook.PK != ChequeBookPK)
				{
					if (IsInDatabase
						&& AH_TransactionType == TransactionTypes.DirectPayment
						&& AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque
						&& !fChequeBookPK.IsValid)
					{
						SetChequeBookFromBankAndChequeNumber();
					}
					else
					{
						fChequeBook = Factory.Load<AccChequeBook>(ChequeBookPK);
					}
				}
				return fChequeBook == null || fChequeBook.IsDeleted ? null : fChequeBook;
			}
		}

		void SetChequeBookFromBankAndChequeNumber()
		{
			ZDecimal chequeNumber = ZDecimal.ParseSafe(AH_ChequeOrReference, ZDecimal.Zero);

			if (chequeNumber != ZDecimal.Zero)
			{
				ZQuery findChequeBookQuery = new ZQuery(AccChequeBookSchema.AK_AB, AH_AB);
				findChequeBookQuery.AddToFilter(AccChequeBookSchema.AK_GB, AH_GB);
				findChequeBookQuery.AddToFilter(AccChequeBookSchema.AK_StartNo, SQLComparisonOperator.LessThanOrEqualTo, chequeNumber);
				findChequeBookQuery.AddToFilter(AccChequeBookSchema.AK_LastNo, SQLComparisonOperator.GreaterThanOrEqualTo, chequeNumber);
				AccChequeBook chequeBook = Factory.LoadTop1<AccChequeBook>(findChequeBookQuery);

				if (chequeBook != null)
				{
					fChequeBookPK = chequeBook.PK;
					fChequeBook = chequeBook;
				}
			}
		}

		AccChequeBook fChequeBook;

		public ZString AH_ChequeOrReferenceLabel_Calc
		{
			get
			{
				return AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque ?
							Res.GetString("DirectTransactionHeaderBase|ChequeNo", "Check No.") :
							Res.GetString("DirectTransactionHeaderBase|ReferenceNo", "Reference No.");
			}
		}

		[MaxLength(20)]
		public ZString AH_Calc_ReceiptTypeLabel
		{
			get { return AH_ChequeOrReferenceLabel_Calc + ":"; }
		}

		public ZPropertyInfo AH_Calc_ReceiptTypeLabelInfo
		{
			get { return GetZPropertyInfo(DirectTransactionSchema.AH_Calc_ReceiptTypeLabel); }
		}

		protected virtual ZBool IsChequeNumberAutoAllocated
		{
			get { return ZBool.False; }
		}

		public override ZBool IsTaxed
		{
			get
			{
				if (!IsInDatabase)
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

		ZBool LinesContainTax
		{
			get
			{
				return Lines.Cast<DependentTransactionLine>().Any(x => x.TaxRate != null);
			}
		}

		public override bool NeedPlaceOfSupplyAtHeaderLevel => NeedPlaceOfSupplyAtLineLevel && IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled;

		public override bool NeedPlaceOfSupplyAtLineLevel => PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Company);

		protected abstract bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled { get; }

		#endregion

		#region RelatedStatementPK

		ZGuid fRelatedStatementPK;
		public ZGuid RelatedStatementPK
		{
			get { return fRelatedStatementPK; }
			set { fRelatedStatementPK = value; }
		}

		#endregion

		#region Implementation

		protected void ResetChequeBook()
		{
			ChequeBookPK = ZGuid.Empty;
			ChequeBookPKInfo.RefreshBinding();
		}

		protected virtual void CheckNumberIsAutoAllocated()
		{
			if (IsChequeNumberAutoAllocated)
			{
				AH_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		ZBool ReadonlyConfigForBankStatement { get; set; } = false;

		public void SetReadOnlyForBankStatement(bool value) => ReadonlyConfigForBankStatement = value;

		public override ZBool CanApplyTaxBranch => AccountingMasterFilesUtils.IsTaxBranchApplicable;
	}
}
