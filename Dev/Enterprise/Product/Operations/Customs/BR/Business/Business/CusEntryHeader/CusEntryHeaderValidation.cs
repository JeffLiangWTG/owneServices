namespace Enterprise.Customs.BR.Business
{
	public class CusEntryHeaderValidation : AutoBRCusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}
	}
}
