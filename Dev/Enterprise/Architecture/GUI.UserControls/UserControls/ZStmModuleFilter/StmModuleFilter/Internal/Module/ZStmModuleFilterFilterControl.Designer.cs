using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmModuleFilterFilterControl
	{
		#region Component Designer generated code

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleName = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleFilterType = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleIsPublished = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleIsSystem = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleModule = new ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmModuleFilter)(null)).S9_FilterName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmModuleFilter)(null)).S9_FilterType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmModuleFilter)(null)).S9_IsPublished);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmModuleFilter)(null)).S9_IsSystem);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((StmModuleFilter)(null)).S9_ModuleID);
			zTextBoxColumnStyleName.ColumnName = "S9_FilterNameMultilingual";
			zTextBoxColumnStyleFilterType.ColumnName = "S9_FilterType";
			zCheckBoxColumnStyleIsPublished.ColumnName = "S9_IsPublished";
			zCheckBoxColumnStyleIsSystem.ColumnName = "S9_IsSystem";
			zTextBoxColumnStyleModule.ColumnName = "S9_ModuleID";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleName);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleFilterType);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleIsPublished);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleIsSystem);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleModule);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 264, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(StmModuleFilter);
			// 
			// ZStmModuleFilterFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ZStmModuleFilterFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
