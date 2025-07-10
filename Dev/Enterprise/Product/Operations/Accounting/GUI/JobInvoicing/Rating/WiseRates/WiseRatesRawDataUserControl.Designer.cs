namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class WiseRatesRawDataUserControl
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
			this.WiseRatesRawDataButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// WiseRatesRawDataButton
			// 
			this.WiseRatesRawDataButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4556536A-236F-4CC1-B99E-B614FC5E8BE0", "Raw Data");
			this.WiseRatesRawDataButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.WiseRatesRawDataButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WiseRatesRawDataButton.Name = "WiseRatesDataButton";
			this.WiseRatesRawDataButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.WiseRatesRawDataButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
			this.WiseRatesRawDataButton.TabIndex = 0;
			this.WiseRatesRawDataButton.ToolTipCaption = null;
			this.WiseRatesRawDataButton.UseVisualStyleBackColor = true;
			this.WiseRatesRawDataButton.Click += new System.EventHandler(this.WiseRatesDataButton_Click);
			// 
			// WiseRatesDataUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WiseRatesRawDataButton);
			this.Name = "WiseRatesDataUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZButton WiseRatesRawDataButton;

	}
}
