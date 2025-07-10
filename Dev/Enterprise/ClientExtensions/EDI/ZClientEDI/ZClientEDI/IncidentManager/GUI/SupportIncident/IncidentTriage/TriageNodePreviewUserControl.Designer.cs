namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class TriageNodePreviewUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.checklistItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checklistItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.triageSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.moduleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.publishedDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.supportDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.isPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isInternalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.nodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.checklistItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistItemsGrid)).BeginInit();
			this.checklistItemsGrid.SuspendLayout();
			this.triageSettingsGroupBox.SuspendLayout();
			this.moduleDropEdit.SuspendLayout();
			this.productAreaDropEdit.SuspendLayout();
			this.productDropEdit.SuspendLayout();
			this.detailsGroupBox.SuspendLayout();
			this.nodeTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage);
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.checklistItemsGroupBox);
			this.mainPanel.Controls.Add(this.triageSettingsGroupBox);
			this.mainPanel.Controls.Add(this.detailsGroupBox);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 449, true);
			this.mainPanel.TabIndex = 0;
			// 
			// checklistItemsGroupBox
			// 
			this.checklistItemsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("eea6e8b6-ef4b-4666-a79c-f496cca882bf", "Checklist Items");
			this.checklistItemsGroupBox.Controls.Add(this.checklistItemsGrid);
			this.checklistItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.checklistItemsGroupBox.Name = "checklistItemsGroupBox";
			this.checklistItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 186, true);
			this.checklistItemsGroupBox.TabIndex = 2;
			this.checklistItemsGroupBox.TabStop = false;
			// 
			// checklistItemsGrid
			// 
			this.checklistItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.checklistItemsGrid, "ChecklistPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)).SyncRoot)).IMP_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_SupportDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_ResponseType)));
			this.checklistItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "IMP_Sequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("68541037-5ce1-462d-b656-9d66abe03eea", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChecklistItem+IMC_SupportDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zTextBoxColumnStyleInfo2.ColumnName = "ChecklistItem+IMC_Category";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "ChecklistItem+IMC_ResponseType";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.checklistItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.checklistItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.checklistItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.checklistItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.checklistItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsGrid.GridId = "56299b05-4bf8-4cc0-8e2b-34f8a9091b61";
			this.checklistItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.checklistItemsGrid.LayoutKey = "checklistItemsGrid";
			this.checklistItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.checklistItemsGrid.Name = "checklistItemsGrid";
			this.checklistItemsGrid.ReadOnly = true;
			this.checklistItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 167, true);
			this.checklistItemsGrid.TabIndex = 0;
			// 
			// triageSettingsGroupBox
			// 
			this.triageSettingsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("b862dd9e-82b3-4255-a2e9-ecc5c92d5f03", "Triage Settings");
			this.triageSettingsGroupBox.Controls.Add(this.moduleDropEdit);
			this.triageSettingsGroupBox.Controls.Add(this.productAreaDropEdit);
			this.triageSettingsGroupBox.Controls.Add(this.productDropEdit);
			this.triageSettingsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.triageSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
			this.triageSettingsGroupBox.Name = "triageSettingsGroupBox";
			this.triageSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 97, true);
			this.triageSettingsGroupBox.TabIndex = 1;
			this.triageSettingsGroupBox.TabStop = false;
			// 
			// moduleDropEdit
			// 
			this.moduleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.moduleDropEdit, "IMT_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Module)));
			this.moduleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("7f2619fc-bf54-49c4-a9e2-8b50c16d37f8", "Sec./Svc./Req.");
			this.moduleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 66, true);
			this.moduleDropEdit.Name = "moduleDropEdit";
			this.moduleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.moduleDropEdit.TabIndex = 2;
			this.moduleDropEdit.ReadOnly = true;
			// 
			// productAreaDropEdit
			// 
			this.productAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productAreaDropEdit, "IMT_ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_ProductArea)));
			this.productAreaDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("9d4a2714-0b44-4314-981a-7e4b0f938bcf", "Product Area");
			this.productAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 40, true);
			this.productAreaDropEdit.Name = "productAreaDropEdit";
			this.productAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.productAreaDropEdit.TabIndex = 1;
			this.productAreaDropEdit.ReadOnly = true;
			// 
			// productDropEdit
			// 
			this.productDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productDropEdit, "IMT_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Product)));
			this.productDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("63fed78d-169b-4803-a06f-d1e8e9e179f3", "Product");
			this.productDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 14, true);
			this.productDropEdit.Name = "productDropEdit";
			this.productDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.productDropEdit.TabIndex = 0;
			this.productDropEdit.ReadOnly = true;
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("e1b65551-5b6c-40ed-8789-62ba3ba2d7b2", "Details");
			this.detailsGroupBox.Controls.Add(this.publishedDescriptionTextBox);
			this.detailsGroupBox.Controls.Add(this.supportDescriptionTextBox);
			this.detailsGroupBox.Controls.Add(this.isPublishedCheckBox);
			this.detailsGroupBox.Controls.Add(this.isInternalCheckBox);
			this.detailsGroupBox.Controls.Add(this.nodeTypeDropEdit);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 166, true);
			this.detailsGroupBox.TabIndex = 0;
			this.detailsGroupBox.TabStop = false;
			// 
			// publishedDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.publishedDescriptionTextBox, "PublishedDescriptionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).PublishedDescriptionText)));
			this.publishedDescriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("3163e2e8-324a-492a-b17b-eb0d1b48778d", "Client Description");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.publishedDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.publishedDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 117, true);
			this.publishedDescriptionTextBox.Multiline = true;
			this.publishedDescriptionTextBox.Name = "publishedDescriptionTextBox";
			this.publishedDescriptionTextBox.ReadOnly = true;
			this.publishedDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 43, true);
			this.publishedDescriptionTextBox.TabIndex = 4;
			// 
			// supportDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.supportDescriptionTextBox, "IMT_SupportDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_SupportDescription)));
			this.supportDescriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("b0e0a4cd-c396-4e03-8a97-e6a58d239543", "Support Description");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.supportDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.supportDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 62, true);
			this.supportDescriptionTextBox.Multiline = true;
			this.supportDescriptionTextBox.Name = "supportDescriptionTextBox";
			this.supportDescriptionTextBox.ReadOnly = true;
			this.supportDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 34, true);
			this.supportDescriptionTextBox.TabIndex = 3;
			// 
			// isPublishedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isPublishedCheckBox, "IMT_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsPublished)));
			this.isPublishedCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("f7ccd494-8a57-4fcd-8525-44567ae35c61", "Publish to Portal");
			this.isPublishedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isPublishedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.isPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 19, true);
			this.isPublishedCheckBox.Name = "isPublishedCheckBox";
			this.isPublishedCheckBox.ReadOnly = true;
			this.isPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.isPublishedCheckBox.TabIndex = 2;
			this.isPublishedCheckBox.UseVisualStyleBackColor = true;
			// 
			// isInternalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isInternalCheckBox, "IMT_IsInternal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsInternal)));
			this.isInternalCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("71e2d2c3-b403-4267-ae55-a691712f98de", "WiseTech Internal");
			this.isInternalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isInternalCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.isInternalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 19, true);
			this.isInternalCheckBox.Name = "isInternalCheckBox";
			this.isInternalCheckBox.ReadOnly = true;
			this.isInternalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.isInternalCheckBox.TabIndex = 1;
			this.isInternalCheckBox.UseVisualStyleBackColor = true;
			// 
			// nodeTypeDropEdit
			// 
			this.nodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nodeTypeDropEdit, "IMT_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Type)));
			this.nodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 19, true);
			this.nodeTypeDropEdit.Name = "nodeTypeDropEdit";
			this.nodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.nodeTypeDropEdit.TabIndex = 0;
			// 
			// TriageNodePreviewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.mainPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "TriageNodePreviewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.checklistItemsGroupBox.ResumeLayout(false);
			this.checklistItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistItemsGrid)).EndInit();
			this.checklistItemsGrid.ResumeLayout(false);
			this.checklistItemsGrid.PerformLayout();
			this.triageSettingsGroupBox.ResumeLayout(false);
			this.triageSettingsGroupBox.PerformLayout();
			this.moduleDropEdit.ResumeLayout(true);
			this.moduleDropEdit.PerformLayout();
			this.productAreaDropEdit.ResumeLayout(true);
			this.productAreaDropEdit.PerformLayout();
			this.productDropEdit.ResumeLayout(true);
			this.productDropEdit.PerformLayout();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.nodeTypeDropEdit.ResumeLayout(true);
			this.nodeTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel mainPanel;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox checklistItemsGroupBox;
		private ZArchitecture.GUI.ZGroupBox triageSettingsGroupBox;
		private ZArchitecture.GUI.ZDropEdit nodeTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox isPublishedCheckBox;
		private ZArchitecture.GUI.ZCheckBox isInternalCheckBox;
		private ZArchitecture.ZTextBox publishedDescriptionTextBox;
		private ZArchitecture.ZTextBox supportDescriptionTextBox;
		private ZArchitecture.GUI.ZDropEdit productDropEdit;
		private ZArchitecture.GUI.ZDropEdit moduleDropEdit;
		private ZArchitecture.GUI.ZDropEdit productAreaDropEdit;
		private ZArchitecture.ZGrid checklistItemsGrid;
	}
}
