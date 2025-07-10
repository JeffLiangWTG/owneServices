using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.AirCargo
{
	public abstract class AirCargoFileImporter
	{
		public AirCargoFileImporter(Form modalForm)
		{
			this.ModalForm = modalForm;
		}

		public void Import()
		{
			if (IsEnvironmentCorrectlySetup)
			{
				string fileFullPath = GetFilePathToImport().Trim();
				if (!string.IsNullOrEmpty(fileFullPath))
				{
					using (ZOpenFileDialog.ForceLocalFile(ref fileFullPath))
					{
						if (IsImportFileOk(fileFullPath))
						{
							LoadImportForm(fileFullPath);
						}
					}
				}
			}
		}

		#region Implementation

		#region abstract

		protected abstract bool CheckEnvironmentValid(out string errorMessage);

		protected abstract ImportManager GetNewImportManager(string fileFullPath);

		protected abstract string ValidFilePattern
		{ get; }

		protected abstract string FileExtensionCore
		{ get; }

		protected abstract string FileType
		{ get; }

		protected abstract string SourceDirectory
		{ get; }

		protected abstract ZForm GetNewForm(ImportManager manager);

		#endregion

		bool IsEnvironmentCorrectlySetup
		{
			get
			{
				bool result = true;
				string errorMessage = "";
				if (!CheckEnvironmentValid(out errorMessage))
				{
					result = false;
					Globals.Message.ShowError(errorMessage, string.Format("{0} Directory Not Correctly Setup", FileType));
				}
				return result;
			}
		}

		protected string FileExtension
		{
			get { return FileExtensionCore.Trim().Replace(".", ""); }
		}

		bool IsImportFileOk(string fileFullPath)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(fileFullPath))
			{
				if (File.Exists(fileFullPath))
				{
					if (IsValidFile(fileFullPath))
					{
						result = true;
					}
					else
					{
						Globals.Message.ShowError(string.Format("File '{0}' is not an {1} file", fileFullPath, FileType), "Invalid File Type");
					}
				}
				else
				{
					Globals.Message.ShowError(string.Format("File '{0}'", fileFullPath), "File Does Not Exist");
				}
			}
			return result;
		}

		void LoadImportForm(string fileFullPath)
		{
			ImportManager manager = GetNewImportManager(fileFullPath);
			if (manager != null)
			{
				NotificationBuffer buffer = new NotificationBuffer();
				LoadFromFile(manager, buffer);

				if (!buffer.HasErrors)
				{
					ZForm form = GetNewForm(manager);
					if (form != null)
					{
						ZFormModaliser.Show(form, ModalForm);
					}
				}
				else
				{
					Globals.Message.ShowError(buffer.AsString, "Error Loading Data");
					manager.MoveFileToProcessedDirectory();
				}
			}
		}

		void LoadFromFile(ImportManager manager, NotificationBuffer buffer)
		{
			using (LoadProgressForm = new ProgressForm())
			{
				LoadProgressForm.ShowCancelButton = false;
				try
				{
					manager.LoadProgress += new TNTProgressEventHandler(Manager_LoadProgress);
					LoadProgressForm.Show();
					manager.LoadFromFile(buffer);
				}
				finally
				{
					manager.LoadProgress -= new TNTProgressEventHandler(Manager_LoadProgress);
				}
				LoadProgressForm.Close();
			}
		}

		void Manager_LoadProgress(object sender, TNTProgressEventArgs e)
		{
			LoadProgressForm.Status = e.Message;
			LoadProgressForm.PercentComplete = e.PercentComplete;
		}

		bool IsValidFile(ZString filename)
		{
			FileInfo file = new FileInfo(filename);
			bool result = Regex.IsMatch(file.Name, ValidFilePattern, RegexOptions.IgnoreCase);
			return result;
		}

		string GetFilePathToImport()
		{
			string result = "";
			if (Globals.IsTest)
			{
				result = TestFilePath;
			}
			else
			{
				using (ZOpenFileDialog dialog = new ZOpenFileDialog())
				{
					dialog.InitialDirectory = SourceDirectory;
					dialog.CheckFileExists = true;
					dialog.Filter = String.Format("{0} Files (*.{1})|*.{1}|All Files (*.*)|*.*", FileType, FileExtension);
					dialog.Title = String.Format("Select an {0} file to import", FileType);

					if (dialog.ShowDialog() == DialogResult.OK)
					{
						result = dialog.UnmappedFileName;
					}
				}
			}
			return result;
		}

		internal string TestFilePath = "";

		ProgressForm LoadProgressForm;
		readonly Form ModalForm;

		#endregion
	}
}
