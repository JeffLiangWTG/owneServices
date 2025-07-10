using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Strategy
{
	public class BankTransferIncludeExchangeVarianceInRowFromStrategy : IBankTransferStrategy
	{
		public BankTransferIncludeExchangeVarianceInRowFromStrategy(BankTransfer bankTransfer)
		{
			BankTransfer = bankTransfer;

			Init();
		}

		#region Sell

		public ZDecimal GetSellAmount() => OSAmountIncludeExchangeVariance;
		public void SetSellAmount(ZDecimal value)
		{
			OSAmountIncludeExchangeVariance = value;
		}

		public ZDecimal GetSellExchangeRate() => ExchangeRateIncludeExchangeVariance;
		public void SetSellExchangeRate(ZDecimal value)
		{
			ExchangeRateIncludeExchangeVariance = value;
		}

		public ZDecimal GetLocalSellAmount() => LocalAmountIncludeExchangeVariance;
		public void SetLocalSellAmount(ZDecimal value)
		{
			LocalAmountIncludeExchangeVariance = value;
		}

		#endregion

		#region Buy

		public ZDecimal GetBuyAmount() => TransferRowTo.AH_OSExTaxAmount;
		public void SetBuyAmount(ZDecimal value)
		{
			TransferRowTo.AH_OSExTaxAmount = value;
			TransferRowTo.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(value, TransferRowTo.AH_ExchangeRate);
			UpdateTransferRowFrom();
		}

		public ZDecimal GetBuyExchangeRate() => TransferRowTo.AH_ExchangeRate;
		public void SetBuyExchangeRate(ZDecimal value)
		{
			TransferRowTo.AH_ExchangeRate = value;
			TransferRowTo.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(TransferRowTo.AH_OSExTaxAmount, value);
			UpdateTransferRowFrom();
		}

		public ZDecimal GetLocalBuyAmount() => TransferRowTo.AH_LocalExTaxAmount;
		public void SetLocalBuyAmount(ZDecimal value)
		{
			UpdateLocalBuyAmount(value);
			UpdateTransferRowFrom();
		}

		#endregion

		#region Include Exchange Variance

		public ZDecimal OSAmountIncludeExchangeVariance
		{
			get => osAmountIncludeExchangeVariance;
			set
			{
				osAmountIncludeExchangeVariance = value;
				localAmountIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.ForeignToLocal(value, ExchangeRateIncludeExchangeVariance);
				UpdateTransferRowFrom();
			}
		}
		ZDecimal osAmountIncludeExchangeVariance;

		public ZDecimal ExchangeRateIncludeExchangeVariance
		{
			get => exchangeRateIncludeExchangeVariance;
			set
			{
				exchangeRateIncludeExchangeVariance = value;
				localAmountIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.ForeignToLocal(OSAmountIncludeExchangeVariance, value);
				UpdateTransferRowFrom();
			}
		}
		ZDecimal exchangeRateIncludeExchangeVariance;

		public ZDecimal LocalAmountIncludeExchangeVariance
		{
			get => localAmountIncludeExchangeVariance;
			set
			{
				UpdateLocalSellAmountIncludeExchangeVariance(value);
				UpdateTransferRowFrom();
			}
		}
		ZDecimal localAmountIncludeExchangeVariance;

		#endregion

		public void UpdateReverseTransaction(BankTransfer reverseTransaction)
		{
		}

		void Init()
		{
			if (BankTransfer.IsPosted)
			{
				localAmountIncludeExchangeVariance = TransferRowFrom.AH_LocalExTaxAmount - BankTransfer.ExRateGainLoss;
				osAmountIncludeExchangeVariance = SellCurrency == BankTransfer.LocalCurrency ? localAmountIncludeExchangeVariance : TransferRowFrom.AH_OSExTaxAmount;
				exchangeRateIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.GetRate(localAmountIncludeExchangeVariance, osAmountIncludeExchangeVariance);
			}
			else
			{
				exchangeRateIncludeExchangeVariance = TransferRowFrom.AH_ExchangeRate;
			}
		}

		void UpdateLocalSellAmountIncludeExchangeVariance(ZDecimal localAmount)
		{
			localAmountIncludeExchangeVariance = localAmount;

			if (SellCurrency == BankTransfer.LocalCurrency)
			{
				osAmountIncludeExchangeVariance = localAmount;
			}
			else
			{
				exchangeRateIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, osAmountIncludeExchangeVariance);
			}
		}

		void UpdateLocalBuyAmount(ZDecimal localAmount)
		{
			TransferRowTo.AH_LocalExTaxAmount = localAmount;

			if (BuyCurrency == BankTransfer.LocalCurrency)
			{
				TransferRowTo.AH_OSExTaxAmount = localAmount;
			}
			else
			{
				TransferRowTo.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, TransferRowTo.AH_OSExTaxAmount);
			}
		}

		void UpdateTransferRowFrom()
		{
			if (IsBankAccountSpecifiedOnTransferRowFrom
				&& OSAmountIncludeExchangeVariance != 0m
				&& LocalAmountIncludeExchangeVariance != 0m
				&& ExchangeRateIncludeExchangeVariance != 0m
				&& GetLocalBuyAmount() != 0m)
			{
				TransferRowFrom.AH_OSExTaxAmount = SellCurrency == BankTransfer.LocalCurrency ? GetLocalBuyAmount() : OSAmountIncludeExchangeVariance;
				TransferRowFrom.AH_LocalExTaxAmount = GetLocalBuyAmount();
				TransferRowFrom.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(TransferRowFrom.AH_LocalExTaxAmount, TransferRowFrom.AH_OSExTaxAmount);
			}
			else
			{
				TransferRowFrom.AH_OSExTaxAmount = OSAmountIncludeExchangeVariance;
				TransferRowFrom.AH_LocalExTaxAmount = LocalAmountIncludeExchangeVariance;
				TransferRowFrom.AH_ExchangeRate = ExchangeRateIncludeExchangeVariance;
			}
		}

		public BankTransfer BankTransfer { get; }

		bool IsBankAccountSpecifiedOnTransferRowFrom => TransferRowFrom.AH_AB.IsValid && TransferRowFrom.BankAccount != null;

		BankTransferFromRow TransferRowFrom => BankTransfer.TransferRowFrom;

		BankTransferToRow TransferRowTo => BankTransfer.TransferRowTo;

		ZString SellCurrency => BankTransfer.SellCurrency;

		ZString BuyCurrency => BankTransfer.BuyCurrency;
	}
}
