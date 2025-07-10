namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusSealValidation : Customs.Business.CusSealValidation
{
	public CusSealValidation(CusSeal parent) : base(parent)
	{
	}

	protected new CusSeal Parent => (CusSeal)base.Parent;

	protected override void CheckBK_SealNumber()
	{
		base.CheckBK_SealNumber();
		SealNumberValidation.ValidateSealNumber(Parent.BK_SealNumber, Parent.BK_SealNumberInfo);
	}
}
