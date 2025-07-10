using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Login;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.GUI.Testing
{
	[TestedType(typeof(EdiTokenAuthOnBoardingDataForm))]
	public class EdiTokenAuthOnBoardingDataFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var sys = Factory.New<EdiTokenAuthOnBoardingData>();
			return new EdiTokenAuthOnBoardingDataForm(sys);
		}

		public void TestTenantFindBoxEnable()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var tenantFindBox = form.FindSingle<ZGuidFindBox>("TenantFindBox");
				Assert(tenantFindBox.Enabled);
			}

			dataSource.TOD_Status = OnBoardingStatuses.Codes.Queued;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var tenantFindBox = form.FindSingle<ZGuidFindBox>("TenantFindBox");
				Assert(tenantFindBox.ReadOnly);
			}
		}

		public void TestCheckEnvironmentWhenVerifySettings()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var verifySettingsButton = form.FindSingle<ZButton>("VerifySettingsButton");
				verifySettingsButton.PerformClick();
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				AssertEquals("Please select an Environment.", unitTestUserNotification.LastMessage.Text);
				unitTestUserNotification.ClearMessagesAndAnswers();
			}

			var dataSource2 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource2))
			{
				dataSource2.Environment = "TST";
				form.Show();
				var verifySettingsButton = form.FindSingle<ZButton>("VerifySettingsButton");
				verifySettingsButton.PerformClick();
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				AssertEquals("Select a valid Environment.", unitTestUserNotification.LastMessage.Text);
			}
		}

		public void TestVerifyProductionB2CSettingsWithStagingEnvironment()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = OnBoardingStatuses.Codes.StagingPullRequest;
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				dataSource.Environment = AzureB2CEnvironmentCodeDescriptionList.Codes.PRD;
				form.Show();
				var verifySettingsButton = form.FindSingle<ZButton>("VerifySettingsButton");
				verifySettingsButton.PerformClick();
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				AssertEquals("Can not choose Production B2C, when the status is StagingPullRequest.", unitTestUserNotification.LastMessage.Text);
			}
		}

		public void TestUpdatePrLinkEnabled()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var prodPrLinkLabel = form.FindSingle<ZLinkLabel>("ProdPrLinkLabel");
				var stagingPrLinkLabel = form.FindSingle<ZLinkLabel>("StagingPrLinkLabel");
				AssertEquals("N/A", prodPrLinkLabel.Text);
				AssertEquals("N/A", stagingPrLinkLabel.Text);
			}

			dataSource.TOD_ProdPRLink = ProdPrLink;
			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var prodPrLinkLabel = form.FindSingle<ZLinkLabel>("ProdPrLinkLabel");
				var stagingPrLinkLabel = form.FindSingle<ZLinkLabel>("StagingPrLinkLabel");
				AssertEquals(ProdPrLink, prodPrLinkLabel.Text);
				AssertEquals(StagingPrLink, stagingPrLinkLabel.Text);
			}
		}

		public void TestUpdateTokenBasedAuthenticationCheckBoxEnabled()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var enableTokenBasedAuthenticationCheckBox = form.FindSingle<ZCheckBox>("EnableTokenBasedAuthenticationCheckBox");
				AssertEquals(false, enableTokenBasedAuthenticationCheckBox.Enabled);
			}

			dataSource.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var enableTokenBasedAuthenticationCheckBox = form.FindSingle<ZCheckBox>("EnableTokenBasedAuthenticationCheckBox");
				AssertEquals(true, enableTokenBasedAuthenticationCheckBox.Enabled);
			}
		}

		public void TestPrLinkClick()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_StagingPRLink = StagingPrLink;
			dataSource.TOD_ProdPRLink = ProdPrLink;
			Factory.Save();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var satgingPrLink = form.FindSingle<ZLinkLabel>("StagingPrLinkLabel");
				var prodPrLink = form.FindSingle<ZLinkLabel>("ProdPrLinkLabel");
				satgingPrLink.OnLinkClicked_Exposed(null);
				AssertEquals(StagingPrLink, WebUrlLauncher.LastUrlLaunched);
				prodPrLink.OnLinkClicked_Exposed(null);
				AssertEquals(ProdPrLink, WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestSetConfigurationIdentifierFromOidcServer()
		{
			using (var formToBash = GetFormToBash())
			{
				AssertType<EdiTokenAuthOnBoardingDataForm>(formToBash);
				var form = (EdiTokenAuthOnBoardingDataForm)formToBash;
				AssertType<EdiTokenAuthOnBoardingData>(form.BusinessEntity);
				var ediTokenAuthOnBoardingData = (EdiTokenAuthOnBoardingData)form.BusinessEntity;
				// 1. When opening a new form, we expect default value to be filled
				CombineAssertions(() =>
				{
					AssertEquals("AZU", ediTokenAuthOnBoardingData.TOD_OIDCServer);
					AssertEquals("Azure", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				});

				// 2. When updating to a new OIDC Server Type, we should reset the value
				// 2.1: Updating to GEN
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "GEN";
				AssertEquals(string.Empty, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				// 2.2: Updating back to AZU
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				AssertEquals("Azure", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				// 2.3: Updating to OKT
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "OKT";
				AssertEquals(string.Empty, ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);

				// 3. If a value is manually set, then we should not reset anymore
				ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Something";
				// 3.1: Updating to GEN
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "GEN";
				AssertEquals("Something", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				// 3.2: Updating back to AZU
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "AZU";
				AssertEquals("Something", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
				// 3.3: Updating to OKT
				ediTokenAuthOnBoardingData.TOD_OIDCServer = "OKT";
				AssertEquals("Something", ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier);
			}
		}

		public void TestSetLicenseEnterpriseFromIncident()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(ediTokenAuthOnBoardingData))
			{
				AssertSame(ediTokenAuthOnBoardingData, form.BusinessEntity);
				AssertType<EdiTokenAuthOnBoardingData>(form.BusinessEntity);
				AssertNull(ediTokenAuthOnBoardingData.Incident);
				AssertNull(ediTokenAuthOnBoardingData.LicenceEnterprise);

				var licenceEnterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
				var incident1 = Factory.NewWithValidTestData<SupportIncident>();
				licenceEnterprise1.LE_EnterpriseCode = "EDI";
				incident1.EnterprisePK = licenceEnterprise1.PK;
				ediTokenAuthOnBoardingData.TOD_IM = incident1.PK;
				CombineAssertions(() =>
				{
					AssertNotNull(ediTokenAuthOnBoardingData.Incident);
					AssertNotNull(ediTokenAuthOnBoardingData.LicenceEnterprise);
					AssertEquals(licenceEnterprise1, ediTokenAuthOnBoardingData.LicenceEnterprise);
					AssertEquals("EDI", ediTokenAuthOnBoardingData.LicenceEnterprise.LE_EnterpriseCode);
				});

				var licenceEnterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
				var incident2 = Factory.NewWithValidTestData<SupportIncident>();
				licenceEnterprise2.LE_EnterpriseCode = "CW1";
				incident2.EnterprisePK = licenceEnterprise2.PK;
				ediTokenAuthOnBoardingData.TOD_IM = incident2.PK;
				CombineAssertions(() =>
				{
					AssertNotNull(ediTokenAuthOnBoardingData.Incident);
					AssertNotNull(ediTokenAuthOnBoardingData.LicenceEnterprise);
					AssertNotEquals(incident1, ediTokenAuthOnBoardingData.Incident);
					AssertNotEquals(licenceEnterprise1, ediTokenAuthOnBoardingData.LicenceEnterprise);
					AssertEquals(licenceEnterprise2, ediTokenAuthOnBoardingData.LicenceEnterprise);
					AssertEquals("CW1", ediTokenAuthOnBoardingData.LicenceEnterprise.LE_EnterpriseCode);
				});
			}
		}

		public void TestSetStatusDoNotResetRetry()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			// We set the value to the last, to be sure that even at the first loop the status will be updated
			var allStatuses = (new OnBoardingStatuses()).GetAllCodes();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				foreach (var status in allStatuses)
				{
					dataSource.TOD_Retry = 5;
					dataSource.TOD_Status = status;
					AssertEquals("Retry should not be reset if the status does", 5, dataSource.TOD_Retry);
				}
			}
		}

		public void TestSetAndResetChangeEventHandlersOnLoad()
		{
			object ReadPrivateFieldOrProperty(object o, string name)
			{
				var fieldInfo = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					return fieldInfo.GetValue(o);
				}
				var propertyInfo = o.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);
				return propertyInfo?.GetValue(o);
			}
			Dictionary<string, EventHandler> GetChangeEventHandlerByField(BusinessObject businessObject)
			{
				var propertyInfoStorage = ReadPrivateFieldOrProperty(businessObject, "PropertyInfoStorage");
				var valueChangedDictionary = ReadPrivateFieldOrProperty(propertyInfoStorage, "ValueChangedDictionary");
				var dictionary = ReadPrivateFieldOrProperty(valueChangedDictionary, "dictionary") as Dictionary<string, Dictionary<BusinessObject, EventHandler>>;
				return dictionary
					?.Where(x => x.Value.ContainsKey(businessObject))
					?.ToDictionary(x => x.Key, x => x.Value[businessObject]);
			}

			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			var expectedFieldListener = new[]
			{
				nameof(dataSource.TOD_OIDCServer),
				nameof(dataSource.TOD_Status),
				nameof(dataSource.TOD_VerificationUsername),
				nameof(dataSource.TOD_VerificationUserPassword),
			};
			var newDataSource = Factory.New<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				AssertContainsExactElementsInAnyOrder(expectedFieldListener, GetChangeEventHandlerByField(dataSource).Keys);
				form.SetDataBinding(newDataSource, "");
				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), GetChangeEventHandlerByField(dataSource).Keys);
				AssertContainsExactElementsInAnyOrder(expectedFieldListener, GetChangeEventHandlerByField(newDataSource).Keys);
			}
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<KeyValuePair<string, Dictionary<BusinessObject, EventHandler>>>(), GetChangeEventHandlerByField(newDataSource));
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<KeyValuePair<string, Dictionary<BusinessObject, EventHandler>>>(), GetChangeEventHandlerByField(dataSource));
		}

		public void TestStatusIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var statusDropEdit = form.FindSingle<ZDropEdit>("StatusDropEdit");
				Assert(statusDropEdit.ReadOnly);
			}
		}

		public void TestVerificationResultIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var verificationResultTextBox = form.FindSingle<ZTextBox>("VerificationResultTextBox");
				Assert(verificationResultTextBox.ReadOnly);
			}
		}

		public void TestEnterpriseCodeIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var enterpriseCode = form.FindSingle<ZGuidFindBox>("LicenseEnterpriseFindBox");
				Assert(enterpriseCode.ReadOnly);
			}
		}

		public void TestTenantFindBoxName()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var tenantFindBox = form.FindSingle<ZGuidFindBox>("TenantFindBox");
				AssertEquals("Tenant", tenantFindBox.CaptionResourceString.Caption);
			}
		}

		public void TestCopyButtonDisabledWhenEmpty()
		{
			var dataSource = Factory.New<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				var verificationUsernameButton = form.FindSingle<ZButton>("VerificationUsernameButton");
				var verificationUserPasswordButton = form.FindSingle<ZButton>("VerificationUserPasswordButton");
				form.Show();
				AssertEquals(false, verificationUsernameButton.Enabled);
				AssertEquals(false, verificationUserPasswordButton.Enabled);
				dataSource.TOD_VerificationUsername = "admin";
				AssertEquals(true, verificationUsernameButton.Enabled);
				AssertEquals(false, verificationUserPasswordButton.Enabled);
				dataSource.TOD_VerificationUserPassword = "pass";
				AssertEquals(true, verificationUsernameButton.Enabled);
				AssertEquals(true, verificationUserPasswordButton.Enabled);
				dataSource.TOD_VerificationUsername = null;
				AssertEquals(false, verificationUsernameButton.Enabled);
				AssertEquals(true, verificationUserPasswordButton.Enabled);
				dataSource.TOD_VerificationUserPassword = null;
				AssertEquals(false, verificationUsernameButton.Enabled);
				AssertEquals(false, verificationUserPasswordButton.Enabled);
			}
		}

		public void TestPasswordCopyButtonDisabledWhenNoPermission()
		{
			var dataSource = Factory.New<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_VerificationUserPassword = "admin";
			EDISecurityCheckpoints.TokenAuthenticationOnBoardingCopyPassword.IsAllowed = false;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				var verificationUserPasswordButton = form.FindSingle<ZButton>("VerificationUserPasswordButton");
				form.Show();
				AssertEquals(false, verificationUserPasswordButton.Enabled);
			}

			EDISecurityCheckpoints.TokenAuthenticationOnBoardingCopyPassword.IsAllowed = true;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				var verificationUserPasswordButton = form.FindSingle<ZButton>("VerificationUserPasswordButton");
				form.Show();
				AssertEquals(true, verificationUserPasswordButton.Enabled);
			}
		}

		public void TestMenuItemsDisabledWhenNew()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", false);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", true);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", false);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuItemsWhenStatusProductionPullRequest()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = OnBoardingStatuses.Codes.ProductionPullRequest;
			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuItemsWhenStatusStagingPullRequest()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = OnBoardingStatuses.Codes.StagingPullRequest;
			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuItemsWhenStatusVerified()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = OnBoardingStatuses.Codes.Verified;
			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", true);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuItemsWhenStatusCustomerTestCompleted()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_StagingPRLink = StagingPrLink;
			dataSource.TOD_Status = OnBoardingStatuses.Codes.CustomerTestCompleted;
			dataSource.TOD_Enabled = true;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", true);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuItemsWhenStatusError()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = OnBoardingStatuses.Codes.Error;
			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", false);
				CheckMenuItem(form, "Process", true);
				Factory.Save();
				CheckMenuItem(form, "Staging Deployment", false);
				CheckMenuItem(form, "Customer Test Completed", false);
				CheckMenuItem(form, "Completed", false);
				CheckMenuItem(form, "Revert", true);
				CheckMenuItem(form, "Restart Processing", true);
				CheckMenuItem(form, "Process", true);
			}
		}

		public void TestMenuProcessDisabledWhenInReadOnlyMode()
		{
			TestMenuProcessEnabled(ODisplayMode.ReadOnly, expectedEnabled: false);
		}

		public void TestMenuProcessDisabledWhenInDeleteMode()
		{
			TestMenuProcessEnabled(ODisplayMode.Delete, expectedEnabled: false);
		}

		public void TestMenuProcessEnabledWhenInEditMode()
		{
			TestMenuProcessEnabled(ODisplayMode.Edit, expectedEnabled: true);
		}

		void TestMenuProcessEnabled(ODisplayMode displayMode, bool expectedEnabled)
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.DisplayMode = displayMode;
				form.Show();
				CheckMenuItem(form, "Process", expectedEnabled);
			}
		}

		public void TestMenuStartProcessingEnabledOnlyWhenSavedAndStatusIsNew()
		{
			TestMenuEnabledOnlyWhenSavedAndStatusMatches("Staging Deployment", OnBoardingStatuses.Codes.New);
		}

		public void TestMenuRevertEnabledOnlyWhenAnyPrLinkIsNotEmpty()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			// sanity check - test case need to be updated if next assertion is wrong to ensure we validate each combination
			AssertEquals(OnBoardingStatuses.Codes.New, dataSource.TOD_Status);
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var menuItem = form.FindMenuItem_ForTest("Revert");
				AssertNotNull("Revert", menuItem);
				AssertEquals(false, menuItem.Enabled);
			}

			dataSource.TOD_StagingPRLink = StagingPrLink;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var menuItem = form.FindMenuItem_ForTest("Revert");
				AssertNotNull("Revert", menuItem);
				AssertEquals(true, menuItem.Enabled);
			}
		}

		void TestMenuEnabledOnlyWhenSavedAndStatusMatches(string menuItemText, params string[] expectedStatus)
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			// sanity check - test case need to be updated if next assertion is wrong to ensure we validate each combination
			AssertEquals(OnBoardingStatuses.Codes.New, dataSource.TOD_Status);
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var menuItem = form.FindMenuItem_ForTest(menuItemText);
				AssertNotNull(menuItemText, menuItem);
				foreach (var status in new OnBoardingStatuses().GetAllCodes())
				{
					dataSource.TOD_Status = status;
					AssertEquals($"We expect {menuItemText} to be disabled when status is {status} and object has change", expected: false, menuItem.Enabled);
					Factory.Save();
					var expectedEnabled = expectedStatus.Contains(status);
					AssertEquals($"We expect {menuItemText} to be {(expectedEnabled ? "enabled" : "disabled")} when status is {status} and object has no change", expectedEnabled, menuItem.Enabled);
				}
			}
		}

		static MenuItem CheckMenuItem(EdiTokenAuthOnBoardingDataForm form, string menuItemText, bool expectedEnabled)
		{
			var menuItem = form.FindMenuItem_ForTest(menuItemText);
			AssertNotNull(menuItemText, menuItem);
			AssertEquals($"We expect {menuItemText} to be {(expectedEnabled ? "enabled" : "disabled")}", expectedEnabled, menuItem.Enabled);
			return menuItem;
		}

		public void TestMenuStagingDeploymentDoUpdateStatus()
		{
			TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.New,
				verificationResultDetails: "",
				menuItemText: "Staging Deployment",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.Queued,
				expectResetRetry: true);
		}

		public void TestMenuRevertDoUpdateStatus_SPR()
		{
			TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.StagingPullRequest,
				verificationResultDetails: null,
				menuItemText: "Revert",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.Revert,
				expectResetRetry: true);
		}

		public void TestMenuRevertDoUpdateStatus_PPR()
		{
			TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.ProductionPullRequest,
				verificationResultDetails: null,
				menuItemText: "Revert",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.Revert,
				expectResetRetry: true);
		}

		public void TestMenuCustomerTestCompletedDoUpdateStatus()
		{
			var datasource = TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.Verified,
				verificationResultDetails: "",
				menuItemText: "Customer Test Completed",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.CustomerTestCompleted,
				expectResetRetry: true);
			AssertEquals(datasource.TOD_Retry, 0);
			AssertEquals(SupportIncidentLookups.Status.Closed, datasource.Incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, datasource.Incident.IM_ResolutionCode);
			AssertEquals(@"You can now switch any CargoWise production environment to use token-based authentication. Please follow the steps outlined in the 'Integrating CargoWise with an Identity Provider' document. The specific information can be found in 'How to Integrate CargoWise with an IdP'.

Document link: https://wisetechacademy.com/search?quickstart=a19756f7-a786-4916-bf8c-c6fe3d5c5499", datasource.Incident.EConversation.JobConversationForTest.GetTimeOrderedMessages()[1].Body);
		}

		public void TestMenuCompletedDoUpdateStatus()
		{
			TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.CustomerTestCompleted,
				verificationResultDetails: "",
				menuItemText: "Completed",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.Completed,
				expectResetRetry: true);
		}

		public void TestMenuRestartProcessingDoUpdateStatus()
		{
			TestUpdateStatusOrRaiseNotification(status: OnBoardingStatuses.Codes.Error,
				verificationResultDetails: "",
				menuItemText: "Restart Processing",
				expectedTextNotification: Array.Empty<string>(),
				expectedStatus: OnBoardingStatuses.Codes.New,
				expectResetRetry: true);
		}

		public void TestClaimMappingControlEnableShouldBeTrueInAnyStatus()
		{
			AssertClaimMappingControls(OnBoardingStatuses.Codes.New);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.Queued);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.StagingPullRequest);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.StagingMergedAndVerified);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.ProductionPullRequest);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.Verified);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.CustomerTestCompleted);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.Completed);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.Error);
			AssertClaimMappingControls(OnBoardingStatuses.Codes.Revert);
		}

		void AssertClaimMappingControls(string status)
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = status;
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				Assert(form.ClaimMappingIdentifierDropEdit.Enabled);
				Assert(form.ClaimMappingNameTextBox.Enabled);
			}
		}

		EdiTokenAuthOnBoardingData TestUpdateStatusOrRaiseNotification(string status, string verificationResultDetails, string menuItemText, string[] expectedTextNotification, string expectedStatus, bool expectResetRetry)
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Retry = 42;
			dataSource.TOD_Status = status;
			dataSource.TOD_Enabled = status == OnBoardingStatuses.Codes.CustomerTestCompleted;
			if (menuItemText == "Revert")
			{
				dataSource.TOD_StagingPRLink = StagingPrLink;
			}
			if (verificationResultDetails != null)
			{
				dataSource.VerificationResultDetails = verificationResultDetails;
			}
			Factory.Save();
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				form.Show();
				var menuItem = CheckMenuItem(form, menuItemText, expectedEnabled: true);
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				unitTestUserNotification.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				var previousMessage = unitTestUserNotification.LastMessage;
				if (expectedTextNotification.Any())
				{
					AssertNotNull(previousMessage.Text);
					var lastTextNotification = previousMessage.Text.SplitByLine();
					CombineAssertions(() =>
					{
						AssertEquals("Azure B2C settings verification", previousMessage.Caption);
						Assert($"Unexpected type for message: {previousMessage}", previousMessage.WasError);
						AssertContainsExactElementsInExactOrder(expectedTextNotification, lastTextNotification);
					});
				}
				else
				{
					if (status == OnBoardingStatuses.Codes.ProductionPullRequest && expectedStatus == OnBoardingStatuses.Codes.Verified)
					{
						AssertEquals("A message will be sent to the customer via the CR9 incident when the form is saved", previousMessage.Text);
					}
					else if (status == OnBoardingStatuses.Codes.Verified && expectedStatus == OnBoardingStatuses.Codes.CustomerTestCompleted)
					{
						AssertEquals("A message will be sent to the customer via the CR9 incident when the form is saved", previousMessage.Text);
					}
					else
					{
						AssertNull(previousMessage.Text);
					}
				}

				AssertEquals(expectedStatus, dataSource.TOD_Status);
				AssertEquals(expectResetRetry ? 0 : 42, dataSource.TOD_Retry);
			}

			return dataSource;
		}

		public class EdiTokenAuthOnBoardingDataFormWithCustomOIDCAuthenticationMessageBox : EdiTokenAuthOnBoardingDataForm
		{
			public Mock<IDisposableOIDCAuthenticationMessageBox> OIDCAuthenticationMessageBoxMock { get; }

			public EdiTokenAuthOnBoardingDataFormWithCustomOIDCAuthenticationMessageBox(EdiTokenAuthOnBoardingData config) : base(config)
			{
				config.Environment = Code;
				OIDCAuthenticationMessageBoxMock = new Mock<IDisposableOIDCAuthenticationMessageBox>(MockBehavior.Strict);
				OIDCAuthenticationMessageBoxMock.Setup(m => m.Dispose());
			}

			public IDisposableOIDCAuthenticationMessageBox BaseBuildOIDCAuthenticationMessageBox() => base.BuildOIDCAuthenticationMessageBox();

			protected override IDisposableOIDCAuthenticationMessageBox BuildOIDCAuthenticationMessageBox() => OIDCAuthenticationMessageBoxMock.Object;
		}

		public void TestBuildOIDCAuthenticationMessageBox()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			using (var form = new EdiTokenAuthOnBoardingDataFormWithCustomOIDCAuthenticationMessageBox(dataSource))
			{
				using (var oidcAuthenticationMessageBox = form.BaseBuildOIDCAuthenticationMessageBox())
				{
					AssertType<OIDCAuthenticationMessageBox>(oidcAuthenticationMessageBox);
					AssertEquals(form, oidcAuthenticationMessageBox.ParentControl);
				}
				AssertEquals(false, form.IsDisposed);
			}
		}

		public void TestVerifySettinsButtonFailWhenDataSourceIsNotProperlySet()
		{
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			var ediIdentityTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			ediIdentityTenant.IDT_Name = "Test";
			dataSource.TOD_IDT = ediIdentityTenant.PK;
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			using (var form = new EdiTokenAuthOnBoardingDataFormWithCustomOIDCAuthenticationMessageBox(dataSource))
			{
				form.Show();
				AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified, dataSource.VerificationResult);
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				unitTestUserNotification.ClearMessagesAndAnswers();
				ClickVerifySettingsButton(form, dataSource, out var lastUncaughtException);
				CombineAssertions(() =>
				{
					AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed, dataSource.VerificationResult);
					AssertEquals("Cannot verify OIDC settings: please click the Save button and ensure all mandatory data is filled in.", dataSource.VerificationResultDetails);
					AssertEquals("Cannot verify OIDC settings: please click the Save button and ensure all mandatory data is filled in.", unitTestUserNotification.LastMessage.Text);
					AssertNull(lastUncaughtException);
				});
				form.OIDCAuthenticationMessageBoxMock.VerifyNoOtherCalls();
			}
		}

		public void TestVerifySettingsButtonSuccessfulWithNoWtgInternalAccount()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var dataSource = BuildEdiTokenAuthOnBoardingDataWithValidConfig();
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
				using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
				using (var form = new EdiTokenAuthOnBoardingDataFormForTest(dataSource))
				{
					form.Show();
					form.VerifySettingsButton_Click(dataSource).GetAwaiter().GetResult();
					AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success, dataSource.VerificationResult);
				}
			}
		}

		public void TestVerifySettingsButtonSetResultFailWhenLoginFail()
		{
			var verifyResult = "Failure for test";
			VerifySettingsButtonSetResult(verifyResult, exceptionToThrow: null, EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed, verifyResult, OIDCVerifyFailedMessage);
		}

		public void TestVerifySettingsButtonSetResultFailWhenLoginThrowNonCriticalException()
		{
			var message = "Exception for unit test";
			var exceptionToThrow = new Exception(message);
			// sanity check
			AssertEquals(false, exceptionToThrow.IsCriticalException());
			// launch the test
			VerifySettingsButtonSetResult(message, exceptionToThrow, EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed, message, OIDCVerifyFailedMessage);
		}

		public void TestVerifySettingsButtonSetResultFailWhenLoginThrowCriticalException()
		{
			var message = "Critical exception for unit test";
			var exceptionToThrow = new AppDomainUnloadedException(message);
			// sanity check
			AssertEquals(true, exceptionToThrow.IsCriticalException());
			// launch the test
			VerifySettingsButtonSetResult(message, exceptionToThrow, EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified, string.Empty);
		}

		public void TestVerifySettingsButtonSetResultSuccessWhenStatusIsPPR()
		{
			var expectedMessage = "A message will be sent to the customer via the CR9 incident when the form is saved";
			VerifySettingsButtonSetResult(string.Empty, exceptionToThrow: null, EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success, string.Empty, expectedMessage, originStatus: OnBoardingStatuses.Codes.ProductionPullRequest, ZDateTime.UtcNow.AddHours(-1));
		}

		public void TestShowMessageWhenItIsNotGoodTimeToVerifyStaging()
		{
			ShowMessageWhenItIsNotGoodTimeToVerify(OnBoardingStatuses.Codes.StagingPullRequest, AzureB2CEnvironmentCodeDescriptionList.Codes.STG, AzureB2CEnvironmentCodeDescriptionList.Descriptions.STG);
		}

		public void TestShowMessageWhenItIsNotGoodTimeToVerifyProduction()
		{
			ShowMessageWhenItIsNotGoodTimeToVerify(OnBoardingStatuses.Codes.ProductionPullRequest, AzureB2CEnvironmentCodeDescriptionList.Codes.PRD, AzureB2CEnvironmentCodeDescriptionList.Descriptions.PRD);
		}

		void ShowMessageWhenItIsNotGoodTimeToVerify(string status, string envCode, string envDesc)
		{
			var updatedTimeUtc = ZDateTime.UtcNow;
			var updatedTimeLocal = updatedTimeUtc.ToDateTime().ToLocalTime();
			var verifyTimeLocal = updatedTimeUtc.ToDateTime().ToLocalTime().AddMinutes(30);
			var dataSource = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			dataSource.TOD_Status = status;
			dataSource.TOD_SystemLastEditTimeUtc = updatedTimeUtc;
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			using (var form = new EdiTokenAuthOnBoardingDataForm(dataSource))
			{
				dataSource.Environment = envCode;
				form.Show();
				var verifySettingsButton = form.FindSingle<ZButton>("VerifySettingsButton");
				verifySettingsButton.PerformClick();

				AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified, dataSource.VerificationResult);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals($"The {envDesc} settings should be verified after {verifyTimeLocal} once the custom policy file changes have had a chance to take effect in the Microsoft Azure B2C tenant. The changes were pushed through at {updatedTimeLocal}.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestVerifySuccessWhenStatusIsSPRAndEnvironmentIsSTG()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var dataSource = BuildEdiTokenAuthOnBoardingDataWithValidConfig();
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
				using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
				using (var form = new EdiTokenAuthOnBoardingDataFormForTest(dataSource))
				{
					dataSource.Environment = "STG";
					dataSource.TOD_Status = OnBoardingStatuses.Codes.StagingPullRequest;
					form.Show();
					form.VerifySettingsButton_Click(dataSource).GetAwaiter().GetResult();
					AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success, dataSource.VerificationResult);
					var logs = dataSource.Logs.Find(a => a.SL_SE_NKEvent == Events.EditedARecordCode).ToArray();
					AssertEquals(1, logs.Length);
					AssertEquals("EDT", logs[0].SL_SE_NKEvent);
					AssertEquals($"change status from 'SPR' to '{dataSource.TOD_Status}'", logs[0].SL_Reference);
				}
			}
		}

		public void TestVerifySuccessWhenStatusIsPPRAndEnvironmentIsPRD()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var dataSource = BuildEdiTokenAuthOnBoardingDataWithValidConfig();
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
				using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
				using (var form = new EdiTokenAuthOnBoardingDataFormForTest(dataSource))
				{
					dataSource.Environment = "PRD";
					dataSource.TOD_Status = OnBoardingStatuses.Codes.ProductionPullRequest;
					form.Show();
					form.VerifySettingsButton_Click(dataSource).GetAwaiter().GetResult();
					AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success, dataSource.VerificationResult);
					var logs = dataSource.Logs.Find(a => a.SL_SE_NKEvent == Events.EditedARecordCode).ToArray();
					AssertEquals(1, logs.Length);
					AssertEquals("EDT", logs[0].SL_SE_NKEvent);
					AssertEquals($"change status from 'PPR' to '{dataSource.TOD_Status}'", logs[0].SL_Reference);
				}
			}
		}

		public void TestVerifySuccessWhenStatusIsPPRAndEnvironmentIsSTG()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var dataSource = BuildEdiTokenAuthOnBoardingDataWithValidConfig();
			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			{
				var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages);
				using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
				using (var form = new EdiTokenAuthOnBoardingDataFormForTest(dataSource))
				{
					dataSource.Environment = "STG";
					dataSource.TOD_Status = OnBoardingStatuses.Codes.ProductionPullRequest;
					form.Show();
					form.VerifySettingsButton_Click(dataSource).GetAwaiter().GetResult();
					AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success, dataSource.VerificationResult);
					var logs = dataSource.Logs.Find(a => a.SL_SE_NKEvent == Events.EditedARecordCode).ToArray();
					AssertEquals(0, logs.Length);
					AssertNotEquals(SupportIncidentLookups.Status.Closed, dataSource.Incident.IM_Status);
					AssertEquals(dataSource.Incident.EConversation.JobConversationForTest.GetTimeOrderedMessages().Count, 0);
				}
			}
		}

		void VerifySettingsButtonSetResult(string mockVerifyOidcConfigResult, Exception exceptionToThrow, string expectedResult, string expectedDetails, string expectMessage = null, string originStatus = null, ZDateTime lastEditTimeUtc = default)
		{
			var dataSource = BuildEdiTokenAuthOnBoardingDataWithValidConfig();
			if (!string.IsNullOrEmpty(originStatus))
			{
				dataSource.TOD_Status = originStatus;
			}
			dataSource.Factory.Save();

			if (lastEditTimeUtc != default)
			{
				dataSource.TOD_SystemLastEditTimeUtc = lastEditTimeUtc;
			}

			using (EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, InitializeCollection()))
			using (var form = new EdiTokenAuthOnBoardingDataFormWithCustomOIDCAuthenticationMessageBox(dataSource))
			{
				var mockSetup = form.OIDCAuthenticationMessageBoxMock
					.Setup(m => m.VerifyOidcConfig(It.IsAny<OIDCConfig>(), It.IsAny<string>()));
				if (exceptionToThrow is null)
				{
					mockSetup.Returns(Task.FromResult(mockVerifyOidcConfigResult));
				}
				else
				{
					mockSetup.Throws(exceptionToThrow);
				}

				form.Show();
				AssertEquals(EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified, dataSource.VerificationResult);
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				unitTestUserNotification.ClearMessagesAndAnswers();
				ClickVerifySettingsButton(form, dataSource, out var lastUncaughtException);
				CombineAssertions(() =>
				{
					AssertEquals(expectedResult, dataSource.VerificationResult);
					AssertEquals(expectedDetails, dataSource.VerificationResultDetails);
					AssertEquals(expectMessage, unitTestUserNotification.LastMessage.Text);
					if (dataSource.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.Failed)
					{
						var logs = dataSource.Logs.Find(a => a.SL_SE_NKEvent == Events.ErrorReportCode).ToArray();
						AssertEquals(1, logs.Length);
						AssertEquals("ERR", logs[0].SL_SE_NKEvent);
						AssertEquals($"Failed to verify settings, check provided data and logs.|Environment=PRD|VerificationResultDetails={expectedDetails}", logs[0].SL_Reference);
					}
					if (dataSource.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.Success && (dataSource.TOD_Status == OnBoardingStatuses.Codes.StagingMergedAndVerified || dataSource.TOD_Status == OnBoardingStatuses.Codes.Verified))
					{
						var logs = dataSource.Logs.Find(a => a.SL_SE_NKEvent == Events.EditedARecordCode).ToArray();
						AssertEquals(1, logs.Length);
						AssertEquals("EDT", logs[0].SL_SE_NKEvent);
						AssertEquals($"change status from '{originStatus}' to '{dataSource.TOD_Status}'", logs[0].SL_Reference);
						AssertEquals(dataSource.TOD_Retry, 0);
						if (dataSource.TOD_Status == OnBoardingStatuses.Codes.Verified)
						{
							Assert(string.IsNullOrEmpty(dataSource.TOD_VerificationUsername));
							Assert(string.IsNullOrEmpty(dataSource.TOD_VerificationUserPassword));
							AssertEquals(SupportIncidentLookups.Status.Closed, dataSource.Incident.IM_Status);
							AssertEquals(@"You can now switch any CargoWise non-production environment to use token-based authentication. Please follow the steps outlined in the 'Integrating CargoWise with an Identity Provider' document. The specific information can be found in 'How to Integrate CargoWise with an IdP'.

Document link: https://wisetechacademy.com/search?quickstart=a19756f7-a786-4916-bf8c-c6fe3d5c5499", dataSource.Incident.EConversation.JobConversationForTest.GetTimeOrderedMessages()[1].Body);
						}
					}
					if (exceptionToThrow?.IsCriticalException() is true)
					{
						AssertType<DeveloperNotificationException>(lastUncaughtException);
						AssertEquals(exceptionToThrow, ((DeveloperNotificationException)lastUncaughtException).InnerException);
					}
					else
					{
						AssertNull("Critical exception should not be caught", lastUncaughtException);
					}
				});
				form.OIDCAuthenticationMessageBoxMock
					.Verify(
						m => m.VerifyOidcConfig(
							It.Is<OIDCConfig>(c => c.AuthorityURL == AuthorityUrl1),
							dataSource.TOD_ConfigurationIdentifier),
						Times.Once());
				form.OIDCAuthenticationMessageBoxMock.Verify(m => m.Dispose(), Times.Once());
				form.OIDCAuthenticationMessageBoxMock.VerifyNoOtherCalls();
			}
		}

		static void ClickVerifySettingsButton(EdiTokenAuthOnBoardingDataForm form, EdiTokenAuthOnBoardingData ediTokenAuthOnBoardingData, out Exception lastUncaughtException)
		{
			lastUncaughtException = null;
			var verifyButton = form.FindSingle<ZButton>("VerifySettingsButton");
			AssertEquals(true, verifyButton.Enabled);
			verifyButton.PerformClick();
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
			{
				while (
					(ediTokenAuthOnBoardingData.VerificationResult == EdiTokenAuthOnBoardingDataLookups.VerificationResult.NotVerified) &&
					(lastUncaughtException is null))
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
					Application.DoEvents();
					lastUncaughtException = ExceptionReporterTestListener.Instance.LastOrDefault();
				}
			}
			if (lastUncaughtException != null)
			{
				AssertContainsExactElementsInExactOrder(new[] { lastUncaughtException }, ExceptionReporterTestListener.Instance);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		EdiTokenAuthOnBoardingData BuildEdiTokenAuthOnBoardingDataWithValidConfig()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var licenceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			incident.EnterprisePK = licenceEnterprise.PK;
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_Onboarding = true;
			tenant.IDT_AuthorityUrl = AuthorityUrl1;
			tenant.IDT_OidcClientId = ClientId;
			tenant.IDT_TenantId = Guid.NewGuid().ToString();
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			ediTokenAuthOnBoardingData.TOD_IM = incident.PK;
			ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifier = OIDCClaimMappingIdentifiers.Codes.LoginName;
			ediTokenAuthOnBoardingData.TOD_ClaimMappingName = "user_name";
			ediTokenAuthOnBoardingData.TOD_ConfigurationIdentifier = "Azure";
			ediTokenAuthOnBoardingData.TOD_OIDCServer = OIDCServerTypesList.Codes.Azure;
			ediTokenAuthOnBoardingData.TOD_SystemUniqueIdentifier = Guid.NewGuid().ToString();
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			return ediTokenAuthOnBoardingData;
		}

		[DeveloperOnlyTest]
		public void TestCopyVerificationUsername()
		{
			TestCopy("VerificationUsernameButton", (ediTokenAuthOnBoardingData) => ediTokenAuthOnBoardingData.TOD_VerificationUsername);
		}

		[DeveloperOnlyTest]
		public void TestCopyVerificationUserPassword()
		{
			TestCopy("VerificationUserPasswordButton", (ediTokenAuthOnBoardingData) => ediTokenAuthOnBoardingData.TOD_VerificationUserPassword);
		}

		void TestCopy(string copyButtonName, Func<EdiTokenAuthOnBoardingData, ZString> getterFunc)
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_VerificationUsername = Guid.NewGuid().ToString();
			ediTokenAuthOnBoardingData.TOD_VerificationUserPassword = Guid.NewGuid().ToString().Replace("-", "");
			using (var form = new EdiTokenAuthOnBoardingDataForm(ediTokenAuthOnBoardingData))
			{
				form.Show();
				var copyButton = form.FindSingle<ZButton>(copyButtonName);
				AssertEquals(true, copyButton.Enabled);
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				unitTestUserNotification.ClearMessagesAndAnswers();
				SafeClipboard.Clear();
				copyButton.PerformClick();
				var clipboard = SafeClipboard.GetText();
				AssertNotNullOrEmpty(clipboard);
				AssertEquals(getterFunc(ediTokenAuthOnBoardingData), clipboard);
				AssertNull(unitTestUserNotification.LastMessage.Text);
			}
		}

		AzureOpenIDConnectConfigurationCollection InitializeCollection()
		{
			var collection = new AzureOpenIDConnectConfigurationCollection();
			var azureApplicationManagement1 = collection.AddNew();
			azureApplicationManagement1.Code = "PRD";
			azureApplicationManagement1.AuthorityUrl = AuthorityUrl1;
			azureApplicationManagement1.ClientID = ClientId;
			var azureApplicationManagement2 = collection.AddNew();
			azureApplicationManagement2.Code = "STG";
			azureApplicationManagement2.AuthorityUrl = AuthorityUrl2;
			azureApplicationManagement2.ClientID = ClientId;
			return collection;
		}

		static Mock<IOIDCLoginServer> SetupMockOidcLoginServer(List<OIDCLoginRequestMessage> loginRequestMessages)
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>(MockBehavior.Strict);
			mockOIDCLoginServer.SetupGet(m => m.IsSupported).Returns(true);
			mockOIDCLoginServer
				.Setup(m => m.LoginLocal(Capture.In(loginRequestMessages), It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(), It.IsAny<OIDCLoginFactory>()))
				.Returns((OIDCLoginRequestMessage loginRequest, OIDCWebLauncher webLauncher, CancellationToken cancellationToken,
						OIDCLoginFactory oidcLoginFactory) =>
					BuilSuccessfuResponseMessage(loginRequest, webLauncher, oidcLoginFactory, cancellationToken));
			mockOIDCLoginServer
				.Setup(m => m.ValidateIdentityToken(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCLoginResponseMessage>(),
					It.IsAny<CancellationToken>()))
				.Returns((OIDCLoginRequestMessage loginRequest, OIDCLoginResponseMessage loginReponse, CancellationToken cancellationToken) =>
					BuildTokenData());
			return mockOIDCLoginServer;
		}

		static JwtSecurityToken BuildTokenData()
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[]
				{
					new Claim("user_name", "Bob", ClaimValueTypes.String, "Bob"),
				}),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("54561bb7-840f-4ee1-bc9c-bd5f0d77fbbe")), SecurityAlgorithms.HmacSha256Signature),
			};
			return tokenHandler.ReadJwtToken(tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor)));
		}

		static OIDCLoginResponseMessage BuilSuccessfuResponseMessage(OIDCLoginRequestMessage request, OIDCWebLauncher webLauncher, OIDCLoginFactory oidcLoginFactory, CancellationToken cancellationToken)
		{
			return OIDCLoginResponseMessage.CreateSuccessResponse(string.Empty, string.Empty, DateTimeOffset.MaxValue);
		}

		public class EdiTokenAuthOnBoardingDataFormForTest : EdiTokenAuthOnBoardingDataForm
		{
			public EdiTokenAuthOnBoardingDataFormForTest(EdiTokenAuthOnBoardingData config) : base(config)
			{
				config.Environment = Code;
				OIDCAuthenticationMessageBox = new OIDCAuthenticationMessageBoxForTest(this);
			}

			protected override IDisposableOIDCAuthenticationMessageBox BuildOIDCAuthenticationMessageBox() => OIDCAuthenticationMessageBox;

			public OIDCAuthenticationMessageBoxForTest OIDCAuthenticationMessageBox { get; }
		}

		public class OIDCAuthenticationMessageBoxForTest : OIDCAuthenticationMessageBox
		{
			public OIDCAuthenticationMessageBoxForTest(ContainerControl parentControl) : base(parentControl) { }
			protected override Task<T> PerformAction<T>(Func<CancellationToken, T> innFunc) => Task.FromResult(innFunc(CancellationTokenSource.Token));
		}

		const string Code = "PRD";

		const string AuthorityUrl1 = "https://www.example.com";

		const string AuthorityUrl2 = "https://www.example2.com";

		const string ClientId = "46546646-e627-46fb-afe4-e5ea9928740e";

		const string ProdPrLink = "http://www.test01.com";

		const string StagingPrLink = "http://www.test02.com";

		const string OIDCVerifyFailedMessage = "Failed to verify settings, check provided data and logs.";
	}
}
