using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAExportClassificationUserControl
	{
		void InitializeComponent()
		{
			this.tariffCodeFindBox = new TariffFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.tariffCodeFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 154, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			// 
			// LookupCodeTextBox
			// 
			this.LookupCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 22, true);
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 20, true);
			this.LookupCodeTextBox.TabIndex = 1;
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 112, true);
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CC_IsActiveCheckBox.TabIndex = 44;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 72, true);
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 32, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// LastAuditDateEdit
			// 
			this.LastAuditDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 110, true);
			this.LastAuditDateEdit.TabIndex = 42;
			// 
			// AuditStaffCodeFindBox
			// 
			this.AuditStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 110, true);
			this.AuditStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.AuditStaffCodeFindBox.TabIndex = 40;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusClassification);
			// 
			// TariffCodeFindBox
			// 
			this.tariffCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.tariffCodeFindBox, "CC_FormattedTariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CC_FormattedTariffNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CC_FormattedTariffNumTariffInfo);
			this.tariffCodeFindBox.BindToTariffPropertyInfo = "CC_FormattedTariffNumTariffInfo";
			this.tariffCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportClassificationUserControl|8f55e5fd-a473-4ebb-932e-a1a49a050fa1", "Exp. HS", "Export HS Code", "Export Classification Code", "");
			this.tariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 47, true);
			this.tariffCodeFindBox.Name = "TariffCodeFindBox";
			this.tariffCodeFindBox.PreBoundMaxLength = 10;
			this.tariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 20, true);
			this.tariffCodeFindBox.TabIndex = 3;
			// 
			// SchedBClassificationUserControl
			// 
			this.Name = "SchedBClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 154, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		TariffFindBox tariffCodeFindBox;
	}
}
