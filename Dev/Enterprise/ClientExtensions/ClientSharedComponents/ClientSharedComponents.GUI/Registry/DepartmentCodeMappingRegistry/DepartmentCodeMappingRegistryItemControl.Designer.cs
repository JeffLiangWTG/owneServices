using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class DepartmentCodeMappingRegistryItemControl : RegistryZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid zGrid1;

		protected virtual void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObject);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObject)(null)).CodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObject)(null)).DepartmentCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.DepartmentCodeMappingRegistryBusinessObject)(null)).ExternalCode)));
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "DepartmentCodes";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("DepartmentCodeMappingRegistryItemControl|f3950284-1898-466f-aa7a-78ff4bd33c09", "Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("DepartmentCodeMappingRegistryItemControl|a40c5040-af32-4196-922e-ee11203740a8", "External Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ExternalCode";
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.GridId = "f07d71f7-5266-4953-b60c-d725933cba48";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			// 
			// DepartmentCodeMappingRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "DepartmentCodeMappingRegistryItemControl";
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
