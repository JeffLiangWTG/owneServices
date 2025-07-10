namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryLineValidation : EUAddInfoValidation
	{
		public AddInfoCusEntryLineValidation(AddInfoCusEntryLine parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryLine Parent
		{
			get { return (AddInfoCusEntryLine)base.Parent; }
		}

		protected AddInfoCusEntryLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
