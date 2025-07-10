using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Env = Enterprise.Environment.Env;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	[CodeProperty(AccPaymentBatchSchema.Constants.APB_BatchNumber)]
	[DescriptionProperty("HumanReadableName")]
	public partial class APPaymentBatchPoster : AccPaymentBatch
	{
		public static APPaymentBatchPoster Create(BusinessObjectFactory factory, bool groupByInvoicePaymentCriticality, bool groupByInvoiceRelatedDebtorOrganisation, bool groupByUser)
		{
			var poster = factory.New<APPaymentBatchPoster>();
			poster.groupByInvoicePaymentCriticality = groupByInvoicePaymentCriticality;
			poster.groupByInvoiceRelatedDebtorOrganisation = groupByInvoiceRelatedDebtorOrganisation;
			poster.groupByUser = groupByUser;
			return poster;
		}

		public APPaymentBatchPoster(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			groupByInvoicePaymentCriticality = false;
			groupByInvoiceRelatedDebtorOrganisation = false;
			groupByUser = false;
		}

		#region Events

		#region OnSelectBankAccountFromDefault

		public delegate ZGuid DefaultBankSelectionEventHandler(AccBankAccountCollection bankAccountCollection);
		public event DefaultBankSelectionEventHandler OnSelectBankAccountFromDefault;

		ZGuid SelectBankAccountFromDefault(AccBankAccountCollection bankAccountCollection)
		{
			if (OnSelectBankAccountFromDefault != null)
			{
				return OnSelectBankAccountFromDefault(bankAccountCollection);
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		#endregion

		#region OnPaymentDeletedFromBatch

		public event EventHandler OnPaymentDeletedFromBatch;

		void RaiseOnPaymentDeletedFromBatch()
		{
			if (OnPaymentDeletedFromBatch != null)
			{
				OnPaymentDeletedFromBatch(null, null);
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (IsInDatabase && !IsFundingCurrencyChanged)
			{
				CheckIfModifiedPaymentApprovalsHaveConcurrencyChanges();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			shouldRefreshQuotesSummary = false;

			if (IsInDatabase && IsFundingCurrencyChanged)
			{
				CheckIfPaymentApprovalsHaveConcurrencyChanges();
				CheckAndReportNewDealInDB();

				foreach (var approval in PaymentApprovalCollectionWithoutCancelledOrPosted)
				{
					var activeQuotes = approval.PaymentQuotes.Where(x => EPaymentStatusCodes.Quote.ActiveStatusCodes.Contains(x.QU_Status.ToString()));
					shouldRefreshQuotesSummary = activeQuotes.Any();
					activeQuotes.ForEach(x => x.QU_Status = EPaymentStatusCodes.Quote.Discarded);
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && shouldRefreshQuotesSummary)
			{
				EPaymentQuotesSummary.ReloadSummaries(false);
			}
		}

		bool shouldRefreshQuotesSummary;

		#region Concurrency Check

		void CheckIfPaymentApprovalsHaveConcurrencyChanges()
		{
			PaymentApprovalConcurrencyHelper.BatchCheckAndReportNewQuoteInDB(PaymentApprovalCollection.Cast<PaymentApprovalBase>());
		}

		void CheckIfModifiedPaymentApprovalsHaveConcurrencyChanges()
		{
			var paymentApprovalsWithChanges = PaymentApprovalCollection.Cast<PaymentApprovalBase>().Where(x => x.HasChanges);
			PaymentApprovalConcurrencyHelper.BatchCheckAndReportNewQuoteInDB(paymentApprovalsWithChanges);
		}

		void CheckAndReportNewDealInDB()
		{
			var tempFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(AccEPaymentDeal));
			var quoteSubQuery = new ZDBOnlySubQuery(typeof(AccEPaymentQuote), AccEPaymentQuoteSchema.PK);
			quoteSubQuery.AddToFilter(AccEPaymentQuoteSchema.QU_AV, PaymentApprovalCollectionWithoutCancelledOrPosted.Select(x => x.PK));
			query.AddSubQuery(AccEPaymentDealSchema.AED_QU_Quote, quoteSubQuery, JoinCondition.And);
			query.AddToFilter(AccEPaymentDealSchema.AED_Status, SQLComparisonOperator.Contains, EPaymentStatusCodes.Deal.ActiveStatusCodes);

			if (tempFactory.Exists(typeof(AccEPaymentDeal), query, true))
			{
				throw new ZCannotSaveException(Res.GetString("016BB563-CDC8-49A7-A0D3-473638B47B99", "Another user created an active E-Payment Deal for this payment batch. Change of Funding Currency is not permitted."),
					Res.GetString("2097a2f7-3676-47b6-ae38-3e3b0de51f4d", "Report Error"));
			}
		}

		#endregion

		#endregion

		#region Public Methods

		public void SetDefaultValuesByTransactions(TransactionHeaderCollection unsortedTransactionCollection)
		{
			if (InitializeTransactionCollection(unsortedTransactionCollection))
			{
				using (SuspendSettingHasChanges())
				{
					SplitTransactionsToPaymentBatch();
					SetupChildObjects();
				}
				SetDefaultValues();
			}
		}

		public void CheckForDefaultBankAccounts()
		{
			if (DefaultBankAccounts.Count == 1)
			{
				APB_AB = DefaultBankAccounts[0].PK;
			}
			else if (DefaultBankAccounts.Count > 1)
			{
				if (Globals.Message.Show((NoResString)"There is more than one Default Bank Account found for these SettlementGroups. " + System.Environment.NewLine + (NoResString)"Would you like to choose one?", (NoResString)"Default Bank Account", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) == ZDialogResult.Yes) // This is spcial cause, because it is Batchoster and it is old code
				{
					APB_AB = SelectBankAccountFromDefault(DefaultBankAccounts);
				}
			}
		}

		public void SetPaymentDetails(PaymentApprovalBase payment)
		{
			using (SuspendSettingHasChanges())
			{
				PaymentForBinding = payment;
			}
		}

		public void ResetPaymentMatchingCollectionFromCurrentPayment()
		{
			ResetPaymentMatchingCollection(MatchedTransactions);
		}

		void ResetPaymentMatchingCollection(IMatchingCollection transactionCollection)
		{
			MatchingCollection.RemoveAll();
			if (transactionCollection != null)
			{
				MatchingCollection.AddRange(transactionCollection);
			}
			RefreshBalance();
		}

		new public void RefreshBinding()
		{
			base.RefreshBinding();
			APB_AKInfo.RefreshBinding();
			APB_PaymentTypeInfo.RefreshBinding();
			APB_ABInfo.RefreshBinding();
			APB_ChequeOrReferenceInfo.RefreshBinding();
			APB_PaymentDateInfo.RefreshBinding();
			APB_PostDateInfo.RefreshBinding();
			PaymentApprovalCollection.RefreshBinding();
			MatchingCollection.RefreshBinding();
			PaymentItemsTotalAmountInfo.RefreshBinding();
			CreditorInfo.RefreshBinding();
			OSOverpaymentAmountInfo.RefreshBinding();
			DiscountAmountInfo.RefreshBinding();
			ExchangeDifferenceAmountInfo.RefreshBinding();
			TotalEPaymentCostAmountInfo.RefreshBinding();
			TotalEPaymentFeeAmountInfo.RefreshBinding();
			CurrencySummary.TransactionsLocalAmountTotalInfo.RefreshBinding();
			if (MatchingBaseObject is APPaymentApprovalMatching approvalMatching && approvalMatching != null)
			{
				approvalMatching.ValidateBalance();
			}
			RefreshBalance();
		}

		public void RemoveTransactionFromPayment(TransactionHeader transaction)
		{
			if (MatchedTransactions != null && MatchedTransactions.Contains(transaction))
			{
				MatchedTransactions.Remove(transaction);

				ResetPaymentMatchingCollection(MatchedTransactions);

				if (!MatchingCollection.Any())
				{
					RemovePaymentFromBatch(PaymentForBinding);
				}

				MatchingCollection.RefreshBinding();
			}
		}

		public ZBool ISPaymentBatchEmpty
		{
			get { return PaymentApprovalCollection.Count == 0; }
		}

		public void RemovePaymentFromBatch(PaymentApprovalBase payment)
		{
			if (payment != null && PaymentApprovalCollection.Contains(payment))
			{
				payment.MatchingBaseObject.DeleteCachedMiscTransactions();
				ResetPaymentMatchingCollection(null);
				UnBindMethodsToPayment(payment);

				if (payment.IsInDatabase)
				{
					PaymentApprovalCollection.Remove(payment);
					payment.CancelChanges();
					payment.AV_APB_PaymentBatch = ZGuid.Empty;
					HasChanges = true;
				}
				else
				{
					PaymentApprovalCollection.Remove(payment);
					payment.AV_APB_PaymentBatch = ZGuid.Empty;
					payment.Delete();
				}

				PaymentApprovalCollection.RefreshBinding();
				RaiseOnPaymentDeletedFromBatch();
			}
		}

		public void MatchTransactions()
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.UnRegisterEditableChildObject(payment.MatchingBaseObject);//removing this row will cause the stack overflow
				payment.MatchingBaseObject.MatchedTransactions.AddTransactionThatMustBeMatched(payment);
				payment.MatchingBaseObject.MatchAndClearTransactions();
			}
		}

		public void UpdatePaymentApprovalStatus()
		{
			if (!IsInDatabase && !IsSavingPaymentBatchAsDraft)
			{
				if (PostPaymentsAsPaymentApprovals)
				{
					foreach (var payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
					{
						var paymentApproval = Factory.Load<APPaymentApprovalWithAuthorisation>(payment.PK);
						var approvalStatus = paymentApproval.GetApprovalStatus();
						payment.AV_Status = approvalStatus;
					}
				}
				else
				{
					foreach (var payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
					{
						payment.AV_Status = PaymentApprovalStatus.FullyApproved;
					}
				}
			}
		}

		public bool IsSavingPaymentBatchAsDraft => Factory.HasContext(BusinessContext.SavingPaymentApprovalAsDraft);

		public void SetReadOnly()
		{
			PaymentApprovalCollection.SetReadOnlyIncludingChildren(true);
			SetReadOnlyIncludingChildren(true);
		}

		public void AllocationOrPrintingFailed()
		{
			if (ChequeBook != null)
			{
				ChequeBook.Reload();
			}
			APB_ChequeOrReference = ZString.Empty;
		}

		#endregion

		#region Properties

		bool groupByInvoicePaymentCriticality { get; set; }
		bool groupByInvoiceRelatedDebtorOrganisation { get; set; }
		bool groupByUser { get; set; }

		public EPaymentQuoteCollection FilteredEPaymentQuotes
		{
			get
			{
				if (filteredEPaymentQuotes == null)
				{
					filteredEPaymentQuotes = new EPaymentQuoteCollection(this);
					filteredEPaymentQuotes.Load();
				}
				return filteredEPaymentQuotes;
			}
		}
		EPaymentQuoteCollection filteredEPaymentQuotes;

		public APPaymentBatchPosterEPaymentQuoteFilterBusinessObject EPaymentQuotesFilterObject => ePaymentQuotesFilterObject ?? (ePaymentQuotesFilterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject());
		APPaymentBatchPosterEPaymentQuoteFilterBusinessObject ePaymentQuotesFilterObject;

		public EPaymentQuoteSummaryCollection EPaymentQuotesSummary => ePaymentQuotesSummary ?? (ePaymentQuotesSummary = new EPaymentQuoteSummaryCollection(this));
		EPaymentQuoteSummaryCollection ePaymentQuotesSummary;

		public bool IsFundingCurrencyChanged => FundingBankAccountCurrency != OriginalFundingBankAccountCurrency;

		#region PaymentType

		[MaxLength(AccPaymentApproval.Schema.AV_PaymentTypeMaxLength)]
		public override ZString APB_PaymentType
		{
			get
			{
				return base.APB_PaymentType;
			}
			set
			{
				if (base.APB_PaymentType != value)
				{
					base.APB_PaymentType = value;

					if (!IsCheque)
					{
						APB_AK = ZGuid.Empty;
					}

					if (APB_PaymentType != ReceiptTypes.EPayment)
					{
						APB_AB_FundingBankAccount = ZGuid.Empty;
					}

					var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(value);
					APB_ChequeOrReference = !defaultReferenceNumber.IsEmpty ? defaultReferenceNumber : APB_PaymentType == ReceiptTypes.Cash ? (ZString)ZArchitecture.Core.ReceiptTypes.Cash : ZString.Empty;
					UpdatePaymentTypeForAllPayments(value);

					CardSecurityCode = ZString.Empty;

					if (!IsValidationSuspended)
					{
						Validation.ValidateAPB_AB();
						Validation.ValidateCardSecurityCode();
					}
					APB_AKInfo.RefreshBinding();
					Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
					Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
				}
			}
		}

		void UpdatePaymentTypeForAllPayments(ZString value)
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.AV_PaymentType = value;
			}
		}

		#endregion

		#region PostDate

		public override ZDateTime APB_PostDate
		{
			get
			{
				return base.APB_PostDate;
			}
			set
			{
				base.APB_PostDate = value;

				UpdatePostDateForAllPayments(value);
			}
		}
		
		void UpdatePostDateForAllPayments(ZDateTime value)
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.AV_PostDate = value;
			}
		}

		#endregion

		#region Date

		public override ZDateTime APB_PaymentDate
		{
			get
			{
				return base.APB_PaymentDate;
			}
			set
			{
				base.APB_PaymentDate = value;

				UpdateDateForAllPayments(value);
			}
		}

		void UpdateDateForAllPayments(ZDateTime value)
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.AV_PaymentDate = value;
			}
		}

		#endregion

		#region APB_AB

		[List("BankAccounts")]
		public override ZGuid APB_AB
		{
			get
			{
				return base.APB_AB;
			}
			set
			{
				if (base.APB_AB != value)
				{
					base.APB_AB = value;

					if (ChequeBook == null || ChequeBook.AK_AB != value)
					{
						APB_AK = ZGuid.Empty;
					}

					ResetChequeBookCollection();
					UpdateBankAccountPKForAllPayments(base.APB_AB);
					UpdateCurrencyForAllPayments();
					UpdatePaymentAmountsForSingleForeignCurrencyPayments();
				}
			}
		}

		void UpdatePaymentAmountsForSingleForeignCurrencyPayments()
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				if (payment.CurrencyCode != LocalCurrency && payment.MatchingBaseObject.IsAllPaidInTheSameCurrency(payment.AV_RX_NKPaymentCurrency))
				{
					payment.AV_Amount = -payment.MatchingBaseObject.CalculateOSBalanceExcludingPaymentReceiptInSpecificCurrencyOnly(payment.AV_RX_NKPaymentCurrency, payment.PK);
				}
			}
		}

		void UpdateBankAccountPKForAllPayments(ZGuid value)
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.AV_AB = value;
			}
		}

		void UpdateCurrencyForAllPayments()
		{
			var bankAccountCurrency = base.BankAccount?.AB_RX_NKAccountCurrency ?? LocalCurrency;

			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				var currency = LocalCurrency;
				if (bankAccountCurrency == LocalCurrency)
				{
					var currencies = payment.MatchingBaseObject.MatchedTransactions.Cast<IMatching>().Select(x => x.CurrencyCode).Distinct().ToList();
					if (currencies.Count == 1)
					{
						currency = currencies[0];
					}
				}
				else
				{
					currency = bankAccountCurrency;
				}
				payment.AV_RX_NKPaymentCurrency = currency;
			}
		}

		#endregion

		#region APB_AK

		[List("ChequeBooks")]
		public override ZGuid APB_AK
		{
			get
			{
				return base.APB_AK;
			}
			set
			{
				base.APB_AK = value;

				UpdateChequeBookForAllPayments(value);
				if (IsCheque && !IsChequeNumberAutoAllocated)
				{
					APB_ChequeOrReference = (ChequeBook != null) ? ChequeBook.AK_CurrentNo.ToString() : "";
				}
				CheckIsAutoPrinted();
			}
		}

		protected bool APB_AK_ReadOnly
		{
			get { return !IsCheque; }
		}

		void UpdateChequeBookForAllPayments(ZGuid value)
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
			{
				payment.AV_AK = value;
			}
		}

		#endregion

		#region FundingBankAccount

		[List("BankAccounts")]
		public override ZGuid APB_AB_FundingBankAccount
		{
			get
			{
				return base.APB_AB_FundingBankAccount;
			}
			set
			{
				base.APB_AB_FundingBankAccount = value;
				FundingBankAccountCurrencyInfo.RefreshBinding();
			}
		}

		[List("Currencies")]
		public ZString FundingBankAccountCurrency => EPaymentFundingInfoProviderFactory.CreateProvider(this).GetFundingCurrency();

		public ZPropertyInfo FundingBankAccountCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(FundingBankAccountCurrency)); }
		}

		public ZString OriginalFundingBankAccountCurrency => EPaymentFundingInfoProviderFactory.CreateProvider(this).GetOriginalFundingCurrency();

		#endregion

		#region ChequeOrReference

		[MaxLength(20)]
		public override ZString APB_ChequeOrReference
		{
			get
			{
				return base.APB_ChequeOrReference;
			}
			set
			{
				if (IsSettingDefaultValues || AskUserIfNeededBeforeChequeOrReferenceUpdate())
				{
					base.APB_ChequeOrReference = (IsCheque && ZDecimal.CanParseAsInteger(value))
						? AccValidationHelper.PadChequeDigitsWithLeadingZeros(base.BankAccount, value)
						: value;

					UpdateChequeOrReferenceForAllPayments(base.APB_ChequeOrReference);

					foreach (PaymentApprovalBase payment in PaymentApprovalCollection)
					{
						payment.Validation.ValidateAV_ChequeOrReference();
					}
				}
			}
		}

		ZBool AskUserIfNeededBeforeChequeOrReferenceUpdate()
		{
			if (PaymentApprovalCollectionWithoutCancelledOrPosted.Count() > 1)
			{
				ZString currentValue = ZString.Empty;
				foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
				{
					var hasDifference = currentValue != ZString.Empty && payment.AV_ChequeOrReference != ZString.Empty && currentValue != payment.AV_ChequeOrReference;
					if (payment.IsCheque && hasDifference &&
						ZDecimal.TryParse(payment.AV_ChequeOrReference, out ZDecimal paymentChequeOrReferenceAsDecimal) &&
						ZDecimal.TryParse(currentValue, out ZDecimal currentValueAsDecimal) &&
						(Math.Abs(paymentChequeOrReferenceAsDecimal - currentValueAsDecimal) > 1 || paymentChequeOrReferenceAsDecimal < currentValueAsDecimal))
					{
						return Globals.Message.Show((NoResString)"Are you sure you want to recalculate the Check number for all payments using the new Start Reference No?", (NoResString)"Check Number", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) == ZDialogResult.Yes; // This is spcial cause, because it is Batchoster and it is old code
					}
					else if (!payment.IsCheque && hasDifference)
					{
						return Globals.Message.Show((NoResString)"Are you sure you want to reset the Reference number for all payments using the new Start Reference No?", (NoResString)"Reference Number", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) == ZDialogResult.Yes; // This is spcial cause, because it is Batchoster and it is old code
					}
					else
					{
						currentValue = payment.AV_ChequeOrReference;
					}
				}
			}
			return ZBool.True;
		}

		public bool APB_ChequeOrReference_ReadOnly
		{
			get { return IsChequeNumberAutoAllocated && IsCheque; }
		}

		void UpdateChequeOrReferenceForAllPayments(ZString value)
		{
			if (IsCheque && ZDecimal.CanParseAsInteger(value))
			{
				var paymentApprovalList = PaymentApprovalCollectionWithoutCancelledOrPosted.ToList();
				for (int i = 0; i < paymentApprovalList.Count; i++)
				{
					paymentApprovalList[i].AV_ChequeOrReference = (ZDecimal.Parse(value) + (ZDecimal)i).ToString(CultureInfo.InvariantCulture);
				}
			}
			else
			{
				foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
				{
					payment.AV_ChequeOrReference = value;
				}
			}
		}

		#endregion

		#region LocalCurrency

		[List("Currencies")]
		public ZString LocalCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrency)); }
		}

		#endregion

		#region ForeignCurrency

		[List("Currencies")]
		public ZString ForeignCurrency
		{
			get
			{
				if (MatchingBaseObject != null)
				{
					return MatchingBaseObject.ForeignCurrency;
				}
				else
				{
					return LocalCurrency;
				}
			}
		}

		public ZPropertyInfo ForeignCurrencyInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ForeignCurrency));
			}
		}

		#endregion

		#region Payment Approval Collection

		[ChildEditable]
		public override PaymentApprovalBaseCollection PaymentApprovalCollection
		{
			get
			{
				if (paymentApprovalCollection == null)
				{
					if (IsInDatabase)
					{
						paymentApprovalCollection = new APPaymentApprovalWithAuthorisationCollection(Factory);
					}
					else
					{
						paymentApprovalCollection = new APPaymentApprovalWithoutAuthorisationCollection(Factory);
					}

					paymentApprovalCollection.Load(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, PK));
					paymentApprovalCollection.ForEach(x => ((PaymentApprovalBase)x).InitializeForPaymentBatch());
				}

				return paymentApprovalCollection;
			}
		}
		PaymentApprovalBaseCollection paymentApprovalCollection;

		public void RefreshPaymentApprovalCollectionCurrentDeals()
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollection)
			{
				payment.RefreshCurrentDeal();
			}
			PaymentApprovalCollection.RefreshBinding();
		}

		public void ResetPaymentApprovalCollectionAccountDetails()
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollection)
			{
				payment.ResetAccountDetails();
			}

			PaymentApprovalCollection.RefreshBinding();
		}

		public void ReloadPaymentApprovalCollectionEPaymentBeneficiaries()
		{
			foreach (PaymentApprovalBase payment in PaymentApprovalCollection)
			{
				payment.EPaymentBeneficiary?.ReloadSafe();
			}

			PaymentApprovalCollection.RefreshBinding();
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalEPaymentCostAmount => PaymentApprovalCollection.Cast<PaymentApprovalBase>().Sum(p => p.DealTotalCost);

		public ZPropertyInfo TotalEPaymentCostAmountInfo => GetZPropertyInfo(nameof(TotalEPaymentCostAmount));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalEPaymentFeeAmount => PaymentApprovalCollection.Cast<PaymentApprovalBase>().Sum(p => p.DealTotalFees);

		public ZPropertyInfo TotalEPaymentFeeAmountInfo => GetZPropertyInfo(nameof(TotalEPaymentFeeAmount));

		public PaymentApprovalBase FirstApproval => PaymentApprovalCollection.Cast<PaymentApprovalBase>().FirstOrDefault();

		public IEnumerable<PaymentApprovalBase> PaymentApprovalCollectionWithoutCancelledOrPosted
		{
			get
			{
				return PaymentApprovalCollection.OfType<PaymentApprovalBase>().Where(x => !x.IsCancelledOrIsPosted);
			}
		}

		#endregion

		#region MatchingCollection

		public IMatchingCollection MatchingCollection
		{
			get
			{
				if (fMatchingCollection == null)
				{
					fMatchingCollection = new IMatchingCollection(Factory);
				}
				return fMatchingCollection;
			}
		}
		IMatchingCollection fMatchingCollection;

		#endregion

		#region PaymentItemsTotalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal PaymentItemsTotalAmount
		{
			get
			{
				if (PaymentForBinding != null)
				{
					return PaymentForBinding.PaymentItemsTotalAmount;
				}
				else
				{
					return ZDecimal.Zero;
				}
			}
		}

		public ZPropertyInfo PaymentItemsTotalAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(PaymentItemsTotalAmount));
			}
		}

		#endregion

		#region Balance

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal Balance
		{
			get
			{
				return fBalance;
			}
			set
			{
				fBalance = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateBalance();
				}
				BalanceInfo.RefreshBinding();
			}
		}
		ZDecimal fBalance;

		public ZPropertyInfo BalanceInfo
		{
			get { return GetZPropertyInfo(nameof(Balance)); }
		}

		protected bool Balance_ReadOnly
		{
			get { return true; }
		}

		void RefreshBalance()
		{
			Balance = PaymentForBinding?.MatchedTransactionsBalanceWithPaymentAmount ?? 0;
		}

		#endregion

		#region Creditor

		[ReadOnly(true)]
		[List("Headers")]
		public ZGuid Creditor
		{
			get
			{
				return (PaymentForBinding != null) ? PaymentForBinding.AV_OH : ZGuid.Empty;
			}
		}

		public ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(nameof(Creditor)); }
		}

		#endregion

		#region OSOverpaymentAmount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OSOverpaymentAmount
		{
			get
			{
				if (PaymentForBinding != null)
				{
					return PaymentForBinding.OSOverpaymentAmount;
				}
				else
				{
					return ZDecimal.Zero;
				}
			}
		}

		public ZPropertyInfo OSOverpaymentAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(OSOverpaymentAmount));
			}
		}

		#endregion

		#region DiscountAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DiscountAmount
		{
			get
			{
				if (PaymentForBinding != null)
				{
					return PaymentForBinding.DiscountAmount;
				}
				else
				{
					return ZDecimal.Zero;
				}
			}
		}

		public ZPropertyInfo DiscountAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DiscountAmount));
			}
		}

		#endregion

		#region ExchangeDifferenceAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ExchangeDifferenceAmount
		{
			get
			{
				if (PaymentForBinding != null)
				{
					return PaymentForBinding.ExchangeDifferenceAmount;
				}
				else
				{
					return ZDecimal.Zero;
				}
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

		#region Decimals

		public int OSDecimals => RefCurrency.LoadFromCurrencyCode(Factory, ForeignCurrency)?.Decimals ?? LocalDecimals;
		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

		#region Calc_ChequeNumberIsAutoAllocatedLabel

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		#endregion

		#region Calc_ChequeIsAutoPrintedLabel

		public ZString Calc_ChequeIsAutoPrintedLabel
		{
			get { return IsChequeNumberAutoAllocated ? AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeIsAutoPrintedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeIsAutoPrintedLabel)); }
		}

		#endregion

		#region Card Security Code

		[MaxLength(4)]
		public ZString CardSecurityCode
		{
			get { return fCardSecurityCode; }
			set
			{
				if (fCardSecurityCode != value)
				{
					SetNonPersistentPropertyValue(CardSecurityCodeInfo, ref fCardSecurityCode, value);
					foreach (PaymentApprovalBase payment in PaymentApprovalCollectionWithoutCancelledOrPosted)
					{
						payment.CreditCardSecurityCode = CardSecurityCode;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateCardSecurityCode();
					}
				}
			}
		}
		ZString fCardSecurityCode;

		public ZPropertyInfo CardSecurityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CardSecurityCode)); }
		}

		protected bool CardSecurityCode_ReadOnly
		{
			get { return APB_PaymentType != ReceiptTypes.eNettCreditCard; }
		}

		#endregion

		#region PostPaymentsAsPaymentApprovals

		ZBool fPostPaymentsAsPaymentApprovals = AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.Value;
		public ZBool PostPaymentsAsPaymentApprovals
		{
			get { return fPostPaymentsAsPaymentApprovals; }
			set { SetNonPersistentPropertyValue(PostPaymentsAsPaymentApprovalsInfo, ref fPostPaymentsAsPaymentApprovals, value); }
		}

		public ZPropertyInfo PostPaymentsAsPaymentApprovalsInfo
		{
			get { return GetZPropertyInfo(nameof(PostPaymentsAsPaymentApprovals)); }
		}

		#endregion

		#endregion

		#region Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			using (new DisposableAction(() => IsSettingDefaultValues = true, () => IsSettingDefaultValues = false))
			{
				base.SetDefaultValues();
				using (SuspendSettingHasChanges())
				{
					if (!IsInDatabase)
					{
						UpdateCurrencyForAllPayments();
					}
				}
			}
			SetHasChangesToFalse();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			base.BankAccount.Delete();

			var bankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD"))
				?? new TestObjectCreator(Factory).AUDBankAccount;
			APB_AB = bankAccount.PK;
		}
