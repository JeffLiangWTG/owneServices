using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract partial class Invoice : InvoicingBase, IInvoiceAssociatedToCashAdvanceRequest
	{
		public Invoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ZString TransactionType
		{
			get { return TransactionTypes.Invoice; }
		}

		public override bool IsCommissionable
		{
			get { return AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.AccountsReceivable; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			UpdateRelevantCashAdvanceRequests();
		}

		protected override void OnSavingBeforeBase()
		{
			HandleReceiptPayment();
			CreateInvoiceAndReceiptMatchLink();
			MatchWithCashAdvanceRequests();
			base.OnSavingBeforeBase();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				if (ReceiptPayment != null)
				{
					TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
					ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK);
					filter.AddToFilter(JoinCondition.Or, AccTransactionMatchLinkSchema.AP_AH, ReceiptPayment.PK);
					matchLinkCollection.Load(filter);
					foreach (TransactionMatchLink matchLink in matchLinkCollection)
					{
						matchLink.Unmatch();
					}
					matchLinkCollection.RemoveAndDeleteAll();
					ReceiptPayment.DeleteFromDB();
					ReceiptPayment = null;
				}
			}
		}

		#region Property Overrides

		#region AH_OH
		protected override ZGuid AH_OHCore
		{
			get
			{
				return base.AH_OHCore;
			}
			set
			{
				base.AH_OHCore = value;

				if (Header != null)
				{
					IsInvoiceReceiptPayment = DefaultIsCashInvoice;

					if (InvoicingValidation != null)
					{
						InvoicingValidation.ValidateOrgDependantLineItems();
					}

					SetWHTReadOnlyState();
				}
			}
		}

		#endregion

		protected virtual bool AH_OSTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSWHTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_LocalExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalWHTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_LocalExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		public override ZGuid OriginalTransactionReference
		{
			get
			{
				return AH_TransactionBelongsToGroup;
			}
			set
			{
				if (OriginalTransactionReference != value)
				{
					var oldValue = base.OriginalTransactionReference;
					base.OriginalTransactionReference = value;
					AH_TransactionBelongsToGroup = value;
					if (OriginalTransactionIsSet)
					{
						AH_OriginalTransactionNum = ZString.Empty;
						AH_OriginalInvoiceDate = ZDate.Empty;
					}
					if (oldValue != value)
					{
						ReasonCode = string.Empty;
						ReasonDescription = string.Empty;
						ReasonCodeInfo.RefreshBinding();
						ReasonDescriptionInfo.RefreshBinding();
					}

					OriginalTransactionReferenceInfo.RefreshBinding();

					if (Validation is InvoiceValidation invoiceValidation)
					{
						invoiceValidation.ValidateOriginalTransactionReference();
					}
				}

				PopulateFromOriginalTransaction(OriginalReferenceTransaction);
			}
		}

		#endregion

		#endregion

		#region Calculated Properites

		#region ReceiptPaymentAH_AB

		[List("BankAccountLookup")]
		public virtual ZGuid ReceiptPaymentAH_AB
		{
			get { return fReceiptPaymentAH_AB; }
			set
			{
				if (fReceiptPaymentAH_AB != value)
				{
					SetNonPersistentPropertyValue(ReceiptPaymentAH_ABInfo, ref fReceiptPaymentAH_AB, value);
					ResetChequeBookCollection();
					ReceiptPaymentAK_AB = ZGuid.Empty;
					ReceiptPaymentAK_ABInfo.RefreshBinding();

					if (ReceiptPaymentBankAccount?.IsCashAccount ?? false)
					{
						ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
					}
				}
				if (!IsValidationSuspended)
				{
					InvoiceValidation validation = Validation as InvoiceValidation;
					if (validation != null)
					{
						validation.ValidateReceiptPaymentAH_AB();
					}
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_ABInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_AB), "Bank Account"); }
		}

		ZGuid fReceiptPaymentAH_AB;

		#endregion

		#region ReceiptPaymentAH_ReceiptType

		ZString fReceiptPaymentAH_ReceiptType;

		[MaxLength(AccTransactionHeader.Schema.AH_ReceiptTypeMaxLength)]
		public virtual ZString ReceiptPaymentAH_ReceiptType
		{
			get
			{
				return fReceiptPaymentAH_ReceiptType;
			}
			set
			{
				if (fReceiptPaymentAH_ReceiptType != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_ReceiptTypeInfo, value);
					SetNonPersistentPropertyValue(ReceiptPaymentAH_ReceiptTypeInfo, ref fReceiptPaymentAH_ReceiptType, value);
					InvoiceValidation invoiceValidation = Validation as InvoiceValidation;

					var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(value);
					if (defaultReferenceNumber.IsEmpty)
					{
						if (value == ReceiptTypes.Cash)
						{
							if (ReceiptPaymentAH_ChequeOrReference.IsEmpty)
							{
								defaultReferenceNumber = "CASH";
							}

							ReceiptPaymentAK_AB = ZGuid.Empty;
						}
						else
						{
							defaultReferenceNumber = ZString.Empty;
						}
					}
					ReceiptPaymentAH_ChequeOrReference = defaultReferenceNumber;

					if (!IsValidationSuspended && invoiceValidation != null)
					{
						invoiceValidation.ValidateReceiptPaymentCardSecurityCode();
						invoiceValidation.ValidateReceiptPaymentAH_AB();
						invoiceValidation.ValidateReceiptPaymentAH_ReceiptType();
						invoiceValidation.ValidateAH_OH();
						invoiceValidation.ValidateReceiptPaymentAH_ChequeOrReference();
					}
					ReceiptPaymentCardSecurityCode = ZString.Empty;
				}
				ReceiptPaymentAK_ABInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_ReceiptTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_ReceiptType)); }
		}

		protected virtual bool ReceiptPaymentAK_AB_ReadOnly
		{
			get { return fReceiptPaymentAH_ReceiptType == ReceiptTypes.Cash; }
		}

		#endregion

		#region ReceiptPaymentAH_Desc

		ZString fReceiptPaymentAH_Desc;

		[MaxLength(AccTransactionHeader.Schema.AH_DescMaxLength)]
		public virtual ZString ReceiptPaymentAH_Desc
		{
			get => fReceiptPaymentAH_Desc;
			set
			{
				if (fReceiptPaymentAH_Desc != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_DescInfo, value);
					fReceiptPaymentAH_Desc = value;
					if (Validation is InvoiceValidation invoiceValidation)
					{
						invoiceValidation.ValidateReceiptPaymentAH_Desc();
					}
					ReceiptPaymentAH_DescInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo ReceiptPaymentAH_DescInfo => GetZPropertyInfo(nameof(ReceiptPaymentAH_Desc));

		#endregion

		#region ReceiptPaymentAH_ChequeOrReference

		ZString fReceiptPaymentAH_ChequeOrReference;
		[MaxLength(AccTransactionHeader.Schema.AH_ChequeOrReferenceMaxLength)]
		public virtual ZString ReceiptPaymentAH_ChequeOrReference
		{
			get { return fReceiptPaymentAH_ChequeOrReference; }
			set
			{
				if (fReceiptPaymentAH_ChequeOrReference != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_ChequeOrReferenceInfo, value);
					fReceiptPaymentAH_ChequeOrReference = value;
					InvoiceValidation invoiceValidation = Validation as InvoiceValidation;
					if (invoiceValidation != null)
					{
						invoiceValidation.ValidateReceiptPaymentAH_ChequeOrReference();
					}
					ReceiptPaymentAH_ChequeOrReferenceInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo ReceiptPaymentAH_ChequeOrReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_ChequeOrReference)); }
		}
		#endregion

		#region ReceiptPaymentAH_DrawerBranch

		ZString fReceiptPaymentAH_DrawerBranch;
		[MaxLength(AccTransactionHeader.Schema.AH_DrawerBranchMaxLength)]
		public ZString ReceiptPaymentAH_DrawerBranch
		{
			get
			{
				return fReceiptPaymentAH_DrawerBranch;
			}
			set
			{
				if (fReceiptPaymentAH_DrawerBranch != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_DrawerBranchInfo, value);
					fReceiptPaymentAH_DrawerBranch = value;
					ReceiptPaymentAH_DrawerBranchInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_DrawerBranchInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_DrawerBranch)); }
		}
		#endregion

		#region ReceiptPaymentAH_ChequeDrawer

		ZString fReceiptPaymentAH_ChequeDrawer;
		[MaxLength(AccTransactionHeader.Schema.AH_ChequeDrawerMaxLength)]
		public ZString ReceiptPaymentAH_ChequeDrawer
		{
			get
			{
				return fReceiptPaymentAH_ChequeDrawer;
			}
			set
			{
				if (fReceiptPaymentAH_ChequeDrawer != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_ChequeDrawerInfo, value);
					fReceiptPaymentAH_ChequeDrawer = value;
					ReceiptPaymentAH_ChequeDrawerInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_ChequeDrawerInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_ChequeDrawer)); }
		}
		#endregion

		#region ReceiptPaymentAH_DrawerBank

		ZString fReceiptPaymentAH_DrawerBank;
		[MaxLength(AccTransactionHeader.Schema.AH_DrawerBankMaxLength)]
		public ZString ReceiptPaymentAH_DrawerBank
		{
			get
			{
				return fReceiptPaymentAH_DrawerBank;
			}
			set
			{
				if (fReceiptPaymentAH_DrawerBank != value)
				{
					CheckMaximumLength(ReceiptPaymentAH_DrawerBankInfo, value);
					fReceiptPaymentAH_DrawerBank = value;
					ReceiptPaymentAH_DrawerBankInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_DrawerBankInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_DrawerBank)); }
		}
		#endregion

		#region ReceiptPaymentAH_PostDate

		protected bool ReceiptPaymentAH_PostDate_ReadOnly
		{
			get { return false; }
		}

		public ZDateTime ReceiptPaymentAH_PostDate
		{
			get
			{
				return fReceiptPaymentAH_PostDate;
			}
			set
			{
				if (fReceiptPaymentAH_PostDate != value)
				{
					SetNonPersistentPropertyValue(ReceiptPaymentAH_PostDateInfo, ref fReceiptPaymentAH_PostDate, value);
				}
				if (!IsValidationSuspended)
				{
					InvoiceValidation validation = Validation as InvoiceValidation;
					if (validation != null)
					{
						validation.ValidateReceiptPaymentAH_PostDate();
					}
				}
			}
		}
		ZDateTime fReceiptPaymentAH_PostDate;

		public ZPropertyInfo ReceiptPaymentAH_PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_PostDate)); }
		}

		#endregion

		#region ReceiptPaymentAH_InvoiceDate

		public ZDateTime ReceiptPaymentAH_InvoiceDate
		{
			get
			{
				return fReceiptPaymentAH_InvoiceDate;
			}
			set
			{
				if (fReceiptPaymentAH_InvoiceDate != value)
				{
					SetNonPersistentPropertyValue(ReceiptPaymentAH_InvoiceDateInfo, ref fReceiptPaymentAH_InvoiceDate, value);
				}

				if (!IsValidationSuspended)
				{
					InvoiceValidation validation = Validation as InvoiceValidation;
					if (validation != null)
					{
						validation.ValidateReceiptPaymentAH_InvoiceDate();
					}
				}
			}
		}
		ZDateTime fReceiptPaymentAH_InvoiceDate;

		public ZPropertyInfo ReceiptPaymentAH_InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_InvoiceDate), Res.GetString("5AA379CD-9CC3-4344-B455-87A801F46B01", "Receipt/Payment Invoice Date")); }
		}

		#endregion

		#region ReceiptPaymentAH_ChequeReferenceLabel
		public ZString ReceiptPaymentAH_ChequeReferenceLabel
		{
			get { return ReceiptPaymentAH_ReceiptType == ReceiptTypes.Cheque ? ChequeNumberLabel : ReferenceNumberLabel; }
		}

		public ZPropertyInfo ReceiptPaymentAH_ChequeReferenceLabelInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_ChequeReferenceLabel)); }
		}

		public ZString ChequeNumberLabel
		{
			get { return Res.GetString("23aec6aa-e9f8-4302-b5ed-03ba545f7ed0", "Check Number"); }
		}

		public ZString ReferenceNumberLabel
		{
			get { return Res.GetString("56EC5CB2-F10B-42c1-9868-C51F21F37687", "Reference Number"); }
		}

		#endregion

		#region ReceiptPaymentAK_AB

		ZGuid fReceiptPaymentAK_AB;

		[List("ChequeBookLookup")]
		public virtual ZGuid ReceiptPaymentAK_AB
		{
			get { return fReceiptPaymentAK_AB; }
			set
			{
				if (fReceiptPaymentAK_AB != value)
				{
					SetNonPersistentPropertyValue(ReceiptPaymentAK_ABInfo, ref fReceiptPaymentAK_AB, value);
					if (fReceiptPaymentAK_AB.IsValid)
					{
						if (ChequeBook != null && !IsChequeNumberAutoAllocated)
						{
							ReceiptPaymentAH_ChequeOrReference = ChequeBook.AK_CurrentNo.ToString();
							ReceiptPaymentAH_ChequeOrReferenceInfo.RefreshBinding();
						}
					}
					else
					{
						ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAK_ABInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAK_AB)); }
		}
		#endregion

		#region ReceiptPaymentAH_OSTotalAmount

		ZDecimal fReceiptPaymentAH_OSTotalAmount;
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public virtual ZDecimal ReceiptPaymentAH_OSTotalAmount
		{
			get { return fReceiptPaymentAH_OSTotalAmount; }
			set
			{
				if (fReceiptPaymentAH_OSTotalAmount != value)
				{
					fReceiptPaymentAH_OSTotalAmount = value;
					InvoiceValidation invoiceValidation = Validation as InvoiceValidation;
					if (invoiceValidation != null)
					{
						invoiceValidation.ValidateReceiptPaymentAH_OSTotalAmount();
					}
					ReceiptPaymentAH_OSTotalAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiptPaymentAH_OSTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_OSTotalAmount)); }
		}

		#endregion

		#region ReceiptPaymentAH_OSTotalAmount_ReadOnly

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal ReceiptPaymentAH_OSTotalAmount_ReadOnly
		{
			get { return ReceiptPaymentAH_OSTotalAmount; }
		}

		public ZPropertyInfo ReceiptPaymentAH_OSTotalAmount_ReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAH_OSTotalAmount_ReadOnly)); }
		}

		#endregion

		#region IsInvoiceReceiptPayment

		ZBool fIsInvoiceReceiptPayment;
		[BusinessObjectTestExclude]
		public virtual ZBool IsInvoiceReceiptPayment
		{
			get
			{
				return fIsInvoiceReceiptPayment;
			}
			set
			{
				if (CanSetIsInvoiceReceiptPayment && SubmittedFromInvoicingForm && fIsInvoiceReceiptPayment != value)
				{
					fIsInvoiceReceiptPayment = value;

					using (GetValidationSuspender())
					{
						if (value)
						{
							SetReceiptPaymentDefaults();
						}
						else
						{
							ClearReceiptPaymentFields();
						}
					}

					InvoiceValidation validation = Validation as InvoiceValidation;
					if (validation != null)
					{
						validation.ValidateReceiptPaymentCardSecurityCode();
					}
				}
				IsInvoiceReceiptPaymentInfo.RefreshBinding();
			}
		}

		void ClearReceiptPaymentFields()
		{
			ReceiptPaymentAH_ReceiptType = ZString.Empty;
			ReceiptPaymentAH_AB = ZGuid.Empty;
			ReceiptPaymentAK_AB = ZGuid.Empty;
			ReceiptPaymentAH_InvoiceDate = ZDateTime.Empty;
			ReceiptPaymentAH_PostDate = ZDateTime.Empty;
			ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
			ReceiptPaymentAH_ChequeDrawer = ZString.Empty;
			ReceiptPaymentAH_DrawerBank = ZString.Empty;
			ReceiptPaymentAH_DrawerBranch = ZString.Empty;
		}

		internal bool CanSetIsInvoiceReceiptPayment = true;

		public ZPropertyInfo IsInvoiceReceiptPaymentInfo
		{
			get { return GetZPropertyInfo(nameof(IsInvoiceReceiptPayment)); }
		}

		#endregion

		#region ChequeBook

		public AccChequeBook ChequeBook
		{
			get { return Factory.Load<AccChequeBook>(fReceiptPaymentAK_AB); }
		}

		#endregion

		#region ReceiptPaymentCardSecurityCode

		[MaxLength(4)]
		public virtual ZString ReceiptPaymentCardSecurityCode
		{
			get
			{
				return fReceiptPaymentCardSecurityCode;
			}
			set
			{
				if (fReceiptPaymentCardSecurityCode != value)
				{
					SetNonPersistentPropertyValue(ReceiptPaymentCardSecurityCodeInfo, ref fReceiptPaymentCardSecurityCode, value);
					if (!IsValidationSuspended)
					{
						InvoiceValidation invoiceValidation = Validation as InvoiceValidation;
						if (invoiceValidation != null)
						{
							invoiceValidation.ValidateReceiptPaymentCardSecurityCode();
						}
					}
				}
			}
		}
		ZString fReceiptPaymentCardSecurityCode;

		public ZPropertyInfo ReceiptPaymentCardSecurityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentCardSecurityCode)); }
		}

		public virtual bool ReceiptPaymentCardSecurityCode_ReadOnly
		{
			get { return ReceiptPaymentAH_ReceiptType != ReceiptTypes.eNettCreditCard; }
		}

		#endregion

		#endregion

		#region Receipt/Payment

		public ReceiptPaymentBase ReceiptPayment
		{
			get { return fReceiptPayment; }
			set { fReceiptPayment = value; }
		}

		ReceiptPaymentBase fReceiptPayment;

		#region ReceiptPayment Defaults
		public void SetReceiptPaymentDefaults()
		{
			if (!IsReceiptPaymentFromFileImport)
			{
				ReceiptPaymentAH_ReceiptType = AH_Ledger == LedgerTypes.AccountsReceivable ? AccountingConfigurationRegistry.Instance.DefaultReceiptType.Value : AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;
				ReceiptPaymentAH_AB = DefaultBankAccount;
				SetChequeDetails();
				if (AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ReceiptPaymentAH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ARCASH, Res.GetString("BB9F4128-59A1-4938-AF7E-FC1FB9CF022A", "AR CASH RECEIPT"));
				}
				else if (AH_Ledger == LedgerTypes.AccountsPayable)
				{
					ReceiptPaymentAH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.APCASH, Res.GetString("DE169EDB-CB51-41B9-9B21-520D0A73CD52", "AP CASH PAYMENT"));
				}
				ReceiptPaymentAH_PostDate = AH_PostDate;
				ReceiptPaymentAH_InvoiceDate = AH_InvoiceDate;
				RefreshBinding();
			}
		}

		protected virtual void SetChequeDetails()
		{
		}

		protected ZString DefaultChequeDrawer
		{
			get
			{
				if (Header != null && Header.OH_IsDebtor)
				{
					return Header.MiscServ.OM_ARPreviousChequeDrawer;
				}
				return ZString.Empty;
			}
		}

		protected ZString DefaultDrawerBank
		{
			get
			{
				if (Header != null && Header.OH_IsDebtor)
				{
					return Header.MiscServ.OM_ARPreviousChequeDrawerBank;
				}
				return ZString.Empty;
			}
		}

		protected ZString DefaultDrawerBranch
		{
			get
			{
				if (Header != null && Header.OH_IsDebtor)
				{
					return Header.MiscServ.OM_ARPreviousChequeDrawerBankBranch;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZString DefaultTypeCurrency
		{
			get
			{
				ZString currency = ZString.Empty;
				if (Header != null)
				{
					if (Ledger == LedgerTypes.AccountsReceivable)
					{
						currency = Header.CompanyData.OB_RX_NKARDDefltCurrency;
					}
					else
					{
						currency = Header.CompanyData.OB_RX_NKAPDefltCurrency;
					}
				}

				if (currency == ZString.Empty)
				{
					return AH_RX_NKTransactionCurrency;
				}
				else
				{
					return currency;
				}
			}
		}

		protected virtual ZGuid DefaultBankAccount
		{
			get
			{
				ZQuery filter = new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True);
				filter.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, DefaultTypeCurrency);

				filter.AddToFilter(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);

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
				else
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

		public ZBool DefaultIsCashInvoice
		{
			get
			{
				return DefaultIsCashInvoiceCore;
			}
		}

		protected virtual ZBool DefaultIsCashInvoiceCore
		{
			get
			{
				ZBool result = ZBool.False;
				if (Header != null)
				{
					if (Ledger == LedgerTypes.AccountsReceivable)
					{
						result = Header.MiscServ.OM_ARReceiptInvoiceAfterPostingDefault;
					}
					else
					{
						result = Header.MiscServ.OM_APPayInvoiceAfterPostingDefault;
					}
				}
				return result;
			}
		}

		public bool IsReceiptPaymentFromFileImport { get; set; }

		#endregion

		#endregion

		#region Virtual/Abstract Properties & Methods

		protected abstract ReceiptPaymentBase NewReceiptPayment { get; }

		public virtual ZBool IsChequeNumberAutoAllocated
		{
			get { return ZBool.False; }
		}

		#endregion

		#region Implementation

		public AccValidationHelper AccValidationHelper
		{
			get
			{
				if (fAccValidationHelper == null)
				{
					fAccValidationHelper = new AccValidationHelper();
				}
				return fAccValidationHelper;
			}
		}
		AccValidationHelper fAccValidationHelper;

		protected override TransactionHeader CopyTransaction()
		{
			Invoice copyOfCurrent = (Invoice)Factory.New(this.GetType());

			copyOfCurrent.AH_JH = this.AH_JH;
			copyOfCurrent.AH_GB = this.AH_GB;
			if (this.Department != null && this.Department.GE_IsActive)
			{
				copyOfCurrent.AH_GE = this.AH_GE;
			}
			copyOfCurrent.AH_AG = this.AH_AG;
			copyOfCurrent.AH_OH = this.AH_OH;
			copyOfCurrent.AH_Ledger = this.AH_Ledger;
			copyOfCurrent.AH_TransactionType = this.AH_TransactionType;
			copyOfCurrent.AH_Desc = this.AH_Desc;
			copyOfCurrent.AH_InvoiceDate = ZDateTime.Now;
			copyOfCurrent.AH_PostDate = ZDateTime.Now;
			copyOfCurrent.AH_DueDate = ZDateTime.Now;
			copyOfCurrent.AH_CashBasisGSTIndicator = this.AH_CashBasisGSTIndicator;
			copyOfCurrent.AH_TransactionCategory = this.AH_TransactionCategory;
			copyOfCurrent.AH_InvoiceTerm = this.AH_InvoiceTerm;
			copyOfCurrent.AH_TransactionCategory = this.AH_TransactionCategory;
			copyOfCurrent.AH_OSExTaxAmount = this.AH_OSExTaxAmount;
			copyOfCurrent.AH_OSTaxAmount = this.AH_OSTaxAmount;
			copyOfCurrent.AH_OSTotalAmount = this.AH_OSTotalAmount;
			copyOfCurrent.ExchangeRate.Currency = this.AH_RX_NKTransactionCurrency;
			copyOfCurrent.AH_OA_InvoiceAddressOverride = this.AH_OA_InvoiceAddressOverride;
			copyOfCurrent.AH_OC_InvoiceContactOverride = this.AH_OC_InvoiceContactOverride;
			copyOfCurrent.AH_GB_TaxBranch = this.AH_GB_TaxBranch;

			foreach (InvoiceLine line in this.Lines)
			{
				InvoiceLine newLine = (InvoiceLine)copyOfCurrent.Lines.AddNew();
				newLine.GenericCharge = line.GenericCharge;

				JobRelatedLogicOnCopiedLine(line, newLine);

				newLine.AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
				newLine.SetExchangeRate();
				newLine.AL_LineType = line.AL_LineType;
				newLine.AL_GB = line.AL_GB;
				if (line.Department != null && line.Department.GE_IsActive)
				{
					newLine.AL_GE = line.AL_GE;
				}
				newLine.AL_LineAmount = line.AL_OSAmount;
				newLine.AL_LineType = line.AL_LineType;
				newLine.AL_OSExTaxAmount = line.AL_OSExTaxAmount;
				newLine.AL_OSTaxAmount = line.AL_OSTaxAmount;
				newLine.AL_AC = line.AL_AC;
				newLine.AL_SupplyType = line.AL_SupplyType;
				newLine.AL_AT = line.AL_AT;
				newLine.AL_A9_VATClass = line.AL_A9_VATClass;
				newLine.AL_Desc = line.AL_Desc;
				newLine.AL_Sequence = line.AL_Sequence;
				newLine.AL_Calc_InputGSTVATRecoverablePercentage = line.AL_Calc_InputGSTVATRecoverablePercentage;

				SubAccountHelper.CopySubAccounts(newLine, line);

				newLine.AL_GovtChargeCode = line.AL_GovtChargeCode;
				newLine.AL_GB_TaxBranch = line.AL_GB_TaxBranch;
			}
			copyOfCurrent.AH_PostedToEFT = this.AH_PostedToEFT;
			return copyOfCurrent;
		}

		protected abstract void JobRelatedLogicOnCopiedLine(InvoiceLine oldLine, InvoiceLine newLine);

		public void HandleReceiptPayment()
		{
			if (!IsDeleted && AH_Ledger != LedgerTypes.IncompleteTransactions && IsInvoiceReceiptPayment && (!IsInDatabase || IsCompletingInvoice))
			{
				CreateReceiptPayment();
				if (ReceiptPayment != null)
				{
					RaiseReceiptPaymentCreated();
				}
			}
		}

		void CreateInvoiceAndReceiptMatchLink()
		{
			if (ReceiptPayment != null)
			{
				MatchingBase matching = AH_Ledger == LedgerTypes.AccountsPayable ? new APMatchingBase(Factory) : new ARMatchingBase(Factory);
				matching.PrimaryOrganization = AH_OH;
				matching.MatchDate = AH_PostDate > ReceiptPayment.AH_PostDate ? AH_PostDate : ReceiptPayment.AH_PostDate;
				matching.MoveFromUnmatchToMatch(new BusinessObject[] { this, ReceiptPayment });

				var matchedTransactionsBalance = matching.MatchedTransactions.Balance;
				if (matchedTransactionsBalance != 0)
				{
					if (ReceiptPayment.AH_ExchangeRate == 1m)
					{
						ReceiptPayment.AH_OSExTaxAmount -= matchedTransactionsBalance;
						((IMatching)ReceiptPayment).OSPartialPaymentAmount = ReceiptPayment.AH_OSExTaxAmount;
					}
					else if (matching.IsAllPaidInTheSameCurrency(ReceiptPayment.AH_RX_NKTransactionCurrency))
					{
						var receiptPayment_localAmount = ReceiptPayment.AH_LocalExTaxAmount;
						var multiplier = AH_Ledger == LedgerTypes.AccountsPayable ? -1 : 1;

						ReceiptPayment.AH_OSExTaxAmount = matching.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly(ReceiptPayment.AH_RX_NKTransactionCurrency, ReceiptPayment.PK) * multiplier;
						ReceiptPayment.AH_LocalExTaxAmount = receiptPayment_localAmount - matchedTransactionsBalance;
						((IMatching)ReceiptPayment).OSPartialPaymentAmount = ReceiptPayment.AH_OSExTaxAmount;
					}
					else
					{
						var transactionDetailsMessage = new ZStringBuilder();
						foreach (var transaction in matching.MatchedTransactions.Cast<IMatching>())
						{
							transactionDetailsMessage.AppendLine($"{nameof(transaction.TransactionType)}: {transaction.TransactionType}, {nameof(transaction.Ledger)}: {transaction.Ledger}");
							transactionDetailsMessage.AppendLine($"{nameof(transaction.CurrencyCode)}: {transaction.CurrencyCode}, {nameof(transaction.ExchangeRateAmount)}: {transaction.ExchangeRateAmount}");
							transactionDetailsMessage.AppendLine($"{nameof(transaction.LocalPartialPaymentAmount)}: {transaction.LocalPartialPaymentAmount}, {nameof(transaction.OSPartialPaymentAmount)}: {transaction.OSPartialPaymentAmount}");
						}
						var message = string.Format(CultureInfo.InvariantCulture,
$@"Adjusting Balance is not implemented when Payment/Receipt is foreign currency and all matching transactions is not in same currency.

All Matched Transactions Details:
{transactionDetailsMessage}

Invoice Amounts:
{nameof(AH_LocalExTaxAmount)}: {AH_LocalExTaxAmount}, {nameof(AH_LocalTaxAmount)}: {AH_LocalTaxAmount}, {nameof(AH_LocalTotalAmount)}: {AH_LocalTotalAmount}
{nameof(AH_OSExTaxAmount)}: {AH_OSExTaxAmount}, {nameof(AH_OSTaxAmount)}: {AH_OSTaxAmount}, {nameof(AH_OSTotalAmount)}: {AH_OSTotalAmount}

Receipt / Payment Amounts:
{nameof(AH_LocalExTaxAmount)}: {ReceiptPayment.AH_LocalExTaxAmount}, {nameof(AH_LocalTaxAmount)}: {ReceiptPayment.AH_LocalTaxAmount}, {nameof(AH_LocalTotalAmount)}: {ReceiptPayment.AH_LocalTotalAmount}
{nameof(AH_OSExTaxAmount)}: {ReceiptPayment.AH_OSExTaxAmount}, {nameof(AH_OSTaxAmount)}: {ReceiptPayment.AH_OSTaxAmount}, {nameof(AH_OSTotalAmount)}: {ReceiptPayment.AH_OSTotalAmount}");
						ErrorReporter.ReportOnce("CashInvoiceBalanceAdjustmentNotImplementedForForeignPaymentOrReceiptAndLocalInvoice", message);
					}
				}

				matching.DoNotSaveFactoryOnMatching = true;
				matching.MatchAndClearTransactions();
			}
		}

		protected virtual ReceiptPaymentBase CreateReceiptPayment()
		{
			if (ReceiptPayment == null)
			{
				ReceiptPayment = NewReceiptPayment;
			}
			ZDecimal receiptPaymentAmount = 0m;
			ZDecimal receiptPaymentExRate = 1m;
			if (ReceiptPaymentAH_AB.IsValid)
			{
				AccBankAccount bankAccount = Factory.Load<AccBankAccount>(ReceiptPaymentAH_AB);
				if (bankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					receiptPaymentAmount = AH_LocalTotalAmount;
				}
				else if (bankAccount.AB_RX_NKAccountCurrency == AH_RX_NKTransactionCurrency)
				{
					receiptPaymentAmount = AH_OSTotalAmount;
					receiptPaymentExRate = AH_ExchangeRate;
				}
			}
			ReceiptPayment.AH_OH = AH_OH;

			ReceiptPayment.AH_GB = AH_GB;
			ReceiptPayment.AH_GE = AH_GE;
			ReceiptPayment.AH_Desc = ReceiptPaymentAH_Desc;
			ReceiptPayment.AH_AB = ReceiptPaymentAH_AB;
			ReceiptPayment.AH_ExchangeRate = receiptPaymentExRate;
			ReceiptPayment.AH_PostDate = ReceiptPaymentAH_PostDate;
			ReceiptPayment.AH_InvoiceDate = ReceiptPaymentAH_InvoiceDate;

			ReceiptPayment.AH_OSExTaxAmount = receiptPaymentAmount;
			ReceiptPayment.AH_LocalExTaxAmount = AH_LocalTotalAmount;
			if (ReceiptPayment.AH_ExchangeRate == 1m)
			{
				ReceiptPayment.AH_OSExTaxAmount = ReceiptPayment.AH_LocalExTaxAmount;
			}
			ReceiptPayment.AH_DueDate = AH_DueDate;

			ReceiptPayment.AH_ReceiptType = ReceiptPaymentAH_ReceiptType;
			ReceiptPayment.AH_ChequeDrawer = ReceiptPaymentAH_ChequeDrawer;
			ReceiptPayment.AH_ChequeOrReference = ReceiptPaymentAH_ChequeOrReference;
			ReceiptPayment.AH_DrawerBank = ReceiptPaymentAH_DrawerBank;
			ReceiptPayment.AH_DrawerBranch = ReceiptPaymentAH_DrawerBranch;
			ReceiptPayment.CardSecurityCode = ReceiptPaymentCardSecurityCode;
			return ReceiptPayment;
		}

		#endregion

		#region Lookups

		#region Bank Account
		AccBankAccountCollection fBankAccountLookup;
		public AccBankAccountCollection BankAccountLookup
		{
			get
			{
				if (fBankAccountLookup == null)
				{
					if (!IsInDatabase)
					{
						return BankAccountCollection;
					}
					else
					{
						fBankAccountLookup = new AccBankAccountCollection(Factory);
					}
				}
				return fBankAccountLookup;
			}
		}

		AccBankAccountCollection fCreditCardBankAccountLookup;
		public AccBankAccountCollection CreditCardBankAccountLookup
		{
			get
			{
				if (fCreditCardBankAccountLookup == null)
				{
					if (!IsInDatabase)
					{
						return CreditCardBankAccountCollection;
					}
					else
					{
						fCreditCardBankAccountLookup = new AccBankAccountCollection(Factory);
					}
				}
				return fCreditCardBankAccountLookup;
			}
		}

		public override ZGuid AH_GB
		{
			get => base.AH_GB;
			set
			{
				base.AH_GB = value;
				ResetBankAccountCollection();
			}
		}

		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				base.AH_RX_NKTransactionCurrency = value;
				ResetBankAccountDetails(value);
			}
		}

		protected void ResetBankAccountDetails(ZString invoiceCurrencyNK)
		{
			this.InvoiceCurrencyNK = invoiceCurrencyNK;
			ResetBankAccountCollection();
		}

		protected void ResetBankAccountCollection()
		{
			fBankAccountLookup = null;
			fBankAccountCollectionFilter = null;
			fBankAccountCollection = null;
			fCreditCardBankAccountLookup = null;
			fCreditCardBankAccountCollectionFilter = null;
			fCreditCardBankAccountCollection = null;
			ResetChequeBookCollection();
		}

		protected void ResetChequeBookCollection()
		{
			fCheckBookCollectionFilter = null;
			fCheckBookCollection = null;
		}

		AccBankAccountCollection fBankAccountCollection;
		public AccBankAccountCollection BankAccountCollection
		{
			get
			{
				if (fBankAccountCollection == null )
				{
					fBankAccountCollection = new AccBankAccountCollection(Factory, Branch, BankAccountCollectionFilter);
				}
				return fBankAccountCollection;
			}
		}

		ZQuery fBankAccountCollectionFilter;
		protected ZQuery BankAccountCollectionFilter
		{
			get
			{
				if (fBankAccountCollectionFilter == null)
				{
					ZQuery currencyFilter = new ZQuery(AccBankAccountSchema.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					if (!InvoiceCurrencyNK.IsEmpty)
					{
						currencyFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_RX_NKAccountCurrency, SQLComparisonOperator.Equal, InvoiceCurrencyNK);
					}

					fBankAccountCollectionFilter = new ZQuery(AccBankAccountSchema.AB_IsActive, true);
					fBankAccountCollectionFilter.AddToFilter(currencyFilter, JoinCondition.And);
				}
				return fBankAccountCollectionFilter;
			}
		}

		AccBankAccountCollection fCreditCardBankAccountCollection;
		public AccBankAccountCollection CreditCardBankAccountCollection
		{
			get
			{
				if (fCreditCardBankAccountCollection == null)
				{
					fCreditCardBankAccountCollection = new AccBankAccountCollection(Factory, Branch, CreditCardBankAccountCollectionFilter);
				}
				return fCreditCardBankAccountCollection;
			}
		}

		ZQuery fCreditCardBankAccountCollectionFilter;
		protected ZQuery CreditCardBankAccountCollectionFilter
		{
			get
			{
				if (fCreditCardBankAccountCollectionFilter == null)
				{
					fCreditCardBankAccountCollectionFilter = new ZQuery(BankAccountCollectionFilter);
					ZQuery creditCardFilter = new ZQuery(AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.CCD);
					creditCardFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.LNK);
					fCreditCardBankAccountCollectionFilter.AddToFilter(creditCardFilter, JoinCondition.And);
				}
				return fCreditCardBankAccountCollectionFilter;
			}
		}

		ZString InvoiceCurrencyNK;

		public AccBankAccount ReceiptPaymentBankAccount
		{
			get { return Factory.Load<AccBankAccount>(ReceiptPaymentAH_AB); }
		}

		#endregion

		#region Cheque Book

		AccChequeBookCollection fChequeBookLookup;
		public AccChequeBookCollection ChequeBookLookup
		{
			get
			{
				if (fChequeBookLookup == null)
				{
					if (!IsInDatabase)
					{
						return CheckBookCollection;
					}
					else
					{
						fChequeBookLookup = new AccChequeBookCollection(Factory);
					}
				}
				return fChequeBookLookup;
			}
		}

		AccChequeBookCollection fCheckBookCollection;
		public AccChequeBookCollection CheckBookCollection
		{
			get
			{
				if (fCheckBookCollection == null)
				{
					if (!ReceiptPaymentAH_AB.IsValid)
					{
						fCheckBookCollection = new ActiveChequeBookCollection(Factory);
					}
					else
					{
						fCheckBookCollection = new ActiveChequeBookCollection(Factory, CheckBookCollectionFilter);
					}
					fCheckBookCollection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("7ddfc1ba-fcc2-4ffe-9902-96f25c08371f", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
				}
				return fCheckBookCollection;
			}
		}

		ZQuery fCheckBookCollectionFilter;
		protected ZQuery CheckBookCollectionFilter
		{
			get
			{
				if (fCheckBookCollectionFilter == null)
				{
					fCheckBookCollectionFilter = new ZQuery(AccChequeBookSchema.AK_AB, ReceiptPaymentAH_AB);
				}
				return fCheckBookCollectionFilter;
			}
		}

		#endregion

		#endregion

		#region Events

		public event EventHandler ReceiptPaymentCreated;

		void RaiseReceiptPaymentCreated()
		{
			if (ReceiptPaymentCreated != null)
			{
				ReceiptPaymentCreated(this, null);
			}
		}

		#endregion

		#region Cash Advance

		void UpdateRelevantCashAdvanceRequests()
		{
			try
			{
				if (IsCashAdvanceFunctionalityEnabled &&
					IsEligibleForUpdatingRelevantCashAdvancesOnSaving)
				{
					UpdateRelevantCashAdvanceRequestsCore();
				}
			}
			catch (CannotGenerateCashAdvanceJournalException ex)
			{
				throw new ZCannotSaveException(ex.Message, Res.GetString("a23d6ab9-919f-4c5d-b9ce-1bdb79c1e42c", "Journal creation failed"));
			}
		}

		protected virtual void UpdateRelevantCashAdvanceRequestsCore()
		{
			if (this is IInvoiceAssociatedToCashAdvanceRequest cahUpdater)
			{
				cahUpdater.Accept(new CashAdvanceRequestUpdateVisitor());
			}
		}

		void IInvoiceAssociatedToCashAdvanceRequest.Accept(ICashAdvanceRequestProcessingByInvoiceVisitor visitor)
		{
			AcceptCore(visitor);
		}

		protected virtual void AcceptCore(ICashAdvanceRequestProcessingByInvoiceVisitor visitor)
		{
		}

		bool IInvoiceAssociatedToCashAdvanceRequest.IsOutstandingAmountPaidOnlyViaCashAdvance()
		{
			var cachedKey = string.Format("IsOutstandingAmountPaidOnlyViaCashAdvance_{0}", this.PK);
			return Factory.GetCachedValue(cachedKey, () =>
			{
				if (IsCashAdvanceFunctionalityEnabled)
				{
					var carInfoByInvoice = new CashAdvanceRequestInfoByInvoice(this);
					var carRequirementLines = carInfoByInvoice.CashAdvanceRequirements;
					var totalLocalPaidAmountViaCA = carRequirementLines?.Where(l => l.IsPaid || l.IsInvoiced).Sum(l => l.LocalPaidAmount) ?? 0m;
					if (totalLocalPaidAmountViaCA == 0m)
					{
						return false;
					}

					decimal exxAmount = 0;
					decimal overpaidJournalAmount = 0;

					var exxTransactions = carInfoByInvoice.EXXTransactions;
					if (exxTransactions.Any())
					{
						// we can't use AH_LocalTotalAmount because we don't want to invert the amount for AR, but EXX's InvertSigns always returns true, regardless AR or AP.
						// so for handle it here by multiply localTotal by -1 for AP only.
						exxAmount = AH_Ledger == LedgerTypes.AccountsPayable ? exxTransactions.Sum(x => x.AH_LocalTotal * -1m) : exxTransactions.Sum(x => x.AH_LocalTotal);
					}

					var overpayJournals = LoadOverpaymentCAIJournals();
					if (overpayJournals.Any())
					{
						overpaidJournalAmount = overpayJournals.Sum(x => x.AH_LocalTotalAmount);
					}

					return AH_LocalTotalAmount + exxAmount + overpaidJournalAmount == AH_LocalOutstandingAmount + totalLocalPaidAmountViaCA;
				}
				return false;
			});
		}

		void IInvoiceAssociatedToCashAdvanceRequest.SetCashAdvanceMatchDetails(List<CashAdvanceMatchingTransactionDetail> cahMatchingTransactionDetails)
		{
			CashAdvanceMatchDetails = cahMatchingTransactionDetails;
		}

		void MatchWithCashAdvanceRequests()
		{
			try
			{
				if (this is IInvoiceAssociatedToCashAdvanceRequest &&
					(CashAdvanceMatchDetails?.Any() ?? false) &&
					IsCashAdvanceFunctionalityEnabled &&
					!IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed &&
					IsEligibleForUpdatingRelevantCashAdvancesOnSaving)
				{
					CashAdvanceMatchingHandler.MatchWithInvoice(this, CashAdvanceMatchDetails);
				}
			}
			catch (CannotMatchCashAdvanceJournalWithInvoiceException ex)
			{
				throw new ZCannotSaveException(ex.Message, Res.GetString("4305223b-4763-47ce-a977-133f900dbd55", "Advance Payment matching failed"));
			}
		}

		protected ZQuery GetOverpaymentCAIJournalsQuery()
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, Ledger);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.CashAdvanceInvoice);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, PK);
			return filter;
		}

		protected internal abstract Journal.Journal[] LoadOverpaymentCAIJournals();

		protected List<CashAdvanceMatchingTransactionDetail> CashAdvanceMatchDetails { get; set; }

		protected bool IsEligibleForUpdatingRelevantCashAdvancesOnSaving => (!IsInDatabase &&
																				!new ZString[] { LedgerTypes.IncompleteTransactions, LedgerTypes.TransactionsPendingAllocation, LedgerTypes.UnapprovedPayableTransactions }.Contains(AH_Ledger))
																			|| IsAPTransactionConvertedFromIncompleteTransaction
																			|| IsAPTransactionConvertedFromUnapprovedTransaction
																			|| IsAPTransactionConvertedFromTransactionPendingAllocation;

		protected internal abstract bool IsCashAdvanceFunctionalityEnabled { get; }

		protected internal abstract bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed { get; }

		#endregion
	}
}
