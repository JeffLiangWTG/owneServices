using System;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public static class DecimalNumberToStringConvertor
	{
		public static string ConvertDecimalToString(decimal inputNumber, string lang)
		{
			if (string.IsNullOrEmpty(lang) || Res.IsEnglish(lang))
			{
				lang = Res.DefaultLanguage;
			}

			var rationalPart = inputNumber - Math.Floor(inputNumber);

			while ((rationalPart - Math.Floor(rationalPart)) != 0)
			{
				rationalPart *= 10;
			}

			var converterType = Type.GetType("Enterprise.DocumentEngine.MacroValueProviders.Utilities.NumberToString_" + lang.Replace('-', '_'), false);

			if (converterType == null)
			{
				if (rationalPart == 0 && inputNumber > 0 && inputNumber <= 10)
				{
					return new CodeDescriptionPairList(OLookUpEditType.Numbers1To10).GetDescriptionFromCode(inputNumber.ToString(CultureInfo.InvariantCulture));
				}

				return string.Empty;
			}

			var numberConverter = Activator.CreateInstance(converterType) as INumberToWords;
			var result = numberConverter.GetNumberAsString(Convert.ToInt64(Math.Floor(inputNumber)));

			if (rationalPart > 0)
			{
				result += " " + numberConverter.DecimalSeperatorAsString + " " + numberConverter.GetNumberAsString(Convert.ToInt64(rationalPart));
			}

			return result;
		}
	}
}
