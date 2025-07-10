using System;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing
{
	[TestedType(typeof(StaffCredentialsUserControl))]
	sealed class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestControlVisibility()
		{
			CombineAssertions(() =>
			{
				RunForControl(control =>
				{
					AssertEquals("ICEGATELoginTextBox should be visible", true, control.ICEGATELoginTextBox.Visible);
					AssertEquals("ICEGATEPasswordTextBox should be visible", true, control.ICEGATEPasswordTextBox.Visible);
					AssertEquals("ICEGATEEmailTextBox should be visible", true, control.ICEGATEEmailTextBox.Visible);
					AssertEquals("SendCopyToTextBox should be visible", true, control.NeedCopyOfEmailsCheckBox.Visible);
					AssertEquals("SendCopyToTextBox should be visible", true, control.SendCopyToTextBox.Visible);
					AssertEquals("CertificateAuthorityDropEdit should be visible", true, control.CertificateAuthorityDropEdit.Visible);
					AssertEquals("ChipsetDropEdit should be visible", true, control.ChipsetDropEdit.Visible);
					AssertEquals("SerialNumberTextBox should be visible", true, control.SerialNumberTextBox.Visible);
				});
			});
		}

		public void TestTextBoxControlsCharacterCasing()
		{
			CombineAssertions(() =>
			{
				RunForControl(control =>
				{
					AssertEquals("ICEGATELoginTextBox allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.ICEGATELoginTextBox.CharacterCasing);
					AssertEquals("ICEGATEPasswordTextBox allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.ICEGATEPasswordTextBox.CharacterCasing);
					AssertEquals("ICEGATEEmailTextBox allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.ICEGATEEmailTextBox.CharacterCasing);
					AssertEquals("SendCopyToTextBox allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.SendCopyToTextBox.CharacterCasing);
					AssertEquals("CertificateAuthorityDropEdit allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.CertificateAuthorityDropEdit.CharacterCasing);
					AssertEquals("ChipsetDropEdit allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.ChipsetDropEdit.CharacterCasing);
					AssertEquals("SerialNumberTextBox allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.SerialNumberTextBox.CharacterCasing);
				});
			});
		}

		public void TestGroupBoxTitle()
		{
			CombineAssertions(() =>
			{
				RunForControl(control =>
				{
					AssertEquals("ICEGATEProfileGroupBox title", "ICEGATE Profile", control.ICEGATEProfileGroupBox.CaptionResourceString.Caption);
					AssertEquals("DscTokenProfileGroupBox title", "Digital Signature Certificate Token Profile", control.DscTokenProfileGroupBox.CaptionResourceString.Caption);
				});
			});
		}

		public void TestChooseCertificateButton()
		{
			RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
			Factory.Save();

			CombineAssertions(() =>
			{
				RunForControl(control =>
				{
					var staffWrapper = (GlbStaffWrapper)control.CurrentDataItem;
					var certificatePassword = staffWrapper.CertificatePassword;
					var button = control.FindSingle<ZButton>("ChooseCertificateButton");
					button.PerformClick();
					AssertEquals("When LibraryName is empty, LastMessage", "You have not entered a Chipset Manufacturer.", UnitTestUserNotification.Instance.LastMessage.Text);

					certificatePassword.GP_Name = "WatchData";
					button.PerformClick();
					AssertEquals("When LibraryName is unknown, LastMessage", "Cannot find PKCS#11 library.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			});
		}

		public void TestClearCertificateButton()
		{
			CombineAssertions(() =>
			{
				RunForControl(control =>
				{
					var staffWrapper = (GlbStaffWrapper)control.CurrentDataItem;
					var certificatePassword = staffWrapper.CertificatePassword;
					var button = control.FindSingle<ZButton>("ClearCertificateButton");

					certificatePassword.GP_CertificateAuthority = "eMudhra";
					certificatePassword.GP_Name = "WatchData";
					certificatePassword.GP_CertificateSerialNumber = "123";
					button.PerformClick();

					AssertEquals("GP_CertificateAuthority", ZString.Empty, certificatePassword.GP_CertificateAuthority);
					AssertEquals("GP_Name", ZString.Empty, certificatePassword.GP_Name);
					AssertEquals("GP_CertificateSerialNumber", ZString.Empty, certificatePassword.GP_CertificateSerialNumber);
				});
			});
		}

		void RunForControl(Action<StaffCredentialsUserControl> methodToRun)
		{
			using (var form = this.GetFormToBash())
			using (var control = new StaffCredentialsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				methodToRun.Invoke(control);
			}
		}
	}
}
