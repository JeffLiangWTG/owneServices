namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class AddInfoCusClassificationValidation : EUAddInfoValidation
	{
		public AddInfoCusClassificationValidation(AddInfoCusClassification parent)
			: base(parent)
		{
		}

		protected new AddInfoCusClassification Parent
		{
			get { return (AddInfoCusClassification)base.Parent; }
		}

		protected AddInfoCusClassificationLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
