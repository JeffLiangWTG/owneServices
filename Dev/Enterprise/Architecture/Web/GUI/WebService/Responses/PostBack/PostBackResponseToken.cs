namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class PostBackResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public PostBackResponseToken(string controlID)
			: base(WebServiceResponseActions.PostBack, controlID, string.Empty)
		{
		}

		#endregion
	}
}
