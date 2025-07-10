namespace Enterprise.Services.OperationalActions.Module
{
	partial class OpenURLActionMethodSettingsControl
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
			this.textBoxURL = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Module.OpenURLActionMethodSettings);
			// 
			// textBoxURL
			// 
			this.textBoxURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxURL, "URL");
			this.textBoxURL.CaptionResourceString = Enterprise.Services.OperationalActions.Module.Res.GetData("44120394-0846-4864-bfd4-231b7c971c92", "URL", "URL to open");
			this.textBoxURL.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxURL, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.textBoxURL.Name = "textBoxURL";
			this.textBoxURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.textBoxURL.TabIndex = 1;
			// 
			// OpenURLActionMethodSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBoxURL);
			this.Name = "OpenURLActionMethodSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox textBoxURL;
	}
}
