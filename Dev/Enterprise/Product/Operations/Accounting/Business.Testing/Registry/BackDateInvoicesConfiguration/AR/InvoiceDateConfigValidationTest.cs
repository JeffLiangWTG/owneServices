namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class InvoiceDateConfigValidationTest : InvoiceDateConfigurationValidationTest
	{
		protected override IInvoiceDateConfiguration GetNewBizObj
		{
			get
			{
				return new InvoiceDateConfiguration();
			}
		}

		protected override IInvoiceDateConfigurationCollection GetNewBizObjCollection
		{
			get
			{
				return new InvoiceDateConfigurationCollection();
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

			((InvoiceDateConfiguration)BizObj).RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.SignificantDateCodeInfo);
			AssertHasErrors(BizObj.BrokerCodeInfo);
			AssertHasErrors(BizObj.ReversalRuleInfo);
		}
	}
}