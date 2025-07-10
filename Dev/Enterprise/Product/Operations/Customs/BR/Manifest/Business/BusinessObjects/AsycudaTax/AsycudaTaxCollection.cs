namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaTaxCollection : ASYCUDA.Business.AsycudaTaxCollection<AsycudaTax, AsycudaBill>
	{
		public AsycudaTaxCollection(AsycudaBill master) : base(master)
		{
		}
	}
}
