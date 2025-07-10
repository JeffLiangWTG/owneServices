using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DigitalCertificateControlWithExportTest : TestCase
	{
		public void TestSaveToDisk()
		{
			using (DummyDigitalCertificateControlWithExport control = new DummyDigitalCertificateControlWithExport(new FileUpLoaderX509CertificateRegistryEditorInfo()))
			using (var form = new Form())
			{
				form.Controls.Add(control);
				form.Show();
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.SaveToDiskButton.PerformClick();
				AssertEquals("An error message should be shown.", "There is currently no Certificate to save to disk.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				byte[] bytes = new byte[] { 1, 2, 3 };
				control.SetFileData(bytes);

				string fileName = Path.Combine(Env.TempPath, "TestSaveToDisk.cer");

				try
				{
					control.LoginDialogResult = DialogResult.Cancel;
					control.SaveToDiskButton.PerformClick();
					AssertNull("There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("File should not be created.", false, File.Exists(fileName));

					control.LoginDialogResult = DialogResult.OK;
					control.LoginPassword = "IAmBrett";
					control.SaveToDiskButton.PerformClick();
					AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("File should not be created.", false, File.Exists(fileName));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					control.LoginPassword = User.MasterPassword;
					control.SaveDialogFileName = fileName;
					control.SaveDialogResult = DialogResult.Cancel;
					control.SaveToDiskButton.PerformClick();
					AssertNull("There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("File should not be created.", false, File.Exists(fileName));

					control.SaveDialogResult = DialogResult.OK;
					control.ExceptionToThrowOnSaveToDisk = new Exception("Can't save!");
					control.SaveToDiskButton.PerformClick();
					AssertEquals("An error message should be shown.", "An error occurred while saving.\r\n\r\nCan't save!", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					control.ExceptionToThrowOnSaveToDisk = null;
					control.SaveToDiskButton.PerformClick();
					AssertNull("There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("File should be created.", true, File.Exists(fileName));
					AssertEquals("File data should be the same as the control's.", bytes, File.ReadAllBytes(fileName));
				}
				finally
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
			}
		}
	}
}
