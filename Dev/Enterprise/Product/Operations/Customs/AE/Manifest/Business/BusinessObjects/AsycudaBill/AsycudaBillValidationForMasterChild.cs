using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
{
	public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
	{
	}

	protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

	protected new AsycudaManifestHeader Header => Parent.Header;

	protected override void CheckABL_E_ARV()
	{
	}

	protected override bool IsABL_E_DEPRequired => false;

	protected override void CheckABL_BillIssueDate()
	{
		base.CheckABL_BillIssueDate();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);

		if (Parent.ABL_BillIssueDate > ZDateTime.Today)
		{
			Parent.ABL_BillIssueDateInfo.AddWarning(Res.GetString("E45EC8BF-CD59-4A04-B48B-DB962F4962C1", "The Issue Date cannot be in the future."));
		}
	}
}
