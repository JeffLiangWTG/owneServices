namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationCSAUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccountingTimeOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsCSAApprovedImporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TradeChainPartnerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tradeChainPartnerGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.AccountingTimeOptionDropEdit.SuspendLayout();
			this.TradeChainPartnerGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeChainPartnerGrid)).BeginInit();
			this.tradeChainPartnerGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.AccountingTimeOptionDropEdit);
			this.DetailsGroupBox.Controls.Add(this.IsCSAApprovedImporterCheckBox);
			this.DetailsGroupBox.Controls.Add(this.TradeChainPartnerGroupBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 210, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// AccountingTimeOptionDropEdit
			// 
			this.AccountingTimeOptionDropEdit.AllowDrop = true;
			this.AccountingTimeOptionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AccountingTimeOptionDropEdit, "ZO_AccountingTimeOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_AccountingTimeOption)));
			this.AccountingTimeOptionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3f33c62a-db06-42af-bfc3-804e63f9e2fb", "Accounting Time Option");
			this.AccountingTimeOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 13, true);
			this.AccountingTimeOptionDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 20, true);
			this.AccountingTimeOptionDropEdit.Name = "AccountingTimeOptionDropEdit";
			this.AccountingTimeOptionDropEdit.PreBoundMaxLength = 3;
			this.AccountingTimeOptionDropEdit.ShouldResizeByMaxLength = true;
			this.AccountingTimeOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 20, true);
			this.AccountingTimeOptionDropEdit.TabIndex = 6;
			// 
			// IsCSAApprovedImporterCheckBox
			// 
			this.IsCSAApprovedImporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsCSAApprovedImporterCheckBox, "ZO_IsCSAApprovedImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsCSAApprovedImporter)));
			this.IsCSAApprovedImporterCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("eaa3485c-f40a-4053-8081-2f3b14e1b1c7", "CSA Approved Importer");
			this.IsCSAApprovedImporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCSAApprovedImporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.IsCSAApprovedImporterCheckBox.Name = "IsCSAApprovedImporterCheckBox";
			this.IsCSAApprovedImporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 17, true);
			this.IsCSAApprovedImporterCheckBox.TabIndex = 1;
			this.IsCSAApprovedImporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// TradeChainPartnerGroupBox
			// 
			this.TradeChainPartnerGroupBox.Controls.Add(this.tradeChainPartnerGrid);
			this.TradeChainPartnerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.TradeChainPartnerGroupBox.Name = "TradeChainPartnerGroupBox";
			this.TradeChainPartnerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 150, true);
			this.TradeChainPartnerGroupBox.TabIndex = 2;
			this.TradeChainPartnerGroupBox.TabStop = false;
			this.TradeChainPartnerGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8617b7ad-a13b-4fcb-bd48-8c4f28c194a2", "Trade Chain Partner");
			// 
			// tradeChainPartnerGrid
			// 
			this.tradeChainPartnerGrid.AllowNavigation = false;
			this.tradeChainPartnerGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.tradeChainPartnerGrid, "TradeChainPartners");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_Org)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).AddInfoLookups.ImportersList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CAOrgAddress.Lookups.SelectedOrganisationAddresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_CSAIDType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_CSAID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_CSAStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartner)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).TradeChainPartners)).SyncRoot)).CA_Action)));
			this.tradeChainPartnerGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "AddInfoLookups+ImportersList";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CA_Org";
			zOrganisationFindBoxColumnStyleInfo2.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zGuidDropEditColumnStyleInfo2.BindToList = "CAOrgAddress.Lookups+SelectedOrganisationAddresses";
			zGuidDropEditColumnStyleInfo2.ColumnName = "CA_Address";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			zDropEditColumnStyleInfo6.ColumnName = "CA_Type";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.ColumnName = "CA_CSAIDType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CA_CSAID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo8.ColumnName = "CA_CSAStatus";
			zDropEditColumnStyleInfo8.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DC3F729A-093E-42A4-BD94-17F3D8981DA5", "CSA Status");
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo9.ColumnName = "StatusDescription";
			zDropEditColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DC3F729A-093E-42A4-BD94-17F3D8981DA5", "CSA Status");
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo10.ColumnName = "CA_Action";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.tradeChainPartnerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.tradeChainPartnerGrid.GridId = "0783c248-7608-4d41-a859-1a012ff9f0dc";
			this.tradeChainPartnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tradeChainPartnerGrid.LayoutKey = "TradeChainPartnerGrid";
			this.tradeChainPartnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.tradeChainPartnerGrid.Name = "tradeChainPartnerGrid";
			this.tradeChainPartnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 130, true);
			this.tradeChainPartnerGrid.TabIndex = 3;
			// 
			// OrganisationCSAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "OrganisationCSAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.AccountingTimeOptionDropEdit.ResumeLayout(true);
			this.AccountingTimeOptionDropEdit.PerformLayout();
			this.TradeChainPartnerGroupBox.ResumeLayout(false);
			this.TradeChainPartnerGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tradeChainPartnerGrid)).EndInit();
			this.tradeChainPartnerGrid.ResumeLayout(false);
			this.tradeChainPartnerGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsCSAApprovedImporterCheckBox;
		private ZArchitecture.GUI.ZGroupBox TradeChainPartnerGroupBox;
		private ZArchitecture.ZGrid tradeChainPartnerGrid;
		private ZArchitecture.GUI.ZDropEdit AccountingTimeOptionDropEdit;
	}
}
