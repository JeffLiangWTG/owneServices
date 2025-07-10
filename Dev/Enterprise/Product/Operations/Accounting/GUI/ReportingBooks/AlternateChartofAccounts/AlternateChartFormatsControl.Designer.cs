using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	partial class AlternateChartFormatsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		Enterprise.ZArchitecture.ZGrid AlternateChartFormatGrid;

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
			this.CaptionRenderingEnabled = true;
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			this.AlternateChartFormatGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartFormatGrid)).BeginInit();
			this.AlternateChartFormatGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccAlternateChartFormat);
			// 
			// AlternateChartFormatGrid
			// 
			this.AlternateChartFormatGrid.AllowNavigation = false;
			this.AlternateChartFormatGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AlternateChartFormatGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(null)).ANF_Tier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(null)).ANF_Format)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(null)).ANF_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(null)).ANF_Separator)));
			this.AlternateChartFormatGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FAE26916-2A7F-439A-A64C-B27EC4D171D9", "Tier");
			zCalcEditColumnStyleInfo1.ColumnName = "ANF_Tier";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("D377FA6C-60EE-43D4-AD58-64F83D8079F8", "Format");
			zTextBoxColumnStyleInfo1.ColumnName = "ANF_Format";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FABE1FF4-840C-4660-83CA-833723E0EBF4", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ANF_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(423);
			zDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B7256B43-CE5A-4E82-9A8C-B1392EB69A0E", "Separator");
			zDropEditColumnStyleInfo.ColumnName = "ANF_Separator";
			zDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			this.AlternateChartFormatGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AlternateChartFormatGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AlternateChartFormatGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AlternateChartFormatGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			this.AlternateChartFormatGrid.GridId = "58447F7C-5712-4419-B2AD-627004341B34";
			this.AlternateChartFormatGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternateChartFormatGrid.LayoutKey = "AlternateChartFormatGrid";
			this.AlternateChartFormatGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.AlternateChartFormatGrid.Name = "AlternateChartFormatGrid";
			this.AlternateChartFormatGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 198, true);
			this.AlternateChartFormatGrid.TabIndex = 50;
			// 
			// AlternateChartFormatsControl
			// 
			this.Controls.Add(this.AlternateChartFormatGrid);
			this.Name = "AlternateChartFormatsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 202, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartFormatGrid)).EndInit();
			this.AlternateChartFormatGrid.ResumeLayout(false);
			this.AlternateChartFormatGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZGrid GetAlternateChartFormatGrid_ForTest()
		{
			return this.AlternateChartFormatGrid;
		}
	}
}
