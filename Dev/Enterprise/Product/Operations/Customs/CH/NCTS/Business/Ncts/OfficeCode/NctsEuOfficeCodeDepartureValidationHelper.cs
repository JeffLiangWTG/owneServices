using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public static class NctsEuOfficeCodeDepartureValidationHelper
{
	public static void CheckEstimatedNumberOfDays(NctsEuOfficeCode parent)
	{
		if (!parent.EstimatedNumberOfDays.IsEmpty && (!ZInt.TryParse(parent.EstimatedNumberOfDays, out var value) || value < 0))
		{
			parent.EstimatedNumberOfDaysInfo.AddError(Res.GetString("162EADA2-0FC9-4F3E-A580-72ECE5871A75", "Estimated Days must be empty or a number between 0 and 99."));
		}
	}
}
