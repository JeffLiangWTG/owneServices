using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class RetrieveLogin : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (IsInLiteViewMode)
			{
				LoginLink1.NavigateUrl = "LoginLite.aspx";
				LoginLink2.NavigateUrl = "LoginLite.aspx";
			}
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}

		protected void RetrieveButton_Click(object sender, EventArgs e)
		{
			var message = BusinessEntity.Retrieve(AppInstance);
			if (message.Equals(LoginRetrieverBizO.GenericEmailSentMessage))
			{
				RetrieveLoginPanel.Visible = false;
				EmailSentMessage.Text = message;
				LoginDetailsRetrievedMessage.Visible = true;
			}
			else
			{
				InvalidEmailMessage.Text = message;
				InvalidEmailMessage.Visible = true;
			}
		}

		LoginRetrieverBizO BusinessEntity
		{
			get { return (LoginRetrieverBizO)DataSource; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new LoginRetrieverBizO(Factory);
		}
	}
}
