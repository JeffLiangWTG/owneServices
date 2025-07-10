using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public static class IBarcodeRuleWithLengthExtensions
	{
		public static ZString CalculateLengthType(this IBarcodeRuleWithLength barcodeRuleWithLength, bool isInDatabase) => isInDatabase
			? BarcodeFormatHelper.GetLengthTypeBasedOnMinAndMaxLength(barcodeRuleWithLength.MinLength, barcodeRuleWithLength.MaxLength)
			: LengthTypes.Codes.Range;

		public static bool IsLengthTypeAny(this IBarcodeRuleWithLength barcodeRuleWithLength) => barcodeRuleWithLength.LengthType.EqualsIgnoringCase(LengthTypes.Codes.Any);

		public static bool IsLengthTypeRange(this IBarcodeRuleWithLength barcodeRuleWithLength) => barcodeRuleWithLength.LengthType.EqualsIgnoringCase(LengthTypes.Codes.Range);

		public static void SetMinAndMaxLength(this IBarcodeRuleWithLength barcodeRuleWithLength)
		{
			switch (barcodeRuleWithLength.LengthType)
			{
				case LengthTypes.Codes.Any:
					barcodeRuleWithLength.MinLength = (ZShort)0;
					barcodeRuleWithLength.MaxLength = (ZShort)0;
					break;

				case LengthTypes.Codes.Fixed:
					barcodeRuleWithLength.MinLength = barcodeRuleWithLength.MaxLength;
					break;

				case LengthTypes.Codes.Range:
				default:
					break;
			}
		}

		public static void SetLengthForLengthTypeFixed(this IBarcodeRuleWithLength barcodeRuleWithLength, ZShort value)
		{
			if (barcodeRuleWithLength.LengthType.EqualsIgnoringCase(LengthTypes.Codes.Fixed))
			{
				barcodeRuleWithLength.MinLength = value;
				barcodeRuleWithLength.MaxLength = value;
			}
		}
	}
}
