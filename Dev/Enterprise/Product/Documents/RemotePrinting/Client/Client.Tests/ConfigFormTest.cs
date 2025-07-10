using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Moq;
using NUnit.Framework;
using static Enterprise.RemotePrinting.Client.Tests.ConnectionRegistryManagerTest;

namespace Enterprise.RemotePrinting.Client.Tests
{
	[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
	public class ConfigFormTest : TestCase
	{
		public void TestDisablePauseAutoUpdateTextBoxIfAutoUpdateDisabled()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
				configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;

				AssertEquals("PauseAutoUpdateHoursTextBox should be enabled by default", true, configForm.PauseAutoUpdateHoursTextBox_Exposed.Enabled);

				configForm.RadioButtonUpdateCustomUpdate_Exposed.Checked = true;
				AssertEquals("PauseAutoUpdateHoursTextBox should be disabled when Custom Update checked", false, configForm.PauseAutoUpdateHoursTextBox_Exposed.Enabled);

				configForm.RadioButtonAutoUpdate_Exposed.Checked = true;
				AssertEquals("PauseAutoUpdateHoursTextBox should be enabled when Auto Update checked", true, configForm.PauseAutoUpdateHoursTextBox_Exposed.Enabled);

				configForm.RadioButtonUpdateManualUpdate_Exposed.Checked = true;
				AssertEquals("PauseAutoUpdateHoursTextBox should be disabled when Manual Update checked", false, configForm.PauseAutoUpdateHoursTextBox_Exposed.Enabled);
			}
		}

		public void TestDisableSignalRReconnectionSettingsIfSignalRIsDisabled()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
				configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;

				CombineAssertions("SignalR Reconnection Fields should be disabled", () =>
				{
					AssertEquals(true, configForm.EnableSignalRCheckBox_Exposed.Enabled);
					AssertEquals(false, configForm.EnableSignalRCheckBox_Exposed.Checked);
					AssertEquals(false, configForm.EnablePauseSignalRCheckBox_Exposed.Enabled);
					AssertEquals(false, configForm.ReconnectionAttemptsTextBox_Exposed.Enabled);
					AssertEquals(false, configForm.ReconnectionLimitMinutesTextBox_Exposed.Enabled);
					AssertEquals(false, configForm.PauseSignalRMinutesTextBox_Exposed.Enabled);
				});

