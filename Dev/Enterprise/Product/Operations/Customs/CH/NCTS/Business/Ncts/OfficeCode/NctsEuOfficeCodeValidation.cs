using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsEuOfficeCodeValidation : NctsEuOfficeCodePhase5Validation
{
	public NctsEuOfficeCodeValidation(NctsEuOfficeCode parent) : base(parent)
	{
	}

	new NctsEuOfficeCode Parent => (NctsEuOfficeCode)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateEstimatedNumberOfDays();
	}

	public void ValidateEstimatedNumberOfDays()
	{
		ValidateCalculatedProperty(Parent.EstimatedNumberOfDaysInfo);
	}

	protected void CheckEstimatedNumberOfDays()
	{
		NctsEuOfficeCodeDepartureValidationHelper.CheckEstimatedNumberOfDays(Parent);
	}
}
