using CargoWise.Common;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();

			var errors = Parent.CustomsOfficeRequirementHelper.Validate();
			errors.ForEach(e => Parent.AMA_CustomsOfficeInfo.AddMessageError(e));
		}
	}
}
