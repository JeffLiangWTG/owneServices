namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public CusClassificationValidation(CusClassification parent)
			: base(parent)
		{
		}

		public new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}
	}
}
