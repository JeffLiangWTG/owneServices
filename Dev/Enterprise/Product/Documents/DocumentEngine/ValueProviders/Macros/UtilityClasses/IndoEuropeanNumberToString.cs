using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public abstract class IndoEuropeanNumberToString : INumberToWords
	{
		public virtual string GetNumberAsString(long number)
		{
			String result;
			if (number < 0)
			{
				throw new FormulaProviderException("You cannot pass a negative number to the <NumberToWords> or <CurrencyToWords> macro.");
			}
			if (number < primitiveNumbers.Length)
			{
				result = primitiveNumbers[number];
			}
			else if (specialNumbers != null && specialNumbers.ContainsKey(number))
			{
				result = specialNumbers[number];
			}
			else if (number < 100)
			{
				result = number % 10 == 0 ? tens[number / 10] : string.Format(tensPattern, tens[number / 10], primitiveNumbers[number % 10]);
			}
			else if (number < 1000)
			{
				result = number % 100 == 0 ? GetHundredsPlace(number) : string.Format(hundredsPattern, GetHundredsPlace(number), GetNumberAsString(number % 100));
			}
			else if (number < million)
			{
				string hundredsPlace = string.Format(number / 1000 > 1 ? manyThosandPlacePattern : oneThosandPlacePattern, GetNumberAsString(number / 1000));
				result = number % 1000 > 0 ? string.Format(thousandsPattern, hundredsPlace, GetNumberAsString(number % 1000)) : hundredsPlace;
			}
			else if (number < billion)
			{
				string millionsPlace = string.Format(number / million > 1 ? manyMillionPlacePattern : oneMillionPlacePattern, GetNumberAsString(number / million));
				result = number % million > 0 ? string.Format(millionsPattern, millionsPlace, GetNumberAsString(number % million)) : millionsPlace;
			}
			else if (number < trillion)
			{
				string billionsPlace = string.Format(number / billion > 1 ? manyBillionPlacePattern : oneBillionPlacePattern, GetNumberAsString(number / billion));
				result = number % billion > 0 ? string.Format(billionsPattern, billionsPlace, GetNumberAsString(number % billion)) : billionsPlace;
			}
			else
			{
				throw new NotSupportedException(String.Format("The number {0} is over one trillion. This cannot be supported by the current version of the document engine.", number));
			}

			return result;
		}

		protected string GetHundredsPlace(long number)
		{
			return hundreds != null ? hundreds[number / 100] : string.Format(number / 100 > 1 ? manyHundredsPlacePattern : oneHundredsPlacePattern, GetNumberAsString(number / 100));
		}

		protected const long million = 1000000;
		protected const long billion = 1000000000;
		protected const long trillion = 1000000000000;

		protected string[] primitiveNumbers;
		protected string[] tens;
		protected string[] hundreds;
		protected string oneHundredsPlacePattern;
		protected string manyHundredsPlacePattern;

		protected string tensPattern;
		protected string hundredsPattern;
		protected string oneThosandPlacePattern;
		protected string manyThosandPlacePattern;
		protected string thousandsPattern;
		protected string oneMillionPlacePattern;
		protected string manyMillionPlacePattern;
		protected string millionsPattern;
		protected string oneBillionPlacePattern;
		protected string manyBillionPlacePattern;
		protected string billionsPattern;

		protected Dictionary<long, string> specialNumbers;

		abstract public string DecimalSeperatorAsString { get; }
	}
}
