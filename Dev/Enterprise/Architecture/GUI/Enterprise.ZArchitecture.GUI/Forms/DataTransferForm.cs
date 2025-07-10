using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class DataTransferForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter")]
		public DataTransferForm()
		{
			InitializeComponent();
			DialogFilter = "Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|XML files (*.xml)|*.xml|All files (*.*)|*.*";
			ExcludeLocalizationLabels();
		}

		public DataTransferForm(NonPersistentBusinessObject bO) : base(bO)
		{
			InitializeComponent();
			ExcludeLocalizationLabels();
		}

		public DataTransferForm(string dialogFilter, string formHeading)
		{
			InitializeComponent();
			this.DialogFilter = dialogFilter;
			this.fFormHeading = formHeading;
			ExcludeLocalizationLabels();
		}

		void ExcludeLocalizationLabels()
		{
#if DEBUG
			TypeDescriptor.AddAttributes(RowsProcessedLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(RowsIncludedLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(RowsExcludedLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public virtual void FinishProcess()
		{
			ProgressBar.Value = 100;
			ProcessButton.Enabled = false;
			CloseButton.Enabled = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void SetProcessProgress(int percentage, int processed, int failed, string logEntry)
		{
			if (percentage > 100)
			{
				percentage = 100;
			}

			ProgressBar.Value = percentage;

			RowsProcessedLabel.Text = processed.ToString();
			RowsIncludedLabel.Text = (processed - failed).ToString();
			RowsExcludedLabel.Text = failed.ToString();

			if (logEntry != null)
			{
				var logEntries = logEntry.Split('\n');
				for (var i = 0; i < logEntries.Length; i++)
				{
					if (logEntries[i].Length > 0)
					{
						LogListBox.Items.Add(logEntries[i]);
					}
				}
			}

			if (LogListBox.Items.Count > 0)
			{
				LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
			}

			Application.DoEvents();
		}

		public string DialogFilter;
		public event ProcessFileEventHandler StartProcess;
		public event EventHandler ProcessCancelled;

		#region Form Overrides

		protected string fFormHeading;
		public override string FormHeading
		{
			get { return fFormHeading; }
		}

		#endregion

		#region Implementation

		void BrowseButton_Click(object sender, EventArgs e)
		{
			SelectFile();
		}

		void SelectFile()
		{
			var selectFile = new ZOpenFileDialog();
			selectFile.Filter = DialogFilter;
			selectFile.Multiselect = false;
			selectFile.Title = Res.GetString("0a254ae4-b368-492b-9f18-905887ed56b2", "Choose the file that contains the data that you want to Import");

			if (selectFile.ShowDialog() == DialogResult.OK)
			{
				FileNameTextBox.Text = selectFile.UnmappedFileName;
			}
		}

		void ProcessButton_Click(object sender, EventArgs e)
		{
			if (ButtonIsCancel)
			{
				CancelProcessFile();
			}
			else if (!ProcessButtonClick())
			{
				CancelProcessFile();
			}
			else if (string.IsNullOrWhiteSpace(FileNameTextBox.Text))
			{
				Globals.Message.ShowInformation(Res.GetString("90569a6e-c8ca-4f84-a9f1-2cd392570e72", "Please select a file to import from."), Res.GetString("c9766438-1534-4ffe-b0d4-7c5b0182770e", "Import File"));
			}
			else if (!CargoWise.IO.PathValidation.IsValid(FileNameTextBox.Text))
			{
				Globals.Message.ShowInformation(Res.GetString("E0F5BEDF-5CDD-4E82-ACF4-FB87FA0BA376", "There are invalid characters in the file path."), Res.GetString("c9766438-1534-4ffe-b0d4-7c5b0182770e", "Import File"));
			}
			else
			{
				ProcessFile();
			}
		}

		protected virtual bool ProcessButtonClick()
		{
			return true;
		}

		void CancelProcessFile()
		{
			CloseButton.Enabled = true;
			if (ProcessCancelled != null)
			{
				ProcessCancelled(this, EventArgs.Empty);
			}
			else
			{
				ProcessButton.Enabled = false;
			}
		}

		void ProcessFile()
		{
			CloseButton.Enabled = false;
			FileNameTextBox.Enabled = false;
			BrowseButton.Enabled = false;
			ProcessButton.Text = Res.GetString("437c93b6-466e-412e-a16c-d21a3005809e", "Cancel");
			ButtonIsCancel = true;
			ProgressBar.Value = 0;

			if (StartProcess != null)
			{
					StartProcess(this, new ProcessFileEventArgs(FileNameTextBox.Text));
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		bool ButtonIsCancel;

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == FileNameTextBox && previousControl == BrowseButton;
		}

		#endregion
	}
}
