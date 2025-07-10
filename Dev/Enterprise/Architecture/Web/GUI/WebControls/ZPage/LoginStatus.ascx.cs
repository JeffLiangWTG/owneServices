using System;
using System.Web.UI;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	///	Displays welcome message and link to Log off
	/// </summary>
	public partial class LoginStatus : BaseUserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.Visible = SiteUser.IsLoggedIn;

			if (SiteUser.IsLoggedIn)
			{
				WelcomeLabel.Text = Res.GetString("086612f7-b2f9-496c-9cc1-c184d056976e", "Welcome,");
				UserName.Text = SiteUser.LoggedInUserName;
				CompanyName.Text = SiteUser.AffiliationName;
				ChangePasswordLinkButton.Visible = ShowChangePasswordLinkButton;
				ChangePasswordLinkButton.Text = Res.GetString("5415383d-4f6d-4a48-bb4e-3ed4afc61f67", "Change Password");
				LogOffLinkButton.Visible = ShowLogOffLinkButton;
				LogOffLinkButton.Text = Res.GetString("0ebca239-9ba1-475e-a779-5d3cd971bc64", "Log Off");
			}
		}

		public void LogOff()
		{
			ZAppInstance.SignOut();
		}

		void LogOffLinkButton_Click(object sender, EventArgs e)
		{
			LogOff();
		}

		protected void ChangePasswordLinkButton_Click(object sender, EventArgs e)
		{
			Response.Redirect(ZAppInstance.ApplicationRoot + TrackingConstants.RelativePath.ChangePasswordPage, false);
		}

		public bool ShowLogOffLinkButton
		{
			get { return LogOffLinkButton.Visible; }
			set { LogOffLinkButton.Visible = value; }
		}

		public bool ShowChangePasswordLinkButton
		{
			get { return ChangePasswordLinkButton.Visible; }
			set { ChangePasswordLinkButton.Visible = value; }
		}

		public ControlCollection AdditionalContent => AdditionalLoginContent.Controls;

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.LogOffLinkButton.Click += new EventHandler(this.LogOffLinkButton_Click);
			this.ChangePasswordLinkButton.Click += new EventHandler(this.ChangePasswordLinkButton_Click);
		}

		#endregion
	}
}
