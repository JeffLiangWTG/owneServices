using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	#region SuppressResourceStringsCheckRegion

	public partial class AutoJobClosureDiagnosisForm : ZChildForm
	{
		public AutoJobClosureDiagnosisForm() : base()
		{
		}
		public AutoJobClosureDiagnosisForm(JCSDiagnosisMonitor monitor)
			: base(monitor)
		{
			InitializeComponent();
			MessageWriter = monitor.GetMessageWriter();
			numberOfJobsSelectedLabel.Text = Res.GetString("7319153a-ef95-4b05-b343-55666f571ae9", "{0} jobs are selected for diagnosis.", TraceMonitorBizo.JobCount);
		}

		void BtnCollect_Click(object sender, EventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (!TraceMonitorBizo.IsTracing)
				{
					TraceMonitorBizo.MessageWriter = MessageWriter;
					TraceMonitorBizo.IntitializeTraceSources();
					MessageStatusBarPanel.Text = Res.GetString("7807e65e-8bfd-4422-8b02-166349cf58e1", "Running diagnosis...");

					TraceMonitorBizo.QueueJobsForDiagnosisAndMonitor();

					diagnoseButton.Enabled = false;
					MessageStatusBarPanel.Text = Res.GetString("edf4f94e-822b-4c62-b83a-11f9b19f0cb0", "Diagnosis complete");
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		void clipboardButton_Click(object sender, EventArgs e)
		{
			var data = new DataObject();
			data.SetData(DataFormats.Text, TextBoxStackTrace.Text);
			if (!SafeClipboard.SetDataObject(data, true))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
			else
			{
				MessageStatusBarPanel.Text = Res.GetString("48b754c7-8c3e-45e5-a605-f2df00c81ab2", "Content is copied to the clipboard");
			}
		}

		void BtnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void TraceForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (TraceMonitorBizo.IsTracing)
			{
				TraceMonitorBizo.ClearTraceSources();
				TraceMonitorBizo.MessageWriter = null;
			}
		}

		IMessageWriter MessageWriter { get; }

		JCSDiagnosisMonitor TraceMonitorBizo => BusinessEntity as JCSDiagnosisMonitor;
	}
	#endregion
}
