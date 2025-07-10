using System;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemUserControl))]
	sealed class FTPSettingsRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new FTPSettings(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var userControl = (FTPSettingsRegistryItemUserControl)control;
			return control.FindSingleOrDefault<ZTextBox>(x => x.Name == "InFolderTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "OutFolderTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "PasswordTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZButton>(x => x.Name == "PasswordViewButton").ReadOnly &&
				control.FindSingleOrDefault<ZCalcEdit>(x => x.Name == "PortCaclEdit").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "ServerTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "UserNameTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZRadioButton>(x => x.Name == "PassiveNoRadioButton").ReadOnly &&
				control.FindSingleOrDefault<ZRadioButton>(x => x.Name == "PassiveYesRadioButton").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "FTPStatusTextBox").ReadOnly;
		}

		public override void TestHasChangesOnPreviouslySavedObject()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			base.TestHasChangesOnPreviouslySavedObject();
		}

		public void TestFTPStatusTestButton()
		{
			using (var ftpTestHelper = new FtpTestHelper("TestName", "TestPassword"))
			{
				ftpTestHelper.Start();

				var fTPSettings = new FTPSettings();
				fTPSettings.UserName = ftpTestHelper.UserName;
				fTPSettings.Password = ftpTestHelper.Password;
				fTPSettings.Server = ftpTestHelper.ServerAddress.ToString();
				fTPSettings.Port = ftpTestHelper.Port;
				fTPSettings.Passive = true;

				using (var control = new FTPSettingsRegistryItemUserControl())
				{
					control.SetDataBinding(fTPSettings, null);
					var statusButton = control.FindSingleOrDefault<ZButton>(x => x.Name == "FTPStatusTestButton");
					var statusTextBox = control.FindSingleOrDefault<ZTextBox>(x => x.Name == "FTPStatusTextBox");
					statusButton.PerformClick();
					AssertEquals("Valid – Connection to the designated FTP server succeeded.", fTPSettings.Status);

					fTPSettings.Password = "wrong password";
					statusButton.PerformClick();
					AssertEquals("Error – Connection to the designated FTP server failed. Please check the provided information.", fTPSettings.Status);
				}
			}
		}
	}
}
