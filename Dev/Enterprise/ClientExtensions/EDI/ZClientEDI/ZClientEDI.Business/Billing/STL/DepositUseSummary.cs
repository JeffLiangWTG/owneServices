using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DepositUseSummary
	{
		public class DepositUseLine
		{
			public DepositUseLine(string chargeCode, string currencyCode, decimal openingBalance)
			{
				ChargeCode = chargeCode;
				CurrencyCode = currencyCode;
				OpeningBalance = openingBalance;
				ClosingBalance = openingBalance;
			}

			public string CurrencyCode { get; set; }
			public decimal OpeningBalance { get; set; }
			public decimal ClosingBalance { get; set; }
			public string ChargeCode { get; private set; }
		}

		public IEnumerable<DepositUseLine> DepositUseLines { get { return chargeCodeToDepositUse != null ? chargeCodeToDepositUse.Values : Enumerable.Empty<DepositUseLine>(); } }

		readonly Dictionary<string, DepositUseLine> chargeCodeToDepositUse;

		public DepositUseSummary(IEnumerable<DepositBalance> openingBalances)
		{
			foreach (var balance in openingBalances)
			{
				if (chargeCodeToDepositUse == null)
				{
					chargeCodeToDepositUse = new Dictionary<string, DepositUseLine>();
				}
				var line = new DepositUseLine(balance.ChargeCode, balance.CurrencyCode, balance.Amount);
				chargeCodeToDepositUse.Add(line.ChargeCode, line);
			}
		}

		public void SetClosingBalance(IEnumerable<DepositBalance> closingBalances, GlbBranch invoicingBranch, CurrencyExchangeService currencyExchangeService)
		{
			foreach (var closing in closingBalances)
			{
				var line = chargeCodeToDepositUse[closing.ChargeCode];
				if (line.OpeningBalance != closing.Amount || line.CurrencyCode != closing.CurrencyCode)
				{
					decimal openingAmountInClosingCurrency = line.CurrencyCode == closing.CurrencyCode
						? line.OpeningBalance
						: currencyExchangeService.GetAmount(line.CurrencyCode, closing.CurrencyCode, invoicingBranch.Company, line.OpeningBalance);

					line.CurrencyCode = closing.CurrencyCode;
					line.OpeningBalance = openingAmountInClosingCurrency;
					line.ClosingBalance = closing.Amount;
				}
				else
				{
					chargeCodeToDepositUse.Remove(closing.ChargeCode);
				}
			}
		}

		/// <summary>
		/// Total amount of deposit use in given currency.
		/// Will be >= 0 since we only take off the deposit, not add to it.
		/// </summary>
		public decimal GetDepositUseAmount(CurrencyExchangeService currencyExchangeService, GlbBranch invoicingBranch, string currencyCode)
		{
			decimal result = 0;

			if (chargeCodeToDepositUse != null)
			{
				foreach (var depositUse in chargeCodeToDepositUse.Values)
				{
					var delta = depositUse.OpeningBalance - depositUse.ClosingBalance;
					if (delta != 0 && currencyCode != depositUse.CurrencyCode)
					{
						delta = currencyExchangeService.GetAmount(depositUse.CurrencyCode, currencyCode, invoicingBranch.Company, delta);
					}
					result += delta;
				}
			}

			return result;
		}

		public decimal GetTotalClosingBalance(CurrencyExchangeService currencyExchangeService, GlbBranch invoicingBranch, string currencyCode)
		{
			decimal result = 0;

			if (chargeCodeToDepositUse != null)
			{
				foreach (var depositUse in chargeCodeToDepositUse.Values)
				{
					if (depositUse.OpeningBalance != depositUse.ClosingBalance)
					{
						var balance = depositUse.ClosingBalance;
						if (currencyCode != depositUse.CurrencyCode)
						{
							balance = currencyExchangeService.GetAmount(depositUse.CurrencyCode, currencyCode, invoicingBranch.Company, balance);
						}

						result += balance;
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public void PopulateSummaryLines(SummaryLineCollection summaryLines, GlbBranch invoicingBranch)
		{
			if (chargeCodeToDepositUse != null)
			{
				foreach (var depositUse in chargeCodeToDepositUse.Values)
				{
					var line = summaryLines.AddNew();
					line.MainDescription = BillingInvoicingHelper.GetChargeCode(invoicingBranch, depositUse.ChargeCode).AC_Desc;
					line.Currency = depositUse.CurrencyCode;
					line.Amount1 = depositUse.OpeningBalance.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					line.Amount2 = (depositUse.ClosingBalance - depositUse.OpeningBalance).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					line.FinalAmount = depositUse.ClosingBalance.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				}
			}
		}
	}
}
