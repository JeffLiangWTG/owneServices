namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class ResponseConditionEqualTokenTest : WebServiceResponseConditionTokenTest
	{
		#region Implementation

		protected override string GetCompareJavaScriptToken()
		{
			return "==";
		}

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new ResponseConditionEqualToken("ControlID", "TestValue");
		}

		protected override string GetExpectedCondition()
		{
			return WebServiceResponseConditions.Equal;
		}

		#endregion
	}
}
