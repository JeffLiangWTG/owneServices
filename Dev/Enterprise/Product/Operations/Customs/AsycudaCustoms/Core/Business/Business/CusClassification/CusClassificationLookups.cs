namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public CusClassificationLookups(CusClassification parent)
			: base(parent)
		{
		}

		public CusClassification Classification
		{
			get { return Parent; }
		}

		protected new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}
	}
}
