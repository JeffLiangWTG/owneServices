namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class AddInfoCusClassificationLookups : EUAddInfoLookups
	{
		public AddInfoCusClassificationLookups(AddInfoCusClassification parent)
			: base(parent)
		{
		}

		public new AddInfoCusClassification Parent
		{
			get { return (AddInfoCusClassification)base.Parent; }
		}
	}
}
