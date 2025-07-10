namespace Enterprise.Customs.DE.GUI
{
	partial class CHGSPODeclarationUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewOwnerReferenceLineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewOwnerReferenceNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewOwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LinesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.DE.GUI.MessagesUserControl();
			this.DeclarationsAndLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DeclarationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.LineDetailsGroupBox.SuspendLayout();
			this.NewOwnerReferenceTypeDropEdit.SuspendLayout();
			this.LinesTabControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsAndLinesSplitContainer)).BeginInit();
			this.DeclarationsAndLinesSplitContainer.Panel1.SuspendLayout();
			this.DeclarationsAndLinesSplitContainer.Panel2.SuspendLayout();
			this.DeclarationsAndLinesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsGrid)).BeginInit();
			this.DeclarationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "CHGSPOCusTempStorageDecs.CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "TSL_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			zTextBoxColumnStyleInfo2.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "65f1cbfd-aaf0-4c42-9799-3a85507997fd";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 332, true);
			this.LinesGrid.TabIndex = 1;
			// 
			// LineDetailsGroupBox
			// 
			this.LineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("73ada148-12dc-45b0-b17f-a466665823da", "Line Details");
			this.LineDetailsGroupBox.Controls.Add(this.NewOwnerReferenceLineNoTextBox);
			this.LineDetailsGroupBox.Controls.Add(this.NewOwnerReferenceNoTextBox);
			this.LineDetailsGroupBox.Controls.Add(this.NewOwnerReferenceTypeDropEdit);
			this.LineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 332, true);
			this.LineDetailsGroupBox.Name = "LineDetailsGroupBox";
			this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 92, true);
			this.LineDetailsGroupBox.TabIndex = 3;
			this.LineDetailsGroupBox.TabStop = false;
			// 
			// NewOwnerReferenceLineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewOwnerReferenceLineNoTextBox, "CHGSPOCusTempStorageDecs.CusTempStorageLines.TSL_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			this.NewOwnerReferenceLineNoTextBox.CaptionResourceString = null;
			this.NewOwnerReferenceLineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 19, true);
			this.NewOwnerReferenceLineNoTextBox.Name = "NewOwnerReferenceLineNoTextBox";
			this.NewOwnerReferenceLineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.NewOwnerReferenceLineNoTextBox.TabIndex = 0;
			// 
			// NewOwnerReferenceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewOwnerReferenceNoTextBox, "CHGSPOCusTempStorageDecs.CusTempStorageLines.TSL_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceNumber)));
			this.NewOwnerReferenceNoTextBox.CaptionResourceString = null;
			this.NewOwnerReferenceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 64, true);
			this.NewOwnerReferenceNoTextBox.Name = "NewOwnerReferenceNoTextBox";
			this.NewOwnerReferenceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.NewOwnerReferenceNoTextBox.TabIndex = 3;
			// 
			// NewOwnerReferenceTypeDropEdit
			// 
			this.NewOwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerReferenceTypeDropEdit, "CHGSPOCusTempStorageDecs.CusTempStorageLines.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			this.NewOwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 42, true);
			this.NewOwnerReferenceTypeDropEdit.Name = "NewOwnerReferenceTypeDropEdit";
			this.NewOwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.NewOwnerReferenceTypeDropEdit.TabIndex = 2;
			// 
			// LinesTabControl
			// 
			this.LinesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LinesTabControl.Controls.Add(this.LinesTabPage);
			this.LinesTabControl.Controls.Add(this.MessagesTabPage);
			this.LinesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesTabControl.Name = "LinesTabControl";
			this.LinesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 451, true);
			this.LinesTabControl.TabIndex = 0;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("e2ea76a5-2fe9-4d72-91cc-9c3f9197ea28", "Lines");
			this.LinesTabPage.Controls.Add(this.LinesGrid);
			this.LinesTabPage.Controls.Add(this.LineDetailsGroupBox);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 424, true);
			this.LinesTabPage.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1a9d5c71-509f-48fb-a693-5352244c7db3", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 424, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "CHGSPOCusTempStorageDecs.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).Messages)));
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 424, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// DeclarationsAndLinesSplitContainer
			// 
			this.DeclarationsAndLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsAndLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsAndLinesSplitContainer.Name = "DeclarationsAndLinesSplitContainer";
			this.DeclarationsAndLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DeclarationsAndLinesSplitContainer.Panel1
			// 
			this.DeclarationsAndLinesSplitContainer.Panel1.Controls.Add(this.DeclarationsGrid);
			// 
			// DeclarationsAndLinesSplitContainer.Panel2
			// 
			this.DeclarationsAndLinesSplitContainer.Panel2.Controls.Add(this.LinesTabControl);
			this.DeclarationsAndLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.DeclarationsAndLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(194);
			this.DeclarationsAndLinesSplitContainer.TabIndex = 0;
			this.DeclarationsAndLinesSplitContainer.TabStop = false;
			// 
			// DeclarationsGrid
			// 
			this.DeclarationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeclarationsGrid, "CHGSPOCusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).STH_IdentificationIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).STH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).STH_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).FormattedOwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).ReferenceNumberColumnFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGSPOCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGSPOCusTempStorageDecs)).SyncRoot)).Lookups.CusTempStorageRegLineCollection)));
			this.DeclarationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "STH_IdentificationIndicator";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
			zDateEditColumnStyleInfo1.ColumnName = "STH_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo3.ColumnName = "STH_MessageStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "Lookups.CusTempStorageRegLineCollection";
			zMultiControlColumnStyleInfo1.ColumnName = "FormattedOwnerReferenceNumber";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ReferenceNumberColumnFieldType";
			zMultiControlColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			this.DeclarationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DeclarationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeclarationsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.DeclarationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsGrid.GridId = "2cee73ba-51c8-46ca-8d40-f152cbb8fb11";
			this.DeclarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationsGrid.LayoutKey = "DeclarationsGrid";
			this.DeclarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsGrid.Name = "DeclarationsGrid";
			this.DeclarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 194, true);
			this.DeclarationsGrid.TabIndex = 0;
			// 
			// CHGSPODeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclarationsAndLinesSplitContainer);
			this.Name = "CHGSPODeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.LineDetailsGroupBox.ResumeLayout(false);
			this.LineDetailsGroupBox.PerformLayout();
			this.NewOwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.NewOwnerReferenceTypeDropEdit.PerformLayout();
			this.LinesTabControl.ResumeLayout(false);
			this.LinesTabControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.DeclarationsAndLinesSplitContainer.Panel1.ResumeLayout(false);
			this.DeclarationsAndLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsAndLinesSplitContainer)).EndInit();
			this.DeclarationsAndLinesSplitContainer.ResumeLayout(false);
			this.DeclarationsAndLinesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsGrid)).EndInit();
			this.DeclarationsGrid.ResumeLayout(false);
			this.DeclarationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KSplitContainer DeclarationsAndLinesSplitContainer;
		internal ZArchitecture.ZGrid DeclarationsGrid;
		internal ZArchitecture.ZGrid LinesGrid;
		internal ZArchitecture.ZTextBox NewOwnerReferenceNoTextBox;
		internal ZArchitecture.GUI.ZDropEdit NewOwnerReferenceTypeDropEdit;
		private ZArchitecture.ZTextBox NewOwnerReferenceLineNoTextBox;
		private ZArchitecture.GUI.ZGroupBox LineDetailsGroupBox;
		internal ZArchitecture.GUI.ZTabPage MessagesTabPage;
		internal ZArchitecture.GUI.ZTabPage LinesTabPage;
		private ZArchitecture.GUI.ZTabControl LinesTabControl;
		private MessagesUserControl MessagesUserControl;
	}
}
