namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalHeaderLookups : ManifestBase.AsycudaArrivalHeaderLookups
	{
		public AsycudaArrivalHeaderLookups(AsycudaArrivalHeader parent) : base(parent)
		{
		}

		public new AsycudaArrivalHeader Parent => (AsycudaArrivalHeader)base.Parent;
	}
}
