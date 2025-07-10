namespace Enterprise.Customs.EU.Business.MasterFiles
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

		// CC_ClassificationType
		protected override void CheckCC_ClassificationType()
		{
			// Import or export information is inherent in the code.  e.g. 1234.56.78.90 = import, trucate that to 1234.56.78 you have export. 
			base.CheckCC_ClassificationType();
			if (!Parent.IsInDatabase && Parent.CC_ClassificationType != CusClassification.ClassificationType.Both)
			{
				Parent.CC_ClassificationTypeInfo.AddError(Res.GetString("02E94237-B2EC-49A5-BA47-3C2F4AB1F0FA", "Only classification type 'BTH' is supported. Change the row to reflect this."));
			}
		}
	}
}
