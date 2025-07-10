namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryLineLookups : EUAddInfoLookups
	{
		public AddInfoCusEntryLineLookups(AddInfoCusEntryLine parent)
			: base(parent)
		{
		}

		public new AddInfoCusEntryLine Parent
		{
			get { return (AddInfoCusEntryLine)base.Parent; }
		}
	}
}
