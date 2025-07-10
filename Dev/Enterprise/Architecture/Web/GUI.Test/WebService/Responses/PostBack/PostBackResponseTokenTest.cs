namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class PostBackResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Implementation

		public override void TestConstructors()
		{
			AssertResponseToken(GetExpectedAction(), "ControlID", string.Empty, GetResponseTokenForTesting() as WebServiceResponseActionToken);
		}

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new PostBackResponseToken("ControlID");
		}

		protected override string GetExpectedAction()
		{
			return WebServiceResponseActions.PostBack;
		}

		#endregion
	}
}
