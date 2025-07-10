using System;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Strategy
{
	public class BankTransferIncludeExchangeVarianceInRowToStrategy : IBankTransferStrategy, IDisposable
	{
		public BankTransferIncludeExchangeVarianceInRowToStrategy(BankTransfer bankTransfer)
		{
			BankTransfer = bankTransfer;

			Init();
		}

		#region Buy

		public ZDecimal GetBuyAmount() => OSAmountIncludeExchangeVariance;
		public void SetBuyAmount(ZDecimal value)
		{
			OSAmountIncludeExchangeVariance = value;
		}

		public ZDecimal GetBuyExchangeRate() => ExchangeRateIncludeExchangeVariance;
		public void SetBuyExchangeRate(ZDecimal value)
		{
			ExchangeRateIncludeExchangeVariance = value;
		}

		public ZDecimal GetLocalBuyAmount() => LocalAmountIncludeExchangeVariance;
		public void SetLocalBuyAmount(ZDecimal value)
		{
			LocalAmountIncludeExchangeVariance = value;
		}

		#endregion

		#region Sell

		public ZDecimal GetSellAmount() => TransferRowFrom.AH_OSExTaxAmount;
		public void SetSellAmount(ZDecimal value)
		{
			TransferRowFrom.AH_OSExTaxAmount = value;
			TransferRowFrom.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(value, TransferRowFrom.AH_ExchangeRate);
			UpdateTransferRowTo();
		}

		public ZDecimal GetSellExchangeRate() => TransferRowFrom.AH_ExchangeRate;
		public void SetSellExchangeRate(ZDecimal value)
		{
			TransferRowFrom.AH_ExchangeRate = value;
			TransferRowFrom.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(TransferRowFrom.AH_OSExTaxAmount, value);
			UpdateTransferRowTo();
		}

		public ZDecimal GetLocalSellAmount() => TransferRowFrom.AH_LocalExTaxAmount;
		public void SetLocalSellAmount(ZDecimal value)
		{
			UpdateLocalSellAmount(value);
			UpdateTransferRowTo();
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
				UpdateTransferRowTo();
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
				UpdateTransferRowTo();
			}
		}
		ZDecimal exchangeRateIncludeExchangeVariance;

		public ZDecimal LocalAmountIncludeExchangeVariance
		{
			get => localAmountIncludeExchangeVariance;
			set
			{
				UpdateLocalBuyAmountIncludeExchangeVariance(value);
				UpdateTransferRowTo();
			}
		}
		ZDecimal localAmountIncludeExchangeVariance;

		#endregion

		public void UpdateReverseTransaction(BankTransfer reverseTransaction)
		{
			reverseTransaction.TransferRowTo.AH_ExchangeRate = TransferRowFrom.AH_ExchangeRate;
			reverseTransaction.TransferRowTo.AH_OSExTaxAmount = TransferRowFrom.AH_OSExTaxAmount;
			reverseTransaction.TransferRowTo.AH_LocalExTaxAmount = TransferRowFrom.AH_LocalExTaxAmount;

			reverseTransaction.SellAmount = BankTransfer.BuyAmount;
			reverseTransaction.LocalSellAmount = BankTransfer.LocalBuyAmount;
		}
		
		public void Dispose()
		{
			UpdateLocalBuyAmount(GetLocalSellAmount());
			UnHookEvents();
		}

		void Init()
		{
			if (BankTransfer.IsPosted)
			{
				localAmountIncludeExchangeVariance = TransferRowTo.AH_LocalExTaxAmount + BankTransfer.ExRateGainLoss;
				osAmountIncludeExchangeVariance = BuyCurrency == BankTransfer.LocalCurrency ? localAmountIncludeExchangeVariance : TransferRowTo.AH_OSExTaxAmount;
				exchangeRateIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.GetRate(localAmountIncludeExchangeVariance, osAmountIncludeExchangeVariance);
			}
			else
			{
				if (!SellCurrency.IsEmpty && SellCurrency != BankTransfer.LocalCurrency)
				{
					TransferRowFrom.ExchangeRate.RefetchExchangeRate();
					TransferRowFrom.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(TransferRowFrom.AH_OSExTaxAmount, TransferRowFrom.AH_ExchangeRate);
				}

				if (!BuyCurrency.IsEmpty && BuyCurrency != BankTransfer.LocalCurrency)
				{
					TransferRowTo.ExchangeRate.RefetchExchangeRate();
					TransferRowTo.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(TransferRowTo.AH_OSExTaxAmount, TransferRowTo.AH_ExchangeRate);
				}

				osAmountIncludeExchangeVariance = TransferRowTo.AH_OSExTaxAmount;
				localAmountIncludeExchangeVariance = TransferRowTo.AH_LocalExTaxAmount;
				exchangeRateIncludeExchangeVariance = TransferRowTo.AH_ExchangeRate;

				UpdateTransferRowTo();
				HookEvents();
			}
		}

		void HookEvents()
		{
			TransferRowTo.ExchangeRate.CurrencyInfo.ValueChanged += OnTransferRowToCurrencyInfoValueChanged;
		}

		void UnHookEvents()
		{
			TransferRowTo.ExchangeRate.CurrencyInfo.ValueChanged -= OnTransferRowToCurrencyInfoValueChanged;
		}

		void OnTransferRowToCurrencyInfoValueChanged(object sender, EventArgs e)
		{
			if (IsBankAccountSpecifiedOnTransferRowTo)
			{
				exchangeRateIncludeExchangeVariance = TransferRowTo.AH_ExchangeRate;
			}
		}

		void UpdateLocalBuyAmountIncludeExchangeVariance(ZDecimal localAmount)
		{
			localAmountIncludeExchangeVariance = localAmount;

			if (BuyCurrency == BankTransfer.LocalCurrency)
			{
				osAmountIncludeExchangeVariance = localAmount;
			}
			else
			{
				exchangeRateIncludeExchangeVariance = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, osAmountIncludeExchangeVariance);
			}
		}

		void UpdateLocalSellAmount(ZDecimal localAmount)
		{
			TransferRowFrom.AH_LocalExTaxAmount = localAmount;

			if (SellCurrency == BankTransfer.LocalCurrency)
			{
				TransferRowFrom.AH_OSExTaxAmount = localAmount;
			}
			else
			{
				TransferRowFrom.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, TransferRowFrom.AH_OSExTaxAmount);
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

		void UpdateTransferRowTo()
		{
			if (IsBankAccountSpecifiedOnTransferRowTo
				&& OSAmountIncludeExchangeVariance != 0m
				&& LocalAmountIncludeExchangeVariance != 0m
				&& ExchangeRateIncludeExchangeVariance != 0m
				&& GetLocalSellAmount() != 0m)
			{
				TransferRowTo.AH_OSExTaxAmount = BuyCurrency == BankTransfer.LocalCurrency ? GetLocalSellAmount() : OSAmountIncludeExchangeVariance;
				TransferRowTo.AH_LocalExTaxAmount = GetLocalSellAmount();
				TransferRowTo.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(TransferRowTo.AH_LocalExTaxAmount, TransferRowTo.AH_OSExTaxAmount);
			}
			else
			{
				TransferRowTo.AH_OSExTaxAmount = OSAmountIncludeExchangeVariance;
				TransferRowTo.AH_LocalExTaxAmount = LocalAmountIncludeExchangeVariance;
				TransferRowTo.AH_ExchangeRate = ExchangeRateIncludeExchangeVariance;
			}
		}

		public BankTransfer BankTransfer { get; }

		bool IsBankAccountSpecifiedOnTransferRowTo => TransferRowTo.AH_AB.IsValid && TransferRowTo.BankAccount != null;

		BankTransferFromRow TransferRowFrom => BankTransfer.TransferRowFrom;

		BankTransferToRow TransferRowTo => BankTransfer.TransferRowTo;

		ZString SellCurrency => BankTransfer.SellCurrency;

		ZString BuyCurrency => BankTransfer.BuyCurrency;
	}
}
