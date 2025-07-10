
namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoCusEntryHeaderLookups : EUAddInfoLookups
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
