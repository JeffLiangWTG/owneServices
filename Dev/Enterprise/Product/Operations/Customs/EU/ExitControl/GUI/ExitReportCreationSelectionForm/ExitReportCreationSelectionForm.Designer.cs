namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ExitReportCreationSelectionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClearAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.GridsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.ClearAllButton);
			this.ButtonsPanel.Controls.Add(this.CancelButton);
			this.ButtonsPanel.Controls.Add(this.SelectAllButton);
			this.ButtonsPanel.Controls.Add(this.OKButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 393, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 33, true);
			this.ButtonsPanel.TabIndex = 3;
			// 
			// ClearAllButton
			// 
			this.ClearAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearAllButton.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("8d911662-c59c-4850-ba6f-4374ffb5fa80", "Clear Selection");
			this.ClearAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 4, true);
			this.ClearAllButton.Name = "ClearAllButton";
			this.ClearAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.ClearAllButton.TabIndex = 0;
			this.ClearAllButton.ToolTipCaption = null;
			this.ClearAllButton.UseVisualStyleBackColor = true;
			this.ClearAllButton.Click += new System.EventHandler(this.ClearAllButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("89cdda62-ddf4-4d8b-96ec-90abab96eb57", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(861, 4, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("ff98585e-3d5f-472f-a551-31dc6a05d9ef", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 4, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("6846f44f-95b2-48ac-93f1-81cf7eebe13f", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(753, 4, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("d4aa7652-54cc-442c-bea8-ed62882e782e", "Items");
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 393, true);
			this.ItemsGroupBox.TabIndex = 5;
			this.ItemsGroupBox.TabStop = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "CusExitConsignmentItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CCI_LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CCI_Calc_ReportGrossMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CCI_Calc_ReportNetMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CCI_UniqueConsignmentReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CCI_Calc_ShouldReportItem)));
			this.ItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CCI_LineNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CCI_Calc_ReportGrossMass";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CCI_Calc_ReportNetMass";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CCI_UniqueConsignmentReference";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo1.ColumnName = "CCI_Calc_ShouldReportItem";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ItemsGrid.DisableImportDataMenuItem = true;
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 374, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("b1b93398-8fb5-48d1-9ef9-06c7d663772e", "Packing Details");
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 0, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 393, true);
			this.PackingDetailsGroupBox.TabIndex = 6;
			this.PackingDetailsGroupBox.TabStop = false;
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingDetailsGrid, "CusExitConsignmentItems.CusExitConsignmentPackagePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_Calc_ReportQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment)(null)).CusExitConsignmentItems)).SyncRoot)).CusExitConsignmentPackagePivots)).SyncRoot)).Package.CXP_Calc_ShouldReportItem)));
			this.PackingDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "Package+CXP_Sequence";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Package+CXP_Calc_ReportQuantity";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "Package+CXP_PackageType";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "Package+CXP_MarksAndNumbers";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.ColumnName = "Package+CXP_Calc_ShouldReportItem";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackingDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PackingDetailsGrid.DisableImportDataMenuItem = true;
			this.PackingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 374, true);
			this.PackingDetailsGrid.TabIndex = 1;
			// 
			// GridsPanel
			// 
			this.GridsPanel.Controls.Add(this.PackingDetailsGroupBox);
			this.GridsPanel.Controls.Add(this.ItemsGroupBox);
			this.GridsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridsPanel.Name = "GridsPanel";
			this.GridsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 393, true);
			this.GridsPanel.TabIndex = 11;
			// 
			// ExitReportCreationSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("e3729d58-3956-43d0-b6e3-c0655bc65734", "Selection Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 450, true);
			this.Controls.Add(this.GridsPanel);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 689, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 389, true);
			this.Name = "ExitReportCreationSelectionForm";
			this.Text = "Selection Form";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.GridsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.GridsPanel.ResumeLayout(false);
			this.GridsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.ZGrid ItemsGrid;
		private ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
		private ZArchitecture.GUI.ZGroupBox PackingDetailsGroupBox;
		private ZArchitecture.ZGrid PackingDetailsGrid;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton SelectAllButton;
		private ZArchitecture.GUI.ZPanel GridsPanel;
		private ZArchitecture.GUI.ZButton ClearAllButton;
	}
}
