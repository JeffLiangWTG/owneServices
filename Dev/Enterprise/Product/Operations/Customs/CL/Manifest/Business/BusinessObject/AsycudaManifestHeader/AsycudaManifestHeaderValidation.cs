using CargoWise.EntityFramework;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAMA_TransshipmentType();
		}

		public void ValidateAMA_TransshipmentType()
		{
			ValidateCalculatedProperty(Parent.AMA_TransshipmentTypeInfo);
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
		}

		protected void CheckAMA_TransshipmentType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_TransshipmentTypeInfo);
		}

		protected override void MandatoryCheckOfCustomsOffice()
		{
		}
	}
}
