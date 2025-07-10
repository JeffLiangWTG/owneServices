using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.IO;

namespace Enterprise.Environment
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ConnectingForm : Form, IProcessStatus // This shouldnt sub-class anything because it is on another thread and it might hit the db
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		public ConnectingForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Icon = BrandingFactory.Instance.ProductIcon;
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

		public void UpdateStatus(string status, int progressValue)
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing )
		{
			if (disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing );
		}
	}
}
