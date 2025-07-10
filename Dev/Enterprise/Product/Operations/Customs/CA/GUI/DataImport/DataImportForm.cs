using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business.DataImport;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class DataImportForm : ZChildForm
	{
		public DataImportForm()
			: this(null)
		{
		}

		public DataImportForm(NonPersistentBusinessObject businessObject)
			: base(businessObject)
		{
			InitializeComponent();
			DataImporter_OnProgress(0, 0, 0);
			CloseButton.GetExtension<ILabelCaptionRenderer>().Caption = CloseButtonCaption;
		}

		public DataImportForm(NonPersistentBusinessObject businessObject, DataImporter dataImporter)
			: this(businessObject)
		{
			this.dataImporter = dataImporter;
			this.dataImporter.SyncInvoke = this;
			this.dataImporter.OnImportStart += DataImporter_OnImportStart;
			this.dataImporter.OnShowNotification += DataImporter_OnShowMessage;
			this.dataImporter.OnProgress += DataImporter_OnProgress;
			this.dataImporter.OnImportFinished += DataImporter_OnImportFinished;
		}

		#region Events

		#region On Click

		void BrowseButton_Click(object sender, EventArgs e)
		{
			if (OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				FileNameTextBox.Text = OpenFileDialog.UnmappedFileName;
			}
		}

		void ImportButton_Click(object sender, EventArgs e)
		{
			var thread = new Thread(StartProcessThread);
			thread.Start();
		}

		protected virtual void StartProcessThread()
		{
			if (!string.IsNullOrEmpty(FileNameTextBox.Text))
			{
				using (var streamReader = new StreamReader(ZOpenFileDialog.OpenFile(FileNameTextBox.Text)))
				{
					StartProcessThread(streamReader);
				}
			}
		}

		protected virtual void StartProcessThread(StreamReader streamReader)
		{
		}

		#endregion

		#region DataImporter Events

		protected virtual void DataImporter_OnImportStart(string message)
		{
			DataImporter_OnShowMessage(message);
			CloseButton.GetExtension<ILabelCaptionRenderer>().Caption = CancelButtonCaption;

			ImportButton.Enabled = BrowseButton.Enabled = false;
		}

		void DataImporter_OnImportFinished(string message)
		{
			DataImporter_OnShowMessage(message);
			CloseButton.Enabled = true;
			CloseButton.GetExtension<ILabelCaptionRenderer>().Caption = CloseButtonCaption;
		}

		void DataImporter_OnShowMessage(string message)
		{
			OutputTextBox.AppendText(message);
		}

		void DataImporter_OnProgress(int linesProcessed, int invalidLines, int linesCount)
		{
			ProgressBar.Value = linesCount == 0 ? 0 : (int)((double)linesProcessed / linesCount * 100);
			ProgressLabel.GetExtension<ILabelCaptionRenderer>().Caption = ProgressBar.Value + "%";
			LinesProcessedLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("c75ae234-c60c-47f0-a653-d2f061d34690", "Lines Processed: {0}", linesProcessed);
			LinesInvalidLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("e8d4980a-e552-43cf-8832-0e1899048f51", "Invalid Lines: {0}", invalidLines);
			LinesCountLabel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("5e9d15e9-95ce-4a8a-8460-4a60f927b376", "Lines Count: {0}", linesCount);
		}

		#endregion

		void DataImportForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (CloseButton.Text == CancelButtonCaption)
			{
				e.Cancel = true;

				if (Globals.Message.Show(Res.GetString("dd02314e-28f9-44a7-bc1b-11d3cf762965", "Are you sure you want to cancel the operation?\r\nIt will be cause of not complete data in the database, but it can be done later."), Res.GetString("49af90e7-4314-4a29-9b85-dfe113d3ca4b", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					dataImporter.Cancel();
				}
			}
		}

		#endregion

		static string CancelButtonCaption
		{
			get { return Res.GetString("09a04969-851a-4dd1-8f80-9c485169742e", "Cancel"); }
		}

		static string CloseButtonCaption
		{
			get { return Res.GetString("9ecb9486-9c83-4518-980b-691f0bd0df5c", "Close"); }
		}

		protected readonly DataImporter dataImporter;
	}
}
