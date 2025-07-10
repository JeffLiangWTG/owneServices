namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GbEDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public GbEDIMessageCollection(CusEntryHeader entry) : base(entry)
		{
		}

		public new GbEDIMessage this[int index] => (GbEDIMessage)base[index];

		public new GbEDIMessage AddNew() => (GbEDIMessage)base.AddNew();
	}
}
