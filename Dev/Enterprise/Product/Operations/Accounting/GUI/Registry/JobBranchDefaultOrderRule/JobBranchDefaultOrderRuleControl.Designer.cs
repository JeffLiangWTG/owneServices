namespace Enterprise.Accounting.Registry.GUI
{
	partial class JobBranchDefaultOrderRuleControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.JobBranchDefaultOrderRuleGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.DefaultToBlankCalcEdit = new ZArchitecture.ZCalcEdit();
			this.DefaultToBlankLabel = new ZArchitecture.ZLabel();
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel = new ZArchitecture.ZLabel();
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit = new ZArchitecture.ZCalcEdit();
			this.DefaultToBranchOfOrganisationLabel = new ZArchitecture.ZLabel();
			this.DefaultToBranchOfOrganisationCalcEdit = new ZArchitecture.ZCalcEdit();
			this.DefaultToLoginUserDefaultLabel = new ZArchitecture.ZLabel();
			this.DefaultToLoginUserDefaultCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobBranchDefaultOrderRuleGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.JobBranchDefaultOrderRule);
			// 
			// JobBranchDefaultOrderRuleGroupBox
			// 
			this.JobBranchDefaultOrderRuleGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|e04762c5-5d2c-4721-b9ed-817ab03dba48", "Job Branch Default Order Rule");
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBlankCalcEdit);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBlankLabel);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBranchOfOrganisationLabel);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToBranchOfOrganisationCalcEdit);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToLoginUserDefaultLabel);
			this.JobBranchDefaultOrderRuleGroupBox.Controls.Add(this.DefaultToLoginUserDefaultCalcEdit);
			this.JobBranchDefaultOrderRuleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobBranchDefaultOrderRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobBranchDefaultOrderRuleGroupBox.Name = "JobBranchDefaultOrderRuleGroupBox";
			this.JobBranchDefaultOrderRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 152, true);
			this.JobBranchDefaultOrderRuleGroupBox.TabIndex = 0;
			this.JobBranchDefaultOrderRuleGroupBox.TabStop = false;
			// 
			// DefaultToBlankCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultToBlankCalcEdit, "DefaultToBlank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobBranchDefaultOrderRule)(null)).DefaultToBlank)));
			this.DefaultToBlankCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DefaultToBlankCalcEdit, false);
			this.DefaultToBlankCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 24, true);
			this.DefaultToBlankCalcEdit.Name = "DefaultToBlankCalcEdit";
			this.DefaultToBlankCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DefaultToBlankCalcEdit.TabIndex = 1;
			this.DefaultToBlankCalcEdit.Text = "0";
			this.DefaultToBlankCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DefaultToBlankLabel
			// 
			this.DefaultToBlankLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|ae44afa7-75cc-4ec3-bd7e-250414cee0db", "Default to Blank");
			this.DefaultToBlankLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.DefaultToBlankLabel.Name = "DefaultToBlankLabel";
			this.DefaultToBlankLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.DefaultToBlankLabel.TabIndex = 0;
			// 
			// DefaultToBranchRelatedToPortOrWarehouseBranchLabel
			// 
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|b68d0dc9-8249-4053-b521-aab60284ec68", "Default to Branch Related to Port / Warehouse Branch");
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel.Name = "DefaultToBranchRelatedToPortOrWarehouseBranchLabel";
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 23, true);
			this.DefaultToBranchRelatedToPortOrWarehouseBranchLabel.TabIndex = 0;
			// 
			// DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit, "DefaultToBranchRelatedToPortOrWarehouseBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobBranchDefaultOrderRule)(null)).DefaultToBranchRelatedToPortOrWarehouseBranch)));
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit, false);
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 56, true);
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.Name = "DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit";
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.TabIndex = 1;
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.Text = "0";
			this.DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DefaultToBranchOfOrganisationLabel
			// 
			this.DefaultToBranchOfOrganisationLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|7bc22241-d6d8-4e95-abcc-009cf4688780", "Default to Branch of Organization");
			this.DefaultToBranchOfOrganisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.DefaultToBranchOfOrganisationLabel.Name = "DefaultToBranchOfOrganisationLabel";
			this.DefaultToBranchOfOrganisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 23, true);
			this.DefaultToBranchOfOrganisationLabel.TabIndex = 0;
			// 
			// DefaultToBranchOfOrganisationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultToBranchOfOrganisationCalcEdit, "DefaultToBranchOfOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobBranchDefaultOrderRule)(null)).DefaultToBranchOfOrganisation)));
			this.DefaultToBranchOfOrganisationCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DefaultToBranchOfOrganisationCalcEdit, false);
			this.DefaultToBranchOfOrganisationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 88, true);
			this.DefaultToBranchOfOrganisationCalcEdit.Name = "DefaultToBranchOfOrganisationCalcEdit";
			this.DefaultToBranchOfOrganisationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DefaultToBranchOfOrganisationCalcEdit.TabIndex = 1;
			this.DefaultToBranchOfOrganisationCalcEdit.Text = "0";
			this.DefaultToBranchOfOrganisationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DefaultToLoginUserDefaultLabel
			// 
			this.DefaultToLoginUserDefaultLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobBranchDefaultOrderRuleControl|43917569-8c46-4708-a920-d83a9bf95269", "Default to Login User Default");
			this.DefaultToLoginUserDefaultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 120, true);
			this.DefaultToLoginUserDefaultLabel.Name = "DefaultToLoginUserDefaultLabel";
			this.DefaultToLoginUserDefaultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
			this.DefaultToLoginUserDefaultLabel.TabIndex = 0;
			// 
			// DefaultToLoginUserDefaultCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultToLoginUserDefaultCalcEdit, "DefaultToLoginUserDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.JobBranchDefaultOrderRule)(null)).DefaultToLoginUserDefault)));
			this.DefaultToLoginUserDefaultCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DefaultToLoginUserDefaultCalcEdit, false);
			this.DefaultToLoginUserDefaultCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 120, true);
			this.DefaultToLoginUserDefaultCalcEdit.Name = "DefaultToLoginUserDefaultCalcEdit";
			this.DefaultToLoginUserDefaultCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DefaultToLoginUserDefaultCalcEdit.TabIndex = 1;
			this.DefaultToLoginUserDefaultCalcEdit.Text = "0";
			this.DefaultToLoginUserDefaultCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobBranchDefaultOrderRuleControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobBranchDefaultOrderRuleGroupBox);
			this.Name = "JobBranchDefaultOrderRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobBranchDefaultOrderRuleGroupBox.ResumeLayout(false);
			this.JobBranchDefaultOrderRuleGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox JobBranchDefaultOrderRuleGroupBox;
		private ZArchitecture.ZLabel DefaultToBlankLabel;
		private ZArchitecture.ZCalcEdit DefaultToBlankCalcEdit;
		private ZArchitecture.ZCalcEdit DefaultToBranchRelatedToPortOrWarehouseBranchCalcEdit;
		private ZArchitecture.ZLabel DefaultToBranchOfOrganisationLabel;
		private ZArchitecture.ZCalcEdit DefaultToBranchOfOrganisationCalcEdit;
		private ZArchitecture.ZLabel DefaultToLoginUserDefaultLabel;
		private ZArchitecture.ZLabel DefaultToBranchRelatedToPortOrWarehouseBranchLabel;
		private ZArchitecture.ZCalcEdit DefaultToLoginUserDefaultCalcEdit;
	}
}
