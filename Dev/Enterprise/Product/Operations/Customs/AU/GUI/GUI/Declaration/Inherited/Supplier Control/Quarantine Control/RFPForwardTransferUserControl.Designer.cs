namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPForwardTransferUserControl
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
			this.TransferToDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QH_TransferExporterLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_TransfereeEDIUserIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_TransferEDIUserLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_TransfereeExporterNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_CancelTransferIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ForwardDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RequiresAcceptanceCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QH_ForwardeeEDIUserIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_ForwardLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_ForwardStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_OH_ForwardLocationOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransferToDetailsGroupBox.SuspendLayout();
			this.QH_TransferExporterLocationDropEdit.SuspendLayout();
			this.QH_TransferEDIUserLocationDropEdit.SuspendLayout();
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.SuspendLayout();
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.SuspendLayout();
			this.ForwardDetailsGroupBox.SuspendLayout();
			this.QH_ForwardLocationDropEdit.SuspendLayout();
			this.QH_ForwardStatusDropEdit.SuspendLayout();
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// TransferToDetailsGroupBox
			// 
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_TransferExporterLocationDropEdit);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_TransfereeEDIUserIdentifierTextBox);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_TransferEDIUserLocationDropEdit);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_TransfereeExporterNumberTextBox);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_CancelTransferIndicatorCheckBox);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox);
			this.TransferToDetailsGroupBox.Controls.Add(this.QH_OH_TransferExporterLocationOrganisationGuidFindBox);
			this.TransferToDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 113, true);
			this.TransferToDetailsGroupBox.Name = "TransferToDetailsGroupBox";
			this.TransferToDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 93, true);
			this.TransferToDetailsGroupBox.TabIndex = 3;
			this.TransferToDetailsGroupBox.TabStop = false;
			this.TransferToDetailsGroupBox.Text = "Transfer To Details";
			// 
			// QH_TransferExporterLocationDropEdit
			// 
			this.QH_TransferExporterLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_TransferExporterLocationDropEdit, "QuarantineExDocHeader+QH_TransferExporterLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TransferExporterLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.Location)));
			this.QH_TransferExporterLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+Location";
			this.QH_TransferExporterLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 42, true);
			this.QH_TransferExporterLocationDropEdit.Name = "QH_TransferExporterLocationDropEdit";
			this.QH_TransferExporterLocationDropEdit.ShouldResizeByMaxLength = true;
			this.QH_TransferExporterLocationDropEdit.ShowDescriptionBox = false;
			this.QH_TransferExporterLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_TransferExporterLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QH_TransferExporterLocationDropEdit.TabIndex = 4;
			// 
			// QH_TransfereeEDIUserIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_TransfereeEDIUserIdentifierTextBox, "QuarantineExDocHeader+QH_TransfereeEDIUserIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TransfereeEDIUserIdentifier)));
			this.QH_TransfereeEDIUserIdentifierTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_TransfereeEDIUserIdentifierTextBox, false);
			this.QH_TransfereeEDIUserIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 16, true);
			this.QH_TransfereeEDIUserIdentifierTextBox.Name = "QH_TransfereeEDIUserIdentifierTextBox";
			this.QH_TransfereeEDIUserIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.QH_TransfereeEDIUserIdentifierTextBox.TabIndex = 2;
			// 
			// QH_TransferEDIUserLocationDropEdit
			// 
			this.QH_TransferEDIUserLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_TransferEDIUserLocationDropEdit, "QuarantineExDocHeader+QH_TransferEDIUserLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TransferEDIUserLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.Location)));
			this.QH_TransferEDIUserLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+Location";
			this.QH_TransferEDIUserLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 16, true);
			this.QH_TransferEDIUserLocationDropEdit.Name = "QH_TransferEDIUserLocationDropEdit";
			this.QH_TransferEDIUserLocationDropEdit.ShouldResizeByMaxLength = true;
			this.QH_TransferEDIUserLocationDropEdit.ShowDescriptionBox = false;
			this.QH_TransferEDIUserLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_TransferEDIUserLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QH_TransferEDIUserLocationDropEdit.TabIndex = 1;
			// 
			// QH_TransfereeExporterNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_TransfereeExporterNumberTextBox, "QuarantineExDocHeader+QH_TransfereeExporterNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TransfereeExporterNumber)));
			this.QH_TransfereeExporterNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_TransfereeExporterNumberTextBox, false);
			this.QH_TransfereeExporterNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 42, true);
			this.QH_TransfereeExporterNumberTextBox.Name = "QH_TransfereeExporterNumberTextBox";
			this.QH_TransfereeExporterNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.QH_TransfereeExporterNumberTextBox.TabIndex = 5;
			// 
			// QH_CancelTransferIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.QH_CancelTransferIndicatorCheckBox, "QuarantineExDocHeader+QH_CancelTransferIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_CancelTransferIndicator)));
			this.QH_CancelTransferIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.QH_CancelTransferIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QH_CancelTransferIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 68, true);
			this.QH_CancelTransferIndicatorCheckBox.Name = "QH_CancelTransferIndicatorCheckBox";
			this.QH_CancelTransferIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.QH_CancelTransferIndicatorCheckBox.TabIndex = 6;
			this.QH_CancelTransferIndicatorCheckBox.Text = "Cancel Transfer:           ";
			this.QH_CancelTransferIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// QH_OH_TransferEDIUserLocationOrganisationGuidFindBox
			// 
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox, "QuarantineExDocHeader+QH_OH_TransferEDIUserLocationOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OH_TransferEDIUserLocationOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.EDIUser)));
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.BindToList = "QuarantineExDocHeader+Lookups+EDIUser";
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 16, true);
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.Name = "QH_OH_TransferEDIUserLocationOrganisationGuidFindBox";
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.ShouldResize = true;
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.TabIndex = 2;
			// 
			// QH_OH_TransferExporterLocationOrganisationGuidFindBox
			// 
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_OH_TransferExporterLocationOrganisationGuidFindBox, "QuarantineExDocHeader+QH_OH_TransferExporterLocationOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OH_TransferExporterLocationOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.ExporterNumber)));
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.BindToList = "QuarantineExDocHeader+Lookups+ExporterNumber";
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 42, true);
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.Name = "QH_OH_TransferExporterLocationOrganisationGuidFindBox";
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.ShouldResize = true;
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.TabIndex = 5;
			// 
			// ForwardDetailsGroupBox
			// 
			this.ForwardDetailsGroupBox.Controls.Add(this.RequiresAcceptanceCheckbox);
			this.ForwardDetailsGroupBox.Controls.Add(this.QH_ForwardeeEDIUserIdentifierTextBox);
			this.ForwardDetailsGroupBox.Controls.Add(this.QH_ForwardLocationDropEdit);
			this.ForwardDetailsGroupBox.Controls.Add(this.QH_ForwardStatusDropEdit);
			this.ForwardDetailsGroupBox.Controls.Add(this.QH_OH_ForwardLocationOrganisationGuidFindBox);
			this.ForwardDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.ForwardDetailsGroupBox.Name = "ForwardDetailsGroupBox";
			this.ForwardDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 100, true);
			this.ForwardDetailsGroupBox.TabIndex = 2;
			this.ForwardDetailsGroupBox.TabStop = false;
			this.ForwardDetailsGroupBox.Text = "Forward Details";
			// 
			// RequiresAcceptanceCheckbox
			// 
			this.BindingSource.SetBindingMember(this.RequiresAcceptanceCheckbox, "QuarantineExDocHeader.QH_ForwardRequiresAcceptance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ForwardRequiresAcceptance)));
			this.RequiresAcceptanceCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequiresAcceptanceCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequiresAcceptanceCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 73, true);
			this.RequiresAcceptanceCheckbox.Name = "RequiresAcceptanceCheckbox";
			this.RequiresAcceptanceCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.RequiresAcceptanceCheckbox.TabIndex = 7;
			this.RequiresAcceptanceCheckbox.Text = "Requires Acceptance:           ";
			this.RequiresAcceptanceCheckbox.UseVisualStyleBackColor = true;
			// 
			// QH_ForwardeeEDIUserIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_ForwardeeEDIUserIdentifierTextBox, "QuarantineExDocHeader+QH_ForwardeeEDIUserIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ForwardeeEDIUserIdentifier)));
			this.QH_ForwardeeEDIUserIdentifierTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_ForwardeeEDIUserIdentifierTextBox, false);
			this.QH_ForwardeeEDIUserIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 19, true);
			this.QH_ForwardeeEDIUserIdentifierTextBox.Name = "QH_ForwardeeEDIUserIdentifierTextBox";
			this.QH_ForwardeeEDIUserIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.QH_ForwardeeEDIUserIdentifierTextBox.TabIndex = 2;
			// 
			// QH_ForwardLocationDropEdit
			// 
			this.QH_ForwardLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ForwardLocationDropEdit, "QuarantineExDocHeader+QH_ForwardLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ForwardLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.Location)));
			this.QH_ForwardLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+Location";
			this.QH_ForwardLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 19, true);
			this.QH_ForwardLocationDropEdit.Name = "QH_ForwardLocationDropEdit";
			this.QH_ForwardLocationDropEdit.ShouldResizeByMaxLength = true;
			this.QH_ForwardLocationDropEdit.ShowDescriptionBox = false;
			this.QH_ForwardLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_ForwardLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QH_ForwardLocationDropEdit.TabIndex = 1;
			// 
			// QH_ForwardStatusDropEdit
			// 
			this.QH_ForwardStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ForwardStatusDropEdit, "QuarantineExDocHeader+QH_ForwardStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ForwardStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.ComplianceCodes)));
			this.QH_ForwardStatusDropEdit.BindToList = "QuarantineExDocHeader+Lookups+ComplianceCodes";
			this.QH_ForwardStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 47, true);
			this.QH_ForwardStatusDropEdit.Name = "QH_ForwardStatusDropEdit";
			this.QH_ForwardStatusDropEdit.ShouldResizeByMaxLength = true;
			this.QH_ForwardStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.QH_ForwardStatusDropEdit.TabIndex = 4;
			// 
			// QH_OH_ForwardLocationOrganisationGuidFindBox
			// 
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_OH_ForwardLocationOrganisationGuidFindBox, "QuarantineExDocHeader+QH_OH_ForwardLocationOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OH_ForwardLocationOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.EDIUser)));
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.BindToList = "QuarantineExDocHeader+Lookups+EDIUser";
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 19, true);
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.Name = "QH_OH_ForwardLocationOrganisationGuidFindBox";
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.ShouldResize = true;
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.TabIndex = 2;
			// 
			// RFPForwardTransferUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransferToDetailsGroupBox);
			this.Controls.Add(this.ForwardDetailsGroupBox);
			this.Name = "RFPForwardTransferUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 215, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransferToDetailsGroupBox.ResumeLayout(false);
			this.TransferToDetailsGroupBox.PerformLayout();
			this.QH_TransferExporterLocationDropEdit.ResumeLayout(true);
			this.QH_TransferExporterLocationDropEdit.PerformLayout();
			this.QH_TransferEDIUserLocationDropEdit.ResumeLayout(true);
			this.QH_TransferEDIUserLocationDropEdit.PerformLayout();
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.ResumeLayout(true);
			this.QH_OH_TransferEDIUserLocationOrganisationGuidFindBox.PerformLayout();
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.ResumeLayout(true);
			this.QH_OH_TransferExporterLocationOrganisationGuidFindBox.PerformLayout();
			this.ForwardDetailsGroupBox.ResumeLayout(false);
			this.ForwardDetailsGroupBox.PerformLayout();
			this.QH_ForwardLocationDropEdit.ResumeLayout(true);
			this.QH_ForwardLocationDropEdit.PerformLayout();
			this.QH_ForwardStatusDropEdit.ResumeLayout(true);
			this.QH_ForwardStatusDropEdit.PerformLayout();
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.ResumeLayout(true);
			this.QH_OH_ForwardLocationOrganisationGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

	#endregion

		private ZArchitecture.GUI.ZGroupBox TransferToDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit QH_TransferExporterLocationDropEdit;
		private ZArchitecture.ZTextBox QH_TransfereeEDIUserIdentifierTextBox;
		private ZArchitecture.GUI.ZDropEdit QH_TransferEDIUserLocationDropEdit;
		private ZArchitecture.ZTextBox QH_TransfereeExporterNumberTextBox;
		private ZArchitecture.GUI.ZCheckBox QH_CancelTransferIndicatorCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox QH_OH_TransferEDIUserLocationOrganisationGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox QH_OH_TransferExporterLocationOrganisationGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox ForwardDetailsGroupBox;
		private ZArchitecture.ZTextBox QH_ForwardeeEDIUserIdentifierTextBox;
		private ZArchitecture.GUI.ZDropEdit QH_ForwardLocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit QH_ForwardStatusDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox QH_OH_ForwardLocationOrganisationGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox RequiresAcceptanceCheckbox;
	}
}
