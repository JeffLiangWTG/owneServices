
using System.Windows.Forms;

namespace Enterprise.Customs.IE.GUI
{
	partial class DocumentsSendingForm
	{
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
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentSendingSupportingDocumentUserControl = new DocumentSendingSupportingDocumentUserControl();
			this.AdditionalInformationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.DocumentSendingSupportingDocumentUserControl.SuspendLayout();
			this.AdditionalInformationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddInfosGrid)).BeginInit();
			this.AddInfosGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SendDocumentsSplitContainer)).BeginInit();
			this.SendDocumentsSplitContainer.Panel1.SuspendLayout();
			this.SendDocumentsSplitContainer.Panel2.SuspendLayout();
			this.SendDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 515, true);
			this.SendButton.TabIndex = 5;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			                                                                                   | System.Windows.Forms.AnchorStyles.Left)
			                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 110, true);
			this.messageSendingObjectsGroupBox.TabIndex = 0;
			this.messageSendingObjectsGroupBox.TabStop = false;
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 108, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(874, 515, true);
			this.CancelButton2.TabIndex = 6;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 536, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.DocumentsSendingActionParent);
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("aa289343-df96-4346-a982-1214ad10fbe1", "Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.DocumentSendingSupportingDocumentUserControl);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 400, true);
			this.SupportingDocumentsGroupBox.TabIndex = 0;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// DocumentSendingSupportingDocumentUserControl
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.DocumentsSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)));
			this.DocumentSendingSupportingDocumentUserControl.TabIndex = 5;
			this.DocumentSendingSupportingDocumentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentSendingSupportingDocumentUserControl.Dock = DockStyle.Fill;
			// 
			// AdditionalInformationsGroupBox
			// 
			this.AdditionalInformationsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("fc331aec-e94d-44a8-b45c-869fd37fb682", "Additional Information");
			this.AdditionalInformationsGroupBox.Controls.Add(this.AddInfosGrid);
			this.AdditionalInformationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationsGroupBox.Name = "AdditionalInformationsGroupBox";
			this.AdditionalInformationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 400, true);
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
			this.SendDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 113, true);
			this.SendDocumentsSplitContainer.Name = "SendDocumentsSplitContainer";
			// 
			// SendDocumentsSplitContainer.Panel1
			// 
			this.SendDocumentsSplitContainer.Panel1.Controls.Add(this.AdditionalInformationsGroupBox);
			// 
			// SendDocumentsSplitContainer.Panel2
			// 
			this.SendDocumentsSplitContainer.Panel2.Controls.Add(this.SupportingDocumentsGroupBox);
			this.SendDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 400, true);
			this.SendDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(525);
			this.SendDocumentsSplitContainer.TabIndex = 4;
			// 
			// DocumentsSendingForm
			//
			this.Controls.Add(this.SendDocumentsSplitContainer);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 559, true);
			this.DataSourceType = typeof(Enterprise.Customs.IE.Business.DocumentsSendingActionParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 598, true);
			this.Name = "DocumentsSendingForm";
			this.Text = "DocumentsSendingForm";
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.SendDocumentsSplitContainer, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
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
			this.DocumentSendingSupportingDocumentUserControl.ResumeLayout(false);
			this.DocumentSendingSupportingDocumentUserControl.PerformLayout();
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZGroupBox AdditionalInformationsGroupBox;
		private ZArchitecture.ZGrid AddInfosGrid;
		private CargoWise.Windows.UI.KSplitContainer SendDocumentsSplitContainer;
		internal DocumentSendingSupportingDocumentUserControl DocumentSendingSupportingDocumentUserControl;
	}
}
