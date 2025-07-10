using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class HelpMenuProvider : IHelpMenuProvider
	{
		public void ShowUserPortal()
		{
			if (ShouldLaunchPortal(ZHelpMenu.CargoWiseWebName))
			{
				new UserPortalLauncher().LaunchUserPortal();
			}
		}

		public void ShowWiseTechAcademy(string path = null, string target = null)
		{
			if (ShouldLaunchPortal(ZHelpMenu.CargoWiseWebName))
			{
				new UserPortalLauncher().LaunchWiseTechAcademy(path, target);
			}
		}

		public void ShowBorderWiseWebApp()
		{
			if (ShouldLaunchPortal(ZHelpMenu.CargoWiseWebName))
			{
				new BorderWiseLauncher().LaunchExternalApplication(new BorderWiseFilters());
			}
		}

		bool ShouldLaunchPortal(string name, SecurityCheckpoint checkpoint = null)
		{
			bool result = false;
			if (GlbStaff.CurrentUser.GS_IsSystemAccount)
			{
				Globals.Message.ShowError(Res.GetString("b595f0bc-3181-4b23-808c-2fe7a258910b", "System users do not have access to {0}.", name), Res.GetString("6ead5dac-e79f-4c7b-9ae9-8babca79200c", "Access Denied"));
			}
			else if (GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty)
			{
				Globals.Message.ShowInformation(Res.GetString("e01ff713-e805-4259-b5a5-2d9714841032", "Email address is required to log you into {0}. Please update your staff profile details and try again.", name));
				ZControllerFactory.Create(ControllerIDs.GlbStaff).ShowEditForm(GlbStaff.CurrentUser);
			}
			else if (checkpoint != null && !checkpoint.IsAllowed)
			{
				Globals.Message.ShowError(checkpoint.ErrorMessageForNotAllowed);
			}
			else
			{
				result = ShowDisclaimerConfirmation();
			}

			return result;
		}

		public void ShowERequestPortal()
		{
			if (ShouldLaunchPortal(ZHelpMenu.eRequestPortalName, Env.Security.IncidentApproval))
			{
				new UserPortalLauncher().GoToIncidentPortal();
			}
		}

		public bool IsCargoWiseWebPortalsConfigured()
		{
			return !string.IsNullOrEmpty(GlowPortalsUri);
		}

		public void ShowCargoWiseWebPortals()
		{
			if (!IsCargoWiseWebPortalsConfigured())
			{
				Globals.Message.ShowError(Res.GetString("77d3f7c3-2dc3-4612-b045-64fe236d99bf", "Cannot open CargoWise Web Portals as GLOW has not been configured for this client."));
				return;
			}

			var isSupportUser = Env.CurrentUser.IsSupportUser;
			if (isSupportUser)
			{
				Globals.Message.ShowInformation(Res.GetString("8cd009dc-7d59-4831-ab67-8f1ba63d58c2", "The CW1 Support login cannot be used for SSO. Please use Support Token authentication on the login page."));
			}

			var url = isSupportUser ? $"{GlowPortalsUri.TrimEnd('/')}/{GHS}" : UrlBuilder.GenerateURL(new Uri(GlowPortalsUri), GHS).ToString();
			WebUrlLauncher.Launch(url);
		}

		string GlowPortalsUri => GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

		const string GHS = "GHS";

		public void ShowAbout()
		{
			EnterpriseInformationForm.Show();
		}
		public void ShowAboutForCWNext()
		{
			EnterpriseInformationCWNextForm.Show();
		}

		public void ShowHotKeyHelp(Control sender)
		{
			var control = sender.GetFrontMostActiveControl();
			if (control != null)
			{
				new AvailableHotkeysForm(control).Show();
			}
		}

		public void ShowDeveloperFeatureControlOverride()
		{
			new DeveloperFeatureControlOverrideForm().Show();
		}

		public bool ShowDisclaimerConfirmation()
		{
			var result = true;
			if (UserPortalDisclaimerBizO.ShouldShowDisclaimer && !WebDataRegistry.Instance.EnableTrustedMessaging.Value)
			{
				var ok = ZFormModaliser.ShowDialogAndDispose(new UserPortalDisclaimerForm(new UserPortalDisclaimerBizO()));
				result = ok == DialogResult.OK;
			}
			return result;
		}

		public void ShowSecurityOverrideToken()
		{
			if (SystemDataRegistry.Instance.EnableSecurityOverrideToken.Value && ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
			{
				var query = new ZQuery();
				query.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SecurityOverrideToken);
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, GlbStaffSchema.Constants.Prefix);
				query.AddToFilter(StmAccessTokenSchema.SAT_ParentId, GlbStaff.CurrentUser.PK);

				var factory = new BusinessObjectFactory();
				var stmAccessToken = factory.LoadTop1<StmAccessToken>(query);

				if (stmAccessToken == null)
				{
					stmAccessToken = factory.New<StmAccessToken>();
					stmAccessToken.SAT_Type = AccessTokenTypes.SecurityOverrideToken;
					stmAccessToken.SAT_ParentTableCode = GlbStaffSchema.Constants.Prefix;
					stmAccessToken.SAT_ParentId = GlbStaff.CurrentUser.PK;
				}

				var token = new Random().Next(0, 1000000).ToString("D6");
				var expired = ZDateTime.Now.AddMinutes(SystemDataRegistry.Instance.SecurityOverrideTokenExpiryTime.Value);
				stmAccessToken.SAT_Token = token;
				stmAccessToken.SAT_ExpiresAt = expired;
				stmAccessToken.SAT_RemainingUseCount = 1;
				factory.Save();

				Globals.Message.Show(Res.GetString("0E5A9E01-B25F-4E12-9C1A-D5AC4AD32A94", "Security override token '{0}' is generated and will be expired at {1}", token, expired));
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("AE9C0B8F-AAD8-4DD5-8B05-D078D3DCA3CA", "Security Override Token is not enabled."));
			}
		}
	}
}
