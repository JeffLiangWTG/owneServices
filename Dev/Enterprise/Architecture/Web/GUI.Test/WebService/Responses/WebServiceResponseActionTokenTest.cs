namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public abstract class WebServiceResponseActionTokenTest : WebServiceResponseTokenTest
	{
		#region Test Cases

		public override void TestConstructors()
		{
			AssertResponseToken(GetExpectedAction(), "ControlID", GetExpectedTokenValue(), GetResponseTokenForTesting() as WebServiceResponseActionToken);
		}

		public void TestAction()
		{
			var testResponse = GetResponseTokenForTesting() as WebServiceResponseActionToken;

			AssertEquals(GetExpectedAction(), testResponse.Action);
		}

		#endregion

		#region Implementation

		protected virtual string GetExpectedTokenValue() => "NewValue";

		protected abstract string GetExpectedAction();

		protected void AssertResponseToken(string expectedAction, string expectedControlID, string expectedValue, WebServiceResponseActionToken token)
		{
			AssertEquals(expectedAction, token.Action);
			AssertEquals(expectedControlID, token.ControlID);
			AssertEquals(expectedValue, token.Value);
		}

		#endregion
	}
}
