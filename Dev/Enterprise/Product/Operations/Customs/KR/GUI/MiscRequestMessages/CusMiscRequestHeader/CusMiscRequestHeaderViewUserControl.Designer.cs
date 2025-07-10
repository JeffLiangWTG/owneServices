namespace Enterprise.Customs.KR.GUI
{
	partial class CusMiscRequestHeaderViewUserControl
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
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
      this.HeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.DynamicHeaderDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.RequestLinesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
      this.CusMiscRequestLinesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
      this.RequestLinePaddingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
      this.DynamicRequestLinePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.LineDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.HeaderDetailsGroupBox.SuspendLayout();
      this.RequestLinesPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.CusMiscRequestLinesBoundGrid)).BeginInit();
      this.CusMiscRequestLinesBoundGrid.SuspendLayout();
      this.RequestLinePaddingPanel.SuspendLayout();
      this.LineDetailsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusMiscRequestHeader);
      // 
      // HeaderDetailsGroupBox
      // 
      this.HeaderDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ea6d86de-6150-4c62-b706-9d4bbc8c9b23", "Details");
      this.HeaderDetailsGroupBox.Controls.Add(this.DynamicHeaderDetailsPanel);
      this.HeaderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.HeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.HeaderDetailsGroupBox.Name = "HeaderDetailsGroupBox";
      this.HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 193, true);
      this.HeaderDetailsGroupBox.TabIndex = 0;
      this.HeaderDetailsGroupBox.TabStop = false;
      // 
      // DynamicHeaderDetailsPanel
      // 
      this.DynamicHeaderDetailsPanel.AllowDrop = true;
      this.DynamicHeaderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DynamicHeaderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
      this.DynamicHeaderDetailsPanel.Name = "DynamicHeaderDetailsPanel";
      this.DynamicHeaderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 178, true);
      this.DynamicHeaderDetailsPanel.TabIndex = 0;
      // 
      // RequestLinesPanel
      // 
      this.RequestLinesPanel.Controls.Add(this.CusMiscRequestLinesBoundGrid);
      this.RequestLinesPanel.Controls.Add(this.RequestLinePaddingPanel);
      this.RequestLinesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RequestLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
      this.RequestLinesPanel.Name = "RequestLinesPanel";
      this.RequestLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 237, true);
      this.RequestLinesPanel.TabIndex = 1;
      // 
      // CusMiscRequestLinesBoundGrid
      // 
      this.CusMiscRequestLinesBoundGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.CusMiscRequestLinesBoundGrid, "RequestLines");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).EntryType)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).FormattedEntryNumber)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).RequestLines)).SyncRoot)).CML_Remarks)));
      this.CusMiscRequestLinesBoundGrid.CaptionVisible = false;
      zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
      zTextBoxColumnStyleInfo1.ColumnName = "EntryType";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
      zTextBoxColumnStyleInfo2.ColumnName = "FormattedEntryNumber";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zMultiLineTextBoxColumnInfo1.ColumnName = "CML_Remarks";
      zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
      zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
      zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
      this.CusMiscRequestLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.CusMiscRequestLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.CusMiscRequestLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
      this.CusMiscRequestLinesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.CusMiscRequestLinesBoundGrid.GridId = "a35f77cb-e3bb-458a-9629-bb572247aad4";
      this.CusMiscRequestLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.CusMiscRequestLinesBoundGrid.LayoutKey = "MiscRequestLinesBoundGrid";
      this.CusMiscRequestLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.CusMiscRequestLinesBoundGrid.Name = "CusMiscRequestLinesBoundGrid";
      this.CusMiscRequestLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 237, true);
      this.CusMiscRequestLinesBoundGrid.TabIndex = 2;
      // 
      // RequestLinePaddingPanel
      // 
      this.RequestLinePaddingPanel.Controls.Add(this.DynamicRequestLinePanel);
      this.RequestLinePaddingPanel.Dock = System.Windows.Forms.DockStyle.Right;
      this.RequestLinePaddingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 0, true);
      this.RequestLinePaddingPanel.Name = "RequestLinePaddingPanel";
      this.RequestLinePaddingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 237, true);
      this.RequestLinePaddingPanel.TabIndex = 3;
      // 
      // DynamicRequestLinePanel
      // 
      this.DynamicRequestLinePanel.AllowDrop = true;
      this.DynamicRequestLinePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.DynamicRequestLinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 1, true);
      this.DynamicRequestLinePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
      this.DynamicRequestLinePanel.Name = "DynamicRequestLinePanel";
      this.DynamicRequestLinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 235, true);
      this.DynamicRequestLinePanel.TabIndex = 0;
      // 
      // LineDetailsGroupBox
      // 
      this.LineDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("af4a0926-46a5-4dd1-a709-33f625763461", "Related Entries");
      this.LineDetailsGroupBox.Controls.Add(this.RequestLinesPanel);
      this.LineDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.LineDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 193, true);
      this.LineDetailsGroupBox.Name = "LineDetailsGroupBox";
      this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 252, true);
      this.LineDetailsGroupBox.TabIndex = 1;
      this.LineDetailsGroupBox.TabStop = false;
      // 
      // ExtendedHoursRequestViewUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.LineDetailsGroupBox);
      this.Controls.Add(this.HeaderDetailsGroupBox);
      this.Name = "ExtendedHoursRequestViewUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 445, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.HeaderDetailsGroupBox.ResumeLayout(false);
      this.HeaderDetailsGroupBox.PerformLayout();
      this.RequestLinesPanel.ResumeLayout(false);
      this.RequestLinesPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.CusMiscRequestLinesBoundGrid)).EndInit();
      this.CusMiscRequestLinesBoundGrid.ResumeLayout(false);
      this.CusMiscRequestLinesBoundGrid.PerformLayout();
      this.RequestLinePaddingPanel.ResumeLayout(false);
      this.RequestLinePaddingPanel.PerformLayout();
      this.LineDetailsGroupBox.ResumeLayout(false);
      this.LineDetailsGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox HeaderDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox LineDetailsGroupBox;
		private ZArchitecture.GUI.ZPanel RequestLinesPanel;
		private ZArchitecture.GUI.ZPanel RequestLinePaddingPanel;
		private Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicHeaderDetailsPanel;
		private Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicRequestLinePanel;
		private ZArchitecture.ZGrid CusMiscRequestLinesBoundGrid;
	}
}
