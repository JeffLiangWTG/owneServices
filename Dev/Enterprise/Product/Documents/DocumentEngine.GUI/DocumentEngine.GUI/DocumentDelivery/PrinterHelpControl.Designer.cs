namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class PrinterHelpControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrinterHelpControl));
			this.zLinkLabel1 = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// zLinkLabel1
			// 
			this.zLinkLabel1.AutoSize = true;
			this.zLinkLabel1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrinterHelpControl|a3d92e87-b550-4ea0-b16e-fa9e6875db41", "Printer help available at My Account");
			this.zLinkLabel1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.zLinkLabel1.IsFontBold = false;
			this.zLinkLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 2, true);
			this.zLinkLabel1.Name = "zLinkLabel1";
			this.zLinkLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 18, true);
			this.zLinkLabel1.TabIndex = 16;
			this.zLinkLabel1.ForeColor = System.Drawing.Color.Blue;
			this.zLinkLabel1.Font = new System.Drawing.Font(this.zLinkLabel1.Font.FontFamily, this.zLinkLabel1.Font.Size, System.Drawing.FontStyle.Underline);
			this.zLinkLabel1.Click += new System.EventHandler(this.zLinkLabel1_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 16, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 15;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// PrinterHelpControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLinkLabel1);
			this.Controls.Add(this.pictureBox1);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			this.Name = "PrinterHelpControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZLinkLabel zLinkLabel1;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
	}
}
