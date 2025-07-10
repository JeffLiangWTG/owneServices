namespace Enterprise.CommissionManagement.GUI
{
	partial class RecentCommissionsMatrixControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.RecentCommissionsMatrixGrid = new Enterprise.CommissionManagement.GUI.RecentCommissionsMatrixGrid();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RecentCommissionsMatrixGrid)).BeginInit();
			this.RecentCommissionsMatrixGrid.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.RecentCommissionsMatrix);
			// 
			// RecentCommissionsMatrixGrid
			// 
			this.RecentCommissionsMatrixGrid.AllowNavigation = false;
			this.RecentCommissionsMatrixGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.RecentCommissionsMatrixGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).Total)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).TotalCurrentMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).TotalPreviousMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).Total2MonthsAgo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).Total3MonthsAgo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.CommissionManagement.Business.RecentCommissionsRow)(null)).TotalOver3MonthsAgo)));
			this.RecentCommissionsMatrixGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Total";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TotalCurrentMonth";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TotalPreviousMonth";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Total2MonthsAgo";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "Total3MonthsAgo";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "TotalOver3MonthsAgo";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.RecentCommissionsMatrixGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.RecentCommissionsMatrixGrid.CopySelectedRowsAllowed = true;
			this.RecentCommissionsMatrixGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecentCommissionsMatrixGrid.GridId = "f9e435f8-7317-42cc-b697-11104722b68e";
			this.RecentCommissionsMatrixGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecentCommissionsMatrixGrid.LayoutKey = "recentCommissionsMatrixGrid";
			this.RecentCommissionsMatrixGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.RecentCommissionsMatrixGrid.Name = "RecentCommissionsMatrixGrid";
			this.RecentCommissionsMatrixGrid.ReadOnly = true;
			this.RecentCommissionsMatrixGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RecentCommissionsMatrixGrid.RowHeaderWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.RecentCommissionsMatrixGrid.ShouldSetErrorsOnTabPage = false;
			this.RecentCommissionsMatrixGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 135, true);
			this.RecentCommissionsMatrixGrid.TabIndex = 0;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("1ea11e94-0ab8-464e-aa19-b1cc4b05c616", "Filtered Results Summary");
			this.MainGroupBox.Controls.Add(this.RecentCommissionsMatrixGrid);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 150, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// RecentCommissionsMatrixControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "RecentCommissionsMatrixControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RecentCommissionsMatrixGrid)).EndInit();
			this.RecentCommissionsMatrixGrid.ResumeLayout(false);
			this.RecentCommissionsMatrixGrid.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected RecentCommissionsMatrixGrid RecentCommissionsMatrixGrid;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
	}
}
