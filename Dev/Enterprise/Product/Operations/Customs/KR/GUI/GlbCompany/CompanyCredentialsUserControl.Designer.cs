namespace Enterprise.Customs.KR.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UnipassCertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnipassCertificateDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnipassCertificateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// UnipassCertificateGroupBox
			// 
			this.UnipassCertificateGroupBox.CaptionResourceString =
				Enterprise.Customs.KR.GUI.Res.GetData("4076af8c-2796-44e7-9071-f835c222d205",
					"Certificate for Unipass");
			this.UnipassCertificateGroupBox.Controls.Add(this.UnipassCertificateDynamicLayoutPanel);
			this.UnipassCertificateGroupBox.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.UnipassCertificateGroupBox.Name = "UnipassCertificateGroupBox";
			this.UnipassCertificateGroupBox.Size =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 236, true);
			this.UnipassCertificateGroupBox.TabIndex = 0;
			this.UnipassCertificateGroupBox.TabStop = false;
			// 
			// UnipassCertificateDynamicLayoutPanel
			// 
			this.UnipassCertificateDynamicLayoutPanel.AllowDrop = true;
			this.UnipassCertificateDynamicLayoutPanel.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.UnipassCertificateDynamicLayoutPanel.Name = "UnipassCertificateDynamicLayoutPanel";
			this.UnipassCertificateDynamicLayoutPanel.Size =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 210, true);
			this.UnipassCertificateDynamicLayoutPanel.TabIndex = 0;
			// 
			// CompanyCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.UnipassCertificateGroupBox);
			this.Name = "CompanyCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 357, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnipassCertificateGroupBox.ResumeLayout(false);
			this.UnipassCertificateGroupBox.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox UnipassCertificateGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel UnipassCertificateDynamicLayoutPanel;
	}
}
