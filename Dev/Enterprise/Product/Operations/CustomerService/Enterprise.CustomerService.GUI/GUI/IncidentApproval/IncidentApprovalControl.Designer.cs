using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using System.Windows.Forms;
using System;

namespace Enterprise.CustomerService.GUI
{
	public partial class IncidentApprovalControl : ZUserControl
	{
		protected ZDropEdit CriticalityDropEdit;
		ZArchitecture.ZLabel ServiceRequestLabel;
		ZCodeFindBox zCodeFindBox1;
		internal ZDropEdit MenuSectionDropEdit;
		internal ZDropEdit Cr8ModuleDropEdit;
		internal ZDropEdit Cr9ModuleDropEdit;
		ZArchitecture.ZTextBox IncidentDetailsTextBox;
		protected ZArchitecture.ZTextBox IncidentSummaryTextBox;
		ZDropEdit StatusDropEdit;
		ZArchitecture.ZTextBox IncidentNumberTextBox;
		ZArchitecture.ZLabel zLabel5;
		ZArchitecture.ZLabel zLabel8;
		ZDropEdit CusStatusEdit;
		ZDropEdit zDropEdit3;
		ZGroupBox EConversationGroupBox;
		ZArchitecture.ZTextBox ClientReferenceTextBox;
		ZCodeFindBox ApprovingStaffCodeFindBox;
		ZCodeFindBox ReportToStaffCodeFindBox;
		ZArchitecture.ZLabel NotificationToLabel;
		ZArchitecture.ZLabel WarningMessgeLabel;
		ZArchitecture.ZGrid DocsGrid;

