using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public abstract class SouthAsiaNumberToString : INumberToWords
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
			else if (number < lakh)
			{
				string thousandsPlace = string.Format(number / 1000 > 1 ? manyThousandPlacePattern : oneThousandPlacePattern, GetNumberAsString(number / 1000));
				result = number % 1000 > 0 ? string.Format(thousandsPattern, thousandsPlace, GetNumberAsString(number % 1000)) : thousandsPlace;
			}
			else if (number < crore)
			{
				string lakhsPlace = string.Format(number / lakh > 1 ? manyLakhPlacePattern : oneLakhPlacePattern, GetNumberAsString(number / lakh));
				result = number % lakh > 0 ? string.Format(LakhsPattern, lakhsPlace, GetNumberAsString(number % lakh)) : lakhsPlace;
			}
			else if (number < trillion)
			{
				string croresPlace = string.Format(number / crore > 1 ? manyCrorePlacePattern : oneCrorePlacePattern, GetNumberAsString(number / crore));
				result = number % crore > 0 ? string.Format(CroresPattern, croresPlace, GetNumberAsString(number % crore)) : croresPlace;
			}
			else
			{
				throw new NotImplementedException("Number over one trillion");
			}

			return result;
		}

		protected string GetHundredsPlace(long number)
		{
			return hundreds != null ? hundreds[number / 100] : string.Format(number / 100 > 1 ? manyHundredsPlacePattern : oneHundredsPlacePattern, GetNumberAsString(number / 100));
		}

		protected const long lakh = 100000;
		protected const long crore = 10000000;
		protected const long trillion = 1000000000000;

		protected string[] primitiveNumbers;
		protected string[] tens;
		protected string[] hundreds;
		protected string oneHundredsPlacePattern;
		protected string manyHundredsPlacePattern;

		protected string tensPattern;
		protected string hundredsPattern;
		protected string oneThousandPlacePattern;
		protected string manyThousandPlacePattern;
		protected string thousandsPattern;
		protected string oneLakhPlacePattern;
		protected string manyLakhPlacePattern;
		protected string LakhsPattern;
		protected string oneCrorePlacePattern;
		protected string manyCrorePlacePattern;
		protected string CroresPattern;

		protected Dictionary<long, string> specialNumbers;

		abstract public string DecimalSeperatorAsString { get; }
	}
}
