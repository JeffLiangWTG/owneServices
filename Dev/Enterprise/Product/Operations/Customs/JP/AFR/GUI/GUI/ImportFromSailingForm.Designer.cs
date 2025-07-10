namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class ImportFromSailingForm
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
		/// 
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.billsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.importSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LegendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoteDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReplaceDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReplaceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.deselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LegendSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.importSplitContainer)).BeginInit();
			this.importSplitContainer.Panel1.SuspendLayout();
			this.importSplitContainer.Panel2.SuspendLayout();
			this.importSplitContainer.SuspendLayout();
			this.LegendGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).BeginInit();
			this.LegendSplitContainer.Panel1.SuspendLayout();
			this.LegendSplitContainer.Panel2.SuspendLayout();
			this.LegendSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 532, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BillImportActionCollection);
			// 
			// NotificationProvider
			// 
			this.NotificationProvider.NotificationRenderer = null;
			// 
			// billsGrid
			// 
			this.billsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.billsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BillImportAction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.BillImportAction)(null)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BillImportAction)(null)).ActionDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BillImportAction)(null)).BillNumber)));
			this.billsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("6082CE76-9BBE-475A-BB90-28DABE7025AD", "Selected?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("D0F9E514-ED71-43E8-BB70-EF355E4866F7", "Action");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ActionDesc";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("F65345FB-5AE3-4EB2-8A40-A09151AE4DE0", "Bill Of Lading");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.billsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.billsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.billsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.billsGrid.CopySelectedRowsAllowed = true;
			this.billsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billsGrid.GridId = "b3281d42-b4fd-45be-8733-52cc0c4af731";
			this.billsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.billsGrid.LayoutKey = "JPAFR|ImportBillsFromSailing";
			this.billsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billsGrid.Name = "billsGrid";
			this.billsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 279, true);
			this.billsGrid.TabIndex = 1;
			// 
			// importSplitContainer
			// 
			this.importSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.importSplitContainer.IsSplitterFixed = true;
			this.importSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importSplitContainer.Name = "importSplitContainer";
			this.importSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// importSplitContainer.Panel1
			// 
			this.importSplitContainer.Panel1.Controls.Add(this.LegendGroupBox);
			// 
			// importSplitContainer.Panel2
			// 
			this.importSplitContainer.Panel2.Controls.Add(this.selectAllButton);
			this.importSplitContainer.Panel2.Controls.Add(this.deselectAllButton);
			this.importSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.importSplitContainer.Panel2.Controls.Add(this.cancelButton);
			this.importSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 249, true);
			this.importSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(216);
			this.importSplitContainer.TabIndex = 2;
			// 
			// LegendGroupBox
			// 
			this.LegendGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("B4472C03-F615-4DA3-ACCF-4D8DEA8C6A5D", "Legend");
			this.LegendGroupBox.Controls.Add(this.NoteDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.ReplaceDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.ReplaceLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteLabel);
			this.LegendGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegendGroupBox.Name = "LegendGroupBox";
			this.LegendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 216, true);
			this.LegendGroupBox.TabIndex = 0;
			this.LegendGroupBox.TabStop = false;
			// 
			// NoteDescriptionLabel
			// 
			this.NoteDescriptionLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("bb30d2c7-6e74-441e-b83e-82136f5ea331", "Note:\r\nYou will not be able to select a Bill to Delete if the bill is currently awaiting for response or it has already been registered.\r\nPlease double check the values of the Bills once the importing is done and make sure the \'Is Master Bill\' is ticked accordingly for each Bill.");
			this.NoteDescriptionLabel.IsFontBold = true;
			this.NoteDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 99, true);
			this.NoteDescriptionLabel.Name = "NoteDescriptionLabel";
			this.NoteDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 114, true);
			this.NoteDescriptionLabel.TabIndex = 5;
			// 
			// ReplaceDescriptionLabel
			// 
			this.ReplaceDescriptionLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("ED017932-A914-406B-8F61-35684CA9D72D", "If selected, these bills will be deleted then re-added.");
			this.ReplaceDescriptionLabel.IsFontBold = true;
			this.ReplaceDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
			this.ReplaceDescriptionLabel.Name = "ReplaceDescriptionLabel";
			this.ReplaceDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 34, true);
			this.ReplaceDescriptionLabel.TabIndex = 4;
			// 
			// ReplaceLabel
			//
			this.ReplaceLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("ab5a4e63-a229-4c59-8ea2-e873e2ce8290", "REPLACE");
			this.ReplaceLabel.AutoSize = true;
			this.ReplaceLabel.BackColor = System.Drawing.Color.Transparent;
			this.ReplaceLabel.IsFontBold = true;
			this.ReplaceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 76, true);
			this.ReplaceLabel.Name = "ReplaceLabel";
			this.ReplaceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.ReplaceLabel.TabIndex = 3;
			// 
			// DeleteDescriptionLabel
			// 
			this.DeleteDescriptionLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("0C9ECD68-5691-4D31-9DC0-3DF4AC169329", "These bills do not existing on the sailing. Select Delete to remove them permanently from the AFR job.");
			this.DeleteDescriptionLabel.IsFontBold = true;
			this.DeleteDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 9, true);
			this.DeleteDescriptionLabel.Name = "DeleteDescriptionLabel";
			this.DeleteDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 45, true);
			this.DeleteDescriptionLabel.TabIndex = 2;
			// 
			// DeleteLabel
			//
			this.DeleteLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4681BADE-C234-48FF-A929-30188A708507", "DELETE");
			this.DeleteLabel.AutoSize = true;
			this.DeleteLabel.BackColor = System.Drawing.Color.Transparent;
			this.DeleteLabel.IsFontBold = true;
			this.DeleteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.DeleteLabel.Name = "DeleteLabel";
			this.DeleteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.DeleteLabel.TabIndex = 1;
			// 
			// selectAllButton
			// 
			this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("A7FCDF34-BB4B-4E5F-BCC3-85DAEF011A62", "Select All");
			this.selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 3, true);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.selectAllButton.TabIndex = 0;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// deselectAllButton
			// 
			this.deselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deselectAllButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("FB50158B-A5A1-4832-AB15-6074625AF73B", "Deselect All");
			this.deselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 3, true);
			this.deselectAllButton.Name = "deselectAllButton";
			this.deselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deselectAllButton.TabIndex = 1;
			this.deselectAllButton.UseVisualStyleBackColor = true;
			this.deselectAllButton.Click += new System.EventHandler(this.deselectAllButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("7A285F74-FA78-442B-B2E1-D88A999EB00D", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("A4096F75-6102-437A-853E-152DE2C04799", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 3, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// LegendSplitContainer
			// 
			this.LegendSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegendSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.LegendSplitContainer.IsSplitterFixed = true;
			this.LegendSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegendSplitContainer.Name = "LegendSplitContainer";
			this.LegendSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// LegendSplitContainer.Panel1
			// 
			this.LegendSplitContainer.Panel1.Controls.Add(this.billsGrid);
			// 
			// LegendSplitContainer.Panel2
			// 
			this.LegendSplitContainer.Panel2.Controls.Add(this.importSplitContainer);
			this.LegendSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 532, true);
			this.LegendSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(279);
			this.LegendSplitContainer.TabIndex = 3;
			// 
			// ImportFromSailingForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("782A4BBD-6063-49C8-AF75-F74485AC97F5", "Existing Bills Actions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 556, true);
			this.Controls.Add(this.LegendSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BillImportActionCollection);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 500, true);
			this.Name = "ImportFromSailingForm";
			this.RememberFormSize = false;
			this.Text = "Existing Bills Actions";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LegendSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).EndInit();
			this.importSplitContainer.Panel1.ResumeLayout(false);
			this.importSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.importSplitContainer)).EndInit();
			this.importSplitContainer.ResumeLayout(false);
			this.LegendGroupBox.ResumeLayout(false);
			this.LegendGroupBox.PerformLayout();
			this.LegendSplitContainer.Panel1.ResumeLayout(false);
			this.LegendSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).EndInit();
			this.LegendSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid billsGrid;
		private System.Windows.Forms.SplitContainer importSplitContainer;
		private ZArchitecture.GUI.ZButton selectAllButton;
		private ZArchitecture.GUI.ZButton deselectAllButton;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private System.Windows.Forms.SplitContainer LegendSplitContainer;
		private ZArchitecture.GUI.ZGroupBox LegendGroupBox;
		private ZArchitecture.ZLabel DeleteDescriptionLabel;
		private ZArchitecture.ZLabel DeleteLabel;
		private ZArchitecture.ZLabel ReplaceDescriptionLabel;
		private ZArchitecture.ZLabel ReplaceLabel;
		private ZArchitecture.ZLabel NoteDescriptionLabel;
	}
}
