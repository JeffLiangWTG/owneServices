using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgDebtorGroupCodeListEditContainer : ZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid OrgDebtorGroupCodeGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgDebtorGroupCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgDebtorGroupCodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.OrgDebtorGroupCodeListCollectionWrapper);
			// 
			// OrgDebtorGroupCodeGrid
			// 
			this.OrgDebtorGroupCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgDebtorGroupCodeGrid, "OrgDebtorGroupCodeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListCollectionWrapper)(null)).OrgDebtorGroupCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListCollectionWrapper)(null)).OrgDebtorGroupCodeList)).SyncRoot)).GroupGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListCollectionWrapper)(null)).OrgDebtorGroupCodeList)).SyncRoot)).OrgDebtorGroupCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgDebtorGroupCodeListCollectionWrapper)(null)).OrgDebtorGroupCodeList)).SyncRoot)).GroupDescription)));
			this.OrgDebtorGroupCodeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "OrgDebtorGroupCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgDebtorGroupCodeListEditContainer|c6b447f1-3509-48cc-8f46-d29e52cb8eaa", "Group Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GroupGuid";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgDebtorGroup;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgDebtorGroupCodeListEditContainer|b7a4483d-56b8-40c7-93ab-d9347b07316f", "Group Description");
			zTextBoxColumnStyleInfo1.ColumnName = "GroupDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.OrgDebtorGroupCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrgDebtorGroupCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgDebtorGroupCodeGrid.GridId = "aa3619b7-f6cb-4bda-a9a2-cf5ca37a43c2";
			this.OrgDebtorGroupCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgDebtorGroupCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgDebtorGroupCodeGrid.LayoutKey = "OrgDebtorGroupCodeGrid";
			this.OrgDebtorGroupCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgDebtorGroupCodeGrid.Name = "OrgDebtorGroupCodeGrid";
			this.OrgDebtorGroupCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.OrgDebtorGroupCodeGrid.TabIndex = 0;
			// 
			// OrgDebtorGroupCodeListEditContainer
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgDebtorGroupCodeGrid);
			this.Name = "OrgDebtorGroupCodeListEditContainer";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgDebtorGroupCodeGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
