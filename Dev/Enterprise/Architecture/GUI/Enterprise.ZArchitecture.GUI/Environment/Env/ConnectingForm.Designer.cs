namespace Enterprise.Environment
{
	public partial class ConnectingForm
	{
		ZArchitecture.GUI.ZPictureBox ConnectingPictureBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectingForm));
			this.ConnectingPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.ConnectingPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// ConnectingPictureBox
			// 
			this.ConnectingPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ConnectingPictureBox.Image")));
			this.ConnectingPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 19, true);
			this.ConnectingPictureBox.Name = "ConnectingPictureBox";
			this.ConnectingPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 32, true);
			this.ConnectingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ConnectingPictureBox.TabIndex = 0;
			this.ConnectingPictureBox.TabStop = false;
			// 
			// ConnectingForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 70, true);
			this.ControlBox = false;
			this.Controls.Add(this.ConnectingPictureBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ConnectingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = " Connecting to Database...";
			((System.ComponentModel.ISupportInitialize)(this.ConnectingPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
