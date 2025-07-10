namespace Enterprise.Accounting.Module
{
	public partial class GlobalChargeCodeIntercompanyFilterControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_APChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_ARChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany)(null)).YG_HasLocalClientOverride)));
			zTextBoxColumnStyleInfo1.ColumnName = "YG_Code";
			zTextBoxColumnStyleInfo2.ColumnName = "YG_Desc";
			zCheckBoxColumnStyleInfo1.ColumnName = "YG_IsActive";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("GlobalChargeCodeIntercompanyFilterControl|412f9b65-d79b-4c45-839c-997affc27334", "AP Charge Code", "Payables Charge Code", "");
			zTextBoxColumnStyleInfo3.ColumnName = "YG_APChargeCode";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("GlobalChargeCodeIntercompanyFilterControl|23954571-79a9-416d-a9f2-e44fd6236d9e", "AR Charge Codes", "Receivables Charge Codes", "");
			zTextBoxColumnStyleInfo4.ColumnName = "YG_ARChargeCodes";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("GlobalChargeCodeIntercompanyFilterControl|49A936B9-23E3-47D5-99CF-469DDE16D5EE", "Local Client Override", "Has Job Local Client Overrides", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "YG_HasLocalClientOverride";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 94, true);
			this.grid.TabIndex = 3;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.GlobalChargeCode.GlobalChargeCodeMapIntercompany);
			//
			// GlobalChargeCodeIntercompanyFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "GlobalChargeCodeIntercompanyFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
