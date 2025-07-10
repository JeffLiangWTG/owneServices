using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public static class ValidationHelper
{
	public static void CheckWithinRange(ZPropertyInfoString propertyInfo, int minimumAcceptableValue, int maximumAcceptableValue)
	{
		if (!propertyInfo.Value.IsEmpty && (!int.TryParse(propertyInfo.Value, out var num) || num < minimumAcceptableValue || num > maximumAcceptableValue))
		{
			propertyInfo.AddMessageError(ValidationMessages.Shared.GetFieldIsNotWithinRangeMessage(propertyInfo.HumanReadableName, minimumAcceptableValue, maximumAcceptableValue));
		}
	}
}
