using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class SaveProgressForm : Form, IProcessStatus, INoActivateModalForm // Can't use Z, run on another thread
	{
		public SaveProgressForm()
		{
			InitializeComponent();
			TopMost = true;
			ShowInTaskbar = false;
		}

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

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ProgressBar.Select(); // take focus away from text box so it doesn't have a cursor in it
		}

		public void UpdateStatus(string status, int percentComplete)
		{
			ProgressBar.Value = percentComplete;
			ProgressTextBox.Text = status;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
			e.Cancel = true;
		}
		#region ProgressTextBox

		// To avoid raise timer event during textbox mouse event which could throw ObjectDisposed exception. WI00204770
		internal class ProgressFormTextBox : KTextBox
		{
#if !WINZOR

			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case 0x0201: //WM_LBUTTONDOWN 
						{
							return;
						}
					case 0x0202: //WM_LBUTTONUP 
						{
							return;
						}
					case 0x0203: //WM_LBUTTONDBLCLK 
						{
							return;
						}
					case 0x0204: //WM_RBUTTONDOWN 
						{
							return;
						}
					case 0x0205: //WM_RBUTTONUP 
						{
							return;
						}
					case 0x0206: //WM_RBUTTONDBLCLK
						{
							return;
						}
				}
				base.WndProc(ref m);
			}

#endif
		}

		#endregion

	}
}
