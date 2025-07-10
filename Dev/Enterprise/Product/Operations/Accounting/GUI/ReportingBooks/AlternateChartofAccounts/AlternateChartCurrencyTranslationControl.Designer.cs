namespace Enterprise.Accounting.GUI
{
	partial class AlternateChartCurrencyTranslationControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		Enterprise.ZArchitecture.ZGrid AlternateChartCurrencyTranslationGrid;

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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo accountTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			AlternateGLAccountFindBoxColumnStyleInfo alternateGLAccountFindBoxColumnStyleInfo = new AlternateGLAccountFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo currencyTranslationLevelDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo exRateTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AlternateChartCurrencyTranslationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartCurrencyTranslationGrid)).BeginInit();
			this.AlternateChartCurrencyTranslationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation);
			// 
			// AlternateChartCurrencyTranslationGrid
			// 
			this.AlternateChartCurrencyTranslationGrid.AllowNavigation = false;
			this.AlternateChartCurrencyTranslationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AlternateChartCurrencyTranslationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(null)).ART_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(null)).ART_AGA_AlternateAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(null)).ART_CurrencyTranslationLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(null)).ART_ExRateType)));
			this.AlternateChartCurrencyTranslationGrid.CaptionVisible = false;
			accountTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("81a564c3-dd43-4031-ba7a-eeb034fb0fa2", "Account Type");
			accountTypeDropEditColumnStyleInfo.ColumnName = "ART_Type";
			accountTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			accountTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			alternateGLAccountFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aece9864-64e9-4113-8c22-f028eba5e2c3", "Alternate Account");
			alternateGLAccountFindBoxColumnStyleInfo.ColumnName = "ART_AGA_AlternateAccount";
			alternateGLAccountFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			alternateGLAccountFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			currencyTranslationLevelDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("63f61322-c89e-48b2-9313-5270209aee8b", "Level");
			currencyTranslationLevelDropEditColumnStyleInfo.ColumnName = "ART_CurrencyTranslationLevel";
			currencyTranslationLevelDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			currencyTranslationLevelDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			exRateTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9d2b7364-8bbd-4b79-b9d7-bf1901656d68", "Rate Type");
			exRateTypeDropEditColumnStyleInfo.ColumnName = "ART_ExRateType";
			exRateTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			exRateTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AlternateChartCurrencyTranslationGrid.ColumnStyles.Add(accountTypeDropEditColumnStyleInfo);
			this.AlternateChartCurrencyTranslationGrid.ColumnStyles.Add(alternateGLAccountFindBoxColumnStyleInfo);
			this.AlternateChartCurrencyTranslationGrid.ColumnStyles.Add(currencyTranslationLevelDropEditColumnStyleInfo);
			this.AlternateChartCurrencyTranslationGrid.ColumnStyles.Add(exRateTypeDropEditColumnStyleInfo);
			this.AlternateChartCurrencyTranslationGrid.GridId = "A04E773A-0E60-4C3B-8C18-8F74029C81F8";
			this.AlternateChartCurrencyTranslationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternateChartCurrencyTranslationGrid.LayoutKey = "AlternateChartFormatGrid";
			this.AlternateChartCurrencyTranslationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.AlternateChartCurrencyTranslationGrid.Name = "AlternateChartCurrencyTranslationGrid";
			this.AlternateChartCurrencyTranslationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 198, true);
			this.AlternateChartCurrencyTranslationGrid.TabIndex = 50;
			// 
			// AccAlternateChartCurrencyTranslation
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AlternateChartCurrencyTranslationGrid);
			this.Name = "AccAlternateChartCurrencyTranslation";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 202, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlternateChartCurrencyTranslationGrid)).EndInit();
			this.AlternateChartCurrencyTranslationGrid.ResumeLayout(false);
			this.AlternateChartCurrencyTranslationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