#endif

		new APPaymentBatchPosterValidation Validation => base.Validation as APPaymentBatchPosterValidation;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new APPaymentBatchPosterFetchStrategy(this);
		}

		protected override AccPaymentBatchValidation GetNewValidation()
		{
			return new APPaymentBatchPosterValidation(this);
		}

		#endregion

		#region Implementation

		ZBool IsSettingDefaultValues;

		void SetHasChangesToFalse()
		{
			foreach (PaymentApprovalBase paymentApproval in PaymentApprovalCollection)
			{
				paymentApproval.HasChanges = false;
				paymentApproval.MatchingBaseObject.MatchedTransactions.ForEach(x => x.HasChanges = false);
				paymentApproval.MatchingBaseObject.HasChanges = false;
			}
			HasChanges = false;
		}

		void SplitTransactionsToPaymentBatch()
		{
			List<PaymentGroup> groups = GroupPayments();

			foreach (PaymentGroup group in groups)
			{
				foreach (TransactionHeader transaction in group.Transactions)
				{
					group.Payment.MatchingBaseObject.UnmatchedTransactions.Add(transaction);
					SetAmountsForMatchingTransaction((IMatching)transaction, group.Payment);
					group.Payment.MatchingBaseObject.MoveFromUnmatchToMatch(new BusinessObject[] { transaction });
					transaction.HasChanges = false;
					group.Payment.MatchingBaseObject.HasChanges = false;
				}
				SetupPaymentAndAddToCollection(group.Payment);
			}
		}

		void SetupChildObjects()
		{
			RegisterEditableChildObject(PaymentApprovalCollection);
			PaymentApprovalCollection.RegisterEditableMatchingObjectsForAllPayments();
			PaymentApprovalCollection.SetReadOnlyIncludingChildren(false);
		}

		List<PaymentGroup> GroupPayments()
		{
			Dictionary<string, PaymentGroup> groups = new Dictionary<string, PaymentGroup>();

			foreach (TransactionHeader transaction in TransactionCollection)
			{
				string groupingKey = GetTransactionGroupingKey(transaction);

				TransactionHeaderCollection collectionToAddTransactionTo;

				if (groups.ContainsKey(groupingKey))
				{
					collectionToAddTransactionTo = groups[groupingKey].Transactions;
				}
				else
				{
					PaymentGroup group = new PaymentGroup(CreateNewPayment(GetSettlementGroup(transaction)));
					groups[groupingKey] = group;
					collectionToAddTransactionTo = group.Transactions;
				}

				collectionToAddTransactionTo.Add(transaction);
			}

			return new List<PaymentGroup>(groups.Values);
		}

		string GetTransactionGroupingKey(TransactionHeader transaction)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(GetSettlementGroup(transaction).ToString());

			if (groupByInvoicePaymentCriticality)
			{
				builder.Append(transaction.AH_RequisitionStatus);

				SystemDefinableCodeDescriptionBoolWithExtraBool setting = AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value.FindElementByCode(transaction.AH_RequisitionStatus);

				if (setting != null)
				{
					if (setting.Bool2)
					{
						builder.Append(Guid.NewGuid().ToString());
					}
				}
			}

			if (groupByInvoiceRelatedDebtorOrganisation)
			{
				InvoicingBase invoice = transaction as InvoicingBase;
				if (invoice != null)
				{
					if (invoice.RelatedTransactionDebtorsCodes.Length == 1)
					{
						builder.Append(invoice.RelatedTransactionDebtorsCodes[0]);
					}
					if (invoice.RelatedTransactionDebtorsCodes.Length > 1)
					{
						builder.Append(Guid.NewGuid().ToString());
					}
				}
			}

			if (groupByUser)
			{
				builder.Append(transaction.Logs.AddedLog.SL_UserNameAndInitials);
			}

			return builder.ToString();
		}

		ZGuid GetSettlementGroup(TransactionHeader transaction)
		{
			return transaction.OH_APSettlementGroup == ZGuid.Empty ? transaction.AH_OH : transaction.OH_APSettlementGroup;
		}

		class PaymentGroup
		{
			public PaymentGroup(PaymentApprovalBase payment)
			{
				this.Payment = payment;
			}

			#region Public Properties

			public PaymentApprovalBase Payment { get; private set; }

			TransactionHeaderCollection fTransactions;
			public TransactionHeaderCollection Transactions
			{
				get { return fTransactions ?? (fTransactions = new TransactionHeaderCollection(Payment.Factory)); }
			}

			#endregion
		}

		public CurrencySummary CurrencySummary => currencySummary ?? (currencySummary = new PaymentBatchCurrencySummary(PaymentApprovalCollection));
		CurrencySummary currencySummary;

		PaymentApprovalBase CreateNewPayment(ZGuid currentSettlementGroup)
		{
			var currentPayment = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			currentPayment.InitializeForPaymentBatch(() => !PostPaymentsAsPaymentApprovals);
			currentPayment.AV_OH = currentSettlementGroup;
			currentPayment.AV_APB_PaymentBatch = PK;
			if (currentPayment.BankAccount != null && !DefaultBankAccounts.Contains(currentPayment.BankAccount))
			{
				DefaultBankAccounts.Add(currentPayment.BankAccount);
			}
			currentPayment.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			return currentPayment;
		}

		public void LoadPayments()
		{
			PaymentApprovalCollection?.Cast<PaymentApprovalBase>().ForEach(x => SetupPaymentAndAddToCollection(x));
			PaymentApprovalCollection.Sort(PaymentApprovalBase.Schema.AV_Calc_Sequence);

			currencySummary = new PaymentBatchCurrencySummary(PaymentApprovalCollection);

			SetupChildObjects();
			SetHasChangesToFalse();
		}

		void SetupPaymentAndAddToCollection(PaymentApprovalBase payment)
		{
			if (payment.IsInDatabase)
			{
				payment.InitializeForPaymentBatch(() => false);
				payment.PaymentMatchingBaseObject.CreateTemporaryTransactions();
				PaymentApprovalCollection.Add(payment);
			}
			else if (!PaymentApprovalCollection.Contains(payment))
			{
				if (payment.MatchingBaseObject.Balance < 0)
				{
					payment.AV_Amount = -Env.CurrentCompany.ExchangeRate.LocalToForeign(payment.MatchingBaseObject.Balance, payment.AV_PayExRate, payment.AV_RX_NKPaymentCurrency);
				}
				payment.PaymentMatchingBaseObject.CreateTemporaryTransactions();
				payment.AV_PayExRateInfo.ValueChanged -= payment.AV_PayExRateChanged_ForPaymentBatch; // Prevent duplicate event hooking.
				payment.AV_PayExRateInfo.ValueChanged += payment.AV_PayExRateChanged_ForPaymentBatch;
				PaymentApprovalCollection.Add(payment);
			}
			else
			{
				if (payment.MatchingBaseObject.Balance < 0)
				{
					payment.AV_Amount = payment.AV_Amount - Env.CurrentCompany.ExchangeRate.LocalToForeign(payment.MatchingBaseObject.Balance, payment.AV_PayExRate, payment.AV_RX_NKPaymentCurrency);
				}
			}
			payment.HasChanges = false;
		}

		void SetAmountsForMatchingTransaction(IMatching transaction, PaymentApprovalBase currentPayment)
		{
			transaction.OSPartialPaymentAmount = transaction.OSOutstandingAmount;
			transaction.OSPartialPaymentAmountInfo.ValueChanged += new EventHandler(currentPayment.MatchingBaseObject.OSPartialPaymentAmountInfo_ValueChanged);
		}

		public void ApplyExchangeGainLossToAllSingleForeignCurrencyPayments(INotifications notifications)
		{
			var adjustmentHasOcurred = false;
			var anyLocalPaymentSkipped = false;
			var anyCancelledOrIsPostedPaymentSkipped = false;

			foreach (PaymentApprovalBase payment in PaymentApprovalCollection)
			{
				if (payment.CurrencyCode == LocalCurrency)
				{
					anyLocalPaymentSkipped = true;
				}
				else if (payment.IsCancelledOrIsPosted)
				{
					anyCancelledOrIsPostedPaymentSkipped = true;
				}
				else
				{
					var requiredAdjustment = -payment.MatchedTransactionsBalanceWithPaymentAmount;
					if (requiredAdjustment != 0)
					{
						if (payment.MatchingBaseObject.ExchangeDiffCurrent != null)
						{
							var newExxAmount = payment.MatchingBaseObject.ExchangeDiffCurrent.AH_OSTotal + requiredAdjustment;
							if (newExxAmount == 0)
							{
								payment.MatchingBaseObject.DeleteMiscTransaction(payment.MatchingBaseObject.ExchangeDiffCurrent);
							}
							else
							{
								payment.MatchingBaseObject.ExchangeDiffCurrent.AH_OSTotal = newExxAmount;
								payment.MatchingBaseObject.ExchangeDiffCurrent.AH_InvoiceAmount = newExxAmount;
								payment.MatchingBaseObject.ExchangeDiffCurrent.AH_OutstandingAmount = newExxAmount;
								payment.MatchingBaseObject.ExchangeDifferenceAmountInfo.RefreshBinding();
							}
						}
						else
						{
							payment.MatchingBaseObject.AddMiscellaneousTransaction(payment.MatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference, requiredAdjustment));
						}
						adjustmentHasOcurred = true;
					}
				}
			}

			if (anyLocalPaymentSkipped)
			{
				notifications.AddWarning(Res.GetString("b9ae7750-4d1e-4a9a-a82f-2887250d1825", "One or more payments were skipped because they were using a local currency"));
			}
			else if (anyCancelledOrIsPostedPaymentSkipped)
			{
				notifications.AddWarning(Res.GetString("C55FA0A0-C2FD-45B1-800F-B25441898637", "One or more payments were skipped because they were Posted Or Canceled."));
			}
			else if (!adjustmentHasOcurred)
			{
				notifications.AddWarning(Res.GetString("e3bbcdbe-182a-44d9-a9d2-ffadf94aaf4e", "No adjustment to the exchange gain/loss was required."));
			}
		}

		ZBool InitializeTransactionCollection(TransactionHeaderCollection unsortedTransactionCollection)
		{
			ZBool result = ZBool.False;
			if (unsortedTransactionCollection != null)
			{
				TransactionCollection = unsortedTransactionCollection;
				if (TransactionCollection.Count > 0)
				{
					TransactionCollection.Sort<TransactionHeader>(AccUtils.CompareTransactionsBySettlementGroupAndOrganisation);
					result = ZBool.True;
				}
			}
			return result;
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

		public PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = GetPeriodValidationProvider();
				}
				return fPeriodValidation;
			}
		}

		PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Factory);
		}

		PeriodValidationProvider fPeriodValidation;

		AccBankAccountCollection DefaultBankAccounts
		{
			get
			{
				if (fDefaultBankAccounts == null)
				{
					fDefaultBankAccounts = new AccBankAccountCollection(Factory);
				}
				return fDefaultBankAccounts;
			}
		}
		AccBankAccountCollection fDefaultBankAccounts;

		AccountingUtils AccUtils
		{
			get
			{
				if (fAccUtils == null)
				{
					fAccUtils = new AccountingUtils();
				}
				return fAccUtils;
			}
		}
		AccountingUtils fAccUtils;

		IMatchingCollection MatchedTransactions => MatchingBaseObject?.MatchedTransactions;

		MatchingBase MatchingBaseObject => PaymentForBinding?.MatchingBaseObject;

		internal PaymentApprovalBase PaymentForBinding
		{
			get
			{
				return fPaymentForBinding;
			}
			set
			{
				if (value != fPaymentForBinding)
				{
					UnBindMethodsToPayment(fPaymentForBinding);
					fPaymentForBinding = value;
					BindMethodsToPayment(fPaymentForBinding);
				}

				ResetPaymentMatchingCollection(MatchedTransactions);
				RefreshBinding();
			}
		}
		PaymentApprovalBase fPaymentForBinding;

		void BindMethodsToPayment(PaymentApprovalBase payment)
		{
			if (payment != null)
			{
				payment.AV_Calc_LocalAmountInfo.ValueChanged += new EventHandler(AV_AmountOrAV_Calc_LocalAmountChanged);
				payment.AV_AmountInfo.ValueChanged += new EventHandler(AV_AmountOrAV_Calc_LocalAmountChanged);
			}
		}

		void UnBindMethodsToPayment(PaymentApprovalBase payment)
		{
			if (payment != null && !payment.IsDeleted)
			{
				payment.AV_AmountInfo.ValueChanged -= new EventHandler(AV_AmountOrAV_Calc_LocalAmountChanged);
				payment.AV_Calc_LocalAmountInfo.ValueChanged -= new EventHandler(AV_AmountOrAV_Calc_LocalAmountChanged);
			}
		}

		void AV_AmountOrAV_Calc_LocalAmountChanged(object sender, EventArgs e)
		{
			RefreshBalance();
		}

		[BusinessObjectTestExclude]
		internal TransactionHeaderCollection TransactionCollection { get; private set; }

		internal bool IsPosted
		{
			get { return PaymentApprovalCollection.Any() && PaymentApprovalCollection.Cast<PaymentApprovalBase>().All(x => x.IsPosted); }
		}

		bool IsCheque
		{
			get { return APB_PaymentType == ReceiptTypes.Cheque; }
		}

		public ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint && IsCheque;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		void CheckIsAutoPrinted()
		{
			Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			if (IsChequeNumberAutoAllocated)
			{
				APB_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#region LookUps

		#region Bank Accounts

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
				}

				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		#endregion

		#region Credit Card Bank Accounts

		public AccBankAccountCollection CreditCardBankAccounts
		{
			get
			{
				if (fCreditCardBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					ZQuery creditCardFilter = new ZQuery(AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.CCD);
					creditCardFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_AccountType, AccountTypeCodeDescriptionPairList.Codes.LNK);
					creditCardFilter.AddToFilter(bankFilter, JoinCondition.And);

					fCreditCardBankAccounts = new AccBankAccountCollection(Factory, creditCardFilter);
				}
				return fCreditCardBankAccounts;
			}
		}
		AccBankAccountCollection fCreditCardBankAccounts;

		#endregion

		#region ChequeBooks

		public AccChequeBookCollection ChequeBooks
		{
			get
			{
				if (fChequeBooks == null)
				{
					ZQuery filter = new ZQuery(AccChequeBookSchema.AK_GB, GlbBranch.CurrentBranch.PK);

					if (APB_AB.IsValid)
					{
						filter.AddToFilter(AccChequeBookSchema.AK_AB, APB_AB);
					}
					fChequeBooks = new ActiveChequeBookCollection(Factory, filter);
					fChequeBooks.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("fe2686de-270a-4d6e-8af2-160e5b610b81", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
				}

				return fChequeBooks;
			}
		}

		AccChequeBookCollection fChequeBooks;

		public void ResetChequeBookCollection()
		{
			fChequeBooks = null;
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}

				return fCurrencies;
			}
		}

		RefCurrencyCollection fCurrencies;

		#endregion

		#region Headers

		public OrgHeaderCollection Headers
		{
			get
			{
				if (fHeaders == null)
				{
					fHeaders = new CreditorCollection(Factory);
				}
				return fHeaders;
			}
		}
		OrgHeaderCollection fHeaders;

		#endregion

		#endregion
	}
}
