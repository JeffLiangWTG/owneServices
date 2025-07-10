using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web.Security;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	///	Control to capture log in information
	/// </summary>
	public class LoginControl : BaseUserControl
	{
		#region Auto

		protected ZTextBox CompanyCodeTextBox;
		protected ZTextBox LoginNameTextBox;
		protected ZTextBox PasswordTextBox;
		protected System.Web.UI.WebControls.Button SigninBtn;
		protected System.Web.UI.WebControls.Label Message;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Probably not used, but raised WI00629332 to get Core team to investigate")]
		void InitializeComponent()
		{
			CompanyCodeTextBox.BindTo = "CompanyCode";
			CompanyCodeTextBox.BindTo = "CompanyCode";
			LoginNameTextBox.BindTo = "UserName";
			LoginNameTextBox.BindTo = "UserName";
			PasswordTextBox.BindTo = "Password";
			PasswordTextBox.BindTo = "Password";
			this.SigninBtn.Click += new EventHandler(this.SigninBtn_Click);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Web.GUI";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.GUI.LoginManager";
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (SiteUser.IsLoggedIn)
			{
				SiteUser.Logout();
				Session.Abandon();
			}
			RegisterOnLoadClientScriptBlock();

			new ZWebControlBinder(LoginMan).Bind(this.Controls);
		}

		#region Helpers

		// EcmaScriptVersion.Major >= 1 implies Javascript support see http://go.microsoft.com/fwlink/?linkid=14202
		protected void RegisterOnLoadClientScriptBlock()
		{
			if (Request.Browser.EcmaScriptVersion.Major >= 1 && !Page.ZClientScript.IsClientForEventScriptBlockRegistered("window", "onload", GetType(), "OnLoad"))
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(GetInitialFocusScript(CompanyCodeTextBox));
				builder.Append(GetWindowAsParentWindowScript());
				Page.ZClientScript.RegisterClientForEventScriptBlock("window", "onload", GetType(), "OnLoad", builder.ToString());
			}
		}

		protected string GetInitialFocusScript(System.Web.UI.WebControls.WebControl control)
		{
			return string.Format("document.getElementById('{0}').focus();", control.ClientID);
		}

		protected string GetWindowAsParentWindowScript()
		{
			return string.Format("if (self != parent) parent.location = '{0}';", RequestUrl.AbsolutePath);
		}

		protected
#if DEBUG
 virtual
#endif
 Uri RequestUrl
		{
			get { return Request.Url; }
		}

		#endregion

		#region Binding

		protected LoginManager LoginMan
		{
			get
			{
				if (fLoginMan == null)
				{
					fLoginMan = new LoginManager();
				}

				return fLoginMan;
			}
		}
		LoginManager fLoginMan;

		#endregion

		void SigninBtn_Click(object sender, EventArgs e)
		{
			if (!LoginMan.HasErrors)
			{
				SiteUser.Login(LoginMan.CompanyCode, LoginMan.UserName, LoginMan.Password, LoginMan.LoginHash);

				if (SiteUser.IsLoggedIn)
				{
					FormsAuthentication.SetAuthCookie(LoginMan.CompanyCode, false);
				}
				else
				{
					Message.Text = Res.GetString("e063d0df-4c1d-49b1-a2a7-98ebfdd9720f", "Login Failed!");
				}
			}
		}
	}

	#endregion
}
