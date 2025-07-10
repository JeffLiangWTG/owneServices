using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	partial class SetupForm : Form, ILogger
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		public SetupForm(string cwShared)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			engine = new RegenEngine();
			this.cwShared = cwShared;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
		public bool DoSetup(RegenFlags regenFlags, out string errorMsg)
		{
			ClearReportBox();
			return engine.DoSetup(this, regenFlags, cwShared, out errorMsg);
		}

		public void HideActionControls()
		{
			IsScriptOnlyCheckBox.Visible = false;
			CompileDbUpgraderCheckBox.Visible = false;
			IncrementMinorVersionCheckBox.Visible = false;
			UndoCheckOutUpgraderButton.Visible = false;
			SetupDatabaseUpgraderButton.Visible = false;

			ControlDpiScalingHelper.SetTop(ref CompletedTasksTextBox, 10, true);
			ControlDpiScalingHelper.SetHeight(ref CompletedTasksTextBox, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height) - 50, true);
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
				AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		void SetupDatabaseUpgraderButton_Click(object sender, EventArgs e)
		{
			var flags = RegenFlags.None;

			if (CompileDbUpgraderCheckBox.Checked)
			{
				flags |= RegenFlags.Compile;
			}

			if (IncrementMinorVersionCheckBox.Checked)
			{
				flags |= RegenFlags.MinorVersion;
			}

			DoSetup(flags, out _);
		}

		void UndoCheckOutUpgraderButton_Click(object sender, EventArgs e)
		{
			engine.DoUndoCheckOut(this);
		}

		void SchemaVersionTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			const int BackSpace = 8;
			if (e.KeyChar != BackSpace && !char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		void ILogger.LogLineRaw(string line)
		{
			CompletedTasksTextBox.AppendText(line + System.Environment.NewLine);
			CompletedTasksTextBox.ScrollToCaret();
			CompletedTasksTextBox.Refresh();
		}

		protected void ClearReportBox()
		{
			CompletedTasksTextBox.Text = "";
		}

		public string GetCompletedTasksText()
		{
			return CompletedTasksTextBox.Text;
		}

		readonly RegenEngine engine;
		readonly string cwShared;
	}
}
