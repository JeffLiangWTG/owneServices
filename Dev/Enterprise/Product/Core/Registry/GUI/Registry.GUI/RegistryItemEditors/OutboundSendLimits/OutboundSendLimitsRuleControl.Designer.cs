using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	partial class OutboundSendLimitsRuleControl : RegistryBusinessObjectTemplateZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox OutboundSendLimitsGroupBox;
		Enterprise.ZArchitecture.ZLabel SendCountLimitLabel;
		Enterprise.ZArchitecture.ZCalcEdit SendCountLimitCalcEdit;
		Enterprise.ZArchitecture.ZLabel SendSizeLimitLabel;
		Enterprise.ZArchitecture.ZCalcEdit SendSizeLimitCalcEdit;

		void InitializeComponent()
		{
			this.OutboundSendLimitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendCountLimitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendCountLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendSizeLimitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendSizeLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OutboundSendLimitsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OutboundSendLimitsRule);
			// 
			// OutboundSendLimitsGroupBox
			// 
			this.OutboundSendLimitsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OutboundSendLimitsRuleControl|01BA9AFA-3E1F-4FA4-B9D5-19D11FBF02E7", "Outbound Send Limits");
			this.OutboundSendLimitsGroupBox.Controls.Add(this.SendCountLimitLabel);
			this.OutboundSendLimitsGroupBox.Controls.Add(this.SendCountLimitCalcEdit);
			this.OutboundSendLimitsGroupBox.Controls.Add(this.SendSizeLimitLabel);
			this.OutboundSendLimitsGroupBox.Controls.Add(this.SendSizeLimitCalcEdit);
			this.OutboundSendLimitsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutboundSendLimitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OutboundSendLimitsGroupBox.Name = "OutboundSendLimitsGroupBox";
			this.OutboundSendLimitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 100, true);
			this.OutboundSendLimitsGroupBox.TabIndex = 0;
			this.OutboundSendLimitsGroupBox.TabStop = false;
			// 
			// SendCountLimitLabel
			// 
			this.SendCountLimitLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OutboundSendLimitsRuleControl|9FDA9432-99C2-4EBE-A107-A7740025C9DD", "Count Limit (Num. of EDI Interchanges)");
			this.SendCountLimitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.SendCountLimitLabel.Name = "SendCountLimitLabel";
			this.SendCountLimitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SendCountLimitLabel.TabIndex = 1;
			// 
			// SendCountLimitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SendCountLimitCalcEdit, "SendCountLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OutboundSendLimitsRule)(null)).SendCountLimit)));
			this.SendCountLimitCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SendCountLimitCalcEdit, false);
			this.SendCountLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 24, true);
			this.SendCountLimitCalcEdit.Name = "SendCountLimitCalcEdit";
			this.SendCountLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.SendCountLimitCalcEdit.TabIndex = 2;
			this.SendCountLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SendSizeLimitLabel
			// 
			this.SendSizeLimitLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|E5C2FF0C-1F21-4176-8448-BBDAA219BC6E", "Size Limit (KB)");
			this.SendSizeLimitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.SendSizeLimitLabel.Name = "SendSizeLimitLabel";
			this.SendSizeLimitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SendSizeLimitLabel.TabIndex = 3;
			// 
			// SendSizeLimitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SendSizeLimitCalcEdit, "SendSizeLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OutboundSendLimitsRule)(null)).SendSizeLimit)));
			this.SendSizeLimitCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SendSizeLimitCalcEdit, false);
			this.SendSizeLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 56, true);
			this.SendSizeLimitCalcEdit.Name = "SendSizeLimitCalcEdit";
			this.SendSizeLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.SendSizeLimitCalcEdit.TabIndex = 4;
			this.SendSizeLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			// 
			// OutboundSendLimitsRuleControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OutboundSendLimitsGroupBox);
			this.Name = "OutboundSendLimitsRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OutboundSendLimitsGroupBox.ResumeLayout(false);
			this.OutboundSendLimitsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
