namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public abstract class WebServiceResponseActionToken : WebServiceResponseToken
	{
		#region Constructors

		public WebServiceResponseActionToken()
			: base()
		{
		}

		public WebServiceResponseActionToken(string action, string controlID, string value)
			: base(controlID, value)
		{
			this.Action = action;
		}

		#endregion

		#region Properties

		public string Action { get; protected set; }

		#endregion
	}
}
