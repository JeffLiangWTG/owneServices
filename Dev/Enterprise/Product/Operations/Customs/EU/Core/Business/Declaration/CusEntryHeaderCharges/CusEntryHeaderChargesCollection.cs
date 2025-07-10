namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderChargesCollection<T> : Customs.Business.CusEntryHeaderChargesCollection<T>
		where T : CusEntryHeaderCharges
	{
		public CusEntryHeaderChargesCollection(CusEntryHeader entryHeader)
			: base(entryHeader)
		{ }
	}
}
