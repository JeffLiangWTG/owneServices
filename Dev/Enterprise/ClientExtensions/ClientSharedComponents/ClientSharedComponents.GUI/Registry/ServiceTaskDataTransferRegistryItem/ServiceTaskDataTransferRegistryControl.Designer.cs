using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	partial class ServiceTaskDataTransferRegistryControl : DataTransferRegistryControl
	{
		void InitializeComponent()
		{
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 46, true);
			// 
			// zTextBox1
			// 
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 82, true);
			this.zTextBox1.Visible = false;
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 82, true);
			this.NextRunDateTimeEdit.Visible = false;
			// 
			// IntervalCalcEdit
			// 
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 108, true);
			this.IntervalCalcEdit.Visible = false;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 108, true);
			this.zDropEdit1.Visible = false;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 111, true);
			this.zLabel5.Visible = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 79, true);
			// 
			// ServiceTaskDataTransferRegistryControl
			// 
			this.Name = "ServiceTaskDataTransferRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 86, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
