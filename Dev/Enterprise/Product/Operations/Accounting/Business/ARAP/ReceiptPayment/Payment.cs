using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public enum ChequeNumberStatus
	{
		CancelledPayment,
		LessThanCurrentNum,
		GreaterThanOrEqualToCurrentNum,
		NoChequeNumber
	}

	[UniversalDataContext(DataContextType.AccountingPayment)]
	public abstract partial class Payment : ReceiptPaymentBase, IPayment, IDocumentSupportable, IDirectDebitBatchTransaction, IChequeNumberAutoAllocation, ICanUpdateChequeNumber
	{
		public Payment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region AH_ReceiptType

		[List("PaymentMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				base.AH_ReceiptType = value;
				ChequeBookInfo.RefreshBinding();
				ResetAccountDetails();
			}
		}

		#endregion

		CodeDescriptionPairList fPaymentMethods;
		public override CodeDescriptionPairList PaymentMethods
		{
			get
			{
				fPaymentMethods = base.PaymentMethods;
				if (IsInDatabase)
				{
					fPaymentMethods.AddPair(ReceiptTypes.DirectDebitLine, Res.GetString("acd4abb3-706c-40eb-ba03-979d0be6cc27", "Direct Debit Line (rolled up into DDR Batch on Bank Rec.)"));
				}
				return fPaymentMethods;
			}
		}

		#region AH_AB

		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				if (base.AH_AB != value)
				{
					ResetChequeBookCollection();
				}
				base.AH_AB = value;
				if (BankAccount != null)
				{
					ChequeBook = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				base.AH_OHCore = value;
				ResetAccountDetails();
			}
		}

		#endregion

		#region ChequeBookIsVisible

		public ZBool ChequeBookIsVisible
		{
			get { return ChequeBook.IsValid; }
		}

		#endregion

		#region RelatedPaymentApproval

		public PaymentApprovalBase RelatedPaymentApproval
		{
			get
			{
				if (fPaymentApproval == null)
				{
					fPaymentApproval = Factory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.AV_AH, this.PK));
				}
				return fPaymentApproval;
			}
			set
			{
				fPaymentApproval = value;
			}
		}

		PaymentApprovalBase fPaymentApproval;

		public EPaymentQuoteForDisplayCollection PaymentQuotes => RelatedPaymentApproval?.PaymentQuotes_ForDisplay ?? (PaymentQuotesDummy ?? (PaymentQuotesDummy = new EPaymentQuoteForDisplayCollection()));
		EPaymentQuoteForDisplayCollection PaymentQuotesDummy;

		public ZString DealProvider => RelatedPaymentApproval?.CurrentDeal?.AED_ProviderCode ?? ZString.Empty;

		public ZString DealProviderReference => RelatedPaymentApproval?.CurrentDeal?.AED_ProviderReference ?? ZString.Empty;

		public ZString DealStatus => RelatedPaymentApproval?.CurrentDeal?.AED_Status ?? ZString.Empty;
		public ZString DealStatusDescription
		{
			get
			{
				ZString description = $"{DealStatus} - {PaymentApprovalLookups.EPaymentStatusList.GetDescriptionFromCode(DealStatus)}";
				return string.IsNullOrEmpty(DealStatus) ? ZString.Empty : description;
			}
		}

		public ZString DealErrorMessage => RelatedPaymentApproval?.CurrentDeal?.AED_ErrorDescription ?? ZString.Empty;

		public ZDateTime DealSubmittedUtc => RelatedPaymentApproval?.CurrentDeal?.AED_SystemCreateTimeUtc ?? ZDateTime.Empty;

		public ZDateTime DealSubmittedLocalTime => RelatedPaymentApproval?.CurrentDeal?.AED_SystemCreateTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;

		public ZDateTime DealLastResponseUtc => RelatedPaymentApproval?.CurrentDeal?.AED_LastResponseReceivedUtc ?? ZDateTime.Empty;

		public ZDateTime DealLastResponseLocalTime => RelatedPaymentApproval?.CurrentDeal?.AED_LastResponseReceivedUtc.ToLocalBranchTime() ?? ZDateTime.Empty;

		[DecimalPlaces(nameof(DealTotalCostCurrencyDecimals))]
		public ZDecimal DealTotalCost => RelatedPaymentApproval?.CurrentDealQuote == null ? 0 : (RelatedPaymentApproval.CurrentDealQuote.QU_FromAmount + RelatedPaymentApproval.CurrentDealQuote.QU_FeeAmount);

		[List("Lookups.TransactionCurrencies")]
		public ZString DealTotalCostCurrencyCode => RelatedPaymentApproval?.CurrentDealQuote?.QU_RX_NKFromCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		protected int DealTotalCostCurrencyDecimals => RelatedPaymentApproval?.CurrentDealQuote?.FromRXDecimals ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		[List("PaymentReasons")]
		public ZString EPaymentReason => RelatedPaymentApproval?.AV_EPaymentReasonCode ?? ZString.Empty;

		[List("Lookups.TransactionCurrencies")]
		public ZString FundingCurrency => EPaymentFundingInfoProviderFactory.CreateProvider(RelatedPaymentApproval).GetFundingCurrency();

		[List(nameof(BankAccounts))]
		public ZGuid FundingBankAccountPK => EPaymentFundingInfoProviderFactory.CreateProvider(RelatedPaymentApproval).GetFundingBankAccount();

		#region PaymentReasons

		public CodeDescriptionPairList PaymentReasons
		{
			get
			{
				var providerCode = BankAccount?.AB_PaymentProvider ?? ZString.Empty;
				return EPaymentPropertyHelper.GetPaymentReasonsForProvider(AH_GC.ToGuid(), providerCode);
			}
		}

		#endregion

		[DecimalPlaces(nameof(DealTotalCostCurrencyDecimals))]
		public ZDecimal DealTotalFees => RelatedPaymentApproval?.CurrentDealQuote == null ? 0 : RelatedPaymentApproval.CurrentDealQuote.QU_FeeAmount;

		public ZBool HasAcceptedEPaymentDeal => PaymentType == ReceiptTypes.EPayment
												&& RelatedPaymentApproval?.CurrentDeal != null
												&& RelatedPaymentApproval.CurrentDeal.AED_Status == EPaymentStatusCodes.Deal.Accepted;

		#endregion

		#region ChequeBook

		[List("ChequeBooks")]
		public virtual ZGuid ChequeBook
		{
			get
			{
				if ((!fChequeBook.IsValid) && (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque))
				{
					if (RelatedPaymentApproval != null)
					{
						AccChequeBook chequeBook = Factory.LoadTop1<AccChequeBook>(new ZQuery(AccChequeBookSchema.PK, RelatedPaymentApproval.AV_AK));
						if (chequeBook != null)
						{
							fChequeBook = chequeBook.PK;
						}
					}
					ZDecimal aH_ChequeOrReferenceAsDecimal;
					if (!fChequeBook.IsValid && AH_ChequeOrReference.IsNumbersOnlyOrEmpty && AH_ChequeOrReference != ZString.Empty && ZDecimal.TryParse(AH_ChequeOrReference, out aH_ChequeOrReferenceAsDecimal))
					{
						ChequeBooks.Load();
						foreach (AccChequeBook cb in ChequeBooks)
						{
							if (cb.AK_StartNo <= aH_ChequeOrReferenceAsDecimal && cb.AK_LastNo >= aH_ChequeOrReferenceAsDecimal)
							{
								fChequeBook = cb.PK;
							}
						}
					}
				}
				return fChequeBook;
			}
			set
			{
				SetNonPersistentPropertyValue(ChequeBookInfo, ref fChequeBook, value);
				if (ChequeBookBizO != null)
				{
					if (IsChequeNumberAutoAllocated)
					{
						AH_ChequeOrReference = ZString.Empty;
					}
					else
					{
						AH_ChequeOrReference = ChequeBookBizO.AK_CurrentNo.ToString();
					}
				}

				if (HeaderValidation != null)
				{
					HeaderValidation.ValidateChequeBook();
				}
			}
		}
		ZGuid fChequeBook;

		public ZPropertyInfo ChequeBookInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeBook)); }
		}

		protected bool ChequeBook_ReadOnly
		{
			get { return AH_ReceiptType != ReceiptTypes.Cheque; }
		}

		public override ZString AH_AKCode
		{
			get { return ChequeBookBizO == null ? ZString.Empty : ChequeBookBizO.AK_Code; }
		}

		#endregion

		#region AH_ChequeOrReference

		public override ZString AH_ChequeOrReference
		{
			get { return base.AH_ChequeOrReference; }
			set
			{
				if (AH_ReceiptType == ReceiptTypes.Cheque)
				{
					value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, value);
				}
				base.AH_ChequeOrReference = value;
			}
		}

		#endregion

		#region ChequeBookBizO

		public AccChequeBook ChequeBookBizO
		{
			get { return Factory.Load<AccChequeBook>(ChequeBook); }
		}

		#endregion

		#region Cheque Book Number Increment

		public ZBool CanIncrementChequeBookNumber
		{
			get
			{
				return ChequeBookBizO != null && GetChequeNumberStatus() == ChequeNumberStatus.GreaterThanOrEqualToCurrentNum &&
										ZDecimal.CanParseAsInteger(AH_ChequeOrReference) && !ChequeBookBizO.IsAutoPrint;
			}
		}

		#endregion

		#region DirectDebitNumber

		public override ZString DirectDebitNumber
		{
			get { return AH_ReceiptBatchNo; }
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				base.AH_RX_NKTransactionCurrency = value;
				ResetAccountDetails();
			}
		}

		#endregion

		public bool IsENettPayment
		{
			get { return AH_ReceiptType == ReceiptTypes.eNettDirectDebit; }
		}

		public bool IsEPayment
		{
			get { return AH_ReceiptType == ReceiptTypes.EPayment; }
		}

		public ZString BankAccountNumber
		{
			get { return BankAccount != null ? BankAccount.AB_AccountNum : ZString.Empty; }
		}

		#endregion

		#region Validation

		public new PaymentValidation HeaderValidation
		{
			get { return Validation as PaymentValidation; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new PaymentValidation(this);
		}

		protected override AccTransactionHeaderValidation GetNewMatchingValidation()
		{
			if (IsInDatabase)
			{
				return new MatchingValidation(this);
			}
			else
			{
				return new PaymentMatchingValidation(this);
			}
		}

		#endregion

		#region Lookups

		#region ChequeBooks

		public AccChequeBookCollection ChequeBooks
		{
			get
			{
				if (fChequeBooks == null)
				{
					ZQuery filter = new ZQuery(AccChequeBookSchema.AK_GB, GlbBranch.CurrentBranch.PK);
					if (AH_AB.IsValid)
					{
						filter.AddToFilter(AccChequeBookSchema.AK_AB, AH_AB);
					}
					fChequeBooks = new ActiveChequeBookCollection(Factory, filter);
					fChequeBooks.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("d4917218-63b9-4a91-9a80-75ef02680c24", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
				}
				return fChequeBooks;
			}
		}

		void ResetChequeBookCollection()
		{
			fChequeBooks = null;
		}

		AccChequeBookCollection fChequeBooks;

		#endregion

		#region OutstandingTransactions

		public IMatchingCollection OutstandingTransactions
		{
			get
			{
				if (fOutstandingTransactions == null)
				{
					fOutstandingTransactions = new IMatchingCollection(Factory);
				}
				return fOutstandingTransactions;
			}
		}

		IMatchingCollection fOutstandingTransactions;

		#endregion

		#region Match Transactions

		public IMatchingCollection MatchTransactions
		{
			get
			{
				if (fMatchTransactions == null)
				{
					fMatchTransactions = new IMatchingCollection(Factory);
				}
				return fMatchTransactions;
			}
		}

		IMatchingCollection fMatchTransactions;

		#endregion

		#endregion

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded) // increment the current chequebook number if required
			{
				if (CanIncrementChequeBookNumber) // increment the current number in cheque book
				{
					AccChequeBook.UpdateCurrentNumber(ChequeBookBizO.PK, ZDecimal.Parse(AH_ChequeOrReference) + 1);
				}
				fIsAutoLogged = true;
			}
		}

		#endregion

		#region Implementation

		bool fIsAutoLogged = true;

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => fIsAutoLogged
			? EnterpriseBusinessObject.AutologState.AutoLogged
			: EnterpriseBusinessObject.AutologState.NotLogged;

		protected override List<string> GetWritableProperties()
		{
			List<string> result = base.GetWritableProperties();
			if (string.IsNullOrEmpty(AH_ReceiptBatchNo))
			{
				result.Add("IncludeInTheBatch");
			}
			return result;
		}

		#region SetReceiptType

		protected override void SetReceiptType()
		{
			base.SetReceiptType();

			if (!IsInDatabase)
			{
				if (AH_ReceiptType == ReceiptTypes.Cash || AH_ReceiptType == ReceiptTypes.CreditCard)
				{
					DisableChequeBook();
				}
				else if (AH_ReceiptType == ReceiptTypes.Cheque)
				{
					ChequeBookInfo.RefreshBinding();
				}
				else if (AH_ReceiptType == ReceiptTypes.DirectDebit || AH_ReceiptType == ReceiptTypes.EFT || AH_ReceiptType == ReceiptTypes.ScheduledEFT || AH_ReceiptType == ReceiptTypes.CollectionRequest)
				{
					DisableChequeBook();
				}
			}
		}

		protected override void SetReferenceNumberCore()
		{
			base.SetReferenceNumberCore();
			if (!IsInDatabase && (AH_ReceiptType == ReceiptTypes.DirectDebit || AH_ReceiptType == ReceiptTypes.EFT || AH_ReceiptType == ReceiptTypes.ScheduledEFT || AH_ReceiptType == ReceiptTypes.CollectionRequest))
			{
				AH_ChequeOrReference = ZString.Empty;
			}
		}

		void DisableChequeBook()
		{
			ChequeBook = ZGuid.Empty;
			ChequeBookInfo.RefreshBinding();
		}

		#endregion

		#region GetChequeNumberStatus

		public ChequeNumberStatus GetChequeNumberStatus()
		{
			ChequeNumberStatus returnStatus = ChequeNumberStatus.NoChequeNumber;
			if (ChequeBook.IsValid)
			{
				if (ChequeBookBizO != null && ZDecimal.CanParseAsInteger(AH_ChequeOrReference))
				{
					var loadedChequeBook = new ReadOnlyBusinessObjectFactory().Load<AccChequeBook>(ChequeBookBizO.PK);
					if (ZDecimal.Parse(AH_ChequeOrReference) >= loadedChequeBook.AK_CurrentNo)
					{
						returnStatus = ChequeNumberStatus.GreaterThanOrEqualToCurrentNum;
					}
					else if (loadedChequeBook.BankAccount != null && loadedChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(AH_ChequeOrReference))   // allow the save but don't increment current number of cheque book
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

		#endregion

		ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBookBizO != null)
				{
					return ChequeBookBizO.IsAutoPrint;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.Payment; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.Payment; }
		}

		public new static readonly TransactionHeaderTypeDecider TypeDecider = new TransactionHeaderTypeDecider();

		#endregion

		#region Document Supporter

		public override DocumentSupporter DocumentSupporter
		{
			get { return new PaymentDocumentSupporter(this); }
		}

		#region PaymentDocumentSupporter

		public class PaymentDocumentSupporter : TransactionHeaderDocumentSupporter
		{
			public PaymentDocumentSupporter(Payment payment)
				: base(payment)
			{
			}

			protected Payment Payment
			{
				get { return (Payment)BusinessObject; }
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.APTransaction; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
				{
					return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, Payment);
				}
				else if (dataContext == Constants.DataContext.AccountingVoucher)
				{
					return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
				}
				else
				{
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.APPayment, Payment) };
				}
			}

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
			{
				if (contact == ContactType.Payables)
				{
					return new OrgHeaderContact(Payment.Header, null);
				}

				return null;
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				List<Constants.DataContext> result = new List<Constants.DataContext>(base.GetSupportedDataContexts());
				result.Add(Core.Constants.DataContext.GenericFreightJob);
				return result.ToArray();
			}

			public override IOrgContact GetAdditionalDeliveryContact()
			{
				return Payment.InvoiceContactOverride;
			}

			public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
			{
				return Payment.InvoiceAddressOverride;
			}
		}

		#endregion

		#endregion

		#region Document Manager

		public class PaymentDocManagerInfo : AccountingDocManagerInfo
		{
			public PaymentDocManagerInfo(BusinessObject parent, string docManagerCode)
			: base(parent, docManagerCode)
			{
			}

			protected override BusinessObject[] GetRelatedObjects()
			{
				var payment = BusinessEntity as Payment;
				if (payment != null && payment.RelatedPaymentApproval != null)
				{
					return new[] { payment.RelatedPaymentApproval };
				}
				else
				{
					return base.GetRelatedObjects();
				}
			}
		}

		#endregion

		#region IAutoAllocation Members

		AccChequeBook IChequeNumberAutoAllocation.ChequeBook
		{
			get { return ChequeBookBizO; }
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
			AH_ChequeOrReference = autoGeneratedChequeNumber;
			UpdateChequeNumberOnRelatedObjects(autoGeneratedChequeNumber);
		}

		protected virtual void UpdateChequeNumberOnRelatedObjects(string value)
		{
			if (RelatedPaymentApproval != null)
			{
				RelatedPaymentApproval.AV_ChequeOrReference = value;
			}
		}

		ZBool IChequeNumberAutoAllocation.IsAllocationPerformed
		{
			get
			{
				return !AH_ChequeOrReference.IsEmpty;
			}
		}

		Guid IChequeNumberAutoAllocation.Printing_ObjectPK
		{
			get
			{
				return PK.ToGuid();
			}
		}

		ZGuid IChequeNumberAutoAllocation.Printing_PrinterPK
		{
			get
			{
				return (ChequeBookBizO != null) ? ChequeBookBizO.AK_SQ : ZGuid.Empty;
			}
		}

		ZBool IChequeNumberAutoAllocation.ChequeIsAutoPrinted
		{
			get
			{
				return fChequeIsAutoPrinted;
			}
			set
			{
				fChequeIsAutoPrinted = value;
			}
		}
		ZBool fChequeIsAutoPrinted;

		void IChequeNumberAutoAllocation.AllocationOrPrintingFailed()
		{
			if (ChequeBookBizO != null)
			{
				ChequeBookBizO.Reload();
			}

			AH_ChequeOrReference = string.Empty;
			UpdateChequeNumberOnRelatedObjects(string.Empty);
		}

		#endregion

		#region IDirectDebitBatchTransaction Implementation

		public OrgHeaderCollection Headers
		{
			get { return Lookups.Headers; }
		}

		public ZString Code
		{
			get { return AH_Ledger; }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
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

		public ZPropertyInfo PayeeBankAccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankAccountNumber)); }
		}

		public ZString PayeeBankBranchName => AccountDetails?.A1_BankBranchName ?? ZString.Empty;

		public ZPropertyInfo PayeeBankBranchNameInfo => GetZPropertyInfo(nameof(PayeeBankBranchName));

		public ZString PayeeBankAddress1 => AccountDetails?.A1_BankAddress1 ?? ZString.Empty;

		public ZPropertyInfo PayeeBankAddress1Info => GetZPropertyInfo(nameof(PayeeBankAddress1));

		public ZString PayeeBankAddress2 => AccountDetails?.A1_BankAddress2 ?? ZString.Empty;

		public ZPropertyInfo PayeeBankAddress2Info => GetZPropertyInfo(nameof(PayeeBankAddress2));

		public ZString PayeeBankAddress3 => AccountDetails?.A1_BankAddress3 ?? ZString.Empty;

		public ZPropertyInfo PayeeBankAddress3Info => GetZPropertyInfo(nameof(PayeeBankAddress3));

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

		public ZPropertyInfo AccountTitleInfo
		{
			get { return GetZPropertyInfo(nameof(AccountTitle)); }
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

		public ZPropertyInfo PayeeBankBSBInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankBSB)); }
		}

		public ZString AccountCurrency
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_RX_NKAccountCurrency;
				}
				return result;
			}
		}

		public ZPropertyInfo AccountCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(AccountCurrency)); }
		}

		public ZString PayeeBankName
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_BankName;
				}
				return result;
			}
		}

		public ZPropertyInfo PayeeBankNameInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankName)); }
		}

		public ZString PayeeBankSwift
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_BankSwift;
				}
				return result;
			}
		}

		public ZPropertyInfo PayeeBankSwiftInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankSwift)); }
		}

		public ZString PayeeIBANNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_IBANNumber;
				}
				return result;
			}
		}

		public ZPropertyInfo PayeeIBANNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeIBANNumber)); }
		}

		public ZString PayeeCountryCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountDetails != null)
				{
					result = AccountDetails.A1_RN_NKCountryCode;
				}
				return result;
			}
		}

		public ZPropertyInfo PayeeCountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeCountryCode)); }
		}

		ZBool fIncludeInTheBatch;
		public ZBool IncludeInTheBatch
		{
			get { return fIncludeInTheBatch; }
			set
			{
				if (value != fIncludeInTheBatch) 
				{
					fIncludeInTheBatch = value;
					IncludeInTheBatchInfo.RefreshBinding();
					if (DDRCollection != null)
					{
						ZDecimal amountToUpdate = fIncludeInTheBatch ? AH_OSExTaxAmount : (ZDecimal)(AH_OSExTaxAmount * -1);
						ZDecimal localAmountToUpdate = fIncludeInTheBatch ? AH_LocalExTaxAmount : (ZDecimal)(AH_LocalExTaxAmount * -1);
						DDRCollection.UpdateSelectedTotal(amountToUpdate, localAmountToUpdate);
					}
				}
			}
		}

		internal void SetDDRCollection(DirectDebitBatchLineCollection dDRCollection)
		{
			fDDRCollection = dDRCollection;
		}

		DirectDebitBatchLineCollection fDDRCollection;
		public DirectDebitBatchLineCollection DDRCollection
		{
			get
			{
				if (fDDRCollection == null)
				{
					fDDRCollection = new DirectDebitBatchLineCollection(Factory);
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						DirectDebitBatchLineCollection parentDDRCollection = parentCollection as DirectDebitBatchLineCollection;
						if (parentDDRCollection != null)
						{
							fDDRCollection = parentDDRCollection;
							break;
						}
					}
				}
				return fDDRCollection;
			}
		}

		bool IDirectDebitBatchComponent.ShouldValidateDirectDebitBatchComponent
		{
			get { return (fDDRCollection != null && fDDRCollection.DDRHeader != null && fDDRCollection.DDRHeader.ShouldValidateDirectDebitBatchComponent); }
		}

		public ZPropertyInfo IncludeInTheBatchInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInTheBatch)); }
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

		public ZPropertyInfo AllowAutoDDRInfo
		{
			get { return GetZPropertyInfo(nameof(AllowAutoDDR)); }
		}

		//DirectDebitBatchHeader fBatchHeader;
		//public DirectDebitBatchHeader BatchHeader
		//{
		//    get { return fBatchHeader; }
		//    set { fBatchHeader = value; }
		//}

		public ZString BankCreateUser => AccountDetails?.A1_SystemCreateUser ?? ZString.Empty;
		public ZPropertyInfo BankCreateUserInfo => GetZPropertyInfo(nameof(BankCreateUser));

		public ZDateTime BankCreateTimeLocal => AccountDetails?.A1_SystemCreateTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo BankCreateTimeLocalInfo => GetZPropertyInfo(nameof(BankCreateTimeLocal));

		public ZString BankLastEditUser => AccountDetails?.A1_SystemLastEditUser ?? ZString.Empty;
		public ZPropertyInfo BankLastEditUserInfo => GetZPropertyInfo(nameof(BankLastEditUser));

		public ZDateTime BankLastEditTimeLocal => AccountDetails?.A1_SystemLastEditTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
		public ZPropertyInfo BankLastEditTimeLocalInfo => GetZPropertyInfo(nameof(BankLastEditTimeLocal));

		void IDirectDebitBatchTransaction.ValidateAutoDDR()
		{
			PaymentValidation paymentValidation = Validation as PaymentValidation;
			if (paymentValidation != null)
			{
				paymentValidation.ValidateAllowAutoDDR();
			}
		}

		void IDirectDebitBatchTransaction.ValidateBankBSB()
		{
			PaymentValidation paymentValidation = Validation as PaymentValidation;
			if (paymentValidation != null)
			{
				paymentValidation.ValidatePayeeBankBSB();
			}
		}

		void IDirectDebitBatchTransaction.ValidateBankAccountNumber()
		{
			PaymentValidation paymentValidation = Validation as PaymentValidation;
			if (paymentValidation != null)
			{
				paymentValidation.ValidatePayeeBankAccountNumber();
			}
		}

		public bool AccountDetailsFound
		{
			get { return AccountDetails != null; }
		}

		public AccAPAccountDetails AccountDetails
		{
			get
			{
				if (Header != null && fAccountDetails == null)
				{
					ZString currencyCode = !AH_RX_NKTransactionCurrency.IsEmpty ? AH_RX_NKTransactionCurrency : AH_Calc_LocalRXCode;
					fAccountDetails = Header.CompanyData.AccountDetailsCollection.GetAccountDetails(GetReceiptTypeForDDRAccountDetails(), currencyCode, true);
				}
				if (fAccountDetails != null && fAccountDetails.IsDeleted)
				{
					fAccountDetails = null;
				}
				return fAccountDetails;
			}
		}
		AccAPAccountDetails fAccountDetails;

		ZString GetReceiptTypeForDDRAccountDetails()
		{
			ZString result = AH_ReceiptType;
			if (result == ReceiptTypes.DirectDebitLine)
			{
				result = ReceiptTypes.DirectDebit;
			}
			return result;
		}

		public void ResetAccountDetails()
		{
			fAccountDetails = null;
		}

		#endregion

		#region ICanUpdateChequeNumber Members

		AccChequeBook ICanUpdateChequeNumber.ChequeBook
		{
			get { return ChequeBookBizO; }
		}

		public void UpdateChequeNumber(string newChequeNumber, bool withReprint)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, PaymentApprovalBulkProcessor.GetCheckNumberUpdatedMessage(AH_ChequeOrReference, newChequeNumber, withReprint));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			fIsAutoLogged = false;
			AH_ChequeOrReference = newChequeNumber;
			UpdateChequeNumberOnRelatedObjects(newChequeNumber);
		}

		#endregion

		protected override ZGuid DefaultOrgAddressPK
		{
			get
			{
				OrgAddress defaultAddress = null;
				if (Header != null)
				{
					defaultAddress = Header.AddressForSendingAPDocuments;
				}

				return defaultAddress == null ? ZGuid.Empty : defaultAddress.PK;
			}
		}

		public ZAddressWithContact OrganisationAddressWithContact
		{
			get
			{
				if (fOrganisationAddressWithContact == null)
				{
					fOrganisationAddressWithContact = new ZAddressWithContact(DisplayInvoiceContactOverrideInfo, DisplayInvoiceAddressOverrideInfo);
				}
				return fOrganisationAddressWithContact;
			}
		}
		ZAddressWithContact fOrganisationAddressWithContact;

		#region Test
#if DEBUG

		protected void ResetRelatedPaymentApproval()
		{
			fPaymentApproval = null;
		}

#endif
		#endregion
	}
}
