using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	partial class ServiceTaskDataTransferHighWaterMarkRegistryControl : DataTransferRegistryControl
	{
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		Enterprise.ZArchitecture.ZLabel zLabel2;

		void InitializeComponent()
		{
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
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
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 134, true);
			this.zTextBox1.Visible = false;
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 134, true);
			this.NextRunDateTimeEdit.Visible = false;
			// 
			// IntervalCalcEdit
			// 
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 127, true);
			this.IntervalCalcEdit.Visible = false;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 127, true);
			this.zDropEdit1.Visible = false;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 111, true);
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.zLabel5.Visible = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zDateEdit1);
			this.zGroupBox1.Controls.Add(this.zLabel2);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 109, true);
			this.zGroupBox1.Controls.SetChildIndex(this.zLabel2, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zDateEdit1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zTextBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.IntervalCalcEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.NextRunDateTimeEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectorySelectorButton, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectoryTextBox, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zLabel5, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zGuidFindBox1, 0);
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "LastRunDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).LastRunDateTime)));
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDateEdit1, false);
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 75, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 21;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 73, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.zLabel2.TabIndex = 20;
			this.zLabel2.Text = "High Water Mark: ";
			// 
			// ServiceTaskDataTransferHighWaterMarkRegistryControl
			// 
			this.Name = "ServiceTaskDataTransferHighWaterMarkRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 114, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
