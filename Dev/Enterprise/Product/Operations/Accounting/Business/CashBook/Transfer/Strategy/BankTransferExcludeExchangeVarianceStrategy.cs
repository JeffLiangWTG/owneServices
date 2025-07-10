using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Strategy
{
	public class BankTransferExcludeExchangeVarianceStrategy : IBankTransferStrategy
	{
		public BankTransferExcludeExchangeVarianceStrategy(BankTransfer bankTransfer)
		{
			BankTransfer = bankTransfer;
		}

		#region Buy

		public ZDecimal GetBuyAmount() => TransferRowTo.AH_OSExTaxAmount;
		public void SetBuyAmount(ZDecimal value)
		{
			if (BankTransfer.BuySellAmountsAndRatesReadOnly)
			{
				TransferRowTo.AH_OSExTaxAmount = value;
				return;
			}

			CalculateBuyAmountFull(value);
		}

		public ZDecimal GetBuyExchangeRate() => TransferRowTo.AH_ExchangeRate;
		public void SetBuyExchangeRate(ZDecimal value)
		{
			TransferRowTo.AH_ExchangeRate = value;
			CalculateBuyAmountLight();
		}

		public ZDecimal GetLocalBuyAmount() => TransferRowTo.AH_LocalExTaxAmount;
		public void SetLocalBuyAmount(ZDecimal value)
		{
			if (BankTransfer.BuySellAmountsAndRatesReadOnly)
			{
				TransferRowTo.AH_LocalExTaxAmount = value;
				return;
			}

			UpdateLocalAmounts(value);
		}

		#endregion

		#region Sell

		public ZDecimal GetSellAmount() => TransferRowFrom.AH_OSExTaxAmount;
		public void SetSellAmount(ZDecimal value)
		{
			TransferRowFrom.AH_OSExTaxAmount = value;
			CalculateBuyAmountLight();
		}

		public ZDecimal GetSellExchangeRate() => TransferRowFrom.AH_ExchangeRate;
		public void SetSellExchangeRate(ZDecimal value)
		{
			TransferRowFrom.AH_ExchangeRate = value;
			CalculateBuyAmountLight();
		}

		public ZDecimal GetLocalSellAmount() => TransferRowFrom.AH_LocalExTaxAmount;
		public void SetLocalSellAmount(ZDecimal value)
		{
			UpdateLocalAmounts(value);
		}

		#endregion

		public void UpdateReverseTransaction(BankTransfer reverseTransaction)
		{
			reverseTransaction.TransferRowFrom.AH_ExchangeRate = TransferRowTo.AH_ExchangeRate;
			reverseTransaction.TransferRowFrom.AH_OSExTaxAmount = TransferRowTo.AH_OSExTaxAmount;
			reverseTransaction.TransferRowFrom.AH_LocalExTaxAmount = TransferRowTo.AH_LocalExTaxAmount;

			reverseTransaction.TransferRowTo.AH_ExchangeRate = TransferRowFrom.AH_ExchangeRate;
			reverseTransaction.TransferRowTo.AH_OSExTaxAmount = TransferRowFrom.AH_OSExTaxAmount;
			reverseTransaction.TransferRowTo.AH_LocalExTaxAmount = TransferRowFrom.AH_LocalExTaxAmount;
		}

		void CalculateBuyAmountFull(ZDecimal value)
		{
			var sellCurrencyIsLocal = SellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var buyCurrencyIsLocal = BuyCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var bothCurrenciesAreLocal = sellCurrencyIsLocal && buyCurrencyIsLocal;

			if (bothCurrenciesAreLocal)
			{
				if (TransferRowFrom.AH_ExchangeRate != 1m)
				{
					TransferRowFrom.AH_ExchangeRate = 1m;
				}

				if (TransferRowTo.AH_ExchangeRate != 1m)
				{
					TransferRowTo.AH_ExchangeRate = 1m;
				}

				TransferRowTo.AH_LocalExTaxAmount = TransferRowFrom.AH_OSExTaxAmount = TransferRowTo.AH_OSExTaxAmount = value;
			}
			else
			{
				var toLocalAmount = TransferRowTo.AH_LocalExTaxAmount;

				if (sellCurrencyIsLocal)
				{
					if (!GetSellAmount().IsEmpty && !value.IsEmpty)
					{
						TransferRowTo.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(GetSellAmount(), value);
					}
				}
				else if (buyCurrencyIsLocal)
				{
					if (TransferRowTo.AH_ExchangeRate != 1m)
					{
						TransferRowTo.AH_ExchangeRate = 1m;
					}

					if (!GetSellAmount().IsEmpty && !value.IsEmpty)
					{
						TransferRowFrom.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(value, GetSellAmount());
					}

					toLocalAmount = value;
				}
				else if (!GetLocalBuyAmount().IsEmpty && !value.IsEmpty)
				{
					TransferRowTo.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(GetLocalBuyAmount(), value);
				}

				TransferRowTo.AH_OSExTaxAmount = value;
				TransferRowTo.AH_LocalExTaxAmount = toLocalAmount;
			}

			TransferRowFrom.AH_InvoiceAmount = TransferRowTo.AH_InvoiceAmount * -1;
		}

		void CalculateBuyAmountLight()
		{
			if (BankTransfer.BuySellAmountsAndRatesReadOnly || BuyCurrency.IsEmpty)
			{
				return;
			}

			if (GetBuyExchangeRate() != 0m && GetSellExchangeRate() != 0m)
			{
				var localAmountWithoutRounding = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(TransferRowFrom.AH_OSExTaxAmount, GetSellExchangeRate());
				TransferRowTo.AH_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeignWithoutRounding(localAmountWithoutRounding, GetBuyExchangeRate(), BuyCurrency);
				TransferRowFrom.AH_LocalExTaxAmount = TransferRowTo.AH_LocalExTaxAmount;
			}
			else
			{
				TransferRowTo.AH_LocalExTaxAmount = TransferRowFrom.AH_LocalExTaxAmount;
			}
		}

		void UpdateLocalAmounts(ZDecimal localAmount)
		{
			if (BuyCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && SellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				TransferRowFrom.AH_OSExTaxAmount = localAmount;
				TransferRowTo.AH_OSExTaxAmount = localAmount;
			}
			else if (BuyCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				TransferRowFrom.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, GetSellAmount());
				TransferRowTo.AH_OSExTaxAmount = localAmount;
			}
			else
			{
				if (SellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					TransferRowFrom.AH_OSExTaxAmount = localAmount;
				}
				else
				{
					TransferRowFrom.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, GetSellAmount());
				}

				TransferRowTo.AH_OSExTaxAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, GetBuyExchangeRate(), BuyCurrency);
			}

			TransferRowTo.AH_LocalExTaxAmount = localAmount;
			TransferRowFrom.AH_InvoiceAmount = TransferRowTo.AH_InvoiceAmount * -1;
		}

		public BankTransfer BankTransfer { get; }

		BankTransferFromRow TransferRowFrom => BankTransfer.TransferRowFrom;

		BankTransferToRow TransferRowTo => BankTransfer.TransferRowTo;

		ZString SellCurrency => BankTransfer.SellCurrency;

		ZString BuyCurrency => BankTransfer.BuyCurrency;
	}
}
