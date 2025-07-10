using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class TaskPageControl : UserControl
	{
		public TaskPageControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Method used to print the output on the output panel (used to refresh the GUI)
		/// </summary>
		/// <param name="outputString"></param>
		protected void PrintOutput(string outputString)
		{
			OutputTextBox.AppendText(outputString + "\r\n");
			OutputTextBox.ScrollToCaret();
		}

		#region Thread Support

		/// <summary>
		/// Starts a backup/restore task in a separate thread
		/// </summary>
		protected void StartTaskOnASeparateThread(StartTaskDelegate startTask)
		{
			this.Parent.Parent.Enabled = false;
			OutputTextBox.Text = "";

			Thread taskThread = new Thread(new ThreadStart(startTask));
			taskThread.Start();
		}

		/// <summary>
		/// SHOULD NOT BE CALLED TO RUN ON THE TASK THREAD. USE INVOKE INSTEAD.
		/// </summary>
		protected void OnTaskThreadFinished()
		{
			this.Parent.Parent.Enabled = true;
		}

		#endregion

		#region Event Handlers

		protected void DbTools_OnTaskStarted(string message)
		{
			PrintOutput(message);
		}

		protected void DbTools_OnSubtaskStarted(string message, int stepNumber)
		{
			DbTools_OnShowInfoMessage(message);
		}

		protected void DbTools_OnTaskCompleted(string message)
		{
			PrintOutput(message);
			PrintOutput("============================ SUCCESSFUL ===============================");
		}

		protected void DbTools_OnTaskFailed(string message)
		{
			PrintOutput(message);
			PrintOutput("============================= FAILED ===============================");
		}

		protected void DbTools_OnShowInfoMessage(string message)
		{
			PrintOutput("\t" + message.Replace("\r\n", "\r\n\t"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		protected void DbTools_OnConfirmationPrompt(ConfirmationPromptArgs promptArgs)
		{
			DialogResult dialogResult = MessageBox.Show(promptArgs.PromptMessage, promptArgs.PromptTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
			promptArgs.Result = dialogResult == DialogResult.Yes ? ConfirmationPromptResult.Yes : ConfirmationPromptResult.No;
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!this.Enabled)
			{
				foreach (Control control in this.Controls)
				{
					if (control.Name != "OuputGroupBox")
					{
						control.Enabled = false;
						control.ForeColor = SystemColors.GrayText;
					}
				}
			}
			else
			{
				foreach (Control control in this.Controls)
				{
					control.Enabled = true;
					control.ForeColor = SystemColors.WindowText;
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.ParentForm.Close();
		}

		#endregion
	}
}
