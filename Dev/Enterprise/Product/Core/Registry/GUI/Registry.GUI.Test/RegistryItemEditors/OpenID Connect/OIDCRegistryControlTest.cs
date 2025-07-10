using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Login;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(OIDCRegistryControl))]
	sealed class OIDCRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestControlRender_HostedSystem_NonSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				zFrom.Controls.Add(control);
				zFrom.Show();

				Assert("Should be visible for none-support user in hosted system.", control.UpdateLabel.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.UpdateButton.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.VerifyLabel.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.VerifyButton.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.EnableLabel.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.EnableButton.Visible);
				Assert("Should be visible for none-support user in hosted system.", control.VerifyResultCheckBox.Visible);
				Assert("Should be read only.", control.VerifyResultCheckBox.ReadOnly);

				Assert("Should be disabled for none-support user in hosted system.", !control.OIDCEnabledCheckBox.Enabled);
				Assert("Should be disabled for none-support user in hosted system.", !control.OIDCServerTypeDropEdit.Enabled);

				AssertNull("Should not be added to the control for none-support user in hosted system.", control.UpdateNoteHintLabel);
				AssertNull("Should not be added to the control for none-support user in hosted system.", control.UpdateNoteLinkLabel);

				AssertEquals("Verify the configuration by first logging in:", control.VerifyLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestControlRender_HostedSystem_SupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlRenderResults();
			}
		}

		public void TestControlRender_SelfHosted_NonSupportUser()
		{
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlRenderResults();
			}
		}

		public void TestControlRender_SelfHosted_SupportUser()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlRenderResults();
			}
		}

		void AssertControlRenderResults()
		{
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				zFrom.Controls.Add(control);
				zFrom.Show();

				AssertNull("Should not be added to control for self hosted system or support user.", control.UpdateLabel);
				AssertNull("Should not be added to control for self hosted system or support user.", control.UpdateButton);
				AssertNull("Should not be added to control for self hosted system or support user.", control.EnableLabel);
				AssertNull("Should not be added to control for self hosted system or support user.", control.EnableButton);
				AssertNull("Should not be added to control for self hosted system or support user.", control.VerifyResultCheckBox);

				Assert("Should be visible for self hosted system or support user.", control.UpdateNoteHintLabel.Visible);
				Assert("Should be visible for self hosted system or support user.", control.UpdateNoteLinkLabel.Visible);
				Assert("Should be visible for self hosted system or support user.", control.OIDCEnabledCheckBox.Visible);
				Assert("Should be visible for self hosted system or support user.", control.OIDCServerTypeDropEdit.Visible);
				Assert("Should be visible for self hosted system or support user.", control.VerifyLabel.Visible);
				Assert("Should be visible for self hosted system or support user.", control.VerifyButton.Visible);

				AssertEquals("Before saving, verify the configuration by first logging in:", control.VerifyLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestSetReadOnly_SelfHosted_SupportUser()
		{
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlEnable();
			}
		}

		public void TestSetReadOnly_SelfHosted_NonSupportUser()
		{
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlEnable();
			}
		}

		public void TestSetReadOnly_HostedSystem_SupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertControlEnable();
			}
		}

		void AssertControlEnable()
		{
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = true,
					OIDCServerType = OIDCServerTypes.Generic,
					AuthorityURL = $"https://localhost:8888",
					ClientIdentifier = "interactive.public",
				};
				control.Value = oidcConfig;
				zFrom.Controls.Add(control);
				zFrom.Show();

				control.SetControlOrBusinessEntityReadOnly(true);
				AssertResult(true);

				control.SetControlOrBusinessEntityReadOnly(false);
				AssertResult(false);

				void AssertResult(bool readOnly)
				{
					AssertEquals(!readOnly, control.VerifyButton.Enabled);
					AssertEquals(!readOnly, control.OptionGroupBox.Enabled);
					AssertEquals(!readOnly, control.OIDCEnabledCheckBox.Enabled);
					AssertEquals(!readOnly, control.OIDCServerTypeDropEdit.Enabled);

					AssertEquals(readOnly, control.AuthorityURLText.ReadOnly);
					AssertEquals(readOnly, control.ClientIdentifierText.ReadOnly);
					AssertEquals(readOnly, control.ClaimsMappingGrid.ReadOnly);
					AssertEquals(readOnly, control.ScopesGrid.ReadOnly);
				}
			}
		}

		public void TestSetReadOnly_HostedSystem_NonSupportUser()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = true,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://localhost:8888",
					ClientIdentifier = "interactive.public",
				};
				control.Value = oidcConfig;
				zFrom.Controls.Add(control);
				zFrom.Show();

				var checkpoint = Env.Security.GetRegistryCheckPoint(SystemDataRegistry.Instance.OIDCConfig.Name, SystemDataRegistry.Instance.OIDCConfig.Caption);
				checkpoint.IsAllowed = true;
				Env.Security.SystemRegistryEdit.IsAllowed = true;

				control.SetControlOrBusinessEntityReadOnly(true); // this will be only true for hosted non support user

				AssertEquals(true, control.OptionGroupBox.Enabled);
				AssertEquals("user with granted security rights is able to access the button.", true, control.UpdateButton.Enabled);
				AssertEquals("user with granted security rights is able to access the button.", true, control.VerifyButton.Enabled);
				AssertEquals("user with granted security rights is able to access the button.", true, control.EnableButton.Enabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Enabled);
				AssertEquals(false, control.OIDCServerTypeDropEdit.Enabled);

				AssertEquals(true, control.AuthorityURLText.ReadOnly);
				AssertEquals(true, control.ClientIdentifierText.ReadOnly);
				AssertEquals(true, control.ClaimsMappingGrid.ReadOnly);
				AssertEquals(true, control.ScopesGrid.ReadOnly);

				checkpoint.IsAllowed = false;
				Env.Security.SystemRegistryEdit.IsAllowed = false;

				control.SetControlOrBusinessEntityReadOnly(true); // this will be only true for hosted non support user

				AssertEquals(false, control.OptionGroupBox.Enabled);

				AssertEquals(false, control.OIDCEnabledCheckBox.Enabled);
				AssertEquals(false, control.OIDCServerTypeDropEdit.Enabled);

				AssertEquals(true, control.AuthorityURLText.ReadOnly);
				AssertEquals(true, control.ClientIdentifierText.ReadOnly);
				AssertEquals(true, control.ClaimsMappingGrid.ReadOnly);
				AssertEquals(true, control.ScopesGrid.ReadOnly);
			}
		}

		public void TestOIDCServerTypeDropEditIncludingNewOptions()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				var oidcConfig = new OIDCConfig
				{
					IsOIDCEnabled = true,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = "https://localhost:8888",
					ClientIdentifier = "interactive.public",
				};
				control.Value = oidcConfig;
				zFrom.Controls.Add(control);
				zFrom.Show();

				var options = control.OIDCServerTypeDropEdit.List.Cast<CodeDescriptionPair>();

				CombineAssertions(() =>
				{
					AssertEquals(options.Count(), 5);
					AssertCollectionContains(new CodeDescriptionPair("ONE", "OneLogin OpenID Connect Server"), options);
					AssertCollectionContains(new CodeDescriptionPair("WTG", "WiseTech Identity Server"), options);
				});
			}
		}

		[RequiresSTA]
		public void TestUpdateButtonClickEvent()
		{
			var mockTokenAuthOnboardingService = new Mock<ITokenAuthOnboardingService>();
			mockTokenAuthOnboardingService.Setup(helper => helper.FetchOidcConfig()).Returns(new TokenAuthOnboardingDataResponse()
			{
				AuthorityUrl = "https://test.com",
				ConfigurationIdentifier = "testid",
				ClaimMappingName = "testmappingname",
				ClaimMappingIdentifier = "GlbStaff.GS_LoginName",
				DomainHint = "WC_XXX"
			});

			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(mockTokenAuthOnboardingService.Object))
			{
				zFrom.Controls.Add(control);
				zFrom.Show();

				control.UpdateButton.PerformClick();

				AssertEquals(OIDCServerTypes.Azure, SystemDataRegistry.Instance.OIDCConfig.Value.OIDCServerType);
				AssertEquals(false, SystemDataRegistry.Instance.OIDCConfig.Value.IsOIDCEnabled);
				AssertEquals("https://test.com", SystemDataRegistry.Instance.OIDCConfig.Value.AuthorityURL);
				AssertEquals("testid", SystemDataRegistry.Instance.OIDCConfig.Value.ClientIdentifier);
				AssertEquals("testmappingname", SystemDataRegistry.Instance.OIDCConfig.Value.ClaimsMappings[0].ClaimName);
				AssertEquals("GlbStaff.GS_LoginName", SystemDataRegistry.Instance.OIDCConfig.Value.ClaimsMappings[0].Identifier);
				AssertEquals(1, SystemDataRegistry.Instance.OIDCConfig.Value.Scopes.Count);
				AssertEquals("testid", SystemDataRegistry.Instance.OIDCConfig.Value.Scopes[0].ScopeName);

				AssertEquals("https://test.com", control.AuthorityURLText.Text);
				AssertEquals("testid", control.ClientIdentifierText.Text);
				AssertEquals("testmappingname", control.ClaimsMappingGrid[0, 0].ToString());
				AssertEquals("GlbStaff.GS_LoginName", control.ClaimsMappingGrid[0, 1].ToString());
				AssertEquals("testid", control.ScopesGrid[0, 0].ToString());

				AssertEquals("WC_XXX", SystemDataRegistry.Instance.DomainHint.Value);
			}
		}

		[RequiresSTA]
		public void TestUpdateButtonClickEventWithErrorRequestResponse()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var mockTokenAuthOnboardingService = new Mock<ITokenAuthOnboardingService>();
			mockTokenAuthOnboardingService.Setup(helper => helper.FetchOidcConfig()).Throws(new TokenAuthOnboardingApiException("error when updating oidcconfig", null));
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(mockTokenAuthOnboardingService.Object))
			{
				zFrom.Controls.Add(control);
				zFrom.Show();

				control.UpdateButton.PerformClick();

				AssertEquals("error when updating oidcconfig", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(SystemDataRegistry.Instance.OIDCConfig.DefaultValue, SystemDataRegistry.Instance.OIDCConfig.Value);
				AssertEquals(SystemDataRegistry.Instance.DomainHint.DefaultValue, SystemDataRegistry.Instance.DomainHint.Value);
			}
		}

		public void TestUpdateButtonClickEventWithErrorOidcConfig()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			var mockTokenAuthOnboardingService = new Mock<ITokenAuthOnboardingService>();
			mockTokenAuthOnboardingService.Setup(helper => helper.FetchOidcConfig()).Returns(new TokenAuthOnboardingDataResponse());
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(mockTokenAuthOnboardingService.Object))
			{
				zFrom.Controls.Add(control);
				zFrom.Show();

				control.UpdateButton.PerformClick();

				AssertEquals("Please enter a value.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(SystemDataRegistry.Instance.OIDCConfig.DefaultValue, SystemDataRegistry.Instance.OIDCConfig.Value);
				AssertEquals(SystemDataRegistry.Instance.DomainHint.DefaultValue, SystemDataRegistry.Instance.DomainHint.Value);
			}
		}

		[RequiresSTA]
		public void TestEnableButtonClickEventForNonProdSystem()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertEnableEvent(isProdSystem: false, isWinzorConfig: false);
		}

		[RequiresSTA]
		public void TestEnableButtonClickEventForNonProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertEnableEvent(isProdSystem: false, isWinzorConfig: true);
		}

		public void TestEnableButtonClickEventForProdSystem()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertEnableEvent(isProdSystem: true, isWinzorConfig: false);
		}

		[RequiresSTA]
		public void TestEnableButtonClickEventForProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertEnableEvent(isProdSystem: true, isWinzorConfig: true);
		}

		void AssertEnableEvent(bool isProdSystem, bool isWinzorConfig = false)
		{
			var mockTokenAuthOnboardingService = new Mock<ITokenAuthOnboardingService>();
			mockTokenAuthOnboardingService.Setup(helper => helper.EnableTokenAuthentication()).Returns(true);
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			var oidcConfigRegistryItem = isWinzorConfig ? SystemDataRegistry.Instance.WinzorOIDCConfig : SystemDataRegistry.Instance.OIDCConfig;

			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (oidcConfigRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(mockTokenAuthOnboardingService.Object, isWinzorConfig))
			{
				zFrom.Controls.Add(control);
				control.Value = oidcConfig;
				zFrom.Show();
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);
				AssertEquals("Enable after double check with support user:", control.EnableLabel.Text);
				AssertEquals("Enable", control.EnableButton.Text);
				AssertEquals(true, control.EnableButton.Enabled);
				AssertEquals(true, control.UpdateButton.Enabled);
				AssertEquals(true, control.VerifyButton.Enabled);

				control.Value.IsVerified = true; // mock verified
				control.EnableButton.PerformClick();

				AssertEquals(true, control.OIDCEnabledCheckBox.Checked);
				var expectedText = isProdSystem && !isWinzorConfig ? "Cannot disable it for PRD system." : "Disable the token authentication:";
				AssertEquals(expectedText, control.EnableLabel.Text);
				AssertEquals("Disable", control.EnableButton.Text);
				AssertEquals("For PRD system, we should not allow user to disable it.", !isProdSystem || isWinzorConfig, control.EnableButton.Enabled);
				AssertEquals(false, control.UpdateButton.Enabled);
				AssertEquals(false, control.VerifyButton.Enabled);

				AssertEquals(true, oidcConfigRegistryItem.Value.IsOIDCEnabled);
			}
		}

		public void TestEnableButtonClickEventWithErrors_ForNonProdSystem()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertEnableEventWithError_NonTokenAuthOnboardingService(false);
		}

		public void TestEnableButtonClickEventWithErrors_ForNonProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertEnableEventWithError_NonTokenAuthOnboardingService(true);
		}

		void AssertEnableEventWithError_NonTokenAuthOnboardingService(bool isWinzorConfig)
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			var oidcConfigRegistryItem = isWinzorConfig ? SystemDataRegistry.Instance.WinzorOIDCConfig : SystemDataRegistry.Instance.OIDCConfig;

			using (oidcConfigRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(isWinzorConfig))
			{
				zFrom.Controls.Add(control);
				control.Value = oidcConfig;
				zFrom.Show();
				AssertEquals(false, oidcConfigRegistryItem.Value.IsOIDCEnabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);

				control.EnableButton.PerformClick();

				AssertEquals("You must verify the settings before you can enable them.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, oidcConfigRegistryItem.Value.IsOIDCEnabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);
			}
		}

		public void TestEnableButtonClickEventWithErrors_ForProdSystem()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var mockTokenAuthOnboardingService = new Mock<ITokenAuthOnboardingService>();
			mockTokenAuthOnboardingService.Setup(helper => helper.EnableTokenAuthentication()).Throws(new TokenAuthOnboardingApiException("error when enable token auth.", null));
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });

			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(mockTokenAuthOnboardingService.Object))
			{
				zFrom.Controls.Add(control);
				control.Value = oidcConfig;
				zFrom.Show();
				AssertEquals(false, SystemDataRegistry.Instance.OIDCConfig.Value.IsOIDCEnabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);

				oidcConfig.IsVerified = true;
				control.EnableButton.PerformClick();

				AssertEquals("error when enable token auth.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, SystemDataRegistry.Instance.OIDCConfig.Value.IsOIDCEnabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);
			}
		}

		public void TestEnableButtonClickEventWithErrors_ForProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertEnableEventWithError_NonTokenAuthOnboardingService(true);
		}

		[RequiresSTA]
		public void TestDisableButtonClickEvent_ForNonProdSystem()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertDisableEvent(isWinzorConfig: false);
		}

		[RequiresSTA]
		public void TestDisableButtonClickEvent_ForNonProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			AssertDisableEvent(isWinzorConfig: true);
		}

		public void TestDisableButtonClickEvent_ForProdSystemAndIsWinzorConfig()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			AssertDisableEvent(isWinzorConfig: true);
		}

		void AssertDisableEvent(bool isWinzorConfig = false)
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;
			var oidcConfigRegistryItem = isWinzorConfig ? SystemDataRegistry.Instance.WinzorOIDCConfig : SystemDataRegistry.Instance.OIDCConfig;

			EnvProxy.SetHostedLocationForTest("SYD");
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (oidcConfigRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest(isWinzorConfig))
			{
				zFrom.Controls.Add(control);
				control.Value = oidcConfig;
				zFrom.Show();
				AssertEquals(true, control.OIDCEnabledCheckBox.Checked);
				AssertEquals("Disable the token authentication:", control.EnableLabel.Text);
				AssertEquals("Disable", control.EnableButton.Text);
				AssertEquals(true, control.EnableButton.Enabled);
				AssertEquals(false, control.UpdateButton.Enabled);
				AssertEquals(false, control.VerifyButton.Enabled);

				control.EnableButton.PerformClick();

				AssertEquals(false, oidcConfigRegistryItem.Value.IsOIDCEnabled);
				AssertEquals(false, control.OIDCEnabledCheckBox.Checked);
				AssertEquals("Enable after double check with support user:", control.EnableLabel.Text);
				AssertEquals("Enable", control.EnableButton.Text);
				AssertEquals(true, control.EnableButton.Enabled);
				AssertEquals(true, control.UpdateButton.Enabled);
				AssertEquals(true, control.VerifyButton.Enabled);
			}
		}

		public void TestDisableButtonIsDisabled_ForProdSystem()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = "https://test.com",
				ClientIdentifier = "testid",
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;

			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				zFrom.Controls.Add(control);
				control.Value = oidcConfig;
				zFrom.Show();
				AssertEquals(true, control.OIDCEnabledCheckBox.Checked);
				AssertEquals("Cannot disable it for PRD system.", control.EnableLabel.Text);
				AssertEquals("Disable", control.EnableButton.Text);
				AssertEquals(false, control.EnableButton.Enabled);
				AssertEquals(false, control.UpdateButton.Enabled);
				AssertEquals(false, control.VerifyButton.Enabled);

				control.EnableButton.Enabled = true; // force to perform click to trigger report once
				control.EnableButton.PerformClick(); // force to perform click to trigger report once

				var expectedMessage = "It's not support to disable OIDC authentication in Prod system. System Identifier: EDIDAT";
				AssertEquals("Should report once if disable event is triggered in PROD system.", expectedMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestVerifyWithOneLogin()
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>();

			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				var oidcConfig = new OIDCConfig
				{
					IsOIDCEnabled = true,
					AuthorityURL = "https://localhost:8888",
					ClientIdentifier = "interactive.public",
				};
				control.Value = oidcConfig;
				zFrom.Controls.Add(control);
				zFrom.Show();

				oidcConfig.OIDCServerTypeCode = "ONE";
				control.VerifyButton.PerformClick();

				while (!control.VerifyButton.Enabled)
				{
					Application.DoEvents();
				}

				mockOIDCLoginServer.Verify(expression: s => s.LoginLocal(
					It.Is<OIDCLoginRequestMessage>(m => m.Scopes.SequenceEqual(new[] { "openid" }) && m.ServerType == OIDCLoginRequestMessage.OIDCServer.OneLogin),
					It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<OIDCLoginFactory>()), Times.Once);
			}
		}

		public void TestManuallyCancelTheVerificationByClosingTheMessageBox()
		{
			var mockIOIDCLoginServer = MockIOIDCLoginServer();

			using (ObjectFactory.Substitute(mockIOIDCLoginServer))
			using (var zFrom = new ZForm())
			using (var control = GetNewControl() as OIDCRegistryControl)
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = true,
					OIDCServerType = OIDCServerTypes.Generic,
					AuthorityURL = $"https://localhost:8888",
					ClientIdentifier = "interactive.public",
				};
				control.Value = oidcConfig;
				zFrom.Controls.Add(control);
				zFrom.Show();

				var verify = control.Controls.Find("VerifyButton", true).First() as ZButton;
				ZFormModaliser.ShowDialogsInTest = true;
				verify.PerformClick();

				while (!verify.Enabled)
				{
					Application.DoEvents();
				}

				var message = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
				AssertEquals("Verifying OpenID Connect Settings. In order to verify, you need to login as a controller user in the web pop up window, click \"Cancel\" to cancel the verification process.", message.Message);
				Assert("Should be closed.", message.IsDisposed);
				var notification = control.Value.Notifications.First().Message;
				AssertEquals("Error - record: Authentication operation was canceled, please try again.\r\n", notification);
			}

			var config = ObjectFactory.Get<IOIDCConfig>();
			AssertEquals("Oidc server Should not be enabled because we don't save it.", false, config.IsOIDCEnabled);
		}

		public void TestOIDCConfigRegistryItem()
		{
			using (var control = new OIDCRegistryControlForTest(true))
			{
				AssertEquals(SystemDataRegistry.Instance.WinzorOIDCConfig, control.OIDCConfigRegistryItem);
			}

			using (var control = new OIDCRegistryControlForTest(false))
			{
				AssertEquals(SystemDataRegistry.Instance.OIDCConfig, control.OIDCConfigRegistryItem);
			}
		}

		[UseSnapshotProtection(true)]
		public void TestVerifyCargowiseCloud()
		{
			TestVerify("SYD", operational: true, verify: true);
			TestVerify("SYD", operational: false, verify: true);
		}

		[UseSnapshotProtection(true)]
		public void TestVerifySelfHosted()
		{
			TestVerify("NCW", operational: true, verify: false);
			TestVerify("NCW", operational: false, verify: true);
		}

		[UseSnapshotProtection(true)]
		[RequiresSTA]
		public void TestVerify_HostedSystem_NonSupportUser()
		{
			var registration = new Mock<IProductRegistration>();
			var regKey = new Mock<IProductRegistrationKey>();
			registration.Setup(r => r.Key).Returns(regKey.Object);
			registration.Setup(reg => reg.IsWiseTechGlobalInternalEDISystem()).Returns(false);
			regKey.Setup(r => r.HostedLocation).Returns("SYD");

			using (ObjectFactory.Substitute(registration.Object))
			using (Env.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				TestVerify(true);
				TestVerify(false);
			}
		}

		void TestVerify(bool verifySucceeded)
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var mockIOIDCLoginServer = verifySucceeded ? MockIOIDCLoginServer(user) : MockIOIDCLoginServer();

			var config = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				AuthorityURL = "https://localhost:8888",
				ClientIdentifier = "interactive.public",
				OIDCServerType = OIDCServerTypes.Generic,
				IsVerified = false
			};

			config.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "ClaimName",
				Identifier = "GlbStaff.GS_LoginName"
			});

			using (ObjectFactory.Substitute(mockIOIDCLoginServer))
			using (var zFrom = new ZForm())
			using (var control = new OIDCRegistryControlForTest())
			{
				control.Value = config;
				zFrom.Controls.Add(control);
				zFrom.Show();
				var button = control.VerifyButton;
				ZFormModaliser.ShowDialogsInTest = true;
				button.PerformClick();
				while (!button.Enabled)
				{
					Application.DoEvents();
				}

				AssertEquals(verifySucceeded, config.IsVerified);
				AssertEquals(verifySucceeded, control.VerifyResultCheckBox.Checked);

				if (!verifySucceeded)
				{
					var expectedError = "Authentication operation was canceled, please try again.\r\n";
					AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void TestVerify(string location, bool operational, bool verify)
		{
			var registration = new Mock<IProductRegistration>();
			var regKey = new Mock<IProductRegistrationKey>();
			registration.Setup(r => r.Key).Returns(regKey.Object);
			regKey.Setup(r => r.HostedLocation).Returns(location);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_IsOperational = operational;
			Factory.Save();
			var mockIOIDCLoginServer = MockIOIDCLoginServer(user);

			var config = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				AuthorityURL = "https://localhost:8888",
				ClientIdentifier = "interactive.public",
				OIDCServerType = OIDCServerTypes.Generic,
				IsVerified = false
			};

			config.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "ClaimName",
				Identifier = "GlbStaff.GS_LoginName"
			});

			using (ObjectFactory.Substitute(registration.Object))
			using (ObjectFactory.Substitute(mockIOIDCLoginServer))
			using (var zFrom = new ZForm())
			using (var control = GetNewControl() as OIDCRegistryControl)
			{
				control.Value = config;
				zFrom.Controls.Add(control);
				zFrom.Show();
				var button = control.Controls.Find("VerifyButton", true).First() as ZButton;
				ZFormModaliser.ShowDialogsInTest = true;
				button.PerformClick();
				while (!button.Enabled)
				{
					Application.DoEvents();
				}

				AssertEquals(verify, control.Value.IsVerified);
			}
		}

		IOIDCLoginServer MockIOIDCLoginServer(GlbStaff user = null)
		{
			var mockIOIDCLoginServer = new Mock<IOIDCLoginServer>();
			if (user != null)
			{
				var claim = new System.Security.Claims.Claim("ClaimName", user.GS_LoginName);
				var token = new JwtSecurityToken(claims: new System.Security.Claims.Claim[] { claim });
				mockIOIDCLoginServer.Setup(s => s.ValidateIdentityToken(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCLoginResponseMessage>(), It.IsAny<CancellationToken>()))
					.Returns(token);
			}

			mockIOIDCLoginServer.Setup(server => server.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCWebLauncher>(), It.IsAny<CancellationToken>(), null))
				.Returns((OIDCLoginRequestMessage a, OIDCWebLauncher b, CancellationToken c, OIDCLoginFactory d) =>
				{
					if (user != null)
					{
						return OIDCLoginResponseMessage.CreateSuccessResponse("idToken", "accessToken", DateTimeOffset.MaxValue);
					}

					var waitCount = 0;

					while (!c.IsCancellationRequested && waitCount++ < 3)
					{
						Thread.Sleep(500);
					}

					if (c.IsCancellationRequested)
					{
						return OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled, "OperationCanceled", "This operation is cancelled by test case.");
					}

					return OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.LoginFailed, "LoginFailed", "The login return failed in test case.");
				});

			return mockIOIDCLoginServer.Object;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return OIDCConfig.DefaultValue;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OIDCRegistryControl)control).ReadOnly;
		}

		class OIDCRegistryControlForTest : OIDCRegistryControl
		{
			public OIDCRegistryControlForTest(bool isWinzorConfig = false) : this(null, isWinzorConfig) { }

			public OIDCRegistryControlForTest(ITokenAuthOnboardingService systemToSystemTrustHelper, bool isWinzorConfig = false) : base(systemToSystemTrustHelper, isWinzorConfig)
			{
			}

			public new void SetControlOrBusinessEntityReadOnly(bool readOnly) => base.SetControlOrBusinessEntityReadOnly(readOnly);

			public ZLabel UpdateLabel => Controls.Find("updateLabel", true).FirstOrDefault() as ZLabel;
			public ZLabel VerifyLabel => Controls.Find("verifyLabel", true).FirstOrDefault() as ZLabel;
			public ZLabel EnableLabel => Controls.Find("enableLabel", true).FirstOrDefault() as ZLabel;
			public ZButton UpdateButton => Controls.Find("updateButton", true).FirstOrDefault() as ZButton;
			public ZButton VerifyButton => Controls.Find("verifyButton", true).FirstOrDefault() as ZButton;
			public ZButton EnableButton => Controls.Find("enableButton", true).FirstOrDefault() as ZButton;
			public ZCheckBox VerifyResultCheckBox => Controls.Find("verifyResultCheckBox", true).FirstOrDefault() as ZCheckBox;
			public ZLabel UpdateNoteHintLabel => Controls.Find("updateNoteHintLabel", true).FirstOrDefault() as ZLabel;
			public ZLinkLabel UpdateNoteLinkLabel => Controls.Find("updateNoteLinkLabel", true).FirstOrDefault() as ZLinkLabel;
			public ZCheckBox OIDCEnabledCheckBox => Controls.Find("enableCheckBox", true).FirstOrDefault() as ZCheckBox;
			public ZDropEdit OIDCServerTypeDropEdit => Controls.Find("serverTypeDropEdit", true).FirstOrDefault() as ZDropEdit;
			public ZGroupBox OptionGroupBox => Controls.Find("OptionGroupBox", true).FirstOrDefault() as ZGroupBox;
			public ZTextBox AuthorityURLText => Controls.Find("AuthorityURLText", true).FirstOrDefault() as ZTextBox;
			public ZTextBox ClientIdentifierText => Controls.Find("ClientIdentifierText", true).FirstOrDefault() as ZTextBox;
			public ZGrid ClaimsMappingGrid => Controls.Find("ClaimsMappingGrid", true).FirstOrDefault() as ZGrid;
			public ZGrid ScopesGrid => Controls.Find("ScopesGrid", true).FirstOrDefault() as ZGrid;
		}
	}
}
