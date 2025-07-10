namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DeclarationSupplyChainActorsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SupplyChainActorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorsGrid)).BeginInit();
			this.SupplyChainActorsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// SupplyChainActorsGrid
			// 
			this.SupplyChainActorsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupplyChainActorsGrid, "CusSupplyChainActors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).CusSupplyChainActors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).CusSupplyChainActors)).SyncRoot)).CFR_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).CusSupplyChainActors)).SyncRoot)).OwnerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).CusSupplyChainActors)).SyncRoot)).CFR_Reference)));
			this.SupplyChainActorsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OwnerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SupplyChainActorsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupplyChainActorsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.SupplyChainActorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupplyChainActorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorsGrid.GridId = "ec2bc73a-5c1f-4cfe-a0e3-232ddf7a6da4";
			this.SupplyChainActorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupplyChainActorsGrid.LayoutKey = "zGrid1";
			this.SupplyChainActorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorsGrid.Name = "SupplyChainActorsGrid";
			this.SupplyChainActorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 142, true);
			this.SupplyChainActorsGrid.TabIndex = 0;
			// 
			// Phase5DeclarationSupplyChainActorsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplyChainActorsGrid);
			this.Name = "Phase5DeclarationSupplyChainActorsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 142, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorsGrid)).EndInit();
			this.SupplyChainActorsGrid.ResumeLayout(false);
			this.SupplyChainActorsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid SupplyChainActorsGrid;
	}
}
