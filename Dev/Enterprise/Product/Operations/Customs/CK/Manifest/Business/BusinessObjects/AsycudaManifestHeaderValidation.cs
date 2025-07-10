using CargoWise.EntityFramework;

namespace Enterprise.Customs.CK.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected override void CheckAMA_RN_NKConveyanceNationalityCore()
		{
			if (Parent.IsSea || Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
			}
		}
	}
}
