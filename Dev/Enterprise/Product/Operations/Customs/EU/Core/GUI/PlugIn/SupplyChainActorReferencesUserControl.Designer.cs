using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class SupplyChainActorReferencesUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.SupplyChainActorReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupplyChainActorReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplyChainActorReferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorReferencesGrid)).BeginInit();
			this.SupplyChainActorReferencesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>);
			// 
			// SupplyChainActorReferencesGroupBox
			// 
			this.SupplyChainActorReferencesGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F70EAFD7-2F2C-4648-A50F-B58B242973D2", "Additional Supply Chain Actor");
			this.SupplyChainActorReferencesGroupBox.Controls.Add(this.SupplyChainActorReferencesGrid);
			this.SupplyChainActorReferencesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorReferencesGroupBox.Name = "SupplyChainActorReferencesGroupBox";
			this.SupplyChainActorReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 283, true);
			this.SupplyChainActorReferencesGroupBox.TabIndex = 2;
			this.SupplyChainActorReferencesGroupBox.TabStop = false;
			// 
			// SupplyChainActorReferencesGrid
			// 
			this.SupplyChainActorReferencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).CFR_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).CFR_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).OwnerOrgPK)));
			this.SupplyChainActorReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OwnerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("B2B25D5E-1D75-45C4-AF74-BF31397D644C", "Owner Address");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.SupplyChainActorReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupplyChainActorReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupplyChainActorReferencesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.SupplyChainActorReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorReferencesGrid.GridId = "1A2B5A12-164C-46DD-9401-3D115A3142AE";
			this.SupplyChainActorReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupplyChainActorReferencesGrid.LayoutKey = "SupplyChainActorReferencesGrid";
			this.SupplyChainActorReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SupplyChainActorReferencesGrid.Name = "SupplyChainActorReferencesGrid";
			this.SupplyChainActorReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 264, true);
			this.SupplyChainActorReferencesGrid.TabIndex = 0;
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplyChainActorReferencesGroupBox);
			this.Name = "SupplyChainActorReferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplyChainActorReferencesGroupBox.ResumeLayout(false);
			this.SupplyChainActorReferencesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorReferencesGrid)).EndInit();
			this.SupplyChainActorReferencesGrid.ResumeLayout(false);
			this.SupplyChainActorReferencesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox SupplyChainActorReferencesGroupBox;
		Enterprise.ZArchitecture.ZGrid SupplyChainActorReferencesGrid;
	}
}
