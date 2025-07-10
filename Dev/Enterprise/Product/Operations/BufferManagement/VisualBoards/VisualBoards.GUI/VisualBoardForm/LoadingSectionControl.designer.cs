namespace Enterprise.VisualBoards.GUI
{
	partial class LoadingSectionControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.loadingIndicatorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// loadingIndicatorLabel
			// 
			this.loadingIndicatorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.loadingIndicatorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.loadingIndicatorLabel.IsFontBold = true;
			this.loadingIndicatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.loadingIndicatorLabel.Name = "loadingIndicatorLabel";
			this.loadingIndicatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 37, true);
			this.loadingIndicatorLabel.TabIndex = 5;
			this.loadingIndicatorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LoadingSectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.loadingIndicatorLabel);
			this.DoubleBuffered = true;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 250, true);
			this.Name = "LoadingSectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 164, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel loadingIndicatorLabel;
	}
}
