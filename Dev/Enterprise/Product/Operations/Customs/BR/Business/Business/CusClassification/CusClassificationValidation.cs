namespace Enterprise.Customs.BR.Business
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
