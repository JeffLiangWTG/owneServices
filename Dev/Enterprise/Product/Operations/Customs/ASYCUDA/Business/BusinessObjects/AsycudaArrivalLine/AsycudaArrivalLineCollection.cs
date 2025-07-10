namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalLineCollection<B> : AsycudaArrivalLineCollection
		where B : AsycudaArrivalLine
	{
		public AsycudaArrivalLineCollection(AsycudaArrivalHeader master)
			: base(master)
		{
		}

		public new B this[int i] => (B)base[i];
		public new B AddNew() => (B)base.AddNew();
	}
}
