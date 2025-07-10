using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AutomaticProcessRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		protected internal ZTextBox zTextBox1;
		protected internal ZDateEdit NextRunDateTimeEdit;
		protected internal ZCalcEdit IntervalCalcEdit;
		protected internal ZDropEdit zDropEdit1;
		protected internal ZLabel zLabel5;
		protected ZGroupBox zGroupBox1;

		void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NextRunDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DataTransferRegistryBusinessObject);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutomaticProcessRegistryControl|301cbbc1-7be7-4c6c-9aef-ba3d33647568", "Settings");
			this.zGroupBox1.Controls.Add(this.zLabel5);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.IntervalCalcEdit);
			this.zGroupBox1.Controls.Add(this.NextRunDateTimeEdit);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 121, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutomaticProcessRegistryControl|00dab868-8a77-4c4e-931c-c0d06307d181", "after that.");
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 88, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel5.TabIndex = 14;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "IntervalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).IntervalType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).IntervalTypeList)));
			this.zDropEdit1.BindToList = "IntervalTypeList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit1, false);
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 85, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zDropEdit1.TabIndex = 13;
			// 
			// IntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.IntervalCalcEdit, "Interval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).Interval)));
			this.IntervalCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutomaticProcessRegistryControl|9d2076dc-6bdf-4f28-a8ff-a0f40e29edf4", "And every");
			this.IntervalCalcEdit.Decimals = 0;
			this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 85, true);
			this.IntervalCalcEdit.Name = "IntervalCalcEdit";
			this.IntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 20, true);
			this.IntervalCalcEdit.TabIndex = 10;
			this.IntervalCalcEdit.Text = "0";
			this.IntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NextRunDateTimeEdit
			// 
			this.NextRunDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.NextRunDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NextRunDateTimeEdit, "NextRunDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).NextRunDateTime)));
			this.NextRunDateTimeEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutomaticProcessRegistryControl|8b47ef26-ff36-4fd4-841e-05082626b3b9", "Next Run");
			this.NextRunDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.NextRunDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 54, true);
			this.NextRunDateTimeEdit.Name = "NextRunDateTimeEdit";
			this.NextRunDateTimeEdit.TabIndex = 6;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "LastRunDateTimeAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DataTransferRegistryBusinessObject)(null)).LastRunDateTimeAsString)));
			this.zTextBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AutomaticProcessRegistryControl|91399826-62bf-4c66-8157-e8a3e4a1e970", "Last Run");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 19, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.zTextBox1.TabIndex = 4;
			// 
			// AutomaticProcessRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "AutomaticProcessRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
