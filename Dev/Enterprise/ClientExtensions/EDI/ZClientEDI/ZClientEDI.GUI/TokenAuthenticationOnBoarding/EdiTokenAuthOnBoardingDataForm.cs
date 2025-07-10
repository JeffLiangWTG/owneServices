using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.GUI
{
	public partial class EdiTokenAuthOnBoardingDataForm : ZTemplateForm
	{
		EdiTokenAuthOnBoardingData EdiTokenAuthOnBoardingData => DataSource as EdiTokenAuthOnBoardingData;

		public EdiTokenAuthOnBoardingDataForm(EdiTokenAuthOnBoardingData config)
			: base(config)
		{
			ControllerID = ClientControllerRegistration.TokenAuthenticationOnBoarding;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var oldDataSource = DataSource;
			if (oldDataSource is EdiTokenAuthOnBoardingData oldEdiTokenAuthOnBoardingData)
			{
				oldEdiTokenAuthOnBoardingData.TOD_OIDCServerInfo.ValueChanged -= UpdateConfigurationIdentifierFromOidcServer;
				oldEdiTokenAuthOnBoardingData.TOD_StatusInfo.ValueChanged -= UpdateMenuItems;
				oldEdiTokenAuthOnBoardingData.TOD_VerificationUsernameInfo.ValueChanged -= UpdateVerificationUsernameButtonEnabled;
				oldEdiTokenAuthOnBoardingData.TOD_VerificationUserPasswordInfo.ValueChanged -= UpdateVerificationUserPasswordButtonEnabled;
				oldEdiTokenAuthOnBoardingData.HasChangesChanged -= UpdateMenuItems;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is EdiTokenAuthOnBoardingData newEdiTokenAuthOnBoardingData)
			{
				newEdiTokenAuthOnBoardingData.TOD_OIDCServerInfo.ValueChanged += UpdateConfigurationIdentifierFromOidcServer;
				newEdiTokenAuthOnBoardingData.TOD_StatusInfo.ValueChanged += UpdateMenuItems;
				newEdiTokenAuthOnBoardingData.TOD_VerificationUsernameInfo.ValueChanged += UpdateVerificationUsernameButtonEnabled;
				newEdiTokenAuthOnBoardingData.TOD_VerificationUserPasswordInfo.ValueChanged += UpdateVerificationUserPasswordButtonEnabled;
				newEdiTokenAuthOnBoardingData.HasChangesChanged += UpdateMenuItems;
			}
			UpdateLayout();
		}

		protected virtual IDisposableOIDCAuthenticationMessageBox BuildOIDCAuthenticationMessageBox() => new OIDCAuthenticationMessageBox(this);

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateLayout();
		}

		void UpdateLayout()
		{
			UpdateMenuItems();
			UpdateVerificationUsernameButtonEnabled();
			UpdateVerificationUserPasswordButtonEnabled();
			UpdatePrLinkEnabled();
			UpdateTokenBasedAuthenticationCheckBoxEnabled();
		}

		void UpdatePrLinkEnabled()
		{
			ProdPrLinkLabel.Text = !string.IsNullOrEmpty(EdiTokenAuthOnBoardingData?.TOD_ProdPRLink) ? EdiTokenAuthOnBoardingData?.TOD_ProdPRLink.ToString() : "N/A";
			StagingPrLinkLabel.Text = !string.IsNullOrEmpty(EdiTokenAuthOnBoardingData?.TOD_StagingPRLink) ? EdiTokenAuthOnBoardingData?.TOD_StagingPRLink.ToString() : "N/A";
		}

		void UpdateTokenBasedAuthenticationCheckBoxEnabled()
		{
			EnableTokenBasedAuthenticationCheckBox.Enabled = EdiTokenAuthOnBoardingData != null && EdiTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.CustomerTestCompleted;
		}

		void UpdateMenuItems(object sender, EventArgs e) => UpdateMenuItems();

		void UpdateMenuItems()
		{
			if (ProcessMenuItem.MenuItems.Count == 0)
			{
				return;
			}

			var isUpdateAllowed = EDISecurityCheckpoints.TokenAuthenticationOnBoarding.IsAllowed && !IsViewOrDeleteMode;
			var statusToCompare =
				isUpdateAllowed && DataSource is EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData && ediTokenAuthOnBoardingData.IsInDatabase && !ediTokenAuthOnBoardingData.HasChanges
					? ediTokenAuthOnBoardingData.TOD_Status
					: ZString.Empty;

			ProcessMenuItem.Enabled = isUpdateAllowed;
			StagingDeploymentMenuItem.Enabled = statusToCompare == OnBoardingStatuses.Codes.New;
			CustomerTestCompletedMenuItem.Enabled = statusToCompare == OnBoardingStatuses.Codes.Verified;
			CompletedMenuItem.Enabled = statusToCompare == OnBoardingStatuses.Codes.CustomerTestCompleted && EdiTokenAuthOnBoardingData.TOD_Enabled;
			RevertMenuItem.Enabled = !string.IsNullOrEmpty(EdiTokenAuthOnBoardingData?.TOD_StagingPRLink) || !string.IsNullOrEmpty(EdiTokenAuthOnBoardingData?.TOD_ProdPRLink);
			RestartProcessingMenuItem.Enabled = statusToCompare == OnBoardingStatuses.Codes.Error;
		}

		void StagingPrLink_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			UrlLauncher(EdiTokenAuthOnBoardingData.TOD_StagingPRLink);
		}

		void ProdPrLink_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			UrlLauncher(EdiTokenAuthOnBoardingData.TOD_ProdPRLink);
		}

		void UrlLauncher(string link)
		{
			try
			{
				WebUrlLauncher.Launch(link);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("45D5BC76-03EB-4802-8960-BACBF2C42057", "Error displaying Pr Link"));
			}
		}

		void UpdateConfigurationIdentifierFromOidcServer(object sender, EventArgs e)
		{
			string GetDefaultConfigurationIdentifier(ZString oidcServerType)
			{
				switch (oidcServerType)
				{
					case OIDCServerTypesList.Codes.Azure: return EdiTokenAuthOnBoardingData.AzureDefaultConfigurationIdentifier;
					default: return string.Empty;
				}
			}

			if ((e is ValueChangedEventArgs valueChangedEventArgs) && (DataSource is EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData) && (valueChangedEventArgs.Info == ediTokenAuthOnBoardingData.TOD_OIDCServerInfo))
			{
				var previousDefaultValue = GetDefaultConfigurationIdentifier((ZString)valueChangedEventArgs.OldValue);
				if (ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier == previousDefaultValue)
				{
					ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = GetDefaultConfigurationIdentifier(ediTokenAuthOnBoardingData.TOD_OIDCServer);
				}
			}
		}

		void UpdateVerificationUsernameButtonEnabled(object sender, EventArgs e) => UpdateVerificationUsernameButtonEnabled();

		void UpdateVerificationUsernameButtonEnabled()
		{
			VerificationUsernameButton.Enabled = !string.IsNullOrWhiteSpace(EdiTokenAuthOnBoardingData?.TOD_VerificationUsername);
		}

		void UpdateVerificationUserPasswordButtonEnabled(object sender, EventArgs e) => UpdateVerificationUserPasswordButtonEnabled();

		void UpdateVerificationUserPasswordButtonEnabled()
		{
			VerificationUserPasswordButton.Enabled = !string.IsNullOrWhiteSpace(EdiTokenAuthOnBoardingData?.TOD_VerificationUserPassword) && CanCopyPassword;
		}

		bool CanCopyPassword => EDISecurityCheckpoints.TokenAuthenticationOnBoardingCopyPassword.IsAllowed;

		void StagingDeploymentMenuItem_Click(object sender, EventArgs e)
		{
			UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.Queued);
		}

		void AddStaffMessageAndCloseIncident(string message)
		{
			EdiTokenAuthOnBoardingData.Incident.AddStaffMessageToCustomer(message);
			EdiTokenAuthOnBoardingData.Incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);
			Globals.Message.Show(Res.GetString("055b4a20-c296-4b67-a738-93eb1c4ddfa4", "A message will be sent to the customer via the CR9 incident when the form is saved"));
		}

		void CompletedMenuItemMenuItem_Click(object sender, EventArgs e)
		{
			UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.Completed);
		}

		void CustomerTestCompletedMenuItemMenuItem_Click(object sender, EventArgs e)
		{
			UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.CustomerTestCompleted);
			AddStaffMessageAndCloseIncident(@"You can now switch any CargoWise production environment to use token-based authentication. Please follow the steps outlined in the 'Integrating CargoWise with an Identity Provider' document. The specific information can be found in 'How to Integrate CargoWise with an IdP'.

Document link: https://wisetechacademy.com/search?quickstart=a19756f7-a786-4916-bf8c-c6fe3d5c5499");
		}

		void RevertMenuItem_Click(object sender, EventArgs e)
		{
			UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.Revert);
		}

		void RestartProcessingMenuItem_Click(object sender, EventArgs e)
		{
			UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.New);
		}

		void UpdateStatusAndResetRetry(string status)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			EdiTokenAuthOnBoardingData.Logs.AddNew(ZArchitecture.Business.AutoEvents.EditedARecord, $"change status from '{EdiTokenAuthOnBoardingData.TOD_Status}' to '{status}'");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			EdiTokenAuthOnBoardingData.TOD_Retry = 0;
			EdiTokenAuthOnBoardingData.TOD_Status = status;
		}

		async void VerifySettingsButton_Click(object sender, EventArgs e)
		{
			await VerifySettingsButton_Click(EdiTokenAuthOnBoardingData);
		}

		internal async Task VerifySettingsButton_Click(EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData)
		{
			if (string.IsNullOrEmpty(ediTokenAuthOnBoardingData.Environment))
			{
				Globals.Message.ShowError(Res.GetString("49CDEED8-C868-4367-B903-2F4B53AB4729", "Please select an Environment."));
				return;
			}

			if (!ediTokenAuthOnBoardingData.AzureB2CEnvironmentCodeDescriptionList.ContainsCode(ediTokenAuthOnBoardingData.Environment))
			{
				Globals.Message.ShowError(Res.GetString("66C803FA-F8F1-40F7-81D9-BD363A5B4FCE", "Select a valid Environment."));
				return;
			}

			if (ediTokenAuthOnBoardingData.Environment == AzureB2CEnvironmentCodeDescriptionList.Codes.PRD &&  ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.StagingPullRequest)
			{
				Globals.Message.ShowError(Res.GetString("01B4DB6E-83FE-42C0-8AF4-43CC48CC3C11", "Can not choose Production B2C, when the status is StagingPullRequest."));
				return;
			}

			if (ediTokenAuthOnBoardingData.Environment == AzureB2CEnvironmentCodeDescriptionList.Codes.STG
				&& ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.StagingPullRequest
				&& ediTokenAuthOnBoardingData.TOD_SystemLastEditTimeUtc > ZDateTime.UtcNow.AddMinutes(-30))
			{
				Globals.Message.ShowWarning(GetVerificationErrorMessageDueToInvalidTime(AzureB2CEnvironmentCodeDescriptionList.Descriptions.STG, ediTokenAuthOnBoardingData.TOD_SystemLastEditTimeUtc));
				return;
			}

			if (ediTokenAuthOnBoardingData.Environment == AzureB2CEnvironmentCodeDescriptionList.Codes.PRD
				&& ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.ProductionPullRequest
				&& ediTokenAuthOnBoardingData.TOD_SystemLastEditTimeUtc > ZDateTime.UtcNow.AddMinutes(-30))
			{
				Globals.Message.ShowWarning(GetVerificationErrorMessageDueToInvalidTime(AzureB2CEnvironmentCodeDescriptionList.Descriptions.PRD, ediTokenAuthOnBoardingData.TOD_SystemLastEditTimeUtc));
				return;
			}

			if ((ediTokenAuthOnBoardingData is null) || !ediTokenAuthOnBoardingData.TryGetOidcConfig(out var oidcConfig, out var domainHint))
			{
				var message = Res.GetString("CAEF8331-E2C2-4914-A348-8054E4888438", "Cannot verify OIDC settings: please click the Save button and ensure all mandatory data is filled in.");
				if (ediTokenAuthOnBoardingData != null)
				{
					ediTokenAuthOnBoardingData.VerificationResultDetails = message;
				}
				Globals.Message.ShowError(message, SettingsVerificationGroupBox.CaptionResourceString.Caption);
			}
			else
			{
				ediTokenAuthOnBoardingData.VerificationResultDetails = await ShowOidcVerificationDialog(oidcConfig, domainHint);
				if (ediTokenAuthOnBoardingData.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed)
				{
					var failMessage = "Failed to verify settings, check provided data and logs.";
					Globals.Message.ShowError(failMessage);
					ediTokenAuthOnBoardingData.Logs.AddNew(AutoEvents.ErrorReport, failMessage, ZDateTimeOffset.Now, new[]
					{
						new KeyValuePair<string, string>("VerificationResultDetails", ediTokenAuthOnBoardingData.VerificationResultDetails),
						new KeyValuePair<string, string>("Environment", ediTokenAuthOnBoardingData.Environment),
					});
					ediTokenAuthOnBoardingData.Logs.Factory.Save();
					return;
				}

				if (ediTokenAuthOnBoardingData.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success)
				{
					if (ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.StagingPullRequest)
					{
						UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.StagingMergedAndVerified);
						return;
					}

					if (ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.ProductionPullRequest && ediTokenAuthOnBoardingData.Environment == AzureB2CEnvironmentCodeDescriptionList.Codes.PRD)
					{
						UpdateStatusAndResetRetry(OnBoardingStatuses.Codes.Verified);
						AddStaffMessageAndCloseIncident(@"You can now switch any CargoWise non-production environment to use token-based authentication. Please follow the steps outlined in the 'Integrating CargoWise with an Identity Provider' document. The specific information can be found in 'How to Integrate CargoWise with an IdP'.

Document link: https://wisetechacademy.com/search?quickstart=a19756f7-a786-4916-bf8c-c6fe3d5c5499");
					}

					if (ediTokenAuthOnBoardingData.TOD_Status == OnBoardingStatuses.Codes.Verified)
					{
						ResetAccount();
					}
				}
			}
		}

		string GetVerificationErrorMessageDueToInvalidTime(string env, ZDateTime lastEditTimeUtc)
		{
			var localTime = lastEditTimeUtc.ToDateTime().ToLocalTime();
			return Res.GetString("B819E472-1CCA-4406-A034-6D07EE806DD2", "The {0} settings should be verified after {1} once the custom policy file changes have had a chance to take effect in the Microsoft Azure B2C tenant. The changes were pushed through at {2}.", env, localTime.AddMinutes(30), localTime);
		}

		void ResetAccount()
		{
			EdiTokenAuthOnBoardingData.TOD_VerificationUsername = string.Empty;
			EdiTokenAuthOnBoardingData.TOD_VerificationUserPassword = string.Empty;
		}

		async Task<string> ShowOidcVerificationDialog(OIDCConfig oidcConfig, string domainHint)
		{
			try
			{
				using (var oidcAuthenticationMessageBox = BuildOIDCAuthenticationMessageBox())
				{
					return await oidcAuthenticationMessageBox.VerifyOidcConfig(oidcConfig, domainHint);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}
		}

		void VerificationUsernameButton_Click(object sender, EventArgs e)
		{
			CopyToClipboard(EdiTokenAuthOnBoardingData.TOD_VerificationUsername);
		}

		void VerificationUserPasswordButton_Click(object sender, EventArgs e)
		{
			CopyToClipboard(EdiTokenAuthOnBoardingData.TOD_VerificationUserPassword);
		}

		void CopyToClipboard(string value)
		{
			if (!SafeClipboard.SetText(value))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}
	}
}
