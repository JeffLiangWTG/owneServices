namespace Enterprise.Customs.CA.Business
{
	public class AddInfoHouseBillValidation : CAAddInfoValidation
	{
		public AddInfoHouseBillValidation(AddInfoHouseBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		public new AddInfoHouseBill Parent
		{
			get { return (AddInfoHouseBill)base.Parent; }
		}
	}
}
