namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class SetFocusResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Test Cases

		public override void TestConstructors()
		{
			AssertResponseToken(GetExpectedAction(), "ControlID", string.Empty, GetResponseTokenForTesting() as WebServiceResponseActionToken);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new SetFocusResponseToken("ControlID");
		}

		protected override string GetExpectedAction()
		{
			return WebServiceResponseActions.SetFocus;
		}

		#endregion
	}
}
