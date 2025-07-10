using System;
using System.ComponentModel.Design.Serialization;
using System.Threading;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class SalesRestoreDbForm : Form
	{
		public SalesRestoreDbForm(string backupFileName)
		{
			InitializeComponent();
			InitialiseRestoreManager();
			this.Icon = BrandingFactory.Instance.ProductIcon;
			backupFileNameBuffer = backupFileName;
		}

#if DEBUG
		public SalesRestoreDbForm()
		{
			InitializeComponent();
			InitialiseRestoreManager();
		}
#endif // DEBUG

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		void InitialiseRestoreManager()
		{
			restoreManager = new SalesRestoreDbManager();
			restoreManager.SyncInvoke = this;
			restoreManager.OnTaskStarted += new InformationEvent(DbTools_OnTaskStarted);
			restoreManager.OnSubtaskStarted += new ProgressEvent(DbTools_OnSubtaskStarted);
			restoreManager.OnTaskCompleted += new InformationEvent(DbTools_OnTaskCompleted);
			restoreManager.OnTaskFailed += new InformationEvent(DbTools_OnTaskFailed);
			restoreManager.OnShowOpenFileDialog = SalesRestoreDbManager_OnShowOpenFileDialog;
		}

		void RestoreDatabases()
		{
			Thread thread = new Thread(new ThreadStart(OnRestoreDatabaseTaskThreadStart));
			thread.Start();
		}

		void OnRestoreDatabaseTaskThreadStart()
		{
			restoreManager.RestoreDatabase(backupFileNameBuffer);
		}

		#region Event Handlers

		void SalesRestoreDbForm_Load(object sender, EventArgs e)
		{
			RestoreDatabases();
		}

		void SalesRestoreDbForm_Shown(object sender, EventArgs e)
		{
			if (firstShow)
			{
				this.Hide();
				firstShow = false;
			}
		}

		bool firstShow = true;

		void hideButton_Click(object sender, EventArgs e)
		{
			if (hideButton.Text == "Hide")
			{
				this.Hide();
			}
			else
			{
				this.Close();
			}
		}

		void SalesRestoreDbForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing && hideButton.Text == "Hide")
			{
				e.Cancel = true;
			}
		}

		void SalesRestoreDbForm_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
			}
			else
			{
				lastState = this.WindowState;
			}
		}

		void ShowForm()
		{
			this.Show();
			this.WindowState = lastState;
		}

		void restoreNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.Visible)
			{
				this.Hide();
			}
			else
			{
				ShowForm();
			}
		}

		void DbTools_OnTaskStarted(string message)
		{
			PrintOutput(message);
		}

		void DbTools_OnSubtaskStarted(string message, int stepNumber)
		{
			PrintOutput("\t" + message.Replace("\r\n", "\r\n\t"));
		}

		void DbTools_OnTaskCompleted(string message)
		{
			PrintOutput(message);
			PrintOutput("============================ SUCCESSFUL ===============================");
			hideButton.Text = "Close";
			ShowForm();
		}

		void DbTools_OnTaskFailed(string message)
		{
			PrintOutput(message);
			PrintOutput("============================= FAILED ===============================");
			hideButton.Text = "Close";
			ShowForm();
		}

		void PrintOutput(string outputString)
		{
			outputTextBox.AppendText(outputString + "\r\n");
			outputTextBox.ScrollToCaret();
		}

		string SalesRestoreDbManager_OnShowOpenFileDialog(string serverName, string defaultBackupFolderLocation)
		{
			string fileName = string.Empty;

			this.Show();
			DbServerBrowser.BrowseResult browseResult = new DbServerBrowser.BrowseResult();
			browseResult.FullPath = defaultBackupFolderLocation;
			DbServerBrowseForm browseForm = new DbServerBrowseForm(browseResult, serverName, false);

			if (browseForm.ShowDialog() == DialogResult.OK)
			{
				fileName = browseResult.FullPath;
			}
			this.Hide();

			return fileName;
		}

		#endregion

		SalesRestoreDbManager restoreManager;
		readonly string backupFileNameBuffer;
		FormWindowState lastState = FormWindowState.Normal;
	}
}
