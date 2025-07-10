using System.IO;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(FileSaveToOpenForm))]
	public class FileSaveToOpenFormTest : ZFormBasherTest
	{
		public void TestemailButton_Click_LegalCharacters()
		{
			AssertNoExceptionThrown(() =>
			{
				using (EnvProxy.Instance.CurrentUser.SetUserEmailAddressInTESTINGOnly("user@test.com"))
				using (var form = FileSaveToOpenForm.InstanceForTesting())
				{
					var closed = false;
					form.FormClosed += new FormClosedEventHandler(delegate
					{
						closed = true;
					});
					form.FileGenerator = new FileSaveToOpenForm.FileGeneratorCallback(delegate(Stream fileStream)
					{
						var data = System.Text.Encoding.ASCII.GetBytes("Save me");
						fileStream.Write(data, 0, data.Length);
					});
					form.DefaultFileName = "<\\|hiclara.exe";
					form.Show();
					form.emailButton.PerformClick();
					Assert(closed);
				}
			});
		}

		public void TestDefaultFileName()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var form = FileSaveToOpenForm.InstanceForTesting())
				{
					form.DefaultFileName = "<\\|hiclara.exe";
				}
			});
		}

		protected override Form GetFormToBashCore()
		{
			return FileSaveToOpenForm.InstanceForTesting();
		}

		public void TestEmail()
		{
			using (EnvProxy.Instance.CurrentUser.SetUserEmailAddressInTESTINGOnly(""))
			using (var form = FileSaveToOpenForm.InstanceForTesting())
			{
				AssertEquals("emailButton.Enabled", false, form.emailButton.Enabled);
			}

			using (EnvProxy.Instance.CurrentUser.SetUserEmailAddressInTESTINGOnly("user@test.com"))
			using (var form = FileSaveToOpenForm.InstanceForTesting())
			{
				AssertEquals("emailButton.Enabled", true, form.emailButton.Enabled);
				var closed = false;
				form.FormClosed += new FormClosedEventHandler(delegate
				{
					closed = true;
				});
				form.FileGenerator = new FileSaveToOpenForm.FileGeneratorCallback(delegate(Stream fileStream)
				{
					var data = System.Text.Encoding.ASCII.GetBytes("Save me");
					fileStream.Write(data, 0, data.Length);
				});
				form.DefaultFileName = "File.txt";
				form.Show();
				form.emailButton.PerformClick();
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
				AssertEquals("user@test.com", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
				AssertEquals("File.txt", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName);
				AssertEquals("Save me", System.Text.Encoding.ASCII.GetString(EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Attachments[0].Data));
				AssertEquals("The document has been emailed as an attachment to user@test.com.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(closed);
			}
		}

		public void TestSaveAsFile()
		{
			using (var form = FileSaveToOpenForm.InstanceForTesting())
			using (var tempFile = TempFile.New())
			{
				var closed = false;
				form.FormClosed += new FormClosedEventHandler(delegate
				{
					closed = true;
				});
				form.FileGenerator = new FileSaveToOpenForm.FileGeneratorCallback(delegate(Stream fileStream)
				{
					var data = System.Text.Encoding.ASCII.GetBytes("Save me");
					fileStream.Write(data, 0, data.Length);
				});
				form.DefaultFileName = "File.txt";
				form.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.saveButton.PerformClick();
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				AssertFileSameAsString(tempFile.Filename, "Save me");
				AssertEquals(string.Format("The document was saved as {0}", tempFile.Filename), UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(closed);
			}
		}

		public void TestCancel()
		{
			using (var form = FileSaveToOpenForm.InstanceForTesting())
			{
				var closed = false;
				form.FormClosed += new FormClosedEventHandler(delegate
				{
					closed = true;
				});
				form.Show();
				form.cancelButton.PerformClick();
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				Assert(closed);
			}
		}
	}
}
