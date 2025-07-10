namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		public AsycudaPackCollection(AsycudaBill master)
			: base(master)
		{
		}
	}
}
