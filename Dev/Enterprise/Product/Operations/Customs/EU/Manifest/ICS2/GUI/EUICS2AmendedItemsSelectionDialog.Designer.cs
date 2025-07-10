namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class EUICS2AmendedItemsSelectionDialog
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 364, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 21, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader);
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "AmendedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)).SyncRoot)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)).SyncRoot)).Identifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)).SyncRoot)).RequestTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)).SyncRoot)).ResponsibleMemberState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader)(null)).AmendedItems)).SyncRoot)).RequestStatus)));
			this.ItemsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Identifier";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "RequestTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "ResponsibleMemberState";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "RequestStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "012D07F1-8F0B-4F52-BE7F-08FD1B78A6EB";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGridGridKey";
			this.ItemsGrid.RemoveAction = ZArchitecture.RemoveAction.NoRemovePossible;
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 343, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.DeselectAllButton);
			this.ButtonPanel.Controls.Add(this.SelectAllButton);
			this.ButtonPanel.Controls.Add(this.SendButton);
			this.ButtonPanel.Controls.Add(this.ButtonCancel);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 30, true);
			this.ButtonPanel.TabIndex = 2;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeselectAllButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("51CD911B-17CE-4CF5-9101-12E1A1988432", "Deselect All");
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 5, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.DeselectAllButton.TabIndex = 3;
			this.DeselectAllButton.ToolTipCaption = null;
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("2559BF51-C121-4444-821D-3773053A93EB", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SelectAllButton.TabIndex = 2;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("74582650-5630-4F12-B65F-7EE655AB89B8", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 5, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SendButton.TabIndex = 4;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("5135D41E-B2B2-4623-AAF2-7A77D6632D14", "&Cancel");
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 5, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ButtonCancel.TabIndex = 5;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("391C39CE-28A8-41F5-B7F1-F06C8A7A3987", "Referral Request");
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 359, true);
			this.ItemsGroupBox.TabIndex = 0;
			this.ItemsGroupBox.TabStop = false;
			// 
			// EUICS2AmendedItemsSelectionDialog
			// 
			this.CancelButton = this.ButtonCancel;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 389, true);
			this.Controls.Add(this.ItemsGroupBox);
			this.Controls.Add(this.ButtonPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.EU.Manifest.ICS2.Business";
			this.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader);
			this.DataSourceTypeName = "Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItemsHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 427, true);
			this.Name = "EUICS2AmendedItemsSelectionDialog";
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ItemsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.ZGrid ItemsGrid;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		Enterprise.ZArchitecture.GUI.ZButton SendButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
		ZArchitecture.GUI.ZButton DeselectAllButton;
		ZArchitecture.GUI.ZButton SelectAllButton;
	}
}
