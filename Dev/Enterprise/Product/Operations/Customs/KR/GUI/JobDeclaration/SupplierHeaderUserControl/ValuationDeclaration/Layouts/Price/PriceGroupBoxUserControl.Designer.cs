using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PriceGroupBoxUserControl
	{

		private void InitializeComponent()
		{
			this.BasisForCalculationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BasisForCalculationPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.AdditionalCostsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalCostsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DeductionCostsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeductionCostsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BasisForCalculationGroupBox.SuspendLayout();
			this.AdditionalCostsGroupBox.SuspendLayout();
			this.DeductionCostsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			// 
			// BasisForCalculationGroupBox
			// 
			this.BasisForCalculationGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6992f7be-c01d-45b6-9059-03479bc336af", "Basis for calculation");
			this.BasisForCalculationGroupBox.Controls.Add(this.BasisForCalculationPanel);
			this.BasisForCalculationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.BasisForCalculationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasisForCalculationGroupBox.Name = "BasisForCalculationGroupBox";
			this.BasisForCalculationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 70, true);
			this.BasisForCalculationGroupBox.TabIndex = 0;
			this.BasisForCalculationGroupBox.TabStop = false;
			// 
			// BasisForCalculationPanel
			// 
			this.BasisForCalculationPanel.AllowDrop = true;
			this.BasisForCalculationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BasisForCalculationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BasisForCalculationPanel.Name = "BasisForCalculationPanel";
			this.BasisForCalculationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 51, true);
			this.BasisForCalculationPanel.TabIndex = 0;
			// 
			// AdditionalCostsGroupBox
			// 
			this.AdditionalCostsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1b3f37a6-096f-4a33-8855-cac988e0493f", "Additional Costs");
			this.AdditionalCostsGroupBox.Controls.Add(this.AdditionalCostsPanel);
			this.AdditionalCostsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalCostsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
			this.AdditionalCostsGroupBox.Name = "AdditionalCostsGroupBox";
			this.AdditionalCostsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 193, true);
			this.AdditionalCostsGroupBox.TabIndex = 1;
			this.AdditionalCostsGroupBox.TabStop = false;
			// 
			// AdditionalCostsPanel
			// 
			this.AdditionalCostsPanel.AllowDrop = true;
			this.AdditionalCostsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalCostsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalCostsPanel.Name = "AdditionalCostsPanel";
			this.AdditionalCostsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 174, true);
			this.AdditionalCostsPanel.TabIndex = 0;
			// 
			// DeductionCostsGroupBox
			// 
			this.DeductionCostsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4663fc9f-19e4-475b-a362-5f68e0fa3b20", "Deduction Costs");
			this.DeductionCostsGroupBox.Controls.Add(this.DeductionCostsPanel);
			this.DeductionCostsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeductionCostsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.DeductionCostsGroupBox.Name = "DeductionCostsGroupBox";
			this.DeductionCostsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 91, true);
			this.DeductionCostsGroupBox.TabIndex = 2;
			this.DeductionCostsGroupBox.TabStop = false;
			// 
			// DeductionCostsPanel
			// 
			this.DeductionCostsPanel.AllowDrop = true;
			this.DeductionCostsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeductionCostsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeductionCostsPanel.Name = "DeductionCostsPanel";
			this.DeductionCostsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 72, true);
			this.DeductionCostsPanel.TabIndex = 0;
			// 
			// PriceGroupBoxUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeductionCostsGroupBox);
			this.Controls.Add(this.AdditionalCostsGroupBox);
			this.Controls.Add(this.BasisForCalculationGroupBox);
			this.Name = "PriceGroupBoxUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 354, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BasisForCalculationGroupBox.ResumeLayout(false);
			this.BasisForCalculationGroupBox.PerformLayout();
			this.AdditionalCostsGroupBox.ResumeLayout(false);
			this.AdditionalCostsGroupBox.PerformLayout();
			this.DeductionCostsGroupBox.ResumeLayout(false);
			this.DeductionCostsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZGroupBox BasisForCalculationGroupBox;
		private DynamicLayoutPanel BasisForCalculationPanel;
		private ZGroupBox AdditionalCostsGroupBox;
		private DynamicLayoutPanel AdditionalCostsPanel;
		private ZGroupBox DeductionCostsGroupBox;
		private DynamicLayoutPanel DeductionCostsPanel;
	}
}
