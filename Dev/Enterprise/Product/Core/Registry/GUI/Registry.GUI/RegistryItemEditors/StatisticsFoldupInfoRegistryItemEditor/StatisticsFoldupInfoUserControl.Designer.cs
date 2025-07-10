namespace Enterprise.Registry.GUI
{
	partial class StatisticsFoldupInfoUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.StatisticsInfoFoldupGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StatisticsInfoFoldupGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.StatisticsFoldupInfoCollection);
			// 
			// StatisticsInfoFoldupGrid
			// 
			this.StatisticsInfoFoldupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StatisticsInfoFoldupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.StatisticsFoldupInfo)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.StatisticsFoldupInfo)(null)).WaitAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StatisticsFoldupInfo)(null)).WaitScale)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.StatisticsFoldupInfo)(null)).AggregateAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StatisticsFoldupInfo)(null)).AggregateScale)));
			this.StatisticsInfoFoldupGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WaitAmount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo1.ColumnName = "WaitScale";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AggregateAmount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.ColumnName = "AggregateScale";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.StatisticsInfoFoldupGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.StatisticsInfoFoldupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.StatisticsInfoFoldupGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.StatisticsInfoFoldupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.StatisticsInfoFoldupGrid.CopySelectedRowsAllowed = true;
			this.StatisticsInfoFoldupGrid.GridId = "58f825ef-6d58-40fb-9f8d-4dc81b657aa5";
			this.StatisticsInfoFoldupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatisticsInfoFoldupGrid.LayoutKey = "zGrid1";
			this.StatisticsInfoFoldupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatisticsInfoFoldupGrid.Name = "StatisticsInfoFoldupGrid";
			this.StatisticsInfoFoldupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 223, true);
			this.StatisticsInfoFoldupGrid.TabIndex = 0;
			this.StatisticsInfoFoldupGrid.Navigate += new System.Windows.Forms.NavigateEventHandler(this.StatisticsInfoFoldupGrid_Navigate);
			// 
			// StatisticsFoldupInfoUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.StatisticsInfoFoldupGrid);
			this.Name = "StatisticsFoldupInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StatisticsInfoFoldupGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid StatisticsInfoFoldupGrid;
	}
}
