using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDeclarationDetailsUserControl
	{

		private void InitializeComponent()
		{
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ProvisionalPriceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProvisionalPricePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ProvisionalPricingReasonsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ProvisionalPricingReasonsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ProvisionalPriceGroupBox.SuspendLayout();
            this.ProvisionalPricingReasonsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("b82d464e-e1cf-44fc-b76d-736259895677", "Details");
			this.DetailsGroupBox.Controls.Add(this.DetailsPanel);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 121, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AllowDrop = true;
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 102, true);
			this.DetailsPanel.TabIndex = 0;
			// 
			// ProvisionalPriceGroupBox
			// 
			this.ProvisionalPriceGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("31480859-8e27-43d5-8466-0e576acd4487", "Provisional Price");
			this.ProvisionalPriceGroupBox.Controls.Add(this.ProvisionalPricePanel);
			this.ProvisionalPriceGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProvisionalPriceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 121, true);
			this.ProvisionalPriceGroupBox.Name = "ProvisionalPriceGroupBox";
			this.ProvisionalPriceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 71, true);
			this.ProvisionalPriceGroupBox.TabIndex = 2;
			this.ProvisionalPriceGroupBox.TabStop = false;
			// 
			// ProvisionalPricePanel
			// 
			this.ProvisionalPricePanel.AllowDrop = true;
			this.ProvisionalPricePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProvisionalPricePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ProvisionalPricePanel.Name = "ProvisionalPricePanel";
			this.ProvisionalPricePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1172, 52, true);
			this.ProvisionalPricePanel.TabIndex = 1;
			// 
			// ProvisionalPricingReasonsGroupBox
			// 
			this.ProvisionalPricingReasonsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("729eae4f-6758-40ed-9f74-794be4986ade", "Provisional Pricing Reasons");
            this.ProvisionalPricingReasonsGroupBox.Controls.Add(this.ProvisionalPricingReasonsPanel);
			this.ProvisionalPricingReasonsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProvisionalPricingReasonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.ProvisionalPricingReasonsGroupBox.Name = "ProvisionalPricingReasonsGroupBox";
			this.ProvisionalPricingReasonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 241, true);
			this.ProvisionalPricingReasonsGroupBox.TabIndex = 3;
			this.ProvisionalPricingReasonsGroupBox.TabStop = false;
			// 
            // ProvisionalPricingReasonsPanel
            // 
            this.ProvisionalPricingReasonsPanel.AllowDrop = true;
            this.ProvisionalPricingReasonsPanel.AutoScroll = true;
            this.ProvisionalPricingReasonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProvisionalPricingReasonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.ProvisionalPricingReasonsPanel.Name = "ProvisionalPricingReasonsPanel";
            this.ProvisionalPricingReasonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 225, true);
            this.ProvisionalPricingReasonsPanel.TabIndex = 1;
            // 
			// ValuationDeclarationDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProvisionalPricingReasonsGroupBox);
			this.Controls.Add(this.ProvisionalPriceGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "ValuationDeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 433, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ProvisionalPriceGroupBox.ResumeLayout(false);
			this.ProvisionalPriceGroupBox.PerformLayout();
            this.ProvisionalPricingReasonsGroupBox.ResumeLayout(false);
            this.ProvisionalPricingReasonsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZGroupBox DetailsGroupBox;
		private ZGroupBox ProvisionalPriceGroupBox;
		private ZGroupBox ProvisionalPricingReasonsGroupBox;
		private DynamicLayoutPanel DetailsPanel;
		private DynamicLayoutPanel ProvisionalPricePanel;
		private DynamicLayoutPanel ProvisionalPricingReasonsPanel;
	}
}
