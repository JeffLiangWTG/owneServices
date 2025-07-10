namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public partial class ReportsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.ReportsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReportsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).BeginInit();
			this.ReportsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.DE.ExitControl.Business.CusExitReport>);
			// 
			// ReportsGroupBox
			// 
			this.ReportsGroupBox.CaptionResourceString = Enterprise.Customs.DE.ExitControl.GUI.Res.GetData("363370b3-b6b1-4269-a41d-6d6666330ddf", "Exit Reports");
			this.ReportsGroupBox.Controls.Add(this.ReportsGrid);
			this.ReportsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsGroupBox.Name = "ReportsGroupBox";
			this.ReportsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 342, true);
			this.ReportsGroupBox.TabIndex = 0;
			this.ReportsGroupBox.TabStop = false;
			// 
			// ReportsGrid
			// 
			this.ReportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_TransportID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_RN_NKTransportNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_DateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_CXC_Consignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Header.CusExitConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_Calc_Discrepancies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).CER_IsFinalized)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Declarant.OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Declarant.Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Declarant.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Declarant.Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Representative.OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Representative.Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Representative.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitReport)(null)).Representative.Organisation.Addresses)));
			this.ReportsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CER_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zDropEditColumnStyleInfo2.ColumnName = "CER_TransportMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDropEditColumnStyleInfo3.ColumnName = "CER_TransportType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "CER_TransportID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CER_RN_NKTransportNationality";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo.ColumnName = "Location";
			zCodeFindBoxColumnStyleInfo.BindToList = "Lookups.LocationCollection";
			zCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "CER_DateTime";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CER_OfficeOfExit";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CER_OfficeOfExport";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "CER_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "CER_MessageStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zTextBoxColumnStyleInfo6.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.BindToList = "Header+CusExitConsignments";
			zGuidDropEditColumnStyleInfo1.ColumnName = "CER_CXC_Consignment";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168);
			zCheckBoxColumnStyleInfo1.ColumnName = "CER_Calc_Discrepancies";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "CER_IsFinalized";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Declarant+Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "Declarant+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.ExitControl.GUI.Res.GetData("D2AE02E9-DD31-4530-A8D7-BA6428406076", "Declarant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.BindToList = "Declarant+Organisation+Addresses";
			zGuidDropEditColumnStyleInfo2.ColumnName = "Declarant+E2_OA_Address";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.ExitControl.GUI.Res.GetData("DC9038CA-646E-474F-8C83-568AB7A754FB", "Declarant Address");
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Representative+Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "Representative+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.ExitControl.GUI.Res.GetData("3A5107D2-7A84-401F-95CD-08CA722699B2", "Representative");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo3.BindToList = "Representative+Organisation+Addresses";
			zGuidDropEditColumnStyleInfo3.ColumnName = "Representative+E2_OA_Address";
			zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.ExitControl.GUI.Res.GetData("7BEE700A-AF47-45D2-B32C-945A541CF6FF", "Representative Address");
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo);
			this.ReportsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.ReportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGrid.GridId = "5011f95c-1a2e-4438-a4c0-f861aa18403c";
			this.ReportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportsGrid.LayoutKey = "ReportsGrid";
			this.ReportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReportsGrid.Name = "ReportsGrid";
			this.ReportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 323, true);
			this.ReportsGrid.TabIndex = 0;
			// 
			// ReportsGridUserControl
			// 
			this.Controls.Add(this.ReportsGroupBox);
			this.Name = "ReportsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 342, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportsGroupBox.ResumeLayout(false);
			this.ReportsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).EndInit();
			this.ReportsGrid.ResumeLayout(false);
			this.ReportsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal ZArchitecture.GUI.ZGroupBox ReportsGroupBox;
		internal ZArchitecture.ZGrid ReportsGrid;
	}
}
