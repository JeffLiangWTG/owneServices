
namespace Enterprise.Customs.CA.Business
{
	public class AddInfoCusEntryHeaderLookups : CAAddInfoLookups
	{
		public AddInfoCusEntryHeaderLookups(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		public new AddInfoCusEntryHeader Parent
		{
			get { return (AddInfoCusEntryHeader)base.Parent; }
		}
	}
}
