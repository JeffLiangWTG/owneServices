using System;
using CargoWise.Common;

namespace Enterprise.Web.TestAspNetWebApplication3
{
	public partial class Error : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			HomeLink.NavigateUrl = "~";
			HomeLink.Text = "Home";

			PageTitle.Text = "Error";
			MessageDescription.Text = $"{nameof(TestAspNetWebApplication3)}->{nameof(Error)}";

			if (Request.Params["data"] != null)
			{
				try
				{
					var qs = new SecureQueryString(Request["data"]);
					PageTitle.Text = qs["title"];
					MessageDescription.Text = qs["message"];
				}
				catch (InvalidQueryStringException)
				{ }
				catch (ExpiredQueryStringException)
				{ }
			}
		}
	}
}
