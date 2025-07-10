namespace Enterprise.ZArchitecture.GUI
{
	partial class BusinessObjectNotificationsViewerForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.NotificationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NotificationsGrid)).BeginInit();
			this.NotificationsGrid.SuspendLayout();
			this.ButtomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 573, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1238, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerSupporter);
			// 
			// NotificationsGrid
			// 
			this.NotificationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NotificationsGrid, "NotificationsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerSupporter)(null)).NotificationsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerLine)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerSupporter)(null)).NotificationsCollection)).SyncRoot)).NotificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerLine)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerSupporter)(null)).NotificationsCollection)).SyncRoot)).NotificationMessage)));
			this.NotificationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("60eddf00-7254-4e23-a3bf-7e19eb89732c", "Notification Type");
			zTextBoxColumnStyleInfo1.ColumnName = "NotificationType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("2e0b5be8-4205-41d7-b8d1-ee8137a6692d", "Notification Message");
			zMultiLineTextBoxColumnInfo1.ColumnName = "NotificationMessage";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.NotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NotificationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.NotificationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotificationsGrid.GridId = "e9564e65-f985-4b98-b997-877855bec277";
			this.NotificationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NotificationsGrid.LayoutKey = "zGrid1";
			this.NotificationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotificationsGrid.Name = "NotificationsGrid";
			this.NotificationsGrid.ReadOnly = true;
			this.NotificationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1238, 573, true);
			this.NotificationsGrid.TabIndex = 1;
			// 
			// RefreshButton
			//
			this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("abc5f844-40fc-4142-9ef0-d7c79a45daf4", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1053, 7, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.RefreshButton.TabIndex = 2;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.Click += RefreshButton_Click;
			// 
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5de82dfa-02fc-41b0-8829-b606e89c8e48", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1144, 7, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += CloseButton_Click;
			// 
			// ButtomPanel
			// 
			this.ButtomPanel.Controls.Add(this.RefreshButton);
			this.ButtomPanel.Controls.Add(this.CloseButton);
			this.ButtomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 539, true);
			this.ButtomPanel.Name = "ButtomPanel";
			this.ButtomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1238, 34, true);
			this.ButtomPanel.TabIndex = 4;
			// 
			// BusinessObjectNotificationsViewerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1238, 597, true);
			this.Controls.Add(this.ButtomPanel);
			this.Controls.Add(this.NotificationsGrid);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.BusinessObjectNotificationsViewerSupporter);
			this.Name = "BusinessObjectNotificationsViewerForm";
			this.Text = "BusinessObjectNotificationsViewerForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtomPanel, 0);
			this.Controls.SetChildIndex(this.NotificationsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NotificationsGrid)).EndInit();
			this.NotificationsGrid.ResumeLayout(false);
			this.NotificationsGrid.PerformLayout();
			this.ButtomPanel.ResumeLayout(false);
			this.ButtomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid NotificationsGrid;
		private ZButton RefreshButton;
		private ZButton CloseButton;
		private ZPanel ButtomPanel;
	}
}
