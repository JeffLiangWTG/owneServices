namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}
	}
}
