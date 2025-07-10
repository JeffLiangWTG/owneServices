using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Registry.GUI
{
	public partial class OIDCRegistryControl : RegistryZUserControl
	{
		public OIDCRegistryControl() : this(new TokenAuthOnboardingService(), false)
		{
		}

		public OIDCRegistryControl(bool isWinzorConfig) : this(new TokenAuthOnboardingService(), isWinzorConfig)
		{
		}

		protected OIDCRegistryControl(ITokenAuthOnboardingService tokenAuthOnboardingApiHelper, bool isWinzorConfig = false)
		{
			this.tokenAuthOnboardingApiHelper = tokenAuthOnboardingApiHelper;
			this.OIDCConfigRegistryItem = isWinzorConfig ? SystemDataRegistry.Instance.WinzorOIDCConfig : SystemDataRegistry.Instance.OIDCConfig;

			InitializeComponent();

			InitializeOptionGroupBox();
		}

		public OIDCConfig Value
		{
			get { return oidcConfig; }
			set
			{
				oidcConfig = value;
				SetDataBinding(value, "");
			}
		}
		OIDCConfig oidcConfig;

		async Task<LoginAuthenticationInfo> ShowOIDCAuthenticationMessageBox()
		{
			using (var oidcAuthenticationMessageBox = new OIDCAuthenticationMessageBox(this))
			{
				return await oidcAuthenticationMessageBox.PerformLogin(oidcConfig.Clone());
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			OptionGroupBox.Enabled = IsHostedSystemWithNonSupportUser ? CheckRegistryEditSecurityRights() : !readOnly;

			AuthorityURLText.ReadOnly = readOnly;
			ClientIdentifierText.ReadOnly = readOnly;
			ClaimsMappingGrid.ReadOnly = readOnly;
			ScopesGrid.ReadOnly = readOnly;
		}

		bool CheckRegistryEditSecurityRights()
		{
			var checkpoint = Env.Security.GetRegistryCheckPoint(OIDCConfigRegistryItem.Name, OIDCConfigRegistryItem.Caption);
			return checkpoint.IsAllowed && Env.Security.SystemRegistryEdit.IsAllowed;
		}

		void UpdateNoteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				WebUrlLauncher.Launch(UpdateNoteUrl);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("AEC0BF57-39E5-4B1F-9894-04AC3254512C", "Error displaying update note"));
			}
		}

		const string UpdateNoteUrl = "https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseUpdateNote20220310.pdf";

		void UpdateButton_Click(object sender, EventArgs e)
		{
			try
			{
				var onboardingData = tokenAuthOnboardingApiHelper.FetchOidcConfig();

				var oidcConfig = ConvertToOidcConfig(onboardingData);

				using (var transaction = Db.Connection.BeginTransactionWithManager())
				{
					OIDCConfigRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);

					if (onboardingData.DomainHint != SystemDataRegistry.Instance.DomainHint.DefaultValue)
					{
						SystemDataRegistry.Instance.DomainHint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, onboardingData.DomainHint);
					}
					transaction.CommitTransaction();
				}
				OIDCConfigRegistryItem.Inner.ClearCurrentValueToUseCache();

				Value = oidcConfig;
			}
			catch (TokenAuthOnboardingApiException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (RegistryValidationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		OIDCConfig ConvertToOidcConfig(TokenAuthOnboardingDataResponse config)
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = false,
				OIDCServerType = Integration.OIDCServerTypes.Azure,
				AuthorityURL = config.AuthorityUrl,
				ClientIdentifier = config.ConfigurationIdentifier
			};

			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping
			{
				ClaimName = config.ClaimMappingName,
				Identifier = config.ClaimMappingIdentifier,
			});

			oidcConfig.Scopes.Add(new OIDCScope()
			{
				ScopeName = config.ConfigurationIdentifier
			});

			return oidcConfig;
		}

		async void VerifyButton_Click(object sender, EventArgs e)
		{
			var verifyButton = sender as ZButton;
			try
			{
				verifyButton.Enabled = false;
				Value.ClearRowNotifications();
				var authenticationInfo = await ShowOIDCAuthenticationMessageBox();

				if (authenticationInfo.LoginValidated)
				{
					if (EnvProxy.IsHostedWithCargowise)
					{
						Value.IsVerified = true;
					}
					else if (authenticationInfo.User.IsOperational)
					{
						Value.AddRowError(Res.GetString("c966d32a-7e9e-4490-9b5e-09e17bd88789", "The user matched the retrieved identity information is flagged as an operational user. Only non-operational users can enable and save this registry item."));
					}
					else
					{
						using (Env.SetTemporaryUserContext(new UserContext(authenticationInfo.User, Env.CurrentUserContext.Branch.PK, Env.CurrentUserContext.Department.PK)))
						{
							var registryEditCheckpoint = Env.Security.SystemRegistryEdit;

							if (registryEditCheckpoint.IsAllowed)
							{
								Value.IsVerified = true;
							}
							else
							{
								Value.AddRowError(Res.GetString("8B8F8624-28F5-49DA-8F11-2C35402295D3", "You must log in with a user who has the rights to edit this registry item to save these changes"));
							}
						}
					}
				}
				else
				{
					var message = authenticationInfo.FailureMessage + System.Environment.NewLine + authenticationInfo.ExtendedErrorInformation;

					if (IsHostedSystemWithNonSupportUser)
					{
						Globals.Message.ShowError(message);
					}
					else
					{
						Value.AddRowError(message);
					}
				}
			}
			finally
			{
				verifyButton.Enabled = true;
			}
		}

		void EnableDisableButton_Click(object sender, EventArgs e)
		{
			if (OIDCConfigRegistryItem.Value.IsOIDCEnabled)
			{
				DisableOIDC();
			}
			else
			{
				EnableOIDC();
			}
		}

		void DisableOIDC()
		{
			if (IsOIDCConfigInProdSystem)
			{
				ErrorReporter.ReportOnce("DisableOIDCForProdSystem", $"It's not support to disable OIDC authentication in Prod system. System Identifier: {ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}");
			}
			else
			{
				SetEnable(false);
			}
		}

		void EnableOIDC()
		{
			if (Value.IsVerified)
			{
				if (IsOIDCConfigInProdSystem)
				{
					try
					{
						if (tokenAuthOnboardingApiHelper.EnableTokenAuthentication())
						{
							SetEnable(true);
						}
					}
					catch (TokenAuthOnboardingApiException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
				else
				{
					SetEnable(true);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("8D35F35F-57F4-42E2-A008-548F46A2AB03", "You must verify the settings before you can enable them."));
			}
		}

		void SetEnable(bool enable)
		{
			var enableOrDisable = enable ? enableString : disableString;
			if (Globals.Message.ShowConfirmation(Res.GetString("EDE07E20-BD2D-4877-9F7C-330293E06E5C", "Are you sure to {0} OIDC authentication?", enableOrDisable), Res.GetString("25C1A8F6-8117-4875-A7CC-59C779C77B63", "{0} OIDC authentication", enableOrDisable), enableOrDisable, MessageBoxIcon.Warning) != DialogResult.OK)
			{
				return;
			}

			try
			{
				var oidcConfig = OIDCConfigRegistryItem.Value;
				oidcConfig.IsOIDCEnabled = enable;
				oidcConfig.IsVerified = enable;

				OIDCConfigRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
				OIDCConfigRegistryItem.Inner.ClearCurrentValueToUseCache();

				Value = oidcConfig;
				UpdateControls();
			}
			catch (RegistryValidationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		ZLabel enableLabel;
		ZButton enableButton;
		ZButton updateButton;
		ZButton verifyButton;
		readonly ITokenAuthOnboardingService tokenAuthOnboardingApiHelper;
		internal readonly OIDCConfigRegistryItem OIDCConfigRegistryItem;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Option string")]
		const string enableString = "Enable";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Option string")]
		const string disableString = "Disable";

		bool IsHostedSystemWithNonSupportUser => EnvProxy.IsHostedWithCargowise && !EnvProxy.Instance.CurrentUser.IsSupportUser;

		bool IsProdSystem => ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production;

		bool IsOIDCConfigInProdSystem => IsProdSystem && OIDCConfigRegistryItem == SystemDataRegistry.Instance.OIDCConfig;

		#region InitiallizeOptionGroupBox

		void InitializeOptionGroupBox()
		{
			if (IsHostedSystemWithNonSupportUser)
			{
				IntializeOptionGroupboxForHostedUsers();

				UpdateControls();
			}
			else
			{
				IntializeOptionGroupboxForSelfHostedUsersAndSupportUser();
			}
		}

		void IntializeOptionGroupboxForSelfHostedUsersAndSupportUser()
		{
			var updateNoteHintLabel = new ZLabel();
			updateNoteHintLabel.AutoSize = true;
			updateNoteHintLabel.CaptionResourceString = Res.GetData("CB8373F7-A94B-4922-A0FE-257103810865", "Please read this update note before using this feature:");
			updateNoteHintLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			updateNoteHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 30, true);
			updateNoteHintLabel.Name = "updateNoteHintLabel";
			updateNoteHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 13, true);
			updateNoteHintLabel.TabIndex = 11;

			var updateNoteLinkLabel = new ZLinkLabel();
			updateNoteLinkLabel.AutoSize = true;
			updateNoteLinkLabel.CaptionResourceString = Res.GetData("1F317F36-EDF6-44EE-8C43-F3B1C1CA265B", "OpenID Connect Configuration");
			updateNoteLinkLabel.IsFontBold = false;
			updateNoteLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 30, true);
			updateNoteLinkLabel.Name = "updateNoteLinkLabel";
			updateNoteLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 13, true);
			updateNoteLinkLabel.TabIndex = 0;
			updateNoteLinkLabel.LinkClicked += UpdateNoteLinkLabel_LinkClicked;

			var enableCheckBox = new ZCheckBox();
			enableCheckBox.AutoSize = true;
			BindingSource.SetBindingMember(enableCheckBox, "IsOIDCEnabled");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OIDCConfig)null).IsOIDCEnabled);
			enableCheckBox.FlatStyle = FlatStyle.System;
			enableCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			enableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 67, true);
			enableCheckBox.Name = "enableCheckBox";
			enableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			enableCheckBox.TabIndex = 1;
			enableCheckBox.UseVisualStyleBackColor = true;

			var serverTypeDropEdit = new ZDropEdit();
			serverTypeDropEdit.AllowDrop = true;
			BindingSource.SetBindingMember(serverTypeDropEdit, "OIDCServerTypeCode");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((((OIDCConfig)null).OIDCServerTypeCode));
			serverTypeDropEdit.CharacterCasing = CharacterCasing.Normal;
			serverTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 65, true);
			serverTypeDropEdit.Name = "serverTypeDropEdit";
			serverTypeDropEdit.PreBoundMaxLength = 3;
			serverTypeDropEdit.ShouldResizeByMaxLength = true;
			serverTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			serverTypeDropEdit.TabIndex = 2;

			var verifyLabel = new ZLabel();
			verifyLabel.CaptionResourceString = Res.GetData("040BA0E6-3AF7-452A-B425-0C8A6402E377", "Before saving, verify the configuration by first logging in:");
			verifyLabel.FontType = (OFontTypes.Normal | OFontTypes.SansSerif);
			verifyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 100, true);
			verifyLabel.Name = "verifyLabel";
			verifyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 31, true);
			verifyLabel.TabIndex = 12;

			var verifyButton = new ZButton();
			verifyButton.CaptionResourceString = Res.GetData("D8097B01-A1FD-4765-9F31-6CACD1588A4E", "Verify");
			verifyButton.IsCaptionOverridden = false;
			verifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 100, true);
			verifyButton.Name = "verifyButton";
			verifyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			verifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			verifyButton.TabIndex = 13;
			verifyButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			verifyButton.ToolTipCaption = null;
			verifyButton.UseVisualStyleBackColor = true;
			verifyButton.Click += VerifyButton_Click;

			OptionGroupBox.Controls.Add(verifyButton);
			OptionGroupBox.Controls.Add(verifyLabel);
			OptionGroupBox.Controls.Add(serverTypeDropEdit);
			OptionGroupBox.Controls.Add(enableCheckBox);
			OptionGroupBox.Controls.Add(updateNoteLinkLabel);
			OptionGroupBox.Controls.Add(updateNoteHintLabel);
		}

		void IntializeOptionGroupboxForHostedUsers()
		{
			var updateLabel = new ZLabel();
			updateLabel.AutoSize = true;
			updateLabel.CaptionResourceString = Res.GetData("7793F9FE-A489-4C8A-A556-194EF46BD80A", "Update configuration before verification:");
			updateLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			updateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 20, true);
			updateLabel.Name = "updateLabel";
			updateLabel.TabIndex = 11;

			updateButton = new ZButton();
			updateButton.CaptionResourceString = Res.GetData("75C98401-DF11-40BA-B818-AD1701715840", "Update");
			updateButton.IsCaptionOverridden = false;
			updateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 15, true);
			updateButton.Name = "updateButton";
			updateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			updateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			updateButton.TabIndex = 13;
			updateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			updateButton.ToolTipCaption = null;
			updateButton.UseVisualStyleBackColor = true;
			updateButton.Click += UpdateButton_Click;

			var verifyLabel = new ZLabel();
			verifyLabel.AutoSize = true;
			verifyLabel.CaptionResourceString = Res.GetData("5B88C02B-A64F-4641-9835-3201ABE1EE80", "Verify the configuration by first logging in:");
			verifyLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			verifyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 50, true);
			verifyLabel.Name = "verifyLabel";
			verifyLabel.TabIndex = 11;

			verifyButton = new ZButton();
			verifyButton.CaptionResourceString = Res.GetData("D8094301-A1FD-4765-9F31-6CACD1534A4E", "Verify");
			verifyButton.IsCaptionOverridden = false;
			verifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 45, true);
			verifyButton.Name = "verifyButton";
			verifyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			verifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			verifyButton.TabIndex = 13;
			verifyButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			verifyButton.ToolTipCaption = null;
			verifyButton.UseVisualStyleBackColor = true;
			verifyButton.Click += VerifyButton_Click;

			var verifyResult = new ZCheckBox();
			verifyResult.AutoSize = true;
			BindingSource.SetBindingMember(verifyResult, "IsVerified");
			verifyResult.CaptionResourceString = Res.GetData("4F77FA6D-352D-4883-AA93-5321ABB51577", "Verified");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OIDCConfig)(null)).IsOIDCEnabled);
			verifyResult.FlatStyle = FlatStyle.System;
			verifyResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			verifyResult.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 50, true);
			verifyResult.Name = "verifyResultCheckBox";
			verifyResult.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			verifyResult.TabIndex = 1;
			verifyResult.UseVisualStyleBackColor = true;
			verifyResult.ReadOnly = true;

			enableLabel = new ZLabel();
			enableLabel.AutoSize = true;
			enableLabel.CaptionResourceString = Res.GetData("CE052859-1BE7-4A3A-A5F6-275A3E43DDF4", "Enable after double check with support user:");
			enableLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			enableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 80, true);
			enableLabel.Name = "enableLabel";
			enableLabel.TabIndex = 11;

			enableButton = new ZButton();
			enableButton.CaptionResourceString = Res.GetData("5B3D6B2E-66B3-4528-A094-DC34591EEE96", "Enable");
			enableButton.IsCaptionOverridden = false;
			enableButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 75, true);
			enableButton.Name = "enableButton";
			enableButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			enableButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			enableButton.TabIndex = 13;
			enableButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			enableButton.ToolTipCaption = null;
			enableButton.UseVisualStyleBackColor = true;
			enableButton.Click += EnableDisableButton_Click;

			var enableCheckBox = new ZCheckBox();
			enableCheckBox.AutoSize = true;
			BindingSource.SetBindingMember(enableCheckBox, "IsOIDCEnabled");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OIDCConfig)(null)).IsOIDCEnabled);
			enableCheckBox.FlatStyle = FlatStyle.System;
			enableCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			enableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 110, true);
			enableCheckBox.Name = "enableCheckBox";
			enableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			enableCheckBox.TabIndex = 1;
			enableCheckBox.UseVisualStyleBackColor = true;
			enableCheckBox.Enabled = false;

			var serverTypeDropEdit = new ZDropEdit();
			serverTypeDropEdit.AllowDrop = true;
			BindingSource.SetBindingMember(serverTypeDropEdit, "OIDCServerTypeCode");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OIDCConfig)null).OIDCServerTypeCode);
			serverTypeDropEdit.CharacterCasing = CharacterCasing.Normal;
			serverTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 108, true);
			serverTypeDropEdit.Name = "serverTypeDropEdit";
			serverTypeDropEdit.PreBoundMaxLength = 3;
			serverTypeDropEdit.ShouldResizeByMaxLength = true;
			serverTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			serverTypeDropEdit.TabIndex = 2;
			serverTypeDropEdit.Enabled = false;

			OptionGroupBox.Controls.Add(updateLabel);
			OptionGroupBox.Controls.Add(updateButton);
			OptionGroupBox.Controls.Add(verifyLabel);
			OptionGroupBox.Controls.Add(verifyButton);
			OptionGroupBox.Controls.Add(verifyResult);
			OptionGroupBox.Controls.Add(enableLabel);
			OptionGroupBox.Controls.Add(enableButton);
			OptionGroupBox.Controls.Add(enableCheckBox);
			OptionGroupBox.Controls.Add(serverTypeDropEdit);
		}

		void UpdateControls()
		{
			if (OIDCConfigRegistryItem.Value.IsOIDCEnabled)
			{
				updateButton.Enabled = false;
				verifyButton.Enabled = false;

				enableButton.Text = Res.GetString("3AC6F589-C240-46CA-A10A-868C6833F58D", "Disable");
				enableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 80, true);

				if (IsOIDCConfigInProdSystem)
				{
					enableLabel.Text = Res.GetString("3A475E44-C5B3-4967-B4E1-243749E9E8CE", $"Cannot disable it for {DatabaseTypes.Codes.Production} system.");
					enableButton.Enabled = false;
				}
				else
				{
					enableLabel.Text = Res.GetString("E73499EF-4276-41F9-BA9B-938338FEF9CC", "Disable the token authentication:");
				}
			}
			else
			{
				updateButton.Enabled = true;
				verifyButton.Enabled = true;

				enableLabel.Text = Res.GetString("CE052859-1BE7-4A3A-A5F6-275A3E43DDF4", "Enable after double check with support user:");
				enableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 80, true);

				enableButton.Text = Res.GetString("5B3D6B2E-66B3-4528-A094-DC34591EEE96", "Enable");
			}
		}

		#endregion
	}
}
