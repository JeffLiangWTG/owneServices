namespace Enterprise.Services.OperationalActions.Module
{
	partial class RunProgramActionMethodSettingsControl
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
			this.textBoxPath = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxArguments = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Module.OpenURLActionMethodSettings);
			// 
			// textBoxPath
			// 
			this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxPath, "Path");
			this.textBoxPath.CaptionResourceString = Enterprise.Services.OperationalActions.Module.Res.GetData("7a3beb0c-a3a1-42b8-aadc-74c727127ebb", "Path", "Program path");
			this.textBoxPath.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxPath, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxPath.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.textBoxPath.Name = "textBoxPath";
			this.textBoxPath.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.textBoxPath.TabIndex = 1;
			// 
			// textBoxArguments
			// 
			this.textBoxArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxArguments, "Arguments");
			this.textBoxArguments.CaptionResourceString = Enterprise.Services.OperationalActions.Module.Res.GetData("598ff88e-3286-4660-ab22-ff1918bf5f6f", "Arguments", "Program arguments");
			this.textBoxArguments.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxArguments, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxArguments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 70, true);
			this.textBoxArguments.Name = "textBoxArguments";
			this.textBoxArguments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.textBoxArguments.TabIndex = 2;
			// 
			// RunProgramActionMethodSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBoxPath);
			this.Controls.Add(this.textBoxArguments);
			this.Name = "RunProgramActionMethodSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox textBoxPath;
		private ZArchitecture.ZTextBox textBoxArguments;
	}
}
