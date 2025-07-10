using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class BulkConsolidationProgressForm : ZChildForm
	{
		public BulkConsolidationProgressForm(LVXBulkConsolidateProcessor processor)
			: base()
		{
			this.processor = processor;
			this.processor.SyncInvoke = this;
			this.processor.OnProgress += new BulkConsolidateProgressEventHandler(DataProcess_OnProgress);
			this.processor.OnNotify += DataProcess_OnNotify;
			this.processor.ProcessCompleted += DataProcess_Completed;
			InitializeComponent();
			CloseButton.Text = CloseButtonCaption;
		}
		protected readonly LVXBulkConsolidateProcessor processor;

		#region On Click

		void StartButton_Click(object sender, EventArgs e)
		{
			CloseButton.Text = CancelButtonCaption;
			StartButton.Enabled = false;
			new Thread(StartProcessThread).Start();
		}

		void DataImportForm_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (CloseButton.Text == CancelButtonCaption)
			{
				e.Cancel = true;
				DialogResult response = Globals.Message.Show(Res.GetString("BC8BDC1F-26EB-4D3B-8FE4-3119BDE59861", "The system is in the process of Bulk Consolidate.\r\nAre you sure you want to stop it? Processed declarations will be saved to database."),
					Res.GetString("C6D4CAC6-B68F-42D0-9BE4-BFA31B50C313", "Stop Bulk Consolidate"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
				if (response == DialogResult.Yes)
				{
					processor.Cancel();
				}
			}
		}
		#endregion

		#region Events
		void DataProcess_Completed(object sender, EventArgs e)
		{
			CloseButton.Text = CloseButtonCaption;
			processor.OnProgress -= new BulkConsolidateProgressEventHandler(DataProcess_OnProgress);
			processor.OnNotify -= DataProcess_OnNotify;
			processor.ProcessCompleted -= DataProcess_Completed;
		}

		void DataProcess_OnProgress(object sender, BulkConsolidateProgressEventArgs e)
		{
			this.MessageStatusBarPanel.Text = e.message;
			ProgressBar.Value = e.percentComplete;
		}

		void DataProcess_OnNotify(string message)
		{
			ClearLogIfRequired();
			ProgressTextBox.AppendText(message);
			ProgressTextBox.AppendText(System.Environment.NewLine);
			ProgressTextBox.ScrollToCaret();
		}

		void StartProcessThread()
		{
			using (Db.DisposableActionForDbConnection())
			{
				processor.StartProcessing();
			}
		}
		#endregion

		void ClearLogIfRequired()
		{
			if (ProgressTextBox.Lines.Length > 1000)
			{
				StringBuilder existingLog = new StringBuilder();
				for (int i = 500; i < ProgressTextBox.Lines.Length; i++)
				{
					existingLog.Append(ProgressTextBox.Lines[i] + System.Environment.NewLine);
				}
				ProgressTextBox.Text = existingLog.ToString();
			}
		}

		static string CancelButtonCaption
		{
			get { return Res.GetString("09a04969-851a-4dd1-8f80-9c485169742e", "Cancel"); }
		}

		static string CloseButtonCaption
		{
			get { return Res.GetString("9ecb9486-9c83-4518-980b-691f0bd0df5c", "Close"); }
		}
	}
}
