namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSAttachmentsSelectionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SendBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageSendingObjectsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageSendingObjectsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent);
			// 
			// SendBoundButton
			// 
			this.SendBoundButton.IsCaptionOverridden = true;
			this.SendBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 255, true);
			this.SendBoundButton.Name = "SendBoundButton";
			this.SendBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.SendBoundButton.TabIndex = 1;
			this.SendBoundButton.Text = "Send";
			this.SendBoundButton.ToolTipCaption = null;
			this.SendBoundButton.Click += new System.EventHandler(this.SendBoundButton_Click);
			// 
			// CancelBoundButton
			// 
			this.CancelBoundButton.IsCaptionOverridden = true;
			this.CancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(721, 255, true);
			this.CancelBoundButton.Name = "CancelBoundButton";
			this.CancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.CancelBoundButton.TabIndex = 1;
			this.CancelBoundButton.Text = "Cancel";
			this.CancelBoundButton.ToolTipCaption = null;
			this.CancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// MessageSendingObjectsGroupBox
			// 
			this.MessageSendingObjectsGroupBox.Controls.Add(this.MessageSendingObjectsGrid);
			this.MessageSendingObjectsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSendingObjectsGroupBox.Name = "MessageSendingObjectsGroupBox";
			this.MessageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 247, true);
			this.MessageSendingObjectsGroupBox.TabIndex = 2;
			this.MessageSendingObjectsGroupBox.TabStop = false;
			this.MessageSendingObjectsGroupBox.Text = "Attachments to be sent";
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageSendingObjectsGrid, "SendingObjectsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ShouldSend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).DocReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageStatusDescription)));
			this.MessageSendingObjectsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49);
			zTextBoxColumnStyleInfo1.ColumnName = "DocReference";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "DocType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "MessageStatus";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo5.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingObjectsGrid.GridId = "6ce39aca-e0e8-4a6e-89bd-c392e898cc11";
			this.MessageSendingObjectsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageSendingObjectsGrid.LayoutKey = "MessageSendingObjectsGrid";
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.MessageSendingObjectsGrid.Name = "MessageSendingObjectsGrid";
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 232, true);
			this.MessageSendingObjectsGrid.TabIndex = 0;
			// 
			// AUCOLSAttachmentsSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 311, true);
			this.Controls.Add(this.SendBoundButton);
			this.Controls.Add(this.CancelBoundButton);
			this.Controls.Add(this.MessageSendingObjectsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivotMessageSendingActionParent);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AUCOLSAttachmentsSelectionForm";
			this.Text = "Select Attachments";
			this.Controls.SetChildIndex(this.MessageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelBoundButton, 0);
			this.Controls.SetChildIndex(this.SendBoundButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageSendingObjectsGroupBox.ResumeLayout(false);
			this.MessageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZButton SendBoundButton;
		internal ZArchitecture.GUI.ZButton CancelBoundButton;
		internal ZArchitecture.GUI.ZGroupBox MessageSendingObjectsGroupBox;
		internal ZArchitecture.ZGrid MessageSendingObjectsGrid;

		#endregion
	}
}
