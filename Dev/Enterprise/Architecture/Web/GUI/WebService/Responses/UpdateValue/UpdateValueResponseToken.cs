namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class UpdateValueResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public UpdateValueResponseToken(string controlID, string value)
			: base(WebServiceResponseActions.UpdateValue, controlID, value)
		{
		}

		#endregion
	}
}
