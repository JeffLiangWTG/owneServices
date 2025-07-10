using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	public static class DeclarationVersionIDConverter
	{
		public static ZShort ToEntryHeaderVersionID(string declarationVersionID)
		{
			if (ZDecimal.TryParse(declarationVersionID, out var zDecimal)
				&& IsPositive(zDecimal)
				&& ZShort.TryParse((zDecimal * Multiplier).ToString(FixedPointNoDecimals), out var result))
			{
				return result;
			}

			return ZShort.Zero;
		}

		[CodeAlive("Will be used in the future")]
		public static string ToDeclarationVersionID(ZShort entryHeaderVersionID)
		{
			var integerPart = entryHeaderVersionID / Multiplier;
			var fractionalPart = entryHeaderVersionID % Multiplier;
			var fractionalPartString = fractionalPart.ToString().Trim('0');

			return string.IsNullOrEmpty(fractionalPartString)
				? $"{integerPart}.0"
				: $"{integerPart}.{fractionalPartString}";
		}

		static bool IsPositive(ZDecimal version)
		{
			return version >= 0;
		}

		const int Multiplier = 1000;
		const string FixedPointNoDecimals = "F0";
	}
}
