namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class ImportFromSailingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImportSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LegendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReplaceDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReplaceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeleteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LegendSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.BillsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportSplitContainer)).BeginInit();
			this.ImportSplitContainer.Panel1.SuspendLayout();
			this.ImportSplitContainer.Panel2.SuspendLayout();
			this.ImportSplitContainer.SuspendLayout();
			this.LegendGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).BeginInit();
			this.LegendSplitContainer.Panel1.SuspendLayout();
			this.LegendSplitContainer.Panel2.SuspendLayout();
			this.LegendSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 485, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.BillImportActionCollection);
			// 
			// NotificationProvider
			// 
			this.NotificationProvider.NotificationRenderer = null;
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.BillImportAction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.Business.BillImportAction)(null)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.BillImportAction)(null)).ActionDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.BillImportAction)(null)).BillNumber)));
			this.BillsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("22C126E8-ADDB-4416-8783-57F54A711829", "Selected?");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo1.IsCustomColumn = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("5B4B00C5-294B-4546-B2F2-9C9E5F1D1049", "Action");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ActionDesc";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("744871A6-1250-4FC7-B8C4-44164A5F9AAA", "Bill Of Lading");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "31CB27BE-E67E-4637-A53B-E1CBAE637DDA";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "billsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 346, true);
			this.BillsGrid.TabIndex = 1;
			// 
			// ImportSplitContainer
			// 
			this.ImportSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.ImportSplitContainer.IsSplitterFixed = true;
			this.ImportSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportSplitContainer.Name = "ImportSplitContainer";
			this.ImportSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ImportSplitContainer.Panel1
			// 
			this.ImportSplitContainer.Panel1.Controls.Add(this.LegendGroupBox);
			// 
			// ImportSplitContainer.Panel2
			// 
			this.ImportSplitContainer.Panel2.Controls.Add(this.SelectAllButton);
			this.ImportSplitContainer.Panel2.Controls.Add(this.DeselectAllButton);
			this.ImportSplitContainer.Panel2.Controls.Add(this.OKButton);
			this.ImportSplitContainer.Panel2.Controls.Add(this.CancelAllButton);
			this.ImportSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 135, true);
			this.ImportSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(102);
			this.ImportSplitContainer.TabIndex = 2;
			// 
			// LegendGroupBox
			// 
			this.LegendGroupBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("9A864917-036D-4631-B041-7316B3A01A87", "Legend");
			this.LegendGroupBox.Controls.Add(this.ReplaceDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.ReplaceLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteDescriptionLabel);
			this.LegendGroupBox.Controls.Add(this.DeleteLabel);
			this.LegendGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegendGroupBox.Name = "LegendGroupBox";
			this.LegendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 102, true);
			this.LegendGroupBox.TabIndex = 0;
			this.LegendGroupBox.TabStop = false;
			// 
			// ReplaceDescriptionLabel
			// 
			this.ReplaceDescriptionLabel.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("9ff57fae-df80-4623-bd2d-ad7738fb8117", "If selected, these bills will be deleted then re-added.");
			this.ReplaceDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ReplaceDescriptionLabel.IsFontBold = true;
			this.ReplaceDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
			this.ReplaceDescriptionLabel.Name = "ReplaceDescriptionLabel";
			this.ReplaceDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 34, true);
			this.ReplaceDescriptionLabel.TabIndex = 4;
			// 
			// ReplaceLabel
			// 
			this.ReplaceLabel.AutoSize = true;
			this.ReplaceLabel.BackColor = System.Drawing.Color.Transparent;
			this.ReplaceLabel.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("11ac12e2-f98e-430a-91c1-6ff057f0238a", "REPLACE");
			this.ReplaceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ReplaceLabel.IsFontBold = true;
			this.ReplaceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 76, true);
			this.ReplaceLabel.Name = "ReplaceLabel";
			this.ReplaceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.ReplaceLabel.TabIndex = 3;
			// 
			// DeleteDescriptionLabel
			// 
			this.DeleteDescriptionLabel.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("6871fe21-6d58-431d-9366-62d73886eb84", "These bills do not existing on the sailing. Select Delete to remove them permanently from the Manifest job.");
			this.DeleteDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DeleteDescriptionLabel.IsFontBold = true;
			this.DeleteDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 9, true);
			this.DeleteDescriptionLabel.Name = "DeleteDescriptionLabel";
			this.DeleteDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 45, true);
			this.DeleteDescriptionLabel.TabIndex = 2;
			// 
			// DeleteLabel
			// 
			this.DeleteLabel.AutoSize = true;
			this.DeleteLabel.BackColor = System.Drawing.Color.Transparent;
			this.DeleteLabel.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("248b11e6-7e99-4d3a-abbd-499915537ebb", "DELETE");
			this.DeleteLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DeleteLabel.IsFontBold = true;
			this.DeleteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.DeleteLabel.Name = "DeleteLabel";
			this.DeleteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.DeleteLabel.TabIndex = 1;
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("A6CE2465-2C5F-450E-9392-560FD3753F74", "Select All");
			this.SelectAllButton.IsCaptionOverridden = false;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 3, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeselectAllButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("DB2A1981-8110-48EF-8FC8-3BD3BEE0989B", "Deselect All");
			this.DeselectAllButton.IsCaptionOverridden = false;
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 3, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeselectAllButton.TabIndex = 1;
			this.DeselectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DeselectAllButton.ToolTipCaption = null;
			this.DeselectAllButton.UseVisualStyleBackColor = true;
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("246B280A-22F3-4B51-B8CB-FCDE69E94C54", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 3, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// CancelAllButton
			// 
			this.CancelAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelAllButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("EBBA5573-18E0-4A97-902E-3C9DD2291B2A", "Cancel");
			this.CancelAllButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAllButton.IsCaptionOverridden = false;
			this.CancelAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 3, true);
			this.CancelAllButton.Name = "CancelAllButton";
			this.CancelAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAllButton.TabIndex = 3;
			this.CancelAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelAllButton.ToolTipCaption = null;
			this.CancelAllButton.UseVisualStyleBackColor = true;
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
			this.LegendSplitContainer.Panel1.Controls.Add(this.BillsGrid);
			// 
			// LegendSplitContainer.Panel2
			// 
			this.LegendSplitContainer.Panel2.Controls.Add(this.ImportSplitContainer);
			this.LegendSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 485, true);
			this.LegendSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(346);
			this.LegendSplitContainer.TabIndex = 3;
			// 
			// ImportFromSailingForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAllButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("64946380-F418-4F2F-9C06-D04D3612D014", "Existing Bills Actions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 509, true);
			this.Controls.Add(this.LegendSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.BillImportActionCollection);
			this.MinimizeBox = false;
			this.Name = "ImportFromSailingForm";
			this.RememberFormSize = false;
			this.Text = "Existing Bills Actions";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LegendSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsGrid.ResumeLayout(false);
			this.BillsGrid.PerformLayout();
			this.ImportSplitContainer.Panel1.ResumeLayout(false);
			this.ImportSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ImportSplitContainer)).EndInit();
			this.ImportSplitContainer.ResumeLayout(false);
			this.ImportSplitContainer.PerformLayout();
			this.LegendGroupBox.ResumeLayout(false);
			this.LegendGroupBox.PerformLayout();
			this.LegendSplitContainer.Panel1.ResumeLayout(false);
			this.LegendSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LegendSplitContainer)).EndInit();
			this.LegendSplitContainer.ResumeLayout(false);
			this.LegendSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid BillsGrid;
		CargoWise.Windows.UI.KSplitContainer ImportSplitContainer;
		ZArchitecture.GUI.ZButton SelectAllButton;
		ZArchitecture.GUI.ZButton DeselectAllButton;
		ZArchitecture.GUI.ZButton OKButton;
		ZArchitecture.GUI.ZButton CancelAllButton;
		CargoWise.Windows.UI.KSplitContainer LegendSplitContainer;
		ZArchitecture.GUI.ZGroupBox LegendGroupBox;
		ZArchitecture.ZLabel DeleteDescriptionLabel;
		ZArchitecture.ZLabel DeleteLabel;
		ZArchitecture.ZLabel ReplaceDescriptionLabel;
		ZArchitecture.ZLabel ReplaceLabel;
	}
}
