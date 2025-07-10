namespace Enterprise.Customs.DE.GUI
{
	partial class AdditionalInfoUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.AdditionalTariffsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContentInfoTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalTariffsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ContentInfoTypeSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTariffsGrid)).BeginInit();
			this.AdditionalTariffsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContentInfoTypeGrid)).BeginInit();
			this.ContentInfoTypeGrid.SuspendLayout();
			this.AdditionalTariffsSeparatorUserControl.SuspendLayout();
			this.ContentInfoTypeSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// AdditionalTariffsGrid
			// 
			this.AdditionalTariffsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalTariffsGrid, "FilteredInvoiceLines.CusLineTariffDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_Qty1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_UQ1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_PercentAlcohol)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).BZ_TobaccoRetailPrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusLineTariffDetails)).SyncRoot)).ExciseValue)));
			this.AdditionalTariffsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BZ_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BZ_Tariff";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BZ_Qty1";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "BZ_UQ1";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(46);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BZ_PercentAlcohol";
			zCalcEditColumnStyleInfo2.MaxValue = 100;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BZ_TobaccoRetailPrice";
			zCalcEditColumnStyleInfo3.Decimals = 6;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ExciseValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AdditionalTariffsGrid.GridId = "CDDD7C23-CF3A-44A2-ABF4-78D2F30EBB8E";
			this.AdditionalTariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalTariffsGrid.LayoutKey = "AdditionalTariffsGrid";
			this.AdditionalTariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 0, true);
			this.AdditionalTariffsGrid.Name = "AdditionalTariffsGrid";
			this.AdditionalTariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 100, true);
			this.AdditionalTariffsGrid.TabIndex = 0;
			// 
			// ContentInfoTypeGrid
			// 
			this.ContentInfoTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContentInfoTypeGrid, "FilteredInvoiceLines.ContentInformationTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ContentInformationTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ContentInformationType)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ContentInformationTypes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.ContentInformationType)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ContentInformationTypes)).SyncRoot)).DegreePercentage)));
			this.ContentInfoTypeGrid.CaptionText = "Content Information";
			this.ContentInfoTypeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "DegreePercentage";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ContentInfoTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ContentInfoTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ContentInfoTypeGrid.GridId = "E8CED8E1-9EFA-40AC-A0A4-105D3D10C423";
			this.ContentInfoTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContentInfoTypeGrid.LayoutKey = "ContentInfoTypeGrid";
			this.ContentInfoTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContentInfoTypeGrid.MaximumRows = 3;
			this.ContentInfoTypeGrid.Name = "ContentInfoTypeGrid";
			this.ContentInfoTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 100, true);
			this.ContentInfoTypeGrid.TabIndex = 0;
			// 
			// AdditionalTariffsSeparatorUserControl
			// 
			this.AdditionalTariffsSeparatorUserControl.AllowDrop = true;
			this.AdditionalTariffsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("e9a14040-27f7-44ca-b8d6-106e495aa547", "Additional Tariffs");
			this.AdditionalTariffsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 120, true);
			this.AdditionalTariffsSeparatorUserControl.Name = "AdditionalTariffsSeparatorUserControl";
			this.AdditionalTariffsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 15, true);
			this.AdditionalTariffsSeparatorUserControl.TabIndex = 1;
			// 
			// ContentInfoTypeSeparatorUserControl
			// 
			this.ContentInfoTypeSeparatorUserControl.AllowDrop = true;
			this.ContentInfoTypeSeparatorUserControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("bb9c08de-3301-488b-b2b6-905a7cbf76ee", "Content Information");
			this.ContentInfoTypeSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.ContentInfoTypeSeparatorUserControl.Name = "ContentInfoTypeSeparatorUserControl";
			this.ContentInfoTypeSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ContentInfoTypeSeparatorUserControl.TabIndex = 2;
			// 
			// AdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalTariffsGrid);
			this.Controls.Add(this.ContentInfoTypeGrid);
			this.Controls.Add(this.AdditionalTariffsSeparatorUserControl);
			this.Controls.Add(this.ContentInfoTypeSeparatorUserControl);
			this.Name = "AdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTariffsGrid)).EndInit();
			this.AdditionalTariffsGrid.ResumeLayout(false);
			this.AdditionalTariffsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContentInfoTypeGrid)).EndInit();
			this.ContentInfoTypeGrid.ResumeLayout(false);
			this.ContentInfoTypeGrid.PerformLayout();
			this.AdditionalTariffsSeparatorUserControl.ResumeLayout(true);
			this.AdditionalTariffsSeparatorUserControl.PerformLayout();
			this.ContentInfoTypeSeparatorUserControl.ResumeLayout(true);
			this.ContentInfoTypeSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid AdditionalTariffsGrid;
		internal ZArchitecture.ZGrid ContentInfoTypeGrid;
		internal ZArchitecture.GUI.SeparatorUserControl AdditionalTariffsSeparatorUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl ContentInfoTypeSeparatorUserControl;
	}
}
