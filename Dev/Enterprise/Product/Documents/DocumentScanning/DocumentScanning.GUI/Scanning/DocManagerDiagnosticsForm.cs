using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Launch
{
	/// <summary>
	/// DocManagerDiagnosticsForm - this form is used for tracing events triggered
	/// by external sources (eg. Windows Sill Image Monitor, and Deliverance).
	/// The diagnostic form shows the parameters passed to Enterprise, and the action taken.  
	/// It does NOT use Odyssey infrastructure (e.g. OWinForm), because that infrastructure may
	/// not have been fully initialized at the time the form is used. 
	/// 
	/// The static RunDiagnosticsWindow() should be called to "run" the window.
	/// 
	/// </summary>
	public partial class DocManagerDiagnosticsForm : KForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>

		public DocManagerDiagnosticsForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			StandardParamsTextBox.Text = (NoResString)"-scanstart " + ProcessesHelp.MagicEnterpriseParam;
		}

		public StringCollection PassThroughParams;

		/// <summary>
		/// After this function returns, the application should exit as soon as practicable. 
		/// </summary>
		/// <param name="aPassThroughParams"></param>
		public static void RunDiagnosticsWindow(StringCollection aPassThroughParams)
		{
			DocManagerDiagnosticsForm aForm = new DocManagerDiagnosticsForm();
			aForm.PassThroughParams = aPassThroughParams;
			Application.Run(aForm);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded usage string")]
		const string UsageString = @"Usage : CargoWiseOne ServerName DatabaseName -OpenDocMaintain:[JobNo] -StiDevice:[DeviceGuid] -StiEvent:[EventGuid]";

		void StartForm_Load(object sender, EventArgs e)
		{
			OutputTextBox.Text = UsageString + (NoResString)"\r\nApplication : " + Application.ExecutablePath;
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < PassThroughParams.Count; i++)
			{
				sb.Append(PassThroughParams[i]);
				if (i < (PassThroughParams.Count - 1))
				{
					sb.Append(" ");
				}
			}

			this.PassThroughParamsTextBox.Text = sb.ToString();
		}

		protected override void WndProc(ref Message m)
		{
			string messageString;
			if (ProcessesHelp.IsScannerMessage(ref m, out messageString))
			{
				this.BeginInvoke(new OutputEventHandler(WriteNewMessage),
					new object[] { this, new OutputEventArgs(messageString) });
				return;
			}
			base.WndProc(ref m);
		}

		protected int NumEntries;

		protected void WriteNewMessage(object sender, OutputEventArgs e)
		{
			NumEntries++;
			if (OutputTextBox.Text.Length != 0)
			{
				OutputTextBox.Text += "\r\n\r\n";
			}

			OutputTextBox.Text += NumEntries.ToString() + ") " + e.MessageLine;
		}

		void PaperLoadedButton_Click(object sender, EventArgs e)
		{
			StartProgram(PassThroughParamsTextBox.Text + " " + StandardParamsTextBox.Text + " " + @"-StiDevice:{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}\0001 -StiEvent:{F5D8E1A0-CCA4-11D2-B118-00A0C93EE7E0}");
		}

		void ScanButton_Click(object sender, EventArgs e)
		{
			StartProgram(PassThroughParamsTextBox.Text + " " + StandardParamsTextBox.Text + " " + @"-StiDevice:{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}\0001 -StiEvent:{F5D8E2A0-CCA4-11D2-B118-00A0C93EE7E0}");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, launching Enterprise not opening a file or url")]
		void StartProgram(string @params)
		{
			string fullPath = Application.ExecutablePath;

			if (File.Exists(fullPath))
			{
				Process.Start(fullPath, @params);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "this code used for diagnostics only")]
		void OpenDocMaintainButton_Click(object sender, EventArgs e)
		{
			string jobNo = JobNoTextBox.Text.Trim();
			if (!string.IsNullOrEmpty(jobNo))
			{
				StartProgram(PassThroughParamsTextBox.Text + " " + StandardParamsTextBox.Text + " " + "-OpenDocMaintain:" + jobNo);
			}
			else
			{
				MessageBox.Show((NoResString)"Please select a job.");
			}
		}
	}

	public class OutputEventArgs : EventArgs
	{
		public OutputEventArgs(string messageLine)
			: base()
		{
			fMessageLine = messageLine;
		}

		readonly string fMessageLine;

		public string MessageLine
		{
			get { return fMessageLine; }
		}
	}

	public delegate void OutputEventHandler(object sender, OutputEventArgs e);
}
