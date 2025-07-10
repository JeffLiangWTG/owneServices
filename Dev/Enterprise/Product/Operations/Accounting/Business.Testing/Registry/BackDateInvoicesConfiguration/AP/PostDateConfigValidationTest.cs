namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class PostDateConfigValidationTest : PostDateConfigurationValidationTest
	{
		protected override PostDateConfiguration GetNewBizObj
		{
			get
			{
				return new PostDateConfiguration();
			}
		}

		protected override PostDateConfigurationCollection GetNewBizObjCollection
		{
			get
			{
				return new PostDateConfigurationCollection();
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.SignificantDateCode = "ALL";
			BizObj.BrokerCode = "SSS";
			BizObj.ReversalRule = "ABC";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.SignificantDateCodeInfo);
			AssertHasErrors(BizObj.BrokerCodeInfo);
			AssertHasErrors(BizObj.ReversalRuleInfo);
		}
	}
}