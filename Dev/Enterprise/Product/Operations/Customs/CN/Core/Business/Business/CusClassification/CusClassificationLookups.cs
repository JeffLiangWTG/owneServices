namespace Enterprise.Customs.CN.Business
{
	public class CusClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public CusClassificationLookups(CusClassification parent)
			: base(parent)
		{
		}

		public CusClassification Classification => Parent;

		protected new CusClassification Parent => (CusClassification)base.Parent;
	}
}
