namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidation
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
	}
}
