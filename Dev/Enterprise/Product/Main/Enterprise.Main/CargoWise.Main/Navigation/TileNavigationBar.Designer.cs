using CargoWise.GUI.TileBar;
using CargoWise.Windows.UI;
using System.ComponentModel;

namespace CargoWise.Main.Navigation
{
	partial class TileNavigationBar
	{
		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.HostControl = new CargoWise.Windows.UI.KElementHost();
			this.rightBorder = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// HostControl
			//
			this.HostControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HostControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HostControl.Name = "HostControl";
			this.HostControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 300, true);
			this.HostControl.TabIndex = 0;
			//
			// rightBorder
			//
			this.rightBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(87)))), ((int)(((byte)(90)))), ((int)(((byte)(93)))));
			this.rightBorder.Dock = System.Windows.Forms.DockStyle.Right;
			this.rightBorder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 0, true);
			this.rightBorder.Name = "rightBorder";
			this.rightBorder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 300, true);
			this.rightBorder.TabIndex = 1;
			//
			// TileNavigationBar
			//
			this.Controls.Add(this.HostControl);
			this.Controls.Add(this.rightBorder);
			this.Name = "TileNavigationBar";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		KPanel rightBorder;

		#endregion
	}
}
