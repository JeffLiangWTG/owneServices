namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	partial class ChiefExportConsolIntegrationUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DeclarationMessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MawbExportAddInfoUserControl = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirInventory.MawbExportAddInfoUserControl();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ChiefDataTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MasterMessages = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelatedConsolsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.chiefRelatedConsolCollectionControl1 = new Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration.ChiefRelatedConsolCollectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationMessagesGrid)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.ChiefDataTab.SuspendLayout();
			this.MasterMessages.SuspendLayout();
			this.RelatedConsolsTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper);
			// 
			// DeclarationMessagesGrid
			// 
			this.DeclarationMessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeclarationMessagesGrid, "Messages");
			this.DeclarationMessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Entry used for messaging";
			zTextBoxColumnStyleInfo6.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo6.ColumnName = "UCR";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(181);
			zTextBoxColumnStyleInfo7.Caption = "Action";
			zTextBoxColumnStyleInfo7.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo7.ColumnName = "Action";
			zTextBoxColumnStyleInfo8.Caption = "Status";
			zTextBoxColumnStyleInfo8.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo8.ColumnName = "Status";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo9.Caption = "Date";
			zTextBoxColumnStyleInfo9.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo9.ColumnName = "CreationTime";
			this.DeclarationMessagesGrid.GridId = "ab52e91e-4034-417a-b3f1-ba7dca49da1b";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo10.Caption = "Entry Status";
			zTextBoxColumnStyleInfo10.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo10.ColumnName = "Entry+CH_EntryStatus";
			this.DeclarationMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DeclarationMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DeclarationMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DeclarationMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DeclarationMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DeclarationMessagesGrid.CopySelectedRowsAllowed = true;
			this.DeclarationMessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationMessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationMessagesGrid.LayoutKey = "MessagesGrid";
			this.DeclarationMessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DeclarationMessagesGrid.Name = "DeclarationMessagesGrid";
			this.DeclarationMessagesGrid.ReadOnly = true;
			this.DeclarationMessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1112, 535, true);
			this.DeclarationMessagesGrid.TabIndex = 0;
			this.DeclarationMessagesGrid.DoubleClick += new System.EventHandler(this.MessagesGrid_DoubleClick);
			// 
			// MawbExportAddInfoUserControl
			// 
			this.MawbExportAddInfoUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MawbExportAddInfoUserControl, "MawbExportHelper");
			this.MawbExportAddInfoUserControl.CaptionResourceString = null;
			this.MawbExportAddInfoUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MawbExportAddInfoUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MawbExportAddInfoUserControl.Name = "MawbExportAddInfoUserControl";
			this.MawbExportAddInfoUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1112, 535, true);
			this.MawbExportAddInfoUserControl.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.ChiefDataTab);
			this.zTabControl1.Controls.Add(this.RelatedConsolsTab);
			this.zTabControl1.Controls.Add(this.MasterMessages);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 568, true);
			this.zTabControl1.TabIndex = 3;
			// 
			// ChiefDataTab
			// 
			this.ChiefDataTab.CaptionResourceString = null;
			this.ChiefDataTab.Controls.Add(this.MawbExportAddInfoUserControl);
			this.ChiefDataTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChiefDataTab.Name = "ChiefDataTab";
			this.ChiefDataTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChiefDataTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 541, true);
			this.ChiefDataTab.TabIndex = 1;
			this.ChiefDataTab.Text = "Customs Data";
			this.ChiefDataTab.UseVisualStyleBackColor = true;
			// 
			// MasterMessages
			// 
			this.MasterMessages.CaptionResourceString = null;
			this.MasterMessages.Controls.Add(this.DeclarationMessagesGrid);
			this.MasterMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MasterMessages.Name = "MasterMessages";
			this.MasterMessages.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MasterMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 541, true);
			this.MasterMessages.TabIndex = 0;
			this.MasterMessages.Text = "Declarations\' Master Messages";
			this.MasterMessages.UseVisualStyleBackColor = true;
			// 
			// RelatedConsolsTab
			// 
			this.RelatedConsolsTab.CaptionResourceString = null;
			this.RelatedConsolsTab.Controls.Add(this.chiefRelatedConsolCollectionControl1);
			this.RelatedConsolsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedConsolsTab.Name = "RelatedConsolsTab";
			this.RelatedConsolsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedConsolsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 541, true);
			this.RelatedConsolsTab.TabIndex = 2;
			this.RelatedConsolsTab.Text = "Child Consols";
			this.RelatedConsolsTab.UseVisualStyleBackColor = true;
			// 
			// chiefRelatedConsolCollectionControl1
			// 
			this.chiefRelatedConsolCollectionControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chiefRelatedConsolCollectionControl1, ".");
			this.chiefRelatedConsolCollectionControl1.CaptionResourceString = null;
			this.chiefRelatedConsolCollectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chiefRelatedConsolCollectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.chiefRelatedConsolCollectionControl1.Name = "chiefRelatedConsolCollectionControl1";
			this.chiefRelatedConsolCollectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1112, 535, true);
			this.chiefRelatedConsolCollectionControl1.TabIndex = 0;
			// 
			// ChiefExportConsolIntegrationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTabControl1);
			this.Name = "ChiefExportConsolIntegrationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 568, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationMessagesGrid)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.ChiefDataTab.ResumeLayout(false);
			this.MasterMessages.ResumeLayout(false);
			this.RelatedConsolsTab.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid DeclarationMessagesGrid;
		protected Ccsuk.CcsukAirInventory.MawbExportAddInfoUserControl MawbExportAddInfoUserControl;
		private ZArchitecture.GUI.ZTabControl zTabControl1;
		internal ZArchitecture.GUI.ZTabPage ChiefDataTab;
		private ZArchitecture.GUI.ZTabPage MasterMessages;
		internal ZArchitecture.GUI.ZTabPage RelatedConsolsTab;
		private ChiefRelatedConsolCollectionControl chiefRelatedConsolCollectionControl1;
	}
}
