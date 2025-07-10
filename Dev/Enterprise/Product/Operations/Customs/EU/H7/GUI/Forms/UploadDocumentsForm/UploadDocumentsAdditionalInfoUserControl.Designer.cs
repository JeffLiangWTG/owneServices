using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class UploadDocumentsAdditionalInfoUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UploadDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AdditionalInformationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AttachmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UploadDocumentsSplitContainer)).BeginInit();
			this.UploadDocumentsSplitContainer.Panel1.SuspendLayout();
			this.UploadDocumentsSplitContainer.Panel2.SuspendLayout();
			this.UploadDocumentsSplitContainer.SuspendLayout();
			this.AdditionalInformationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).BeginInit();
			this.AddInfosGrid.SuspendLayout();
			this.AttachmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
			this.AttachmentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction);
			// 
			// UploadDocumentsSplitContainer
			// 
			this.UploadDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UploadDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UploadDocumentsSplitContainer.Name = "UploadDocumentsSplitContainer";
			this.UploadDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// UploadDocumentsSplitContainer.Panel1
			// 
			this.UploadDocumentsSplitContainer.Panel1.Controls.Add(this.AdditionalInformationsGroupBox);
			// 
			// UploadDocumentsSplitContainer.Panel2
			// 
			this.UploadDocumentsSplitContainer.Panel2.Controls.Add(this.AttachmentsGroupBox);
			this.UploadDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 351, true);
			this.UploadDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(171);
			this.UploadDocumentsSplitContainer.SplitterWidth = 9;
			this.UploadDocumentsSplitContainer.TabIndex = 0;
			// 
			// AdditionalInformationsGroupBox
			//
			this.AdditionalInformationsGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("b1302375-e8a0-45a6-906b-3b57b6d7f5dc", "Requested Documents");
			this.AdditionalInformationsGroupBox.Controls.Add(this.AddInfosGrid);
			this.AdditionalInformationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationsGroupBox.Name = "AdditionalInformationsGroupBox";
			this.AdditionalInformationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 171, true);
			this.AdditionalInformationsGroupBox.TabIndex = 0;
			this.AdditionalInformationsGroupBox.TabStop = false;
			// 
			// AddInfosGrid
			// 
			this.AddInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddInfosGrid, "AddInfoCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).DocumentInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).ReferenceNumber)));
			this.AddInfosGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "DocumentType";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiLineTextBoxColumnInfo1.ColumnName = "DocumentInformation";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo1.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AddInfosGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AddInfosGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AddInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddInfosGrid.GridId = "3cf35449-9522-4f03-b30c-fc1947a3140b";
			this.AddInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddInfosGrid.LayoutKey = "AddInfosGrid";
			this.AddInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.AddInfosGrid.Name = "AddInfosGrid";
			this.AddInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 156, true);
			this.AddInfosGrid.TabIndex = 0;
			// 
			// AttachmentsGroupBox
			// 
			this.AttachmentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("efc75f97-a66c-49c1-9f94-e535189e6df4", "Attachments");
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsGrid);
			this.AttachmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttachmentsGroupBox.Name = "AttachmentsGroupBox";
			this.AttachmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 176, true);
			this.AttachmentsGroupBox.TabIndex = 0;
			this.AttachmentsGroupBox.TabStop = false;
			// 
			// AttachmentsGrid
			// 
			this.AttachmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttachmentsGrid, "AddInfoCollection.EDocsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).EDocsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).EDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileSizeInKB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.UploadDocumentsSendingAction)(null)).AddInfoCollection)).SyncRoot)).EDocsCollection)).SyncRoot)).FileDescription)));
			this.AttachmentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.ColumnName = "FileName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "FileSizeInKB";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "FileDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AttachmentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AttachmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AttachmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGrid.GridId = "10ecb2c6-afa9-440e-87eb-241a78425299";
			this.AttachmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttachmentsGrid.LayoutKey = "AttachmentsGrid";
			this.AttachmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.AttachmentsGrid.Name = "AttachmentsGrid";
			this.AttachmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 161, true);
			this.AttachmentsGrid.TabIndex = 0;
			// 
			// UploadDocumentsAdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UploadDocumentsSplitContainer);
			this.Name = "UploadDocumentsAdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 351, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UploadDocumentsSplitContainer.Panel1.ResumeLayout(false);
			this.UploadDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.UploadDocumentsSplitContainer)).EndInit();
			this.UploadDocumentsSplitContainer.ResumeLayout(false);
			this.UploadDocumentsSplitContainer.PerformLayout();
			this.AdditionalInformationsGroupBox.ResumeLayout(false);
			this.AdditionalInformationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).EndInit();
			this.AddInfosGrid.ResumeLayout(false);
			this.AddInfosGrid.PerformLayout();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
			this.AttachmentsGrid.ResumeLayout(false);
			this.AttachmentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer UploadDocumentsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox AdditionalInformationsGroupBox;
		private ZArchitecture.GUI.ZGroupBox AttachmentsGroupBox;
		private ZArchitecture.ZGrid AttachmentsGrid;
		private ZArchitecture.ZGrid AddInfosGrid;
	}
}
