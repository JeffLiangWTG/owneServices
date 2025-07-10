using System.Windows.Forms;

namespace Enterprise.Services.OperationalActions.GUI
{
	partial class DocumentDeliveryControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZGrid documentsGrid;
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZPanel documentsTopPanel;
			Enterprise.ZArchitecture.GUI.ZDropEdit bulkDeliveryMethodDropEdit;
			Enterprise.ZArchitecture.GUI.ZCheckBox includeCoverNoteCheckBox;
			Enterprise.ZArchitecture.GUI.ZDropEdit attachmentOptionsDropEdit;
			Enterprise.ZArchitecture.GUI.ZGuidDropEdit printerDropEdit;
			ZArchitecture.GUI.ZDropEdit documentPrintLanguageDropEdit;
			CargoWise.Windows.UI.KSplitContainer coverNoteDocumentSplitPanel;
			Enterprise.ZArchitecture.ZTextBox coverNoteTextBox;
			documentsGrid = new Enterprise.ZArchitecture.ZGrid();
			documentsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			bulkDeliveryMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			includeCoverNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			attachmentOptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			deliverDocumentsInOneEmailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			printerDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			documentPrintLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			coverNoteDocumentSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			coverNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			overrideEmailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			emailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(documentsGrid)).BeginInit();
			documentsTopPanel.SuspendLayout();
			coverNoteDocumentSplitPanel.Panel1.SuspendLayout();
			coverNoteDocumentSplitPanel.Panel2.SuspendLayout();
			coverNoteDocumentSplitPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionRunner);
			// 
			// documentsGrid
			// 
			documentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(documentsGrid, "Action+DocumentPivotsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).Action.DocumentPivotsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivot)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).Action.DocumentPivotsView)).SyncRoot)).SF_SU_Outward)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionDocumentPivot)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).Action.DocumentPivotsView)).SyncRoot)).Lookups.Documents)));
			documentsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.Documents";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("DocumentDeliveryControl|72e40bac-05a1-461c-8432-3541df75ead6", "Name");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SF_SU_Outward";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.StmMenuItem;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(390);
			documentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			documentsGrid.GridId = "556be965-c6a4-462c-a747-f2aa7735f3ca";
			documentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			documentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			documentsGrid.IsWholeRowSelectedOnClick = true;
			documentsGrid.LayoutKey = "documentsGrid";
			documentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			documentsGrid.Name = "documentsGrid";
			documentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			documentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 67, true);
			documentsGrid.TabIndex = 0;
			// 
			// documentsTopPanel
			// 
			documentsTopPanel.Controls.Add(bulkDeliveryMethodDropEdit);
			documentsTopPanel.Controls.Add(includeCoverNoteCheckBox);
			documentsTopPanel.Controls.Add(deliverDocumentsInOneEmailCheckBox);
			documentsTopPanel.Controls.Add(attachmentOptionsDropEdit);
			documentsTopPanel.Controls.Add(printerDropEdit);
			documentsTopPanel.Controls.Add(documentPrintLanguageDropEdit);
			documentsTopPanel.Controls.Add(overrideEmailCheckBox);
			documentsTopPanel.Controls.Add(emailTextBox);
			documentsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			documentsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			documentsTopPanel.Name = "documentsTopPanel";
			documentsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 120, true);
			documentsTopPanel.TabIndex = 0;
			// 
			// bulkDeliveryMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(bulkDeliveryMethodDropEdit, "BulkDeliveryMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).BulkDeliveryMethod)));
			bulkDeliveryMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 8, true);
			bulkDeliveryMethodDropEdit.Name = "bulkDeliveryMethodDropEdit";
			bulkDeliveryMethodDropEdit.PreBoundMaxLength = 15;
			bulkDeliveryMethodDropEdit.ShowDescriptionBox = false;
			bulkDeliveryMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			bulkDeliveryMethodDropEdit.TabIndex = 0;
			// 
			// includeCoverNoteCheckBox
			// 
			includeCoverNoteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(includeCoverNoteCheckBox, "IncludeCoverNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).IncludeCoverNote)));
			includeCoverNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			includeCoverNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 80, true);
			includeCoverNoteCheckBox.Name = "includeCoverNoteCheckBox";
			includeCoverNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 17, true);
			includeCoverNoteCheckBox.TabIndex = 3;
			// 
			// deliverDocumentsInOneEmailCheckBox
			// 
			deliverDocumentsInOneEmailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(deliverDocumentsInOneEmailCheckBox, "DeliverDocumentsInOneEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).DeliverDocumentsInOneEmail)));
			deliverDocumentsInOneEmailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			deliverDocumentsInOneEmailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 80, true);
			deliverDocumentsInOneEmailCheckBox.Name = "deliverDocumentsInOneEmailCheckBox";
			deliverDocumentsInOneEmailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 17, true);
			deliverDocumentsInOneEmailCheckBox.TabIndex = 4;
			// 
			// attachmentOptionsDropEdit
			// 
			this.BindingSource.SetBindingMember(attachmentOptionsDropEdit, "AttachmentOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).AttachmentOptions)));
			attachmentOptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 78, true);
			attachmentOptionsDropEdit.Name = "attachmentOptionsDropEdit";
			attachmentOptionsDropEdit.PreBoundMaxLength = 30;
			attachmentOptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			attachmentOptionsDropEdit.TabIndex = 5;
			// 
			// printerDropEdit
			// 
			this.BindingSource.SetBindingMember(printerDropEdit, "Printer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).Printer)));
			printerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 32, true);
			printerDropEdit.Name = "printerDropEdit";
			printerDropEdit.PreBoundMaxLength = 30;
			printerDropEdit.ShowDescriptionBox = false;
			printerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			printerDropEdit.TabIndex = 1;
			// 
			// documentPrintLanguageDropEdit
			// 
			BindingSource.SetBindingMember(documentPrintLanguageDropEdit, "DocumentPrintLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).DocumentPrintLanguage)));
			documentPrintLanguageDropEdit.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("DocumentDeliveryControl|AFE6A6A6-B9E7-4508-8C44-6543BE68D9F0", "Print Language");
			documentPrintLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 56, true);
			documentPrintLanguageDropEdit.Name = "documentPrintLanguageDropEdit";
			documentPrintLanguageDropEdit.PreBoundMaxLength = 30;
			documentPrintLanguageDropEdit.ShowDescriptionBox = false;
			documentPrintLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			documentPrintLanguageDropEdit.TabIndex = 2;
			// 
			// coverNoteDocumentSplitPanel
			// 
			coverNoteDocumentSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			coverNoteDocumentSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 98, true);
			coverNoteDocumentSplitPanel.Name = "coverNoteDocumentSplitPanel";
			coverNoteDocumentSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// coverNoteDocumentSplitPanel.Panel1
			// 
			coverNoteDocumentSplitPanel.Panel1.Controls.Add(coverNoteTextBox);
			// 
			// coverNoteDocumentSplitPanel.Panel2
			// 
			coverNoteDocumentSplitPanel.Panel2.Controls.Add(documentsGrid);
			coverNoteDocumentSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 181, true);
			coverNoteDocumentSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(110);
			coverNoteDocumentSplitPanel.TabIndex = 1;
			// 
			// coverNoteTextBox
			// 
			coverNoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(coverNoteTextBox, "CoverNoteText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).CoverNoteText)));
			coverNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(coverNoteTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			coverNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			coverNoteTextBox.Multiline = true;
			coverNoteTextBox.Name = "coverNoteTextBox";
			coverNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 86, true);
			coverNoteTextBox.TabIndex = 0;
			// 
			// DocumentDeliveryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(coverNoteDocumentSplitPanel);
			this.Controls.Add(documentsTopPanel);
			this.Name = "DocumentDeliveryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 279, true);
			// 
			// overrideEmailCheckBox
			// 
			overrideEmailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(overrideEmailCheckBox, "OverrideRecipientEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).OverrideRecipientEmail)));
			overrideEmailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			overrideEmailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 100, true);
			overrideEmailCheckBox.Name = "overrideEmailCheckBox";
			overrideEmailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 17, true);
			overrideEmailCheckBox.TabIndex = 6;
			overrideEmailCheckBox.Visible = false;
			// 
			// emailCheckBox
			// 
			emailTextBox.AutoSize = true;
			this.BindingSource.SetBindingMember(emailTextBox, "RecipientEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalActionRunner)(null)).RecipientEmail)));
			emailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 100, true);
			emailTextBox.Name = "emailTextBox";
			emailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			emailTextBox.TabIndex = 7;
			emailTextBox.Visible = false;
			emailTextBox.CharacterCasing = CharacterCasing.Normal;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(documentsGrid)).EndInit();
			documentsTopPanel.ResumeLayout(false);
			documentsTopPanel.PerformLayout();
			coverNoteDocumentSplitPanel.Panel1.ResumeLayout(false);
			coverNoteDocumentSplitPanel.Panel1.PerformLayout();
			coverNoteDocumentSplitPanel.Panel2.ResumeLayout(false);
			coverNoteDocumentSplitPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		Enterprise.ZArchitecture.GUI.ZCheckBox deliverDocumentsInOneEmailCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox overrideEmailCheckBox;
		Enterprise.ZArchitecture.ZTextBox emailTextBox;
	}
}
