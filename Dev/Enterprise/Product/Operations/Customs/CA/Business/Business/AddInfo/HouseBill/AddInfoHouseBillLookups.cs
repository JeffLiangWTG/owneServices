
namespace Enterprise.Customs.CA.Business
{
	public class AddInfoHouseBillLookups : CAAddInfoLookups
	{
		public AddInfoHouseBillLookups(AddInfoHouseBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		public new AddInfoHouseBill Parent
		{
			get { return (AddInfoHouseBill)base.Parent; }
		}
	}
}
