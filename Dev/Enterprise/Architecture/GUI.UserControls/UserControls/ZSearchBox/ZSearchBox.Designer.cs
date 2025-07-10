namespace Enterprise.ZArchitecture.GUI
{
	partial class ZSearchBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZSearchBox));
			this.InnerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearSearchPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ClearSearchPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// InnerTextBox
			// 
			this.InnerTextBox.AcceptsReturn = true;
			this.InnerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InnerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.InnerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InnerTextBox, false);
			this.InnerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.InnerTextBox.Name = "InnerTextBox";
			this.InnerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 13, true);
			this.InnerTextBox.TabIndex = 0;
			this.InnerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InnerTextBox_KeyDown);
			// 
			// ClearSearchPictureBox
			// 
			this.ClearSearchPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearSearchPictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ClearSearchPictureBox.BackgroundImage")));
			this.ClearSearchPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClearSearchPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 0, true);
			this.ClearSearchPictureBox.Name = "ClearSearchPictureBox";
			this.ClearSearchPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.ClearSearchPictureBox.TabIndex = 1;
			this.ClearSearchPictureBox.TabStop = false;
			this.ClearSearchPictureBox.Visible = false;
			this.ClearSearchPictureBox.Click += new System.EventHandler(this.ClearSearchPictureBox_Click);
			// 
			// ZSearchBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClearSearchPictureBox);
			this.Controls.Add(this.InnerTextBox);
			this.Name = "ZSearchBox";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ClearSearchPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZTextBox InnerTextBox;
		private Enterprise.ZArchitecture.GUI.ZPictureBox ClearSearchPictureBox;
	}
}
