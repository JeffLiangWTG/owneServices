using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public static class NctsValidationHelper
{
	public static void RunActionIfValidationOfType<TValidation>(this ZValidation validation, Action<TValidation> action) where TValidation : ZValidation
	{
		Argument.NotNull(action, nameof(action));

		if (validation is TValidation tValidation)
		{
			action(tValidation);
		}
	}
}
