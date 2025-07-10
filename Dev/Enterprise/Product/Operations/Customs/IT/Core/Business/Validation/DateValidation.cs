using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Validation;

public static class DateValidation
{
	public static void MessageErrorIfDateIsInFuture(ZPropertyInfo propertyInfo)
	{
		if (propertyInfo.Value.IsEmpty)
		{
			return;
		}

		if (propertyInfo.Value is ZDateTime dateTime && dateTime.Date > ZDateTime.Today)
		{
			var fieldFromPropertyInfo = (ZString)MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
			var errorMessage = Res.GetString("6599002E-FFC6-434C-9D89-6C43EB71FEDA", "{0} is greater than today's date", fieldFromPropertyInfo);
			propertyInfo.AddMessageError(errorMessage);
		}
	}
}
