using System;
using System.Web.UI.WebControls;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Default : BasePage
	{
		internal protected void Page_Load(object sender, EventArgs e)
		{
			SetupAccreditationLink();
			UpdateControlsForLiteViewMode();
			DisableLinksForNoSecurityRightsUser();
		}

		void SetupAccreditationLink()
		{
			var url = AccreditationLinkCreator.GetUrl(SiteUser);
			AccreditationLink.NavigateUrl = url;
		}

		void DisableLinksForNoSecurityRightsUser()
		{
			DisableLink(EDIWebSecurityRightsList.Downloads, DownloadsLink);
			DisableLink(EDIWebSecurityRightsList.EDIMyAccountReports, ReportsLink);
			DisableLink(EDIWebSecurityRightsList.UpdateNotes, UpdatenotesLink);
			DisableLink(EDIWebSecurityRightsList.WebSecurityAdministration, WebSecurityLink);
			DisableLink(EDIWebSecurityRightsList.WebSecurityAdministration, NotificationRolesLink);
			DisableLink(EDIWebSecurityRightsList.EDIEnterpriseWiseLearning, eLearningLink);
			DisableLink(EDIWebSecurityRightsList.SapphireWiseLearning, sapphireELearningLink);
			DisableLink(EDIWebSecurityRightsList.OdysseyWiseLearning, odysseyELearningLink);
			DisableLink(EDIWebSecurityRightsList.LearningArchive, AccreditationLink);
		}

		void DisableLink(WebSecurityRight securityRight, HyperLink link)
		{
			if (!SiteUser.AreSecurityRightsGranted(securityRight))
			{
				const string accessDeniedScript = "javascript:alert('You do not have permission to access this page.');";
				link.CssClass += " disabled";
				link.NavigateUrl = accessDeniedScript;
				link.Target = "";
			}
		}

		void UpdateControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;

				UpdatenotesLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/UpdateNotes.aspx";
				DownloadsLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/Downloads.aspx";
				WebSecurityLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/WebSecurity.aspx";
				NotificationRolesLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/NotificationRoles.aspx";
				ReportsLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/Reports.aspx";
				ChangepasswordLink.NavigateUrl = AppInstance.HostingSiteRoot + "Home/ChangePassword.aspx";

				eLearningLink.Target = "_top";
				sapphireELearningLink.Target = "_top";
				odysseyELearningLink.Target = "_top";
				AccreditationLink.Target = "_top";
				UpdatenotesLink.Target = "_top";
				DownloadsLink.Target = "_top";
				WebSecurityLink.Target = "_top";
				NotificationRolesLink.Target = "_top";
				ReportsLink.Target = "_top";
				ChangepasswordLink.Target = "_top";
			}
		}

		internal HyperLink InternalDownloadsLink {
			get { return DownloadsLink; }
			set { DownloadsLink = value; }
		}
		internal HyperLink InternalWebSecurityLink {
			get { return WebSecurityLink; }
			set { WebSecurityLink = value; }
		}
		internal HyperLink InternalNotificationRolesLink {
			get { return NotificationRolesLink; }
			set { NotificationRolesLink = value; }
		}
		internal HyperLink InternalReportsLink {
			get { return ReportsLink; }
			set { ReportsLink = value; }
		}
		internal HyperLink InternalAccreditationLink {
			get { return AccreditationLink; }
			set { AccreditationLink = value; }
		}
		internal HyperLink InternalUpdatenotesLink {
			get { return UpdatenotesLink; }
			set { UpdatenotesLink = value; }
		}
		internal HyperLink InternalChangepasswordLink {
			get { return ChangepasswordLink; }
			set { ChangepasswordLink = value; }
		}
		internal HyperLink InternaleLearningLink {
			get { return eLearningLink; }
			set { eLearningLink = value; }
		}
		internal HyperLink InternalsapphireELearningLink {
			get { return sapphireELearningLink; }
			set { sapphireELearningLink = value; }
		}
		internal HyperLink InternalodysseyELearningLink {
			get { return odysseyELearningLink; }
			set { odysseyELearningLink = value; }
		}
	}
}