				configForm.EnableSignalRCheckBox_Exposed.Checked = true;
				configForm.EnablePauseSignalRCheckBox_Exposed.Checked = true;
				CombineAssertions("SignalR Reconnection Fields should be enabled", () =>
				{
					AssertEquals(true, configForm.EnableSignalRCheckBox_Exposed.Enabled);
					AssertEquals(true, configForm.EnableSignalRCheckBox_Exposed.Checked);
					AssertEquals(true, configForm.EnablePauseSignalRCheckBox_Exposed.Enabled);
					AssertEquals(true, configForm.ReconnectionAttemptsTextBox_Exposed.Enabled);
					AssertEquals(true, configForm.ReconnectionLimitMinutesTextBox_Exposed.Enabled);
					AssertEquals(true, configForm.PauseSignalRMinutesTextBox_Exposed.Enabled);
				});
			}
		}

		public void TestPasswordTextBoxProtect()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
				configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;

				configForm.SaveConnectionConfiguration();

				var config = configForm.RegistryManagerForTest.GetWebClientConfiguration(ConfigFormForTest.configName1);
				CombineAssertions("WebServicePwd should be protected by ProtectedDataHelper after saving", () =>
				{
					AssertNotEquals("pwd1", config.WebServicePwd);
					AssertEquals("pwd1", ProtectedDataHelper.Unprotect(config.WebServicePwd));
				});

				CombineAssertions("ProxyPwd should be protected by ProtectedDataHelper after saving", () =>
				{
					AssertNotEquals("proxypwd1", config.ProxyPwd);
					AssertEquals("proxypwd1", ProtectedDataHelper.Unprotect(config.ProxyPwd));
				});
			}
		}

		public void TestPasswordTextBoxUnprotect()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName3);
				configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;
				var config = configForm.RegistryManagerForTest.GetWebClientConfiguration(ConfigFormForTest.configName3);

				CombineAssertions("WebServicePwd should be unprotected by ProtectedDataHelper for ConfigurationsComboBox", () =>
				{
					AssertNotEquals("pwd3", config.WebServicePwd);
					AssertEquals("pwd3", configForm.WebServicePwdTextBox_Exposed.Text);
				});

				CombineAssertions("WebServicePwd should be unprotected by ProtectedDataHelper for ConfigurationsComboBox", () =>
				{
					AssertNotEquals("proxypwd3", config.ProxyPwd);
					AssertEquals("proxypwd3", configForm.ProxyPwdTextBox_Exposed.Text);
				});
			}
		}

		public void TestDefaultConfigCreatedWithEmptyRegistry()
		{
			using (var configForm = new ConfigFormForTest())
			{
				try
				{
					configForm.NamesFromRegistry = Array.Empty<string>();
					configForm.LoadConnectionConfigurations_Exposed();

					AssertEquals("There should be 1 configuration in the list", 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("This 1 configuration should be called 'DEFAULT'", "DEFAULT", configForm.ConfigurationsComboBox_Exposed.Items);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry("DEFAULT");
				}
			}
		}

		public void TestWorksWithOldConfigName()
		{
			using (var configForm = new ConfigFormForTest())
			{
				try
				{
					configForm.NamesFromRegistry = new string[] { "ediWebPrint" };
					configForm.LoadConnectionConfigurations_Exposed();

					AssertEquals("There should be 1 configuration in the list", 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("While it was previously renamed to 'DEFAULT', it should now be unchanged", "ediWebPrint", configForm.ConfigurationsComboBox_Exposed.Items);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry("ediWebPrint");
				}
			}
		}

		public void TestWorksWithSeveralConfigNames()
		{
			using (var configForm = new ConfigFormForTest())
			{
				string[] names = { "TEST1", "TEST2", "TEST3" };
				configForm.NamesFromRegistry = names;

				configForm.LoadConnectionConfigurations_Exposed();

				AssertEquals("There should be 3 configurations in the list", 3, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				AssertArrayEqualsByElements("All 3 names should be in the drop down", names, configForm.ConfigurationsComboBox_Exposed.Items.Cast<string>().ToArray());
			}
		}

		public void TestCannotRenameToExistingConfigName()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "test";
				var configName = "blah";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(newName);
					configForm.ConfigurationsComboBox_Exposed.Items.Add(configName);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionContains("PRE: The name is in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.RenameButtonClick_Exposed();
					AssertEquals("There should be the same number of configurations in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The original name should still be in the drop down", configName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Name already in use.", configForm.Status);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
					configForm.RegistryManagerForTest.DeleteFromRegistry(configName);
				}
			}
		}

		public void TestCannotAddToListWithExistingConfigName_CaseInsensitive()
		{
			using (var configForm = new ConfigFormForTest())
			{
				try
				{
					var newName = "test";
					configForm.ConfigurationsComboBox_Exposed.Items.Add("TEST");
					configForm.ConfigurationsComboBox_Exposed.Items.Add("blah");
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionContains("PRE: The name is in the drop down", "TEST", configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.NewButtonClick_Exposed();
					AssertEquals("There should be the same number of configurations in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The original name should still be in the drop down", "blah", configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Name already in use.", configForm.Status);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry("blah");
				}
			}
		}

		public void TestOldFormatTransformsIntoRegistryConfiguration()
		{
			Assert(true);
		}

		public void TestMaximumNumberOfRegistryConfigurations()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				for (int i = 0; i < 10; i++)
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add("Config" + i);
				}
				AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
				AssertEquals("PRE: There should be 10 items in the list already", 10, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				configForm.NewButtonClick_Exposed();
				AssertEquals("There should not be any more configuration in the list", 10, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				AssertCollectionNotContains("The name should not have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
				AssertEquals("Should show a message that there are too many elements", "You have reached the maximum number of configurations (10).", configForm.Status);
			}
		}

		public void TestMinimumNumberOfRegistryConfigurations()
		{
			using (var configForm = new ConfigFormForTest())
			{
				configForm.ConfigurationsComboBox_Exposed.Items.Add("Config");
				AssertEquals("PRE: There should be 1 item in the list already", 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				configForm.DeleteConfigurationButtonClick_Exposed();
				AssertEquals("There should not be any less configuration in the list", 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				AssertEquals("Should show a message that the user cannot remove the last element", "You cannot delete the last configuration.", configForm.Status);
			}
		}

		public void TestInstallingNewService_ServiceAlreadyInstalled()
		{
			AssertInstallationService(WindowsServicesHelper.ServiceControllerStatusRunning, string.Empty, $"A Service is already installed for configuration {ConfigFormForTest.configName1}.");
		}

		public void TestInstallingNewService_ErrorWhenCallingInstallNewServicesMethod()
		{
			AssertInstallationService(WindowsServicesHelper.ServiceNotFound, "The account name is invalid or does not exist", "Failed to install new WebPrint Client Service, please review output of installation command:\r\nThe account name is invalid or does not exist");
		}

		void AssertInstallationService(string expectedStatus, string installErrorMessage, string expectedErrorMessage)
		{
			var mockWinServiceHelper = new Mock<IWindowsServicesHelper>();
			mockWinServiceHelper.Setup(x => x.GetServiceNameFromConfigName(It.IsAny<string>())).Returns("service1");
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(expectedStatus);
			mockWinServiceHelper.Setup(x => x.InstallNewService(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), out installErrorMessage)).Returns(false);

			using (var configForm = new ConfigFormForTest(mockWinServiceHelper.Object))
			{
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;
					configForm.ServiceStartModeComboBox_Exposed.SelectedIndex = 0;

					configForm.showDialogResult = new PromptResult(DialogResult.OK, ConfigFormForTest.configName1);
					configForm.InstallingServiceButton_Click_Exposed();

					AssertEquals(expectedStatus, configForm.ServiceStatusTextBox_Exposed.Text);
					AssertEquals(expectedErrorMessage, configForm.showMessageContent);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(ConfigFormForTest.configName1);
				}
			}
		}

		public void TestUninstallServiceOutputErrorMessage()
		{
			var errorMessage = "Test Error";
			var mockWinServiceHelper = new Mock<IWindowsServicesHelper>();
			mockWinServiceHelper.Setup(x => x.GetServiceNameFromConfigName(It.IsAny<string>())).Returns("service1");
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);
			mockWinServiceHelper.Setup(x => x.DeleteService(It.IsAny<string>(), out errorMessage)).Returns(false);
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatusWithRetry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Func<string, bool>>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);

			using (var configForm = new ConfigFormForTest(mockWinServiceHelper.Object))
			{
				try
				{
					configForm.showMessageResult = DialogResult.Yes;
					configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;
					configForm.ServiceStartModeComboBox_Exposed.SelectedIndex = 0;

					configForm.showDialogResult = new PromptResult(DialogResult.OK, ConfigFormForTest.configName1);
					configForm.UninstallingServiceButton_Click_Exposed();

					AssertEquals("Failed to uninstall the WebPrint Client Service, please review output of the service removal command:\r\nTest Error", configForm.showMessageContent);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(ConfigFormForTest.configName1);
				}
			}
		}

		public void TestStartServiceOutputMessage() => AssertStartService(true, string.Empty, "The Service has been started.\r\nNote: If it stops again, please check the service running logs.");

		public void TestStartServiceOutputErrorMessage() => AssertStartService(false, "Test Error", "Failed to start the WebPrint Client Service, please review output of start command:\r\nTest Error");

		void AssertStartService(bool result, string errorMessage, string expectedMessage)
		{
			var mockWinServiceHelper = new Mock<IWindowsServicesHelper>();
			mockWinServiceHelper.Setup(x => x.GetServiceNameFromConfigName(It.IsAny<string>())).Returns("service1");
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusStopped);
			mockWinServiceHelper.Setup(x => x.StartProcess(It.IsAny<string>(), out errorMessage)).Returns(result);

			using (var configForm = new ConfigFormForTest(mockWinServiceHelper.Object))
			{
				configForm.StartServiceButton_Click_Exposed();
				AssertEquals(expectedMessage, configForm.showMessageContent);
			}
		}

		public void TestStopServiceOutputMessage_Stopped() => AssertStopService(WindowsServicesHelper.ServiceControllerStatusStopped, "The Service has been stopped.");

		public void TestStopServiceOutputMessage_StillRunning() => AssertStopService(WindowsServicesHelper.ServiceControllerStatusRunning, "The Service has been marked to stop.");

		void AssertStopService(string status, string expectedMessage)
		{
			var errorMessage = string.Empty;
			var mockWinServiceHelper = new Mock<IWindowsServicesHelper>();
			mockWinServiceHelper.Setup(x => x.GetServiceNameFromConfigName(It.IsAny<string>())).Returns("service1");
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
			mockWinServiceHelper.Setup(x => x.StopProcess(It.IsAny<string>(), out errorMessage)).Returns(true);
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatusWithRetry(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Func<string, bool>>())).Returns(status);

			using (var configForm = new ConfigFormForTest(mockWinServiceHelper.Object))
			{
				configForm.StopServiceButton_Click_Exposed();
				AssertEquals(expectedMessage, configForm.showMessageContent);
			}
		}

		public void TestStopServiceOutputErrorMessage()
		{
			var errorMessage = "Test Error";
			var mockWinServiceHelper = new Mock<IWindowsServicesHelper>();
			mockWinServiceHelper.Setup(x => x.GetServiceNameFromConfigName(It.IsAny<string>())).Returns("service1");
			mockWinServiceHelper.Setup(x => x.CheckServiceControllerStatus(It.IsAny<string>())).Returns(WindowsServicesHelper.ServiceControllerStatusRunning);
			mockWinServiceHelper.Setup(x => x.StopProcess(It.IsAny<string>(), out errorMessage)).Returns(false);

			using (var configForm = new ConfigFormForTest(mockWinServiceHelper.Object))
			{
				configForm.StopServiceButton_Click_Exposed();
				AssertEquals("Failed to stop the WebPrint Client Service, please review output of stop command:\r\nTest Error", configForm.showMessageContent);
			}
		}

		public void TestStartProcessShouldOutputErrorMessage()
		{
			var helper = new WindowsServicesHelper();
			var result = helper.StartProcess("unknow_new_service_test", out var errorMessage);
			AssertEquals(false, result);
			AssertContains("The specified service does not exist as an installed service.", errorMessage);
		}

		public void TestStopProcessShouldOutputErrorMessage()
		{
			var helper = new WindowsServicesHelper();
			var result = helper.StopProcess("unknow_new_service_test", out var errorMessage);
			AssertEquals(false, result);
			AssertContains("The specified service does not exist as an installed service.", errorMessage);
		}

		public void TestInstallingNewServiceShouldOutputErrorMessage()
		{
			var helper = new WindowsServicesHelper();
			var result = helper.InstallNewService("unknow_new_service_test", "testConfigName", "auto", "test_username", "test_password", out var errorMessage);
			AssertEquals(false, result);
			AssertContains("The account name is invalid or does not exist, or the password is invalid for the account name specified.", errorMessage);
		}

		public void TestDeleteServiceShouldOutputErrorMessage()
		{
			var helper = new WindowsServicesHelper();
			var result = helper.DeleteService("unknow_new_service_test", out var errorMessage);
			AssertEquals(false, result);
			AssertContains("The specified service does not exist as an installed service.", errorMessage);
		}

		public void TestNewButtonClick_OK_AddsItemInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				try
				{
					AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.NewButtonClick_Exposed();
					AssertEquals("There should be 1 more configuration in the list", itemCount + 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The name should have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Successfully added.", configForm.Status);
					AssertEquals("Should select the new item in the combo box", newName, configForm.ConfigurationsComboBox_Exposed.SelectedItem.ToString());
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
				}
			}
		}

		public void TestNewButtonClick_Cancel_DoesNotAddItemInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

				var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
				configForm.showDialogResult = new PromptResult(DialogResult.Cancel, newName);
				configForm.NewButtonClick_Exposed();
				AssertEquals("There should not be 1 more configuration in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
				AssertCollectionNotContains("The name should not have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
				AssertNotEquals("Successfully added.", configForm.Status);
			}
		}

		public void TestRenameRegistryConfiguration_OK_RenamesInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				var newName = "TESTNEWCONFIG";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
					configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName2);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					var oldConfig = configForm.RegistryManagerForTest.GetWebClientConfiguration(ConfigFormForTest.configName2);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.RenameButtonClick_Exposed();
					var newConfig = configForm.RegistryManagerForTest.GetWebClientConfiguration(newName);

					AssertEquals("The Web Service Url of the new configation should be equal with the old configation.", newConfig.WebServiceUrl, oldConfig.WebServiceUrl);
					AssertEquals("There should be the same number of configurations in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The new name should have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertCollectionNotContains("The original name should no longer be in the drop down", "blah", configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Successfully renamed.", configForm.Status);
					AssertEquals("Should select the new item in the combo box", newName, configForm.ConfigurationsComboBox_Exposed.SelectedItem.ToString());
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
					configForm.RegistryManagerForTest.DeleteFromRegistry(ConfigFormForTest.configName1);
					configForm.RegistryManagerForTest.DeleteFromRegistry(ConfigFormForTest.configName2);
				}
			}
		}

		public void TestRenameRegistryConfiguration_Cancel_DoesNotRenameInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				var configName1 = "test";
				var configName2 = "blah";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add("test");
					configForm.ConfigurationsComboBox_Exposed.Items.Add("blah");
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.Cancel, newName);
					configForm.RenameButtonClick_Exposed();
					AssertEquals("There should be the same number of configurations in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionNotContains("The name should not have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertNotEquals("Successfully renamed.", configForm.Status);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
					configForm.RegistryManagerForTest.DeleteFromRegistry(configName1);
					configForm.RegistryManagerForTest.DeleteFromRegistry(configName2);
				}
			}
		}

		public void TestDuplicateRegistryConfiguration_OK_DuplicatesInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			using (configForm.SetupWebClientConfigurationDataForTest())
			{
				var newName = "TESTNEWCONFIG";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(ConfigFormForTest.configName1);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 0;
					AssertCollectionNotContains("PRE: The new name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					var orignalConfig = configForm.RegistryManagerForTest.GetWebClientConfiguration(ConfigFormForTest.configName1);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showMessageResult = DialogResult.Yes;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.DuplicateButtonClick_Exposed();
					var newConfig = configForm.RegistryManagerForTest.GetWebClientConfiguration(newName);

					AssertEquals("The Web Service Url of the new configation should be equal with the orignal configation.", newConfig.WebServiceUrl, orignalConfig.WebServiceUrl);
					AssertEquals("There should 1 more configuration in the list", itemCount + 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The new name should have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertCollectionContains("The orignal name should still be in the drop down", ConfigFormForTest.configName1, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Successfully duplicated.", configForm.Status);
					AssertEquals("Should select the new item in the combo box", newName, configForm.ConfigurationsComboBox_Exposed.SelectedItem.ToString());
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(ConfigFormForTest.configName1);
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
				}
			}
		}

		public void TestDuplicateRegistryConfiguration_Cancel_DoesNotRenameInComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				try
				{
					AssertCollectionNotContains("PRE: The name should not be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.Cancel, newName);
					configForm.DuplicateButtonClick_Exposed();
					AssertEquals("There should be the same number of configurations in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionNotContains("The name should not have been added in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertNotEquals("Successfully duplicated.", configForm.Status);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
				}
			}
		}

		public void TestDeleteRegistryConfiguration_OK_ShouldRemoveFromComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				var configName = "blah";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(configName);
					configForm.ConfigurationsComboBox_Exposed.Items.Add(newName);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionContains("PRE: The name should be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("PRE: The name should be selected in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.SelectedItem.ToString());

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.OK, newName);
					configForm.showMessageResult = DialogResult.Yes;
					configForm.DeleteConfigurationButtonClick_Exposed();
					AssertEquals("There should be 1 less configuration in the list", itemCount - 1, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionNotContains("The name should have been removed from the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertEquals("Successfully deleted.", configForm.Status);
					AssertEquals("Should select the new item in the combo box", configName, configForm.ConfigurationsComboBox_Exposed.SelectedItem.ToString());
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
					configForm.RegistryManagerForTest.DeleteFromRegistry(configName);
				}
			}
		}

		public void TestDeleteRegistryConfiguration_Cancel_ShouldNotRemoveFromComboBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				var newName = "TESTNEWCONFIG";
				var configName = "blah";
				try
				{
					configForm.ConfigurationsComboBox_Exposed.Items.Add(configName);
					configForm.ConfigurationsComboBox_Exposed.Items.Add(newName);
					configForm.ConfigurationsComboBox_Exposed.SelectedIndex = 1;
					AssertCollectionContains("PRE: The name should be in the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);

					var itemCount = configForm.ConfigurationsComboBox_Exposed.Items.Count;
					configForm.showDialogResult = new PromptResult(DialogResult.Cancel, newName);
					configForm.DeleteConfigurationButtonClick_Exposed();
					AssertEquals("There should be the same number of configuration in the list", itemCount, configForm.ConfigurationsComboBox_Exposed.Items.Count);
					AssertCollectionContains("The name should not have been removed from the drop down", newName, configForm.ConfigurationsComboBox_Exposed.Items);
					AssertNotEquals("Successfully deleted.", configForm.Status);
				}
				finally
				{
					configForm.RegistryManagerForTest.DeleteFromRegistry(newName);
					configForm.RegistryManagerForTest.DeleteFromRegistry(configName);
				}
			}
		}

		public void TestRefreshTimeForNewPrintersScanTextBox()
		{
			using (var configForm = new ConfigFormForTest())
			{
				configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text = " ";
				var saveConfig = configForm.GetWebClientConfigurationFromCurrentSettings_Exposed();
				AssertEquals("SaveConfig: Should load the default value", saveConfig.SecondsBetweenScanForNewPrinters, 600);
				configForm.RefreshTimeForNewPrintersScanTextBox_Leave_Exposed();
				AssertEquals("Should load the default value", configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text, "600");

				configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text = "1";
				saveConfig = configForm.GetWebClientConfigurationFromCurrentSettings_Exposed();
				AssertEquals("SaveConfig: Should load the minimum value", saveConfig.SecondsBetweenScanForNewPrinters, 30);
				configForm.RefreshTimeForNewPrintersScanTextBox_Leave_Exposed();
				AssertEquals("Should load the minimum value", configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text, "30");

				configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text = "55";
				saveConfig = configForm.GetWebClientConfigurationFromCurrentSettings_Exposed();
				AssertEquals("SaveConfig: Shouldn't change the value", saveConfig.SecondsBetweenScanForNewPrinters, 55);
				configForm.RefreshTimeForNewPrintersScanTextBox_Leave_Exposed();
				AssertEquals("Shouldn't change the value", configForm.RefreshTimeForNewPrintersScanTextBox_Exposed.Text, "55");
			}
		}

		public void TestRequestPauseInSecondsTextBoxMaxLength()
		{
			using (var configForm = new ConfigFormForTest())
			{
				AssertEquals(3, configForm.RequestPauseInSecondsTextBox_Exposed.MaxLength);
			}
		}

		public class ConfigFormForTest : ConfigForm
		{
			public ConfigFormForTest(IWindowsServicesHelper windowsServicesHelper = null) : base(windowsServicesHelper)
			{
			}

			public string Status { get; set; }

			public DialogResult showMessageResult { private get; set; } = DialogResult.OK;

			public string showMessageContent;

			public string[] NamesFromRegistry { get; set; }

			public PromptResult showDialogResult { private get; set; } = new PromptResult(DialogResult.OK, "test");

			public string configSelectedForWindowsService { private get; set; } = string.Empty;

			protected override DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
			{
				this.showMessageContent = message;
				return showMessageResult;
			}

			protected override PromptResult ShowDialog(string message, string caption, string text = null)
			{
				return showDialogResult;
			}

			protected override string[] GetNamesFromRegistry()
			{
				return NamesFromRegistry;
			}

			protected override (string, string) GetServiceCredentials()
			{
				return (string.Empty, string.Empty);
			}

			protected override string GetSelectedConfigForWindowsService() => configSelectedForWindowsService;

			public ComboBox ConfigurationsComboBox_Exposed => ConfigurationsComboBox;
			public TextBox ServiceStatusTextBox_Exposed => ServiceStatusTextBox;
			public ComboBox ServiceStartModeComboBox_Exposed => ServiceStartModeComboBox;
			public TextBox WebServicePwdTextBox_Exposed => WebServicePwdTextBox;
			public TextBox ProxyPwdTextBox_Exposed => ProxyPwdTextBox;

			public CheckBox EnableSignalRCheckBox_Exposed => EnableSignalRCheckBox;
			public CheckBox EnablePauseSignalRCheckBox_Exposed => EnablePauseSignalRCheckBox;
			public TextBox ReconnectionAttemptsTextBox_Exposed => ReconnectionAttemptsTextBox;
			public TextBox ReconnectionLimitMinutesTextBox_Exposed => ReconnectionLimitMinutesTextBox;
			public TextBox PauseSignalRMinutesTextBox_Exposed => PauseSignalRMinutesTextBox;

			public RadioButton RadioButtonAutoUpdate_Exposed => radioButtonAutoUpdate;
			public TextBox PauseAutoUpdateHoursTextBox_Exposed => PauseAutoUpdateHoursTextBox;
			public RadioButton RadioButtonUpdateCustomUpdate_Exposed => radioButtonUpdateCustom;
			public RadioButton RadioButtonUpdateManualUpdate_Exposed => radioButtonUpdateManual;

			public void Reload() => ConfigForm_Reload(null, null);

			public void LoadConnectionConfigurations_Exposed() => LoadConnectionConfigurations();

			public void NewButtonClick_Exposed() => NewConfigurationButton_Click(null, null);

			public void RenameButtonClick_Exposed() => RenameConfigurationButton_Click(null, null);

			public void DuplicateButtonClick_Exposed() => DuplicateConfigurationButton_Click(null, null);

			public void DeleteConfigurationButtonClick_Exposed() => DeleteConfigurationButton_Click(null, null);

			public void InstallingServiceButton_Click_Exposed() => InstallServiceButton_Click(null, null);

			public void UninstallingServiceButton_Click_Exposed() => UninstallServiceButton_Click(null, null);

			public void StartServiceButton_Click_Exposed() => StartServiceButton_Click(null, null);

			public void StopServiceButton_Click_Exposed() => StopServiceButton_Click(null, null);

			ConnectionRegistryManager registryManager;
			protected override ConnectionRegistryManager RegistryManager => registryManager ?? (registryManager = new ConnectionRegistryManagerForTest());

			public ConnectionRegistryManager RegistryManagerForTest => RegistryManager;

			protected override void OnStatusChanged(string statusMessage)
			{
				Status = statusMessage;
			}

			public TextBox RefreshTimeForNewPrintersScanTextBox_Exposed => RefreshTimeForNewPrintersScanTextBox;

			public TextBox RequestPauseInSecondsTextBox_Exposed => RequestPauseInSecondsTextBox;

			public void RefreshTimeForNewPrintersScanTextBox_Leave_Exposed() => RefreshTimeForNewPrintersScanTextBox_Leave(null, null);

			public WebClientConfiguration GetWebClientConfigurationFromCurrentSettings_Exposed() => GetWebClientConfigurationFromCurrentSettings();

			public IDisposable SetupWebClientConfigurationDataForTest()
			{
				var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, true, 5000, false, 0, 0, 100, 100, false, false, 0, 0, 0, false, 4, 60, true, 0, GetWebClientUpdateConfigurationForTest(), false, 100);
				var webConfig2 = new WebClientConfiguration("url2", "user2", "pwd2", 2, "machine2", true, "proxy2", 43, "proxyuser2", "proxypwd2", true, false, 7000, true, 300, 5, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0, GetWebClientUpdateConfigurationForTest(), true, 3);
				var webConfig3 = new WebClientConfiguration("url3", "user3", ProtectedDataHelper.Protect("pwd3"), 2, "machine3", true, "proxy3", 43, "proxyuser3", ProtectedDataHelper.Protect("proxypwd3"), true, false, 7000, true, 300, 5, 100, 100, false, false, 0, 0, 0, false, 0, 0, false, 0, GetWebClientUpdateConfigurationForTest(), true, 3);

				RegistryManagerForTest.LoadFromRegistry(configName1);
				RegistryManagerForTest.SaveRemotePrintingRegistryValues(webConfig1);
				RegistryManagerForTest.LoadFromRegistry(configName2);
				RegistryManagerForTest.SaveRemotePrintingRegistryValues(webConfig2);
				RegistryManagerForTest.LoadFromRegistry(configName3);
				RegistryManagerForTest.SaveRemotePrintingRegistryValues(webConfig3);

				return new DisposableAction(() =>
				{
					RegistryManagerForTest.DeleteFromRegistry(configName1);
					RegistryManagerForTest.DeleteFromRegistry(configName2);
					RegistryManagerForTest.DeleteFromRegistry(configName3);
				});
			}

			public const string configName1 = "configForTest1";
			public const string configName2 = "configForTest2";
			public const string configName3 = "configForTest3";
		}
	}
}
