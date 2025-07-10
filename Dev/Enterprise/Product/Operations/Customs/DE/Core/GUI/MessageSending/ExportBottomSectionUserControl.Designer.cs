namespace Enterprise.Customs.DE.GUI
{
	partial class ExportBottomSectionUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AnnotationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AnnotationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlternativeEvidenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AlternativeEvidenceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExitInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotSubmitConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportCustomsOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AnnotationAndExitInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AdditionalWarningUserControl = new Enterprise.Customs.GUI.MessageSendingFormBottomSectionUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AnnotationGroupBox.SuspendLayout();
			this.AlternativeEvidenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).BeginInit();
			this.AlternativeEvidenceGrid.SuspendLayout();
			this.ExitInfoPanel.SuspendLayout();
			this.SecurityDropEdit.SuspendLayout();
			this.ExitCustomsOfficeCodeFindBox.SuspendLayout();
			this.ExitDateEdit.SuspendLayout();
			this.ExitTypeDropEdit.SuspendLayout();
			this.EntryLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).BeginInit();
			this.EntryLinesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.AnnotationAndExitInfoPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.AdditionalWarningUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent);
			// 
			// AnnotationGroupBox
			// 
			this.AnnotationGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7d2929a9-5a3e-4b0d-a010-f3d2fcf1e247", "Annotation");
			this.AnnotationGroupBox.Controls.Add(this.AnnotationTextBox);
			this.AnnotationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.AnnotationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnnotationGroupBox.Name = "AnnotationGroupBox";
			this.AnnotationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 170, true);
			this.AnnotationGroupBox.TabIndex = 0;
			this.AnnotationGroupBox.TabStop = false;
			// 
			// AnnotationTextBox
			// 
			this.BindingSource.SetBindingMember(this.AnnotationTextBox, "SendingObjectsCollection.Annotation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Annotation)));
			this.AnnotationTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("A63F9C06-099E-48AE-8D64-3850732B9689", "Annotation");
			this.AnnotationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AnnotationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AnnotationTextBox, false);
			this.AnnotationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AnnotationTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 141, true);
			this.AnnotationTextBox.Multiline = true;
			this.AnnotationTextBox.Name = "AnnotationTextBox";
			this.AnnotationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 151, true);
			this.AnnotationTextBox.TabIndex = 0;
			// 
			// AlternativeEvidenceGroupBox
			// 
			this.AlternativeEvidenceGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("0B644092-8EF8-4C64-9570-63C9B6C1EFBE", "Alternative Evidence");
			this.AlternativeEvidenceGroupBox.Controls.Add(this.AlternativeEvidenceGrid);
			this.AlternativeEvidenceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlternativeEvidenceGroupBox.Name = "AlternativeEvidenceGroupBox";
			this.AlternativeEvidenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 84, true);
			this.AlternativeEvidenceGroupBox.TabIndex = 0;
			this.AlternativeEvidenceGroupBox.TabStop = false;
			// 
			// AlternativeEvidenceGrid
			// 
			this.AlternativeEvidenceGrid.AllowNavigation = false;
			this.AlternativeEvidenceGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.AlternativeEvidenceGrid, "SendingObjectsCollection.AlternativeEvidences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).EvidenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).Reference)));
			this.AlternativeEvidenceGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EvidenceType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.ColumnName = "DocType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AlternativeEvidenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidenceGrid.GridId = "C1D80A39-7125-4469-9359-3BEDAB1A8AB9";
			this.AlternativeEvidenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternativeEvidenceGrid.LayoutKey = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AlternativeEvidenceGrid.Name = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 65, true);
			this.AlternativeEvidenceGrid.TabIndex = 0;
			// 
			// ExitInfoPanel
			// 
			this.ExitInfoPanel.Controls.Add(this.SecurityDropEdit);
			this.ExitInfoPanel.Controls.Add(this.NotSubmitConsigneeCheckBox);
			this.ExitInfoPanel.Controls.Add(this.ExportCustomsOfficeTextBox);
			this.ExitInfoPanel.Controls.Add(this.ExitCustomsOfficeCodeFindBox);
			this.ExitInfoPanel.Controls.Add(this.ExitDateEdit);
			this.ExitInfoPanel.Controls.Add(this.ExitTypeDropEdit);
			this.ExitInfoPanel.Controls.Add(this.MovementReferenceNumberTextBox);
			this.ExitInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExitInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 0, true);
			this.ExitInfoPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 160, true);
			this.ExitInfoPanel.Name = "ExitInfoPanel";
			this.ExitInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 170, true);
			this.ExitInfoPanel.TabIndex = 1;
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "SendingObjectsCollection.SecurityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).SecurityType)));
			this.SecurityDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("00b9ac52-58a1-4e3c-b09e-92d6fdd1c951", "Security");
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 94, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.SecurityDropEdit.TabIndex = 5;
			// 
			// NotSubmitConsigneeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NotSubmitConsigneeCheckBox, "SendingObjectsCollection.NotSubmitConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).NotSubmitConsignee)));
			this.NotSubmitConsigneeCheckBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("33363d88-bd65-439e-a164-82ee3f6322ef", "Don\'t submit Consignee");
			this.NotSubmitConsigneeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NotSubmitConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 114, true);
			this.NotSubmitConsigneeCheckBox.Name = "NotSubmitConsigneeCheckBox";
			this.NotSubmitConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 17, true);
			this.NotSubmitConsigneeCheckBox.TabIndex = 6;
			this.NotSubmitConsigneeCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.NotSubmitConsigneeCheckBox.Visible = false;
			// 
			// ExportCustomsOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCustomsOfficeTextBox, "SendingObjectsCollection.ExportCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExportCustomsOffice)));
			this.ExportCustomsOfficeTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("a7350a98-35d6-49ac-b36d-24241d91a0db", "Export Customs Office");
			this.ExportCustomsOfficeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 52, true);
			this.ExportCustomsOfficeTextBox.Name = "ExportCustomsOfficeTextBox";
			this.ExportCustomsOfficeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ExportCustomsOfficeTextBox.TabIndex = 3;
			// 
			// ExitCustomsOfficeCodeFindBox
			// 
			this.ExitCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitCustomsOfficeCodeFindBox, "SendingObjectsCollection.ExitCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitCustomsOffice)));
			this.ExitCustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2e097fea-a6ce-4f9d-a614-b19d8e036b7e", "Exit Customs Office");
			this.ExitCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 73, true);
			this.ExitCustomsOfficeCodeFindBox.Name = "ExitCustomsOfficeCodeFindBox";
			this.ExitCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExitCustomsOfficeCodeFindBox.ParentType = null;
			this.ExitCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ExitCustomsOfficeCodeFindBox.TabIndex = 4;
			// 
			// ExitDateEdit
			// 
			this.ExitDateEdit.AllowDrop = true;
			this.ExitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ExitDateEdit, "SendingObjectsCollection.ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitDate)));
			this.ExitDateEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ea8cae0f-b08e-41b1-8b65-6b3dca85e2ab", "Exit Date");
			this.ExitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 31, true);
			this.ExitDateEdit.Name = "ExitDateEdit";
			this.ExitDateEdit.TabIndex = 1;
			// 
			// ExitTypeDropEdit
			// 
			this.ExitTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitTypeDropEdit, "SendingObjectsCollection.ExitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitType)));
			this.ExitTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("e77e40bf-4d48-4176-98a1-d734366b87f2", "Exit Type");
			this.ExitTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 10, true);
			this.ExitTypeDropEdit.Name = "ExitTypeDropEdit";
			this.ExitTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ExitTypeDropEdit.TabIndex = 0;
			// 
			// MovementReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MovementReferenceNumberTextBox, "SendingObjectsCollection.MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MovementReferenceNumber)));
			this.MovementReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2E381893-50FC-44E4-A486-9AD77C1CDBF9", "MRN");
			this.MovementReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 131, true);
			this.MovementReferenceNumberTextBox.Name = "MovementReferenceNumberTextBox";
			this.MovementReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.MovementReferenceNumberTextBox.TabIndex = 7;
			this.MovementReferenceNumberTextBox.Visible = false;
			// 
			// EntryLinesGroupBox
			// 
			this.EntryLinesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("77e804ed-84f2-4318-b262-95011b002ba7", "Entry Lines to be sent");
			this.EntryLinesGroupBox.Controls.Add(this.EntryLinesGrid);
			this.EntryLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLinesGroupBox.Name = "EntryLinesGroupBox";
			this.EntryLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 88, true);
			this.EntryLinesGroupBox.TabIndex = 2;
			this.EntryLinesGroupBox.TabStop = false;
			// 
			// EntryLinesGrid
			// 
			this.EntryLinesGrid.AllowNavigation = false;
			this.EntryLinesGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.EntryLinesGrid, "SendingObjectsCollection.EntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).EntryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).EntryLines)).SyncRoot)).ShouldSend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).EntryLines)).SyncRoot)).LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).EntryLines)).SyncRoot)).Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).EntryLines)).SyncRoot)).Description)));
			this.EntryLinesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "LineNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "Tariff";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EntryLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesGrid.GridId = "C1D80A39-7125-4469-9359-3BEDAB1A8AB9";
			this.EntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLinesGrid.LayoutKey = "EntryLinesGrid";
			this.EntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLinesGrid.Name = "EntryLinesGrid";
			this.EntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 69, true);
			this.EntryLinesGrid.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
			this.SplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 170, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.AlternativeEvidenceGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 176, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.EntryLinesGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(84);
			this.SplitContainer.TabIndex = 1;
			// 
			// AnnotationAndExitInfoPanel
			// 
			this.AnnotationAndExitInfoPanel.Controls.Add(this.ExitInfoPanel);
			this.AnnotationAndExitInfoPanel.Controls.Add(this.AnnotationGroupBox);
			this.AnnotationAndExitInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnnotationAndExitInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.AnnotationAndExitInfoPanel.Name = "AnnotationAndExitInfoPanel";
			this.AnnotationAndExitInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 170, true);
			this.AnnotationAndExitInfoPanel.TabIndex = 8;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.AnnotationAndExitInfoPanel);
			this.TopPanel.Controls.Add(this.AdditionalWarningUserControl);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 250, true);
			this.TopPanel.TabIndex = 0;
			// 
			// AdditionalWarningUserControl
			// 
			this.AdditionalWarningUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalWarningUserControl, ".");
			this.AdditionalWarningUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalWarningUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalWarningUserControl.Name = "AdditionalWarningUserControl";
			this.AdditionalWarningUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 80, true);
			this.AdditionalWarningUserControl.TabIndex = 4;
			// 
			// ExportBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 330, true);
			this.Name = "ExportBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 438, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AnnotationGroupBox.ResumeLayout(false);
			this.AnnotationGroupBox.PerformLayout();
			this.AlternativeEvidenceGroupBox.ResumeLayout(false);
			this.AlternativeEvidenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).EndInit();
			this.AlternativeEvidenceGrid.ResumeLayout(false);
			this.AlternativeEvidenceGrid.PerformLayout();
			this.ExitInfoPanel.ResumeLayout(false);
			this.ExitInfoPanel.PerformLayout();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.ExitCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.ExitCustomsOfficeCodeFindBox.PerformLayout();
			this.ExitDateEdit.ResumeLayout(true);
			this.ExitDateEdit.PerformLayout();
			this.ExitTypeDropEdit.ResumeLayout(true);
			this.ExitTypeDropEdit.PerformLayout();
			this.EntryLinesGroupBox.ResumeLayout(false);
			this.EntryLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).EndInit();
			this.EntryLinesGrid.ResumeLayout(false);
			this.EntryLinesGrid.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.AnnotationAndExitInfoPanel.ResumeLayout(false);
			this.AnnotationAndExitInfoPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.AdditionalWarningUserControl.ResumeLayout(true);
			this.AdditionalWarningUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox AnnotationGroupBox;
		ZArchitecture.ZTextBox AnnotationTextBox;
		ZArchitecture.GUI.ZPanel ExitInfoPanel;
		ZArchitecture.GUI.ZCodeFindBox ExitCustomsOfficeCodeFindBox;
		ZArchitecture.GUI.ZDateEdit ExitDateEdit;
		ZArchitecture.GUI.ZDropEdit ExitTypeDropEdit;
		ZArchitecture.ZTextBox ExportCustomsOfficeTextBox;
		ZArchitecture.ZTextBox MovementReferenceNumberTextBox;
		ZArchitecture.GUI.ZCheckBox NotSubmitConsigneeCheckBox;
		ZArchitecture.GUI.ZDropEdit SecurityDropEdit;
		ZArchitecture.GUI.ZGroupBox AlternativeEvidenceGroupBox;
		Enterprise.ZArchitecture.ZGrid AlternativeEvidenceGrid;
		ZArchitecture.GUI.ZGroupBox EntryLinesGroupBox;
		ZArchitecture.ZGrid EntryLinesGrid;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal ZArchitecture.GUI.ZPanel AnnotationAndExitInfoPanel;
		internal Enterprise.Customs.GUI.MessageSendingFormBottomSectionUserControl AdditionalWarningUserControl;
		internal ZArchitecture.GUI.ZPanel TopPanel;
	}
}
