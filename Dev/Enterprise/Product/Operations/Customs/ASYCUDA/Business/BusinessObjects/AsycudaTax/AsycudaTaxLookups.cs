namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTaxLookups : ManifestBase.AsycudaTaxLookups
	{
		public AsycudaTaxLookups(AsycudaTax parent)
			: base(parent)
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;
	}
}
