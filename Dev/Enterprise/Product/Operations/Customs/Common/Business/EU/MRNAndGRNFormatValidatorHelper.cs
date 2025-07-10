using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Common.EU
{
	public static class MRNAndGRNFormatValidatorHelper
	{
		public static (bool isGRNDigitValid, ZInt calculatedCheckDigit) IsGRNDigitValid(ZString grn) => ReferenceNumberCheckDigit(grn, CalculatedTotalGRNValue(grn));

		public static (bool isMRNDigitValid, ZInt calculatedCheckDigit) IsMRNDigitValid(ZString mrn) => ReferenceNumberCheckDigit(mrn, CalculatedTotalMRNValue(mrn));

		static (bool isValid, ZInt calculatedCheckDigit) ReferenceNumberCheckDigit(ZString referenceNumber, ZInt calculatedResult)
		{
			var expectedCheckDigit = referenceNumber[referenceNumber.Length - 1].ToString();

			calculatedResult %= 11;
			if (calculatedResult == 10)
			{
				calculatedResult = 0;
			}

			return (expectedCheckDigit.Equals(calculatedResult.ToString()), calculatedResult);
		}

		public static bool IsCountryCodeValid(ZString countryCode, BusinessObjectFactory factory)
		{
			return factory.LoadFromNaturalKey<IRefCountry>(ZArchitecture.Schema.RefCountrySchema.RN_Code, countryCode) != null;
		}

		static ZInt CalculatedTotalGRNValue(ZString grn) => CalculatedTotalValue(grn, 16);

		static ZInt CalculatedTotalMRNValue(ZString mrn) => CalculatedTotalValue(mrn, 17);

		static ZInt CalculatedTotalValue(ZString value, int length)
		{
			var totalCalculatedResult = 0;
			var position = 0;
			var valueWithoutCheckDigit = value.SubstringSafe(0, length);

			foreach (var c in valueWithoutCheckDigit)
			{
				var factor = System.Math.Pow(2, position);
				totalCalculatedResult += (Dictionary[c] * System.Convert.ToInt32(factor));
				position++;
			}

			return totalCalculatedResult;
		}

		static IReadOnlyDictionary<char, int> Dictionary => dictionary.Value;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static Lazy<IReadOnlyDictionary<char, int>> dictionary => new Lazy<IReadOnlyDictionary<char, int>>(() =>
			new Dictionary<char, int>
				{
					{ '0', 0 },
					{ '1', 1 },
					{ '2', 2 },
					{ '3', 3 },
					{ '4', 4 },
					{ '5', 5 },
					{ '6', 6 },
					{ '7', 7 },
					{ '8', 8 },
					{ '9', 9 },
					{ 'A', 10 },
					{ 'B', 12 },
					{ 'C', 13 },
					{ 'D', 14 },
					{ 'E', 15 },
					{ 'F', 16 },
					{ 'G', 17 },
					{ 'H', 18 },
					{ 'I', 19 },
					{ 'J', 20 },
					{ 'K', 21 },
					{ 'L', 23 },
					{ 'M', 24 },
					{ 'N', 25 },
					{ 'O', 26 },
					{ 'P', 27 },
					{ 'Q', 28 },
					{ 'R', 29 },
					{ 'S', 30 },
					{ 'T', 31 },
					{ 'U', 32 },
					{ 'V', 34 },
					{ 'W', 35 },
					{ 'X', 36 },
					{ 'Y', 37 },
					{ 'Z', 38 }
				});
	}
}
