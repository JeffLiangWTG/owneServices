
namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaPackValidation : EU.H7.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(ASYCUDA.Business.AsycudaPack parent)
			: base(parent)
		{
		}

		protected override bool AllowNonAlphanumericCharactersForDescription => true;
	}
}
