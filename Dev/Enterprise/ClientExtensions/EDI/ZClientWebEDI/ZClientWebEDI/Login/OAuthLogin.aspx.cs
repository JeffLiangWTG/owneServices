using System;
using System.Globalization;
using System.Web.UI;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class OAuthLogin : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			// Dynamically set the URL that the form should post to.
			loginForm.Attributes["action"] = ResolveUrl("~/oauth/grant");

			// Pass our current OAuth state around.
			// Variable names come from "id" attributes. This has to match "name",
			// since runat="server" means that ASP.NET will overwrite "name" with "id".
			redirect_uri.Attributes["value"] = Request.QueryString["redirect_uri"];
			state.Attributes["value"] = Request.QueryString["state"];
			scope.Attributes["value"] = Request.QueryString["scope"];
			client_id.Attributes["value"] = Request.QueryString["client_id"];

			// Set the error message if we have one to display.
			var error = Request.QueryString["error_message"];
			if (error != null)
			{
				errorMessage.Text = error;
			}
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}
	}
}