using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPInspectionDetailsUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.APPGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_LotNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AvAnimalAgeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.QH_OriginCatchZoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.NexDocCatchZonesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NexDocCatchZonesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ApprovedCertifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_InspectionRequestedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.QH_AuthorisedStartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.QH_AuthorisedEndDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.AuthorisedOfficerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_AuthorisingOfficerIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QH_InspectorCommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.StorageEstablishmenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_StorageLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.QH_StorageEstablishmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QH_OA_StorageEstablishmentAddressGuidFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.VesselHoldGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_EndHoldSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QH_StartHoldSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AuthorisationEstablishmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.QH_AuthorisationCommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QH_AuthorisationFlagCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.QH_AuthorisationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.QH_AuthorisationLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.QH_AuthorisationEstablishmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QH_AuthorisationEstablishmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.APPGroupBox.SuspendLayout();
            this.AvAnimalAgeDropEdit.SuspendLayout();
            this.NexDocCatchZonesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NexDocCatchZonesGrid)).BeginInit();
            this.NexDocCatchZonesGrid.SuspendLayout();
            this.ApprovedCertifierDropEdit.SuspendLayout();
            this.zGroupBox3.SuspendLayout();
            this.QH_InspectionRequestedDateDateEdit.SuspendLayout();
            this.QH_AuthorisedStartDateDateEdit.SuspendLayout();
            this.QH_AuthorisedEndDateDateEdit.SuspendLayout();
            this.AuthorisedOfficerGroupBox.SuspendLayout();
            this.StorageEstablishmenGroupBox.SuspendLayout();
            this.QH_StorageLocationDropEdit.SuspendLayout();
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.SuspendLayout();
            this.VesselHoldGroupBox.SuspendLayout();
            this.AuthorisationEstablishmentGroupBox.SuspendLayout();
            this.QH_AuthorisationDateDateEdit.SuspendLayout();
            this.QH_AuthorisationLocationDropEdit.SuspendLayout();
            this.QH_AuthorisationEstablishmentCodeFindBox.SuspendLayout();
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
            // 
            // APPGroupBox
            // 
            this.APPGroupBox.Controls.Add(this.QH_LotNumberTextBox);
            this.APPGroupBox.Controls.Add(this.AvAnimalAgeDropEdit);
            this.APPGroupBox.Controls.Add(this.QH_OriginCatchZoneTextBox);
            this.APPGroupBox.Controls.Add(this.NexDocCatchZonesGroupBox);
            this.APPGroupBox.Controls.Add(this.ApprovedCertifierDropEdit);
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.APPGroupBox, false);
            this.APPGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 187, true);
            this.APPGroupBox.Name = "APPGroupBox";
            this.APPGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 104, true);
            this.APPGroupBox.TabIndex = 8;
            this.APPGroupBox.TabStop = false;
            // 
            // QH_LotNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_LotNumberTextBox, "QuarantineExDocHeader+QH_LotNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_LotNumber)));
            this.QH_LotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 40, true);
            this.QH_LotNumberTextBox.Multiline = true;
            this.QH_LotNumberTextBox.Name = "QH_LotNumberTextBox";
            this.QH_LotNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 37, true);
            this.QH_LotNumberTextBox.TabIndex = 7;
            // 
            // AvAnimalAgeDropEdit
            // 
            this.AvAnimalAgeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AvAnimalAgeDropEdit, "QuarantineExDocHeader+QH_AvAnimalAge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AvAnimalAge)));
            this.AvAnimalAgeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 14, true);
            this.AvAnimalAgeDropEdit.Name = "AvAnimalAgeDropEdit";
            this.AvAnimalAgeDropEdit.ShowDescriptionBox = false;
            this.AvAnimalAgeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.AvAnimalAgeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.AvAnimalAgeDropEdit.TabIndex = 8;
            // 
            // QH_OriginCatchZoneTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_OriginCatchZoneTextBox, "QuarantineExDocHeader+QH_OriginCatchZone");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OriginCatchZone)));
            this.QH_OriginCatchZoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 40, true);
            this.QH_OriginCatchZoneTextBox.Multiline = true;
            this.QH_OriginCatchZoneTextBox.Name = "QH_OriginCatchZoneTextBox";
            this.QH_OriginCatchZoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 37, true);
            this.QH_OriginCatchZoneTextBox.TabIndex = 9;
            // 
            // NexDocCatchZonesGroupBox
            // 
            this.NexDocCatchZonesGroupBox.Controls.Add(this.NexDocCatchZonesGrid);
            this.NexDocCatchZonesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 33, true);
            this.NexDocCatchZonesGroupBox.Name = "NexDocCatchZonesGroupBox";
            this.NexDocCatchZonesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 69, true);
            this.NexDocCatchZonesGroupBox.TabIndex = 9;
            this.NexDocCatchZonesGroupBox.TabStop = false;
            this.NexDocCatchZonesGroupBox.Text = "Catch Zones";
            // 
            // NexDocCatchZonesGrid
            // 
            this.NexDocCatchZonesGrid.AllowNavigation = false;
            this.NexDocCatchZonesGrid.AllowSorting = false;
            this.NexDocCatchZonesGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.NexDocCatchZonesGrid, "QuarantineExDocHeader.NexDocCatchZones");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.NexDocCatchZones)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineCatchZone)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.NexDocCatchZones)).SyncRoot)).CY_Data)));
            this.NexDocCatchZonesGrid.CaptionVisible = false;
            this.NexDocCatchZonesGrid.ColumnHeadersVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsSortable = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(222);
            this.NexDocCatchZonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.NexDocCatchZonesGrid.GridId = "7e044af8-4ec5-41af-a571-fc15c25002a2";
            this.NexDocCatchZonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.NexDocCatchZonesGrid.IsCustomiseMenuVisible = false;
            this.NexDocCatchZonesGrid.LayoutKey = "NexDocCatchZonesGrid";
            this.NexDocCatchZonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
            this.NexDocCatchZonesGrid.Name = "NexDocCatchZonesGrid";
            this.NexDocCatchZonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 52, true);
            this.NexDocCatchZonesGrid.TabIndex = 0;
            // 
            // ApprovedCertifierDropEdit
            // 
            this.ApprovedCertifierDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ApprovedCertifierDropEdit, "QuarantineExDocHeader+QH_ApprovedCertifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ApprovedCertifier)));
            this.ApprovedCertifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 14, true);
            this.ApprovedCertifierDropEdit.Name = "ApprovedCertifierDropEdit";
            this.ApprovedCertifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 15, true);
            this.ApprovedCertifierDropEdit.TabIndex = 6;
            // 
            // zGroupBox3
            // 
            this.zGroupBox3.Controls.Add(this.QH_InspectionRequestedDateDateEdit);
            this.zGroupBox3.Controls.Add(this.QH_AuthorisedStartDateDateEdit);
            this.zGroupBox3.Controls.Add(this.QH_AuthorisedEndDateDateEdit);
            this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(655, 93, true);
            this.zGroupBox3.Name = "zGroupBox3";
            this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 176, true);
            this.zGroupBox3.TabIndex = 10;
            this.zGroupBox3.TabStop = false;
            this.zGroupBox3.Text = "Inspection Process";
            // 
            // QH_InspectionRequestedDateDateEdit
            // 
            this.QH_InspectionRequestedDateDateEdit.AllowDrop = true;
            this.QH_InspectionRequestedDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.QH_InspectionRequestedDateDateEdit, "QuarantineExDocHeader+QH_InspectionRequestedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_InspectionRequestedDate)));
            this.QH_InspectionRequestedDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.QH_InspectionRequestedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 22, true);
            this.QH_InspectionRequestedDateDateEdit.Name = "QH_InspectionRequestedDateDateEdit";
            this.QH_InspectionRequestedDateDateEdit.TabIndex = 1;
            // 
            // QH_AuthorisedStartDateDateEdit
            // 
            this.QH_AuthorisedStartDateDateEdit.AllowDrop = true;
            this.QH_AuthorisedStartDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.QH_AuthorisedStartDateDateEdit, "QuarantineExDocHeader+QH_AuthorisedStartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisedStartDate)));
            this.QH_AuthorisedStartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 47, true);
            this.QH_AuthorisedStartDateDateEdit.Name = "QH_AuthorisedStartDateDateEdit";
            this.QH_AuthorisedStartDateDateEdit.TabIndex = 3;
            // 
            // QH_AuthorisedEndDateDateEdit
            // 
            this.QH_AuthorisedEndDateDateEdit.AllowDrop = true;
            this.QH_AuthorisedEndDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.QH_AuthorisedEndDateDateEdit, "QuarantineExDocHeader+QH_AuthorisedEndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisedEndDate)));
            this.QH_AuthorisedEndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 72, true);
            this.QH_AuthorisedEndDateDateEdit.Name = "QH_AuthorisedEndDateDateEdit";
            this.QH_AuthorisedEndDateDateEdit.TabIndex = 5;
            // 
            // AuthorisedOfficerGroupBox
            // 
            this.AuthorisedOfficerGroupBox.Controls.Add(this.QH_AuthorisingOfficerIDTextBox);
            this.AuthorisedOfficerGroupBox.Controls.Add(this.QH_InspectorCommentsTextBox);
            this.AuthorisedOfficerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(925, 7, true);
            this.AuthorisedOfficerGroupBox.Name = "AuthorisedOfficerGroupBox";
            this.AuthorisedOfficerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 262, true);
            this.AuthorisedOfficerGroupBox.TabIndex = 11;
            this.AuthorisedOfficerGroupBox.TabStop = false;
            this.AuthorisedOfficerGroupBox.Text = "Authorised Officer";
            // 
            // QH_AuthorisingOfficerIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_AuthorisingOfficerIDTextBox, "QuarantineExDocHeader+QH_AuthorisingOfficerID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisingOfficerID)));
            this.QH_AuthorisingOfficerIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 17, true);
            this.QH_AuthorisingOfficerIDTextBox.Name = "QH_AuthorisingOfficerIDTextBox";
            this.QH_AuthorisingOfficerIDTextBox.PasswordChar = '*';
            this.QH_AuthorisingOfficerIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 15, true);
            this.QH_AuthorisingOfficerIDTextBox.TabIndex = 1;
            // 
            // QH_InspectorCommentsTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_InspectorCommentsTextBox, "QuarantineExDocHeader+QH_InspectorComments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_InspectorComments)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QH_InspectorCommentsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.QH_InspectorCommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 57, true);
            this.QH_InspectorCommentsTextBox.Multiline = true;
            this.QH_InspectorCommentsTextBox.Name = "QH_InspectorCommentsTextBox";
            this.QH_InspectorCommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 120, true);
            this.QH_InspectorCommentsTextBox.TabIndex = 3;
            // 
            // StorageEstablishmenGroupBox
            // 
            this.StorageEstablishmenGroupBox.Controls.Add(this.QH_StorageLocationDropEdit);
            this.StorageEstablishmenGroupBox.Controls.Add(this.QH_StorageEstablishmentTextBox);
            this.StorageEstablishmenGroupBox.Controls.Add(this.QH_OA_StorageEstablishmentAddressGuidFindBox);
            this.StorageEstablishmenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 131, true);
            this.StorageEstablishmenGroupBox.Name = "StorageEstablishmenGroupBox";
            this.StorageEstablishmenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 50, true);
            this.StorageEstablishmenGroupBox.TabIndex = 7;
            this.StorageEstablishmenGroupBox.TabStop = false;
            this.StorageEstablishmenGroupBox.Text = "Storage Establishment";
            // 
            // QH_StorageLocationDropEdit
            // 
            this.QH_StorageLocationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QH_StorageLocationDropEdit, "QuarantineExDocHeader+QH_StorageLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_StorageLocation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.Location)));
            this.QH_StorageLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+Location";
            this.QH_StorageLocationDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e7c556cd-1fa8-4b06-a184-ae0e83e1e162", "Location");
            this.QH_StorageLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 20, true);
            this.QH_StorageLocationDropEdit.Name = "QH_StorageLocationDropEdit";
            this.QH_StorageLocationDropEdit.ShowDescriptionBox = false;
            this.QH_StorageLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.QH_StorageLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.QH_StorageLocationDropEdit.TabIndex = 0;
            // 
            // QH_StorageEstablishmentTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_StorageEstablishmentTextBox, "QuarantineExDocHeader+QH_StorageEstablishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_StorageEstablishment)));
            this.QH_StorageEstablishmentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("61bac394-2405-494e-8203-44afccf0eb0a", "Establishment");
            this.QH_StorageEstablishmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
            this.QH_StorageEstablishmentTextBox.Name = "QH_StorageEstablishmentTextBox";
            this.QH_StorageEstablishmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.QH_StorageEstablishmentTextBox.TabIndex = 1;
            // 
            // QH_OA_StorageEstablishmentAddressGuidFindBox
            // 
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QH_OA_StorageEstablishmentAddressGuidFindBox, "QuarantineExDocHeader+QH_OA_StorageEstablishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OA_StorageEstablishment)));
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.BindToOrgList = "Invoices.QuarantineExDocHeader+Lookups+Establishment";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_OA_StorageEstablishmentAddressGuidFindBox, false);
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.Name = "QH_OA_StorageEstablishmentAddressGuidFindBox";
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.PopupCaption = "";
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.ShowAddress = false;
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 15, true);
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.TabIndex = 2;
            // 
            // VesselHoldGroupBox
            // 
            this.VesselHoldGroupBox.Controls.Add(this.QH_EndHoldSealTextBox);
            this.VesselHoldGroupBox.Controls.Add(this.QH_StartHoldSealTextBox);
            this.VesselHoldGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(655, 7, true);
            this.VesselHoldGroupBox.Name = "VesselHoldGroupBox";
            this.VesselHoldGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 80, true);
            this.VesselHoldGroupBox.TabIndex = 9;
            this.VesselHoldGroupBox.TabStop = false;
            this.VesselHoldGroupBox.Text = "Vessel Hold";
            // 
            // QH_EndHoldSealTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_EndHoldSealTextBox, "QuarantineExDocHeader+QH_EndHoldSeal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_EndHoldSeal)));
            this.QH_EndHoldSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 46, true);
            this.QH_EndHoldSealTextBox.Name = "QH_EndHoldSealTextBox";
            this.QH_EndHoldSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 15, true);
            this.QH_EndHoldSealTextBox.TabIndex = 3;
            // 
            // QH_StartHoldSealTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_StartHoldSealTextBox, "QuarantineExDocHeader+QH_StartHoldSeal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_StartHoldSeal)));
            this.QH_StartHoldSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 20, true);
            this.QH_StartHoldSealTextBox.Name = "QH_StartHoldSealTextBox";
            this.QH_StartHoldSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 15, true);
            this.QH_StartHoldSealTextBox.TabIndex = 1;
            // 
            // AuthorisationEstablishmentGroupBox
            // 
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationCommentsTextBox);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationFlagCheckBox);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationDateDateEdit);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationLocationDropEdit);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationEstablishmentTextBox);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_AuthorisationEstablishmentCodeFindBox);
            this.AuthorisationEstablishmentGroupBox.Controls.Add(this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox);
            this.AuthorisationEstablishmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
            this.AuthorisationEstablishmentGroupBox.Name = "AuthorisationEstablishmentGroupBox";
            this.AuthorisationEstablishmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 117, true);
            this.AuthorisationEstablishmentGroupBox.TabIndex = 6;
            this.AuthorisationEstablishmentGroupBox.TabStop = false;
            this.AuthorisationEstablishmentGroupBox.Text = "Authorisation Establishment";
            // 
            // QH_AuthorisationCommentsTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_AuthorisationCommentsTextBox, "QuarantineExDocHeader+QH_AuthorisationComments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationComments)));
            this.QH_AuthorisationCommentsTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("6791f99c-c03d-424a-9690-a4539ecf6935", "Comments");
            this.QH_AuthorisationCommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 70, true);
            this.QH_AuthorisationCommentsTextBox.Multiline = true;
            this.QH_AuthorisationCommentsTextBox.Name = "QH_AuthorisationCommentsTextBox";
            this.QH_AuthorisationCommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 41, true);
            this.QH_AuthorisationCommentsTextBox.TabIndex = 5;
            // 
            // QH_AuthorisationFlagCheckBox
            // 
            this.BindingSource.SetBindingMember(this.QH_AuthorisationFlagCheckBox, "QuarantineExDocHeader+QH_AuthorisationFlag");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationFlag)));
            this.QH_AuthorisationFlagCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 44, true);
            this.QH_AuthorisationFlagCheckBox.Name = "QH_AuthorisationFlagCheckBox";
            this.QH_AuthorisationFlagCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
            this.QH_AuthorisationFlagCheckBox.TabIndex = 6;
            this.QH_AuthorisationFlagCheckBox.UseVisualStyleBackColor = true;
            // 
            // QH_AuthorisationDateDateEdit
            // 
            this.QH_AuthorisationDateDateEdit.AllowDrop = true;
            this.QH_AuthorisationDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.QH_AuthorisationDateDateEdit, "QuarantineExDocHeader+QH_AuthorisationDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationDate)));
            this.QH_AuthorisationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 44, true);
            this.QH_AuthorisationDateDateEdit.Name = "QH_AuthorisationDateDateEdit";
            this.QH_AuthorisationDateDateEdit.TabIndex = 4;
            // 
            // QH_AuthorisationLocationDropEdit
            // 
            this.QH_AuthorisationLocationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QH_AuthorisationLocationDropEdit, "QuarantineExDocHeader+QH_AuthorisationLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationLocation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.LocationWithAqisPlace)));
            this.QH_AuthorisationLocationDropEdit.BindToList = "QuarantineExDocHeader+Lookups+LocationWithAqisPlace";
            this.QH_AuthorisationLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 20, true);
            this.QH_AuthorisationLocationDropEdit.Name = "QH_AuthorisationLocationDropEdit";
            this.QH_AuthorisationLocationDropEdit.ShowDescriptionBox = false;
            this.QH_AuthorisationLocationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.QH_AuthorisationLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.QH_AuthorisationLocationDropEdit.TabIndex = 0;
            // 
            // QH_AuthorisationEstablishmentTextBox
            // 
            this.BindingSource.SetBindingMember(this.QH_AuthorisationEstablishmentTextBox, "QuarantineExDocHeader+QH_AuthorisationEstablishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationEstablishment)));
            this.QH_AuthorisationEstablishmentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("aed99499-ecab-42c5-b76a-471f34629ba6", "Location Code of the Inspection place.");
            this.QH_AuthorisationEstablishmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
            this.QH_AuthorisationEstablishmentTextBox.Name = "QH_AuthorisationEstablishmentTextBox";
            this.QH_AuthorisationEstablishmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.QH_AuthorisationEstablishmentTextBox.TabIndex = 3;
            // 
            // QH_AuthorisationEstablishmentCodeFindBox
            // 
            this.QH_AuthorisationEstablishmentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QH_AuthorisationEstablishmentCodeFindBox, "QuarantineExDocHeader+QH_AuthorisationEstablishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_AuthorisationEstablishment)));
            this.QH_AuthorisationEstablishmentCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("C8283454-E9C2-4D6F-BE84-070C4C962B30", "Location Code of the Quarantine Office.");
            this.QH_AuthorisationEstablishmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
            this.QH_AuthorisationEstablishmentCodeFindBox.Name = "QH_AuthorisationEstablishmentCodeFindBox";
            this.QH_AuthorisationEstablishmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.QH_AuthorisationEstablishmentCodeFindBox.ParentType = null;
            this.QH_AuthorisationEstablishmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 15, true);
            this.QH_AuthorisationEstablishmentCodeFindBox.TabIndex = 1;
            // 
            // QH_OA_AuthorisationEstablishmentAddressGuidFindBox
            // 
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox, "QuarantineExDocHeader+QH_OA_AuthorisationEstablishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_OA_AuthorisationEstablishment)));
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.BindToOrgList = "Invoices.QuarantineExDocHeader+Lookups+Establishment";
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox, false);
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.Name = "QH_OA_AuthorisationEstablishmentAddressGuidFindBox";
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.PopupCaption = "";
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.ShowAddress = false;
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 15, true);
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.TabIndex = 2;
            // 
            // RFPInspectionDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.APPGroupBox);
            this.Controls.Add(this.zGroupBox3);
            this.Controls.Add(this.AuthorisedOfficerGroupBox);
            this.Controls.Add(this.StorageEstablishmenGroupBox);
            this.Controls.Add(this.VesselHoldGroupBox);
            this.Controls.Add(this.AuthorisationEstablishmentGroupBox);
            this.Name = "RFPInspectionDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 397, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.APPGroupBox.ResumeLayout(false);
            this.APPGroupBox.PerformLayout();
            this.AvAnimalAgeDropEdit.ResumeLayout(true);
            this.AvAnimalAgeDropEdit.PerformLayout();
            this.NexDocCatchZonesGroupBox.ResumeLayout(false);
            this.NexDocCatchZonesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NexDocCatchZonesGrid)).EndInit();
            this.NexDocCatchZonesGrid.ResumeLayout(false);
            this.NexDocCatchZonesGrid.PerformLayout();
            this.ApprovedCertifierDropEdit.ResumeLayout(true);
            this.ApprovedCertifierDropEdit.PerformLayout();
            this.zGroupBox3.ResumeLayout(false);
            this.zGroupBox3.PerformLayout();
            this.QH_InspectionRequestedDateDateEdit.ResumeLayout(true);
            this.QH_InspectionRequestedDateDateEdit.PerformLayout();
            this.QH_AuthorisedStartDateDateEdit.ResumeLayout(true);
            this.QH_AuthorisedStartDateDateEdit.PerformLayout();
            this.QH_AuthorisedEndDateDateEdit.ResumeLayout(true);
            this.QH_AuthorisedEndDateDateEdit.PerformLayout();
            this.AuthorisedOfficerGroupBox.ResumeLayout(false);
            this.AuthorisedOfficerGroupBox.PerformLayout();
            this.StorageEstablishmenGroupBox.ResumeLayout(false);
            this.StorageEstablishmenGroupBox.PerformLayout();
            this.QH_StorageLocationDropEdit.ResumeLayout(true);
            this.QH_StorageLocationDropEdit.PerformLayout();
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.ResumeLayout(true);
            this.QH_OA_StorageEstablishmentAddressGuidFindBox.PerformLayout();
            this.VesselHoldGroupBox.ResumeLayout(false);
            this.VesselHoldGroupBox.PerformLayout();
            this.AuthorisationEstablishmentGroupBox.ResumeLayout(false);
            this.AuthorisationEstablishmentGroupBox.PerformLayout();
            this.QH_AuthorisationDateDateEdit.ResumeLayout(true);
            this.QH_AuthorisationDateDateEdit.PerformLayout();
            this.QH_AuthorisationLocationDropEdit.ResumeLayout(true);
            this.QH_AuthorisationLocationDropEdit.PerformLayout();
            this.QH_AuthorisationEstablishmentCodeFindBox.ResumeLayout(true);
            this.QH_AuthorisationEstablishmentCodeFindBox.PerformLayout();
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.ResumeLayout(true);
            this.QH_OA_AuthorisationEstablishmentAddressGuidFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox APPGroupBox;
		private ZArchitecture.ZTextBox QH_LotNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit AvAnimalAgeDropEdit;
		private ZArchitecture.ZTextBox QH_OriginCatchZoneTextBox;
		private ZArchitecture.ZGrid NexDocCatchZonesGrid;
		private ZArchitecture.GUI.ZGroupBox NexDocCatchZonesGroupBox;
		private ZArchitecture.GUI.ZDropEdit ApprovedCertifierDropEdit;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private ZArchitecture.GUI.ZDateEdit QH_InspectionRequestedDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit QH_AuthorisedStartDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit QH_AuthorisedEndDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox AuthorisedOfficerGroupBox;
		private ZArchitecture.ZTextBox QH_AuthorisingOfficerIDTextBox;
		private ZArchitecture.ZTextBox QH_InspectorCommentsTextBox;
		private ZArchitecture.GUI.ZGroupBox StorageEstablishmenGroupBox;
		private ZArchitecture.GUI.ZDropEdit QH_StorageLocationDropEdit;
		private ZArchitecture.ZTextBox QH_StorageEstablishmentTextBox;
		private ZArchitecture.GUI.ZAddressControl QH_OA_StorageEstablishmentAddressGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox VesselHoldGroupBox;
		private ZArchitecture.ZTextBox QH_EndHoldSealTextBox;
		private ZArchitecture.ZTextBox QH_StartHoldSealTextBox;
		private ZArchitecture.GUI.ZGroupBox AuthorisationEstablishmentGroupBox;
		private ZArchitecture.ZTextBox QH_AuthorisationCommentsTextBox;
		private ZArchitecture.GUI.ZDateEdit QH_AuthorisationDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit QH_AuthorisationLocationDropEdit;
		private ZArchitecture.ZTextBox QH_AuthorisationEstablishmentTextBox;
		private ZArchitecture.GUI.ZCodeFindBox QH_AuthorisationEstablishmentCodeFindBox;
		private ZArchitecture.GUI.ZAddressControl QH_OA_AuthorisationEstablishmentAddressGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox QH_AuthorisationFlagCheckBox;
	}
}
