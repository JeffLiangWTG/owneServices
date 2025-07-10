namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class MessageChooserItemCollection<T> : MessageChooserItemCollection
		where T : MessageChooserItem
	{
		public new T this[int i] => (T)base[i];

		public new T AddNew() => (T)base.AddNew();
	}
}
