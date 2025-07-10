using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public static class CommonValidation
	{
		public static void CheckGreaterThanZero(ZPropertyInfo targetInfo)
		{
			MandatoryValidation.CheckNotNegative(targetInfo);
			MandatoryValidation.MessageErrorIfIsZero(targetInfo);
		}

		public static void CheckValueShouldGreaterThanOrEqualTo(ZPropertyInfo greaterInfo, ZPropertyInfo smallerInfo, ZPropertyInfo targetInfo)
		{
			if (greaterInfo.Value is INumericZType greaterValue && smallerInfo.Value is INumericZType smallerValue && greaterValue.CompareTo(smallerValue) < 0)
			{
				targetInfo.AddMessageError(Res.GetString(
					"9A71C925-DFB7-4DE0-B6B7-2B43794A10CB",
					"{0} should be greater than or at least equal to {1}.",
					greaterInfo.HumanReadableName,
					smallerInfo.HumanReadableName
				));
			}
		}

		public static string GetShouldBePositiveOrShouldBeRemovedMessage(ZPropertyInfo targetInfo) => Res.GetString(
			"7E4E2190-3E3B-4843-9BE5-5F14EF4B4315", "{0} should be greater than Zero or this {1} should be removed from selection.", targetInfo.HumanReadableName, targetInfo.BizObj.HumanReadableName
		);
	}
}
