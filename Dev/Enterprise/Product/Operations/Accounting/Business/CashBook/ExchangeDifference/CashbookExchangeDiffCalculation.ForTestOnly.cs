#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public partial class CashbookExchangeDiffCalculation
	{
		public decimal FOSBankBalance_ForTestOnly
		{
			get { return fOSBankBalance; }
			set { fOSBankBalance = value; }
		}

		public decimal FLocalBankBalance_ForTestOnly
		{
			get { return fLocalBankBalance; }
			set { fLocalBankBalance = value; }
		}

		public ZDecimal FLocalAmount_ForTestOnly
		{
			get { return fLocalAmount; }
			set { fLocalAmount = value; }
		}
	}
}

#endif
