using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JXCWarningMessageBox : ZMessageBox
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// JXCWarningMessageBox
			// 
			this.AutoSize = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 94, true);
			this.Name = "JXCWarningMessageBox";
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
