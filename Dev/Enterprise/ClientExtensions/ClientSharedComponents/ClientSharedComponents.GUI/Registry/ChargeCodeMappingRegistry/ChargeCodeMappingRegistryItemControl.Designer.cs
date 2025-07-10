using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class ChargeCodeMappingRegistryItemControl : RegistryZUserControl
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
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObject);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObject)(null)).CodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObject)(null)).ChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.ChargeCodeMappingRegistryBusinessObject)(null)).ExternalCode)));
			this.zGrid1.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCodes";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("ChargeCodeMappingRegistryItemControl|033ccd92-6e45-41f3-bdcf-c27353aed73e", "Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("ChargeCodeMappingRegistryItemControl|0a7403b9-44ee-46b3-81f2-2fe747ab89ec", "External Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ExternalCode";
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.GridId = "48f7f1ce-0739-4dd7-9d76-0902e273818f";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			// 
			// ChargeCodeMappingRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "ChargeCodeMappingRegistryItemControl";
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
