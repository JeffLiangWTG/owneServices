using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TIP
{
	internal partial class OrgPartRelationRegistryControl : RegistryZUserControl
	{
		internal ZGrid zGrid1;

		void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)).OrgHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)).Organisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)).OrgName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)).RelationshipType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TIP.OrgPartRelationRegistryBusinessObject)(null)).RelationshipTypeList)));
			this.zGrid1.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Organisations";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Code";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrgHeaderPK";
			zTextBoxColumnStyleInfo1.Caption = "Name";
			zTextBoxColumnStyleInfo1.ColumnName = "OrgName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "RelationshipTypeList";
			zDropEditColumnStyleInfo1.Caption = "Relationship Type";
			zDropEditColumnStyleInfo1.ColumnName = "RelationshipType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.GridId = "71fa2257-4395-467f-ba17-eba94932e5b3";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 469, true);
			this.zGrid1.TabIndex = 0;
			// 
			// OrgPartRelationRegistryControl
			// 
			this.Controls.Add(this.zGrid1);
			this.Name = "OrgPartRelationRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 469, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
