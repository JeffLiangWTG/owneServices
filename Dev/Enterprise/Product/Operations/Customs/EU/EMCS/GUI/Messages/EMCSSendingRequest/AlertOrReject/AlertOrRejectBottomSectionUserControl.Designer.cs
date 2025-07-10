namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class AlertOrRejectBottomSectionUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.AlertRejectionDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RejectedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReasonGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AlertOrRejectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AlertRejectionDate.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReasonGrid)).BeginInit();
			this.ReasonGrid.SuspendLayout();
			this.AlertOrRejectionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent);
			// 
			// AlertRejectionDate
			// 
			this.AlertRejectionDate.AllowDrop = true;
			this.AlertRejectionDate.AutoCompleteMonthThreshold = 1;
			this.AlertRejectionDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AlertRejectionDate, "SendingObjectsCollection.DateOfAlertOrRejection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).DateOfAlertOrRejection)));
			this.AlertRejectionDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 46, true);
			this.AlertRejectionDate.Name = "AlertRejectionDate";
			this.AlertRejectionDate.TabIndex = 1;
			// 
			// RejectedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RejectedCheckBox, "SendingObjectsCollection.RejectedFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).RejectedFlag)));
			this.RejectedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RejectedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RejectedCheckBox, false);
			this.RejectedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 25, true);
			this.RejectedCheckBox.Name = "RejectedCheckBox";
			this.RejectedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 15, true);
			this.RejectedCheckBox.TabIndex = 0;
			this.RejectedCheckBox.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.RejectedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReasonGrid
			// 
			this.ReasonGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReasonGrid, "SendingObjectsCollection.AlertOrRejectionReasons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlertOrRejectionReasons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectReason)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlertOrRejectionReasons)).SyncRoot)).Reason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectReason)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.AlertOrRejectSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AlertOrRejectionReasons)).SyncRoot)).Information)));
			this.ReasonGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Reason";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zMultiLineTextBoxColumnInfo1.ColumnName = "Information";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			this.ReasonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReasonGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ReasonGrid.GridId = "e5f7cfbe-10d2-4d05-b667-0fffa0c64aa0";
			this.ReasonGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReasonGrid.LayoutKey = "reasonGrid";
			this.ReasonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 72, true);
			this.ReasonGrid.Name = "ReasonGrid";
			this.ReasonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 128, true);
			this.ReasonGrid.TabIndex = 2;
			// 
			// AlertOrRejectionGroupBox
			// 
			this.AlertOrRejectionGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7A79B166-E63A-4E94-B606-2D92368ACD0D", "Alert Or Rejection");
			this.AlertOrRejectionGroupBox.Controls.Add(this.RejectedCheckBox);
			this.AlertOrRejectionGroupBox.Controls.Add(this.AlertRejectionDate);
			this.AlertOrRejectionGroupBox.Controls.Add(this.ReasonGrid);
			this.AlertOrRejectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlertOrRejectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlertOrRejectionGroupBox.Name = "AlertOrRejectionGroupBox";
			this.AlertOrRejectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 214, true);
			this.AlertOrRejectionGroupBox.TabIndex = 0;
			this.AlertOrRejectionGroupBox.TabStop = false;
			// 
			// AlertOrRejectBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AlertOrRejectionGroupBox);
			this.Name = "AlertOrRejectBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 214, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AlertRejectionDate.ResumeLayout(true);
			this.AlertRejectionDate.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReasonGrid)).EndInit();
			this.ReasonGrid.ResumeLayout(false);
			this.ReasonGrid.PerformLayout();
			this.AlertOrRejectionGroupBox.ResumeLayout(false);
			this.AlertOrRejectionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox AlertOrRejectionGroupBox;
		internal ZArchitecture.GUI.ZDateEdit AlertRejectionDate;
		internal ZArchitecture.GUI.ZCheckBox RejectedCheckBox;
		internal ZArchitecture.ZGrid ReasonGrid;
	}
}
