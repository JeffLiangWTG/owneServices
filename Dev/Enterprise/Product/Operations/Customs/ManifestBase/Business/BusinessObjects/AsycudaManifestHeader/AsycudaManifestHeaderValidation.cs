namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaManifestHeaderValidation : AutoAsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AutoAsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;
	}
}
