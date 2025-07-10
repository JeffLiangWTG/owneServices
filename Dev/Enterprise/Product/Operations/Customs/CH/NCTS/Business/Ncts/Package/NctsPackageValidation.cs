namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsPackageValidation : EU.NCTS.Business.NctsPackagePhase5Validation
{
	public NctsPackageValidation(NctsPackage parent) : base(parent)
	{
	}

	protected override void CheckB5_TypeOfDifference()
	{
		if (!Parent.IsPackDifference)
		{
			base.CheckB5_TypeOfDifference();
		}
	}
}
