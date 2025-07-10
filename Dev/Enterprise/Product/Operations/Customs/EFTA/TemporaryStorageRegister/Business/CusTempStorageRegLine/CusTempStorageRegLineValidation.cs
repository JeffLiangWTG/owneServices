using CargoWise.EntityFramework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineValidation : AutoCusTempStorageRegLineValidation
{
	public CusTempStorageRegLineValidation(AutoCusTempStorageRegLine parent) : base(parent)
	{
	}

	protected override void CheckSRL_LineNumber()
	{
		base.CheckSRL_LineNumber();
		MandatoryValidation.CheckNotZero(Parent.SRL_LineNumberInfo);
		MandatoryValidation.CheckNotNegative(Parent.SRL_LineNumberInfo);
	}

	protected override void CheckSRL_PackagesRemaining()
	{
		base.CheckSRL_PackagesRemaining();
		if (Parent.SRL_PackagesRemaining < 0)
		{
			Parent.SRL_PackagesRemainingInfo.AddError(Res.GetString("A0DEFC9B-712A-4FB3-BA5D-7186C131873E", "Packages Remaining should not be negative after calculating based on all the transactions Package Quantity."));
		}
	}
}
