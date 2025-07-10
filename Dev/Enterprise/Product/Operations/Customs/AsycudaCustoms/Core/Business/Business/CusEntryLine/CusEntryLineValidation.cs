namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryLineValidation : Customs.Business.CusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return Parent; }
		}

		protected new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}
	}
}
