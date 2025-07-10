namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPEUTransitUserControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.AQISTransitDestinationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransitLocationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ATDTransitDestinationAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.AQISResponsiblePersonAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ContactInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactInfoAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TestResultRequired = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.placeOfDestinationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PlaceOfDestinationDocAddressControl = new Enterprise.Customs.AU.Declaration.GUI.NEXDOCJobDocAddressUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AQISTransitDestinationGroupBox.SuspendLayout();
			this.TransitLocationTypeDropEdit.SuspendLayout();
			this.ATDTransitDestinationAddressControl.SuspendLayout();
			this.AQISResponsiblePersonAddressControl.SuspendLayout();
			this.ContactInfoGroupBox.SuspendLayout();
			this.ContactInfoAddressControl.SuspendLayout();
			this.placeOfDestinationGroupBox.SuspendLayout();
			this.PlaceOfDestinationDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// AQISTransitDestinationGroupBox
			// 
			this.AQISTransitDestinationGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("F4143FC0-3E07-437E-A26C-F7AB08A739AB", "Transit Destination");
			this.AQISTransitDestinationGroupBox.Controls.Add(this.ApprovalNumberTextBox);
			this.AQISTransitDestinationGroupBox.Controls.Add(this.TransitLocationTypeDropEdit);
			this.AQISTransitDestinationGroupBox.Controls.Add(this.ATDTransitDestinationAddressControl);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.AQISTransitDestinationGroupBox, true);
			this.AQISTransitDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 8, true);
			this.AQISTransitDestinationGroupBox.Name = "AQISTransitDestinationGroupBox";
			this.AQISTransitDestinationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 289, true);
			this.AQISTransitDestinationGroupBox.TabIndex = 23;
			this.AQISTransitDestinationGroupBox.TabStop = false;
			this.AQISTransitDestinationGroupBox.Text = "Transit Destination";
			// 
			// ApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovalNumberTextBox, "QuarantineExDocHeader+QH_ApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ApprovalNumber)));
			this.ApprovalNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9d3bb587-0e99-4e5d-b8f6-55ea875a6728", "Approval Number");
			this.ApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 225, true);
			this.ApprovalNumberTextBox.Name = "ApprovalNumberTextBox";
			this.ApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.ApprovalNumberTextBox.TabIndex = 20;
			// 
			// TransitLocationTypeDropEdit
			// 
			this.TransitLocationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitLocationTypeDropEdit, "QuarantineExDocHeader+QH_TransitLocationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TransitLocationType)));
			this.TransitLocationTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("62326cf6-97a0-4b7b-ad53-5e55c7e4c1d0", "Transit Location");
			this.TransitLocationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 204, true);
			this.TransitLocationTypeDropEdit.Name = "TransitLocationTypeDropEdit";
			this.TransitLocationTypeDropEdit.PreBoundMaxLength = 3;
			this.TransitLocationTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TransitLocationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.TransitLocationTypeDropEdit.TabIndex = 19;
			// 
			// ATDTransitDestinationAddressControl
			// 
			this.ATDTransitDestinationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ATDTransitDestinationAddressControl, "AQISTransitDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISTransitDestination)));
			this.ATDTransitDestinationAddressControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.ATDTransitDestinationAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("83d36e42-d789-4f5d-9530-3541a723a2b3", "Address");
			this.ATDTransitDestinationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 18, true);
			this.ATDTransitDestinationAddressControl.Name = "ATDTransitDestinationAddressControl";
			this.ATDTransitDestinationAddressControl.ReadOnly = false;
			this.ATDTransitDestinationAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ATDTransitDestinationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ATDTransitDestinationAddressControl.TabIndex = 18;
			this.ATDTransitDestinationAddressControl.ValidationJustForced = false;
			// 
			// AQISResponsiblePersonAddressControl
			// 
			this.AQISResponsiblePersonAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AQISResponsiblePersonAddressControl, "AQISResponsiblePerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISResponsiblePerson)));
			this.AQISResponsiblePersonAddressControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.AQISResponsiblePersonAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ea51af50-4d7b-4175-b2e0-ce3fd7ed02af", "Responsible Person");
			this.AQISResponsiblePersonAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.AQISResponsiblePersonAddressControl.Name = "AQISResponsiblePersonAddressControl";
			this.AQISResponsiblePersonAddressControl.ReadOnly = false;
			this.AQISResponsiblePersonAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.AQISResponsiblePersonAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.AQISResponsiblePersonAddressControl.TabIndex = 22;
			this.AQISResponsiblePersonAddressControl.ValidationJustForced = false;
			// 
			// ContactInfoGroupBox
			// 
			this.ContactInfoGroupBox.Controls.Add(this.ContactInfoAddressControl);
			this.ContactInfoGroupBox.Controls.Add(this.CommentTextBox);
			this.ContactInfoGroupBox.Controls.Add(this.TestResultRequired);
			this.ContactInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 7, true);
			this.ContactInfoGroupBox.Name = "ContactInfoGroupBox";
			this.ContactInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 290, true);
			this.ContactInfoGroupBox.TabIndex = 24;
			this.ContactInfoGroupBox.TabStop = false;
			this.ContactInfoGroupBox.Text = "Contact Information";
			// 
			// ContactInfoAddressControl
			// 
			this.ContactInfoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactInfoAddressControl, "AQISEUContactPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISEUContactPerson)));
			this.ContactInfoAddressControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.ContactInfoAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("99eea234-0566-4abc-986a-22b97fb8d595", "EU Contact Person");
			this.ContactInfoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ContactInfoAddressControl.Name = "ContactInfoAddressControl";
			this.ContactInfoAddressControl.ReadOnly = false;
			this.ContactInfoAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ContactInfoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ContactInfoAddressControl.TabIndex = 21;
			this.ContactInfoAddressControl.ValidationJustForced = false;
			// 
			// CommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentTextBox, "QuarantineExDocHeader+QH_EUComments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_EUComments)));
			this.CommentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ac903e4a-fba5-4feb-8717-ceba6667fc61", "Comments");
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 207, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 46, true);
			this.CommentTextBox.TabIndex = 22;
			// 
			// TestResultRequired
			// 
			this.BindingSource.SetBindingMember(this.TestResultRequired, "QuarantineExDocHeader+QH_EUTestResultRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_EUTestResultRequired)));
			this.TestResultRequired.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("231c6d09-c646-4381-8a20-7ea46f26a8c8", "Test Result Required");
			this.TestResultRequired.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TestResultRequired.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 259, true);
			this.TestResultRequired.Name = "TestResultRequired";
			this.TestResultRequired.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 24, true);
			this.TestResultRequired.TabIndex = 23;
			// 
			// placeOfDestinationGroupBox
			// 
			this.placeOfDestinationGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("140e48e4-61e3-4cbe-be48-c4477a056bc0", "Place of Destination");
			this.placeOfDestinationGroupBox.Controls.Add(this.PlaceOfDestinationDocAddressControl);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.placeOfDestinationGroupBox, true);
			this.placeOfDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(801, 7, true);
			this.placeOfDestinationGroupBox.Name = "placeOfDestinationGroupBox";
			this.placeOfDestinationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 289, true);
			this.placeOfDestinationGroupBox.TabIndex = 25;
			this.placeOfDestinationGroupBox.TabStop = false;
			this.placeOfDestinationGroupBox.Text = "Place of Destination";
			// 
			// PlaceOfDestinationDocAddressControl
			// 
			this.PlaceOfDestinationDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfDestinationDocAddressControl, "AQISEUPlaceOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.AUJobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISEUPlaceOfDestination)));
			this.PlaceOfDestinationDocAddressControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlaceOfDestinationDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PlaceOfDestinationDocAddressControl.Name = "PlaceOfDestinationDocAddressControl";
			this.PlaceOfDestinationDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 272, true);
			this.PlaceOfDestinationDocAddressControl.TabIndex = 18;
			// 
			// RFPEUTransitUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.placeOfDestinationGroupBox);
			this.Controls.Add(this.AQISTransitDestinationGroupBox);
			this.Controls.Add(this.AQISResponsiblePersonAddressControl);
			this.Controls.Add(this.ContactInfoGroupBox);
			this.Name = "RFPEUTransitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 301, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AQISTransitDestinationGroupBox.ResumeLayout(false);
			this.AQISTransitDestinationGroupBox.PerformLayout();
			this.TransitLocationTypeDropEdit.ResumeLayout(true);
			this.TransitLocationTypeDropEdit.PerformLayout();
			this.ATDTransitDestinationAddressControl.ResumeLayout(true);
			this.ATDTransitDestinationAddressControl.PerformLayout();
			this.AQISResponsiblePersonAddressControl.ResumeLayout(true);
			this.AQISResponsiblePersonAddressControl.PerformLayout();
			this.ContactInfoGroupBox.ResumeLayout(false);
			this.ContactInfoGroupBox.PerformLayout();
			this.ContactInfoAddressControl.ResumeLayout(true);
			this.ContactInfoAddressControl.PerformLayout();
			this.placeOfDestinationGroupBox.ResumeLayout(false);
			this.placeOfDestinationGroupBox.PerformLayout();
			this.PlaceOfDestinationDocAddressControl.ResumeLayout(true);
			this.PlaceOfDestinationDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AQISTransitDestinationGroupBox;
		private ZArchitecture.ZTextBox ApprovalNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit TransitLocationTypeDropEdit;
		private MasterFiles.GUI.ZDocAddressControl ATDTransitDestinationAddressControl;
		private MasterFiles.GUI.ZDocAddressControl AQISResponsiblePersonAddressControl;
		private ZArchitecture.GUI.ZGroupBox ContactInfoGroupBox;
		private MasterFiles.GUI.ZDocAddressControl ContactInfoAddressControl;
		private ZArchitecture.ZTextBox CommentTextBox;
		private ZArchitecture.GUI.ZCheckBox TestResultRequired;
		private ZArchitecture.GUI.ZGroupBox placeOfDestinationGroupBox;
		private Enterprise.Customs.AU.Declaration.GUI.NEXDOCJobDocAddressUserControl PlaceOfDestinationDocAddressControl;
	}
}
