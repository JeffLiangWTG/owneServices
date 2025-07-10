namespace Enterprise.Customs.EU.GUI
{
	partial class ItineraryCountriesUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ItineraryCountriesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryCountriesGrid)).BeginInit();
			this.ItineraryCountriesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// ItineraryCountriesGrid
			// 
			this.ItineraryCountriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItineraryCountriesGrid, "ItineraryCountries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ItineraryCountries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.ItineraryCountry)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ItineraryCountries)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.ItineraryCountry)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ItineraryCountries)).SyncRoot)).CY_Code)));
			this.ItineraryCountriesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			this.ItineraryCountriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItineraryCountriesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ItineraryCountriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItineraryCountriesGrid.GridId = "abc63ce7-097b-441c-8ad3-b32759c66e34";
			this.ItineraryCountriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItineraryCountriesGrid.LayoutKey = "ItineraryCountriesGrid";
			this.ItineraryCountriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItineraryCountriesGrid.Name = "ItineraryCountriesGrid";
			this.ItineraryCountriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 177, true);
			this.ItineraryCountriesGrid.TabIndex = 0;
			// 
			// ItineraryCountriesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItineraryCountriesGrid);
			this.Name = "ItineraryCountriesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 177, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryCountriesGrid)).EndInit();
			this.ItineraryCountriesGrid.ResumeLayout(false);
			this.ItineraryCountriesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ItineraryCountriesGrid;
	}
}
