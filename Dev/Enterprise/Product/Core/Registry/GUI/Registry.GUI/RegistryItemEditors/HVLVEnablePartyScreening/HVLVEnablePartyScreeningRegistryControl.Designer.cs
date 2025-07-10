using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public partial class HVLVEnablePartyScreeningRegistryControl : RegistryZUserControl
	{
		ZCheckBox enableHVLVPartyScreeningCheckBox;
		ZCheckBox enableNewDPSResultFormCheckBox;

		void InitializeComponent()
		{
			this.enableHVLVPartyScreeningCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enableNewDPSResultFormCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HVLVEnablePartyScreening);
			// 
			// enableHVLVPartyScreeningCheckBox
			// 
			this.BindingSource.SetBindingMember(this.enableHVLVPartyScreeningCheckBox, "EnableHVLVPartyScreening");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVEnablePartyScreening)(null)).EnableHVLVPartyScreening)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.enableHVLVPartyScreeningCheckBox, false);
			this.enableHVLVPartyScreeningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 8, true);
			this.enableHVLVPartyScreeningCheckBox.Name = "enableHVLVPartyScreeningCheckBox";
			this.enableHVLVPartyScreeningCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.enableHVLVPartyScreeningCheckBox.TabIndex = 0;
			this.enableHVLVPartyScreeningCheckBox.Text = "Enable HVLV Party Screening";
			this.enableHVLVPartyScreeningCheckBox.UseVisualStyleBackColor = true;
			this.enableHVLVPartyScreeningCheckBox.CheckStateChanged += new System.EventHandler(this.EnableHVLVPartyScreeningCheckBox_CheckStateChanged);
			// 
			// enableNewDPSResultFormCheckBox
			// 
			this.BindingSource.SetBindingMember(this.enableNewDPSResultFormCheckBox, "EnableNewDPSResultForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVEnablePartyScreening)(null)).EnableNewDPSResultForm)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.enableNewDPSResultFormCheckBox, false);
			this.enableNewDPSResultFormCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 36, true);
			this.enableNewDPSResultFormCheckBox.Name = "enableNewDPSResultFormCheckBox";
			this.enableNewDPSResultFormCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.enableNewDPSResultFormCheckBox.TabIndex = 1;
			this.enableNewDPSResultFormCheckBox.Text = "Enable New DPS Result Form";
			this.enableNewDPSResultFormCheckBox.UseVisualStyleBackColor = true;
			// 
			// HVLVEnablePartyScreeningRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.enableHVLVPartyScreeningCheckBox);
			this.Controls.Add(this.enableNewDPSResultFormCheckBox);
			this.Name = "HVLVEnablePartyScreeningRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
