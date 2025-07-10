namespace Enterprise.DocumentVisualizer.GUI
{
	public partial class NotificationsView
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.notificationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.infoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.notificationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 305, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentVisualizer.Business.BindableNotification);
			// 
			// notificationsGrid
			// 
			this.notificationsGrid.AllowNavigation = false;
			this.notificationsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notificationsGrid, "Notifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(null)).Notifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(null)).Notifications)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(null)).Notifications)).SyncRoot)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.BindableNotification)(null)).Notifications)).SyncRoot)).Message)));
			this.notificationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("0301a831-d5d8-4b2f-a071-881636e1f11c", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Type";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("fcbab0a2-946e-4c12-8a5b-6de283e7d8f1", "Source");
			zTextBoxColumnStyleInfo2.ColumnName = "Source";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("04cf9bcd-6dbf-4115-8089-df96e909762e", "Message");
			zTextBoxColumnStyleInfo3.ColumnName = "Message";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(440);
			zTextBoxColumnStyleInfo3.WordWrap = true;
			this.notificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.notificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.notificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.notificationsGrid.CopySelectedRowsAllowed = true;
			this.notificationsGrid.GridId = "b98bda41-3379-4fab-a128-7869458f5952";
			this.notificationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.notificationsGrid.IsCustomiseMenuVisible = false;
			this.notificationsGrid.LayoutKey = "notificationsGrid";
			this.notificationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 53, true);
			this.notificationsGrid.Name = "notificationsGrid";
			this.notificationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 217, true);
			this.notificationsGrid.TabIndex = 0;
			this.notificationsGrid.TabStop = false;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("aad3b166-839b-4d66-ac5c-c9bb28d47218", "&Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 276, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 1;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.infoLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("d2c93e05-7721-48ef-8b5f-c6634feb4d7f", "Please address the following issues before delivering the document");
			this.infoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 31, true);
			this.infoLabel.TabIndex = 2;
			// 
			// DocumentNotificationsView
			// 
			this.AcceptButton = this.closeButton;
			this.AutoAddPreviousNextButtons = false;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("f09d8775-bb0a-44a0-ac72-de6bf693a3b3", "Notifications");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 329, true);
			this.Controls.Add(this.infoLabel);
			this.Controls.Add(this.notificationsGrid);
			this.Controls.Add(this.closeButton);
			this.DataSourceType = typeof(Enterprise.DocumentVisualizer.Business.BindableNotifications);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 290, true);
			this.Name = "NotificationsView";
			this.RememberFormPosition = false;
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.notificationsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.infoLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.notificationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		private ZArchitecture.ZGrid notificationsGrid;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.ZLabel infoLabel;
	}
}