namespace Enterprise.Customs.BE.GUI
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AnnotationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AnnotationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlternativeEvidenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AlternativeEvidenceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportCustomsOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AnnotationGroupBox.SuspendLayout();
			this.AlternativeEvidenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).BeginInit();
			this.AlternativeEvidenceGrid.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.SecurityDropEdit.SuspendLayout();
			this.ExitCustomsOfficeCodeFindBox.SuspendLayout();
			this.ExitDateEdit.SuspendLayout();
			this.ExitTypeDropEdit.SuspendLayout();
			this.JustificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JustificationTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent);
			// 
			// AnnotationGroupBox
			// 
			this.AnnotationGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("14E84A2B-3082-45E3-A218-71D04DAB6ABB", "Annotation");
			this.AnnotationGroupBox.Controls.Add(this.AnnotationTextBox);
			this.AnnotationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.AnnotationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnnotationGroupBox.Name = "AnnotationGroupBox";
			this.AnnotationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 130, true);
			this.AnnotationGroupBox.TabIndex = 0;
			this.AnnotationGroupBox.TabStop = false;
			// 
			// AnnotationTextBox
			// 
			this.BindingSource.SetBindingMember(this.AnnotationTextBox, "SendingObjectsCollection.Annotation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Annotation)));
			this.AnnotationTextBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("86B5A372-30E5-4258-B2F5-6562B85FDC00", "Annotation");
			this.AnnotationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AnnotationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AnnotationTextBox, false);
			this.AnnotationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AnnotationTextBox.Multiline = true;
			this.AnnotationTextBox.Name = "AnnotationTextBox";
			this.AnnotationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 132, true);
			this.AnnotationTextBox.TabIndex = 0;
			// 
			// AlternativeEvidenceGroupBox
			// 
			this.AlternativeEvidenceGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("83F9FF06-CEAF-4FE0-B5C8-DF23E841BC21", "Alternative Evidence");
			this.AlternativeEvidenceGroupBox.Controls.Add(this.AlternativeEvidenceGrid);
			this.AlternativeEvidenceGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AlternativeEvidenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.AlternativeEvidenceGroupBox.Name = "AlternativeEvidenceGroupBox";
			this.AlternativeEvidenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 80, true);
			this.AlternativeEvidenceGroupBox.TabIndex = 0;
			this.AlternativeEvidenceGroupBox.TabStop = false;
			// 
			// AlternativeEvidenceGrid
			// 
			this.AlternativeEvidenceGrid.AllowNavigation = false;
			this.AlternativeEvidenceGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.AlternativeEvidenceGrid, "SendingObjectsCollection.AlternativeEvidences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).Reference)));
			this.AlternativeEvidenceGrid.CaptionVisible = false;
			zCodeFindColumnStyleInfo1.ColumnName = "DocType";
			zCodeFindColumnStyleInfo1.IsCustomColumn = false;
			zCodeFindColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zTextBoxColumnStyleInfo1.ColumnName = "Reference";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zCodeFindColumnStyleInfo1);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AlternativeEvidenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidenceGrid.GridId = "C1D80A39-7125-4469-9359-3BEDAB1A8AB9";
			this.AlternativeEvidenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternativeEvidenceGrid.LayoutKey = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AlternativeEvidenceGrid.Name = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 61, true);
			this.AlternativeEvidenceGrid.TabIndex = 0;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.SecurityDropEdit);
			this.RightPanel.Controls.Add(this.ExportCustomsOfficeTextBox);
			this.RightPanel.Controls.Add(this.ExitCustomsOfficeCodeFindBox);
			this.RightPanel.Controls.Add(this.ExitDateEdit);
			this.RightPanel.Controls.Add(this.ExitTypeDropEdit);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 160, true);
			this.RightPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 160, true);
			this.RightPanel.TabIndex = 1;
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "SendingObjectsCollection.SecurityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).SecurityType)));
			this.SecurityDropEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("950F0F64-ECD4-4A60-9E7B-A3BE0EAA45C6", "Security");
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 94, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.ShouldResizeByMaxLength = true;
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.SecurityDropEdit.TabIndex = 5;
			// 
			// ExportCustomsOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCustomsOfficeTextBox, "SendingObjectsCollection.ExportCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExportCustomsOffice)));
			this.ExportCustomsOfficeTextBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("3F4539B8-24DF-4128-BC31-EC901F51B349", "Export Customs Office");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitCustomsOffice)));
			this.ExitCustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("704C7C3A-2404-456A-8F04-100343C3B8AE", "Exit Customs Office");
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
			this.ExitDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExitDateEdit, "SendingObjectsCollection.ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitDate)));
			this.ExitDateEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("890581B7-0FA9-4DB5-9C52-D47FD4D0FC96", "Exit Date");
			this.ExitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 31, true);
			this.ExitDateEdit.Name = "ExitDateEdit";
			this.ExitDateEdit.TabIndex = 1;
			// 
			// ExitTypeDropEdit
			// 
			this.ExitTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitTypeDropEdit, "SendingObjectsCollection.ExitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitType)));
			this.ExitTypeDropEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("CDF042D0-D03F-4A86-933E-F2E25A1853DA", "Exit Type");
			this.ExitTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 10, true);
			this.ExitTypeDropEdit.Name = "ExitTypeDropEdit";
			this.ExitTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ExitTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ExitTypeDropEdit.TabIndex = 0;
			// 
			// JustificationGroupBox
			// 
			this.JustificationGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("c614119f-b5dd-4c51-b2c7-b501e94974c1", "Justification");
			this.JustificationGroupBox.Controls.Add(this.JustificationTextBox);
			this.JustificationGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.JustificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JustificationGroupBox.Name = "JustificationGroupBox";
			this.JustificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 100, true);
			this.JustificationGroupBox.TabIndex = 0;
			this.JustificationGroupBox.TabStop = false;
			// 
			// JustificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JustificationTextBox, "SendingObjectsCollection.Justification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.ExportEntryMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.ExportDeclarationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Justification)));
			this.JustificationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 40, true);
			this.JustificationTextBox.Multiline = true;
			this.JustificationTextBox.Name = "JustificationTextBox";
			this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 100, true);
			this.JustificationTextBox.TabIndex = 0;
			// 
			// ExportBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RightPanel);
			this.Controls.Add(this.AnnotationGroupBox);
			this.Controls.Add(this.AlternativeEvidenceGroupBox);
			this.Controls.Add(this.JustificationGroupBox);
			this.Name = "ExportBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 240, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AnnotationGroupBox.ResumeLayout(false);
			this.AnnotationGroupBox.PerformLayout();
			this.AlternativeEvidenceGroupBox.ResumeLayout(false);
			this.AlternativeEvidenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).EndInit();
			this.AlternativeEvidenceGrid.ResumeLayout(false);
			this.AlternativeEvidenceGrid.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.ExitCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.ExitCustomsOfficeCodeFindBox.PerformLayout();
			this.ExitDateEdit.ResumeLayout(true);
			this.ExitDateEdit.PerformLayout();
			this.ExitTypeDropEdit.ResumeLayout(true);
			this.ExitTypeDropEdit.PerformLayout();
			this.JustificationGroupBox.ResumeLayout(true);
			this.JustificationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AnnotationGroupBox;
		private ZArchitecture.ZTextBox AnnotationTextBox;
		private ZArchitecture.GUI.ZPanel RightPanel;
		private ZArchitecture.GUI.ZCodeFindBox ExitCustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit ExitDateEdit;
		private ZArchitecture.GUI.ZDropEdit ExitTypeDropEdit;
		private ZArchitecture.ZTextBox ExportCustomsOfficeTextBox;
		private ZArchitecture.GUI.ZDropEdit SecurityDropEdit;
		private ZArchitecture.GUI.ZGroupBox AlternativeEvidenceGroupBox;
		private Enterprise.ZArchitecture.ZGrid AlternativeEvidenceGrid;
		private ZArchitecture.GUI.ZGroupBox JustificationGroupBox;
		private ZArchitecture.ZTextBox JustificationTextBox;
	}
}
