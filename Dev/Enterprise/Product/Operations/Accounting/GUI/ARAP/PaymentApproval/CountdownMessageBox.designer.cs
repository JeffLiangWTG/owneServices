using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Accounting.GUI
{
	public partial class CountdownMessageBox
	{
		#region Windows Form Designer generated code

		private ZLabel CountdownLabel;
		private CargoWise.Windows.UI.KProgressBar progressBar;

		void InitializeComponent()
		{
			this.components = new Container();
			this.progressBar = new CargoWise.Windows.UI.KProgressBar();
			this.CountdownLabel = new ZLabel();
			this.timer = new Timer(this.components);
			((ISupportInitialize)(this.PictureBox)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Button1
			// 
			this.Button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 217, true);
			// 
			// Button2
			// 
			this.Button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 217, true);
			// 
			// Button3
			// 
			this.Button3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 217, true);
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 178, true);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 23, true);
			this.progressBar.TabIndex = 6;
			// 
			// CountdownLabel
			// 
			this.CountdownLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.CountdownLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 152, true);
			this.CountdownLabel.Name = "CountdownLabel";
			this.CountdownLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 23, true);
			this.CountdownLabel.TabIndex = 7;
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new EventHandler(this.timer_Tick);
			// 
			// CountdownMessageBox
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 251, true);
			this.Controls.Add(this.CountdownLabel);
			this.Controls.Add(this.progressBar);
			this.Name = "CountdownMessageBox";
			this.Controls.SetChildIndex(this.progressBar, 0);
			this.Controls.SetChildIndex(this.CountdownLabel, 0);
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			((ISupportInitialize)(this.PictureBox)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
