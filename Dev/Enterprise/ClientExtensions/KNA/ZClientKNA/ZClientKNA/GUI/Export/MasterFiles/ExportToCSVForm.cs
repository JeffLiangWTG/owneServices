using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ExportToCSVForm : ZChildForm
	{
		public ExportToCSVForm(NonPersistentBusinessObject business) : base(business)
		{
			Init();
		}

		public ExportToCSVForm()
		{
			Init();
		}

		void Init()
		{
			SelectFileButton.Click += SelectFileButton_Click;
			StartButton.Click += StartButton_Click;
			MessageStatusBarPanel.Text = Res.GetString("ExportToCSVForm|MessageStatusBarPanel", "Enter a filename and click the \"Start Export\" button");
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Select File

		void SelectFileButton_Click(object sender, EventArgs e)
		{
			SelectFile();
		}

		protected virtual void SelectFile()
		{
			if (SaveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				FileNameTextBox.Text = SaveFileDialog.UnmappedFileName;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is not for DAT or DAT clients, but for real user clients who will generally have a C drive, on which their document is most likely to reside")]
		public ZSaveFileDialog SaveFileDialog
		{
			get
			{
				if (fSaveFileDialog == null)
				{
					fSaveFileDialog = new ZSaveFileDialog();
					fSaveFileDialog.Filter = FileDialogFilter;
					fSaveFileDialog.InitialDirectory = "C:\\";
				}

				return fSaveFileDialog;
			}
		}

		public virtual string FileDialogFilter
		{
			get { return Res.GetString("8d877f55-a838-432d-abde-c488604a7f77", "Excel files (*.CSV)|*.CSV|All files (*.*)|*.*"); }
		}

		ZSaveFileDialog fSaveFileDialog;

		#endregion

		#region Export Data

		void StartButton_Click(object sender, EventArgs e)
		{
			var dataLocation = FileNameTextBox.Text.Trim();

			if (string.IsNullOrEmpty(dataLocation))
			{
				Globals.Message.ShowError(Res.GetString("1533493a-d7b5-413d-8ba9-c83e85fda435", "Please enter the location of the file you wish to export data to."));
			}
			else if (ConfirmSaveData(dataLocation))
			{
				SaveSelectedData(dataLocation);
			}
		}

		public virtual bool ConfirmSaveData(ZString filename)
		{
			return true;
		}

		protected void SaveSelectedData(string dataToLoad)
		{
			InitialiseRun();

			try
			{
				SaveSpecificDataType(dataToLoad);
			}
			catch (IOException fileException)
			{
				Globals.Message.ShowError(Res.GetString("d6b662d5-f7fd-437d-a634-ef8972fde45a", "Cannot export data! The detailed exception is:\r\n\r\n{0}", fileException.Message));
			}
			finally
			{
				FinaliseRun();
			}
		}

		public virtual void SaveSpecificDataType(string dataToLoad)
		{
		}

		#endregion

		#region Update Form

		void InitialiseRun()
		{
			SetButtons(false);
			OutputListBox.Items.Clear();
			MainStatusBar.Text = Res.GetString("ExportToCSVForm|InitialiseRun", "Exporting data... Please wait...");
		}

		void FinaliseRun()
		{
			SetButtons(true);
			MainStatusBar.Text = Res.GetString("dd2602e8-0616-495b-98a1-dad529955d0e", "Export Complete.");
		}

		void SetButtons(bool isEnabled)
		{
			FileNameTextBox.Enabled = isEnabled;
			SelectFileButton.Enabled = isEnabled;
			StartButton.Enabled = isEnabled;
			CopyLogToClipboardButton.Enabled = isEnabled;
			CopyLogToClipboardButton.Visible = isEnabled;
			CloseButton.Enabled = isEnabled;
			CloseButton.Visible = isEnabled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void DataSaver_ProgressChanged(DataSave sender, SaveProgressChangedEventArgs e)
		{
			ProgressBar.Maximum = e.ProductsExporting;
			ProgressBar.Value = e.CurrentRow;
			Application.DoEvents();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Logging

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void DataSaver_LogUpdated(DataSave sender, SaveLogUpdatedEventArgs e)
		{
			string logText = e.LogMessage;

			if (logText.StartsWith("\r\n"))
			{
				OutputListBox.Items.Add("");
				logText = logText.Remove(0, 2);
			}

			if (logText.EndsWith("\r\n"))
			{
				logText = logText.Replace("\r\n", "");
				OutputListBox.Items.Add(logText);
				OutputListBox.Items.Add("");
			}
			else
			{
				OutputListBox.Items.Add(logText);
			}

			OutputListBox.SelectedIndex = OutputListBox.Items.Count - 1;
			OutputListBox.SelectedIndex = -1;

			Application.DoEvents();
		}

		void CopyLogToClipboardButton_Click(object sender, EventArgs e)
		{
			string clipboardData = GetLog();

			if (!SafeClipboard.SetDataObject(clipboardData, true))
			{
				string fileCreated = CreateLogInDataDirectory(clipboardData);
				Globals.Message.ShowInformation(Res.GetString("161665f8-1587-4c96-92bb-5099b3b135b8", "Copy to clipboard failed.\r\nThe log has been copied to a temporary file.\r\nLog File is: {0}", fileCreated));
			}
		}

		protected string GetLog()
		{
			StringBuilder builder = new StringBuilder();
			foreach (string line in OutputListBox.Items)
			{
				builder.Append(line);
				builder.Append(System.Environment.NewLine);
			}

			return builder.ToString();
		}

		protected string CreateLogInDataDirectory(string logData)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			ZString fileName = ClientSharedComponents.SharedUtil.GetFinalPath(FileNameTextBox.Text);

			DirectoryInfo directory = new DirectoryInfo(fileName.Trim());

			if (directory.FullName.EndsWith(".csv") || directory.FullName.EndsWith(".CSV"))
			{
				directory = directory.Parent;
			}

			try
			{
				if (!directory.Exists)
				{
					directory.Create();
				}

				string fileName1 = Path.Combine(directory.FullName, "ProductDataImportLog." + currentTimeString + ".txt");
				using (StreamWriter sw = new StreamWriter(fileName1, true))
				{
					sw.WriteLine(logData);
					sw.Flush();
				}
				return fileName1;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error saving product update log file.", ex);
				return Res.GetString("22d70385-c773-4ec1-b48f-2d1775dc2dc2", "Not Created");
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (fSaveFileDialog != null)
				{
					fSaveFileDialog.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
