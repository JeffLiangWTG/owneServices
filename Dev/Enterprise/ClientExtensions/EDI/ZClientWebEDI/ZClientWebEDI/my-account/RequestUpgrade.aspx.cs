using System;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.ZClientWebCargoWiseEDI.My_account
{
	public partial class RequestUpgrade : BasePage
	{
		LicenceDatabase licenceDatabase;
		WebUpgradeRequestCollectionContainer webUpgradeRequestCollectionContainer;

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		#region Http Version

		HttpDownload HttpVersion
		{
			get
			{
				if (fHttpVersion == null)
				{
					fHttpVersion = new HttpDownload();
				}

				return fHttpVersion;
			}
		}

		HttpDownload fHttpVersion;

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory();

			string encryptedData = Request.QueryString[SecureQueryString.QueryStringKey];
			var secureQueryString = new SecureQueryString(encryptedData);

			LabelReleaseBuildName.Text = secureQueryString["ReleaseBuildName"];

			var org = SiteUser.LoggedInOrganisation as EDIOrgHeader;
			var licenceDatabasePK = secureQueryString["LicenceDatabasePK"];
			if (!string.IsNullOrEmpty(licenceDatabasePK))
			{
				var databaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				databaseQuery.AddToFilter(LicenceDatabaseSchema.PK, new ZGuid(licenceDatabasePK));
				databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_IsActive, ZBool.True);
				var licenceSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LD);
				var companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceHeaderSchema.LA_LC);
				companySubQuery.AddToFilter(LicenceCompanySchema.LC_OH, org.PK);
				licenceSubQuery.AddSubQuery(companySubQuery, JoinCondition.And);
				databaseQuery.AddSubQuery(licenceSubQuery, JoinCondition.And);

				licenceDatabase = factory.LoadTop1<LicenceDatabase>(databaseQuery);
			}

			if (licenceDatabase != null)
			{
				if (!IsPostBack)
				{
					FillUpgradeMethodDropDownList();
					CloseButton.Visible = false;
					UpgradeMethod.Visible = true;
					RequestUpgradeButton.Visible = true;
					LabelUpgradeMethod.Visible = true;
					LabelPleaseSelectUpgradeMethod.Visible = true;
					LabelUpgradeMethodDescription.Text = GetUpgradeMethodDescriptionByCode(UpgradeMethod.SelectedValue);
				}
				else
				{
					string wasItRequestUpgradeButton = Request["RequestUpgradeButton"];
					if (!string.IsNullOrEmpty(wasItRequestUpgradeButton) && wasItRequestUpgradeButton != "null")
					{
						String clientScript = "window.close()";
						CloseButton.Attributes.Add("OnClick", clientScript);
						CloseButton.Visible = true;

						LabelUpgradeMethodInfo.Text = "";
						UpgradeMethod.Visible = false;
						RequestUpgradeButton.Visible = false;
						LabelUpgradeMethod.Visible = false;
						LabelPleaseSelectUpgradeMethod.Visible = false;
						LabelUpgradeMethodDescription.Visible = false;

						ReleaseBuild releaseBuild = null;
						String releaseBuildPK = secureQueryString["ReleaseBuildPK"];
						if (!string.IsNullOrEmpty(releaseBuildPK))
						{
							releaseBuild = factory.Load<ReleaseBuild>(new ZGuid(releaseBuildPK));
						}

						if (releaseBuild != null)
						{
							var isValidReleaseBuild = releaseBuild.HL_ReleaseStatus == licenceDatabase.LD_ReleaseRing;
							if (!isValidReleaseBuild)
							{
								var builds = new LatestReleaseBuildsDictionary(factory);
								builds.Load();

								var currentBuild = licenceDatabase.CurrentVersion;

								if (currentBuild != null && !currentBuild.VersionNumber.IsEmpty)
								{
									isValidReleaseBuild = releaseBuild == builds.GetLatestAvailableBuildForInstalledVersion(licenceDatabase.LD_Product, licenceDatabase.LD_ReleaseRing, currentBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);

									if (!isValidReleaseBuild && currentBuild.HL_ReleaseStatus == ReleaseRings.Codes.GP2)
									{
										isValidReleaseBuild = releaseBuild == builds.GetLatestAvailableBuild(licenceDatabase.LD_Product, ReleaseRings.Codes.GP1, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);
									}
								}
								else
								{
									isValidReleaseBuild = releaseBuild == builds.GetLatestAvailableBuild(licenceDatabase.LD_Product, licenceDatabase.LD_ReleaseRing, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);
								}
							}

							if (isValidReleaseBuild)
							{
								string selectedUpgradeMethod = UpgradeMethod.SelectedValue;
								string oldUpgradeMethod = licenceDatabase.LD_AvailableUpgradeMethod;
								licenceDatabase.LD_AvailableUpgradeMethod = selectedUpgradeMethod;

								UpgradeRequest upgradeRequest = new UpgradeRequest(factory, org, licenceDatabase);
								UpgradeRequestCollection upgradeRequestCollection = new UpgradeRequestCollection(factory);
								upgradeRequestCollection.Add(upgradeRequest);
								webUpgradeRequestCollectionContainer = new WebUpgradeRequestCollectionContainer(factory, upgradeRequestCollection);
								webUpgradeRequestCollectionContainer.ReleaseBuildPK = releaseBuild.PK;

								String resultDetails = "";
								bool isRequestSuccessful = webUpgradeRequestCollectionContainer.RequestUpgrade(out resultDetails);
								LabelStatus.Text = resultDetails;
								LabelStatus.ForeColor = isRequestSuccessful ? System.Drawing.Color.Green : System.Drawing.Color.Red;

								licenceDatabase.LD_AvailableUpgradeMethod = oldUpgradeMethod;
								factory.Save();
							}
							else
							{
								LabelStatus.Text = "Release ring doesn't match database ring";
								LabelStatus.ForeColor = System.Drawing.Color.Red;
							}
						}
						else
						{
							LabelStatus.Text = "No such Release";
							LabelStatus.ForeColor = System.Drawing.Color.Red;
						}
					}
					else
					{
						if (UpgradeMethod.SelectedValue == UpgradeMethods.Codes.Http
							&& licenceDatabase != null
							&& !HttpVersion.IsSupportingVersion(licenceDatabase.CurrentVersion))
						{
							LabelUpgradeMethodInfo.Text = string.Format(
								"{0} is unavailable on your current version. " +
								"For the newly improved Http secured download method, " +
								"GPR clients must be on v{1}.{2}.{3}.{4} or later, " +
								"STD clients on v{5}.{6}.{7}.{8}+ and DPR clients on 1.4.3603.22+.<br />",
								UpgradeMethods.Descriptions.Http,
								HttpDownload.GprVersion.Major, HttpDownload.GprVersion.Minor, HttpDownload.GprVersion.Release, HttpDownload.GprVersion.Patch,
								HttpDownload.StdVersion.Major, HttpDownload.StdVersion.Minor, HttpDownload.StdVersion.Release, HttpDownload.StdVersion.Patch);

							RequestUpgradeButton.Enabled = false;
						}
						else
						{
							SetDefaultUpgradeMethodInfo();
							RequestUpgradeButton.Enabled = true;
						}

						LabelUpgradeMethodDescription.Text = GetUpgradeMethodDescriptionByCode(UpgradeMethod.SelectedValue);
					}
				}
			}
			else
			{
				LabelUpgradeMethodInfo.Text = "";
				UpgradeMethod.Visible = false;
				RequestUpgradeButton.Visible = false;
				LabelUpgradeMethod.Visible = false;
				LabelPleaseSelectUpgradeMethod.Visible = false;
				LabelUpgradeMethodDescription.Visible = false;

				LabelStatus.Text = "Invalid database";
				LabelStatus.ForeColor = System.Drawing.Color.Red;
			}
		}

		const string HttpDescription = "The upgrade package will be stored on the web server and your batch processor will download it automatically.";

		string GetUpgradeMethodDescriptionByCode(string upgradeMethod)
		{
			switch (upgradeMethod)
			{
				case UpgradeMethods.Codes.Http:
					return HttpDescription;
				default:
					return "";
			}
		}

		public void FillUpgradeMethodDropDownList()
		{
			if (licenceDatabase != null)
			{
				UpgradeMethod.Items.Clear();
				int currentUpgradeMethod = -1;

				string defaultUpgradeMethod = licenceDatabase.LD_AvailableUpgradeMethod;

				for (int i = 0; i < licenceDatabase.Lookups.UpgradeMethodsList.Count; i++)
				{
					string code = licenceDatabase.Lookups.UpgradeMethodsList[i].Code;
					if (code != UpgradeMethods.Codes.Blocked)
					{
						ListItem item = new ListItem(code + " - " + licenceDatabase.Lookups.UpgradeMethodsList[i].Description, code);
						UpgradeMethod.Items.Add(item);

						if (code == defaultUpgradeMethod)
						{
							currentUpgradeMethod = UpgradeMethod.Items.Count - 1;
						}
					}
				}

				if (currentUpgradeMethod >= 0)
				{
					UpgradeMethod.SelectedIndex = currentUpgradeMethod;
				}

				SetDefaultUpgradeMethodInfo();
			}
		}

		void SetDefaultUpgradeMethodInfo()
		{
			if (licenceDatabase.LD_AvailableUpgradeMethod == UpgradeMethods.Codes.Blocked)
			{
				LabelUpgradeMethodInfo.Text = "The nominated upgrade method for this server is currently 'Blocked'. However, you can choose an upgrade method below to request an upgrade. <br/>";
				LabelStatus.Text = "";
			}
			else
			{
				LabelUpgradeMethodInfo.Text = "";
			}
		}
	}
}
