using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.ZClientWebCargoWiseEDI.My_Account
{
	public partial class Downloads : BasePage
	{
		public LicenceCompanyLicenceDatabaseCollection LicenceDatabases;

		protected void Page_Load(object sender, EventArgs e)
		{
			SetupDvdDownloadInfo();
			SetupReleaseRingDownloadInfo();
			AdjustControlsForLiteViewMode();
		}

		protected void SetupDvdDownloadInfo()
		{
			CW1DvdIsoFileLink.HRef = EDIDataRegistry.Instance.CW1DvdIsoFileDownloadURL.Value;
			CW1DvdZipFileLink.HRef = EDIDataRegistry.Instance.CW1DvdZipFileDownloadURL.Value;
			CW1ExeFileLink.HRef = EDIDataRegistry.Instance.CW1ExeFileDownloadURL.Value;
			CheckLink(CW1DvdIsoFileLink);
			CheckLink(CW1DvdZipFileLink);
			CheckLink(CW1ExeFileLink);
		}

		void CheckLink(HtmlAnchor link)
		{
			if (string.IsNullOrEmpty(link.HRef))
			{
				link.HRef = "#";
				link.Attributes["onclick"] = "javascript:alert('Download link is currently unavailable.')";
			}
		}

		void SetupReleaseRingDownloadInfo()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			EDIOrgHeader org = factory.Load<EDIOrgHeader>(SiteUser.LoggedInOrganisation.PK);
			PopulateActiveDatabasesSupportedDownload(org);
			Repeater1.DataSource = LicenceDatabases;
			Repeater1.DataBind();
		}

		internal void PopulateActiveDatabasesSupportedDownload(EDIOrgHeader org)
		{
			if (org.LicCompany != null)
			{
				var activeList = new LicenceCompanyLicenceDatabaseCollection(org.LicCompany);
				var filter = new ZQuery();
				filter.AddToFilter(LicenceDatabaseSchema.LD_Product, new string[] { ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseOne, ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWise, ProductTypes.Codes.GLOW });
				filter.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);
				activeList.Load(filter);
				if (activeList.Count > 0)
				{
					LicenceDatabases = activeList;
				}
			}
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		internal protected void R1_ItemDataBound(Object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				var licenceDatabaseCurrent = (LicenceDatabase)(e.Item.DataItem);

				var dropDownVersion = (DropDownList)(e.Item.FindControl("DropDownVersion"));

				if (dropDownVersion != null)
				{
					dropDownVersion.ID = "DropDownVersion_" + licenceDatabaseCurrent.PK.ToString();

					var builds = new LatestReleaseBuildsDictionary(licenceDatabaseCurrent.Factory);
					builds.Load();

					List<ReleaseBuild> releaseBuilds = new List<ReleaseBuild>();
					ReleaseBuild currentBuild = licenceDatabaseCurrent.CurrentVersion;
					Label labelCurrentVersion = (Label)(e.Item.FindControl("CurrentVersion"));

					if (currentBuild != null && !currentBuild.VersionNumber.IsEmpty)
					{
						labelCurrentVersion.Text = currentBuild.FullDisplayText.ToString();
						var latestBuild = builds.GetLatestAvailableBuildForInstalledVersion(licenceDatabaseCurrent.LD_Product, licenceDatabaseCurrent.LD_ReleaseRing, currentBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: licenceDatabaseCurrent.IsHostedOnWiseCloud);
						releaseBuilds.Add(latestBuild);

						if (latestBuild != null && currentBuild.HL_ReleaseStatus == ReleaseRings.Codes.GP2)
						{
							releaseBuilds.Add(builds.GetLatestAvailableBuild(licenceDatabaseCurrent.LD_Product, ReleaseRings.Codes.GP1, takeWeeklyBuildsInsteadOfLatest: licenceDatabaseCurrent.IsHostedOnWiseCloud));
						}
					}
					else
					{
						labelCurrentVersion.Text = "";
						releaseBuilds.Add(builds.GetLatestAvailableBuild(licenceDatabaseCurrent.LD_Product, licenceDatabaseCurrent.LD_ReleaseRing, takeWeeklyBuildsInsteadOfLatest: licenceDatabaseCurrent.IsHostedOnWiseCloud));
					}

					foreach (var releaseBuild in releaseBuilds.Where(x => x != null).Distinct())
					{
						ListItem item = new ListItem(releaseBuild.FullDisplayText, releaseBuild.PK.ToString());
						dropDownVersion.Items.Add(item);
					}
				}

				var linkDetails = ((HyperLink)e.Item.FindControl("LinkDetails"));

				if (linkDetails != null)
				{
					if (dropDownVersion != null && dropDownVersion.Items.Count > 0)
					{
						linkDetails.ID = "LinkDetails_" + licenceDatabaseCurrent.PK.ToString();
						linkDetails.NavigateUrl = "../Incidents/IncidentsForReleaseRing.aspx?RequestedReleaseBuildPK=" + dropDownVersion.Items[0].Value +
																							 "&RequestedReleaseBuildName=" + dropDownVersion.Items[0].Text +
																							 "&CurrentReleaseBuildPK=" + ((licenceDatabaseCurrent.CurrentVersion != null) ? licenceDatabaseCurrent.CurrentVersion.PK.ToString() : "null");
					}
				}

				var linkUpdateNotes = ((HyperLink)e.Item.FindControl("LinkUpdateNotes"));

				if (linkUpdateNotes != null)
				{
					if (dropDownVersion != null && dropDownVersion.Items.Count > 0)
					{
						string pageUrl = IsInLiteViewMode ? AppInstance.HostingSiteRoot + "Home/UpdateNotes.aspx" : "../ReleaseNotes/ReleaseNotes.aspx";
						linkUpdateNotes.ID = "LinkUpdateNotes_" + licenceDatabaseCurrent.PK.ToString();
						linkUpdateNotes.NavigateUrl = pageUrl + "?RequestedReleaseBuildPK=" + dropDownVersion.Items[0].Value +
																"&RequestedReleaseBuildName=" + dropDownVersion.Items[0].Text +
																"&CurrentReleaseBuildPK=" + ((licenceDatabaseCurrent.CurrentVersion != null) ? licenceDatabaseCurrent.CurrentVersion.PK.ToString() : "null");
						linkUpdateNotes.Target = IsInLiteViewMode ? "_top" : "_self";
					}
				}

				var requestUpgradeButton = (Button)(e.Item.FindControl("RequestUpgrade"));

				if (requestUpgradeButton != null)
				{
					if (dropDownVersion != null && dropDownVersion.Items.Count > 0)
					{
						requestUpgradeButton.ID = "RequestUpgrade_" + licenceDatabaseCurrent.PK.ToString();

						var secureQueryString = new SecureQueryString();
						secureQueryString["ReleaseBuildPK"] = dropDownVersion.SelectedValue;
						secureQueryString["ReleaseBuildName"] = dropDownVersion.SelectedItem.Text;
						secureQueryString["LicenceDatabasePK"] = licenceDatabaseCurrent.PK.ToString();

						var clientscript = "window.open('RequestUpgrade.aspx?" + SecureQueryString.QueryStringKey + "=" + WebUtility.UrlEncode(secureQueryString.ToString()) + "', 'popupwindow','width=600, height=250, menubar=no, scrollbars=yes, resizable=no');";
						requestUpgradeButton.Attributes.Add("OnClick", clientscript);
					}
				}
			}
		}

		protected void cmbDropDownVersion_SelectedIndexChanged(object sender, EventArgs e)
		{
			var dropDownVersion = (DropDownList)sender;
			var databaseLicenceCurrentPK = dropDownVersion.ID.Substring(dropDownVersion.ID.IndexOf("_") + 1);
			HyperLink linkDetails = null;
			HyperLink linkUpdateNotes = null;
			Button requestUpgradeButton = null;

			LicenceDatabase licenceDatabaseCurrent = Factory.Load<LicenceDatabase>(new ZGuid(databaseLicenceCurrentPK));

			foreach (Control ctrl in this.Repeater1.Controls)
			{
				linkDetails = (HyperLink)(ctrl.FindControl("LinkDetails_" + databaseLicenceCurrentPK));
				if (linkDetails != null)
				{
					if (dropDownVersion != null && dropDownVersion.SelectedIndex > 0)
					{
						linkDetails.NavigateUrl = "../Incidents/IncidentsForReleaseRing.aspx?RequestedReleaseBuildPK=" + dropDownVersion.SelectedValue +
																							"&RequestedReleaseBuildName=" + dropDownVersion.SelectedItem.Text +
																							"&CurrentReleaseBuildPK=" + ((licenceDatabaseCurrent != null && licenceDatabaseCurrent.CurrentVersion != null) ? licenceDatabaseCurrent.CurrentVersion.PK.ToString() : "null");
					}
					break;
				}
			}

			foreach (Control ctrl in this.Repeater1.Controls)
			{
				linkUpdateNotes = (HyperLink)(ctrl.FindControl("LinkUpdateNotes_" + databaseLicenceCurrentPK));
				if (linkUpdateNotes != null)
				{
					if (dropDownVersion != null && dropDownVersion.SelectedIndex > 0)
					{
						string pageUrl = IsInLiteViewMode ? AppInstance.HostingSiteRoot + "Home/UpdateNotes.aspx" : "../ReleaseNotes/ReleaseNotes.aspx";
						linkUpdateNotes.NavigateUrl = pageUrl + "?RequestedReleaseBuildPK=" + dropDownVersion.SelectedValue +
																"&RequestedReleaseBuildName=" + dropDownVersion.SelectedItem.Text +
																"&CurrentReleaseBuildPK=" + ((licenceDatabaseCurrent != null && licenceDatabaseCurrent.CurrentVersion != null) ? licenceDatabaseCurrent.CurrentVersion.PK.ToString() : "null");
						linkUpdateNotes.Target = IsInLiteViewMode ? "_top" : "_self";
					}
					break;
				}
			}

			foreach (Control ctrl in this.Repeater1.Controls)
			{
				requestUpgradeButton = (Button)(ctrl.FindControl("RequestUpgrade_" + databaseLicenceCurrentPK));
				if (requestUpgradeButton != null)
				{
					if (dropDownVersion != null && dropDownVersion.SelectedIndex > 0)
					{
						var secureQueryString = new SecureQueryString();
						secureQueryString["ReleaseBuildPK"] = dropDownVersion.SelectedValue;
						secureQueryString["ReleaseBuildName"] = dropDownVersion.SelectedItem.Text;
						secureQueryString["LicenceDatabasePK"] = databaseLicenceCurrentPK;

						var clientscript = "window.open('RequestUpgrade.aspx?" + SecureQueryString.QueryStringKey + "=" + WebUtility.UrlEncode(secureQueryString.ToString()) + "', 'popupwindow','width=600, height=250, menubar=no, scrollbars=yes, resizable=no');";
						requestUpgradeButton.Attributes.Add("OnClick", clientscript);
					}
					break;
				}
			}
		}
	}
}
