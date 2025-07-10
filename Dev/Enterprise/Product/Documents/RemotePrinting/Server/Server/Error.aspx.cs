using System;
using System.Web;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Res = RemotePrinting.Server.Res;

namespace Enterprise.RemotePrinting.Server
{
	public partial class Error : ZAjaxPage
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "query string constant")]
		protected override void OnLoad(EventArgs e)
		{
			HomeLink.NavigateUrl = AppInstance.HomePage;
			HomeLink.Text = Res.GetString("ad448ec1-c1b2-454a-b3a7-3711a6ed0d7", "Home");

			PageTitle.Text = Res.GetString("d3fd152e-977c-4953-a490-4acea9519a61", "Error");
			MessageDescription.Text = Res.GetString("8bc0c4b8-e51c-4e0d-bfe4-e998142ecd3a", "A problem has been encountered. Sorry for any inconvenience this may have caused.");

			if (HttpContext.Current.Request.Params["data"] != null)
			{
				try
				{
					SecureQueryString qs = new SecureQueryString(Request["data"]);
					PageTitle.Text = qs["title"];
					MessageDescription.Text = qs["message"];
				}
				catch (InvalidQueryStringException)
				{ }
				catch (ExpiredQueryStringException)
				{ }
			}
			else if (HttpContext.Current.Request.Params["invalidQuery"] == "true")
			{
				MessageDescription.Text = Res.GetString("84e8f43a-b7df-40b8-9fdd-ec837e9feade", "If you directly typed this address please check for typos.\r\nIf you copied & pasted this address please check you included the entire address.");
			}
		}
	}
}
