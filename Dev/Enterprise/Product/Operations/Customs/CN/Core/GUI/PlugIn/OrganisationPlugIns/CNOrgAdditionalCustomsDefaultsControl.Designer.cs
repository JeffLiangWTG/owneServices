using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNOrgAdditionalCustomsDefaultsControl
	{
		void InitializeComponent()
		{
			this.CustomsProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LevyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManualNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsProcedureDropEdit.SuspendLayout();
			this.LevyTypeDropEdit.SuspendLayout();
			this.ManualNoTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CNOrgSupplierBuyerLinkAddInfo);
			// 
			// CustomsProcedureDropEdit
			// 
			this.CustomsProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsProcedureDropEdit, "ZO_ProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((object)(((Enterprise.Customs.CN.Business.CNOrgSupplierBuyerLinkAddInfo)(null)).ZO_ProcedureCode)));
			this.CustomsProcedureDropEdit.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("CNOrgAdditionalCustomsDefaultsControl|ZO_ProcedureCode",
					"Procedure Code");
			this.CustomsProcedureDropEdit.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 11, true);
			this.CustomsProcedureDropEdit.Name = "CustomsProcedureDropEdit";
			this.CustomsProcedureDropEdit.Size =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CustomsProcedureDropEdit.TabIndex = 0;
			// 
			// LevyTypeDropEdit
			// 
			this.LevyTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LevyTypeDropEdit, "ZO_LevyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((object)(((Enterprise.Customs.CN.Business.CNOrgSupplierBuyerLinkAddInfo)(null)).ZO_LevyType)));
			this.LevyTypeDropEdit.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("CNOrgAdditionalCustomsDefaultsControl|ZO_LevyType",
					"Levy Type");
			this.LevyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 40, true);
			this.LevyTypeDropEdit.Name = "LevyTypeDropEdit";
			this.LevyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.LevyTypeDropEdit.TabIndex = 1;
			// 
			// ManualNoTextBox
			// 
			this.ManualNoTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManualNoTextBox, "ZO_ManualNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((object)(((Enterprise.Customs.CN.Business.CNOrgSupplierBuyerLinkAddInfo)(null)).ZO_ManualNo)));
			this.ManualNoTextBox.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("CNOrgAdditionalCustomsDefaultsControl|ZO_ManualNo",
					"Manual No");
			this.ManualNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 67, true);
			this.ManualNoTextBox.Name = "ManualNoTextBox";
			this.ManualNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ManualNoTextBox.TabIndex = 2;
			// 
			// USOrgAdditionalCustomsDefaultsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsProcedureDropEdit);
			this.Controls.Add(this.LevyTypeDropEdit);
			this.Controls.Add(this.ManualNoTextBox);
			this.Name = "CNOrgAdditionalCustomsDefaultsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsProcedureDropEdit.ResumeLayout(true);
			this.CustomsProcedureDropEdit.PerformLayout();
			this.LevyTypeDropEdit.ResumeLayout(true);
			this.LevyTypeDropEdit.PerformLayout();
			this.ManualNoTextBox.ResumeLayout(true);
			this.ManualNoTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZDropEdit CustomsProcedureDropEdit;
		internal ZDropEdit LevyTypeDropEdit;
		internal ZArchitecture.ZTextBox ManualNoTextBox;
	}
}
