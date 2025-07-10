namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryLineValidation : CAAddInfoValidation
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
