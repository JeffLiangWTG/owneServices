namespace Enterprise.Customs.IE.H7.GUI
{
	partial class RF415MessageSendingForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
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
			this.DocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			this.DocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 240, true);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 381, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(124);
			// 
			// WarningSplitContainer
			// 
			// 
			// WarningSplitContainer.Panel1
			// 
			this.WarningSplitContainer.Panel1.Controls.Add(this.DocumentsGroupBox);
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 240, true);
			this.WarningSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.WarningSplitContainer.Panel2Collapsed = true;
			this.WarningSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 433, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 433, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent);
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.Controls.Add(this.DocumentsGrid);
			this.DocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 240, true);
			this.DocumentsGroupBox.TabIndex = 9;
			this.DocumentsGroupBox.TabStop = false;
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "SendingObjectsCollection.DocumentSendingObjectCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DocumentSendingObjectCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.H7.Business.RF415DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DocumentSendingObjectCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.H7.Business.RF415DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DocumentSendingObjectCollection)).SyncRoot)).DocumentIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IE.H7.Business.RF415DocumentSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DocumentSendingObjectCollection)).SyncRoot)).DocumentDate)));
			this.DocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "DocumentType";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentIdentifier";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "DocumentDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "ea26db38-af41-4cbe-a8a4-57ae91a6cd8a";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 225, true);
			this.DocumentsGrid.TabIndex = 9;
			// 
			// RF415MessageSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 487, true);
			this.DataSourceType = typeof(Enterprise.Customs.IE.H7.Business.RF415MessageSendingObjectParent);
			this.Name = "RF415MessageSendingForm";
			this.Controls.SetChildIndex(this.SplitContainer, 0);
			this.Controls.SetChildIndex(this.SendWithValidationErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.SendWithAdditionalWarningCheckBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.PreviewMessageCheckBox, 0);
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
			this.DocumentsGroupBox.ResumeLayout(false);
			this.DocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			this.DocumentsGrid.ResumeLayout(false);
			this.DocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DocumentsGroupBox;
		private ZArchitecture.ZGrid DocumentsGrid;
	}
}
