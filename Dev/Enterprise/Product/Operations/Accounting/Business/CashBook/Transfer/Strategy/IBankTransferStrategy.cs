using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Strategy
{
	public interface IBankTransferStrategy
	{
		ZDecimal GetBuyAmount();
		void SetBuyAmount(ZDecimal value);

		ZDecimal GetBuyExchangeRate();
		void SetBuyExchangeRate(ZDecimal value);

		ZDecimal GetLocalBuyAmount();
		void SetLocalBuyAmount(ZDecimal value);

		ZDecimal GetSellAmount();
		void SetSellAmount(ZDecimal value);

		ZDecimal GetSellExchangeRate();
		void SetSellExchangeRate(ZDecimal value);

		ZDecimal GetLocalSellAmount();
		void SetLocalSellAmount(ZDecimal value);

		void UpdateReverseTransaction(BankTransfer reverseTransaction);
	}
}
