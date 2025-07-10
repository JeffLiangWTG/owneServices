namespace Enterprise.Security.ActiveDirectory.GUI
{
	partial class ADPasswordSettingsRegistryControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ADPasswordSettingsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.ADPasswordSettings);
			// 
			// ADPasswordSettingsButton
			// 
			this.ADPasswordSettingsButton.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("B8611F13-4CDA-478A-9AB7-3F402ED4DE96", "Override Domain Password Policy");
			this.ADPasswordSettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 19, true);
			this.ADPasswordSettingsButton.Name = "ADPasswordSettingsButton";
			this.ADPasswordSettingsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ADPasswordSettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 23, true);
			this.ADPasswordSettingsButton.TabIndex = 24;
			this.ADPasswordSettingsButton.ToolTipCaption = null;
			this.ADPasswordSettingsButton.Click += new System.EventHandler(this.ADPasswordSettingsButton_Click);
			// 
			// ADPasswordSettingsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ADPasswordSettingsButton);
			this.Name = "ADPasswordSettingsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton ADPasswordSettingsButton;
	}
}