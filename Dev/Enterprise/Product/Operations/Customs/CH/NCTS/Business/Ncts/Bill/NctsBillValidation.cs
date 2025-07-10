namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsBillValidation : EU.NCTS.Business.NctsBillValidation
{
	public NctsBillValidation(EU.NCTS.Business.NctsBill parent) : base(parent)
	{
	}

	new NctsBill Parent => (NctsBill)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		PassarValidation.CheckNP70254(Parent);
	}

	protected override void CheckB0_WeightErrorIfNotEntered(EU.NCTS.Business.NctsBill parent)
	{
	}
}
