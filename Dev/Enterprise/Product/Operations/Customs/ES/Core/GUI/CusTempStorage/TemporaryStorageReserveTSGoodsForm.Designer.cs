namespace Enterprise.Customs.ES.GUI
{
	partial class TemporaryStorageReserveTSGoodsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalPackagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalGrossWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.DataGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 489, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.BottomPanel);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 141, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 346, true);
			this.MainPanel.TabIndex = 0;
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Controls.Add(this.DataGroupBox);
			this.MainPanel.Controls.Add(this.LinesGrid);
			// 
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.TotalPackagesLabel);
			this.BottomPanel.Controls.Add(this.TotalGrossWeightLabel);
			this.BottomPanel.Controls.Add(this.SaveButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 40, true);
			this.BottomPanel.TabIndex = 2;
			//
			// TotalPackagesLabel
			//
			this.TotalPackagesLabel.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("3E7016D8-E6D6-4489-867E-B5D7932BFDB7", "Total Packages:");
			this.TotalPackagesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TotalPackagesLabel.IsFontBold = true;
			this.TotalPackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.TotalPackagesLabel.Name = "TotalPackagesLabel";
			this.TotalPackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.TotalPackagesLabel.TabIndex = 0;
			this.TotalPackagesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.TotalPackagesLabel.AutoSize = true;
			// 
			// TotalGrossWeightLabel
			//
			this.TotalGrossWeightLabel.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("{2E545A40-B6C2-41AF-BB9C-A0A38D7F701D}", "Total Gross Weight:");
			this.TotalGrossWeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TotalGrossWeightLabel.IsFontBold = true;
			this.TotalGrossWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.TotalGrossWeightLabel.Name = "TotalGrossWeightLabel";
			this.TotalGrossWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.TotalGrossWeightLabel.TabIndex = 0;
			this.TotalGrossWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.TotalGrossWeightLabel.AutoSize = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.BackColor = System.Drawing.SystemColors.Control;
			this.SaveButton.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("DF633F01-A670-49C7-9009-757F4FC4235A", "&Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 6, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = false;
			this.SaveButton.Click += new System.EventHandler(this.OnSaveButtonClick);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("1F5A1439-F8F5-48E8-9941-96B8DA1D1BF5", "&Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(723, 6, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = false;
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
			// 
			// DataGroupBox
			// 
			this.DataGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("6AFB9388-CC98-4FAF-A870-BDDEBB0148E1", "Lines");
			this.DataGroupBox.Controls.Add(this.LinesGrid);
			this.DataGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DataGroupBox.Name = "DataGroupBox";
			this.DataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 161, true);
			this.DataGroupBox.TabIndex = 2;
			this.DataGroupBox.TabStop = false;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = true;
			this.BindingSource.SetBindingMember(this.LinesGrid, "ReserveTsGoodsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).TSDNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).TSDItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).RemainingPackageQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).PackagesToUse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).RemainingGrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader)(null)).ReserveTSGoodsCollection)).SyncRoot)).GrossWeightToUse)));
			this.LinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TSDNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "TSDItemNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "PackageType";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "Location";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "Reference";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RemainingPackageQty";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "RemainingGrossWeight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "PackagesToUse";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.AllowNegative = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "GrossWeightToUse";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.AllowNegative = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.Decimals = 6;
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "9DEA7787-4A45-4EBF-97FA-C7A623F6D0BF";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 144, true);
			this.LinesGrid.TabIndex = 0;
			// 
			// TemporaryStorageReserveTSGoodsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 513, true);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegHeader);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 550, true);
			this.Name = "TemporaryStorageReserveTSGoodsForm";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.DataGroupBox.ResumeLayout(false);
			this.DataGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton SaveButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZGroupBox DataGroupBox;
		private ZArchitecture.ZGrid LinesGrid;
		private ZArchitecture.ZLabel TotalPackagesLabel;
		private ZArchitecture.ZLabel TotalGrossWeightLabel;
	}
}
