using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI.Design;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	internal partial class ErrorLogProcessorForm : Form
	{
#if DEBUG
		public ErrorLogProcessorForm() { }
#endif // DEBUG

		public ErrorLogProcessorForm(InternalLogProcessorBase processor, string title)
		{
			Argument.NotNull(processor, "processor");
			this.processor = processor;

			InitializeComponent();

			this.Text = title;
		}

		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion

		public void RunProcessor()
		{
			processor.OnUpdateProgress += OnUpdateProgress;

			try
			{
				closeButton.Enabled = false;

				using (new ZWaitCursorChanger())
				{
					processor.Process();
				}
			}
			finally
			{
				processor.OnUpdateProgress -= OnUpdateProgress;
				closeButton.Enabled = true;
			}

			if (processor.ErrorMessage != null)
			{
				Globals.Message.ShowError(processor.ErrorMessage);
			}
			else
			{
				progressLabel.Text = "Process completed successfully\r\n\r\n" + processor.CompletedStatusMessage;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void OnUpdateProgress(string progressText)
		{
			progressLabel.Text = progressText;
			Application.DoEvents();
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		readonly InternalLogProcessorBase processor;
	}
}
