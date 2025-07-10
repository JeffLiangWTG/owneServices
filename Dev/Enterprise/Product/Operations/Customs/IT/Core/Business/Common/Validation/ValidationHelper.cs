using CargoWise.Common;
using CargoWise.EntityFramework;
using static Enterprise.Customs.IT.Business.SADConstants;

namespace Enterprise.Customs.IT.Business;

public static class ValidationHelper
{
	public static void ValidateDefermentAccountNumber(ZPropertyInfoString defermentAccountNumberPtyInfo)
	{
		Argument.NotNull(defermentAccountNumberPtyInfo, nameof(defermentAccountNumberPtyInfo));

		if (defermentAccountNumberPtyInfo.Value.Length > CustomsFieldMaxLength.AuthorisationReferenceWithCin)
		{
			defermentAccountNumberPtyInfo.AddMessageError(ValidationCaptions.Shared.GetFieldExceedsCustomsMaxLengthCaption(CustomsFieldMaxLength.AuthorisationReferenceWithCin));
		}
	}
}
