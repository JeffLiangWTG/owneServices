namespace Enterprise.Registry.GUI
{
	partial class DefaultNumberOfDecimalsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DefaultNumberOfDecimalsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultNumberOfDecimalsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DefaultNumberOfDecimalsCollection);
			// 
			// DefaultNumberOfDecimalsGrid
			// 
			this.DefaultNumberOfDecimalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultNumberOfDecimalsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).UnitOfMeasureList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).TransportModeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).NumberOfDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).RoundingMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DefaultNumberOfDecimals)(null)).RoundingModeList)));
			this.DefaultNumberOfDecimalsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "UnitOfMeasureList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0a495c17-a317-40fb-82db-11aabc5d8fa1", "Unit Of Measure");
			zDropEditColumnStyleInfo1.ColumnName = "UnitOfMeasure";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "TransportModeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7c6453a2-d1a4-4a3f-a795-fb6736a429a8", "Transport Mode");
			zDropEditColumnStyleInfo2.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("82cbde66-0b59-4cd6-874e-075f017ee3d3", "Decimal Places");
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfDecimals";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo3.BindToList = "RoundingModeList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d2af51eb-6289-485f-b6bd-b731543eb7c2", "Rounding Mode");
			zDropEditColumnStyleInfo3.ColumnName = "RoundingMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefaultNumberOfDecimalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultNumberOfDecimalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DefaultNumberOfDecimalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DefaultNumberOfDecimalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.DefaultNumberOfDecimalsGrid.CopySelectedRowsAllowed = true;
			this.DefaultNumberOfDecimalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultNumberOfDecimalsGrid.GridId = "6a458611-1a09-489e-9adc-6284487b07eb";
			this.DefaultNumberOfDecimalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultNumberOfDecimalsGrid.LayoutKey = "DefaultNumberOfDecimalsGrid";
			this.DefaultNumberOfDecimalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultNumberOfDecimalsGrid.Name = "DefaultNumberOfDecimalsGrid";
			this.DefaultNumberOfDecimalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			this.DefaultNumberOfDecimalsGrid.TabIndex = 0;
			// 
			// DefaultNumberOfDecimalsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultNumberOfDecimalsGrid);
			this.Name = "DefaultNumberOfDecimalsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultNumberOfDecimalsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid DefaultNumberOfDecimalsGrid;
	}
}
