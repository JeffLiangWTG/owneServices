namespace Enterprise.ZArchitecture.GUI
{
	partial class SeparatorUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.SeparatorText = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SeparatorText
			// 
			this.SeparatorText.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SeparatorText.AutoSize = true;
			this.SeparatorText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SeparatorText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 1, true);
			this.SeparatorText.Name = "SeparatorText";
			this.SeparatorText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.SeparatorText.TabIndex = 1;
			this.SeparatorText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SeparatorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SeparatorText);
			this.Name = "SeparatorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.Load += new System.EventHandler(this.SeparatorUserControl_Load);
			this.Resize += new System.EventHandler(this.SeparatorUserControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZLabel SeparatorText;
	}
}
