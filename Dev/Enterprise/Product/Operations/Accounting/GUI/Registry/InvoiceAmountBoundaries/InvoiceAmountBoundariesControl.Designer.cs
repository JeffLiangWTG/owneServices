namespace Enterprise.Accounting.Registry.GUI
{
	partial class InvoiceAmountBoundariesControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoiceAmountBoundaryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceAmountBoundaryGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AmountsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InvoiceAmountBoundaryLabelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceAmountBoundaryLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceAmountBoundaryPanel.SuspendLayout();
			this.InvoiceAmountBoundaryGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AmountsGrid)).BeginInit();
			this.InvoiceAmountBoundaryLabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.InvoiceAmountBoundaryCollection);
			// 
			// InvoiceAmountBoundaryPanel
			// 
			this.InvoiceAmountBoundaryPanel.Controls.Add(this.InvoiceAmountBoundaryGridPanel);
			this.InvoiceAmountBoundaryPanel.Controls.Add(this.InvoiceAmountBoundaryLabelPanel);
			this.InvoiceAmountBoundaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceAmountBoundaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceAmountBoundaryPanel.Name = "InvoiceAmountBoundaryPanel";
			this.InvoiceAmountBoundaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			this.InvoiceAmountBoundaryPanel.TabIndex = 0;
			// 
			// InvoiceAmountBoundaryGridPanel
			// 
			this.InvoiceAmountBoundaryGridPanel.Controls.Add(this.AmountsGrid);
			this.InvoiceAmountBoundaryGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceAmountBoundaryGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.InvoiceAmountBoundaryGridPanel.Name = "InvoiceAmountBoundaryGridPanel";
			this.InvoiceAmountBoundaryGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 359, true);
			this.InvoiceAmountBoundaryGridPanel.TabIndex = 2;
			// 
			// AmountsGrid
			// 
			this.AmountsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AmountsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.InvoiceAmountBoundary)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.InvoiceAmountBoundary)(null)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.InvoiceAmountBoundary)(null)).EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.InvoiceAmountBoundary)(null)).Amount)));
			this.AmountsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AmountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AmountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AmountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AmountsGrid.GridId = "542865f5-f899-4398-b133-b009a4fed249";
			this.AmountsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AmountsGrid.LayoutKey = "AmountsGrid";
			this.AmountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmountsGrid.Name = "AmountsGrid";
			this.AmountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 359, true);
			this.AmountsGrid.TabIndex = 0;
			// 
			// InvoiceAmountBoundaryLabelPanel
			// 
			this.InvoiceAmountBoundaryLabelPanel.Controls.Add(this.InvoiceAmountBoundaryLabel);
			this.InvoiceAmountBoundaryLabelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceAmountBoundaryLabelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceAmountBoundaryLabelPanel.Name = "InvoiceAmountBoundaryLabelPanel";
			this.InvoiceAmountBoundaryLabelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 30, true);
			this.InvoiceAmountBoundaryLabelPanel.TabIndex = 1;
			// 
			// InvoiceAmountBoundaryLabel
			// 
			this.InvoiceAmountBoundaryLabel.AutoSize = true;
			this.InvoiceAmountBoundaryLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceAmountBoundariesControl|014a2a1e-f884-42c6-a362-fb1f81d0c755", "Amount (NIS)");
			this.InvoiceAmountBoundaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 10, true);
			this.InvoiceAmountBoundaryLabel.Name = "InvoiceAmountBoundaryLabel";
			this.InvoiceAmountBoundaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.InvoiceAmountBoundaryLabel.TabIndex = 1;
			// 
			// InvoiceAmountBoundariesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceAmountBoundaryPanel);
			this.Name = "InvoiceAmountBoundariesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceAmountBoundaryPanel.ResumeLayout(false);
			this.InvoiceAmountBoundaryGridPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AmountsGrid)).EndInit();
			this.InvoiceAmountBoundaryLabelPanel.ResumeLayout(false);
			this.InvoiceAmountBoundaryLabelPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel InvoiceAmountBoundaryPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel InvoiceAmountBoundaryGridPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel InvoiceAmountBoundaryLabelPanel;
		private Enterprise.ZArchitecture.ZLabel InvoiceAmountBoundaryLabel;
		internal Enterprise.ZArchitecture.ZGrid AmountsGrid;
	}
}
