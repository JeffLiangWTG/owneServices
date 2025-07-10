namespace Enterprise.Customs.EU.GUI
{
	partial class SummaryControl
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
			this.gDMBasicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.meursingCalculateResultPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.additionalCodesContainerPanel = new Enterprise.Customs.EU.GUI.AdditionalCodesContainerPanel();
			this.documentConditionsContainerPanel = new Enterprise.Customs.EU.GUI.DocumentConditionsContainerPanel();
			this.vatContainerPanel = new Enterprise.Customs.EU.GUI.VATContainerPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingCondition);
			// 
			// gDMBasicLayoutPanel
			// 
			this.gDMBasicLayoutPanel.AllowDrop = true;
			this.gDMBasicLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.gDMBasicLayoutPanel.AutoScroll = true;
			this.gDMBasicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gDMBasicLayoutPanel.Name = "gDMBasicLayoutPanel";
			this.gDMBasicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 220, true);
			this.gDMBasicLayoutPanel.TabIndex = 1;
			// 
			// meursingCalculateResultPanel
			// 
			this.meursingCalculateResultPanel.AllowDrop = true;
			this.meursingCalculateResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.meursingCalculateResultPanel.AutoScroll = true;
			this.meursingCalculateResultPanel.AutoSize = true;
			this.meursingCalculateResultPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 212, true);
			this.meursingCalculateResultPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.meursingCalculateResultPanel.Name = "meursingCalculateResultPanel";
			this.meursingCalculateResultPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(100, 0, 0, 0, true);
			this.meursingCalculateResultPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 30, true);
			this.meursingCalculateResultPanel.TabIndex = 2;
			// 
			// additionalCodesContainerPanel
			// 
			this.additionalCodesContainerPanel.AutoSize = true;
			this.additionalCodesContainerPanel.InSummary = true;
			this.additionalCodesContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.additionalCodesContainerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.additionalCodesContainerPanel.Name = "additionalCodesContainerPanel";
			this.additionalCodesContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 0, true);
			this.additionalCodesContainerPanel.TabIndex = 3;
			// 
			// documentConditionsContainerPanel
			// 
			this.documentConditionsContainerPanel.AutoSize = true;
			this.documentConditionsContainerPanel.InSummary = true;
			this.documentConditionsContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.documentConditionsContainerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.documentConditionsContainerPanel.Name = "documentConditionsContainerPanel";
			this.documentConditionsContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 0, true);
			this.documentConditionsContainerPanel.TabIndex = 4;
			// 
			// vatContainerPanel
			// 
			this.vatContainerPanel.AutoSize = true;
			this.vatContainerPanel.InSummary = true;
			this.vatContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.vatContainerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.vatContainerPanel.Name = "vatContainerPanel";
			this.vatContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 0, true);
			this.vatContainerPanel.TabIndex = 5;
			// 
			// SummaryControl
			// 
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.meursingCalculateResultPanel);
			this.Controls.Add(this.gDMBasicLayoutPanel);
			this.Controls.Add(this.additionalCodesContainerPanel);
			this.Controls.Add(this.documentConditionsContainerPanel);
			this.Controls.Add(this.vatContainerPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SummaryControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 30, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 473, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel gDMBasicLayoutPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel meursingCalculateResultPanel;
		private AdditionalCodesContainerPanel additionalCodesContainerPanel;
		private DocumentConditionsContainerPanel documentConditionsContainerPanel;
		private VATContainerPanel vatContainerPanel;
	}
}
