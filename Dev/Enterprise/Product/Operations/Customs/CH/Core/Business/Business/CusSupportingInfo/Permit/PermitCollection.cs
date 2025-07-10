using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PermitCollection : CusSupportingInfoCollection<Permit>
{
	public PermitCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.Permit)
	{
		MaxCountValidationEnable(MaxNumberOfAllowedPermits, Res.GetString("64F0B654-4559-42CF-86B7-CA62D84902E5", "You are only allowed a maximum of {0} permits here.", MaxNumberOfAllowedPermits));
	}

	protected override void OnRemoved(BusinessObject bizO)
	{
		base.OnRemoved(bizO);

		Master.MarkAsNeedingValidation();
	}

	const int MaxNumberOfAllowedPermits = 9;
}
