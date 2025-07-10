using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.GUI.Records
{
	partial class ArchivedRecordForm
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchivedRecordForm));
			this.previewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.gridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.storageDocsGrid = new Enterprise.DocumentScanning.GUI.DocumentsZGrid();
			this.postingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.offlineLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.gridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.storageDocsGrid)).BeginInit();
			this.postingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 540, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain);
			// 
			// previewPanel
			// 
			this.previewPanel.AutoSize = true;
			this.previewPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.previewPanel.Name = "previewPanel";
			this.previewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 538, true);
			this.previewPanel.TabIndex = 1;
			// 
			// mainPanel
			// 
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mainPanel.Controls.Add(this.splitContainer1);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 538, true);
			this.mainPanel.TabIndex = 4;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.previewPanel);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.gridPanel);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 538, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(499);
			this.splitContainer1.TabIndex = 5;
			// 
			// gridPanel
			// 
			this.gridPanel.Controls.Add(this.storageDocsGrid);
			this.gridPanel.Controls.Add(this.postingPanel);
			this.gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridPanel.Name = "gridPanel";
			this.gridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 538, true);
			this.gridPanel.TabIndex = 3;
			// 
			// storageDocsGrid
			// 
			this.storageDocsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.storageDocsGrid, "eDocsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_DocType_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_FileNameWithExtension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_FriendlyFileDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain)(null)).eDocsView)).SyncRoot)).SC_IsSystemGenerated)));
			this.storageDocsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "SC_Date";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "Date that this eDoc was last changed";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo11.ColumnName = "SC_DocType";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.ToolTip = "The 3-letter Document Type for this eDoc";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchivedRecordForm|3bba8b27-3b2a-474f-86a4-effdc9165ecb", "Doc Type Description");
			zTextBoxColumnStyleInfo12.ColumnName = "SC_DocType_Description";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo13.ColumnName = "SC_Desc";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.ToolTip = "Description for this eDoc";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchivedRecordForm|ab68f83a-5ded-454d-885d-6d5190ade01a", "File Name");
			zTextBoxColumnStyleInfo14.ColumnName = "SC_FileNameWithExtension";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.ToolTip = "Filename for this eDoc, if applicable";
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchivedRecordForm|6214c13a-a841-49fd-b3ad-4be5e9681e00", "Program Desc.", "Program Description");
			zTextBoxColumnStyleInfo15.ColumnName = "SC_FriendlyFileDescription";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.ToolTip = "Program used to open this file";
			zCheckBoxColumnStyleInfo3.ColumnName = "SC_IsSystemGenerated";
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.ToolTip = resources.GetString("zCheckBoxColumnStyleInfo3.ToolTip");
			this.storageDocsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.storageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.storageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.storageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.storageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.storageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.storageDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.storageDocsGrid.GridId = "fe3823fa-4028-4914-ab79-0c89481415d4";
			this.storageDocsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.storageDocsGrid.DocumentManipulationTarget = null;
			this.storageDocsGrid.DragDropTarget = null;
			this.storageDocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.storageDocsGrid.LayoutKey = "storageDocsGrid";
			this.storageDocsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.storageDocsGrid.Name = "storageDocsGrid";
			this.storageDocsGrid.ReadOnly = true;
			this.storageDocsGrid.ShowCopyMenuItem = true;
			this.storageDocsGrid.ShowCopyLinkMenuItem = true;
			this.storageDocsGrid.ShowDeliverDocumentMenuItem = true;
			this.storageDocsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 508, true);
			this.storageDocsGrid.TabIndex = 2;
			// 
			// postingPanel
			// 
			this.postingPanel.AutoSize = true;
			this.postingPanel.Controls.Add(this.okButton);
			this.postingPanel.Controls.Add(this.offlineLabel);
			this.postingPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.postingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 508, true);
			this.postingPanel.Name = "postingPanel";
			this.postingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 30, true);
			this.postingPanel.TabIndex = 5;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchivedRecordForm|c222b626-c526-44c6-806b-ab85d4d6da4f", "Close");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 4, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// offlineLabel
			// 
			this.offlineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.offlineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.offlineLabel.Name = "offlineLabel";
			this.offlineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 23, true);
			this.offlineLabel.TabIndex = 4;
			// 
			// ArchivedRecordForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.okButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 564, true);
			this.CaptionResourceString = Enterprise.ArchiveManager.GUI.Res.GetData("ArchivedRecordForm|13d1b86c-3b01-4162-a8b4-26256e071065", "Archived Record");
			this.Controls.Add(this.mainPanel);
			this.DataSourceType = typeof(Enterprise.ArchiveManager.Business.Records.ArchiveStorageMain);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 450, true);
			this.Name = "ArchivedRecordForm";
			this.Text = "ArchivedRecordForm";
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.gridPanel.ResumeLayout(false);
			this.gridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.storageDocsGrid)).EndInit();
			this.postingPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel previewPanel;
		private Enterprise.DocumentScanning.GUI.DocumentsZGrid storageDocsGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel mainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel gridPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel postingPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private Enterprise.ZArchitecture.ZLabel offlineLabel;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
	}
}
