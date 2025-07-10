using System;
using CargoWise.Common;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Error : BasePage
	{
		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			HomeLink.NavigateUrl = AppInstance.HomePage;
			HomeLink.Text = "Home";

			if (Request.Params["data"] != null)
			{
				try
				{
					PageTitle.InnerText = PageSecureQueryString["title"];
					MessageDescription.Text = PageSecureQueryString["message"];

					if (PageSecureQueryString["hometext"] != null)
					{
						HomeLink.Text = PageSecureQueryString["hometext"];
					}

					if (PageSecureQueryString["homeurl"] != null)
					{
						HomeLink.NavigateUrl = PageSecureQueryString["homeurl"];
					}

					if (IsInLiteViewMode)
					{
						HomeLink.NavigateUrl = AppInstance.HostingSiteHomePage;
						HomeLink.Target = "_top";
					}
				}
				catch (InvalidQueryStringException)
				{
				}
				catch (ExpiredQueryStringException)
				{
				}
			}
		}

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		protected System.Web.UI.HtmlControls.HtmlGenericControl pagehead;

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
		}
	}
}
