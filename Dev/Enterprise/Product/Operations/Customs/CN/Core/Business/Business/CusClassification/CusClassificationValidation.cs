namespace Enterprise.Customs.CN.Business
{
	public class CusClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public CusClassificationValidation(CusClassification parent)
			: base(parent)
		{
		}

		public new CusClassification Parent => (CusClassification)base.Parent;
	}
}
