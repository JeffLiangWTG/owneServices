using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	partial class ReportsGridUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReportsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReportsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).BeginInit();
			this.ReportsGrid.SuspendLayout();
			this.ReportsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.IE.ExitControl.Business.CusExitReport>);
			// 
			// ReportsGrid
			// 
			this.ReportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_AdditionalDeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_Discrepancies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_CXC_Consignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Header.CusExitConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_FormattedDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_FormattedDateTime_FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_TypeOfLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_UNLOCO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_TransportID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_RN_NKTransportNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).DeclarantOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Declarant.Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).DeclarantAddressPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Declarant.Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).RepresentativeOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Representative.Lookups.OrgHeader_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).RepresentativeAddressPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Representative.Organisation.Addresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_DeclarantType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_EnquiryInformationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).TypeDescription)));
			this.ReportsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CER_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CER_AdditionalDeclarationType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zCheckBoxColumnStyleInfo1.ColumnName = "CER_Calc_Discrepancies";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zGuidDropEditColumnStyleInfo1.BindToList = "Header+CusExitConsignments";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CER_CXC_Consignment";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CER_OfficeOfExport";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CER_OfficeOfExit";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CER_Calc_FormattedDateTime";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CER_Calc_FormattedDateTime_FieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CER_Calc_TypeOfLocation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CER_Calc_UNLOCO";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CER_Location";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CER_TransportMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "CER_TransportType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CER_TransportID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "CER_RN_NKTransportNationality";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Declarant+Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("2390a186-419c-4fcd-ba4f-5cad5b24ca82", "Declarant");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "DeclarantOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("0f63185c-0550-4c76-a2a2-73cf16b9d6cf", "Declarant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zGuidDropEditColumnStyleInfo2.BindToList = "Declarant+Organisation+Addresses";
			zGuidDropEditColumnStyleInfo2.Caption = "";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("f1facf16-9fc5-4102-990e-e39ae4ac94bd", "Declarant Address");
			zGuidDropEditColumnStyleInfo2.ColumnName = "DeclarantAddressPK";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("385fce2d-853e-4b31-adb0-8c8dace2b7a7", "Declarant");
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Representative+Lookups+OrgHeader_List";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "Representative";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "RepresentativeOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("1ba78cf1-e42e-433f-b9db-f3e7b49f94e6", "Representative");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zGuidDropEditColumnStyleInfo3.BindToList = "Representative+Organisation+Addresses";
			zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("4a1b5f98-23a9-4ccf-96d4-b830a529d3c4", "Representative Address");
			zGuidDropEditColumnStyleInfo3.ColumnName = "RepresentativeAddressPK";
			zGuidDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("1fd9932e-aece-43ff-bb86-62eec2b8b686", "Representative");
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "CER_DeclarantType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CER_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CER_MessageStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "CER_EnquiryInformationCode";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ReportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ReportsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ReportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ReportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ReportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGrid.GridId = "63A5A06E-DF49-41E2-9063-27E13A6DC95B";
			this.ReportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportsGrid.LayoutKey = "ReportsGrid";
			this.ReportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReportsGrid.Name = "ReportsGrid";
			this.ReportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1394, 88, true);
			this.ReportsGrid.TabIndex = 0;
			// 
			// ReportsGroupBox
			// 
			this.ReportsGroupBox.Controls.Add(this.BottomPanel);
			this.ReportsGroupBox.Controls.Add(this.ReportsGrid);
			this.ReportsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsGroupBox.Name = "ReportsGroupBox";
			this.ReportsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 185, true);
			this.ReportsGroupBox.TabIndex = 0;
			this.ReportsGroupBox.TabStop = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 104, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1394, 108, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// ReportsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportsGroupBox);
			this.Name = "ReportsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 185, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsGrid)).EndInit();
			this.ReportsGrid.ResumeLayout(false);
			this.ReportsGrid.PerformLayout();
			this.ReportsGroupBox.ResumeLayout(false);
			this.ReportsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ReportsGrid;
		internal ZArchitecture.GUI.ZGroupBox ReportsGroupBox;
		private ZPanel BottomPanel;
	}
}

