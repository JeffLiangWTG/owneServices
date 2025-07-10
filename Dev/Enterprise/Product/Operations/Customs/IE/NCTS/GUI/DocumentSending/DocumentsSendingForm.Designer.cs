namespace Enterprise.Customs.IE.NCTS.GUI
{
	partial class DocumentsSendingForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.LrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalInformationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.AdditionalInformationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).BeginInit();
			this.AddInfosGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendDocumentsSplitContainer)).BeginInit();
			this.SendDocumentsSplitContainer.Panel1.SuspendLayout();
			this.SendDocumentsSplitContainer.Panel2.SuspendLayout();
			this.SendDocumentsSplitContainer.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 509, true);
			this.SendButton.TabIndex = 5;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(874, 509, true);
			this.CancelButton2.TabIndex = 6;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Visible = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 536, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.NCTS.Business.DocumentSendingActionParent);
			// 
			// LrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.LrnTextBox, "SendingObjectsCollection.LocalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).LocalReference)));
			this.LrnTextBox.CaptionResourceString = null;
			this.LrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 42, true);
			this.LrnTextBox.Name = "LrnTextBox";
			this.LrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.LrnTextBox.TabIndex = 2;
			// 
			// MrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.MrnTextBox, "SendingObjectsCollection.MovementReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MovementReference)));
			this.MrnTextBox.CaptionResourceString = null;
			this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 16, true);
			this.MrnTextBox.Name = "MrnTextBox";
			this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.MrnTextBox.TabIndex = 1;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("8425CEDD-DABD-47DA-908B-E5B2DD319295", "Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupportingDocumentsGrid);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 432, true);
			this.SupportingDocumentsGroupBox.TabIndex = 0;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SendingObjectsCollection.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IE.Business.DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).EDoc)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "31b62fa2-333d-4b5e-972a-b34d5a5c4239";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 413, true);
			this.SupportingDocumentsGrid.TabIndex = 5;
			// 
			// AdditionalInformationsGroupBox
			// 
			this.AdditionalInformationsGroupBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("B114F49E-BFCA-40D5-A120-0B420B5CF2A4", "Additional Information");
			this.AdditionalInformationsGroupBox.Controls.Add(this.AddInfosGrid);
			this.AdditionalInformationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationsGroupBox.Name = "AdditionalInformationsGroupBox";
			this.AdditionalInformationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 432, true);
			this.AdditionalInformationsGroupBox.TabIndex = 0;
			this.AdditionalInformationsGroupBox.TabStop = false;
			// 
			// AddInfosGrid
			// 
			this.AddInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddInfosGrid, "SendingObjectsCollection.AddInfoCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.AdditionalInfoSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AddInfoCollection)).SyncRoot)).DocumentInformation)));
			this.AddInfosGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zMultiLineTextBoxColumnInfo1.ColumnName = "DocumentInformation";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.AddInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddInfosGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AddInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddInfosGrid.GridId = "7db0dee8-ba32-41a7-8d31-7c91fd452e3e";
			this.AddInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddInfosGrid.LayoutKey = "AddInfosGrid";
			this.AddInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddInfosGrid.Name = "AddInfosGrid";
			this.AddInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 413, true);
			this.AddInfosGrid.TabIndex = 4;
			// 
			// SendDocumentsSplitContainer
			// 
			this.SendDocumentsSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SendDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 68, true);
			this.SendDocumentsSplitContainer.Name = "SendDocumentsSplitContainer";
			// 
			// SendDocumentsSplitContainer.Panel1
			// 
			this.SendDocumentsSplitContainer.Panel1.Controls.Add(this.AdditionalInformationsGroupBox);
			// 
			// SendDocumentsSplitContainer.Panel2
			// 
			this.SendDocumentsSplitContainer.Panel2.Controls.Add(this.SupportingDocumentsGroupBox);
			this.SendDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 432, true);
			this.SendDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(525);
			this.SendDocumentsSplitContainer.TabIndex = 4;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.Controls.Add(this.LrnTextBox);
			this.MainGroupBox.Controls.Add(this.MrnTextBox);
			this.MainGroupBox.Controls.Add(this.SendDocumentsSplitContainer);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 503, true);
			this.MainGroupBox.TabIndex = 4;
			this.MainGroupBox.TabStop = false;
			// 
			// DocumentsSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 559, true);
			this.Controls.Add(this.MainGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.IE.Business.DocumentsSendingActionParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 598, true);
			this.Name = "DocumentsSendingForm";
			this.Text = "DocumentsSendingForm";
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.AdditionalInformationsGroupBox.ResumeLayout(false);
			this.AdditionalInformationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).EndInit();
			this.AddInfosGrid.ResumeLayout(false);
			this.AddInfosGrid.PerformLayout();
			this.SendDocumentsSplitContainer.Panel1.ResumeLayout(false);
			this.SendDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SendDocumentsSplitContainer)).EndInit();
			this.SendDocumentsSplitContainer.ResumeLayout(false);
			this.SendDocumentsSplitContainer.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox LrnTextBox;
		private Enterprise.ZArchitecture.ZTextBox MrnTextBox;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		internal ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.ZGrid SupportingDocumentsGrid;
		internal ZArchitecture.GUI.ZGroupBox AdditionalInformationsGroupBox;
		private ZArchitecture.ZGrid AddInfosGrid;
		private CargoWise.Windows.UI.KSplitContainer SendDocumentsSplitContainer;
	}
}

