using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public abstract partial class PaymentApprovalMatchingBase : MatchingBase
	{
		protected PaymentApprovalMatchingBase(BusinessObjectFactory factory, PaymentApprovalBase paymentApprovalDetail)
			: base(factory, paymentApprovalDetail.IsLoadedFromGUI)
		{
			this.PaymentApprovalDetail = paymentApprovalDetail;

			IMatching paymentApprovalMatching = paymentApprovalDetail;
			paymentApprovalMatching.OSPartialPaymentAmount = paymentApprovalMatching.OSOutstandingAmount;
			paymentApprovalMatching.OSPartialPaymentAmountInfo.ValueChanged += new EventHandler(OSPartialPaymentAmountInfo_ValueChanged);
			MatchedTransactions.AddTransactionThatMustBeMatched(paymentApprovalMatching);
			MatchDate = paymentApprovalMatching.PostDate;

			PrimaryOrganisationForGUINotification = paymentApprovalDetail.AV_OH;
			PrimaryOrganization = paymentApprovalDetail.AV_OH;
			IsInitializedFromPayment = true;

			LoadExistingPaymentApprovalItems();
			UpdateMatchDateToMaxPostDateOfMatchedTransactions();

			BalanceInfo.RefreshBinding();
			Factory.Saved += Factory_Saved;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				ClearAndSetMatchedTransactionsFactoryCache();
			}
		}

		void UpdateMatchDateToMaxPostDateOfMatchedTransactions()
		{
			if (MatchedTransactions.Count > 0)
			{
				ZDateTime maxDate = MatchedTransactions[0].PostDate;
				foreach (IMatching matching in MatchedTransactions)
				{
					if (matching.PostDate > maxDate)
					{
						maxDate = matching.PostDate;
					}
				}
				if (MatchDate.Date < maxDate.Date)
				{
					MatchDate = maxDate;
				}
			}
		}

		public void LoadExistingPaymentApprovalItems()
		{
			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(PaymentApproval);
			approvalItems.Load();

			foreach (PaymentApprovalItem item in approvalItems)
			{
				TransactionHeader matchedHeader = item.Header;

				if (matchedHeader != null && matchedHeader is IMatching)
				{
					if (UnmatchedTransactions.Contains(matchedHeader))
					{
						UnmatchedTransactions.Remove(matchedHeader);
					}
					((IMatching)matchedHeader).OSPartialPaymentAmount = item.OSAmountPaidThisRun;
					MatchedTransactions.Add(matchedHeader);
				}
			}

			BalanceInfo.RefreshBinding();
			ClearAndSetMatchedTransactionsFactoryCache();
		}

		IEnumerable<ZGuid> ClearAndSetMatchedTransactionsFactoryCache()
		{
			Factory.ClearCachedValue<IEnumerable<ZGuid>>(GetMatchedTransactionsCacheKey());
			return Factory.GetCachedValue(GetMatchedTransactionsCacheKey(), () => MatchedTransactions.Select(x => x.PK), CacheStalenessPolicy.NeverStale);
		}

		string GetMatchedTransactionsCacheKey()
		{
			return $"MatchedTransactionsOriginal|{PaymentApproval.PK.ToStringKey()}";
		}

		public bool IsMatchTransactionsInDbChangedForDraft
		{
			get
			{
				var result = false;
				if (PaymentApproval.IsSavingPaymentApprovalAsDraft)
				{
					IEnumerable<ZGuid> matchedTransactionPKsInCache;
					Factory.TryGetValueFromCacheOnly(GetMatchedTransactionsCacheKey(), out matchedTransactionPKsInCache);
					if (matchedTransactionPKsInCache != null)
					{
						var approvalItems = new PaymentApprovalItemCollection(PaymentApproval);
						approvalItems.Reload(true);

						foreach (PaymentApprovalItem item in approvalItems)
						{
							if (!matchedTransactionPKsInCache.Contains(item.Header.PK))
							{
								result = true;
								break;
							}
						}
					}
				}

				return result;
			}
		}

		public void CreateTemporaryTransactions()
		{
			if (PaymentApprovalDetail.AV_ExchangeDifference != 0 || PaymentApprovalDetail.AV_Discount != 0)
			{
				using (MatchedTransactions.SuspendBalanceCalculation())
				{
					if (PaymentApprovalDetail.AV_ExchangeDifference != 0 && ExchangeDifferenceBizO == null)
					{
						ExchangeDifferenceBizO = (ExchangeDifference)GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference, PaymentApprovalDetail.AV_ExchangeDifference);
						ExchangeDifferenceBizO.IsOSPartialPaymentAmountReadOnly = true;
						ExchangeDifferenceBizO.AH_MatchStatus = PaymentApprovalDetail.AV_ExxMatchStatus;
						ExchangeDifferenceBizO.AH_MatchStatusReasonCode = PaymentApprovalDetail.AV_ExxMatchStatusReasonCode;
						PaymentApprovalDetail.AV_ExchangeDifference = 0;
						MatchedTransactions.Add(ExchangeDifferenceBizO);
					}

					if (PaymentApprovalDetail.AV_Discount != 0 && DiscountBizO == null)
					{
						DiscountBizO = (Discount)GetMiscellaneousTransaction(TransactionTypes.Discount, PaymentApprovalDetail.AV_Discount);
						DiscountBizO.IsOSPartialPaymentAmountReadOnly = true;
						DiscountBizO.AH_MatchStatus = PaymentApprovalDetail.AV_DscMatchStatus;
						DiscountBizO.AH_MatchStatusReasonCode = PaymentApprovalDetail.AV_DscMatchStatusReasonCode;
						PaymentApprovalDetail.AV_Discount = 0;
						MatchedTransactions.Add(DiscountBizO);
					}
				}
			}
		}

		public void DeleteTemporaryTransactions(bool onlyIfTransactionCreated = false)
		{
			if (!onlyIfTransactionCreated || ExchangeDifferenceBizO != null)
			{
				PaymentApprovalDetail.AV_ExchangeDifference = ExchangeDifferenceAmount;
				PaymentApprovalDetail.AV_ExxMatchStatus = ExchangeDifferenceBizO?.AH_MatchStatus ?? ZString.Empty;
				PaymentApprovalDetail.AV_ExxMatchStatusReasonCode = ExchangeDifferenceBizO?.AH_MatchStatusReasonCode ?? ZString.Empty;
				DeleteExchangeDiff();
			}

			if (!onlyIfTransactionCreated || DiscountBizO != null)
			{
				PaymentApprovalDetail.AV_Discount = DiscountAmount;
				PaymentApprovalDetail.AV_DscMatchStatus = DiscountBizO?.AH_MatchStatus ?? ZString.Empty;
				PaymentApprovalDetail.AV_DscMatchStatusReasonCode = DiscountBizO?.AH_MatchStatusReasonCode ?? ZString.Empty;
				DeleteDiscount();
			}
		}

		protected override ZQuery GetFilter()
		{
			ZQuery result = base.GetFilter();

			if (PaymentApproval.AV_PaymentType == ReceiptTypes.eNettDirectDebit)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				result.AddToFilter(AccTransactionHeaderSchema.AH_GC, PaymentApproval.Branch.Company.PK);
			}

			return result;
		}

		protected override bool MatchAndClearTransactionsCore()
		{
			return Match();
		}

		protected override ZBool Match()
		{
			OrgHeader orgBizO = Factory.Load<OrgHeader>(PrimaryOrganization);

			ZBool matchResult = SessionBalancesToZero || PaymentApproval.IsSavingPaymentApprovalAsDraft;

			if (matchResult && !MatchedTransactions.ContainsFullyPaidTransaction &&
				MatchedTransactions.ContainsTransactionFromSpecifiedOrg(orgBizO))
			{
				GeneratePaymentApprovalItems();
			}

			if (PaymentApprovalDetail != null)
			{
				PaymentApprovalDetail.CurrentMatchingDate = MatchDate;
			}

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PaymentApproval.PK,
				CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails, () =>
				{
					return GetMatchingDetails("PaymentApprovalMatchingBase.Match()");
				});

			return matchResult;
		}

		protected override ZDecimal BalanceCore
		{
			get
			{
				ZDecimal result = base.BalanceCore;

				if (ExchangeDifferenceBizO == null)
				{
					result += PaymentApprovalDetail.AV_ExchangeDifference;
				}

				if (DiscountBizO == null)
				{
					result += PaymentApprovalDetail.AV_Discount;
				}

				return result;
			}
		}

		public bool SessionBalancesToZero
		{
			get { return Balance == 0; }
		}

		protected void GeneratePaymentApprovalItems()
		{
			PaymentApprovalItems.AddRange(MatchedTransactions.GeneratePaymentApprovalItems(PaymentApprovalDetail));

			PaymentApprovalItemCollection items = new PaymentApprovalItemCollection(PaymentApprovalDetail);
			items.Load();

			PaymentApprovalItemCollection itemsToRemoveAndDelete = new PaymentApprovalItemCollection(Factory);

			foreach (PaymentApprovalItem item in items)
			{
				IMatching itemHeaderAsIMatching = (IMatching)item.Header;

				if (!MatchedTransactions.Contains(item.A2_AH) || itemHeaderAsIMatching.LocalPartialPaymentAmount == 0)
				{
					if (PaymentApprovalItems.Contains(item.PK))
					{
						PaymentApprovalItems.Remove(item.PK);
					}

					itemsToRemoveAndDelete.Add(item);
				}
			}

			if (itemsToRemoveAndDelete.Count > 0)
			{
				itemsToRemoveAndDelete.RemoveAndDeleteAll();
			}
		}

		protected override ZBool AllowAlteringOfPaymentCore
		{
			get { return true; }
		}

		protected override RefCurrency PaymentCurrency
		{
			get { return PaymentApproval != null ? PaymentApproval.PaymentCurrency : null; }
		}

		protected override ZGuid PaymentDetailPK
		{
			get { return PaymentApproval != null ? PaymentApproval.PK : ZGuid.Empty; }
		}

		protected override void UpdatePaymentOSPartialPaidAmountCore()
		{
		}

		protected override ZBool IsNotMatchingPaymentCore
		{
			get { return ZBool.False; }
		}

		protected override ZBool IsMatchingPaymentOrReceiptCore
		{
			get { return ZBool.True; }
		}

		public override void Delete()
		{
			MatchedTransactions.CountChanged -= new CollectionCountChangedEventHandler(fTransactions_CountChanged);
			base.Delete();
		}

		#region PaymentApprovalItems

		public PaymentApprovalItemCollection PaymentApprovalItems
		{
			get
			{
				if (fPaymentApprovalItems == null)
				{
					fPaymentApprovalItems = new PaymentApprovalItemCollection(PaymentApproval);
				}

				return fPaymentApprovalItems;
			}
		}

		PaymentApprovalItemCollection fPaymentApprovalItems;

		#endregion

		#region Implementation

		public ZDecimal CalculateOSBalanceExcludingPayment()
		{
			ZDecimal localBalance = 0m;

			foreach (IMatching matchable in MatchedTransactions)
			{
				if (matchable.Identifier != PaymentApprovalDetail.PK)
				{
					localBalance += matchable.LocalPartialPaymentAmount;
				}
			}

			ZDecimal result = PaymentApprovalDetail.AV_Amount;

			if (localBalance != 0m && PaymentApprovalDetail.PaymentCurrency != null)
			{
				ZDecimal exchangeRate = PaymentApprovalDetail.AV_PayExRate;
				ZString foreignCurrencyNK = PaymentApprovalDetail.AV_RX_NKPaymentCurrency;
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localBalance * -1, exchangeRate, foreignCurrencyNK);
			}

			return result;
		}

		protected override ZBool IsOverPaymentAllowedInThisMatchingSessionCore
		{
			get { return false; }
		}

		public PaymentApprovalBase PaymentApproval
		{
			get { return PaymentApprovalDetail; }
		}

		protected PaymentApprovalBase PaymentApprovalDetail;

		#region Validate

		protected override void ValidateZeroBalance()
		{
			if (!PaymentApprovalDetail.IsSavingPaymentApprovalAsDraft)
			{
				base.ValidateZeroBalance();
			}
		}

		void ValidateZeroPaymentApprovalAmount()
		{
			var error = Res.GetString("929f422b-4693-449a-baee-2f349dbe25e1", "Overseas amount can not be zero");
			ClearRowNotificationsContaining(error);

			if (PaymentApproval.AV_Amount == 0)
			{
				AddRowError(error);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateZeroPaymentApprovalAmount();
		}

		#endregion

		#endregion
	}
}
