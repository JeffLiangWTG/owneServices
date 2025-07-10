using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.BR
{
	public static class BRCusEntryNumValidationHelper
	{
		public static bool CheckValidDUEEntryNumberFormat(ZString entryNumberDUE)
		{
			return entryNumberDUE.Length == 14 && Regex.IsMatch(entryNumberDUE, @"^[0-9]{2}BR[0-9]{9}[0-9]$");
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design")]
		public static bool CheckValidDUEEntryNumberCheckDigit(ZString entryNumberDUE, out int correctCheckDigit)
		{
			correctCheckDigit = GetDUEEntryNumberCheckDigit(entryNumberDUE);
			return entryNumberDUE.EndsWith(correctCheckDigit.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
		}

		static int GetDUEEntryNumberCheckDigit(ZString entryNumberDUE)
		{
			var numbers = FormatDUEForCheckDigitCalculation(entryNumberDUE);
			return GetModulusElevenCheckDigit(numbers);
		}

		#region Implementation

		static int[] FormatDUEForCheckDigitCalculation(ZString entryNumberDUE)
		{
			return entryNumberDUE
				.Left(entryNumberDUE.Length - 1)
				.KeepNumericCharacters()
				.ToString()
				.Select(x => (int)char.GetNumericValue(x))
				.ToArray();
		}

		static int GetModulusElevenCheckDigit(int[] numbers)
		{
			int sum = 0;
			int numberPosition = numbers.Length + 1;
			foreach (var number in numbers)
			{
				sum += number * numberPosition--;
			}

			int checkDigit = 11 - sum % 11;
			return checkDigit >= 10 ? 0 : checkDigit;
		}

		#endregion

		public static bool CheckValidMasterUCREntryNumberFormat(ZString entryNumberMasterUCR)
		{
			return Regex.IsMatch(entryNumberMasterUCR, @"^[0-9]BR[0-9]{11}[0-9][a-zA-Z0-9]{20}$") || Regex.IsMatch(entryNumberMasterUCR, @"^[0-9]BR[0-9]{8}[0-9][a-zA-Z0-9]{23}$");
		}

		public static MultilingualString GetUCREntryNumberInvalidMessage(string propertyName) => ResString.GetMultilingualString("F2148C93-5F9A-4A54-905C-6ED07219F4D0", "The entered {0} does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.", propertyName);
	}
}
