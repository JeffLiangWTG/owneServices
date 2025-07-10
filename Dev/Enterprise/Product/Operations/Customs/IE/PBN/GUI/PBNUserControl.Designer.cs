using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI
{
	partial class PBNUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IsEmptyVehicleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarrierCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TransitReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransitReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PersonsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PersonsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierCodeDropEdit.SuspendLayout();
			this.CustomsReferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsReferencesGrid)).BeginInit();
			this.CustomsReferencesGrid.SuspendLayout();
			this.TransitReferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransitReferencesGrid)).BeginInit();
			this.TransitReferencesGrid.SuspendLayout();
			this.PersonsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PersonsGrid)).BeginInit();
			this.PersonsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader);
			// 
			// IsEmptyVehicleCheckBox
			//
			this.IsEmptyVehicleCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsEmptyVehicleCheckBox, "IsEmptyVehicle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).IsEmptyVehicle)));
			this.IsEmptyVehicleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
			this.IsEmptyVehicleCheckBox.Name = "IsEmptyVehicleCheckBox";
			this.IsEmptyVehicleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 17, true);
			this.IsEmptyVehicleCheckBox.TabIndex = 6;
			this.IsEmptyVehicleCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsEmptyVehicleCheckBox.UseVisualStyleBackColor = true;
			// 
			// CarrierCodeDropEdit
			// 
			this.CarrierCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeDropEdit, "AMA_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).AMA_CarrierCode)));
			this.CarrierCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 29, true);
			this.CarrierCodeDropEdit.Name = "CarrierCodeDropEdit";
			this.CarrierCodeDropEdit.PreBoundMaxLength = 10;
			this.CarrierCodeDropEdit.ShouldResizeByMaxLength = false;
			this.CarrierCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 15, true);
			this.CarrierCodeDropEdit.TabIndex = 1;
			// 
			// CustomsReferencesGroupBox
			// 
			this.CustomsReferencesGroupBox.CaptionResourceString = Enterprise.Customs.IE.PBN.GUI.Res.GetData("33197460-DB58-4545-AD58-8BEFD9B1327D", "Customs References - Customs Declarations");
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
			this.BindingSource.SetBindingMember(this.CustomsReferencesGrid, "CustomsReferenceCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNCustomsDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNCustomsDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNCustomsDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IE.PBN.Business.PBNCustomsDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNCustomsDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).CustomsReferenceCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.CustomsReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IE.PBN.GUI.Res.GetData("52E4200C-150D-4FA1-8A0A-FE97CEDE9AA5", "Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CustomsReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CustomsReferencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsReferencesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsReferencesGrid.GridId = "70599236-9E6A-4340-9496-656243F9F642";
			this.CustomsReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsReferencesGrid.LayoutKey = "CustomsReferencesGrid";
			this.CustomsReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.CustomsReferencesGrid.Name = "CustomsReferencesGrid";
			this.CustomsReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 159, true);
			this.CustomsReferencesGrid.TabIndex = 0;
			// 
			// TransitReferencesGroupBox
			// 
			this.TransitReferencesGroupBox.CaptionResourceString = Enterprise.Customs.IE.PBN.GUI.Res.GetData("3EE242A7-EC5C-4EF6-9DA4-7806CA0AEC08", "Customs References - Transit Declarations");
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
			this.BindingSource.SetBindingMember(this.TransitReferencesGrid, "TransitDeclarationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNTransitDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNTransitDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNTransitDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IE.PBN.Business.PBNTransitDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.PBNTransitDeclarationItem)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).TransitDeclarationCollection)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.TransitReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IE.PBN.GUI.Res.GetData("13E5BDA5-DAE5-464A-BBFA-1D3A1B922ABB", "Type");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransitReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransitReferencesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransitReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TransitReferencesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransitReferencesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TransitReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransitReferencesGrid.GridId = "AB3A61DA-81B6-4FB3-AF50-58C69F285AC2";
			this.TransitReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransitReferencesGrid.LayoutKey = "TransitReferencesGrid";
			this.TransitReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.TransitReferencesGrid.Name = "TransitReferencesGrid";
			this.TransitReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 156, true);
			this.TransitReferencesGrid.TabIndex = 0;
			// 
			// PersonsGroupBox
			// 
			this.PersonsGroupBox.CaptionResourceString = Enterprise.Customs.IE.PBN.GUI.Res.GetData("1534BDB1-6973-4182-A189-80F00F70B0F3", "Persons");
			this.PersonsGroupBox.Controls.Add(this.PersonsGrid);
			this.PersonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 140, true);
			this.PersonsGroupBox.Name = "PersonsGroupBox";
			this.PersonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 160, true);
			this.PersonsGroupBox.TabIndex = 4;
			this.PersonsGroupBox.TabStop = false;
			// 
			// PersonsGrid
			// 
			this.PersonsGrid.AllowNavigation = false;
			this.PersonsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.PersonsGrid, "Persons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).CPN_PER_Person)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).PersonFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).PersonAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).PersonEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).PersonHomePhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.PBN.Business.CusPerson)(((System.Collections.IList)(((Enterprise.Customs.IE.PBN.Business.AsycudaManifestHeader)(null)).Persons)).SyncRoot)).PersonMobilePhone)));
			this.PersonsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CPN_PER_Person";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "PersonFullName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "PersonAddress";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "PersonEmail";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "PersonHomePhone";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "PersonMobilePhone";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PersonsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PersonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PersonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PersonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PersonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PersonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PersonsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PersonsGrid.GridId = "A4F5705E-E56F-42E4-A6E4-258F8F342B52";
			this.PersonsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PersonsGrid.LayoutKey = "PersonsGrid";
			this.PersonsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.PersonsGrid.Name = "PersonsGrid";
			this.PersonsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 145, true);
			this.PersonsGrid.TabIndex = 0;
			this.PersonsGrid.TabStop = false;
			// 
			// PBNUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PersonsGroupBox);
			this.Controls.Add(this.IsEmptyVehicleCheckBox);
			this.Controls.Add(this.TransitReferencesGroupBox);
			this.Controls.Add(this.CustomsReferencesGroupBox);
			this.Controls.Add(this.CarrierCodeDropEdit);
			this.Name = "PBNUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 647, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierCodeDropEdit.ResumeLayout(true);
			this.CarrierCodeDropEdit.PerformLayout();
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
			this.PersonsGroupBox.ResumeLayout(false);
			this.PersonsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PersonsGrid)).EndInit();
			this.PersonsGrid.ResumeLayout(false);
			this.PersonsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZCheckBox IsEmptyVehicleCheckBox;
		internal ZDropEdit CarrierCodeDropEdit;
		internal ZGroupBox CustomsReferencesGroupBox;
		internal ZArchitecture.ZGrid CustomsReferencesGrid;
		internal ZGroupBox TransitReferencesGroupBox;
		internal ZArchitecture.ZGrid TransitReferencesGrid;
		internal ZGroupBox PersonsGroupBox;
		internal ZGrid PersonsGrid;
	}
}
