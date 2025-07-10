namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTaxValidation : ManifestBase.AsycudaTaxValidation
	{
		public AsycudaTaxValidation(AsycudaTax parent)
			: base(parent)
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;
	}
}
