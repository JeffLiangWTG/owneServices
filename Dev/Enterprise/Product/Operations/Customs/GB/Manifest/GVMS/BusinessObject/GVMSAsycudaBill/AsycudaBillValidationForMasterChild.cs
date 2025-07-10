using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.GVMS
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckMandatoryABL_E_DEP()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_DEPInfo);
		}

		protected override void CheckMandatoryABL_E_ARV()
		{
		}

		protected override void CheckABL_BillNumber()
		{
			// manifest number not required
		}
	}
}
