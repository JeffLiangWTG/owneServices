using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	public partial class ClientDeviceHeaderForm : ZTemplateForm
	{
		ZGroupBox zGroupBox1;
		ZTextBox zTextBox2;

		ZTextBox zTextBox3;
		ZTextBox zTextBox4;
		ZGuidFindBox zGuidFindBox2;
		ZDropEdit zDropEdit1;
		ZGroupBox zGroupBox2;
		ZLabel zLabel1;
		ZGrid zGrid2;
		ZGrid zGrid1;
		ZTextBox zTextBox5;
		ZCodeFindBox enterpriseCodeFindBox;
		ZCodeFindBox serverCodeFindBox;
		ZDropEdit deviceKindDropEdit;
		ZTextBox deviceIdentifierTextBox;
		ZGroupBox tcaRegistrationGroupBox;
		ZGrid rimRegistrationGrid;
		ZButton rimRegisterSchemeButton;
		ZButton rimCancelSchemeButton;
		ZTextBox zTextBox1;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo rimRegistrationSchemeCodeColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo rimRegistrationStartTimeColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo rimRegistrationEndTimeColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.deviceKindDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.enterpriseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.serverCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.deviceIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.tcaRegistrationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.rimRegisterSchemeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.rimRegistrationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.rimCancelSchemeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.deviceKindDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zGuidFindBox2.SuspendLayout();
			this.enterpriseCodeFindBox.SuspendLayout();
			this.serverCodeFindBox.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.zGrid2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.tcaRegistrationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rimRegistrationGrid)).BeginInit();
			this.rimRegistrationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 882, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.tcaRegistrationGroupBox);
			this.MainTabPage.Controls.Add(this.zGroupBox2);
			this.MainTabPage.Controls.Add(this.zGroupBox1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 855, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 855, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 855, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 882, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(817);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = ZClientEDI.Res.GetData("fe6bc5e0-40e6-47fa-af9d-4b11dc4df18d", "Device Details");
			this.zGroupBox1.Controls.Add(this.zTextBox5);
			this.zGroupBox1.Controls.Add(this.deviceKindDropEdit);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.zGuidFindBox2);
			this.zGroupBox1.Controls.Add(this.zTextBox4);
			this.zGroupBox1.Controls.Add(this.zTextBox3);
			this.zGroupBox1.Controls.Add(this.enterpriseCodeFindBox);
			this.zGroupBox1.Controls.Add(this.serverCodeFindBox);
			this.zGroupBox1.Controls.Add(this.deviceIdentifierTextBox);
			this.zGroupBox1.Controls.Add(this.zTextBox2);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 172, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "CDH_Identifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_Identifier)));
			this.zTextBox5.CaptionResourceString = null;
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 16, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.zTextBox5.TabIndex = 0;
			// 
			// deviceKindDropEdit
			// 
			this.deviceKindDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deviceKindDropEdit, "CDH_DeviceKind");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_DeviceKind)));
			this.deviceKindDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 42, true);
			this.deviceKindDropEdit.Name = "deviceKindDropEdit";
			this.deviceKindDropEdit.ShouldResizeByMaxLength = true;
			this.deviceKindDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.deviceKindDropEdit.TabIndex = 2;
			// 
			// enrolmentSchemeDropEdit
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "CDH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_Status)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 16, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "LicenceOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).LicenceOrgPK)));
			this.zGuidFindBox2.CaptionResourceString = ZClientEDI.Res.GetData("5cfbe46a-30a9-4f6b-8372-6c14818e25bb", "Customer");
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 120, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.zGuidFindBox2.TabIndex = 8;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "ClientParentTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).ClientParentTypeDescription)));
			this.zTextBox4.CaptionResourceString = null;
			this.zTextBox4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 146, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.zTextBox4.TabIndex = 9;
			// 
			// deviceLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "CDH_ClientParentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_ClientParentID)));
			this.zTextBox3.CaptionResourceString = null;
			this.zTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 146, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.zTextBox3.TabIndex = 10;
			// 
			// enterpriseCodeFindBox
			// 
			this.enterpriseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.enterpriseCodeFindBox, "CDH_EnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_EnterpriseCode)));
			this.enterpriseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 94, true);
			this.enterpriseCodeFindBox.Name = "enterpriseCodeFindBox";
			this.enterpriseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.enterpriseCodeFindBox.TabIndex = 6;
			// 
			// serverCodeFindBox
			// 
			this.serverCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serverCodeFindBox, "CDH_ServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_ServerCode)));
			this.serverCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 94, true);
			this.serverCodeFindBox.Name = "serverCodeFindBox";
			this.serverCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.serverCodeFindBox.TabIndex = 7;
			// 
			// deviceIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.deviceIdentifierTextBox, "CDH_DeviceIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_DeviceIdentifier)));
			this.deviceIdentifierTextBox.CaptionResourceString = null;
			this.deviceIdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.deviceIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 42, true);
			this.deviceIdentifierTextBox.Name = "deviceIdentifierTextBox";
			this.deviceIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.deviceIdentifierTextBox.TabIndex = 3;
			// 
			// VehicleIdentificationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "CDH_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_Description)));
			this.zTextBox2.CaptionResourceString = null;
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 68, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.zTextBox2.TabIndex = 5;
			// 
			// vehicleRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CDH_ModelID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).CDH_ModelID)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 68, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.zTextBox1.TabIndex = 4;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = ZClientEDI.Res.GetData("7f91aae2-24dc-4c5f-b79d-83d9927b4351", "Components");
			this.zGroupBox2.Controls.Add(this.zLabel1);
			this.zGroupBox2.Controls.Add(this.zGrid2);
			this.zGroupBox2.Controls.Add(this.zGrid1);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 181, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 473, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = ZClientEDI.Res.GetData("6e034b40-94c2-41e3-bf3d-4b4eae93cd0d", "Identification Details");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 232, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 13, true);
			this.zLabel1.TabIndex = 1;
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.zGrid2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid2, "Components.Identifiers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).Identifiers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponentIdentification)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).Identifiers)).SyncRoot)).CDD_IdentificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponentIdentification)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).Identifiers)).SyncRoot)).IdentificationTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponentIdentification)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).Identifiers)).SyncRoot)).CDD_Identifier)));
			this.zGrid2.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CDD_IdentificationType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "IdentificationTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "CDD_Identifier";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.zGrid2.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid2.GridId = "6bc27325-2fe2-4ee1-98ea-51ffa6dd8d75";
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid1";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 248, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 218, true);
			this.zGrid2.TabIndex = 2;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_ComponentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).ComponentTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_ModelIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_OH_Manufacturer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_BatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceComponent)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).Components)).SyncRoot)).CDC_OrderDateUtc)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "CDC_ComponentType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "ComponentTypeDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.ColumnName = "CDC_ModelIdentifier";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CDC_OH_Manufacturer";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CDC_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.ColumnName = "CDC_BatchNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "CDC_OrderDateUtc";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.GridId = "6bc27325-2fe2-4ee1-98ea-51ffa6dd8d75";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 19, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 207, true);
			this.zGrid1.TabIndex = 0;
			// 
			// tcaRegistrationGroupBox
			// 
			this.tcaRegistrationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.tcaRegistrationGroupBox.Controls.Add(this.rimCancelSchemeButton);
			this.tcaRegistrationGroupBox.Controls.Add(this.rimRegisterSchemeButton);
			this.tcaRegistrationGroupBox.Controls.Add(this.rimRegistrationGrid);
			this.tcaRegistrationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 660, true);
			this.tcaRegistrationGroupBox.Name = "tcaRegistrationGroupBox";
			this.tcaRegistrationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 168, true);
			this.tcaRegistrationGroupBox.TabIndex = 2;
			this.tcaRegistrationGroupBox.TabStop = false;
			this.tcaRegistrationGroupBox.Text = "TCA Registration Details";
			// 
			// rimRegisterSchemeButton
			// 
			this.rimRegisterSchemeButton.CaptionResourceString = ZClientEDI.Res.GetData("9869f6bc-9764-45ee-9e5c-fc878d27a3b7", "Register Scheme");
			this.rimRegisterSchemeButton.IsCaptionOverridden = true;
			this.rimRegisterSchemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 129, true);
			this.rimRegisterSchemeButton.Name = "rimRegisterSchemeButton";
			this.rimRegisterSchemeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.rimRegisterSchemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.rimRegisterSchemeButton.TabIndex = 1;
			this.rimRegisterSchemeButton.Text = "Register Scheme";
			this.rimRegisterSchemeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.rimRegisterSchemeButton.ToolTipCaption = null;
			this.rimRegisterSchemeButton.UseVisualStyleBackColor = true;
			this.rimRegisterSchemeButton.Click += new System.EventHandler(this.ClientTelRimRegistrationOpenRegistrationFormClick);
			// 
			// rimRegistrationGrid
			// 
			this.rimRegistrationGrid.AllowNavigation = false;
			this.rimRegistrationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.rimRegistrationGrid, "RimRegistrations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientTelRimRegistration)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)).SyncRoot)).TRR_EnrolmentScheme)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientTelRimRegistration)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)).SyncRoot)).TRR_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientTelRimRegistration)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)).SyncRoot)).TRR_EndTime)));
			this.rimRegistrationGrid.CaptionVisible = false;
			rimRegistrationSchemeCodeColumnStyleInfo.CaptionResourceString = ZClientEDI.Res.GetData("2e011321-2faa-4cf3-a0c4-60a38c1a671b", "Scheme Code");
			rimRegistrationSchemeCodeColumnStyleInfo.ColumnName = "TRR_EnrolmentScheme";
			rimRegistrationSchemeCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			rimRegistrationStartTimeColumnStyleInfo.CaptionResourceString = ZClientEDI.Res.GetData("50975928-82bf-4c31-be9d-47c2275ba7e3", "Start Time");
			rimRegistrationStartTimeColumnStyleInfo.ColumnName = "TRR_StartTime";
			rimRegistrationStartTimeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			rimRegistrationEndTimeColumnStyleInfo.CaptionResourceString = ZClientEDI.Res.GetData("ebe4076f-35d2-4110-a343-0e52b2c799d0", "End Time");
			rimRegistrationEndTimeColumnStyleInfo.ColumnName = "TRR_EndTime";
			rimRegistrationEndTimeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.rimRegistrationGrid.ColumnStyles.Add(rimRegistrationSchemeCodeColumnStyleInfo);
			this.rimRegistrationGrid.ColumnStyles.Add(rimRegistrationStartTimeColumnStyleInfo);
			this.rimRegistrationGrid.ColumnStyles.Add(rimRegistrationEndTimeColumnStyleInfo);
			this.rimRegistrationGrid.GridId = "18780c5e-8693-4c52-887a-de4ec9ad6379";
			this.rimRegistrationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rimRegistrationGrid.LayoutKey = "rimRegistrationGrid";
			this.rimRegistrationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 20, true);
			this.rimRegistrationGrid.Name = "rimRegistrationGrid";
			this.rimRegistrationGrid.ReadOnly = true;
			this.rimRegistrationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 102, true);
			this.rimRegistrationGrid.TabIndex = 0;
			// 
			// rimCancelSchemeButton
			// 
			this.rimCancelSchemeButton.IsCaptionOverridden = true;
			this.rimCancelSchemeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(741, 129, true);
			this.rimCancelSchemeButton.Name = "rimCancelSchemeButton";
			this.rimCancelSchemeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.rimCancelSchemeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.rimCancelSchemeButton.TabIndex = 2;
			this.rimCancelSchemeButton.Text = "Cancel Scheme";
			this.rimCancelSchemeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.rimCancelSchemeButton.ToolTipCaption = null;
			this.rimCancelSchemeButton.UseVisualStyleBackColor = true;
			this.rimCancelSchemeButton.Click += new System.EventHandler(this.ClientTelRimRegistrationCancelRegistrationClick);
			// 
			// ClientDeviceHeaderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 938, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader);
			this.DataSourceTypeName = "Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 725, true);
			this.Name = "ClientDeviceHeaderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.deviceKindDropEdit.ResumeLayout(true);
			this.deviceKindDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zGuidFindBox2.ResumeLayout(true);
			this.zGuidFindBox2.PerformLayout();
			this.enterpriseCodeFindBox.ResumeLayout(true);
			this.enterpriseCodeFindBox.PerformLayout();
			this.serverCodeFindBox.ResumeLayout(true);
			this.serverCodeFindBox.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.zGrid2.ResumeLayout(false);
			this.zGrid2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.tcaRegistrationGroupBox.ResumeLayout(false);
			this.tcaRegistrationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.rimRegistrationGrid)).EndInit();
			this.rimRegistrationGrid.ResumeLayout(false);
			this.rimRegistrationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void ClientTelRimRegistrationOpenRegistrationFormClick(object sender, System.EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ClientTelRimRegistrationOpenRegistrationForm((ClientDeviceHeader)BusinessEntity), this);
		}

		void ClientTelRimRegistrationCancelRegistrationClick(object sender, System.EventArgs e)
		{
			var processor = new RimEnrolmentRequestProcessorFactory()
				.GetProcessor(TimeSpan.FromMilliseconds(30000));
			if (!processor.TryProcessEnrolmentCancellation(rimRegistrationGrid.SelectedElements.Cast<ClientTelRimRegistration>(), DateTimeOffset.UtcNow, out var message))
			{
				Globals.Message.ShowError(Res.GetString("EEC1CCD6-DC38-45BE-BF54-4E9CD98DD638", "Registration Cancellation Failure: {0}", message));
			}
		}

		void SetTcaRegistrationVisibility(object sender, System.EventArgs e)
		{
			var device = (ClientDeviceHeader)BusinessEntity;
			tcaRegistrationGroupBox.Visible = device.IsNotTemplate && device.CDH_DeviceKind == GlbDeviceKindCodes.WTGEmbedded;
		}
	}
}
