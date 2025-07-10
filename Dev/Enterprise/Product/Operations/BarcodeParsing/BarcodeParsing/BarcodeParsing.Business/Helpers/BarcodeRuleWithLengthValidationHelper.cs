using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public static class BarcodeRuleWithLengthValidationHelper
	{
		public static void CheckMinLength(IBarcodeRuleWithLength barcodeRuleWithLength)
		{
			MandatoryValidation.CheckNotNegative(barcodeRuleWithLength.MinLengthInfo);
			CheckMinLengthIsLessThanMaxLength(barcodeRuleWithLength);
			CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero(barcodeRuleWithLength, barcodeRuleWithLength.MinLengthInfo);
			CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny(barcodeRuleWithLength, barcodeRuleWithLength.MinLengthInfo);
			CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange(barcodeRuleWithLength, barcodeRuleWithLength.MinLengthInfo);
		}

		public static void CheckMaxLength(IBarcodeRuleWithLength barcodeRuleWithLength)
		{
			MandatoryValidation.CheckNotNegative(barcodeRuleWithLength.MaxLengthInfo);
			CheckMaxLengthIsGreaterThanMinLength(barcodeRuleWithLength);
			CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero(barcodeRuleWithLength, barcodeRuleWithLength.MaxLengthInfo);
			CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny(barcodeRuleWithLength, barcodeRuleWithLength.MaxLengthInfo);
			CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange(barcodeRuleWithLength, barcodeRuleWithLength.MaxLengthInfo);
		}

		static void CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny(IBarcodeRuleWithLength barcodeRuleWithLength, ZPropertyInfo info)
		{
			if (!info.HasErrors() && !barcodeRuleWithLength.IsLengthTypeAny() && (ZShort)info.Value == 0)
			{
				info.AddError(Res.GetString("a5bdf238-ab27-4a5a-a7ea-1675a1e1ebd1", "Min and Max Length should be greater than zero if Length Type is not Any."));
			}
		}

		static void CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange(IBarcodeRuleWithLength barcodeRuleWithLength, ZPropertyInfo info)
		{
			if (!info.HasErrors() && barcodeRuleWithLength.IsLengthTypeRange() && barcodeRuleWithLength.MinLength >= barcodeRuleWithLength.MaxLength)
			{
				info.AddError(Res.GetString("d0d61791-4189-490e-b8b1-f30dddf24f96", "Max Length should be greater than Min Length if Length Type is Range."));
			}
		}

		static void CheckMinLengthIsLessThanMaxLength(IBarcodeRuleWithLength barcodeRuleWithLength)
		{
			if (!barcodeRuleWithLength.MinLengthInfo.HasErrors() && barcodeRuleWithLength.MinLength > barcodeRuleWithLength.MaxLength)
			{
				barcodeRuleWithLength.MinLengthInfo.AddError(Res.GetString("e002bb43-2f51-4f8f-a87d-1456e6d932b1", "Min Length cannot be greater than Max Length."));
			}
		}

		static void CheckMaxLengthIsGreaterThanMinLength(IBarcodeRuleWithLength barcodeRuleWithLength)
		{
			if (!barcodeRuleWithLength.MaxLengthInfo.HasErrors() && barcodeRuleWithLength.MaxLength < barcodeRuleWithLength.MinLength)
			{
				barcodeRuleWithLength.MaxLengthInfo.AddError(Res.GetString("906d0de4-4464-4ffe-969c-34690900cb43", "Max Length cannot be less than Min Length."));
			}
		}

		static void CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero(IBarcodeRuleWithLength barcodeRuleWithLength, ZPropertyInfo info)
		{
			if (!info.HasErrors() && barcodeRuleWithLength.MaxLength > 0 && barcodeRuleWithLength.MinLength <= 0)
			{
				info.AddError(Res.GetString("3781207d-ad55-44cb-842c-a9b989b88064", "Min Length should be greater than zero if Max Length is greater than zero."));
			}
		}
	}
}

