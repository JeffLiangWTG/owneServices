namespace Enterprise.Customs.KR.GUI
{
	partial class FinalPriceExtensionRequestNewUserControl
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.FinalPriceExtensionRequestHeaderPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.LineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.FinalPriceExtensionRequestLinesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.HeaderGroupBox.SuspendLayout();
            this.LineGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FinalPriceExtensionRequestLinesBoundGrid)).BeginInit();
            this.FinalPriceExtensionRequestLinesBoundGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader);
            // 
            // HeaderGroupBox
            // 
            this.HeaderGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3D38FA7C-299D-425C-BEF1-E9C22C949EDF", "Details");
            this.HeaderGroupBox.Controls.Add(this.FinalPriceExtensionRequestHeaderPanel);
            this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.HeaderGroupBox.Name = "HeaderGroupBox";
            this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 123, true);
            this.HeaderGroupBox.TabIndex = 0;
            this.HeaderGroupBox.TabStop = false;
            // 
            // FinalPriceExtensionRequestHeaderPanel
            // 
            this.FinalPriceExtensionRequestHeaderPanel.AllowDrop = true;
            this.FinalPriceExtensionRequestHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FinalPriceExtensionRequestHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.FinalPriceExtensionRequestHeaderPanel.Name = "FinalPriceExtensionRequestHeaderPanel";
            this.FinalPriceExtensionRequestHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 106, true);
            this.FinalPriceExtensionRequestHeaderPanel.TabIndex = 0;
            // 
            // LineGroupBox
            // 
            this.LineGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4AB134E2-167D-4003-8595-765F5AC61E56", "Related Entries");
            this.LineGroupBox.Controls.Add(this.FinalPriceExtensionRequestLinesBoundGrid);
            this.LineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 123, true);
            this.LineGroupBox.Name = "LineGroupBox";
            this.LineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 336, true);
            this.LineGroupBox.TabIndex = 1;
            this.LineGroupBox.TabStop = false;
            // 
            // FinalPriceExtensionRequestLinesBoundGrid
            // 
            this.FinalPriceExtensionRequestLinesBoundGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.FinalPriceExtensionRequestLinesBoundGrid, "FinalPriceReportByDateExtensionLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader)(null)).FinalPriceReportByDateExtensionLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader)(null)).FinalPriceReportByDateExtensionLines)).SyncRoot)).ImportDeclarationNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader)(null)).FinalPriceReportByDateExtensionLines)).SyncRoot)).ExtensionDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader)(null)).FinalPriceReportByDateExtensionLines)).SyncRoot)).ApplicationReason)));
            this.FinalPriceExtensionRequestLinesBoundGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "ImportDeclarationNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zDateEditColumnStyleInfo1.ColumnName = "ExtensionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsMandatory = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zMultiLineTextBoxColumnInfo1.ColumnName = "ApplicationReason";
            zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
            this.FinalPriceExtensionRequestLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.FinalPriceExtensionRequestLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.FinalPriceExtensionRequestLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
            this.FinalPriceExtensionRequestLinesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FinalPriceExtensionRequestLinesBoundGrid.GridId = "a185bdfe-5902-407e-a3f0-f88267d6227e";
            this.FinalPriceExtensionRequestLinesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.FinalPriceExtensionRequestLinesBoundGrid.LayoutKey = "LineGrid";
            this.FinalPriceExtensionRequestLinesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.FinalPriceExtensionRequestLinesBoundGrid.Name = "FinalPriceExtensionRequestLinesBoundGrid";
            this.FinalPriceExtensionRequestLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 319, true);
            this.FinalPriceExtensionRequestLinesBoundGrid.TabIndex = 0;
            // 
            // FinalPriceExtensionRequestNewUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.LineGroupBox);
            this.Controls.Add(this.HeaderGroupBox);
            this.Name = "FinalPriceExtensionRequestNewUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 459, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.HeaderGroupBox.ResumeLayout(false);
            this.HeaderGroupBox.PerformLayout();
            this.LineGroupBox.ResumeLayout(false);
            this.LineGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FinalPriceExtensionRequestLinesBoundGrid)).EndInit();
            this.FinalPriceExtensionRequestLinesBoundGrid.ResumeLayout(false);
            this.FinalPriceExtensionRequestLinesBoundGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private ZArchitecture.GUI.ZGroupBox LineGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel FinalPriceExtensionRequestHeaderPanel;
		private ZArchitecture.ZGrid FinalPriceExtensionRequestLinesBoundGrid;
	}
}
