using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaBillValidationForMasterChild : EU.H7.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateABL_ShipmentType();
		}

		protected override void CheckABL_ShipmentType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ABL_ShipmentTypeInfo);
		}
	}
}
