namespace Enterprise.Customs.CA.GUI
{
	partial class CusCAeMHMasterUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (masterBill != null)
				{
					masterBill.OnOverrideFreightDefaultsChanging -= new System.ComponentModel.CancelEventHandler(MasterBill_OnOverrideFreightDefaultsChanging);
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MasterSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverrideFreightDefaultsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ATADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestNoticeProcessingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestD4NoticeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmendmentReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargeSubLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustDischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MasterHouseCCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterHouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PlaceOfConsolidationDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsolidationDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PrimaryCCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.eMHMasterTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.eMHHouseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusCAeMHHouseUserControl = new Enterprise.Customs.CA.GUI.CusCAeMHHouseUserControl();
			this.eMHContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusCAeMHContainerUserControl = new Enterprise.Customs.CA.GUI.CusCAeMHContainerUserControl();
			this.ConsolMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Customs.CA.GUI.CAMessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MasterSplitContainer)).BeginInit();
			this.MasterSplitContainer.Panel1.SuspendLayout();
			this.MasterSplitContainer.Panel2.SuspendLayout();
			this.MasterSplitContainer.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ATADateEdit.SuspendLayout();
			this.LatestNoticeProcessingDateDateEdit.SuspendLayout();
			this.AmendmentReasonDropEdit.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.ETADateEdit.SuspendLayout();
			this.DischargePortCodeFindBox.SuspendLayout();
			this.DischargeSubLocationCodeFindBox.SuspendLayout();
			this.CustDischargePortCodeFindBox.SuspendLayout();
			this.CarrierCodeFindBox.SuspendLayout();
			this.PlaceOfConsolidationDocAddressControl.SuspendLayout();
			this.ConsolidationDocAddressControl.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.eMHMasterTabControl.SuspendLayout();
			this.eMHHouseTabPage.SuspendLayout();
			this.cusCAeMHHouseUserControl.SuspendLayout();
			this.eMHContainerTabPage.SuspendLayout();
			this.cusCAeMHContainerUserControl.SuspendLayout();
			this.ConsolMessagesTabPage.SuspendLayout();
			this.messagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusCAeMHMaster);
			// 
			// MasterSplitContainer
			// 
			this.MasterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MasterSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterSplitContainer.Name = "MasterSplitContainer";
			this.MasterSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MasterSplitContainer.Panel1
			// 
			this.MasterSplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			// 
			// MasterSplitContainer.Panel2
			// 
			this.MasterSplitContainer.Panel2.Controls.Add(this.eMHMasterTabControl);
			this.MasterSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 611, true);
			this.MasterSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(268);
			this.MasterSplitContainer.TabIndex = 0;
			this.MasterSplitContainer.TabStop = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.OverrideFreightDefaultsCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ATADateEdit);
			this.DetailsGroupBox.Controls.Add(this.LatestNoticeProcessingDateDateEdit);
			this.DetailsGroupBox.Controls.Add(this.LatestD4NoticeTextBox);
			this.DetailsGroupBox.Controls.Add(this.AmendmentReasonDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CustomsStatusDropEdit);
			this.DetailsGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ETADateEdit);
			this.DetailsGroupBox.Controls.Add(this.DischargePortCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.DischargeSubLocationCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.CustDischargePortCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.CarrierCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.MasterHouseCCNTextBox);
			this.DetailsGroupBox.Controls.Add(this.MasterHouseBillTextBox);
			this.DetailsGroupBox.Controls.Add(this.PlaceOfConsolidationDocAddressControl);
			this.DetailsGroupBox.Controls.Add(this.ConsolidationDocAddressControl);
			this.DetailsGroupBox.Controls.Add(this.PrimaryCCNTextBox);
			this.DetailsGroupBox.Controls.Add(this.MasterBillTextBox);
			this.DetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 259, true);
			this.DetailsGroupBox.TabIndex = 18;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("864D2B9C-1C5C-4B97-BE07-3054543C8409", "eManifest Details");
			// 
			// OverrideFreightDefaultsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideFreightDefaultsCheckBox, "BP_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_OverrideFreightDefaults)));
			this.OverrideFreightDefaultsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideFreightDefaultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 0, true);
			this.OverrideFreightDefaultsCheckBox.Name = "OverrideFreightDefaultsCheckBox";
			this.OverrideFreightDefaultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 22, true);
			this.OverrideFreightDefaultsCheckBox.TabIndex = 0;
			this.OverrideFreightDefaultsCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("FED838E4-4798-4C9B-A465-06974CB84515", "Override Default Values from Consol");
			this.OverrideFreightDefaultsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.OverrideFreightDefaultsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ATADateEdit
			// 
			this.ATADateEdit.AllowDrop = true;
			this.ATADateEdit.AutoCompleteMonthThreshold = 1;
			this.ATADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ATADateEdit, "BP_ATA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_ATA)));
			this.ATADateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fb51c038-d19a-4361-8dae-b070b748eec8", "ATA");
			this.ATADateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ATADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 229, true);
			this.ATADateEdit.Name = "ATADateEdit";
			this.ATADateEdit.TabIndex = 16;
			// 
			// LatestNoticeProcessingDateDateEdit
			// 
			this.LatestNoticeProcessingDateDateEdit.AllowDrop = true;
			this.LatestNoticeProcessingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestNoticeProcessingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestNoticeProcessingDateDateEdit, "BP_RNSProcessingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_RNSProcessingDate)));
			this.LatestNoticeProcessingDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4f74c28d-68c3-4bc2-81ec-20527705af14", "Processing Date");
			this.LatestNoticeProcessingDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestNoticeProcessingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 45, true);
			this.LatestNoticeProcessingDateDateEdit.Name = "LatestNoticeProcessingDateDateEdit";
			this.LatestNoticeProcessingDateDateEdit.TabIndex = 4;
			// 
			// LatestD4NoticeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LatestD4NoticeTextBox, "FormattedLatestD4MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).FormattedLatestD4MessageStatus)));
			this.LatestD4NoticeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e9d2da76-54b4-49ca-810e-a3e2727575ea", "Latest D4 Notice");
			this.LatestD4NoticeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LatestD4NoticeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 45, true);
			this.LatestD4NoticeTextBox.Name = "LatestD4NoticeTextBox";
			this.LatestD4NoticeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.LatestD4NoticeTextBox.TabIndex = 3;
			// 
			// AmendmentReasonDropEdit
			// 
			this.AmendmentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentReasonDropEdit, "BP_AmendReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_AmendReasonCode)));
			this.AmendmentReasonDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7372a686-1c7c-4613-b336-42ad6e3c32a1", "Amendment Reason");
			this.AmendmentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 229, true);
			this.AmendmentReasonDropEdit.Name = "AmendmentReasonDropEdit";
			this.AmendmentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 18, true);
			this.AmendmentReasonDropEdit.TabIndex = 15;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "BP_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_CustomsStatus)));
			this.CustomsStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a4aa947f-99b2-4e53-a548-004801249655", "Customs Status");
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 22, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.CustomsStatusDropEdit.TabIndex = 2;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "BP_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_MessageStatus)));
			this.MessageStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0e02b7a1-1c48-4a85-8bc1-16995d00d86f", "Message Status");
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 22, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.MessageStatusDropEdit.TabIndex = 1;
			this.MessageStatusDropEdit.TabStop = false;
			// 
			// ETADateEdit
			// 
			this.ETADateEdit.AllowDrop = true;
			this.ETADateEdit.AutoCompleteMonthThreshold = 1;
			this.ETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETADateEdit, "BP_ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_ETA)));
			this.ETADateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6edd2121-9f04-4e8d-8b8f-33c3b8e1a527", "ETA");
			this.ETADateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 137, true);
			this.ETADateEdit.Name = "ETADateEdit";
			this.ETADateEdit.TabIndex = 11;
			// 
			// DischargePortCodeFindBox
			// 
			this.DischargePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargePortCodeFindBox, "BP_RL_NKDiscPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_RL_NKDiscPort)));
			this.DischargePortCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e1591e6f-ce37-44ba-a7b8-b8c5ec565329", "Discharge Port");
			this.DischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 137, true);
			this.DischargePortCodeFindBox.Name = "DischargePortCodeFindBox";
			this.DischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 18, true);
			this.DischargePortCodeFindBox.TabIndex = 10;
			// 
			// DischargeSubLocationCodeFindBox
			// 
			this.DischargeSubLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeSubLocationCodeFindBox, "BP_CBSADischargeSubLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_CBSADischargeSubLocation)));
			this.DischargeSubLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("caa70dcd-f5dc-46f8-9876-1f5038a924bc", "Discharge Sub-Location");
			this.DischargeSubLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 206, true);
			this.DischargeSubLocationCodeFindBox.Name = "DischargeSubLocationCodeFindBox";
			this.DischargeSubLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 18, true);
			this.DischargeSubLocationCodeFindBox.TabIndex = 14;
			// 
			// CustDischargePortCodeFindBox
			// 
			this.CustDischargePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustDischargePortCodeFindBox, "BP_CBSADischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_CBSADischargePort)));
			this.CustDischargePortCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("25631747-208b-4118-8a07-2e7b6db3b7a3", "Cust. Port of Discharge");
			this.CustDischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 183, true);
			this.CustDischargePortCodeFindBox.Name = "CustDischargePortCodeFindBox";
			this.CustDischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 18, true);
			this.CustDischargePortCodeFindBox.TabIndex = 13;
			// 
			// CarrierCodeFindBox
			// 
			this.CarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeFindBox, "BP_CBSACarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_CBSACarrierCode)));
			this.CarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e3453a8a-5767-4620-8459-05ede646b83e", "FF Carrier Code");
			this.CarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 160, true);
			this.CarrierCodeFindBox.Name = "CarrierCodeFindBox";
			this.CarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 18, true);
			this.CarrierCodeFindBox.TabIndex = 12;
			// 
			// MasterHouseCCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterHouseCCNTextBox, "BP_MasterHouseCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_MasterHouseCCN)));
			this.MasterHouseCCNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a073c3c5-3992-457f-a0af-c6545e91a354", "Previous CCN", "Sub-Master (Previous) CCN");
			this.MasterHouseCCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 114, true);
			this.MasterHouseCCNTextBox.Name = "MasterHouseCCNTextBox";
			this.MasterHouseCCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.MasterHouseCCNTextBox.TabIndex = 9;
			// 
			// MasterHouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterHouseBillTextBox, "BP_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_MasterHouseBill)));
			this.MasterHouseBillTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f51b9f2b-3131-4ba7-acb6-e3dd762ce101", "Sub-Master Bill");
			this.MasterHouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 92, true);
			this.MasterHouseBillTextBox.Name = "MasterHouseBillTextBox";
			this.MasterHouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.MasterHouseBillTextBox.TabIndex = 7;
			// 
			// PlaceOfConsolidationDocAddressControl
			// 
			this.PlaceOfConsolidationDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfConsolidationDocAddressControl, "PlaceOfConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).PlaceOfConsolidation)));
			this.PlaceOfConsolidationDocAddressControl.BindToOrganisations = "Lookups.ForwardersAndServices";
			this.PlaceOfConsolidationDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("992ac2ec-a4bf-4b1d-a51c-76057636e47e", "Place of Consolidation");
			this.PlaceOfConsolidationDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 75, true);
			this.PlaceOfConsolidationDocAddressControl.Name = "PlaceOfConsolidationDocAddressControl";
			this.PlaceOfConsolidationDocAddressControl.ReadOnly = false;
			this.PlaceOfConsolidationDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PlaceOfConsolidationDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.PlaceOfConsolidationDocAddressControl.TabIndex = 17;
			this.PlaceOfConsolidationDocAddressControl.ValidationJustForced = false;
			// 
			// ConsolidationDocAddressControl
			// 
			this.ConsolidationDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidationDocAddressControl, "Consolidator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).Consolidator)));
			this.ConsolidationDocAddressControl.BindToOrganisations = "Lookups.ForwardersAndServices";
			this.ConsolidationDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f8a2d729-f1d1-49a1-9b3f-40756480aadd", "Consolidator");
			this.ConsolidationDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 75, true);
			this.ConsolidationDocAddressControl.Name = "ConsolidationDocAddressControl";
			this.ConsolidationDocAddressControl.ReadOnly = false;
			this.ConsolidationDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsolidationDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsolidationDocAddressControl.TabIndex = 18;
			this.ConsolidationDocAddressControl.ValidationJustForced = false;
			// 
			// PrimaryCCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrimaryCCNTextBox, "BP_PrimaryCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_PrimaryCCN)));
			this.PrimaryCCNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("223d05ea-58ea-4a70-89e3-d0ae6c5e18ad", "Primary CCN");
			this.PrimaryCCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 114, true);
			this.PrimaryCCNTextBox.Name = "PrimaryCCNTextBox";
			this.PrimaryCCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.PrimaryCCNTextBox.TabIndex = 8;
			// 
			// MasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "BP_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_MasterBill)));
			this.MasterBillTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("284b1523-afeb-4b08-bf59-d49fc1aa965c", "Master Bill");
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 92, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.MasterBillTextBox.TabIndex = 6;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "BP_ModeOfTransport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).BP_ModeOfTransport)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("395d5f28-a4c6-44bc-932f-9925b02705a5", "Transport Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 68, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.TransportModeDropEdit.TabIndex = 5;
			// 
			// eMHMasterTabControl
			// 
			this.eMHMasterTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.eMHMasterTabControl.Controls.Add(this.eMHHouseTabPage);
			this.eMHMasterTabControl.Controls.Add(this.eMHContainerTabPage);
			this.eMHMasterTabControl.Controls.Add(this.ConsolMessagesTabPage);
			this.eMHMasterTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eMHMasterTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eMHMasterTabControl.Name = "eMHMasterTabControl";
			this.eMHMasterTabControl.SelectedIndex = 0;
			this.eMHMasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 340, true);
			this.eMHMasterTabControl.TabIndex = 0;
			// 
			// eMHHouseTabPage
			// 
			this.eMHHouseTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("39e8aaff-73cf-4865-ad59-8ec0b9f3d088", "House Bills");
			this.eMHHouseTabPage.Controls.Add(this.cusCAeMHHouseUserControl);
			this.eMHHouseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.eMHHouseTabPage.Name = "eMHHouseTabPage";
			this.eMHHouseTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.eMHHouseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(969, 317, true);
			this.eMHHouseTabPage.TabIndex = 0;
			this.eMHHouseTabPage.UseVisualStyleBackColor = true;
			// 
			// cusCAeMHHouseUserControl
			// 
			this.cusCAeMHHouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusCAeMHHouseUserControl, "HouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusCAeMHHouseCollection)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).HouseBills)));
			this.cusCAeMHHouseUserControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b3fb1486-485f-472a-b143-277c18f1800b", "House Bill No.");
			this.cusCAeMHHouseUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusCAeMHHouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusCAeMHHouseUserControl.Name = "cusCAeMHHouseUserControl";
			this.cusCAeMHHouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 50, true);
			this.cusCAeMHHouseUserControl.TabIndex = 0;
			// 
			// eMHContainerTabPage
			// 
			this.eMHContainerTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("39cf7fca-56b3-4852-9c9d-2f59037de7ad", "Containers");
			this.eMHContainerTabPage.Controls.Add(this.cusCAeMHContainerUserControl);
			this.eMHContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.eMHContainerTabPage.Name = "eMHContainerTabPage";
			this.eMHContainerTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.eMHContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(969, 317, true);
			this.eMHContainerTabPage.TabIndex = 1;
			this.eMHContainerTabPage.UseVisualStyleBackColor = true;
			// 
			// cusCAeMHContainerUserControl
			// 
			this.cusCAeMHContainerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusCAeMHContainerUserControl, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusCAeMHContainerCollection)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)).Containers)));
			this.cusCAeMHContainerUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusCAeMHContainerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusCAeMHContainerUserControl.Name = "cusCAeMHContainerUserControl";
			this.cusCAeMHContainerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 50, true);
			this.cusCAeMHContainerUserControl.TabIndex = 0;
			// 
			// ConsolMessagesTabPage
			// 
			this.ConsolMessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e205f5d5-7c2d-4678-bbba-cd4b19ac0e20", "Consol Messages");
			this.ConsolMessagesTabPage.Controls.Add(this.messagesUserControl);
			this.ConsolMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ConsolMessagesTabPage.Name = "ConsolMessagesTabPage";
			this.ConsolMessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConsolMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(969, 317, true);
			this.ConsolMessagesTabPage.TabIndex = 2;
			this.ConsolMessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// messagesUserControl
			// 
			this.messagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.CA.Business.CusCAeMHMaster)(null)))));
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesUserControl.Name = "messagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(962, 310, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// CusCAeMHMasterUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MasterSplitContainer);
			this.Name = "CusCAeMHMasterUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 611, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MasterSplitContainer.Panel1.ResumeLayout(false);
			this.MasterSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MasterSplitContainer)).EndInit();
			this.MasterSplitContainer.ResumeLayout(false);
			this.MasterSplitContainer.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ATADateEdit.ResumeLayout(true);
			this.ATADateEdit.PerformLayout();
			this.LatestNoticeProcessingDateDateEdit.ResumeLayout(true);
			this.LatestNoticeProcessingDateDateEdit.PerformLayout();
			this.AmendmentReasonDropEdit.ResumeLayout(true);
			this.AmendmentReasonDropEdit.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ETADateEdit.ResumeLayout(true);
			this.ETADateEdit.PerformLayout();
			this.DischargePortCodeFindBox.ResumeLayout(true);
			this.DischargePortCodeFindBox.PerformLayout();
			this.DischargeSubLocationCodeFindBox.ResumeLayout(true);
			this.DischargeSubLocationCodeFindBox.PerformLayout();
			this.CustDischargePortCodeFindBox.ResumeLayout(true);
			this.CustDischargePortCodeFindBox.PerformLayout();
			this.CarrierCodeFindBox.ResumeLayout(true);
			this.CarrierCodeFindBox.PerformLayout();
			this.PlaceOfConsolidationDocAddressControl.ResumeLayout(true);
			this.PlaceOfConsolidationDocAddressControl.PerformLayout();
			this.ConsolidationDocAddressControl.ResumeLayout(true);
			this.ConsolidationDocAddressControl.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.eMHMasterTabControl.ResumeLayout(false);
			this.eMHMasterTabControl.PerformLayout();
			this.eMHHouseTabPage.ResumeLayout(false);
			this.eMHHouseTabPage.PerformLayout();
			this.cusCAeMHHouseUserControl.ResumeLayout(true);
			this.cusCAeMHHouseUserControl.PerformLayout();
			this.eMHContainerTabPage.ResumeLayout(false);
			this.eMHContainerTabPage.PerformLayout();
			this.cusCAeMHContainerUserControl.ResumeLayout(true);
			this.cusCAeMHContainerUserControl.PerformLayout();
			this.ConsolMessagesTabPage.ResumeLayout(false);
			this.ConsolMessagesTabPage.PerformLayout();
			this.messagesUserControl.ResumeLayout(true);
			this.messagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MasterSplitContainer;
		private ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private ZArchitecture.ZTextBox MasterHouseCCNTextBox;
		private ZArchitecture.ZTextBox MasterHouseBillTextBox;
		private MasterFiles.GUI.ZDocAddressControl PlaceOfConsolidationDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl ConsolidationDocAddressControl;
		private ZArchitecture.ZTextBox PrimaryCCNTextBox;
		private ZArchitecture.ZTextBox MasterBillTextBox;
		private ZArchitecture.GUI.ZCodeFindBox DischargeSubLocationCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CustDischargePortCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CarrierCodeFindBox;
		private ZArchitecture.GUI.ZTabControl eMHMasterTabControl;
		private ZArchitecture.GUI.ZTabPage eMHHouseTabPage;
		private ZArchitecture.GUI.ZTabPage eMHContainerTabPage;
		internal CusCAeMHHouseUserControl cusCAeMHHouseUserControl;
		private CusCAeMHContainerUserControl cusCAeMHContainerUserControl;
		private ZArchitecture.GUI.ZCodeFindBox DischargePortCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit ETADateEdit;
		private ZArchitecture.GUI.ZTabPage ConsolMessagesTabPage;
		private CAMessagesUserControl messagesUserControl;
		private ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit AmendmentReasonDropEdit;
		private ZArchitecture.ZTextBox LatestD4NoticeTextBox;
		private ZArchitecture.GUI.ZDateEdit LatestNoticeProcessingDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit ATADateEdit;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZCheckBox OverrideFreightDefaultsCheckBox;
	}
}
