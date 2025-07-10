using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class SwitchCompany : BasePage
	{
		#region Setup Grid

		protected override void SetupGrids()
		{
			SetupCompaniesGrid();
		}

		protected void SetupCompaniesGrid()
		{
			ZBindToChecker.CheckBindTo(((OrgContact)null).OrganisationCode);
			ZLinkButtonColumn codeColumn = new ZLinkButtonColumn(Res.GetString("3fbe534a-a74d-48da-b7f2-a49d305240ea", "Company Code"), nameof(OrgContact.OrganisationCode));
			codeColumn.Command = CmdSwitchCompany;
			codeColumn.ClientClickHandler = $@"return handleClientClick(this, 'switchCompany');";
			RelatedContactsDataGrid.Columns.Add(codeColumn);

			ZBindToChecker.CheckBindTo(((OrgContact)null).WorkingAddressCompanyName);
			RelatedContactsDataGrid.Columns.Add(new ZTextEditColumn(Res.GetString("e2bd48dc-09e7-4f29-b603-defcd9d6bbfa", "Company Name"), nameof(OrgContact.WorkingAddressCompanyName)));

			ZBindToChecker.CheckBindTo(((OrgContact)null).OC_Email);
			RelatedContactsDataGrid.Columns.Add(new ZTextEditColumn(Res.GetString("e2bc7e98-1389-4afa-b803-71fae7c74900", "Email"), nameof(OrgContactSchema.Constants.OC_Email)));

			RelatedContactsDataGrid.ItemCommand += new DataGridCommandEventHandler(CompaniesDataGrid_ItemCommand);
			RelatedContactsDataGrid.ItemDataBound += new DataGridItemEventHandler(CompaniesDataGrid_ItemDataBound);

			//To remove default border from grid https://www.thecodingforums.com/threads/why-does-gridview-always-emit-style-border-collapse-collapse.778291/
			RelatedContactsDataGrid.GridLines = GridLines.None;
			RelatedContactsDataGrid.CellSpacing = -1;
		}

		protected string CmdSwitchCompany => "switch";

		#endregion

		#region Event Handlers

		void CompaniesDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				HighlightRowForCurrentCompany(e.Item);
			}
		}

		protected void CompaniesDataGrid_ItemCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.CommandName == CmdSwitchCompany)
			{
				var selectedContact = ((OrgContactCollection)RelatedContactsDataGrid.DataSource)[e.Item.DataSetIndex];
				var selectedCompanyCode = selectedContact.OrganisationCode;
				var selectedEmail = selectedContact.OC_Email;

				if (!SwitchCurrentCompany(selectedCompanyCode, selectedEmail))
				{
					var securityQuery = new SecureQueryString() { { "message", "You will need to reauthenticate for this user." } };
					HttpContext.Current.Response.Redirect($"{AppInstance.LoginPage}?{MyAccountLoginHelper.RefKey}={DataSourceIndexer}&data={WebUtility.UrlEncode(securityQuery.ToString())}");
				}
			}
		}

		#endregion

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new LoginManager();
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				result.CompanyCode = SiteUser.LoggedInOrganisation.OH_Code;
				result.UserName = SiteUser.LoggedInUser.OC_Email;
			}
			return result;
		}

		protected LoginManager LoginMan => DataSource as LoginManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		#region Implementation

		protected bool SwitchCurrentCompany(string selectedCompanyCode, string selectedEmail)
		{
			if (!SiteUser.IsLoggedIn)
			{
				return false;
			}

			if (OIDCLoginHelper.IsCurrentUserLoggingInViaOIDC == OIDCLoginHelper.CanLoginViaOIDC(Factory, selectedEmail, selectedCompanyCode))
			{
				SiteUser.Logout();
				LoginMan.CompanyCode = selectedCompanyCode;
				LoginMan.UserName = selectedEmail;

				if (OIDCLoginHelper.IsCurrentUserLoggingInViaOIDC)
				{
					LoginMan.Password = Enterprise.ZArchitecture.Environment.User.WebTransientPassword;
				}

				var loginHelper = IsInLiteViewMode ? new MyAccountLoginLiteHelper(this, false) : new MyAccountLoginHelper(this, false);
				var result = loginHelper.SignIn();

				if (result)
				{
					var eventLogHelper = new EventLogHelper();
					eventLogHelper.CreateLogForUserLoggedIn(SiteUser);
				}
				return result;
			}

			this.AppInstance.ApplicationCookie.WriteUser(selectedCompanyCode ?? string.Empty, selectedEmail, "");
			SiteUser.Logout();
			return false;
		}

		protected void HighlightRowForCurrentCompany(DataGridItem row)
		{
			var contact = (OrgContact)row.DataItem;
			if (contact.OrganisationCode == LoginMan.CompanyCode && contact.OC_Email == LoginMan.UserName)
			{
				row.Cells.Cast<TableCell>().ForEach(x => x.BackColor = Color.FromArgb(230, 255, 230));
			}
		}

		#endregion
	}
}
