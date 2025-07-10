namespace Enterprise.Customs.IE.GUI
{
	partial class AISDocumentsUploadForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DynamicAddInfoGridsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.AdditionalInformationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ValidationErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
			this.WarningSplitContainer.Panel1.SuspendLayout();
			this.WarningSplitContainer.SuspendLayout();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalInformationsGroupBox.SuspendLayout();
			this.AttachmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
			this.AttachmentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendDocumentsSplitContainer)).BeginInit();
			this.SendDocumentsSplitContainer.Panel1.SuspendLayout();
			this.SendDocumentsSplitContainer.Panel2.SuspendLayout();
			this.SendDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 140, true);
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 509, true);
			// 
			// PreviewMessageCheckBox
			// 
			this.PreviewMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 533, true);
			// 
			// SplitContainer
			// 
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SendDocumentsSplitContainer);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 530, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(179);
			// 
			// WarningSplitContainer
			// 
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 347, true);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
			this.WarningSplitContainer.Visible = false;
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 533, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(874, 533, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 562, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent);
			// 
			// DynamicAddInfoGridsPanel
			// 
			this.DynamicAddInfoGridsPanel.AllowDrop = true;
			this.DynamicAddInfoGridsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicAddInfoGridsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicAddInfoGridsPanel.Name = "DynamicAddInfoGridsPanel";
			this.DynamicAddInfoGridsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 80, true);
			this.DynamicAddInfoGridsPanel.TabIndex = 1;
			// 
			// AdditionalInformationsGroupBox
			// 
			this.AdditionalInformationsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("E96B2BE5-2DB2-4FE4-B4AF-15294BABD98F", "Requested Documents");
			this.AdditionalInformationsGroupBox.Controls.Add(this.DynamicAddInfoGridsPanel);
			this.AdditionalInformationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationsGroupBox.Name = "AdditionalInformationsGroupBox";
			this.AdditionalInformationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 99, true);
			this.AdditionalInformationsGroupBox.TabIndex = 0;
			this.AdditionalInformationsGroupBox.TabStop = false;
			// 
			// AttachmentsGroupBox
			// 
			this.AttachmentsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("9BE2FDAC-B12A-4123-A94D-4D4EA7ADB677", "Attachments");
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsGrid);
			this.AttachmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttachmentsGroupBox.Name = "AttachmentsGroupBox";
			this.AttachmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 55, true);
			this.AttachmentsGroupBox.TabIndex = 0;
			this.AttachmentsGroupBox.TabStop = false;
			// 
			// AttachmentsGrid
			// 
			this.AttachmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttachmentsGrid, "SendingObjectsCollection.AddInfoCollection.EDocsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).EDocsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).EDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.IE.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileSizeInKB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileDescription)));
			this.AttachmentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.ColumnName = "FileName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "FileSizeInKB";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo5.ColumnName = "FileDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AttachmentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AttachmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGrid.GridId = "1AF9713D-7E9E-4644-B66B-23C91FE031B3";
			this.AttachmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttachmentsGrid.LayoutKey = "AttachmentsGrid";
			this.AttachmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AttachmentsGrid.Name = "AttachmentsGrid";
			this.AttachmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 36, true);
			this.AttachmentsGrid.TabIndex = 4;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeDateOfAcceptance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsJustification)));
			// 
			// SendDocumentsSplitContainer
			// 
			this.SendDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SendDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SendDocumentsSplitContainer.Name = "SendDocumentsSplitContainer";
			this.SendDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SendDocumentsSplitContainer.Panel1
			// 
			this.SendDocumentsSplitContainer.Panel1.Controls.Add(this.AdditionalInformationsGroupBox);
			// 
			// SendDocumentsSplitContainer.Panel2
			// 
			this.SendDocumentsSplitContainer.Panel2.Controls.Add(this.AttachmentsGroupBox);
			this.SendDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 158, true);
			this.SendDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(99);
			this.SendDocumentsSplitContainer.TabIndex = 0;
			// 
			// AISDocumentsUploadForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 585, true);
			this.DataSourceType = typeof(Enterprise.Customs.IE.Business.UploadDocumentsSendingActionParent);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 624, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 624, true);
			this.Name = "AISDocumentsUploadForm";
			this.Text = "DocumentsUploadForm";
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.WarningSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).EndInit();
			this.WarningSplitContainer.ResumeLayout(false);
			this.WarningSplitContainer.PerformLayout();
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalInformationsGroupBox.ResumeLayout(false);
			this.AdditionalInformationsGroupBox.PerformLayout();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
			this.AttachmentsGrid.ResumeLayout(false);
			this.AttachmentsGrid.PerformLayout();
			this.SendDocumentsSplitContainer.Panel1.ResumeLayout(false);
			this.SendDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SendDocumentsSplitContainer)).EndInit();
			this.SendDocumentsSplitContainer.ResumeLayout(false);
			this.SendDocumentsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer SendDocumentsSplitContainer;
		ZArchitecture.GUI.ZGroupBox AdditionalInformationsGroupBox;
		ZArchitecture.GUI.ZGroupBox AttachmentsGroupBox;
		ZArchitecture.GUI.DynamicLayoutPanel DynamicAddInfoGridsPanel;
		ZArchitecture.ZGrid AttachmentsGrid;
	}
}
