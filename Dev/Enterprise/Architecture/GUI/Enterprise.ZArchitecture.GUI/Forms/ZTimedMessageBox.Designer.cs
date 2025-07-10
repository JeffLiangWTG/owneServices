using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZTimedMessageBox
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		private void InitializeComponent()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		{
			this.components = new System.ComponentModel.Container();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.countdownLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(TimerTick_Event);
			// 
			// CountdownLabel
			// 
			this.countdownLabel.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.countdownLabel.FontType = OFontTypes.Small;
			this.countdownLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 80, true);
			this.countdownLabel.Name = "CountdownLabel";
			this.countdownLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 23, true);
			this.countdownLabel.TabIndex = 6;
			// 
			// ZTimedMessageBox
			// 
			this.CaptionRenderingEnabled = true;
			this.Text = "ZTimedMessageBox";
			this.Height = this.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10);
			this.Controls.Add(countdownLabel);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected System.Windows.Forms.Timer timer;
		ZLabel countdownLabel;
	}
}
