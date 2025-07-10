namespace Enterprise.Customs.EU.GUI
{
	public partial class VATControl
	{
		private ZArchitecture.GUI.ZGroupBox VATControlGroupBox;

		private void InitializeComponent()
		{
			this.VATControlGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingVATCollection);
			// 
			// VATControlGroupBox
			// 
			this.VATControlGroupBox.AutoSize = true;
			this.VATControlGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.VATControlGroupBox.Name = "VATControlGroupBox";
			this.VATControlGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 10, true);
			this.VATControlGroupBox.TabIndex = 0;
			this.VATControlGroupBox.TabStop = false;
			// 
			// VATControl
			// 
			this.AutoSize = true;
			this.Controls.Add(this.VATControlGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.Name = "VATControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 10, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
