using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public static class FatturaElettronicaXmlValueFormatter
	{
		public static ZString GetFormattedTransactionNumber(this ZString transactionNum)
		{
			var formattedTransactionNumber = transactionNum;
			if (transactionNum.Length > 20)
			{
				formattedTransactionNumber = transactionNum.Substring(0, 20);
			}
			return formattedTransactionNumber.EnsureComplianceWithBasicLatin();
		}

		/// <summary>
		/// Converts a number to base36, padded to 5 characters.
		/// The filename produced in EsterometroXmlWriter only supports 5 characters, so this restricts the values to fit in that range.
		/// </summary>
		public static string ProgressiveNumberToBase36(this int number)
		{
			if (number < 0 || number > maxSupportedBase36Number)
			{
				throw new ArgumentOutOfRangeException(nameof(number), number, FormattableString.Invariant($"Only numbers between 0 and {maxSupportedBase36Number} are supported, to fit in 5 characters."));
			}

			// CC BY-SA 3.0 Citation: https://stackoverflow.com/a/923814
			var value = number;
			var result = string.Empty;
			const int targetBase = 36;
			do
			{
				result = base36Alphabet[value % targetBase] + result;
				value = value / targetBase;
			} while (value > 0);

			while (result.Length < 5)
			{
				result = "0" + result;
			}
			return result;
		}
		const string base36Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const int maxSupportedBase36Number = 60466175;     // Maximum which can be represented in 5 characters of base36.

		public static string EnsureComplianceWithBasicLatinAndLatin1Supplement(this object input)
		{
			return RemoveSpecialCharacters(input, 0xFF);
		}

		public static string EnsureComplianceWithBasicLatin(this object input)
		{
			return RemoveSpecialCharacters(input, 0x7F);
		}

		public static string RemoveSpecialCharacters(this object input, int unicodeCutoff)
		{
			string res = null;
			if (input != null && (input is ZString || input is string))
			{
				var inputValue = new ZString(input);
				foreach (char x in inputValue)
				{
					int unicodeVal = x;
					if (unicodeVal > unicodeCutoff)
					{
						unicodeVal = ' ';
					}
					res += (char)unicodeVal;
				}
			}
			return res;
		}

		public static string ToDateType(this ZDateTime? date)
		{
			return date.HasValue ? date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
		}

		public static string ToDateType(this ZDateTime date)
		{
			ZDateTime? dateToFormat = date;
			return dateToFormat.ToDateType();
		}

		public static string GetItalyPostCode(ZString? source)
		{
			var result = "00000";

			if (source.HasValue && new Regex("^[0-9]+$").IsMatch(source.Value))
			{
				var value = source.Value;
				result = (value.Length < 5) ? value.PadLeft(5, '0') : value.Substring(0, 5);
			}

			return result;
		}

		public static string ToAmountDecimalType(this ZDecimal? source, TransactionInfo transactionInfo = null)
		{
			string result = null;

			if (source.HasValue)
			{
				var multiplier = ((transactionInfo?.TransactionType == TransactionType.CRD || (transactionInfo?.TransactionType == TransactionType.ADJ && transactionInfo?.OSTotal < 0)) ? -1 : 1);
				var sourceWithMultiplier = (ZDecimal)(source.Value * multiplier);
				result = ToDecimalType(sourceWithMultiplier);
			}

			return result;
		}

		public static string ToAmountDecimalType(this ZDecimal source, TransactionInfo transactionInfo = null)
		{
			return ((ZDecimal?)source).ToAmountDecimalType(transactionInfo);
		}

		public static string ToRateDecimalType(this ZDecimal? source)
		{
			return source.HasValue ? ToDecimalType(source.Value) : null;
		}

		public static string ToRateDecimalType(this ZDecimal source)
		{
			return ((ZDecimal?)source).ToRateDecimalType();
		}

		static string ToDecimalType(ZDecimal source)
		{
			return (source.DecimalPlaces < 2) ?
					source.ToString("0.00", CultureInfo.InvariantCulture) :
					source.ToString("0.###########################", CultureInfo.InvariantCulture);
		}
	}
}
