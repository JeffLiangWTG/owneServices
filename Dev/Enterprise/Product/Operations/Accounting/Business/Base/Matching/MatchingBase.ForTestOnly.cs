#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class MatchingBase
	{
		public ARAP.ReceiptPayment.ReceiptPaymentBase ReceiptPaymentDetail_ForTestOnly
		{
			get { return ReceiptPaymentDetail; }
		}

		public ARAP.Overpayment.Overpayment OverpaymentBizO_ForTestOnly
		{
			get { return OverpaymentBizO; }
			set { OverpaymentBizO = value; }
		}

		public ZBool Match_ForTestOnly()
		{
			return Match();
		}

		public ZString LedgerTypeCore_ForTestOnly => LedgerTypeCore;

		public ARAP.Discount DiscountBizO_ForTestOnly
		{
			get { return DiscountBizO; }
			set { DiscountBizO = value; }
		}

		public ARAP.ExchangeDifference ExchangeDifferenceBizO_ForTestOnly
		{
			get { return ExchangeDifferenceBizO; }
			set { ExchangeDifferenceBizO = value; }
		}

		public ARAP.Journal.Journal BankFeeBizO_ForTestOnly
		{
			get { return BankFeeBizO; }
			set { BankFeeBizO = value; }
		}

		public Transaction.TransactionHeaderCollection LoadedTransactions_ForTestOnly => LoadedTransactions;

		public void LoadTransactionsMatchingTheFilterCore_ForTestOnly(ZQuery additionalFilter)
		{
			LoadTransactionsMatchingTheFilterCore(additionalFilter);
		}

		public BusinessObject[] FMatchingLoadedBizOs_ForTestOnly
		{
			get { return fMatchingLoadedBizOs; }
			set { fMatchingLoadedBizOs = value; }
		}

		public void SetIsLoadedFromGUIForTest_ForTestOnly(bool value)
		{
			SetIsLoadedFromGUIForTest(value);
		}

		public void MoveFilterMatchingTransactionsToOutstanding_ForTestOnly()
		{
			MoveFilterMatchingTransactionsToOutstanding();
		}

		public void OnSuccessfulMatching_ForTestOnly()
		{
			OnSuccessfulMatching();
		}

		public void ClearCachedMiscTransactions_ForTestOnly()
		{
			ClearCachedMiscTransactions();
		}

		public bool IsLoadedFromGUI_ForTestOnly
		{
			get { return IsLoadedFromGUI; }
			set { IsLoadedFromGUI = value; }
		}

		public bool AllowFutureMatchDate_ForTestOnly => AllowFutureMatchDate;
	}
}

#endif
