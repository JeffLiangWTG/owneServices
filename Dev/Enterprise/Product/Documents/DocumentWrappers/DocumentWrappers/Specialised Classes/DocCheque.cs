using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngineCore.DocWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocCheque : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocCheque(ZString payee, ZDecimal amount, DocCurrency currency, ZInt amountColumnWidth)
		{
			fPayee = payee;
			if (amount < 0)
			{
				var message = Res.GetString("701ebfb5-4571-4d18-9aa5-72d6b5299b07", "Cheque to \'{0}\' has negative amount: {1}. Please confirm this is a valid payment.", payee, amount);
				throw new InvalidDocumentWrapperParameterException(message);
			}
			fAmount = amount;
			fCurrency = currency;
			FirstLinePaymentWidth = amountColumnWidth;
		}

		public ZString ChequePayTo
		{
			get { return fPayee; }
		}

		public ZDecimal ChequeAmount
		{
			get { return fAmount; }
		}

		public ZString ChequeAmountFormatted
		{
			get
			{
				return "***" + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(fAmount, PaymentCurrency);
			}
		}

		public DocCurrency PaymentCurrency
		{
			get { return fCurrency; }
		}

		public ZString TodaysDateSplit
		{
			get
			{
				ZString result = ZString.Empty;
				ZString date = ZDateTime.Today.Date.ToString("dd");
				ZString month = ZDateTime.Today.Date.ToString("MM");
				ZString year = ZDateTime.Today.Date.ToString("yy");

				result += date[0] + "   " + date[1] + "   ";
				result += month[0] + "   " + month[1] + "   ";
				result += year[0] + "   " + year[1];

				return result;
			}
		}

		public ZString TodaysDateSplitDDMMYYYY
		{
			get
			{
				ZString result = ZString.Empty;
				ZString date = ZDateTime.Today.Date.ToString("dd");
				ZString month = ZDateTime.Today.Date.ToString("MM");
				ZString year = ZDateTime.Today.Date.ToString("yyyy");

				result += date[0] + " " + date[1] + " ";
				result += month[0] + " " + month[1] + " ";
				result += year[0] + " " + year[1] + " " + year[2] + " " + year[3];

				return result;
			}
		}

		public ZString ChequeAmountInWords
		{
			get
			{
				ZString result = ZString.Empty;
				if (PaymentCurrency != null)
				{
					result = (new CurrencyToWords_EN()).ConvertToWords(Convert.ToDouble(ChequeAmount), PaymentCurrency.Code);
				}
				return result.ToUpper();
			}
		}

		public ZString FirstLineAmountInWords
		{
			get { return ChequeAmountInWordsWrapped.Split('\n')[0]; }
		}

		public ZString SecondLineAmountInWords
		{
			get
			{
				if (ChequeAmountInWordsWrapped.Split('\n').Length > 1)
				{
					return ChequeAmountInWords.SubstringSafe(FirstLineAmountInWords.Length).Trim();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString FirstLineChequePayTo
		{
			get { return ChequePayToWrapped.Split('\n')[0]; }
		}

		public ZString SecondLineChequePayTo
		{
			get
			{
				if (ChequePayToWrapped.Split('\n').Length > 1)
				{
					return ChequePayToWrapped.SubstringSafe(FirstLineChequePayTo.Length).Trim();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#region Number of Powers of Ten

		public ZString TenMillions
		{
			get { return NumberOfPowersOfTen(7); }
		}

		public ZString Millions
		{
			get { return NumberOfPowersOfTen(6); }
		}

		public ZString HundredThousands
		{
			get { return NumberOfPowersOfTen(5); }
		}

		public ZString TenThousands
		{
			get { return NumberOfPowersOfTen(4); }
		}

		public ZString Thousands
		{
			get { return NumberOfPowersOfTen(3); }
		}

		public ZString Hundreds
		{
			get { return NumberOfPowersOfTen(2); }
		}

		public ZString Tens
		{
			get { return NumberOfPowersOfTen(1); }
		}

		public ZString Ones
		{
			get { return NumberOfPowersOfTen(0); }
		}

		public ZString Cents
		{
			get
			{
				ZString amount = ChequeAmount.ToString(2);
				return amount.Right(2);
			}
		}

		#endregion

		#region Implementation

		protected ZString fTransactionType;
		protected ZString fPayee;
		protected ZDecimal fAmount;
		protected DocCurrency fCurrency;
		protected ZInt FirstLinePaymentWidth;
		protected ZInt FirstLinePayToWidth = 20;
		ZString fChequeAmountInWordsWrapped;
		ZString fChequePayToWrapped;

		protected ZString ChequeAmountInWordsWrapped
		{
			get
			{
				if (fChequeAmountInWordsWrapped.IsEmpty)
				{
					fChequeAmountInWordsWrapped = WrapLongTextToTwoLines(ChequeAmountInWords, FirstLinePaymentWidth);
				}
				return fChequeAmountInWordsWrapped;
			}
		}

		protected ZString ChequePayToWrapped
		{
			get
			{
				if (fChequePayToWrapped.IsEmpty)
				{
					fChequePayToWrapped = WrapLongTextToTwoLines(ChequePayTo, FirstLinePayToWidth);
				}
				return fChequePayToWrapped;
			}
		}

		protected ZString WrapLongTextToTwoLines(ZString text, ZInt allowedWidth)
		{
			if (text.Length > allowedWidth)
			{
				if (text[allowedWidth] == ' ')
				{
					return text.Insert(allowedWidth, "\n");
				}
				else
				{
					ZInt i = allowedWidth;
					for (; i >= 0; i--)
					{
						if (text[i] == ' ')
						{
							return text.Insert(i, "\n");
						}
					}
				}
			}

			return text;
		}

		protected ZString NumberOfPowersOfTen(int power)
		{
			double adjustedAmount = Math.Floor((double)ChequeAmount / Math.Pow(10, power));
			string amountInWords = Res.GetString("df1dd6aa-d5d7-4c1d-956f-276dd95b5381", "zero");
			int units = (int)(adjustedAmount % 10);
			if (units != 0)
			{
				amountInWords = NumberToString_EN.ConvertNumberToWords(units);
			}
			return amountInWords;
		}

		#endregion
	}
}
