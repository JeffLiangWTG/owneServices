using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgHeaderCodeListEditContainer : ZUserControl
	{
		protected Enterprise.ZArchitecture.ZGrid OrgHeaderCodeGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgHeaderCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgHeaderCodeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.OrgHeaderCodeListCollectionWrapper);
			// 
			// OrgHeaderCodeGrid
			// 
			this.OrgHeaderCodeGrid.AllowNavigation = false;
			this.OrgHeaderCodeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgHeaderCodeGrid, "OrgHeaderCodeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgHeaderCodeListCollectionWrapper)(null)).OrgHeaderCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.OrgHeaderCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgHeaderCodeListCollectionWrapper)(null)).OrgHeaderCodeList)).SyncRoot)).ClientGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgHeaderCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgHeaderCodeListCollectionWrapper)(null)).OrgHeaderCodeList)).SyncRoot)).OrgHeaderCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.GUI.OrgHeaderCodeListElement)(((System.Collections.IList)(((Enterprise.Registry.GUI.OrgHeaderCodeListCollectionWrapper)(null)).OrgHeaderCodeList)).SyncRoot)).ClientName)));
			this.OrgHeaderCodeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "OrgHeaderCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgHeaderCodeListEditContainer|16a78573-11b9-468c-bc9f-20a8a4419b5d", "Client Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientGuid";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OrgHeaderCodeListEditContainer|2267b0c2-1885-4502-bb74-d12d438fb750", "Client Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.OrgHeaderCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrgHeaderCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgHeaderCodeGrid.GridId = "88fb09b4-d4e9-4dce-933c-4717106fc5e1";
			this.OrgHeaderCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgHeaderCodeGrid.LayoutKey = "zGrid1";
			this.OrgHeaderCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgHeaderCodeGrid.Name = "OrgHeaderCodeGrid";
			this.OrgHeaderCodeGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.OrgHeaderCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 144, true);
			this.OrgHeaderCodeGrid.TabIndex = 0;
			// 
			// OrgHeaderCodeListEditContainer
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgHeaderCodeGrid);
			this.Name = "OrgHeaderCodeListEditContainer";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgHeaderCodeGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
