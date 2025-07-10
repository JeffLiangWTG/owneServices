namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CalculateLiabilityNonPersistentBizObjValidation : AutoCalculateLiabilityNonPersistentBizObjValidation
	{
		public CalculateLiabilityNonPersistentBizObjValidation(AutoCalculateLiabilityNonPersistentBizObj parent) : base(parent)
		{
		}

		protected override void CheckLiabilityPercentage()
		{
			base.CheckLiabilityPercentage();

			if (!Parent.LiabilityPercentage.IsInRange(0, 100))
			{
				var propertyInfo = Parent.LiabilityPercentageInfo;
				propertyInfo.AddError(Res.GetString("90d72aa4-2bbd-4f6a-8075-a05dd091e783", "{0} should be a value between 0 and 100.", propertyInfo.HumanReadableName));
			}
		}
	}
}