		readonly System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CriticalityDropEdit = new ZDropEdit();
			this.ServiceRequestLabel = new ZArchitecture.ZLabel();
			this.zCodeFindBox1 = new ZCodeFindBox();
			this.MenuSectionDropEdit = new ZDropEdit();
			this.Cr8ModuleDropEdit = new ZDropEdit();
			this.Cr9ModuleDropEdit = new ZDropEdit();
			this.IncidentDetailsTextBox = new ZArchitecture.ZTextBox();
			this.IncidentSummaryTextBox = new ZArchitecture.ZTextBox();
			this.StatusDropEdit = new ZDropEdit();
			this.IncidentNumberTextBox = new ZArchitecture.ZTextBox();
			this.zLabel5 = new ZArchitecture.ZLabel();
			this.zLabel8 = new ZArchitecture.ZLabel();
			this.CusStatusEdit = new ZDropEdit();
			this.zDropEdit3 = new ZDropEdit();
			this.EConversationGroupBox = new ZGroupBox();
			this.ClientReferenceTextBox = new ZArchitecture.ZTextBox();
			this.ApprovingStaffCodeFindBox = new ZCodeFindBox();
			this.ReportToStaffCodeFindBox = new ZCodeFindBox();
			this.NotificationToLabel = new ZArchitecture.ZLabel();
			this.WarningMessgeLabel = new ZArchitecture.ZLabel();
			this.DocsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CriticalityDropEdit.SuspendLayout();
			this.zCodeFindBox1.SuspendLayout();
			this.MenuSectionDropEdit.SuspendLayout();
			this.Cr8ModuleDropEdit.SuspendLayout();
			this.Cr9ModuleDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.CusStatusEdit.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.ApprovingStaffCodeFindBox.SuspendLayout();
			this.ReportToStaffCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocsGrid)).BeginInit();
			this.DocsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IncidentApproval);
			// 
			// CriticalityDropEdit
			// 
			this.CriticalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CriticalityDropEdit, "IA_Criticality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_Criticality);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Lookups.CriticalityList);
			this.CriticalityDropEdit.BindToList = "Lookups+CriticalityList";
			this.CriticalityDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 128, true);
			this.CriticalityDropEdit.Name = "CriticalityDropEdit";
			this.CriticalityDropEdit.PreBoundMaxLength = 3;
			this.CriticalityDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.CriticalityDropEdit.TabIndex = 4;
			// 
			// ServiceRequestLabel
			// 
			this.ServiceRequestLabel.AutoSize = true;
			this.ServiceRequestLabel.CaptionResourceString = Res.GetData("IncidentApprovalControl|ed309e6d-ed7c-4e53-a1aa-01fabc29db53", "eRequest");

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.ServiceRequestLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			After:
						this.ServiceRequestLabel.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.SansSerif)));
			*/
			this.ServiceRequestLabel.FontType = (OFontTypes.Normal | OFontTypes.SansSerif);
			this.ServiceRequestLabel.ForeColor = System.Drawing.Color.DarkBlue;
			this.ServiceRequestLabel.Location = ControlDpiScalingHelper.NewScaledPoint(33, 11, true);
			this.ServiceRequestLabel.Name = "ServiceRequestLabel";
			this.ServiceRequestLabel.Size = ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.ServiceRequestLabel.TabIndex = 0;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "ReportedByStaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).ReportedByStaffCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Lookups.ReportingStaff);
			this.zCodeFindBox1.BindToList = "Lookups+ReportingStaff";
			this.zCodeFindBox1.CaptionResourceString = Res.GetData("6fde524e-cca5-4094-a310-fd23dbc72f3e", "Reported By");
			this.zCodeFindBox1.Location = ControlDpiScalingHelper.NewScaledPoint(105, 355, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 3;
			this.zCodeFindBox1.ShouldResize = true;
			this.zCodeFindBox1.Size = ControlDpiScalingHelper.NewScaledSize(274, 17, true);
			this.zCodeFindBox1.TabIndex = 10;
			// 
			// MenuSectionDropEdit
			// 
			this.MenuSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MenuSectionDropEdit, "MenuSection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).MenuSection);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).MenuSectionDescription);
			this.MenuSectionDropEdit.BindToForDescription = "MenuSectionDescription";
			this.MenuSectionDropEdit.CaptionResourceString = Res.GetData("7a615ee6-2e35-40e6-a5af-a6b614906bce", "Menu Section");
			this.MenuSectionDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 154, true);
			this.MenuSectionDropEdit.Name = "MenuSectionDropEdit";
			this.MenuSectionDropEdit.PreBoundMaxLength = 3;
			this.MenuSectionDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.MenuSectionDropEdit.TabIndex = 5;
			this.MenuSectionDropEdit.Visible = false;
			// 
			// Cr8ModuleDropEdit
			// 
			this.Cr8ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Cr8ModuleDropEdit, "Cr8Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Cr8Module);
			this.Cr8ModuleDropEdit.CaptionResourceString = Res.GetData("b966d704-6adc-43f3-8419-5fe64b53aed8", "Requirement");
			this.Cr8ModuleDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 154, true);
			this.Cr8ModuleDropEdit.Name = "Cr8ModuleDropEdit";
			this.Cr8ModuleDropEdit.PreBoundMaxLength = 3;
			this.Cr8ModuleDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.Cr8ModuleDropEdit.TabIndex = 5;
			this.Cr8ModuleDropEdit.Visible = false;
			// 
			// Cr9ModuleDropEdit
			// 
			this.Cr9ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Cr9ModuleDropEdit, "Cr9Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Cr9Module);
			this.Cr9ModuleDropEdit.CaptionResourceString = Res.GetData("a5b63706-f90f-47ea-8f73-e018cb72ff6e", "Service");
			this.Cr9ModuleDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 154, true);
			this.Cr9ModuleDropEdit.Name = "Cr9ModuleDropEdit";
			this.Cr9ModuleDropEdit.PreBoundMaxLength = 3;
			this.Cr9ModuleDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.Cr9ModuleDropEdit.TabIndex = 5;
			this.Cr9ModuleDropEdit.Visible = false;
			// 
			// IncidentDetailsTextBox
			// 
			this.IncidentDetailsTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.IncidentDetailsTextBox, "IA_IncidentDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_IncidentDetails);
			this.IncidentDetailsTextBox.CharacterCasing = CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.IncidentDetailsTextBox, 1);
			this.IncidentDetailsTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(105, 207, true);
			this.IncidentDetailsTextBox.Multiline = true;
			this.IncidentDetailsTextBox.Name = "IncidentDetailsTextBox";
			this.IncidentDetailsTextBox.ScrollBars = ScrollBars.Both;
			this.IncidentDetailsTextBox.Size = ControlDpiScalingHelper.NewScaledSize(354, 87, true);
			this.IncidentDetailsTextBox.TabIndex = 7;
			this.IncidentDetailsTextBox.Enter += new EventHandler(this.IncidentDetailsTextBox_Enter);
			this.IncidentDetailsTextBox.Leave += new EventHandler(this.IncidentDetailsTextBox_Leave);
			this.IncidentDetailsTextBox.MouseLeave += new EventHandler(this.IncidentDetailsTextBox_MouseLeave);
			// 
			// IncidentSummaryTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncidentSummaryTextBox, "IA_IncidentSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_IncidentSummary);
			this.IncidentSummaryTextBox.CharacterCasing = CharacterCasing.Normal;
			this.IncidentSummaryTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(105, 181, true);
			this.IncidentSummaryTextBox.Name = "IncidentSummaryTextBox";
			this.IncidentSummaryTextBox.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.IncidentSummaryTextBox.TabIndex = 6;
			this.IncidentSummaryTextBox.Enter += new EventHandler(this.IncidentSummaryTextBox_Enter);
			this.IncidentSummaryTextBox.Leave += new EventHandler(this.IncidentSummaryTextBox_Leave);
			this.IncidentSummaryTextBox.MouseLeave += new EventHandler(this.IncidentSummaryTextBox_MouseLeave);
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "IA_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_Status);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Lookups.StatusList);
			this.StatusDropEdit.BindToList = "Lookups+StatusList";
			this.StatusDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 102, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.StatusDropEdit.TabIndex = 3;
			// 
			// IncidentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncidentNumberTextBox, "IA_IncidentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_IncidentNumber);
			this.IncidentNumberTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(340, 76, true);
			this.IncidentNumberTextBox.Name = "IncidentNumberTextBox";
			this.IncidentNumberTextBox.Size = ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.IncidentNumberTextBox.TabIndex = 2;
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.CaptionResourceString = Res.GetData("IncidentApprovalControl|13596e81-c480-4dbd-afa6-fcfeadc88ad6", "eDocs:");

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.zLabel5.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			After:
						this.zLabel5.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.Bold)));
			*/
			this.zLabel5.FontType = (OFontTypes.Normal | OFontTypes.Bold);
			this.zLabel5.IsFontBold = true;
			this.zLabel5.Location = ControlDpiScalingHelper.NewScaledPoint(9, 455, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			this.zLabel5.TabIndex = 28;
			// 
			// zLabel8
			// 
			this.zLabel8.CaptionResourceString = Res.GetData("IncidentApprovalControl|62d1ab10-5f00-437d-b685-9d888bb706d8", "To attach further files, please drag them onto this form so they are included in the eDocs tab.");

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.zLabel8.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			After:
						this.zLabel8.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.SansSerif)));
			*/
			this.zLabel8.FontType = (OFontTypes.Normal | OFontTypes.SansSerif);
			this.zLabel8.Location = ControlDpiScalingHelper.NewScaledPoint(9, 474, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = ControlDpiScalingHelper.NewScaledSize(450, 19, true);
			this.zLabel8.TabIndex = 50;
			this.zLabel8.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// CusStatusEdit
			// 
			this.CusStatusEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusStatusEdit, "IA_ClientSpecifiedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_ClientSpecifiedStatus);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Lookups.CustomerStatusList);
			this.CusStatusEdit.BindToList = "Lookups+CustomerStatusList";
			this.CusStatusEdit.Location = ControlDpiScalingHelper.NewScaledPoint(105, 329, true);
			this.CusStatusEdit.Name = "CusStatusEdit";
			this.CusStatusEdit.PreBoundMaxLength = 3;
			this.CusStatusEdit.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.CusStatusEdit.TabIndex = 9;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "IA_LicenceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_LicenceCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).Lookups.LicenceCompanyList);
			this.zDropEdit3.BindToList = "Lookups+LicenceCompanyList";
			this.zDropEdit3.Location = ControlDpiScalingHelper.NewScaledPoint(105, 303, true);
			this.zDropEdit3.MaxItemsToShowInDropDown = 25;
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 9;
			this.zDropEdit3.Size = ControlDpiScalingHelper.NewScaledSize(354, 17, true);
			this.zDropEdit3.TabIndex = 8;
			// 
			// EConversationGroupBox
			// 
			this.EConversationGroupBox.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Left)
			| AnchorStyles.Right);
			this.EConversationGroupBox.CaptionResourceString = Res.GetData("6e842359-31bb-4d4b-bed1-079d2dfbfa50", "eConversation");
			this.EConversationGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(475, 76, true);
			this.EConversationGroupBox.Name = "EConversationGroupBox";
			this.EConversationGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(491, 505, true);
			this.EConversationGroupBox.TabIndex = 20;
			this.EConversationGroupBox.TabStop = false;
			// 
			// ClientReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientReferenceTextBox, "IA_ClientReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_ClientReference);
			this.ClientReferenceTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(105, 76, true);
			this.ClientReferenceTextBox.Name = "ClientReferenceTextBox";
			this.ClientReferenceTextBox.Size = ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.ClientReferenceTextBox.TabIndex = 1;
			// 
			// ApprovingStaffCodeFindBox
			// 
			this.ApprovingStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApprovingStaffCodeFindBox, "IA_GS_NKApprovingStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_GS_NKApprovingStaff);
			this.ApprovingStaffCodeFindBox.CaptionResourceString = Res.GetData("5faca262-326e-45a3-a3a8-0bbf3f081a3e", "Approved By");
			this.ApprovingStaffCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(105, 381, true);
			this.ApprovingStaffCodeFindBox.Name = "ApprovingStaffCodeFindBox";
			this.ApprovingStaffCodeFindBox.PreBoundMaxLength = 3;
			this.ApprovingStaffCodeFindBox.ShouldResize = true;
			this.ApprovingStaffCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(274, 17, true);
			this.ApprovingStaffCodeFindBox.TabIndex = 11;
			// 
			// ReportToStaffCodeFindBox
			// 
			this.ReportToStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportToStaffCodeFindBox, "IA_GS_NKReportingStaff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).IA_GS_NKReportingStaff);
			this.ReportToStaffCodeFindBox.CaptionResourceString = Res.GetData("79770605-253f-4329-bb0c-08ab71c9dde5", "Third Party Notify");
			this.ReportToStaffCodeFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(105, 407, true);
			this.ReportToStaffCodeFindBox.Name = "ReportToStaffCodeFindBox";
			this.ReportToStaffCodeFindBox.PreBoundMaxLength = 3;
			this.ReportToStaffCodeFindBox.ShouldResize = true;
			this.ReportToStaffCodeFindBox.Size = ControlDpiScalingHelper.NewScaledSize(274, 17, true);
			this.ReportToStaffCodeFindBox.TabIndex = 12;
			// 
			// NotificationToLabel
			// 
			this.BindingSource.SetBindingMember(this.NotificationToLabel, "NotificationTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).NotificationTo);

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.NotificationToLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			After:
						this.NotificationToLabel.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.Bold)));
			*/
			this.NotificationToLabel.FontType = (OFontTypes.Normal | OFontTypes.Bold);
			this.NotificationToLabel.IsFontBold = true;
			this.NotificationToLabel.Location = ControlDpiScalingHelper.NewScaledPoint(102, 432, true);
			this.NotificationToLabel.Name = "NotificationToLabel";
			this.NotificationToLabel.Size = ControlDpiScalingHelper.NewScaledSize(357, 19, true);
			this.NotificationToLabel.TabIndex = 51;
			this.NotificationToLabel.Text = "<Notification To>";
			// 
			// WarningMessgeLabel
			// 
			this.WarningMessgeLabel.Anchor = ((AnchorStyles.Top | AnchorStyles.Left)
			| AnchorStyles.Right);

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.WarningMessgeLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			After:
						this.WarningMessgeLabel.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.SansSerif)));
			*/
			this.WarningMessgeLabel.FontType = (OFontTypes.Normal | OFontTypes.SansSerif);
			this.WarningMessgeLabel.ForeColor = System.Drawing.Color.Red;
			this.WarningMessgeLabel.Location = ControlDpiScalingHelper.NewScaledPoint(399, 0, true);
			this.WarningMessgeLabel.Name = "WarningMessgeLabel";
			this.WarningMessgeLabel.Size = ControlDpiScalingHelper.NewScaledSize(318, 68, true);
			this.WarningMessgeLabel.TabIndex = 56;
			this.WarningMessgeLabel.Visible = false;
			// 
			// DocsGrid
			// 
			this.DocsGrid.AllowNavigation = false;
			this.DocsGrid.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Left);
			this.BindingSource.SetBindingMember(this.DocsGrid, "AttachedEDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApproval)(null)).AttachedEDocs);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApprovalAttachment)(((System.Collections.IList)(((IncidentApproval)(null)).AttachedEDocs)).SyncRoot)).Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IncidentApprovalAttachment)(((System.Collections.IList)(((IncidentApproval)(null)).AttachedEDocs)).SyncRoot)).FileName);
			this.DocsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("d882238f-5ce4-45ce-b4f4-3b89b81cc46d", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("78fa7df7-15f8-4116-9bfc-adff23838931", "File Name");
			zTextBoxColumnStyleInfo2.ColumnName = "FileName";
			zTextBoxColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocsGrid.GridId = "fea1252d-685b-4c0d-a196-989c8065076b";
			this.DocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocsGrid.LayoutKey = "DocsGrid";
			this.DocsGrid.Location = ControlDpiScalingHelper.NewScaledPoint(8, 498, true);
			this.DocsGrid.Name = "DocsGrid";
			this.DocsGrid.Size = ControlDpiScalingHelper.NewScaledSize(451, 80, true);
			this.DocsGrid.TabIndex = 57;
			this.DocsGrid.DoubleClick += new EventHandler(this.DocsGrid_DoubleClick);
			// 
			// IncidentApprovalControl
			// 
			this.AutoScaleMode = AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocsGrid);
			this.Controls.Add(this.WarningMessgeLabel);
			this.Controls.Add(this.NotificationToLabel);
			this.Controls.Add(this.ReportToStaffCodeFindBox);
			this.Controls.Add(this.ApprovingStaffCodeFindBox);
			this.Controls.Add(this.EConversationGroupBox);
			this.Controls.Add(this.zDropEdit3);
			this.Controls.Add(this.CusStatusEdit);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.IncidentNumberTextBox);
			this.Controls.Add(this.ClientReferenceTextBox);
			this.Controls.Add(this.CriticalityDropEdit);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.ServiceRequestLabel);
			this.Controls.Add(this.zCodeFindBox1);
			this.Controls.Add(this.MenuSectionDropEdit);
			this.Controls.Add(this.Cr8ModuleDropEdit);
			this.Controls.Add(this.Cr9ModuleDropEdit);
			this.Controls.Add(this.IncidentDetailsTextBox);
			this.Controls.Add(this.IncidentSummaryTextBox);
			this.Name = "IncidentApprovalControl";
			this.Size = ControlDpiScalingHelper.NewScaledSize(973, 584, true);
			this.DragDrop += new DragEventHandler(this.IncidentApprovalControl_DragDrop);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CriticalityDropEdit.ResumeLayout(true);
			this.CriticalityDropEdit.PerformLayout();
			this.zCodeFindBox1.ResumeLayout(true);
			this.zCodeFindBox1.PerformLayout();
			this.MenuSectionDropEdit.ResumeLayout(true);
			this.MenuSectionDropEdit.PerformLayout();
			this.Cr8ModuleDropEdit.ResumeLayout(true);
			this.Cr8ModuleDropEdit.PerformLayout();
			this.Cr9ModuleDropEdit.ResumeLayout(true);
			this.Cr9ModuleDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.CusStatusEdit.ResumeLayout(true);
			this.CusStatusEdit.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.ApprovingStaffCodeFindBox.ResumeLayout(true);
			this.ApprovingStaffCodeFindBox.PerformLayout();
			this.ReportToStaffCodeFindBox.ResumeLayout(true);
			this.ReportToStaffCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocsGrid)).EndInit();
			this.DocsGrid.ResumeLayout(false);
			this.DocsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
