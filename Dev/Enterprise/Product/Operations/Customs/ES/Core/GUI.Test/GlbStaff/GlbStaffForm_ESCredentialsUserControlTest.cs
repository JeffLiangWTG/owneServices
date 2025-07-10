using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(GlbStaffForm_ESCredentialsUserControl))]
	class GlbStaffForm_ESCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestCertGrid()
		{
			using (var form = new GlbStaffForm_ESCredentialsUserControl())
			{
				var certGrid = (ZGrid)form.Controls.Find("ESCertGrid", true).Single();
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Grid is visible", true, certGrid.Visible);
					AssertEquals("Grid have 8 columns", 8, certGrid.ColumnStyles.Count);

					AssertEquals("GP_MailboxID isn't ReadOnly", false, certGrid.GetColumnStyle(GlbExternalPassword.Schema.GP_MailBoxID).IsReadOnly);
					AssertEquals("GP_PasswordStatus is ReadOnly", true, certGrid.GetColumnStyle(GlbExternalPassword.Schema.GP_PasswordStatus).IsReadOnly);

					AssertNotNull("Context menu to change cert status exists", certGrid.ContextMenu.MenuItems.FindByText("Activate/Deactivate"));
				});
			}
		}

		public void TestAuthorisationsGrid()
		{
			using (var form = new GlbStaffForm_ESCredentialsUserControl())
			{
				var certGrid = (ZGrid)form.Controls.Find("ESAuthorisationsGrid", true).Single();
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Grid is visible", true, certGrid.Visible);
					AssertEquals("Grid have 3 columns", 3, certGrid.ColumnStyles.Count);

					AssertEquals("GEA_SystemCreateTimeUtc is ReadOnly", true, certGrid.GetColumnStyle(GlbExternalPasswordAuthorisation.Schema.GEA_SystemCreateTimeUtc).IsReadOnly);
					AssertEquals("StaffName is ReadOnly", true, certGrid.GetColumnStyle(GlbExternalPasswordAuthorisation.Schema.StaffName).IsReadOnly);

					AssertEquals("GEA_GS_AuthorisedStaff isn't ReadOnly", false, certGrid.GetColumnStyle(GlbExternalPasswordAuthorisation.Schema.GEA_GS_AuthorisedStaff).IsReadOnly);
				});
			}
		}

		public void TestChangeCertStatus_Validations()
		{
			var staff = Factory.New<MasterFiles.Business.GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert.GP_PasswordStatus = MasterFiles.Business.PasswordStatusList.Codes.PasswordOK;

			cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert2";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert.GP_PasswordStatus = MasterFiles.Business.PasswordStatusList.Codes.PasswordOK;

			Factory.Save();

			using (var form = new ZForm(wrapper))
			using (var control = new GlbStaffForm_ESCredentialsUserControl())
			{
				form.Controls.Add(control);
				var certGrid = (ZGrid)form.Controls.Find("ESCertGrid", true).Single();
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var changeCertStatusMenuItem = certGrid.ContextMenu.MenuItems.FindByText("Activate/Deactivate");

				CombineAssertions(() =>
				{
					certGrid.Select();
					certGrid.Focus();
					changeCertStatusMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

					certGrid.SelectAllElements();
					changeCertStatusMenuItem.PerformClick();
					AssertEquals("Can only select one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

					certGrid.SelectSingleElement(cert);
					changeCertStatusMenuItem.PerformClick();
					AssertEquals("Can only change status of Active (VAL) or Inactive (INA) certs", "Please select active (VAL) or inactive (INA) certificate", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestChangeCertStatus()
		{
			var staff = Factory.New<MasterFiles.Business.GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";
			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert.GP_PasswordStatus = MasterFiles.Business.PasswordStatusList.Codes.Deactivated;

			Factory.Save();

			using (var form = new ZForm(wrapper))
			using (var control = new GlbStaffForm_ESCredentialsUserControl())
			{
				form.Controls.Add(control);
				var certGrid = (ZGrid)form.Controls.Find("ESCertGrid", true).Single();
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var changeCertStatusMenuItem = certGrid.ContextMenu.MenuItems.FindByText("Activate/Deactivate");

				CombineAssertions(() =>
				{
					AssertEquals("Before doing anything status is INA", "INA", cert.GP_PasswordStatus);

					certGrid.SelectAllElements();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					changeCertStatusMenuItem.PerformClick();
					AssertEquals("When selecting Cancel in pop up nothing is changed (status is INA)", "INA", cert.GP_PasswordStatus);
					AssertEquals("Last pop up is the confirmation when INA", "Are you sure you want to activate this certificate?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					certGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before changing status", cert.Factory);
					changeCertStatusMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After changing status", cert.Factory);
					AssertEquals("When selecting OK in pop up the status is changed from INA to VAL", "VAL", cert.GP_PasswordStatus);
					AssertEquals("Last pop up is the message informing of the change from INA to VAL", "Status changed from INA to VAL", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					certGrid.SelectAllElements();
					changeCertStatusMenuItem.PerformClick();
					AssertEquals("When selecting Cancel in pop up nothing is changed (status is VAL)", "VAL", cert.GP_PasswordStatus);
					AssertEquals("Last pop up is the confirmation when VAL", "Are you sure you want to deactivate this certificate? If this certificate is deactivated it can no longer be eligible for any declaration.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					certGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before changing status", cert.Factory);
					changeCertStatusMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After changing status", cert.Factory);
					AssertEquals("When selecting OK in pop up the status is changed from VAL to INA", "INA", cert.GP_PasswordStatus);
					AssertEquals("Last pop up is the message informing of the change from VAL to INA", "Status changed from VAL to INA", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
