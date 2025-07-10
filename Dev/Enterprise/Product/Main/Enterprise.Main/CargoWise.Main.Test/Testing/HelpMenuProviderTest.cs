using System;
using System.Web;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class HelpMenuProviderTest : TestCaseWithFactory
	{
		public void TestShowSecurityOverrideToken_FailedBecauseRegistryNotEnabled()
		{
			var provider = new HelpMenuProvider();
			var mockIOIDCConfig = new Mock<IOIDCConfig>();

			mockIOIDCConfig.Setup(mockIOIDCConfig => mockIOIDCConfig.IsOIDCEnabled).Returns(false);
			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				provider.ShowSecurityOverrideToken();
				AssertEquals("Should show error if registry not enabled", "Security Override Token is not enabled.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				provider.ShowSecurityOverrideToken();
				AssertEquals("Should show error if registry not enabled", "Security Override Token is not enabled.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			mockIOIDCConfig.Setup(mockIOIDCConfig => mockIOIDCConfig.IsOIDCEnabled).Returns(true);
			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				provider.ShowSecurityOverrideToken();
				AssertEquals("Should show error if registry not enabled", "Security Override Token is not enabled.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		[TestDate(2025, 4, 17, 12, 0, 0)]
		public void TestShowSecurityOverrideToken_Success()
		{
			var provider = new HelpMenuProvider();
			var mockIOIDCConfig = new Mock<IOIDCConfig>();
			mockIOIDCConfig.Setup(mockIOIDCConfig => mockIOIDCConfig.IsOIDCEnabled).Returns(true);
			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.SecurityOverrideTokenExpiryTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				provider.ShowSecurityOverrideToken();
				var stmAccessTokens = Factory.Load<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SecurityOverrideToken));
				AssertEquals(1, stmAccessTokens.Length);
				var token1 = stmAccessTokens[0].SAT_Token;
				CombineAssertions(() =>
				{
					var stmAccessToken = stmAccessTokens[0];
					AssertEquals("SAT_Type", AccessTokenTypes.SecurityOverrideToken, stmAccessToken.SAT_Type);
					AssertEquals("SAT_ParentTableCode", GlbStaffSchema.Constants.Prefix, stmAccessToken.SAT_ParentTableCode);
					AssertEquals("SAT_ParentId", GlbStaff.CurrentUser.PK, stmAccessToken.SAT_ParentId);
					AssertEquals("SAT_RemainingUseCount", 1, stmAccessToken.SAT_RemainingUseCount);
					AssertEquals("SAT_ExpiresAt", ZDateTime.Now.AddMinutes(10), stmAccessToken.SAT_ExpiresAt);
					AssertEquals($"Security override token '{stmAccessToken.SAT_Token}' is generated and will be expired at {stmAccessToken.SAT_ExpiresAt}", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				stmAccessTokens[0].SAT_ExpiresAt = ZDateTime.Now;
				stmAccessTokens[0].SAT_RemainingUseCount = 0;
				Factory.Save();

				provider.ShowSecurityOverrideToken();
				stmAccessTokens = Factory.Load<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SecurityOverrideToken));
				AssertEquals("There should be still one item", 1, stmAccessTokens.Length);

				CombineAssertions(() =>
				{
					var stmAccessToken = stmAccessTokens[0];
					AssertNotEquals("token should be refreshed", token1, stmAccessToken.SAT_Token);
					AssertEquals("SAT_RemainingUseCount should be refreshed.", 1, stmAccessToken.SAT_RemainingUseCount);
					AssertEquals("SAT_ExpiresAt should be refreshed.", ZDateTime.Now.AddMinutes(10), stmAccessToken.SAT_ExpiresAt);
					AssertEquals($"Security override token '{stmAccessToken.SAT_Token}' is generated and will be expired at {stmAccessToken.SAT_ExpiresAt}", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		[GuiTest]
		public void TestShowDisclaimerConfirmation_Shows_WhenTrustedMessagingDisabled()
		{
			var provider = new HelpMenuProvider();

			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			provider.ShowDisclaimerConfirmation();
			AssertEquals("Disclaimer confirmation not shown", null, ZFormModaliser.LastFormShownDialogForTest);

			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			provider.ShowDisclaimerConfirmation();
			AssertEquals("Disclaimer confirmation shown", typeof(UserPortalDisclaimerForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		[GuiTest]
		public void TestShowCargoWiseWebPortals_Error_WhenCargoWiseWebPortalsIsNotConfiguredAndUserIsCWSupport()
		{
			var provider = new HelpMenuProvider();

			using (SetupGlowPortalsUri(null))
			{
				provider.ShowCargoWiseWebPortals();

				AssertEquals("Should show error if CargiWuse Wev Portals is not valid", "Cannot open CargoWise Web Portals as GLOW has not been configured for this client.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty("No URL was launched", WebUrlLauncher.LastUrlLaunched);
			}
		}

		[GuiTest]
		public void TestShowCargoWiseWebPortals_Shows_WhenCargoWiseWebPortalsIsConfiguredAndUserIsCWSupport()
		{
			var provider = new HelpMenuProvider();

			using (SetupGlowPortalsUri("https://glow.portals.com"))
			{
				provider.ShowCargoWiseWebPortals();
				AssertUrlLaunched();
			}

			using (SetupGlowPortalsUri("https://glow.portals.com/"))
			{
				provider.ShowCargoWiseWebPortals();
				AssertUrlLaunched();
			}

			static void AssertUrlLaunched()
			{
				AssertEquals("Should show information if user is CWSupport", "The CW1 Support login cannot be used for SSO. Please use Support Token authentication on the login page.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("https://glow.portals.com/GHS", WebUrlLauncher.LastUrlLaunched);
			}
		}

		[GuiTest]
		public void TestShowCargoWiseWebPortals_Error_WhenCargoWiseWebPortalsIsNotConfigured()
		{
			var staff = Factory.New<IGlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (SetupGlowPortalsUri(null))
			{
				var provider = new HelpMenuProvider();

				provider.ShowCargoWiseWebPortals();

				AssertEquals("Should show error if CargiWuse Wev Portals is not valid", "Cannot open CargoWise Web Portals as GLOW has not been configured for this client.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty("No URL was launched", WebUrlLauncher.LastUrlLaunched);
			}
		}

		[GuiTest]
		public void TestShowCargoWiseWebPortals_Shows_WhenCargoWiseWebPortalsIsConfigured()
		{
			var staff = Factory.New<IGlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var provider = new HelpMenuProvider();

				using (SetupGlowPortalsUri("https://glow.portals.com"))
				{
					provider.ShowCargoWiseWebPortals();
					AssertUrlLaunched();
				}

				using (SetupGlowPortalsUri("https://glow.portals.com/"))
				{
					provider.ShowCargoWiseWebPortals();
					AssertUrlLaunched();
				}
			}

			static void AssertUrlLaunched()
			{
				var uri = new Uri(WebUrlLauncher.LastUrlLaunched, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				AssertEquals("https", uri.Scheme);
				AssertEquals("glow.portals.com", uri.Host);
				AssertEquals("/GHS", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestIsCargoWiseWebPortalsConfigured_ShouldReturnFalse_WhenRegistryValueIsNullOrEmpty()
		{
			var provider = new HelpMenuProvider();

			using (SetupGlowPortalsUri(null))
			{
				AssertEquals(false, provider.IsCargoWiseWebPortalsConfigured());
			}

			using (SetupGlowPortalsUri(string.Empty))
			{
				AssertEquals(false, provider.IsCargoWiseWebPortalsConfigured());
			}
		}

		public void TestIsCargoWiseWebPortalsConfigured_ShouldReturnTrue_WhenRegistryValueIsNotEmpty()
		{
			var provider = new HelpMenuProvider();

			using (SetupGlowPortalsUri("https://glow.portals.com"))
			{
				AssertEquals(true, provider.IsCargoWiseWebPortalsConfigured());
			}
		}

		static IDisposable SetupGlowPortalsUri(string url)
		{
			WebUrlLauncher.ClearLastUrlLaunched();
			return GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, url);
		}

		protected override void SetUp()
		{
			base.SetUp();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected override void TearDown()
		{
			base.TearDown();
			WebUrlLauncher.ClearLastUrlLaunched();
		}
	}
}
