using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.H7.GUI
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AttachmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AttachmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
			this.AttachmentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction);
			// 
			
			// AttachmentsGroupBox
			// 
			this.AttachmentsGroupBox.CaptionResourceString = Enterprise.Customs.GB.H7.GUI.Res.GetData("4992d0eb-a845-49a1-8575-6de4b6b6d87a", "Attachments");
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
			this.BindingSource.SetBindingMember(this.AttachmentsGrid, "EDocsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)).SyncRoot)).EDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)).SyncRoot)).FileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)).SyncRoot)).FileSizeInKB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.H7.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.GB.H7.Business.UploadDocumentsSendingAction)(null)).EDocsCollection)).SyncRoot)).FileDescription)));
			this.AttachmentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo1.ColumnName = "FileName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "FileSizeInKB";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "DocumentType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "FileDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AttachmentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AttachmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentsGrid.GridId = "23526d91-b632-43bf-844b-dd6a964c4cb9";
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
			this.Controls.Add(this.AttachmentsGroupBox);
			this.Name = "UploadDocumentsAdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 351, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
			this.AttachmentsGrid.ResumeLayout(false);
			this.AttachmentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AttachmentsGroupBox;
		private ZArchitecture.ZGrid AttachmentsGrid;
	}
}
