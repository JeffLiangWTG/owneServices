using System;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class TermsAndConditions : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (WebContract == null)
			{
				AcknowledgeWrapper.Visible = false;
				ErrorMessage.Text = Res.GetString("381b0eb6-5008-4206-b760-4dec6b4ea3ef", "The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.");
				return;
			}

			WebContractContentHolder.InnerHtml = WebContract.WebContractContentForWeb;

			if (WebContract.ContractSignatory.TablePrefix == OrgHeaderSchema.Constants.Prefix)
			{
				AuthorisedUserCheckBox.Text = string.Format("I hereby certify that I am authorised to agree to this Agreement on behalf of {0}, and {0} agrees to be bound by the terms and conditions of this Agreement.", IdentityManager.Contact.Header.OH_FullName);
			}
			else
			{
				AuthorisedUserCheckBox.Text = "I have read and accept this Agreement";
				AgreementNoteDiv.Visible = false;
			}

			ContentChangedMessage.Visible = WebContract.HasWebContractSigned && WebContract.HasWebContractVersionChanged;

			AdjustControlsForLiteViewMode();
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				AcknowledgeWrapper.Style["margin"] = "0";
			}
		}

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		protected MyAccountWebContract WebContract
		{
			get { return DataSource as MyAccountWebContract; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			if (IdentityManager.Contact != null)
			{
				var webContact = new EDIOrgHeaderWebContract(IdentityManager.Contact);
				return webContact.NeedSignWebContract ? (MyAccountWebContract)webContact : new EDIOrgContactWebContract(IdentityManager.Contact);
			}
			return null;
		}

		protected void AcceptButton_Click(object sender, EventArgs e)
		{
			if (AuthorisedUserCheckBox.Checked)
			{
				string ipAddress = GetRequestIpAddress();
				WebContract?.SignWebContract(ipAddress);

				var contract = (MyAccountWebContract)GetNewDataSource();
				if (contract?.NeedSignWebContract ?? false)
				{
					Response.Redirect(Request.RawUrl);
				}
				else
				{
					var originalRequestUrl = LoginRouter.GetOriginalUrlFromRequest(Request);
					RedirectViaLoginRouter(originalRequestUrl, IdentityManager.Token);
				}
			}
		}

		protected virtual string GetRequestIpAddress()
		{
			string result;

			result = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (!string.IsNullOrEmpty(result))
			{
				string[] ipRange = result.Split(',');
				result = ipRange[0].Trim();
			}
			else
			{
				result = Request.ServerVariables["REMOTE_ADDR"];
			}
			return result;
		}

		protected void DeclineButton_Click(object sender, EventArgs e)
		{
			string ipAddress = GetRequestIpAddress();
			WebContract?.RejectWebContract(ipAddress);

			if (IsInLiteViewMode)
			{
				AppInstance.SignOut(false);
				HttpCookie expiredCookie = new HttpCookie("USERVIEWMODE");
				expiredCookie.Expires = ZDateTime.Now.AddDays(-1).ToDateTime();
				Response.Cookies.Add(expiredCookie);
			}
			else
			{
				SiteUser?.Logout();
			}
			Response.Redirect(AppInstance.LoginPage);
		}
	}
}
