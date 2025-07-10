using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Main.Startup.Tools.JetBrains;
using Enterprise.Startup.Tools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools
{
	public class DialogService : IDialogService
	{
		public DialogService(Form parentForm)
		{
			this.parentForm = parentForm;
		}

		public bool AcceptLicenseAgreement(string license)
		{
			using (var licenseForm = new LicenseForm(license))
			{
				var result = licenseForm.ShowDialog(parentForm);
				return result == DialogResult.OK;
			}
		}

		public string Download(string fileUrl)
		{
			using (var downloadForm = new DownloadForm(new Uri(fileUrl)))
			{
				var result = downloadForm.ShowDialog();
				return result == DialogResult.OK
					? downloadForm.DownloadedFilePath
					: string.Empty;
			}
		}

		public string SelectFolder(string defaultFolder, string description)
		{
			using (var dlg = CreateSelectFolderDialog())
			{
				dlg.SelectedPath = defaultFolder;
				dlg.Description = description;

				// Required so that the dialog doesn't allow the user to select an unmappable path in which case the dialog will return null
				// and thus cause null reference exception down the code.
				dlg.RequireMappablePath = true;

				while (true)
				{
					var res = dlg.ShowDialog(null, canceledIfPathCannotAccess: false);
					if (res != DialogResult.OK)
					{
						break;
					}

					if (CheckFolderAccess(dlg.UnmappedSelectedPath))
					{
						return dlg.UnmappedSelectedPath;
					}
				}
			}

			return null;
		}

		public Stream SelectSaveAs(string fileName)
		{
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.CheckFileExists = true;
				dialog.OverwritePrompt = true;
				dialog.FileName = fileName;
				dialog.Title = Res.GetString("D57EEC0F-2190-45C7-A739-DA3997AAB7F4", "Save as...");

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					return dialog.OpenFile();
				}

				return default;
			}
		}

		public void ProfilePerformance(ProfilePerformanceModel model)
		{
			var form = new ProfilePerformanceForm(model);
			form.Show();
		}

		public void ProfileMemory(ProfileMemoryModel model)
		{
			var form = new ProfileMemoryForm(model);
			form.Show();
		}

		public virtual IZFolderBrowserDialog CreateSelectFolderDialog()
		{
			return new ZFolderBrowserDialog();
		}

		public virtual bool CheckFolderAccess(string path)
		{
			try
			{
				var testFile = Path.Combine(path, "permission-check.tmp");
				using (ZSaveFileDialog.OpenFile(testFile)) { }
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				Globals.Message.ShowInformation(
					Res.GetString("ba8390f1-cab8-4593-b172-8df20da24354",
						"The current user has no write permission in this folder, please pick different folder."),
					Res.GetString("20755e89-51b3-4165-bad5-193db029bc9c", "Unable to use the path"));
			}
			catch (IOException ex)
			{
				Globals.Message.ShowInformation(
					Res.GetString("4990fad6-22ef-41a8-8525-a5bf2b65d825",
						"Unable to write in this folder due to '{0}'. Please pick different folder.", ex.Message),
					Res.GetString("20755e89-51b3-4165-bad5-193db029bc9c", "Unable to use the path"));
			}
			return false;
		}

		readonly Form parentForm;
	}
}
