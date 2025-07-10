using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is used by reflection in CurrencyToWords.cs.")]
	public abstract class CurrencyConvertor
	{
		public CurrencyConvertor()
		{
			Factory = new BusinessObjectFactory();
		}

		public abstract string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None);

		protected long MajorUnitsInAmount(double amount)
		{
			return Convert.ToInt64(Math.Floor(amount));
		}

		protected long MinorUnitsInAmount(double amount, int ratio)
		{
			return Convert.ToInt64((amount - MajorUnitsInAmount(amount)) * ratio);
		}

		protected string PaddedMinorUnitsInAmount(double amount, int ratio)
		{
			string result = MinorUnitsInAmount(amount, ratio).ToString();
			int length = ratio.ToString().Length - 1;
			while (result.Length < length)
			{
				result = "0" + result;
			}
			return result;
		}

		protected readonly BusinessObjectFactory Factory;

		public enum SpecialFormat
		{
			None,
			///supports minor unit in words
			LTR,
			SAF
		}
	}
}
