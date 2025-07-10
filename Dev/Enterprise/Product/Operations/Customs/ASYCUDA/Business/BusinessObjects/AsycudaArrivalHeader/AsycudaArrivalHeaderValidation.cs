namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalHeaderValidation : ManifestBase.AsycudaArrivalHeaderValidation
	{
		public AsycudaArrivalHeaderValidation(AsycudaArrivalHeader parent) : base(parent)
		{
		}

		public new AsycudaArrivalHeader Parent => (AsycudaArrivalHeader)base.Parent;
	}
}
