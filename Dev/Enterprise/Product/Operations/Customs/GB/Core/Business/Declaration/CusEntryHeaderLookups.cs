namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
		{
		}

		protected new CusEntryHeader Parent => (CusEntryHeader)base.Parent;
	}
}
