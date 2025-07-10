using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using MetaData = CargoWise.ComponentModel.MetaData;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	[SystemDefinedValues]
	[CodeProperty(nameof(HeaderTransactionNumber))]
	[DescriptionProperty(nameof(AV_PaymentComment))]
	public abstract partial class PaymentApprovalBase : AccPaymentApproval, IMatching, IDocumentSupportable, ITransaction, IChequeNumberAutoAllocation, IeNettPayment, IAccountingNumberFountainDataSource
	{
		public new abstract class Schema : AccPaymentApproval.Schema
		{
			public const string MatchStatus = "MatchStatus";
			public const string MatchStatusReasonCode = "MatchStatusReasonCode";
			public const string AV_ExxMatchStatus = "AV_ExxMatchStatus";
			public const string AV_ExxMatchStatusReasonCode = "AV_ExxMatchStatusReasonCode";
			public const string AV_DscMatchStatus = "AV_DscMatchStatus";
			public const string AV_DscMatchStatusReasonCode = "AV_DscMatchStatusReasonCode";
			public const string AV_Calc_Sequence = "AV_Calc_Sequence";
		}

		public PaymentApprovalBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			IsLoadedFromGUI = true;
		}

		public bool IsLoadedFromGUI { get; set; }

		ZGuid DepartmentForImport { get; set; }

		public bool OSPartialPaymentAmount_ReadOnly { get { return true; } }

		public static readonly PaymentApprovalTypeDecider TypeDecider = new PaymentApprovalTypeDecider();

		public FunctionalitySuspender PostPaymentWithMatchingSetExchangeSuspender
		{
			get { return postPaymentWithMatchingSuspender ?? (postPaymentWithMatchingSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender postPaymentWithMatchingSuspender;

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				AV_PaymentApprovalReference = IsPayables ? AccountingNumberFountainWrapperFactory.Instance.APPaymentApprovalReference.Generate(this) :
				AccountingNumberFountainWrapperFactory.Instance.ARPaymentApprovalReference.Generate(this);
			}
			base.OnSaving();
		}

		#region IAccountingNumberFountainDataSource Members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		GlbDepartment IAccountingNumberFountainDataSource.Department => GlbDepartment.CurrentDepartment;

		#endregion

		#region Logs

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			HeaderTransactionNumberInfo.RefreshBinding();
			if (saveSucceeded)
			{
				ClearCachedLogsAfterSave();
			}
		}

		protected StmALog CurrentTransactionPostedLog;

		protected virtual void ClearCachedLogsAfterSave()
		{
			CurrentTransactionPostedLog = null;
		}

		#region Posted Log (PST)

		protected virtual void CreateLogForTransactionPosted()
		{
			if (IsPosted && CurrentTransactionPostedLog == null)
			{
				CurrentTransactionPostedLog = Logs.AddNew(Events.TransactionPosted, UserCreatedAndPostedTransactionLogText);
			}
		}

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Events

		public delegate void HotChequeSelectedHandler(object sender, HotChequeLink link);
		public event HotChequeSelectedHandler DisplayHotCheques;

		public delegate void PaymentFieldsUneditableHandler(object sender, string message);
		public event PaymentFieldsUneditableHandler NotifyUserPaymentUneditable;

		#endregion

		#region Public Members

		public OrgHeader PayeeOrganisation => Factory.Load<OrgHeader>(AV_OH);

		public ZString OrganisationFullName => PayeeOrganisation?.OH_FullName ?? ZString.Empty;

		public AccPaymentBatch PaymentBatch => GetPaymentBatch(this.AV_APB_PaymentBatch);

		AccPaymentBatch GetPaymentBatch(ZGuid pK) => Factory.Load<AccPaymentBatch>(pK);

		[ResourceStringData("7A51B4C3-C7D5-4E26-A485-BEA4A54F38CA", Caption = "Payment Batch Number", MediumCaption = "Batch Number", ShortCaption = "Batch #")]
		public ZString PaymentBatchNumber => PaymentBatch?.APB_BatchNumber ?? ZString.Empty;
		public ZPropertyInfo PaymentBatchNumberInfo => GetZPropertyInfo(nameof(PaymentBatchNumber));

		public void SetValues(Invoice invoice, Job job, Charge charge, ZDateTime postingTime)
		{
			if (!RelatedCharges.Contains(charge))
			{
				if (!RelatedCharges.Any())  // First Charge to set up Payment Approval
				{
					AV_PaymentComment = APPayment.DefaultDescriptionForJobRelatedPayment + " " + job.JH_JobNum;
					AV_PaymentDate = postingTime;
					AV_RX_NKPaymentCurrency = invoice.AH_RX_NKTransactionCurrency;
					AV_PaymentType = charge.JR_PaymentType;
					AV_OH = charge.JR_OH_CostAccount;
					AV_AB = charge.JR_AB;

					if (charge.ChequeBook != null)
					{
						AV_AK = charge.ChequeBook.PK;
					}

					AV_ChequeOrReference = charge.JR_ChequeNo;

					if (AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						AV_PayExRate = invoice.AH_ExchangeRate;
					}

					AV_GB = AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value ? GlbBranch.CurrentBranch.PK : job.JH_GB;
					AV_GC = Branch.GB_GC;
				}

				RelatedCharges.Add(charge);
			}
		}

		public bool IsPostWithoutMatching { get; set; }

		public bool IsPayables
		{
			get { return AV_Ledger == LedgerTypes.AccountsPayable; }
		}

		public bool IsReceivables
		{
			get { return AV_Ledger == LedgerTypes.AccountsReceivable; }
		}

		public ZBool IsPosted
		{
			get { return AV_Status == PaymentApprovalStatus.Posted; }
		}

		public ZBool IsDraft => AV_Status == PaymentApprovalStatus.Draft;

		public ZBool IsRejected => AV_Status == PaymentApprovalStatus.Rejected;

		public ZBool IsCancelled => AV_Status == PaymentApprovalStatus.Cancelled;

		public ZBool HasReversedTransaction
		{
			get
			{
				PaymentApprovalItemCollection paymentApprovalItems = new PaymentApprovalItemCollection(this);
				paymentApprovalItems.Load();

				return paymentApprovalItems.Any(x => ((PaymentApprovalItem)x).TransactionHeader.AH_IsCancelled);
			}
		}

		public ZBool IsFullyApproved
		{
			get { return AV_Status == PaymentApprovalStatus.FullyApproved; }
		}

		public ZBool IsAwaitingApproval
		{
			get { return AV_Status == PaymentApprovalStatus.AwaitingApproval; }
		}

		public bool IsCheque
		{
			get { return AV_PaymentType == ReceiptTypes.Cheque; }
		}

		public bool IsCash
		{
			get { return AV_PaymentType == ReceiptTypes.Cash; }
		}

		public bool IsDirectDebit
		{
			get { return AV_PaymentType == ReceiptTypes.DirectDebit; }
		}

		public bool IsInMatchingContext
		{
			get { return ParentMatchingCollection != null; }
		}

		public bool IsENettPayment
		{
			get { return AV_PaymentType == ReceiptTypes.eNettDirectDebit; }
		}

		public bool IsEPayment
		{
			get { return AV_PaymentType == ReceiptTypes.EPayment; }
		}

		public bool UseExchangeRateFromENettWebService
		{
			get
			{
				return IsENettPayment && BankAccount != null && AV_RX_NKPaymentCurrency != BankAccount.AB_RX_NKAccountCurrency &&
					!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty &&
					eNettHelper.IsBankAccountEnettRegistered(BankAccount) &&
					(eNettHelper.IsOrganisationeNettRegistered(Header) ||
					eNettHelper.DoesOrgHaveeNettDDRAccount(Header));
			}
		}

		ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint && !IsHotChequeImported && IsCheque;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		public bool IsProcessingPaymentDetail { get; set; }

		public ZString GetDescription() => Res.GetString("b37f008d-8f1e-4c7f-ab3e-66230294baf8", "Payment {0} {1} {2} ({3} {4})", Header.OH_Code, AV_PaymentType, AV_ChequeOrReference, AV_RX_NKPaymentCurrency, AV_Amount.ToString(RXDecimals));

		#region Currency & Exchange Rate

		[ReadOnly(true)]
		public override ZString AV_RX_NKPaymentCurrency
		{
			get { return base.AV_RX_NKPaymentCurrency; }
			set
			{
				ZString oldValue = base.AV_RX_NKPaymentCurrency;
				if (oldValue != value)
				{
					base.AV_RX_NKPaymentCurrency = value;
					ResetAccountDetails();
					UpdateEPaymentReason();
					if (value != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						SetExchangeRateFromEnett();
					}
				}
			}
		}

		protected bool AV_RX_NKPaymentCurrency_ReadOnly => BankAccount != null && BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency || IsCashAccountType;

		public ZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, GetRateType(), AV_PayExRateInfo, (ZPropertyInfoString)AV_RX_NKPaymentCurrencyInfo, null);
					fExchangeRate.IsCurrencyRequired = true;
					fExchangeRate.IsRateRequired = true;
				}

				return fExchangeRate;
			}
		}

		ZExchangeRate fExchangeRate;

		public int RXDecimals => PaymentCurrency != null ? PaymentCurrency.Decimals : LocalRXDecimals;

		public int LocalRXDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		[ReadOnly(true)]
		[List("Lookups.PaymentCurrencies")]
		public ZString AV_Calc_LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo AV_Calc_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_LocalCurrency)); }
		}

		#endregion

		#region ReadOnly Properties

		[ReadOnly(true)]
		[List("Lookups.PaymentCurrencies")]
		public ZString OSCurrencyForDisplay
		{
			get { return AV_RX_NKPaymentCurrency; }
		}

		public ZPropertyInfo OSCurrencyForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(OSCurrencyForDisplay)); }
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public virtual ZDecimal AV_Calc_LocalAmount
		{
			get => CalculateLocalAmount(AV_PayExRate, AV_Amount);
			set
			{
				using (StopRecalculatingAV_AmountWhenExRateChanged.GetSuspender())
				{
					AV_PayExRate = Env.CurrentCompany.ExchangeRate.GetRate(value, AV_Amount, AccPaymentApprovalSchema.AV_PayExRate.Scale);
				}

				ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
				AV_Calc_LocalAmountInfo.RefreshBinding();
			}
		}

		public virtual ZDecimal GetAV_Calc_LocalAmountOriginalValue()
			=> CalculateLocalAmount((ZDecimal)AV_PayExRateInfo.OriginalValue, (ZDecimal)AV_AmountInfo.OriginalValue);

		protected ZDecimal CalculateLocalAmount(ZDecimal exchangeRate, ZDecimal paymentAmount)
			=> AccountingUtils.Round(Env.CurrentCompany.ExchangeRate.ForeignToLocal(paymentAmount, exchangeRate), AV_Calc_LocalCurrency);

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal AV_Calc_LocalCachedAmount => localAmountRecalculatingSuspender?.CachedLocalAmount ?? AV_Calc_LocalAmount;

		public ZPropertyInfo AV_Calc_LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_LocalAmount)); }
		}

		protected bool AV_Calc_LocalAmount_ReadOnly
		{
			get { return AV_RX_NKPaymentCurrency.IsEmpty || AV_RX_NKPaymentCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency || IsPosted; }
		}

		[ReadOnly(true)]
		public ZString HeaderTransactionNumber
		{
			get { return TransactionHeader != null ? TransactionHeader.AH_TransactionNum : ZString.Empty; }
		}

		public ZPropertyInfo HeaderTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(HeaderTransactionNumber)); }
		}

		public ZString AV_Calc_ChequeReferenceLabel
		{
			get { return IsCheque ? Res.GetString("PaymentApproval|ChequeReferenceLabel|ChequeNumber", "Check Number") : Res.GetString("PaymentApproval|ChequeReferenceLabel|ReferenceNumber", "Reference Number"); }
		}

		public ZPropertyInfo AV_Calc_ChequeReferenceLabelInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_ChequeReferenceLabel)); }
		}

		public ZString AV_Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && AV_ChequeOrReference.IsEmpty ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo AV_Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		public ZString AV_Calc_ChequeIsAutoPrintedLabel
		{
			get { return IsChequeNumberAutoAllocated && AV_ChequeOrReference.IsEmpty ? AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel : ""; }
		}

		public ZPropertyInfo AV_Calc_ChequeIsAutoPrintedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_ChequeIsAutoPrintedLabel)); }
		}

		public ZString DefaultDescription
		{
			get { return DefaultDescriptionCore; }
		}

		protected virtual ZString DefaultDescriptionCore
		{
			get
			{ return (AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(DefaultLedger + (TransactionType.ToString() == AccountingConstants.VoucherItemRegistryCode.UNPaymentTransactionType ? TransactionTypes.Payment : TransactionType.ToString()), DefaultLedger + " PAYMENT")); }
		}

		public void AlterPaymentApprovalFormEvent(bool readOnly)
		{
			alterPaymentApprovalFormEvent = readOnly;
		}

		bool alterPaymentApprovalFormEvent;

		#endregion

		#region ExRateRecalculatingFromLocalSuspender

		public IDisposable SuspendLocalAmountRecalculating()
		{
			return localAmountRecalculatingSuspender ?? new LocalAmountRecalculatingSuspender(this);
		}

		LocalAmountRecalculatingSuspender localAmountRecalculatingSuspender;

		public class LocalAmountRecalculatingSuspender : Disposable
		{
			public LocalAmountRecalculatingSuspender(PaymentApprovalBase parent)
			{
				this.parent = parent;
				CachedLocalAmount = parent.AV_Calc_LocalAmount;
				if (parent.localAmountRecalculatingSuspendedCount == 0)
				{
					parent.localAmountRecalculatingSuspender = this;
				}
				parent.localAmountRecalculatingSuspendedCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					parent.localAmountRecalculatingSuspendedCount--;
					if (parent.localAmountRecalculatingSuspendedCount == 0)
					{
						parent.localAmountRecalculatingSuspender = null;
					}
					if (parent.localAmountRecalculatingSuspendedCount < 0)
					{
						ErrorReporter.ReportOnce("PaymentApprovalBase.ExRateRecalculatingFromLocalSuspendedCount.Dispose", "Suspend Count is below zero.");
					}
				}
			}

			public readonly ZDecimal CachedLocalAmount;
			readonly PaymentApprovalBase parent;
		}

		int localAmountRecalculatingSuspendedCount;

		#endregion

		#region RecalculatingAV_AmountWhenExRateChangedSuspender

		public FunctionalitySuspender StopRecalculatingAV_AmountWhenExRateChanged => stopRecalculatingAV_AmountWhenExRateChanged ?? (stopRecalculatingAV_AmountWhenExRateChanged = new FunctionalitySuspender());
		FunctionalitySuspender stopRecalculatingAV_AmountWhenExRateChanged;

		#endregion

		#region CreditCardSecurityCode

		[MaxLength(4)]
		public ZString CreditCardSecurityCode
		{
			get { return fCreditCardSecurityCode; }
			set
			{
				if (fCreditCardSecurityCode != value)
				{
					SetNonPersistentPropertyValue(CreditCardSecurityCodeInfo, ref fCreditCardSecurityCode, value);
					if (!IsValidationSuspended)
					{
						PaymentApprovalValidation validation = Validation as PaymentApprovalValidation;
						if (validation != null)
						{
							validation.ValidateCreditCardSecurityCode();
						}
					}
				}
			}
		}
		ZString fCreditCardSecurityCode;

		public ZPropertyInfo CreditCardSecurityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CreditCardSecurityCode)); }
		}

		protected bool CreditCardSecurityCode_ReadOnly
		{
			get { return AV_PaymentType != ReceiptTypes.eNettCreditCard; }
		}

		#endregion

		#region PaymentApprovalBase Overrides

		#region AV_EPaymentReasonCode

		[List("Lookups.PaymentReasons")]
		public override ZString AV_EPaymentReasonCode
		{
			get => base.AV_EPaymentReasonCode;
			set
			{
				base.AV_EPaymentReasonCode = value;
			}
		}

		public bool AV_EPaymentReasonCode_ReadOnly => !IsEPayment;

		void UpdateEPaymentReason()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				if (!IsEPayment)
				{
					AV_EPaymentReasonCode = ZString.Empty;
				}
				else if (!IsInDatabase || (ZString)AV_PaymentTypeInfo.OriginalValue != ReceiptTypes.EPayment)
				{
					AV_EPaymentReasonCode = PayeeOrganisation?.CompanyData?.GetDefaultEPaymentReason(EPaymentProvider, AV_RX_NKPaymentCurrency) ?? ZString.Empty;
				}
				else
				{
					AV_EPaymentReasonCode = (ZString)AV_EPaymentReasonCodeInfo.OriginalValue;
				}
			}
		}

		#endregion

		public override ZDecimal AV_PayExRate
		{
			get => base.AV_PayExRate;
			set
			{
				base.AV_PayExRate = value;
				LocalPartialPaymentAmountInfo.RefreshBinding();
			}
		}

		[List("Lookups.Headers")]
		public override ZGuid AV_OH
		{
			get { return base.AV_OH; }
			set
			{
				var previousAV_OH = AV_OH;

				if (HotChequeIsAlreadyImported)
				{
					if (value != AV_OH)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_OHError);
					}

					AV_PaymentTypeInfo.RefreshBinding();
				}
				else
				{
					base.AV_OH = value;
					DefaultBankAccountFromOrg();

					if (IsPayables)
					{
						AccHotChequeCollection hotCheques = GetActiveHotCheques();

						if (hotCheques.Count > 0)
						{
							FireDisplayHotCheques(hotCheques);
						}
					}
				}

				if (previousAV_OH != AV_OH)
				{
					UpdatePaymentAddress();
					UpdateEPaymentReason();
				}
			}
		}

		protected virtual bool AV_OH_ReadOnly
		{
			get { return alterPaymentApprovalFormEvent; }
		}

		void DefaultBankAccountFromOrg()
		{
			if (AV_Ledger == LedgerTypes.AccountsPayable && Header != null && Header.CompanyData != null)
			{
				AccBankAccount bank = Factory.Load<AccBankAccount>(Header.CompanyData.OB_AB_APDefaultBankAccount);
				if (bank != null && bank.AB_GC == GlbCompany.CurrentCompany.PK)
				{
					AV_AB = bank.PK;
				}
			}
		}

		[List("Lookups.PaymentMethods")]
		public override ZString AV_PaymentType
		{
			get { return base.AV_PaymentType; }
			set
			{
				ZString oldValue = base.AV_PaymentType;
				if (oldValue != value)
				{
					if (HotChequeIsAlreadyImported)
					{
						if (value != ReceiptTypes.Cheque)
						{
							FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_PaymentTypeError);
						}

						AV_PaymentTypeInfo.RefreshBinding();
					}
					else
					{
						base.AV_PaymentType = value;

						if (!IsCheque)
						{
							AV_AK = ZGuid.Empty;
						}
						UpdateEPaymentReason();
						ExchangeRate.AdditionalRateReadOnlyCondition = IsENettPayment;

						var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(value);
						AV_ChequeOrReference = !defaultReferenceNumber.IsEmpty ? defaultReferenceNumber : IsCash ? (ZString)ReceiptTypes.Cash : ZString.Empty;
						AV_Calc_ChequeReferenceLabelInfo.RefreshBinding();

						Validation.ValidateAV_AB();
					}

					CheckNumberIsAutoAllocated();
					CreditCardSecurityCode = ZString.Empty;
					Validation.ValidateAV_PaymentType();

					if (AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						SetExchangeRateFromEnett();
					}
				}
			}
		}

		[List("Lookups.BankAccounts")]
		public override ZGuid AV_AB
		{
			get { return base.AV_AB; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (ImportedHotCheque.ChequeBook != null &&
						ImportedHotCheque.ChequeBook.BankAccount.PK != value)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_ABError);
					}

					AV_ABInfo.RefreshBinding();
				}
				else
				{
					var previousAV_AB = AV_AB;
					base.AV_AB = value;

					Lookups.ResetChequeBookCollection();
					if (previousAV_AB != AV_AB)
					{
						UpdateEPaymentReason();
					}

					if (ChequeBook == null || ChequeBook.AK_AB != value)
					{
						AV_AK = ZGuid.Empty;
					}

					if (BankAccount != null)
					{
						ExchangeRate.Currency = BankAccount.AB_RX_NKAccountCurrency;

						if (ExchangeRate.Currency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							ExchangeRate.RefetchExchangeRate();
						}

						if (IsCheque)
						{
							AV_ChequeOrReference = ZString.Empty;
						}

						if (IsCashAccountType)
						{
							AV_PaymentType = ReceiptTypes.Cash;
						}
					}
				}
			}
		}

		[List("Lookups.ChequeBooks")]
		public override ZGuid AV_AK
		{
			get { return base.AV_AK; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (ImportedHotCheque.AQ_AK != value)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_AKError);
					}

					AV_AKInfo.RefreshBinding();
				}
				else
				{
					base.AV_AK = value;

					if (IsCheque && PostsOnSave && !IsChequeNumberAutoAllocated)
					{
						PopulateChequeNumberFromChequeBook();
					}
				}
				CheckNumberIsAutoAllocated();
			}
		}

		protected virtual bool AV_AK_ReadOnly
		{
			get { return !IsCheque; }
		}

		[MaxLength(Schema.AV_ChequeOrReferenceMaxLength)]
		public override ZString AV_ChequeOrReference
		{
			get { return base.AV_ChequeOrReference; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (ImportedHotCheque.AQ_ChequeNumber != value)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_ChequeOrReferenceError);
					}
					AV_ChequeOrReferenceInfo.RefreshBinding();
				}
				else
				{
					if (IsCheque)
					{
						value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, value);
					}

					base.AV_ChequeOrReference = value;

					if (!AV_ChequeOrReference.IsEmpty && fImportedHotCheque == null)
					{
						AccHotChequeCollection matchedHotCheques = GetMatchedActiveHotCheques();
						if (matchedHotCheques.Count != 0)
						{
							FireDisplayHotCheques(matchedHotCheques);
						}
					}
				}
			}
		}

		protected bool AV_ChequeOrReference_ReadOnly
		{
			get { return IsChequeNumberAutoAllocated; }
		}

		[DecimalPlaces(nameof(RXDecimals))]
		public override ZDecimal AV_Amount
		{
			get { return base.AV_Amount; }
			set
			{
				ZDecimal oldValue = base.AV_Amount;
				if (oldValue != value)
				{
					if (HotChequeIsAlreadyImported)
					{
						if (ImportedHotCheque.AQ_ActualOrMaxIndicator == ZArchitecture.Core.ActualOrMaxIndicator.Actual && ImportedHotCheque.AQ_Amount != value)
						{
							FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AV_ActualAmountError);
						}
						else if (ImportedHotCheque.AQ_ActualOrMaxIndicator == ZArchitecture.Core.ActualOrMaxIndicator.Max)
						{
							if (ImportedHotCheque.AQ_Amount < value)
							{
								FireNotifyUserPaymentUneditable(HotChequeErrorMessages.GetMaximumError(ImportedHotCheque.AQ_Amount));
							}
							else
							{
								base.AV_Amount = value;
								OSPartialPaymentAmount = AV_Amount;
							}
						}

						AV_AmountInfo.RefreshBinding();
					}
					else if (value != AV_Amount)
					{
						base.AV_Amount = value;
						OSPartialPaymentAmount = AV_Amount;
					}

					if (AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						SetExchangeRateFromEnett();
					}
				}
			}
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal AV_Discount
		{
			get { return base.AV_Discount; }
			set { base.AV_Discount = value; }
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal AV_ExchangeDifference
		{
			get { return base.AV_ExchangeDifference; }
			set { base.AV_ExchangeDifference = value; }
		}

		[ReadOnly(true)]
		public override ZString AV_Ledger
		{
			get { return base.AV_Ledger; }
			set { base.AV_Ledger = value; }
		}

		[ReadOnly(true)]
		[List("Lookups.AV_StatusList")]
		public override ZString AV_Status
		{
			get { return base.AV_Status; }
			set
			{
				Previous_AV_Status = base.AV_Status;
				base.AV_Status = value;

				if (value == PaymentApprovalStatus.Posted || value == PaymentApprovalStatus.Cancelled)
				{
					PaymentBatch?.SetNeedUpdateBatchStatusWhenSaving();
				}
			}
		}

		ZString Previous_AV_Status { get; set; }

		public override ZGuid AV_APB_PaymentBatch
		{
			get { return base.AV_APB_PaymentBatch; }
			set
			{
				var oldValue = base.AV_APB_PaymentBatch;
				if (value != oldValue)
				{
					GetPaymentBatch(oldValue)?.SetNeedUpdateBatchStatusWhenSaving();
					GetPaymentBatch(value)?.SetNeedUpdateBatchStatusWhenSaving();
				}

				base.AV_APB_PaymentBatch = value;
			}
		}

		public override AccTransactionHeader TransactionHeader
		{
			get { return Factory.Load<TransactionHeader>(AV_AH); } //to use TransactionHeaderTypeDecider
		}

		#endregion

		#region MatchingDate

		//Date of current matching proccess
		public ZDateTime CurrentMatchingDate
		{
			get { return fCurrentMatchingDate; }
			set { fCurrentMatchingDate = value; }
		}
		ZDateTime fCurrentMatchingDate;

		#endregion

		public ZByte AH_NumberOfSupportingDocuments
		{
			get
			{ return fNumberOfSupportingDocuments; }
			set { fNumberOfSupportingDocuments = value; }
		}
		ZByte fNumberOfSupportingDocuments;

		public ZBool AH_NumberOfSupportingDocumentsVisible_ReadOnly
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China; }
		}

		#region Payment Type Security

		Dictionary<ZString, SecurityCheckpoint> fPaymentTypeSecurityMap;
		protected abstract Dictionary<ZString, SecurityCheckpoint> InitPaymentTypeSecurityMap();

		public SecurityCheckpoint PaymentTypeSecurityNeededToPost
		{
			get
			{
				if (fPaymentTypeSecurityMap == null)
				{
					fPaymentTypeSecurityMap = InitPaymentTypeSecurityMap();
				}

				SecurityCheckpoint securityToCheck;
				fPaymentTypeSecurityMap.TryGetValue(AV_PaymentType, out securityToCheck);
				return securityToCheck;
			}
		}

		public bool UserHasPaymentTypeSecurityToPost()
		{
			if (PaymentTypeSecurityNeededToPost != null && !PaymentTypeSecurityNeededToPost.IsAllowed)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Payment Type Validation

		internal bool IsCashAccountType => BankAccount != null && BankAccount.IsCashAccount;

		#endregion

		#region Process E-Payment Security

		public abstract SecurityCheckpoint ProcessEPaymentSecurityCheckPoint { get; }

		#endregion

		#endregion

		#region Hot Cheques

		void FireDisplayHotCheques(AccHotChequeCollection hotCheques)
		{
			if (DisplayHotCheques != null)
			{
				HotChequeLink link = new HotChequeLink(hotCheques);
				DisplayHotCheques(this, link);
			}
		}

		void FireNotifyUserPaymentUneditable(string message)
		{
			if (NotifyUserPaymentUneditable != null && !fIsImportingHotCheque)
			{
				NotifyUserPaymentUneditable(this, message);
			}
		}

		public AccHotCheque ImportedHotCheque
		{
			get
			{
				if (!AlreadyCheckedForExistingdHotCheque && IsInDatabase && fImportedHotCheque == null)
				{
					AlreadyCheckedForExistingdHotCheque = true;
					ImportExistingHotCheque();
				}

				return fImportedHotCheque;
			}
		}

		AccHotCheque fImportedHotCheque;

		public bool HotChequeIsAlreadyImported
		{
			get { return IsHotChequeImported && !fIsImportingHotCheque; }
		}

		public bool IsHotChequeImported
		{
			get { return ImportedHotCheque != null; }
		}

		public void ImportExistingHotCheque()
		{
			if (IsCheque && !AV_ChequeOrReference.IsEmpty)
			{
				ZQuery findExistingHotChequeQuery = new ZQuery(AccHotChequeSchema.AQ_ChequeNumber, AV_ChequeOrReference);
				findExistingHotChequeQuery.AddToFilter(AccHotChequeSchema.AQ_Cancelled, ZBool.False);
				findExistingHotChequeQuery.AddToFilter(AccHotChequeSchema.AQ_OH, AV_OH);
				findExistingHotChequeQuery.AddToFilter(AccHotChequeSchema.AQ_AK, AV_AK);

				AccHotCheque existingHotCheque = Factory.LoadTop1<AccHotCheque>(findExistingHotChequeQuery);

				if (existingHotCheque != null)
				{
					fImportedHotCheque = existingHotCheque;
				}
			}
		}

		AccHotChequeCollection GetMatchedActiveHotCheques()
		{
			ZQuery query = new ZQuery(AccHotChequeSchema.AQ_ChequeNumber, AV_ChequeOrReference);
			query.AddToFilter(AccHotChequeSchema.AQ_OH, AV_OH);
			query.AddToFilter(AccHotChequeSchema.AQ_AK, AV_AK);
			query.AddToFilter(AccHotChequeSchema.AQ_Cancelled, ZBool.False);
			query.AddToFilter(AccHotChequeSchema.AQ_AH, SQLComparisonOperator.Equal, null);

			AccHotChequeCollection allHotCheques = new AccHotChequeCollection(Factory, query);
			allHotCheques.Load();

			AccHotChequeCollection hotChequesToReturn = new AccHotChequeCollection(Factory);

			foreach (AccHotCheque hotCheque in allHotCheques)
			{
				if (!HotChequeIsAlreadyUsedOnAnotherPaymentApproval(hotCheque))
				{
					hotChequesToReturn.Add(hotCheque);
				}
			}

			return hotChequesToReturn;
		}

		AccHotChequeCollection GetActiveHotCheques()
		{
			ZQuery query = new ZQuery(AccHotChequeSchema.AQ_OH, AV_OH);
			query.AddToFilter(AccHotChequeSchema.AQ_Cancelled, ZBool.False);
			query.AddToFilter(AccHotChequeSchema.AQ_AH, SQLComparisonOperator.Equal, null);

			AccHotChequeCollection allHotCheques = new AccHotChequeCollection(Factory, query);
			allHotCheques.Load();

			AccHotChequeCollection hotChequesToReturn = new AccHotChequeCollection(Factory);

			foreach (AccHotCheque hotCheque in allHotCheques)
			{
				if (!HotChequeIsAlreadyUsedOnAnotherPaymentApproval(hotCheque))
				{
					hotChequesToReturn.Add(hotCheque);
				}
			}

			return hotChequesToReturn;
		}

		public void ImportSelectedHotCheque(AccHotCheque hotCheque)
		{
			if (hotCheque != null)
			{
				SetHotChequeInactiveWhenPosting(hotCheque);
				BeginImportingHotCheque();
				PopulateFieldsUsingHotCheque(hotCheque);
				FinishImportingHotCheque();
			}
		}

		void SetHotChequeInactiveWhenPosting(AccHotCheque hotCheque)
		{
			fImportedHotCheque = hotCheque;
		}

		void BeginImportingHotCheque()
		{
			fIsImportingHotCheque = true;
		}

		void PopulateFieldsUsingHotCheque(AccHotCheque hotCheque)
		{
			AV_PaymentType = ReceiptTypes.Cheque;
			AV_AB = (hotCheque.ChequeBook != null && hotCheque.ChequeBook.BankAccount != null) ? hotCheque.ChequeBook.BankAccount.PK : ZGuid.Empty;
			AV_AK = hotCheque.AQ_AK;
			ExchangeRate.Currency = hotCheque.AQ_Calc_RX_NK;
			AV_ChequeOrReference = hotCheque.AQ_ChequeNumber;
			AV_Amount = hotCheque.AQ_Amount;
		}

		void FinishImportingHotCheque()
		{
			fIsImportingHotCheque = false;
		}

		bool HotChequeIsAlreadyUsedOnAnotherPaymentApproval(AccHotCheque hotCheque)
		{
			ZQuery query = new ZQuery(AccPaymentApprovalSchema.AV_PaymentType, ReceiptTypes.Cheque);
			query.AddToFilter(AccPaymentApprovalSchema.AV_AK, hotCheque.AQ_AK);
			query.AddToFilter(AccPaymentApprovalSchema.AV_ChequeOrReference, hotCheque.AQ_ChequeNumber);
			query.AddToFilter(AccPaymentApprovalSchema.PK, SQLComparisonOperator.NotEqual, PK);

			PaymentApprovalBase otherPaymentApprovalUsingThisHotCheque = Factory.LoadTop1<PaymentApprovalBase>(query);
			return otherPaymentApprovalUsingThisHotCheque != null;
		}

		bool AlreadyCheckedForExistingdHotCheque;

		bool fIsImportingHotCheque;

		public static class HotChequeErrorMessages
		{
			public static string AV_OHError
			{
				get { return Res.GetString("c1c72fdb-209d-40be-9df6-a79a92365383", "Entered creditor must be the same as the creditor on the imported hot check"); }
			}

			public static string AV_ABError
			{
				get { return Res.GetString("0771e44f-9e68-4d86-9c82-61dc80a8d5f5", "Entered bank must be the same as the bank on the imported hot check"); }
			}

			public static string AV_ChequeOrReferenceError
			{
				get { return Res.GetString("1d5ec3bc-a354-4475-af12-d587cfe14deb", "Entered cheque number must be the same as the reference number on the imported hot check"); }
			}

			public static string AV_AKError
			{
				get { return Res.GetString("3ec70672-e88f-48d7-a2e8-59777923d07d", "Entered cheque book must be the same as the cheque book on the imported hot check"); }
			}

			public static string AV_ActualAmountError
			{
				get { return Res.GetString("3483e46b-b0d1-4411-b4d3-fe189777779a", "Entered amount must be the same as the amount used on the imported hot check"); }
			}

			public static string AV_PaymentTypeError
			{
				get { return Res.GetString("23faed26-9a01-4963-9dad-d5732d5b66c0", "This payment must be a cheque payment because a hot check was imported"); }
			}

			public static string GetMaximumError(decimal maximumAmount)
			{
				return Res.GetString("63c9b6ba-bcbb-47a3-90f9-e6f3b6bc680f", "Entered amount must be less than or equal to the amount used on the imported hot check ({0})", Utilities.Round(maximumAmount, 2));
			}
		}

		#endregion

		#region Cheque Number

		public ZBool CanIncrementChequeBookNumber
		{
			get
			{
				return ChequeBook != null && GetChequeNumberStatus() == ChequeNumberStatus.GreaterThanOrEqualToCurrentNum &&
					ZDecimal.CanParseAsInteger(AV_ChequeOrReference);
			}
		}

		public ChequeNumberStatus GetChequeNumberStatus()
		{
			ChequeNumberStatus returnStatus = ChequeNumberStatus.NoChequeNumber;

			if (AV_AK.IsValid)
			{
				if (ChequeBook != null && ZDecimal.CanParseAsInteger(AV_ChequeOrReference))
				{
					var loadedChequeBook = new ReadOnlyBusinessObjectFactory().Load<AccChequeBook>(ChequeBook.PK);
					if (ZDecimal.Parse(AV_ChequeOrReference) >= loadedChequeBook.AK_CurrentNo)
					{
						returnStatus = ChequeNumberStatus.GreaterThanOrEqualToCurrentNum;
					}
					else if (loadedChequeBook.BankAccount != null && loadedChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(AV_ChequeOrReference))   // allow the save but don't increment current number of cheque book
					{
						returnStatus = ChequeNumberStatus.CancelledPayment;
					}
					else    // do gui validation saying that the number is less than current number
					{
						returnStatus = ChequeNumberStatus.LessThanCurrentNum;
					}
				}
			}

			return returnStatus;
		}

		public void PopulateChequeNumberFromChequeBook()
		{
			PopulateChequeNumberFromChequeBookCore();
		}

		protected virtual void PopulateChequeNumberFromChequeBookCore()
		{
			if (IsCheque && AV_ChequeOrReference.IsEmpty && ChequeBook != null)
			{
				AV_ChequeOrReference = ChequeBook.AK_CurrentNo.ToString();
			}
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return PaymentApprovalDocumentSupporter; }
		}

		public PaymentApprovalDocumentSupporter PaymentApprovalDocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new PaymentApprovalDocumentSupporter(this);
				}

				return fDocumentSupporter;
			}
		}

		PaymentApprovalDocumentSupporter fDocumentSupporter;

		#endregion

		#region ITransaction Members

		ZString ITransaction.TransactionNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		ZPropertyInfo ITransaction.TransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumber)); }
		}

		public ZDateTime TransactionDate
		{
			get { return AV_PaymentDate; }
			set { AV_PaymentDate = value; }
		}

		public ZPropertyInfo TransactionDateInfo
		{
			get { return AV_PaymentDateInfo; }
		}

		[List("Lookups.PaymentCurrencies")]
		public ZString CurrencyCode
		{
			get { return AV_RX_NKPaymentCurrency; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return AV_RX_NKPaymentCurrencyInfo; }
		}

		public ZInt CurrencyDecimals
		{
			get { return RXDecimals; }
		}

		public ZPropertyInfo CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyDecimals)); }
		}

		public ZInt LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		public ZPropertyInfo LoginCompanyCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(LoginCompanyCurrencyDecimals)); }
		}

		[DecimalPlaces(nameof(RXDecimals))]
		public ZDecimal OverseasTotalAmount
		{
			get { return AV_Amount; }
		}

		public ZPropertyInfo OverseasTotalAmountInfo
		{
			get { return AV_AmountInfo; }
		}

		public ZGuid Organization
		{
			get { return AV_OH; }
			set { AV_OH = value; }
		}

		public ZPropertyInfo OrganizationInfo
		{
			get { return AV_OHInfo; }
		}

		public ZString OriginalTransactionType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo { get { return GetZPropertyInfo(nameof(OriginalTransactionType)); } }

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		[BusinessObjectTestExclude]
		public ZString SupportingDocumentNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo { get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); } }

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public OrgHeaderCollection Headers
		{
			get { return Lookups.Headers; }
		}

		public bool UserAllowedToBackPost
		{
			get
			{
				bool result = false;
				switch (AV_Ledger)
				{
					case ZArchitecture.Core.LedgerTypes.AccountsReceivable:
						result = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
						break;
					case ZArchitecture.Core.LedgerTypes.AccountsPayable:
						result = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
						break;
				}
				return result;
			}
		}

		public ZDateTime ITransactionPostDate
		{
			get { return PostDate; }
			set
			{
				PostDate = value;
			}
		}

		public ZPropertyInfo ITransactionPostDateInfo
		{
			get { return PostDateInfo; }
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set { SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value); }
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(UnmatchDate)); }
		}

		#region ReversalStatusCode

		[BusinessObjectTestExclude]
		public ZString ReversalStatusCode
		{
			get => ZString.Empty;
			set { }
		}

		public ZPropertyInfo ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion ReversalStatusCode

		#endregion

		#region IAutoAllocation Members

		AccChequeBook IChequeNumberAutoAllocation.ChequeBook
		{
			get { return ChequeBook; }
		}

		ZBool IChequeNumberAutoAllocation.IsAutoAllocationEnabled
		{
			get
			{
				return IsChequeNumberAutoAllocated;
			}
		}

		void IChequeNumberAutoAllocation.AssignChequeNumber(string autoGeneratedChequeNumber)
		{
			AV_ChequeOrReference = autoGeneratedChequeNumber;
			if (NewPayment != null)
			{
				NewPayment.AH_ChequeOrReference = autoGeneratedChequeNumber;
			}
		}

		ZBool IChequeNumberAutoAllocation.IsAllocationPerformed
		{
			get
			{
				return !AV_ChequeOrReference.IsEmpty;
			}
		}

		Guid IChequeNumberAutoAllocation.Printing_ObjectPK
		{
			get
			{
				return (NewPayment != null) ? NewPayment.PK.ToGuid() : Guid.Empty;
			}
		}

		ZGuid IChequeNumberAutoAllocation.Printing_PrinterPK
		{
			get
			{
				return (ChequeBook != null) ? ChequeBook.AK_SQ : ZGuid.Empty;
			}
		}

		ZBool IChequeNumberAutoAllocation.ChequeIsAutoPrinted
		{
			get
			{
				return (NewPayment != null) ? ((IChequeNumberAutoAllocation)NewPayment).ChequeIsAutoPrinted : ZBool.False;
			}
			set
			{
				if (NewPayment != null)
				{
					((IChequeNumberAutoAllocation)NewPayment).ChequeIsAutoPrinted = value;
				}
			}
		}

		void IChequeNumberAutoAllocation.AllocationOrPrintingFailed()
		{
			ChequeBook.Reload();
			AV_ChequeOrReference = ZString.Empty;
			if (NewPayment != null)
			{
				NewPayment.AH_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#region Authorisation Related Virtual Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		public const string UserCreatedAndPostedTransactionLogText = "Created and Posted Payment with 'New Payment' Security Rights";

		public virtual bool NoAuthorisationRequired
		{
			get { return true; }
		}

		protected virtual MultilingualString AV_Calc_DescriptionOfAuthorisationRequiredCore
		{
			get { return ResString.GetMultilingualString("935628d1-4c71-4134-ac8a-725881ae4a3d", "No further Authorization is required."); }
		}

		protected virtual void UpdateStatus()
		{
		}

		protected virtual void FireRequiredAuthorisationChanged()
		{
		}

		protected virtual void FireFirstApprovalStatusChanged()
		{
		}

		protected virtual void FireSecondApprovalStatusChanged()
		{
		}

		protected virtual void FireThirdApprovalStatusChanged()
		{
		}

		protected virtual void AuthorisationChanged()
		{
		}

		#endregion

		#region IMatching Members

		public MatchingBase MatchingBaseObject
		{
			get { return PaymentMatchingBaseObject; }
		}

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return currencyNK == AV_RX_NKPaymentCurrency;
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			return currencyNK == AV_RX_NKPaymentCurrency ? ((IMatching)this).OSPartialPaymentAmount : 0;
		}

		ZString IMatching.TransactionNumber
		{
			get { return ZString.Empty; }
		}

		ZString IMatching.JobLocalReference
		{
			get { return ZString.Empty; }
		}

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }

		public bool HasPaymentMatchingBaseObjectBeenCreated
		{
			get { return fPaymentMatchingBaseObject != null; }
		}

		public PaymentApprovalMatchingBase PaymentMatchingBaseObject
		{
			get
			{
				if (fPaymentMatchingBaseObject == null)
				{
					fPaymentMatchingBaseObject = GetNewPaymentMatchingBaseObject();
					fPaymentMatchingBaseObject.TransactionsSelectedChanged += new EventHandler(fPaymentMatchingBaseObject_TransactionsSelectedChanged);
				}

				if (fPaymentMatchingBaseObject.PrimaryOrganization != AV_OH)
				{
					fPaymentMatchingBaseObject.PrimaryOrganization = AV_OH;
					fPaymentMatchingBaseObject.PrimaryOrganisationForGUINotification = AV_OH;
				}

				return fPaymentMatchingBaseObject;
			}
		}

		PaymentApprovalMatchingBase fPaymentMatchingBaseObject;

		public void ResetPaymentMatchingBaseObject()
		{
			fPaymentMatchingBaseObject = null;
		}

		void fPaymentMatchingBaseObject_TransactionsSelectedChanged(object sender, EventArgs e)
		{
			TransactionsSelectedChanged();
		}

		protected virtual void TransactionsSelectedChanged()
		{
			Validation.ValidateAV_Amount();
		}

		protected abstract PaymentApprovalMatchingBase GetNewPaymentMatchingBaseObject();

		public ZDateTime PostDate
		{
			get { return AV_PostDate; }
			set
			{
				AV_PostDate = value;
				PostDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostDate)); }
		}

		public bool PostDate_ReadOnly
		{
			get { return true; }
		}

		public bool IsMatched
		{
			get { return true; }
		}

		public void FullyPay(ZDateTime fullyPaidDate)
		{
		}

		public void PartiallyPay()
		{
		}

		public void GenerateMatchLinksCore()
		{
		}

		public UnmatchingResult CanUnmatch(ZDecimal matchLinkAmount)
		{
			return UnmatchingResult.CanNotUnmatch;
		}

		public void Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
		}

		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }
		public ZString VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		public ZString ShipmentHouseBill { get { return ZString.Empty; } }
		public ZString ShipmentMasterBill { get { return ZString.Empty; } }
		ZString IMatching.RelatedClaimStatus { get { return ZString.Empty; } }
		ZString IMatching.QueryNumber { get { return ZString.Empty; } }
		ZString IMatching.InvoiceRemittanceReference { get { return ZString.Empty; } }

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get
			{
				if (fCurrentMatchGroup == null)
				{
					fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory);
				}

				return fCurrentMatchGroup;
			}
		}

		TransactionMatchLinkGroup fCurrentMatchGroup;

		public TransactionMatchLinkCollection Matchlinks
		{
			get
			{
				ZQuery findMatchLinks = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK);
				return new TransactionMatchLinkCollection(Factory, findMatchLinks);
			}
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

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(RXDecimals))]
		public ZDecimal OSOutstandingAmount
		{
			get { return AV_Amount; }
		}

		public ZPropertyInfo OSOutstandingAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSOutstandingAmount)); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal OutstandingAmount
		{
			get { return AV_Calc_LocalAmount; }
		}

		public ZPropertyInfo OutstandingAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OutstandingAmount)); }
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal OriginalOutstandingAmount
		{
			get { return AV_Calc_LocalAmount; }
		}

		[ReadOnly(true)]
		public ZString TransactionType
		{
			get { return PostsOnSave ? ZArchitecture.Core.TransactionTypes.Payment : "UNA"; }
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		[ReadOnly(true)]
		public ZString TransactionCategory
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionCategory)); }
		}

		[ReadOnly(true)]
		[List("Organisations")]
		public ZGuid Organisation
		{
			get { return AV_OH; }
		}

		public ZPropertyInfo OrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(Organisation)); }
		}

		[ReadOnly(true)]
		[List("Lookups.Branches")]
		public ZGuid BranchGuid
		{
			get { return AV_GB; }
		}

		public ZPropertyInfo BranchGuidInfo
		{
			get { return GetZPropertyInfo(nameof(BranchGuid)); }
		}

		[ReadOnly(true)]
		[List("Lookups.Departments")]
		public ZGuid DepartmentGuid
		{
			get { return ZGuid.Empty; }
		}

		public ZPropertyInfo DepartmentGuidInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentGuid)); }
		}

		[ReadOnly(true)]
		public ZString Ledger
		{
			get { return AV_Ledger; }
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(Ledger)); }
		}

		[ReadOnly(true)]
		public ZString TransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumber)); }
		}

		[ReadOnly(true)]
		public ZString JobLocalReference
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo JobLocalReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(JobLocalReference)); }
		}

		[DecimalPlaces(nameof(RXDecimals))]
		public ZDecimal OSPartialPaymentAmount
		{
			get { return fOSPartialPaymentAmount.IsEmpty ? AV_Amount : fOSPartialPaymentAmount; }
			set
			{
				fOSPartialPaymentAmount = value;
				OSPartialPaymentAmountInfo.RefreshBinding();
				LocalPartialPaymentAmountInfo.RefreshBinding();
				PaymentApprovalValidation paymentApprovalValidation = Validation as PaymentApprovalValidation;
				if (paymentApprovalValidation != null && !IsValidationSuspended)
				{
					paymentApprovalValidation.ValidateOSPartialPaymentAmount();
				}
			}
		}
		ZDecimal fOSPartialPaymentAmount;

		public ZPropertyInfo OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSPartialPaymentAmount)); }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal LocalPartialPaymentAmount
		{
			get
			{
				ZDecimal localAmountTemp = OutstandingAmount;
				if (((IMatching)this).OSPartialPaymentAmount != OSOutstandingAmount)
				{
					localAmountTemp = Env.CurrentCompany.ExchangeRate.ForeignToLocal(((IMatching)this).OSPartialPaymentAmount, AV_PayExRate);
				}

				return localAmountTemp;
			}
		}

		public virtual ZPropertyInfo LocalPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalPartialPaymentAmount)); }
		}

		[MaxLength(3)]
		[ReadOnly(true)]
		public ZString PaymentCurrencyCode
		{
			get { return AV_RX_NKPaymentCurrency; }
		}

		public ZPropertyInfo PaymentCurrencyCodeInfo => GetZPropertyInfo(nameof(AV_RX_NKPaymentCurrency));

		[ReadOnly(true)]
		public ZString Description
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		[MaxLength(Schema.AV_ChequeOrReferenceMaxLength)]
		public ZString ChequeOrReference
		{
			get { return AV_ChequeOrReference; }
			set
			{
				AV_ChequeOrReference = value;
				ChequeOrReferenceInfo.RefreshBinding();
			}
		}

		public bool ChequeOrReference_ReadOnly { get; set; }

		public ZPropertyInfo ChequeOrReferenceInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChequeOrReference));
			}
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal ExchangeRateAmount
		{
			get { return AV_PayExRate; }
		}

		public ZPropertyInfo ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateAmount)); }
		}

		[ReadOnly(true)]
		public ZString TransactionReference
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo TransactionReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionReference)); }
		}

		[ReadOnly(true)]
		public ZString ConsolidatedRef
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo ConsolidatedRefInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolidatedRef)); }
		}

		[ReadOnly(true)]
		public ZString InvoiceBatchNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo InvoiceBatchNumberInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceBatchNumber)); }
		}

		[ReadOnly(true)]
		public ZDateTime InvoiceDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceDate)); }
		}

		[ReadOnly(true)]
		public ZDateTime DueDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo DueDateInfo
		{
			get { return GetZPropertyInfo(nameof(DueDate)); }
		}

		public ZString AV_ExxMatchStatus
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.AV_ExxMatchStatus);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.AV_ExxMatchStatus, AddOnColumnDataType.Codes.String, value);
			}
		}

		public ZString AV_ExxMatchStatusReasonCode
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.AV_ExxMatchStatusReasonCode);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.AV_ExxMatchStatusReasonCode, AddOnColumnDataType.Codes.String, value);
			}
		}

		public ZString AV_DscMatchStatus
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.AV_DscMatchStatus);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.AV_DscMatchStatus, AddOnColumnDataType.Codes.String, value);
			}
		}

		public ZString AV_DscMatchStatusReasonCode
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.AV_DscMatchStatusReasonCode);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.AV_DscMatchStatusReasonCode, AddOnColumnDataType.Codes.String, value);
			}
		}

		[MaxLength(3)]
		[List("MatchStatusList")]
		public ZString MatchStatus
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.MatchStatus);
			}
			set
			{
				CheckMaximumLength(MatchStatusInfo, value);
				this.SetSystemDefinedValue(Schema.MatchStatus, AddOnColumnDataType.Codes.String, value);
				MatchStatusInfo.RefreshBinding();
			}
		}

		public ReadOnlyCodeDescriptionPairList MatchStatusList => AccountingUtils.GetMatchStatusList();

		public ZPropertyInfo MatchStatusInfo
		{
			get { return GetZPropertyInfo(nameof(MatchStatus)); }
		}

		ZString IMatching.MatchStatus
		{
			get { return MatchStatus; }
			set { MatchStatus = value; }
		}

		ZPropertyInfo IMatching.MatchStatusInfo
		{
			get { return GetZPropertyInfo(nameof(MatchStatus)); }
		}

		ZString IMatching.MatchStatusReasonCode
		{
			get { return MatchStatusReasonCode; }
			set { MatchStatusReasonCode = value; }
		}

		[MaxLength(3)]
		[List("MatchStatusReasonCodeList")]
		public ZString MatchStatusReasonCode
		{
			get
			{
				return this.GetSystemDefinedValue<ZString>(Schema.MatchStatusReasonCode);
			}
			set
			{
				CheckMaximumLength(MatchStatusReasonCodeInfo, value);
				this.SetSystemDefinedValue(Schema.MatchStatusReasonCode, AddOnColumnDataType.Codes.String, value);
				MatchStatusReasonCodeInfo.RefreshBinding();
			}
		}

		public ReadOnlyCodeDescriptionPairList MatchStatusReasonCodeList => AccountingUtils.GetMatchStatusReasonCodeList();

		public ZPropertyInfo MatchStatusReasonCodeInfo
		{
			get { return GetZPropertyInfo(nameof(MatchStatusReasonCode)); }
		}

		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo
		{
			get { return GetZPropertyInfo(nameof(MatchStatusReasonCode)); }
		}

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList => AccountingUtils.GetMatchStatusList();

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList => AccountingUtils.GetMatchStatusReasonCodeList();

		[ReadOnly(true)]
		public ZDateTime MatchDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo MatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(MatchDate)); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new CreditorCollection(Factory); }
		}

		public GlbBranchCollection BranchCollection
		{
			get { return Lookups.Branches; }
		}

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return Lookups.Departments; }
		}

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		[List("Lookups.Addresses")]
		public ZGuid DisplayInvoiceAddressOverride
		{
			get { return ZGuid.Empty; }
		}

		public ZPropertyInfo DisplayInvoiceAddressOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayInvoiceAddressOverride)); }
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal NotionalWHTTax => 0M;
		public ZPropertyInfo NotionalWHTTaxInfo => GetZPropertyInfo(nameof(IMatching.NotionalWHTTax));

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal RealizedWHTTax => 0M;
		public ZPropertyInfo RealizedWTHTaxInfo => GetZPropertyInfo(nameof(IMatching.RealizedWHTTax));

		ZString IMatching.InvoiceTransactionReference => ZString.Empty;

		#endregion

		#region Payment Creation

		public void CreateNewPayment()
		{
			CreateNewPaymentCore();
		}

		public bool IsBackDatePostingNotAllowedAndPostDateNotToday
		{
			get
			{
				return (AV_PostDate.Date < ZDateTime.Today
					&& !(UserAllowedToBackPost && AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value));
			}
		}

		public bool IsFuturePostingNotAllowedAndPostDateNotToday
		{
			get
			{
				return AV_PostDate.Date > ZDateTime.Today && !AllowFuturePosting;
			}
		}

		public static ZString UpdatePostDateWarning
		{
			get
			{
				return Res.GetString("4b43e202-624e-4847-b690-39913408d6f3", "You do not have security rights to back date the Post Date.\r\nThe Post Date will automatically be updated to Today's Date when you post these transactions.\r\n\r\nAlternatively, set the Registry 'Allow Back Posting Sub Ledger Transaction' to YES and ensure that you are allowed to back post transactions in Staff and Resources before posting these transactions.");
			}
		}

		protected virtual void CreateNewPaymentCore()
		{
			try
			{
				if (IsBackDatePostingNotAllowedAndPostDateNotToday || IsFuturePostingNotAllowedAndPostDateNotToday)
				{
					AV_PostDate = ZDateTime.Now;
				}

				fPaymentCreationErrorMessages = null;
				fNewPayment = (Payment)Factory.New(GetNewPaymentType());
				fNewPayment.IsLoadedFromGUI = false;
				fNewPayment.RelatedPaymentApproval = this;
				if (!IsChequeNumberAutoAllocated)
				{
					PopulateChequeNumberFromChequeBook();
				}
				SetFieldsOnPayment(NewPayment);
				if (!HotChequeIsAlreadyImported)
				{
					ImportExistingHotCheque();
				}
				ImportHotChequeOnPayment(NewPayment);

				if (AllowFuturePosting)
				{
					NewPayment.MatchingBaseObject.MatchDate = AV_PostDate;
				}

				if (!IsPostWithoutMatching)
				{
					fNewPaymentMatchingObject = NewPayment.MatchingBaseObject;
				}

				NewPayment.OSPartialPaymentAmount = OSPartialPaymentAmount;
				AV_AH = NewPayment.PK;
				AV_Status = PaymentApprovalStatus.Posted;

				if (!IsPostWithoutMatching)
				{
					OrgLedgerFilterCollection settlementOrgInfos = MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos;
					NewPaymentMatchingObject.MatchingFilterBizO.SettlementOrgInfos.AddRange(settlementOrgInfos);

					NewPaymentMatchingObject.CreateMiscTransactionsFromPaymentApprovalDetails(this);
					MatchTransactionsToPayment(NewPaymentMatchingObject);
					if (NewPaymentMatchingObject.MatchDate.Date < MatchingBaseObject.MatchDate.Date)
					{
						NewPaymentMatchingObject.MatchDate = MatchingBaseObject.MatchDate;
					}
				}

				Validation.ValidateAll();

				if (NewPayment != null)
				{
					NewPayment.Validation.ValidateAll();
				}

				if (!IsPostWithoutMatching)
				{
					PaymentMatchingBaseObject.UpdateAndValidateBalance();
					NewPaymentMatchingObject.ValidateBalance();
					NewPayment.Validation.ValidateAll();

					if (AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.Value)
					{
						NewPaymentMatchingObject.RunPreSaveValidation();
					}
					else
					{
						NewPaymentMatchingObject.ValidateBalance();
					}
				}

				if (!PaymentHasErrors)
				{
					try
					{
						if (!IsPostWithoutMatching)
						{
							NewPaymentMatchingObject.PaymentPKOnlyForErrorReport = PK;
							using (NewPaymentMatchingObject.ValidateBalanceSuspender.GetSuspender())
							{
								NewPaymentMatchingObject.MatchAndClearTransactions();
							}
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						NewPayment.AddRowError(Res.GetString("b5148b54-fb19-414d-ae4c-d5796c5574cd", "Payment matching error: {0}", e.Message));
					}
				}
			}
			finally
			{
				if (PaymentHasErrors)
				{
					if (!IsPostWithoutMatching)
					{
						PaymentCreationErrorMessages.AddRange(new ZNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors());
						PaymentCreationErrorMessages.AddRange(PaymentMatchingBaseObject != null && PaymentMatchingBaseObject.BalanceInfo != null ? PaymentMatchingBaseObject.BalanceInfo.GetErrors() : NotificationCollection.Empty);
						PaymentCreationErrorMessages.AddRange(NewPayment != null ? new ZNotificationCollector(NewPayment, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors() : NotificationCollection.Empty);
						PaymentCreationErrorMessages.AddRange(NewPaymentMatchingObject != null ? new ZNotificationCollector(NewPaymentMatchingObject, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors() : NotificationCollection.Empty);
					}
					else
					{
						PaymentCreationErrorMessages.AddRange(new ZNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors());
						PaymentCreationErrorMessages.AddRange(NewPayment != null ? new ZNotificationCollector(NewPayment, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors() : NotificationCollection.Empty);
					}
					UndoCreateNewPayment();
				}
			}
		}

		protected abstract Type GetNewPaymentType();

		bool PaymentHasErrors
		{
			get
			{
				bool newPaymentHasErrors = NewPayment?.HasErrors ?? true;
				bool newPaymentMatchingObjectErrors = (NewPaymentMatchingObject?.HasErrors ?? false) && AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.Value;
				return HasErrors || newPaymentHasErrors || newPaymentMatchingObjectErrors;
			}
		}

		protected void SetFieldsOnPayment(Payment payment)
		{
			payment.AH_InvoiceDate = AV_PaymentDate;
			payment.AH_PostDate = AV_PostDate;
			payment.AH_GB = AV_GB;
			payment.AH_GE = DepartmentForImport.IsValid ? DepartmentForImport : GlbDepartment.CurrentDepartment.PK;
			payment.AH_OH = AV_OH;
			payment.AH_Desc = AV_PaymentComment;
			payment.AH_ReceiptType = AV_PaymentType;
			payment.AH_AB = AV_AB;
			payment.ChequeBook = AV_AK;
			payment.AH_RX_NKTransactionCurrency = AV_RX_NKPaymentCurrency;
			payment.AH_ChequeOrReference = AV_ChequeOrReference;
			payment.AH_ExchangeRate = AV_PayExRate;
			payment.AH_OSExTaxAmount = AV_Amount;
			payment.AH_LocalExTaxAmount = AV_Calc_LocalAmount;
			payment.CurrentMatchingDate = CurrentMatchingDate;
			payment.OSPartialPaymentAmount = OSPartialPaymentAmount;
			payment.AH_InvoiceApproved = ZBool.True;
			payment.AH_NumberOfSupportingDocuments = AH_NumberOfSupportingDocuments;
			payment.AH_OA_InvoiceAddressOverride = AV_OA_AddressOverride;
			payment.AH_OC_InvoiceContactOverride = AV_OC_ContactOverride;
			payment.AH_MatchStatus = MatchStatus;
			payment.AH_MatchStatusReasonCode = MatchStatusReasonCode;

			if (payment is APPayment apPayment)
			{
				foreach (Charge charge in RelatedCharges)
				{
					apPayment.AddRelatedCharge(charge);
				}
			}
		}

		public void SetFieldsFromPayment(Payment payment)
		{
			AV_PaymentDate = payment.AH_InvoiceDate;
			AV_PostDate = payment.AH_PostDate;
			AV_GB = payment.AH_GB;
			AV_GC = payment.AH_GC;
			DepartmentForImport = payment.AH_GE;
			AV_OH = payment.AH_OH;
			AV_PaymentComment = payment.AH_Desc;
			AV_PaymentType = payment.AH_ReceiptType;
			AV_AB = payment.AH_AB;
			AV_ChequeOrReference = payment.AH_ChequeOrReference;
			AV_AK = payment.ChequeBook;
			if (!AV_ChequeOrReference.IsEmpty)
			{
				AV_ChequeOrReference = payment.AH_ChequeOrReference;
			}
			AV_RX_NKPaymentCurrency = payment.AH_RX_NKTransactionCurrency;
			AV_PayExRate = payment.AH_ExchangeRate;
			AV_Amount = payment.AH_OSExTaxAmount;
			AV_Calc_LocalAmount = payment.AH_LocalExTaxAmount;
			AH_NumberOfSupportingDocuments = payment.AH_NumberOfSupportingDocuments;

			AV_OA_AddressOverride = payment.AH_OA_InvoiceAddressOverride;
			AV_OC_ContactOverride = payment.AH_OC_InvoiceContactOverride;
		}

		void ImportHotChequeOnPayment(Payment payment)
		{
			if (IsHotChequeImported)
			{
				APPayment paymentAsAPPayment = payment as APPayment;

				if (paymentAsAPPayment != null)
				{
					paymentAsAPPayment.SetHotChequeInactiveWhenPosting(ImportedHotCheque);
				}
			}
		}

		void MatchTransactionsToPayment(MatchingBase paymentMatchingObject)
		{
			PaymentApprovalItemCollection paymentApprovalItems = new PaymentApprovalItemCollection(this);
			paymentApprovalItems.Load();

			foreach (PaymentApprovalItem currentItem in paymentApprovalItems)
			{
				IMatching transaction = currentItem.Header as IMatching;

				if (transaction != null)
				{
					paymentMatchingObject.MatchedTransactions.Add(transaction);
					transaction.OSPartialPaymentAmount = currentItem.OSAmountPaidThisRun;
				}
			}
		}

		public NotificationCollection PaymentCreationErrorMessages
		{
			get
			{
				if (fPaymentCreationErrorMessages == null)
				{
					fPaymentCreationErrorMessages = new NotificationCollection();
				}
				return fPaymentCreationErrorMessages;
			}
		}

		NotificationCollection fPaymentCreationErrorMessages;

		public MatchingBase NewPaymentMatchingObject
		{
			get { return fNewPaymentMatchingObject; }
		}

		MatchingBase fNewPaymentMatchingObject;

		public Payment NewPayment
		{
			get { return fNewPayment; }
		}

		Payment fNewPayment;

		#endregion

		#region DDR Validation

		public bool AccountDetailsFound
		{
			get { return AccountDetails != null; }
		}

		AccAPAccountDetails AccountDetails
		{
			get { return Factory.GetCachedValue(GetCachingKey(), GetAccountDetails); }
		}

		AccAPAccountDetails GetAccountDetails()
		{
			if (Header != null)
			{
				var key = GetCachingKey().Split('|');
				if (key.Length != 3)
				{
					throw new InvalidOperationException("GetCachingKey().Split('|') must have length 3");
				}
				// key[0] is guid for Header.
				var paymentMethod = key[1];
				var currencyCode = key[2];
				Header.CompanyData.AccountDetailsCollection.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
				return Header.CompanyData.AccountDetailsCollection.GetAccountDetails(paymentMethod, currencyCode, true);
			}

			return null;
		}

		public void ResetAccountDetails()
		{
			Factory.ClearCachedValue<AccAPAccountDetails>(GetCachingKey());
		}

		string GetCachingKey()
		{
			var paymentMethod = AV_PaymentType;
			if (AV_PaymentType == ReceiptTypes.EPayment && BankAccount != null)
			{
				switch (BankAccount.AB_PaymentProvider)
				{
					case EPaymentProviderCodes.Codes.OFX:
						paymentMethod = EPaymentMethods.EPaymentViaOFX;
						break;
				}
			}

			var currencyCode = !AV_RX_NKPaymentCurrency.IsEmpty ? AV_RX_NKPaymentCurrency : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			return $"{AV_OH}|{paymentMethod}|{currencyCode}";
		}

		public ZBool AllowAutoDDR
		{
			get
			{
				ZBool result = ZBool.False;
				if (AccountDetails != null)
				{
					result = AccountDetails.AutoDirectDebit;
				}
				return result;
			}
		}

		public ZString PayeeBankBSB
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_BankBsb;
				}
				return result;
			}
		}

		public ZString AccountTitle
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_AccountName;
				}
				return result;
			}
		}

		public ZString PayeeBankAccountNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_BankAccount;
				}
				return result;
			}
		}

		public ZString PayeeBankName => AccountDetails?.A1_BankName ?? ZString.Empty;
		public ZPropertyInfo PayeeBankNameInfo => GetZPropertyInfo(nameof(PayeeBankName));

		public ZString PayeeBankAccountCountry => AccountDetails?.A1_RN_NKCountryCode ?? ZString.Empty;
		public ZPropertyInfo PayeeBankAccountCountryInfo => GetZPropertyInfo(nameof(PayeeBankAccountCountry));

		public ZString BankCreateUser => AccountDetails?.A1_SystemCreateUser ?? ZString.Empty;
		public ZPropertyInfo BankCreateUserInfo => GetZPropertyInfo(nameof(BankCreateUser));

		public ZDateTime BankCreateTimeLocal => AccountDetails?.A1_SystemCreateTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo BankCreateTimeLocalInfo => GetZPropertyInfo(nameof(BankCreateTimeLocal));

		public ZString BankLastEditUser => AccountDetails?.A1_SystemLastEditUser ?? ZString.Empty;
		public ZPropertyInfo BankLastEditUserInfo => GetZPropertyInfo(nameof(BankLastEditUser));

		public ZDateTime BankLastEditTimeLocal => AccountDetails?.A1_SystemLastEditTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo BankLastEditTimeLocalInfo => GetZPropertyInfo(nameof(BankLastEditTimeLocal));

		#endregion

		#region Overriden Readonly properties

		protected virtual bool AV_PayExRate_ReadOnly
		{
			get
			{
				bool result = false;
				if (!inAV_PayExRate_ReadOnly)
				{
					inAV_PayExRate_ReadOnly = true;
					result = ExchangeRate.IsRateReadOnly;
					inAV_PayExRate_ReadOnly = false;
				}
				return result;
			}
		}
		bool inAV_PayExRate_ReadOnly;

		protected bool AV_PostDate_ReadOnly
		{
			get { return false; }
		}

		internal bool AllowBackPosting
		{
			get { return AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && UserAllowedToBackPost; }
		}

		internal bool AllowFuturePosting
		{
			get { return AccountingUtils.IsAllowFuturePostingRegistryEnabled && AccountingUtils.DoesUserHaveFuturePostingSecurity; }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !IsPosted && !HasActiveDeal;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (HasActiveDeal)
				{
					return ActiveDealErrorTextForActionItems;
				}
				else
				{
					return ResString.GetMultilingualString("Accounting|PaymentProcessing|YouCannotDeleteThisTransactionBecauseItIsPosted", "You cannot delete this Transaction because it is posted.");
				}
			}
		}

		#endregion

		#region Implementation

		List<Charge> fRelatedCharges;
		public List<Charge> RelatedCharges
		{
			get { return fRelatedCharges ?? (fRelatedCharges = new List<Charge>()); }
		}

		IMatchingCollection ParentMatchingCollection
		{
			get
			{
				IMatchingCollection result = null;

				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						if (collection is IMatchingCollection)
						{
							result = (IMatchingCollection)collection;
							break;
						}
					}
				}

				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PaymentApprovalFetchStrategy(this);
		}

		public virtual void RunPrePostingValidation()
		{
			Validation.ValidateAll();
		}
		
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AV_PaymentDate = ZDateTime.Now;
			AV_PostDate = ZDateTime.Now;
			AV_Ledger = DefaultLedger;
			AV_GB = GlbBranch.CurrentBranch.PK;
			AV_GC = GlbCompany.CurrentCompany.PK;
			AV_PayExRate = 1M;
			ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AV_Status = PaymentApprovalStatus.FullyApproved;
			AV_PaymentComment = DefaultDescription;// DefaultLedger + " PAYMENT";
			AV_PaymentType = AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;

			ZString transactionType = TransactionType.ToString() == AccountingConstants.VoucherItemRegistryCode.UNPaymentTransactionType ? TransactionTypes.Payment : TransactionType.ToString();
			AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(DefaultLedger + transactionType, 1);
		}

		protected abstract ZString DefaultLedger
		{
			get;
		}

		protected override void OnFactorySaving()
		{
			if (HasChanges && IsPaymentCreated)
			{
				if (ShouldAllocateChequeNumber())
				{
					var nextChequeNumber = new AccountingUtils().GetNextChequeNumberFromActiveChequeBookWithLock(ChequeBook, Factory);
					if (nextChequeNumber != 0)
					{
						((IChequeNumberAutoAllocation)NewPayment).AssignChequeNumber(nextChequeNumber.ToString());
					}
				}
			}

			base.OnFactorySaving();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!IsPostWithoutMatching)
			{
				if (HasChanges && IsInMatchingContext)
				{
					PaymentMatchingBaseObject.CreateTemporaryTransactions();
				}
				if (fPaymentMatchingBaseObject != null)
				{
					PaymentMatchingBaseObject.DeleteTemporaryTransactions(!HasChanges);
				}
			}

			if (HasChanges || ShouldPostPayment)
			{
				PrepareForSave();
				CreateLogsBeforeSaving();
			}

			if (!IsInDatabase)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}

			DiscardActiveFXQuotes(providerCode: string.Empty, discardOnlyIfPaymentDetailsAreChanged: true, shouldCheckHasExistedQuoteInDB: true);

			if (IsInDatabase && ParentPaymentBatch == null && FundingCurrency != OriginalFundingCurrency)
			{
				var query = GetCurrentDealQuery();
				query.AddToFilter(AccEPaymentDealSchema.AED_Status, SQLComparisonOperator.Contains, EPaymentStatusCodes.Deal.ActiveStatusCodes);

				if (Factory.Exists(typeof(AccEPaymentDeal), query, mergeDbAndCacheResult: false))
				{
					throw new ZCannotSaveException(Res.GetString("016BB563-CDC8-49A7-A0D3-473638B47B99", "Another user created an active E-Payment Deal for this payment batch. Change of Funding Currency is not permitted."),
						Res.GetString("2097a2f7-3676-47b6-ae38-3e3b0de51f4d", "Report Error"));
				}

				DiscardActiveFXQuotes(providerCode: string.Empty, discardOnlyIfPaymentDetailsAreChanged: false, activeQuoteStatuses: QuoteStatusCodes.ActiveStatusCodes);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		protected virtual bool ShouldPostPayment => false;

		protected virtual void CreateLogsBeforeSaving()
		{
			CreateLogForTransactionPosted();
		}

		void PrepareForSave()
		{
			if (IsAllowedToPost && IsFullyApproved)
			{
				IsAllowedToPost = false;

				CreateNewPayment();
				IsPaymentCreated = true;
			}
		}

		bool IsPaymentCreated;

		protected virtual bool ShouldAllocateChequeNumber()
		{
			return NewPayment != null && NewPayment.AH_ReceiptType == ReceiptTypes.Cheque && NewPayment.ChequeBookBizO != null && NewPayment.ChequeBookBizO.AK_AutoPrintCheque
				&& NewPayment.AH_ChequeOrReference.IsEmpty;
		}

		internal virtual bool IsAllowedToPost
		{
			get;
			set;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (PostsOnSave)
				{
					ReadOnly = true;
				}

				UpdateChequeBookCurrentNo();
			}
			else
			{
				if (NewPayment != null && !NewPayment.AH_ChequeOrReference.IsEmpty)
				{
					NewPayment.AH_ChequeOrReference = string.Empty;
				}
			}
		}

		public ZDecimal UpdateChequeBookCurrentNo()
		{
			ZDecimal result = ZDecimal.Zero;

			if (IsCheque && CanIncrementChequeBookNumber && !IsChequeNumberAutoAllocated)
			{
				AccChequeBook.UpdateCurrentNumber(ChequeBook.PK, result = ZDecimal.Parse(AV_ChequeOrReference) + 1);
			}

			return result;
		}

		protected override AccPaymentApprovalLookups GetNewLookups()
		{
			return new PaymentApprovalLookups(this);
		}

		protected override AccPaymentApprovalValidation GetNewValidation()
		{
			return new PaymentApprovalValidation(this);
		}

		public override void Delete()
		{
			if (fPaymentMatchingBaseObject != null)
			{
				fPaymentMatchingBaseObject.Delete();
			}

			if (!IsDeleted)
			{
				PaymentBatch?.SetNeedUpdateBatchStatusWhenSaving();
			}

			PaymentQuotes.RemoveAndDeleteAll();

			PaymentApprovalItemCollection paymentApprovalItems = new PaymentApprovalItemCollection(this);
			paymentApprovalItems.Load();
			paymentApprovalItems.RemoveAndDeleteAll();

			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();

			base.Delete();
		}

		public void UndoCreateNewPayment()
		{
			if (IsPostDraftPaymentApproval)
			{
				Previous_AV_Status = PaymentApprovalStatus.Draft;
			}
			if (!Previous_AV_Status.IsEmpty)
			{
				AV_Status = Previous_AV_Status;
			}
			AV_AH = ZGuid.Empty;

			if (NewPaymentMatchingObject != null)
			{
				foreach (TransactionMatchLink matchLink in NewPaymentMatchingObject.MatchLinks)
				{
					if (!(matchLink.MatchingTransaction is Discount)
						&& !(matchLink.MatchingTransaction is Overpayment.Overpayment)
						&& !(matchLink.MatchingTransaction is ExchangeDifference))
					{
						matchLink.Unmatch();
					}
				}
				NewPaymentMatchingObject.MatchLinks.RemoveAndDeleteAll();
				NewPaymentMatchingObject.DeleteCachedMiscTransactions();
				NewPaymentMatchingObject.MatchedTransactions.RemoveAll();
			}
			if (NewPayment != null)
			{
				NewPayment.Delete();
			}
			if (NewPaymentMatchingObject != null)
			{
				NewPaymentMatchingObject.Delete();
			}
			fNewPayment = null;
			fNewPaymentMatchingObject = null;
			HasChanges = false;
		}

		public bool PostsOnSave
		{
			get { return PostsOnSaveCore; }
		}

		protected virtual bool PostsOnSaveCore
		{
			get { return !IsDraft || PaymentBatch != null; }
		}

		public new PaymentApprovalLookups Lookups
		{
			get { return (PaymentApprovalLookups)base.Lookups; }
		}

		protected AccValidationHelper AccValidationHelper
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

		protected abstract ExchangeRateType GetRateType();

		void CheckNumberIsAutoAllocated()
		{
			AV_Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
			AV_Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			if (IsChequeNumberAutoAllocated)
			{
				AV_ChequeOrReference = ZString.Empty;
			}
		}

		#region IeNettPayment Members

		public ZString PaymentType
		{
			get { return AV_PaymentType; }
		}

		public eNettWebServiceResult PayWithCreditCardViaENett()
		{
			IeNettWebServiceWrapper wrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
			return wrapper.ProcessCreditCard((TransactionHeader)TransactionHeader, CreditCardSecurityCode);
		}

		#endregion
		#endregion

		#region E-Payment Quote

		public EPaymentQuoteForDisplayCollection PaymentQuotes_ForDisplay
		{
			get
			{
				if (paymentQuotes_ForDisplay == null)
				{
					paymentQuotes_ForDisplay = new EPaymentQuoteForDisplayCollection(PaymentQuotes);
					paymentQuotes_ForDisplay.Load();
				}

				return paymentQuotes_ForDisplay;
			}
		}
		EPaymentQuoteForDisplayCollection paymentQuotes_ForDisplay;

		public EPaymentQuoteCollection PaymentQuotes
		{
			get
			{
				if (paymentQuotes == null)
				{
					paymentQuotes = new EPaymentQuoteCollection(this);
					paymentQuotes.Load();
				}

				return paymentQuotes;
			}
		}
		EPaymentQuoteCollection paymentQuotes;

		public QuoteCreationResult TryToCreateQuote(string providerCode, bool skipCheckingForDuplicateRequestedQuotes = false)
		{
			if (skipCheckingForDuplicateRequestedQuotes)
			{
				return new QuoteCreationResult(CreateQuote(), string.Empty, EPaymentDealCreator.QuoteAcceptingStatus.NoErrors);
			}
			else
			{
				var requestedQuoteStatuses = new ZString[]
					{
					QuoteStatusCodes.Queued,
					QuoteStatusCodes.Requested
					};

				var requestedQuotes = PaymentQuotes.Cast<AccEPaymentQuote>()
													.Where(q => requestedQuoteStatuses.Contains(q.QU_Status) &&
																q.QU_ProviderCode == providerCode &&
																q.QU_ToAmount == AV_Amount &&
																q.QU_RX_NKToCurrency == AV_RX_NKPaymentCurrency);

				if (!(requestedQuotes?.Any() ?? false))
				{
					return new QuoteCreationResult(CreateQuote(), string.Empty, EPaymentDealCreator.QuoteAcceptingStatus.NoErrors);
				}
				else if (requestedQuotes.Any(x => x.QU_Status == QuoteStatusCodes.Requested))
				{
					return new QuoteCreationResult(null, Res.GetString("82bd4a8d-8b44-4f1a-8c7e-d5e1af5bf6b1", "E-Quote already requested. Response may take up to several minutes to be received. If you generate a new request, then the previous request will be discarded. Are you sure you want to proceed?"), EPaymentDealCreator.QuoteAcceptingStatus.QuoteRequested);
				}
				else
				{
					return new QuoteCreationResult(null, Res.GetString("ac2fd802-3011-42dc-a433-bec91145b052", "Exchange rate already requested"), EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors);
				}
			}

			EPaymentQuote CreateQuote()
			{
				var quote = PaymentQuotes.AddNew();
				quote.QU_AV = PK;
				quote.QU_ProviderCode = providerCode;
				quote.QU_ToAmount = AV_Amount;
				quote.QU_RX_NKToCurrency = AV_RX_NKPaymentCurrency;
				quote.QU_RX_NKFromCurrency = EPaymentFundingInfoProviderFactory.CreateProvider(this).GetFundingCurrency();
				quote.QU_GC = GlbCompany.CurrentCompany.PK;
				return quote;
			}
		}

		public void DiscardActiveFXQuotes(string providerCode, bool discardOnlyIfPaymentDetailsAreChanged, bool shouldCheckHasExistedQuoteInDB = false, string[] activeQuoteStatuses = null)
		{
			if (activeQuoteStatuses == null)
			{
				activeQuoteStatuses = new string[]
				{
					QuoteStatusCodes.Accepted,
					QuoteStatusCodes.Expired,
					QuoteStatusCodes.Error,
					QuoteStatusCodes.Failed,
					QuoteStatusCodes.Received,
					QuoteStatusCodes.Requested
				};
			}

			var quotesNeedToBediscarded = PaymentQuotes
											.Cast<AccEPaymentQuote>()
											.Where(q => q.QU_Status != QuoteStatusCodes.Discarded &&
														(q.QU_ProviderCode == providerCode || string.IsNullOrEmpty(providerCode)) &&
														((!discardOnlyIfPaymentDetailsAreChanged && activeQuoteStatuses.Contains(q.QU_Status.ToString())) ||
															q.QU_ToAmount != AV_Amount ||
															q.QU_RX_NKToCurrency != CurrencyCode));

			if (shouldCheckHasExistedQuoteInDB
				&& IsInDatabase
				&& HasChanges
				&& !IsLoadedFromPaymentBatch)
			{
				PaymentApprovalConcurrencyHelper.CheckAndReportNewQuoteInDB(PK, PaymentQuotes.Select(x => x.PK));
			}

			if (quotesNeedToBediscarded.Any())
			{
				quotesNeedToBediscarded.ForEach(q =>
				{
					q.QU_Status = QuoteStatusCodes.Discarded;
				});
			}
		}

		public void RefreshQuotes()
		{
			PaymentQuotes.Reload(true);
			PaymentQuotes_ForDisplay.Load();
			PaymentQuotes_ForDisplay.RefreshBindingIncludingChildren();
		}

		public bool CanCreateQuotes => (!IsInDatabase ||
									(AV_Status == PaymentApprovalStatus.AwaitingApproval
									|| AV_Status == PaymentApprovalStatus.FullyApproved
									|| AV_Status == PaymentApprovalStatus.Rejected
									|| AV_Status == PaymentApprovalStatus.Draft))
								&& CurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		#endregion

		#region E-Payment Deal

		public bool CheckIfDealIsConfirmedByProvider(out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			var isDealConfirmed = true;
			if (BankAccount.IsEPaymentAccount)
			{
				if (CurrentDeal == null)
				{
					isDealConfirmed = false;
					errorMessage = Res.GetString("19cb66c3-321e-4077-9f8f-d16801142fbc", "Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.");
				}
				else if (!DealStatusCodes.ProviderConfirmedStatusCodes.Contains(CurrentDeal.AED_Status.ToString()))
				{
					isDealConfirmed = false;
					errorMessage = Res.GetString("afc971ca-0f0c-4bd9-a26d-a4dac3d9f698", "Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.");
				}
			}
			return isDealConfirmed;
		}

		public bool HasActiveDeal => CurrentDeal != null && DealStatusCodes.ActiveStatusCodes.Contains(CurrentDeal.AED_Status.ToString());

		public static MultilingualString ActiveDealErrorTextForActionItems => ResString.GetMultilingualString("f36f184b-a078-4056-b057-b06ed48cf3a8", "The payment has an active E-Payment Deal. Action not permitted.");

		public static string ActiveDealErrorTextForChangingPaymentDetails => Res.GetString("41f9eeab-c224-4836-bf79-312a449b393e", "This payment has an active E-Payment Deal. Changing payment details is not permitted.");

		public AccEPaymentDeal CurrentDeal => (currentDealWrapper ?? (currentDealWrapper = new PaymentDealWrapper(GetCurrentDealFromDB()))).AccEPaymentDeal;
		PaymentDealWrapper currentDealWrapper;

		class PaymentDealWrapper
		{
			public AccEPaymentDeal AccEPaymentDeal { get; }

			public PaymentDealWrapper(AccEPaymentDeal accEPaymentDeal)
			{
				AccEPaymentDeal = accEPaymentDeal;
			}
		}

		internal AccEPaymentQuote CurrentDealQuote => CurrentDeal?.Quote;

		public void RefreshCurrentDeal()
		{
			currentDealWrapper = new PaymentDealWrapper(GetCurrentDealFromDB());
			currentDealWrapper.AccEPaymentDeal?.ReloadSafe();
		}

		internal void ResetCurrentDeal() => currentDealWrapper = null;

		AccEPaymentDeal GetCurrentDealFromDB()
		{
			var query = GetCurrentDealQuery();
			query.OrderBy = AccEPaymentDeal.Schema.AED_SystemCreateTimeUtc + " desc";
			return Factory.LoadTop1<AccEPaymentDeal>(query);
		}

		ZQuery GetCurrentDealQuery()
		{
			var query = new ZDBOnlyQuery(typeof(AccEPaymentDeal));
			query.ReLoadExistingRows = true;

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(AccEPaymentQuote), AccEPaymentQuoteSchema.PK);
			quoteSubQuery.AddToFilter(AccEPaymentQuoteSchema.QU_AV, PK);
			query.AddSubQuery(AccEPaymentDealSchema.AED_QU_Quote, quoteSubQuery, JoinCondition.And);

			return query;
		}

		public ZString DealProvider => CurrentDeal?.AED_ProviderCode ?? ZString.Empty;
		public ZPropertyInfo DealProviderInfo => GetZPropertyInfo(nameof(DealProvider));

		public ZString DealProviderReference => CurrentDeal is null ? ZString.Empty :
			(CurrentDeal.AED_ProviderCode == EPaymentProviderCodes.Codes.OFX ?
			CurrentDeal.AED_ProviderReference.SubstringSafe(0, 8) : CurrentDeal.AED_ProviderReference);

		public ZPropertyInfo DealProviderReferenceInfo => GetZPropertyInfo(nameof(DealProviderReference));

		public ZString DealStatus => CurrentDeal?.AED_Status ?? ZString.Empty;
		public ZPropertyInfo DealStatusInfo => GetZPropertyInfo(nameof(DealStatus));

		public ZString DealStatusDescription
		{
			get
			{
				ZString description = $"{DealStatus} - {PaymentApprovalLookups.EPaymentStatusList.GetDescriptionFromCode(DealStatus)}";
				return string.IsNullOrEmpty(DealStatus) ? ZString.Empty : description;
			}
		}

		public ZPropertyInfo DealStatusDescriptionInfo => GetZPropertyInfo(nameof(DealStatusDescription));

		public ZString DealErrorMessage => CurrentDeal?.AED_ErrorDescription ?? ZString.Empty;
		public ZPropertyInfo DealErrorMessageInfo => GetZPropertyInfo(nameof(DealErrorMessage));

		public ZDateTime DealSubmittedUtc => CurrentDeal?.AED_SystemCreateTimeUtc ?? ZDateTime.Empty;
		public ZPropertyInfo DealSubmittedUtcInfo => GetZPropertyInfo(nameof(DealSubmittedUtc));

		public ZDateTime DealSubmittedLocalTime => DealSubmittedUtc.ToLocalBranchTime();
		public ZPropertyInfo DealSubmittedLocalTimeInfo => GetZPropertyInfo(nameof(DealSubmittedLocalTime));

		public ZDateTime DealLastResponseUtc => CurrentDeal?.AED_LastResponseReceivedUtc ?? ZDateTime.Empty;
		public ZPropertyInfo DealLastResponseUtcInfo => GetZPropertyInfo(nameof(DealLastResponseUtc));

		public ZDateTime DealLastResponseLocalTime => DealLastResponseUtc.ToLocalBranchTime();
		public ZPropertyInfo DealLastResponseLocalTimeInfo => GetZPropertyInfo(nameof(DealLastResponseLocalTime));

		[DecimalPlaces(nameof(DealTotalCostCurrencyDecimals))]
		public ZDecimal DealTotalCost => CurrentDealQuote == null ? 0 : (CurrentDealQuote.QU_FromAmount + CurrentDealQuote.QU_FeeAmount);
		public ZPropertyInfo DealTotalCostInfo => GetZPropertyInfo(nameof(DealTotalCost));

		[DecimalPlaces(nameof(DealTotalCostCurrencyDecimals))]
		public ZDecimal DealTotalFees => CurrentDealQuote == null ? 0 : CurrentDealQuote.QU_FeeAmount;
		public ZPropertyInfo DealTotalFeesInfo => GetZPropertyInfo(nameof(DealTotalFees));

		[List("Lookups.PaymentCurrencies")]
		public ZString DealTotalCostCurrencyCode => CurrentDealQuote?.QU_RX_NKFromCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		public ZPropertyInfo DealTotalCostCurrencyCodeInfo => GetZPropertyInfo(nameof(DealTotalCostCurrencyCode));

		protected int DealTotalCostCurrencyDecimals => CurrentDealQuote?.FromRXDecimals ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region Funding Currency

		[List(nameof(Currencies))]
		public ZString FundingCurrency => EPaymentFundingInfoProviderFactory.CreateProvider(this).GetFundingCurrency();

		public ZPropertyInfo FundingCurrencyInfo => GetWrappedZPropertyInfo(nameof(FundingCurrency), x => AV_AB_FundingBankAccountInfo);

		public ZString OriginalFundingCurrency => EPaymentFundingInfoProviderFactory.CreateProvider(this).GetOriginalFundingCurrency();

		[List("Lookups.FundingBankAccounts")]
		public ZGuid FundingBankAccountPK
		{
			get
			{
				return EPaymentFundingInfoProviderFactory.CreateProvider(this).GetFundingBankAccount();
			}
			set
			{
				base.AV_AB_FundingBankAccount = value;
				FundingBankAccountPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FundingBankAccountPKInfo => GetWrappedZPropertyInfo(nameof(FundingBankAccountPK), x => AV_AB_FundingBankAccountInfo);

		#endregion

		internal ZString EPaymentProvider => BankAccount?.AB_PaymentProvider ?? ZString.Empty;

		public ZDateTime EPaymentRecipientListLastUpdatedTimeLocal => PaymentBatch?.MatchEPaymentRecipients?.LastReceivedRequest?.ABR_LastResponseReceivedUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo EPaymentRecipientListLastUpdatedTimeLocalInfo => GetZPropertyInfo(nameof(EPaymentRecipientListLastUpdatedTimeLocal));

		public AccEPaymentBeneficiary EPaymentBeneficiary => AccountDetails?.EPaymentBeneficiary;

		public ZDateTime EPaymentBeneficiaryLastEditTimeLocal => EPaymentBeneficiary?.ABF_SystemLastEditTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo EPaymentBeneficiaryLastEditTimeLocalInfo => GetZPropertyInfo(nameof(EPaymentBeneficiaryLastEditTimeLocal));

		GetFxQuoteResult LastENettQuoteResult
		{
			get;
			set;
		}

		public delegate void ShowMesageDelegate(string message);

		public GetFxQuoteResult GetEnettExchangeRate(ShowMesageDelegate showMesageDelegate)
		{
			GetFxQuoteResult result = new GetFxQuoteResult();
			if (Header != null && !CurrencyCode.IsEmpty && AV_Amount != 0m)
			{
				IeNettWebServiceWrapper wrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
				//uncomment the following two code blocks to test with random exchange rates
				/*#if DEBUG
								Random random = new Random();
								decimal modifier = ZArchitecture.Core.Utilities.Round((decimal)random.NextDouble() * 0.2m, Env.CurrentCompany.ExchangeRate.RateDecimals);
								GetFxQuoteResult webMethodResult = wrapper.GetFxQuote(Header, AV_PaymentType, CurrencyCode, AV_Amount, modifier);
				#else*/
				GetFxQuoteResult webMethodResult = wrapper.GetFxQuote(Header, AV_PaymentType, CurrencyCode, AV_Amount);
				//#endif
				bool success = webMethodResult.Success;
				if (success)
				{
					result = webMethodResult;
					LastENettQuoteResult = result;
					if ((webMethodResult.Rate == 0m || webMethodResult.LocalAmount == 0m) && !string.IsNullOrEmpty(webMethodResult.Message))
					{
						success = false;
					}
				}

				if (!success)
				{
					string message = Res.GetString("d5ea49e7-a543-46fd-ad54-2d91ce45cfe9", "Failed to get exchange rate from ComPay");
					if (webMethodResult.Message != null)
					{
						message += System.Environment.NewLine +
						System.Environment.NewLine +
						webMethodResult.Message;
					}

					if (showMesageDelegate != null)
					{
						showMesageDelegate(message);
					}
				}
			}

			return result;
		}

		public void SetExchangeRateFromEnett()
		{
			if (UseExchangeRateFromENettWebService)
			{
				GetFxQuoteResult exchangeDetails = GetEnettExchangeRate(null);
				AV_Calc_LocalAmount = exchangeDetails.LocalAmount;
			}
		}

		#region IeNettPayment Members

		public void PayWithEnettDirectDebitFX()
		{
			ENettSend();
		}

		void ENettSend()
		{
			if (IsENettPayment)
			{
				IeNettWebServiceWrapper wrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
				bool success = true;
				eNettWebServiceResult result = new eNettWebServiceResult();
				if (AV_RX_NKPaymentCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && LastENettQuoteResult.QuoteID != 0)
				{
					success = false;
					result = wrapper.ProcessDirectDebitFx(NewPayment, LastENettQuoteResult.QuoteID);
					success = result.success;
				}
				if (!success)
				{
					throw new ENettProcessDirectDebitFxException("Failed to send payment to ComPay - Payment is not posted", result.errorCode, result.errorMessage);
				}
			}
		}

		#endregion

		#region OrganisationAddressWithContact

		void UpdatePaymentAddress()
		{
			OrganisationAddressWithContact.OrgPK = AV_OH;
		}

		public ZAddressWithContact OrganisationAddressWithContact
		{
			get { return organisationAddressWithContact ?? (organisationAddressWithContact = GetOrganisationAddressWithContact()); }
		}
		ZAddressWithContact organisationAddressWithContact;

		ZAddressWithContact GetOrganisationAddressWithContact()
		{
			var addressWithContact = new ZAddressWithContact(AV_OC_ContactOverrideInfo, AV_OA_AddressOverrideInfo);
			addressWithContact.GetDefaultAddress = GetDefaultAddress;

			return addressWithContact;
		}

		ZGuid GetDefaultAddress(IOrgHeader org)
		{
			OrgAddress defaultAddress = null;
			OrgHeader header = org as OrgHeader;

			if (header != null)
			{
				defaultAddress = header.AddressForSendingAPDocuments;
			}

			return defaultAddress == null ? ZGuid.Empty : defaultAddress.PK;
		}

		protected bool AV_OA_AddressOverride_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.EditPaymentAddress.Value; }
		}

		protected bool AV_OC_ContactOverride_ReadOnly
		{
			get { return AV_OA_AddressOverride_ReadOnly; }
		}

		#region AV_OA_AddressOverride

		[List("AV_OA_AddressOverrides")]
		public override ZGuid AV_OA_AddressOverride
		{
			get
			{
				if (base.AV_OA_AddressOverride.IsValid)
				{
					return base.AV_OA_AddressOverride;
				}
				else
				{
					return GetDefaultAddress(Header);
				}
			}
			set { base.AV_OA_AddressOverride = value; }
		}

		public OrgAddressDependentCollection AV_OA_AddressOverrides
		{
			get
			{
				var addresses = new OrgAddressDependentCollection(Factory);
				var parent = Header;
				if (parent != null)
				{
					var filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					addresses = new OrgAddressDependentCollection(parent, filter);
					addresses.Load();
				}
				return addresses;
			}
		}

		#endregion

		#region AV_OC_ContactOverride

		[List("AV_OC_ContactOverrides")]
		public override ZGuid AV_OC_ContactOverride
		{
			get { return base.AV_OC_ContactOverride; }
			set { base.AV_OC_ContactOverride = value; }
		}

		public OrgContactDependentCollection AV_OC_ContactOverrides
		{
			get
			{
				var contacts = new OrgContactDependentCollection(Factory);
				var parent = Header;
				if (parent != null)
				{
					var filter = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					contacts = new OrgContactDependentCollection(parent, filter);
					contacts.Load();
				}
				return contacts;
			}
		}

		#endregion

		#endregion

		#region Save as Draft

		public bool IsSavingPaymentApprovalAsDraft => Factory.HasContext(BusinessContext.SavingPaymentApprovalAsDraft);

		bool IsPostDraftPaymentApproval => this.HasContext(BusinessContext.PostDraftPaymentApproval);

		public ZBool IsValidToSaveAsDraft => IsSavingPaymentApprovalAsDraft && (!IsInDatabase || IsDraft) && !HasNonDraftTransactions;

		bool HasNonDraftTransactions => MatchingBaseObject.BalancingAPJournals.Count > 0 || MatchingBaseObject.BalancingARJournals.Count > 0 || MatchingBaseObject.BankFeeCurrent != null;

		public string CheckCanSaveAsDraft()
		{
			var result = string.Empty;

			if (IsInDatabase && !IsDraft)
			{
				result = Res.GetString("516a6691-6267-41a2-be3d-9280a4f7c3b9", "{0} can not Save as Draft since status is {1}", GetDescription(), Lookups.AV_StatusList.GetDescriptionFromCode(AV_Status));
			}
			else if (HasNonDraftTransactions)
			{
				result = Res.GetString("78f043b5-57bf-48a2-99ee-f9de2831700b", "Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.");
			}

			return result;
		}

		public virtual void UpdateDraftStatus()
		{
			if (!IsInDatabase && !HasNonDraftTransactions && IsSavingPaymentApprovalAsDraft)
			{
				AV_Status = PaymentApprovalStatus.Draft;
			}
		}

		#endregion

		#region GUIBindableProperties

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal MatchedTransactionsBalanceWithPaymentAmount
		{
			get
			{
				return ((PaymentApprovalMatchingBase)MatchingBaseObject).Balance;
			}
		}

		public ZPropertyInfo MatchedTransactionsBalanceWithPaymentAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(MatchedTransactionsBalanceWithPaymentAmount));
			}
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal PaymentItemsTotalAmount
		{
			get
			{
				return MatchingBaseObject.MatchedTransactions.Balance;
			}
		}

		public ZPropertyInfo PaymentItemsTotalAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(PaymentItemsTotalAmount));
			}
		}

		[DecimalPlaces(nameof(RXDecimals))]
		public ZDecimal OSOverpaymentAmount
		{
			get
			{
				return MatchingBaseObject.OSOverpaymentAmount;
			}
		}

		public ZPropertyInfo OSOverpaymentAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(OSOverpaymentAmount));
			}
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal DiscountAmount
		{
			get
			{
				return MatchingBaseObject.DiscountAmount;
			}
		}

		public ZPropertyInfo DiscountAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DiscountAmount));
			}
		}

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal ExchangeDifferenceAmount
		{
			get
			{
				return MatchingBaseObject.ExchangeDifferenceAmount;
			}
		}

		public ZPropertyInfo ExchangeDifferenceAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ExchangeDifferenceAmount));
			}
		}

		#endregion

		public ZBool IsCancelledOrIsPosted => IsCancelled || IsPosted;

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (IsEditOrViewPaymentBatch && IsCancelledOrIsPosted)
			{
				result = true;
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public virtual void SetExchangeRate(ZDecimal exchangeRate)
		{
			if (!PostPaymentWithMatchingSetExchangeSuspender.IsSuspended && !IsCancelledOrIsPosted)
			{
				AV_PayExRate = exchangeRate;
			}
		}

		#region AV_Calc_Sequence

		public bool IsDiscrepancyWithPaymentBatch
		{
			get
			{
				return PaymentBatch != null
				&& (PaymentBatch.APB_PaymentType != AV_PaymentType
				|| PaymentBatch.APB_AB != AV_AB
				|| PaymentBatch.APB_AK != AV_AK
				|| PaymentBatch.APB_PaymentDate != AV_PaymentDate);
			}
		}

		public ZInt AV_Calc_Sequence
		{
			get
			{
				if (IsDiscrepancyWithPaymentBatch)
				{
					return 0;
				}
				else if (IsPosted)
				{
					return 1;
				}
				else if (IsCancelled)
				{
					return 2;
				}
				else
				{
					return 3;
				}
			}
		}

		public ZPropertyInfo AV_Calc_SequenceInfo => GetZPropertyInfo(nameof(AV_Calc_Sequence));

		#endregion

		#region Payment Batch

		public virtual void InitializeForPaymentBatch(Func<bool> shouldPost = null)
		{
			IsLoadedFromPaymentBatch = true;
		}

		public bool IsLoadedFromPaymentBatch { get; protected set; }

		public bool IsEditOrViewPaymentBatch => PaymentBatch != null && PaymentBatch.IsInDatabase && IsLoadedFromPaymentBatch;

		public void AV_PayExRateChanged_ForPaymentBatch(object sender, EventArgs e)
		{
			if (AV_PayExRate == 0)
			{
				AV_Amount = 0;
			}
			else
			{
				var amountToUseForLocal = AV_Calc_LocalCachedAmount;
				if (OldCurrencyNK != AV_RX_NKPaymentCurrency || AV_Amount == 0)
				{
					amountToUseForLocal = -PaymentItemsTotalAmount;
					AV_Amount = 0;
				}
				if (!StopRecalculatingAV_AmountWhenExRateChanged.IsSuspended && !(MatchingBaseObject.IsAllPaidInTheSameCurrency(AV_RX_NKPaymentCurrency)))
				{
					AV_Amount = Env.CurrentCompany.ExchangeRate.LocalToForeign(amountToUseForLocal, AV_PayExRate, AV_RX_NKPaymentCurrency);
				}
			}
			OldCurrencyNK = AV_RX_NKPaymentCurrency;
		}

		ZString OldCurrencyNK = Env.CurrentCompany.LocalCurrency.Code;

		public APPaymentBatchPoster ParentPaymentBatch
		{
			get
			{
				if (parentPaymentBatch == null && !AV_APB_PaymentBatch.IsEmpty)
				{
					parentPaymentBatch = Factory.Load<APPaymentBatchPoster>(AV_APB_PaymentBatch);
				}
				return parentPaymentBatch;
			}
		}

		APPaymentBatchPoster parentPaymentBatch;

		#endregion
	}
}
