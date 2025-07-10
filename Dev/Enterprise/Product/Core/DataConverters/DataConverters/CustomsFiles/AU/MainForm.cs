using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataConverters.CustomsFiles.AU.DataImporters.DbCyber2;
using Enterprise.DataConverters.GUI;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataConverters.CustomsFiles
{
	public partial class MainForm : KForm, IZForm, ICaptionRenderingSupport, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public MainForm()
		{
			InitializeComponent();
			this.Text = Res.GetString("3c6f46d1-bf5d-48da-bac3-e3b57a72d0c0", "Customs Files Data Import");
		}

		#region ILicensedComponent Members

		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get
			{
				if (fLicensedComponentManager == null)
				{
					fLicensedComponentManager = new LicensedComponentManager(this);
				}

				return fLicensedComponentManager;
			}
		}
		LicensedComponentManager fLicensedComponentManager;

		#endregion

		#region IZForm Members

		public ControllerID ControllerID
		{
			get { return fControllerID; }
			set { fControllerID = value; }
		}
		ControllerID fControllerID;

		public ModuleResultsBusinessObject ModuleResultsBusinessObject
		{
			set { }
		}

		public ODisplayMode DisplayMode
		{
			get
			{
				return ODisplayMode.Undefined;
			}
			set
			{
			}
		}

		public IBusiness BusinessEntityForPersistingForm { get; set; }
		public Guid IdentifierForPersistingForm { get; set; }
		bool IZForm.IsActivityLogFinished { get; set; }

		#endregion

		#region Copy Log To Clipboard

		void CopyLogToClipboardButton_Click(object sender, EventArgs e)
		{
			string clipboardData = "";
			foreach (object line in OutputListBox.Items)
			{
				clipboardData += line + "\r\n";
			}

			if (!SafeClipboard.SetDataObject(clipboardData, true))
			{
				CreateLogInDataDirectory(clipboardData);
				Globals.Message.ShowInformation("Copy to clipboard does not work with this connection." + System.Environment.NewLine + "The log has instead been written to file in the data directory.");
			}
		}

		void SaveLogOutput()
		{
			string logData = "";
			foreach (object line in OutputListBox.Items)
			{
				logData += line + "\r\n";
			}
			CreateLogInDataDirectory(logData);
		}

		void CreateLogInDataDirectory(string logData)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			DirectoryInfo directory = new DirectoryInfo(PathTextBox.Text.Trim());
			if (directory.FullName.EndsWith(".csv") || directory.FullName.EndsWith(".CSV"))
			{
				directory = directory.Parent;
			}

			try
			{
				string importData = "Parts";
				if (ClassificationRadioButton.Checked || LimitedClassRadioButton.Checked)
				{
					importData = "ClassLookup";
				}

				string fileName = Path.Combine(directory.FullName, importData + "DataImportLog." + currentTimeString + ".txt");
				using (StreamWriter sw = new StreamWriter(ZSaveFileDialog.OpenFile(fileName)))
				{
					sw.WriteLine(logData);
					sw.Flush();
				}
			}
			catch (IOException) { }
			catch (ArgumentException) { }
			catch (ObjectDisposedException) { }
		}

		#endregion

		#region Start Import Data

		void StartConversionButton_Click(object sender, EventArgs e)
		{
			string dataLocation = PathTextBox.Text.Trim();
			if (string.IsNullOrEmpty(dataLocation))
			{
				Globals.Message.ShowError("Please enter the directory or file location");
			}
			else if (ExcelRadioButton.Checked && !System.IO.File.Exists(dataLocation))
			{
				Globals.Message.ShowError(String.Format("File {0} doesn't exist!", dataLocation));
			}
			else
			{
				try
				{
					InitialiseRun(InterbaseRadioButton.Checked);

					if (InterbaseRadioButton.Checked)
					{
						ImportData();
					}
					else
					{
						var dataType = OleDBConnectionTypes.Excel;
						DataConverter = new Converter(dataLocation, dataType, ClassificationRadioButton.Checked, TreatExcludeRadioButton.Checked, LimitedClassRadioButton.Checked);
						DataConverter.OnProgress += new EventHandler(ImportProgress);
						DataConverter.OnError += new EventHandler(AConverter_OnError);
						DataConverter.ImportData();
					}
				}
				catch (ImportValidationException ex)
				{
					DisplayGeneralErrorMessage(ex.Message);
				}
				catch (System.Data.OleDb.OleDbException ex)
				{
					DisplayGeneralErrorMessage(ex.Message);
				}
				catch (IOException ex)
				{
					DisplayGeneralErrorMessage(ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					DisplayGeneralErrorMessage(ex.Message);
				}
				finally
				{
					FinaliseRun();
				}
			}
		}

		protected void ImportData()
		{
			ZBool toCSVFile = ToCSVCheckBox.Checked;
			ZString connectionString = ConnectionTextBox.Text.Trim();
			if (ClassificationRadioButton.Checked)
			{
				LookupDataImporter lookupImporter = new LookupDataImporter(Logger, connectionString, TreatExcludeRadioButton.Checked, toCSVFile);
				lookupImporter.Import();
			}
			else if (ProductsRadioButton.Checked)
			{
				PartDataImporter partImporter = new PartDataImporter(Logger, connectionString, TreatExcludeRadioButton.Checked, toCSVFile);
				partImporter.Import();
			}
		}

		#endregion

		#region Implementation

		#region Importer Progress Event Handlers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Logger_OnUpdateCounters(object sender, EventArgs e)
		{
			ProgressLogger logger = (ProgressLogger)sender;

			RowsUpdatedLabel.Text = logger.RecordsUpdated.ToString();
			RowsUpdatedLabel.Refresh();
			RowsExcludedLabel.Text = logger.RecordsExcluded.ToString();
			RowsExcludedLabel.Refresh();
			RowsImportingLabel.Text = logger.RecordsToBeProcessed.ToString();
			RowsImportingLabel.Refresh();
			RowsProcessedLabel.Text = logger.RecordsProcessed.ToString();
			RowsProcessedLabel.Refresh();

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Logger_OnUpdateLogText(object sender, EventArgs e)
		{
			this.OutputListBox.Items.Add(sender);
			Application.DoEvents();
		}

		#endregion

		void InitialiseRun(bool importingInterbaseData)
		{
			if (importingInterbaseData)
			{
				Logger = new ProgressLogger(OleDBConnectionTypes.Interbase);
			}
			else
			{
				Logger = new ProgressLogger();
			}

			Logger.UpdateCounters += new EventHandler(Logger_OnUpdateCounters);
			Logger.UpdateLogText += new EventHandler(Logger_OnUpdateLogText);

			OutputListBox.Items.Clear();
			LogDetailsDisplayed = 0;
			RowsProcessedLabel.Text = "0";
			RowsProcessedLabel.Refresh();
			SetButtons(true);
			CopyLogToClipboardButton.Enabled = false;
			CopyLogToClipboardButton.Visible = false;
			CloseButton.Enabled = false;
			CloseButton.Visible = false;
			StatusLabel.Text = "Importing data...Please wait...";
		}

		void FinaliseRun()
		{
			Logger.UpdateCounters -= new EventHandler(Logger_OnUpdateCounters);
			Logger.UpdateLogText -= new EventHandler(Logger_OnUpdateLogText);

			StatusLabel.Text = "Import Complete.";
			SetButtons(false);
			CopyLogToClipboardButton.Enabled = true;
			CopyLogToClipboardButton.Visible = true;
			CloseButton.Enabled = true;
			CloseButton.Visible = true;
			SaveLogOutput();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ImportProgress(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				ConverterData data = (ConverterData)sender;

				RowsImportingLabel.Text = data.RecordsImporting.ToString();
				RowsImportingLabel.Refresh();

				RowsProcessedLabel.Text = data.CurrentRow.ToString();
				RowsProcessedLabel.Refresh();

				RowsExcludedLabel.Text = data.RecordsExcluded.ToString();
				RowsExcludedLabel.Refresh();

				RowsUpdatedLabel.Text = data.RecordsUpdated.ToString();
				RowsUpdatedLabel.Refresh();

				int logIndex = data.Log.Count;
				while (logIndex > LogDetailsDisplayed)
				{
					string logText = data.Log[LogDetailsDisplayed];
					LogDetailsDisplayed++;
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
				}

				Application.DoEvents();
			}
		}

		void SetButtons(bool isEnabled)
		{
			PathTextBox.Enabled = !isEnabled;
			BrowseFileButton.Enabled = !isEnabled;
			StartConversionButton.Enabled = !isEnabled;
			DataSourceGroupBox.Enabled = !isEnabled;
			TableToImportGroupBox.Enabled = !isEnabled;
			TreatRecsThatExistGroupBox.Enabled = !isEnabled;
		}

		void AConverter_OnError(object sender, EventArgs e)
		{
			OutputListBox.Items.Add(sender.ToString());
			LogDetailsDisplayed++;
		}

		void DisplayGeneralErrorMessage(string detailedExceptionMessage)
		{
			CloseButton.Enabled = true;
			CloseButton.Visible = true;
			Globals.Message.ShowError("Cannot import data! The detailed exception is:\n\n" + detailedExceptionMessage);
		}

		void BrowseFileButton_Click(object sender, EventArgs e)
		{
			if (ExcelRadioButton.Checked || InterbaseRadioButton.Checked)
			{
				BrowseFile();
			}
			else
			{
				BrowseDirectory();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Setting initial/default directory in file dialog form")]
		void BrowseFile()
		{
			OpenFileDialog.InitialDirectory = "c:\\"; // Setting initial/default directory in file dialog form
			OpenFileDialog.Filter = "TXT files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			OpenFileDialog.FilterIndex = 2;
			OpenFileDialog.RestoreDirectory = true;

			if (OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				PathTextBox.Text = OpenFileDialog.UnmappedFileName;
				PathTextBox.Focus();
				PathTextBox.SelectionStart = PathTextBox.Text.Length;
				PathTextBox.SelectionLength = 0;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Default directory for Dialog form")]
		void BrowseDirectory()
		{
			var folderDialog = new ZFolderBrowserDialog();
			folderDialog.SelectedPath = "C:\\"; // Default directory for Dialog form
			folderDialog.CreateDirectory = true;
			folderDialog.Description = "Browse/Select Data Directory";
			DialogResult result = folderDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				PathTextBox.Text = folderDialog.UnmappedSelectedPath;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		int LogDetailsDisplayed;
		ProgressLogger Logger;
		Converter DataConverter;
		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		public static bool IsShown = false;

		#endregion

		void InterbaseRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			bool interbaseOnly = InterbaseRadioButton.Checked;
			ToCSVCheckBox.Enabled = interbaseOnly;
			ConnectionTextBox.Enabled = interbaseOnly;
			ConnectionButton.Enabled = interbaseOnly;
		}

		void ConnectionButton_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyImporter importer = new DummyImporter(factory, PathTextBox.Text);

			using (ConnectionForm connect = new ConnectionForm(importer))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(connect) == DialogResult.OK)
				{
					ConnectionTextBox.Text = importer.ConnectionText;
				}
			}
		}

		void IZForm.FormInitialSize()
		{
		}

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == ProductsRadioButton && previousControl == LimitedClassRadioButton;
		}

		#endregion
	}
}
