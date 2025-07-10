using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class BranchDepartmentCodeMappingRegistryItemControl : RegistryZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid zGrid1;

		protected virtual void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).BranchCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).BranchCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).DepartmentCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).DepartmentCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).ProfitCentre)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.BranchDepartmentCodeMappingRegistryBusinessObject)(null)).NominalDepartment)));
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "BranchCodes";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("BranchDepartmentCodeMappingRegistryItemControl|b7c6fd1f-c110-4293-8447-869b36fe9934", "Branch Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BranchCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			zGuidFindBoxColumnStyleInfo2.BindToList = "DepartmentCodes";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("BranchDepartmentCodeMappingRegistryItemControl|db9a931e-6179-4b24-8ed4-f091ce1add52", "Department Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DepartmentCodePK";
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("BranchDepartmentCodeMappingRegistryItemControl|d425f857-44d3-482b-9e48-a0dafdfa319b", "Profit Center");
			zTextBoxColumnStyleInfo1.ColumnName = "ProfitCentre";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("BranchDepartmentCodeMappingRegistryItemControl|8f80a088-f095-42f8-af29-a26c793bfeee", "Nominal Department");
			zTextBoxColumnStyleInfo2.ColumnName = "NominalDepartment";
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.GridId = "8fdba044-844c-42ee-81a2-892e49b6a917";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			// 
			// BranchDepartmentCodeMappingRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "BranchDepartmentCodeMappingRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
