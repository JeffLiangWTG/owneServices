using CargoWise.EntityFramework;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
			}
		}
	}
}
