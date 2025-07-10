namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryHeaderValidation : CAAddInfoValidation
	{
		public AddInfoCusEntryHeaderValidation(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryHeader Parent
		{
			get { return (AddInfoCusEntryHeader)base.Parent; }
		}

		protected AddInfoCusEntryHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
