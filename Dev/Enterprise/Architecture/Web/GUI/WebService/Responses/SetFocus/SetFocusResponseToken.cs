namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class SetFocusResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public SetFocusResponseToken(string controlID)
			: base(WebServiceResponseActions.SetFocus, controlID, string.Empty)
		{
		}

		#endregion
	}
}
