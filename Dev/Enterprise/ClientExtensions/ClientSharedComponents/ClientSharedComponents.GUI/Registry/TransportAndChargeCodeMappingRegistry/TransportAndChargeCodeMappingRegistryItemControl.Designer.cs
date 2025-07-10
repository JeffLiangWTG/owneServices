using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class TransportAndChargeCodeMappingRegistryItemControl : RegistryZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid zGrid1;

		protected virtual void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).TransportModeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).TransportModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).ChargeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).NominalCostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ClientSharedComponents.Registry.TransportAndChargeCodeMappingRegistryBusinessObject)(null)).NominalRevenueCode)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "TransportModes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("TransportAndChargeCodeMappingRegistryItemControl|d51f8d3d-3e35-45fe-abf5-0bf2ba694c5c", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportModeCode";
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCodes";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("TransportAndChargeCodeMappingRegistryItemControl|d9c11e79-1960-4891-81f7-c91047056516", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("TransportAndChargeCodeMappingRegistryItemControl|a8965ac7-8b23-4d00-8ce1-6284a3c0cefb", "Nominal Cost Code");
			zTextBoxColumnStyleInfo1.ColumnName = "NominalCostCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ClientSharedComponents.GUI.Res.GetData("TransportAndChargeCodeMappingRegistryItemControl|725262b2-ed61-4ae1-b4ce-7049a02b2c37", "Nominal Revenue Code");
			zTextBoxColumnStyleInfo2.ColumnName = "NominalRevenueCode";
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.GridId = "0c6a9f5f-6826-46cb-8bee-abb643a789b3";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 424, true);
			this.zGrid1.TabIndex = 0;
			// 
			// TransportAndChargeCodeMappingRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGrid1);
			this.Name = "TransportAndChargeCodeMappingRegistryItemControl";
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
