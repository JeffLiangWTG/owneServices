namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class SetReadOnlyResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public SetReadOnlyResponseToken(string controlID, bool value)
			: base(WebServiceResponseActions.SetReadOnly, controlID, value.ToString())
		{
		}

		#endregion
	}
}
