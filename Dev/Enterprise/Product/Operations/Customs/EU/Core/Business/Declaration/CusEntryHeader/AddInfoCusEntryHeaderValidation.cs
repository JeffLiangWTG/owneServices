namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryHeaderValidation : EUAddInfoValidation
	{
		public AddInfoCusEntryHeaderValidation(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryHeader Parent => (AddInfoCusEntryHeader)base.Parent;

		protected virtual AddInfoCusEntryHeaderLookups Lookups => Parent.Lookups;
	}
}
