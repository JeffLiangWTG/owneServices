using Enterprise.Registry.GUI;

namespace Enterprise.Client.DFD.Registry
{
	internal partial class AdditionalSettingsRegistryControl : DataTransferRegistryControl
	{
		Enterprise.ZArchitecture.ZTextBox ExportFileNameTextBox;

		void InitializeComponent()
		{
			this.ExportFileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGuidFindBox1.SuspendLayout();
			this.NextRunDateTimeEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 196, true);
			this.zGuidFindBox1.TabIndex = 8;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 93, true);
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 128, true);
			this.NextRunDateTimeEdit.TabIndex = 5;
			this.NextRunDateTimeEdit.Visible = false;
			// 
			// IntervalCalcEdit
			// 
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 159, true);
			this.IntervalCalcEdit.TabIndex = 6;
			this.IntervalCalcEdit.Visible = false;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 159, true);
			this.zDropEdit1.TabIndex = 7;
			this.zDropEdit1.Visible = false;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 162, true);
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.zLabel5.Visible = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.ExportFileNameTextBox);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 227, true);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectorySelectorButton, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.DirectoryTextBox, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zTextBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.NextRunDateTimeEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.IntervalCalcEdit, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zLabel5, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.zGuidFindBox1, 0);
			this.zGroupBox1.Controls.SetChildIndex(this.ExportFileNameTextBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.DFD.Registry.AdditionalSettingsRegistryBusinessObject);
			// 
			// ExportFileNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportFileNameTextBox, "ExportFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.DFD.Registry.AdditionalSettingsRegistryBusinessObject)(null)).ExportFileName)));
			this.ExportFileNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("AdditionalSettingsRegistryControl|9eaae753-5916-4fa9-9f5a-d010e513244f", "Export File Prefix");
			this.ExportFileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 67, true);
			this.ExportFileNameTextBox.Name = "ExportFileNameTextBox";
			this.ExportFileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.ExportFileNameTextBox.TabIndex = 3;
			this.ExportFileNameTextBox.Visible = false;
			// 
			// AdditionalSettingsRegistryControl
			// 
			this.Name = "AdditionalSettingsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 227, true);
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.NextRunDateTimeEdit.ResumeLayout(true);
			this.NextRunDateTimeEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
