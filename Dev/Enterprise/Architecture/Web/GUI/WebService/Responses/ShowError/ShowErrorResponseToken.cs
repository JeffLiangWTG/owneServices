using System;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class ShowErrorResponseToken : WebServiceResponseActionToken
	{
		#region Constructors

		public ShowErrorResponseToken(string message)
			: base(WebServiceResponseActions.ShowError, string.Empty, message)
		{
		}

		public ShowErrorResponseToken(Exception ex)
			: this(ex.Message)
		{
		}

		#endregion
	}
}
