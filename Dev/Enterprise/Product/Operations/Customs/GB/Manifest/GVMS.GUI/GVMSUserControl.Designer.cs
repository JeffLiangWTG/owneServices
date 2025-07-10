using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GVMS.GUI
{
	partial class GVMSUserControl
	{
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.CarrierCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RouteIdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CustomsReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.TransitReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TransitReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.OtherReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.OtherReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.IsUnaccompaniedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.EmptyVehicleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.InspectionRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.InspectionLocationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.InspectionLocationsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.HaulierTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CarrierCodeDropEdit.SuspendLayout();
            this.RouteIdDropEdit.SuspendLayout();
            this.CustomsReferencesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsReferencesGrid)).BeginInit();
            this.CustomsReferencesGrid.SuspendLayout();
            this.TransitReferencesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TransitReferencesGrid)).BeginInit();
            this.TransitReferencesGrid.SuspendLayout();
            this.OtherReferencesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OtherReferencesGrid)).BeginInit();
            this.OtherReferencesGrid.SuspendLayout();
            this.EmptyVehicleDropEdit.SuspendLayout();
            this.InspectionLocationsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InspectionLocationsGrid)).BeginInit();
            this.InspectionLocationsGrid.SuspendLayout();
            this.HaulierTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.GVMS.AsycudaManifestHeader);
            // 
            // CarrierCodeDropEdit
            // 
            this.CarrierCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CarrierCodeDropEdit, "AMA_CarrierCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).AMA_CarrierCode)));
            this.CarrierCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 29, true);
            this.CarrierCodeDropEdit.Name = "CarrierCodeDropEdit";
            this.CarrierCodeDropEdit.PreBoundMaxLength = 10;
            this.CarrierCodeDropEdit.ShouldResizeByMaxLength = false;
            this.CarrierCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
            this.CarrierCodeDropEdit.TabIndex = 1;
            // 
            // RouteIdDropEdit
            // 
            this.RouteIdDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RouteIdDropEdit, "RouteId");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).RouteId)));
            this.RouteIdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 3, true);
            this.RouteIdDropEdit.Name = "RouteIdDropEdit";
            this.RouteIdDropEdit.PreBoundMaxLength = 10;
            this.RouteIdDropEdit.ShouldResizeByMaxLength = false;
            this.RouteIdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
            this.RouteIdDropEdit.TabIndex = 2;
            // 
            // CustomsReferencesGroupBox
            // 
            this.CustomsReferencesGroupBox.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("7ceb86d4-e256-46fc-8465-3a43b7775b10", "Customs References - Customs Declarations");
            this.CustomsReferencesGroupBox.Controls.Add(this.CustomsReferencesGrid);
            this.CustomsReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 127, true);
            this.CustomsReferencesGroupBox.Name = "CustomsReferencesGroupBox";
            this.CustomsReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 174, true);
            this.CustomsReferencesGroupBox.TabIndex = 3;
            this.CustomsReferencesGroupBox.TabStop = false;
            // 
            // CustomsReferencesGrid
            // 
            this.CustomsReferencesGrid.AllowNavigation = false;
            this.CustomsReferencesGrid.AllowReadOnlyRowsToBeDeleted = true;
            this.BindingSource.SetBindingMember(this.CustomsReferencesGrid, "GvmsCustomsReferenceCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsCustomsReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsCustomsReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsCustomsReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)).SyncRoot)).CSI_ReferenceNumber2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.GVMS.GvmsCustomsReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)).SyncRoot)).CSI_DateOfIssue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsCustomsReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsCustomsReferenceCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
            this.CustomsReferencesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("c502b3d7-7ae2-48af-8162-0e434744e450", "Type");
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo1.IsMandatory = true;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("9acddef3-df37-4e71-af65-687da5b726ac", "S&S Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.CustomsReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.CustomsReferencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.CustomsReferencesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.CustomsReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomsReferencesGrid.GridId = "54121fef-b9bc-4121-bf1f-090bf8f14787";
            this.CustomsReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CustomsReferencesGrid.LayoutKey = "CustomsReferencesGrid";
            this.CustomsReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.CustomsReferencesGrid.Name = "CustomsReferencesGrid";
            this.CustomsReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 155, true);
            this.CustomsReferencesGrid.TabIndex = 0;
            // 
            // TransitReferencesGroupBox
            // 
            this.TransitReferencesGroupBox.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("1a65721f-1219-47fa-975e-85083b5bf209", "Customs References - Transit Declarations");
            this.TransitReferencesGroupBox.Controls.Add(this.TransitReferencesGrid);
            this.TransitReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 304, true);
            this.TransitReferencesGroupBox.Name = "TransitReferencesGroupBox";
            this.TransitReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 171, true);
            this.TransitReferencesGroupBox.TabIndex = 4;
            this.TransitReferencesGroupBox.TabStop = false;
            // 
            // TransitReferencesGrid
            // 
            this.TransitReferencesGrid.AllowNavigation = false;
            this.TransitReferencesGrid.AllowReadOnlyRowsToBeDeleted = true;
            this.BindingSource.SetBindingMember(this.TransitReferencesGrid, "GvmsTransitReferenceCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_ReferenceNumber2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_DateOfIssue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).CSI_Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.GvmsTransitReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsTransitReferenceCollection)).SyncRoot)).Lookups.YesNoList)));
            this.TransitReferencesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("a9f591bb-a51d-40fc-b38c-3908e9afb90d", "Type");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo2.IsMandatory = true;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("91947838-bebb-478c-9e70-a9b6a7f99dcc", "S&S Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfIssue";
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo3.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            this.TransitReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransitReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransitReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.TransitReferencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.TransitReferencesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TransitReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.TransitReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransitReferencesGrid.GridId = "2010792f-7bb2-4d9e-a4e1-1d04b400b0d8";
            this.TransitReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TransitReferencesGrid.LayoutKey = "TransitReferencesGrid";
            this.TransitReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.TransitReferencesGrid.Name = "TransitReferencesGrid";
            this.TransitReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 152, true);
            this.TransitReferencesGrid.TabIndex = 0;
            // 
            // OtherReferencesGroupBox
            // 
            this.OtherReferencesGroupBox.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("963b85d1-e1ac-4f92-bb7c-1d4231a89f09", "Customs References - Other");
            this.OtherReferencesGroupBox.Controls.Add(this.OtherReferencesGrid);
            this.OtherReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 478, true);
            this.OtherReferencesGroupBox.Name = "OtherReferencesGroupBox";
            this.OtherReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 164, true);
            this.OtherReferencesGroupBox.TabIndex = 5;
            this.OtherReferencesGroupBox.TabStop = false;
            // 
            // OtherReferencesGrid
            // 
            this.OtherReferencesGrid.AllowNavigation = false;
            this.OtherReferencesGrid.AllowReadOnlyRowsToBeDeleted = true;
            this.BindingSource.SetBindingMember(this.OtherReferencesGrid, "GvmsEidrAndOralReferenceCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_ReferenceNumber2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_DateOfIssue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsEidrReference)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).GvmsEidrAndOralReferenceCollection)).SyncRoot)).CSI_Procedure)));
            this.OtherReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("a9b9a2a0-f0e2-469a-a242-d84be0a60da0", "Type");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("F54E09BE-C928-4C9A-964A-66684A0289BB", "EORI/Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("4a38e4d5-760f-43ab-91fd-36d35c940f4e", "S&S Reference");
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
            zDateEditColumnStyleInfo3.ColumnName = "CSI_DateOfIssue";
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
            zCodeFindBoxColumnStyleInfo3.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("4D0C638C-2DCE-4614-974A-68285E6A0D25", "LRN/Local Reference Number");
			zTextBoxColumnStyleInfo9.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(166);
			zTextBoxColumnStyleInfo10.ColumnName = "CSI_Procedure";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OtherReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OtherReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OtherReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.OtherReferencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.OtherReferencesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.OtherReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OtherReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.OtherReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OtherReferencesGrid.GridId = "925bf374-8916-463d-9015-6e9f3971b628";
            this.OtherReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.OtherReferencesGrid.LayoutKey = "EidrReferencesGrid";
            this.OtherReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.OtherReferencesGrid.Name = "OtherReferencesGrid";
            this.OtherReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 145, true);
            this.OtherReferencesGrid.TabIndex = 0;
            // 
            // IsUnaccompaniedCheckBox
            // 
            this.IsUnaccompaniedCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsUnaccompaniedCheckBox, "IsUnaccompanied");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).IsUnaccompanied)));
            this.IsUnaccompaniedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
            this.IsUnaccompaniedCheckBox.Name = "IsUnaccompaniedCheckBox";
			this.IsUnaccompaniedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 17, true);
            this.IsUnaccompaniedCheckBox.TabIndex = 6;
            this.IsUnaccompaniedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.IsUnaccompaniedCheckBox.UseVisualStyleBackColor = true;
            // 
            // EmptyVehicleDropEdit
            // 
            this.EmptyVehicleDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EmptyVehicleDropEdit, "EmptyVehicle");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).EmptyVehicle)));
            this.EmptyVehicleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 55, true);
            this.EmptyVehicleDropEdit.Name = "EmptyVehicleDropEdit";
            this.EmptyVehicleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
            this.EmptyVehicleDropEdit.TabIndex = 7;
            // 
            // InspectionRequiredCheckBox
            // 
            this.InspectionRequiredCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.InspectionRequiredCheckBox, "InspectionRequired");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).InspectionRequired)));
            this.InspectionRequiredCheckBox.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("e6e354b8-06bc-4c68-a86c-2b1d6b3fe2c8", "Inspection Required");
            this.InspectionRequiredCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
            this.InspectionRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 104, true);
            this.InspectionRequiredCheckBox.Name = "InspectionRequiredCheckBox";
            this.InspectionRequiredCheckBox.ReadOnly = true;
			this.InspectionRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 17, true);
            this.InspectionRequiredCheckBox.TabIndex = 8;
            this.InspectionRequiredCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.InspectionRequiredCheckBox.UseVisualStyleBackColor = true;
            // 
            // InspectionLocationsGroupBox
            // 
            this.InspectionLocationsGroupBox.CaptionResourceString = Enterprise.Customs.GB.GVMS.GUI.Res.GetData("86329d3e-dc0e-493e-9379-6df94f93db48", "Inspection(s) Details");
            this.InspectionLocationsGroupBox.Controls.Add(this.InspectionLocationsGrid);
            this.InspectionLocationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 127, true);
            this.InspectionLocationsGroupBox.Name = "InspectionLocationsGroupBox";
            this.InspectionLocationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 174, true);
            this.InspectionLocationsGroupBox.TabIndex = 9;
            this.InspectionLocationsGroupBox.TabStop = false;
            // 
            // InspectionLocationsGrid
            // 
            this.InspectionLocationsGrid.AllowNavigation = false;
            this.InspectionLocationsGrid.AllowReadOnlyRowsToBeDeleted = true;
            this.BindingSource.SetBindingMember(this.InspectionLocationsGrid, "InspectionLocations");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).InspectionLocations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsInspectionAtLocationCusCodeData)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).InspectionLocations)).SyncRoot)).InspectionType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.GVMS.GvmsInspectionAtLocationCusCodeData)(((System.Collections.IList)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).InspectionLocations)).SyncRoot)).InspectionLocation)));
            this.InspectionLocationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "InspectionType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "InspectionLocation";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.InspectionLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InspectionLocationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.InspectionLocationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InspectionLocationsGrid.GridId = "925bf374-8916-463d-9015-6e9f3971b628";
            this.InspectionLocationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.InspectionLocationsGrid.LayoutKey = "InspectionLocationsGrid";
            this.InspectionLocationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.InspectionLocationsGrid.Name = "InspectionLocationsGrid";
            this.InspectionLocationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 155, true);
            this.InspectionLocationsGrid.TabIndex = 0;
            // 
            // HaulierTypeDropEdit
            // 
            this.HaulierTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HaulierTypeDropEdit, "HaulierType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.GVMS.AsycudaManifestHeader)(null)).HaulierType)));
            this.HaulierTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 3, true);
            this.HaulierTypeDropEdit.Name = "HaulierTypeDropEdit";
            this.HaulierTypeDropEdit.PreBoundMaxLength = 10;
            this.HaulierTypeDropEdit.ShouldResizeByMaxLength = false;
            this.HaulierTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
            this.HaulierTypeDropEdit.TabIndex = 10;
            // 
            // GVMSUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.HaulierTypeDropEdit);
            this.Controls.Add(this.InspectionLocationsGroupBox);
            this.Controls.Add(this.InspectionRequiredCheckBox);
            this.Controls.Add(this.EmptyVehicleDropEdit);
            this.Controls.Add(this.IsUnaccompaniedCheckBox);
            this.Controls.Add(this.OtherReferencesGroupBox);
            this.Controls.Add(this.TransitReferencesGroupBox);
            this.Controls.Add(this.CustomsReferencesGroupBox);
            this.Controls.Add(this.CarrierCodeDropEdit);
            this.Controls.Add(this.RouteIdDropEdit);
            this.Name = "GVMSUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 647, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CarrierCodeDropEdit.ResumeLayout(true);
            this.CarrierCodeDropEdit.PerformLayout();
            this.RouteIdDropEdit.ResumeLayout(true);
            this.RouteIdDropEdit.PerformLayout();
            this.CustomsReferencesGroupBox.ResumeLayout(false);
            this.CustomsReferencesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsReferencesGrid)).EndInit();
            this.CustomsReferencesGrid.ResumeLayout(false);
            this.CustomsReferencesGrid.PerformLayout();
            this.TransitReferencesGroupBox.ResumeLayout(false);
            this.TransitReferencesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TransitReferencesGrid)).EndInit();
            this.TransitReferencesGrid.ResumeLayout(false);
            this.TransitReferencesGrid.PerformLayout();
            this.OtherReferencesGroupBox.ResumeLayout(false);
            this.OtherReferencesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OtherReferencesGrid)).EndInit();
            this.OtherReferencesGrid.ResumeLayout(false);
            this.OtherReferencesGrid.PerformLayout();
            this.EmptyVehicleDropEdit.ResumeLayout(true);
            this.EmptyVehicleDropEdit.PerformLayout();
            this.InspectionLocationsGroupBox.ResumeLayout(false);
            this.InspectionLocationsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InspectionLocationsGrid)).EndInit();
            this.InspectionLocationsGrid.ResumeLayout(false);
            this.InspectionLocationsGrid.PerformLayout();
            this.HaulierTypeDropEdit.ResumeLayout(true);
            this.HaulierTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZDropEdit CarrierCodeDropEdit;
		internal ZDropEdit RouteIdDropEdit;
		internal ZGroupBox CustomsReferencesGroupBox;
		internal ZArchitecture.ZGrid CustomsReferencesGrid;
		internal ZGroupBox TransitReferencesGroupBox;
		internal ZArchitecture.ZGrid TransitReferencesGrid;
		internal ZGroupBox OtherReferencesGroupBox;
		internal ZArchitecture.ZGrid OtherReferencesGrid;
		internal ZCheckBox IsUnaccompaniedCheckBox;
		internal ZDropEdit EmptyVehicleDropEdit;
		internal ZCheckBox InspectionRequiredCheckBox;
		internal ZGroupBox InspectionLocationsGroupBox;
		internal ZGrid InspectionLocationsGrid;
		internal ZDropEdit HaulierTypeDropEdit;
	}
}
