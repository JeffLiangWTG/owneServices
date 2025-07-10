namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class ShowErrorResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Test Cases

		public override void TestConstructors()
		{
			AssertResponseToken(GetExpectedAction(), string.Empty, "Test Message", GetResponseTokenForTesting() as WebServiceResponseActionToken);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new ShowErrorResponseToken("Test Message");
		}

		protected override string GetExpectedAction()
		{
			return WebServiceResponseActions.ShowError;
		}

		#endregion
	}
}
