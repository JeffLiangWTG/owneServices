namespace Enterprise.ComplianceRisk.GUI
{
	partial class ComplianceWorkflowInitiationRiskControl
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
			this.RiskReason = new Enterprise.ZArchitecture.ZLabel();
			this.RiskDescription = new Enterprise.ZArchitecture.ZLabel();
			this.RiskFactor = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// RiskReason
			// 
			this.RiskReason.BackColor = System.Drawing.SystemColors.Window;
			this.RiskReason.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RiskReason.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RiskReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 0, true);
			this.RiskReason.Name = "RiskReason";
			this.RiskReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 26, true);
			this.RiskReason.TabIndex = 9;
			// 
			// RiskDescription
			// 
			this.RiskDescription.BackColor = System.Drawing.SystemColors.Window;
			this.RiskDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RiskDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RiskDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 0, true);
			this.RiskDescription.Name = "RiskDescription";
			this.RiskDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 26, true);
			this.RiskDescription.TabIndex = 8;
			// 
			// RiskFactor
			// 
			this.RiskFactor.BackColor = System.Drawing.SystemColors.Window;
			this.RiskFactor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RiskFactor.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RiskFactor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.RiskFactor.Name = "RiskFactor";
			this.RiskFactor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 26, true);
			this.RiskFactor.TabIndex = 7;
			// 
			// ComplianceWorkflowInitiationRiskControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.RiskReason);
			this.Controls.Add(this.RiskDescription);
			this.Controls.Add(this.RiskFactor);
			this.Name = "ComplianceWorkflowInitiationRiskControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel RiskReason;
		private ZArchitecture.ZLabel RiskDescription;
		private ZArchitecture.ZLabel RiskFactor;
	}
}
