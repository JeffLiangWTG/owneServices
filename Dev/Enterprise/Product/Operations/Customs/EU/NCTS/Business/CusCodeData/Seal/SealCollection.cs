namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SealCollection<T> : SealCollection where T : Seal
	{
		public SealCollection(NctsHeader master) : base(master)
		{
		}

		public SealCollection(NctsArrivalMovementHeader master) : base(master)
		{
		}

		public new T AddNew() => (T)base.AddNew();

		public new T this[int index] => (T)base[index];
	}
}
