using System.ComponentModel;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class ElectronicProcessingChargeCurrencyControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components = null;

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

		internal Enterprise.ZArchitecture.ZGrid ElectronicProcessingChargeCurrencyGrid;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ElectronicProcessingChargeCurrencyGrid.ReadOnly = readOnly;
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			//Add 2 columns to the grid
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo currencyGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo validFromDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();

			this.ElectronicProcessingChargeCurrencyGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeCurrencyGrid)).BeginInit();
			this.ElectronicProcessingChargeCurrencyGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeCurrencyCollection);
			// 
			// ElectronicProcessingChargeCurrencyGrid
			//
			this.ElectronicProcessingChargeCurrencyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ElectronicProcessingChargeCurrencyGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeCurrency)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeCurrency)(null)).CurrencyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeCurrency)(null)).ValidFromDate)));

			currencyGuidFindBoxColumnStyleInfo.ColumnName = "CurrencyPK";
			currencyGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("013bd92b-f144-41ec-bfae-218ffb10c788", "Currency");
			currencyGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			validFromDateEditColumnStyleInfo.ColumnName = "ValidFromDate";
			validFromDateEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6be1e5c0-4335-4cbc-b47e-70d41cf614aa", "Valid From Date ");
			validFromDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			validFromDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.ElectronicProcessingChargeCurrencyGrid.ColumnStyles.Add(currencyGuidFindBoxColumnStyleInfo);
			this.ElectronicProcessingChargeCurrencyGrid.ColumnStyles.Add(validFromDateEditColumnStyleInfo);
			this.ElectronicProcessingChargeCurrencyGrid.CaptionVisible = false;
			this.ElectronicProcessingChargeCurrencyGrid.GridId = "d674fe2f-f307-41e6-8076-0c2ff30311bf";
			this.ElectronicProcessingChargeCurrencyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ElectronicProcessingChargeCurrencyGrid.LayoutKey = "ElectronicProcessingChargeCurrencyGrid";
			this.ElectronicProcessingChargeCurrencyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ElectronicProcessingChargeCurrencyGrid.Name = "ElectronicProcessingChargeCurrencyGrid";
			this.ElectronicProcessingChargeCurrencyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 372, true);
			this.ElectronicProcessingChargeCurrencyGrid.TabIndex = 0;
			this.ElectronicProcessingChargeCurrencyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ElectronicProcessingChargeDescriptionOverrideControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ElectronicProcessingChargeCurrencyGrid);
			this.Name = "ElectronicProcessingChargeCurrencyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeCurrencyGrid)).EndInit();
			this.ElectronicProcessingChargeCurrencyGrid.ResumeLayout(false);
			this.ElectronicProcessingChargeCurrencyGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

