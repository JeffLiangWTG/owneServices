using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	/// <summary>
	/// LoginSuperseded page for the web site
	/// </summary>
	public partial class LoginSuperseded : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new OrgContactSupersededHelper(IdentityManager.Contact);
			return result;
		}

		protected OrgContactSupersededHelper Helper => DataSource as OrgContactSupersededHelper;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Helper.HasValidContacts)
			{
				ErrorMessage.Visible = false;
			}
			else
			{
				SupersededInstructionsLabel.Visible = false;
				SetMasterPasswordButton.Visible = false;
				ErrorMessage.Text = Res.GetString("dff4a3f4-94f9-4784-81ff-2d87304e7ed9", "You have been redirected here because your account was deactivated. However, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.");
			}
		}

		protected void SetMasterPasswordButton_Click(object sender, EventArgs e)
		{
			var masterPasswordPage = new Uri(AppInstance.SetMasterPasswordPage);
			var setMasterPasswordUrl = Helper.GenerateSetMasterPasswordUrl(masterPasswordPage, LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
			Response.Redirect(setMasterPasswordUrl.IsAbsoluteUri ? setMasterPasswordUrl.AbsoluteUri : setMasterPasswordUrl.OriginalString);
		}
	}
}
