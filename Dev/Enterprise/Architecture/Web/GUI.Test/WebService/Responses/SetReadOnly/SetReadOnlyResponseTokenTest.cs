namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class SetReadOnlyResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Implementation

		protected override string GetExpectedTokenValue() => "True";

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new SetReadOnlyResponseToken("ControlID", true);
		}

		protected override string GetExpectedAction()
		{
			return WebServiceResponseActions.SetReadOnly;
		}

		#endregion
	}
}
