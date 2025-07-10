using CargoWise.EntityFramework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegHeaderValidation : AutoCusTempStorageRegHeaderValidation
{
	public CusTempStorageRegHeaderValidation(AutoCusTempStorageRegHeader parent) : base(parent)
	{
	}

	protected override void CheckSRH_CustomsOffice()
	{
		base.CheckSRH_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCode(Parent.SRH_CustomsOfficeInfo);
	}
}
