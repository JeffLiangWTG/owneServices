
namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryLineLookups : CAAddInfoLookups
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
