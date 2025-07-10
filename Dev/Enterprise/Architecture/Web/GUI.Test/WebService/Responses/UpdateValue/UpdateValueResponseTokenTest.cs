namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class UpdateValueResponseTokenTest : WebServiceResponseActionTokenTest
	{
		#region Implementation

		protected override WebServiceResponseToken GetResponseTokenForTesting()
		{
			return new UpdateValueResponseToken("ControlID", "NewValue");
		}

		protected override string GetExpectedAction()
		{
			return WebServiceResponseActions.UpdateValue;
		}

		#endregion
	}
}
