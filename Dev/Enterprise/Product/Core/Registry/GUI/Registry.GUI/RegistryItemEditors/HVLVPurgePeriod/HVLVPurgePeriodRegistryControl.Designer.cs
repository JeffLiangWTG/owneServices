using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class HVLVPurgePeriodRegistryControl : RegistryZUserControl
	{
		ZCheckBox enablePurgingCheckBox;
		ZCalcEdit purgePeriodCalcEdit;

		void InitializeComponent()
		{
			this.purgePeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.enablePurgingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HVLVPurgePeriod);
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.purgePeriodCalcEdit, "PurgePeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.HVLVPurgePeriod)(null)).PurgePeriod)));
			this.purgePeriodCalcEdit.CaptionResourceString = null;
			this.purgePeriodCalcEdit.DecimalPlaces = 0;
			this.purgePeriodCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.purgePeriodCalcEdit, false);
			this.purgePeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.purgePeriodCalcEdit.Name = "zCalcEdit1";
			this.purgePeriodCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.purgePeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 31, true);
			this.purgePeriodCalcEdit.TabIndex = 2;
			this.purgePeriodCalcEdit.Text = "0";
			this.purgePeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.enablePurgingCheckBox, "IsEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVPurgePeriod)(null)).IsEnabled)));
			this.enablePurgingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.enablePurgingCheckBox, false);
			this.enablePurgingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.enablePurgingCheckBox.Name = "zCheckBox1";
			this.enablePurgingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 24, true);
			this.enablePurgingCheckBox.TabIndex = 1;
			this.enablePurgingCheckBox.Text = "Enable HVLV Purging";
			this.enablePurgingCheckBox.UseVisualStyleBackColor = true;
			// 
			// HVLVPurgePeriodRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.enablePurgingCheckBox);
			this.Controls.Add(this.purgePeriodCalcEdit);
			this.Name = "HVLVPurgePeriodRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
