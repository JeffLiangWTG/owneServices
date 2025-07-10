namespace Enterprise.Customs.BR.GUI
{
	partial class SpecialCasesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SpecialCasesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SpecialCasesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LegalActYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LegalActNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LegalActIssuingBodyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LegalActTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfMeasureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RateOrUnitValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCasesGrid)).BeginInit();
			this.SpecialCasesGrid.SuspendLayout();
			this.SpecialCasesGroupBox.SuspendLayout();
			this.LegalActIssuingBodyDropEdit.SuspendLayout();
			this.LegalActTypeDropEdit.SuspendLayout();
			this.UnitOfMeasureDropEdit.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.TaxTypeDropEdit.SuspendLayout();
			this.TaxGroupDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// SpecialCasesGrid
			// 
			this.SpecialCasesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SpecialCasesGrid, "SpecialCaseTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).TaxGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).TaxGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).TaxType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).RateOrUnitValue)));
			this.SpecialCasesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TaxGroup";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TaxGroupDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			zDropEditColumnStyleInfo2.ColumnName = "TaxType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RateOrUnitValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.SpecialCasesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SpecialCasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SpecialCasesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SpecialCasesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SpecialCasesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialCasesGrid.GridId = "48614c2e-9b1b-4b30-af18-8a37a67bc078";
			this.SpecialCasesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SpecialCasesGrid.LayoutKey = "SpecialCasesGrid";
			this.SpecialCasesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SpecialCasesGrid.Name = "SpecialCasesGrid";
			this.SpecialCasesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 182, true);
			this.SpecialCasesGrid.TabIndex = 0;
			// 
			// SpecialCasesGroupBox
			// 
			this.SpecialCasesGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("B42A2C25-58FC-4A42-86C2-436138AA372A", "Special Cases");
			this.SpecialCasesGroupBox.Controls.Add(this.LegalActYearTextBox);
			this.SpecialCasesGroupBox.Controls.Add(this.LegalActNumberTextBox);
			this.SpecialCasesGroupBox.Controls.Add(this.LegalActIssuingBodyDropEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.LegalActTypeDropEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.QuantityCalcEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.UnitOfMeasureDropEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.CurrencyCodeFindBox);
			this.SpecialCasesGroupBox.Controls.Add(this.RateOrUnitValueCalcEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.TaxTypeDropEdit);
			this.SpecialCasesGroupBox.Controls.Add(this.TaxGroupDropEdit);
			this.SpecialCasesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SpecialCasesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.SpecialCasesGroupBox.Name = "SpecialCasesGroupBox";
			this.SpecialCasesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 211, true);
			this.SpecialCasesGroupBox.TabIndex = 1;
			this.SpecialCasesGroupBox.TabStop = false;
			// 
			// LegalActYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.LegalActYearTextBox, "SpecialCaseTaxes.LegalActYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).LegalActYear)));
			this.LegalActYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 175, true);
			this.LegalActYearTextBox.Name = "LegalActYearTextBox";
			this.LegalActYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.LegalActYearTextBox.TabIndex = 9;
			// 
			// LegalActNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LegalActNumberTextBox, "SpecialCaseTaxes.LegalActNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).LegalActNumber)));
			this.LegalActNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 149, true);
			this.LegalActNumberTextBox.Name = "LegalActNumberTextBox";
			this.LegalActNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.LegalActNumberTextBox.TabIndex = 8;
			// 
			// LegalActIssuingBodyDropEdit
			// 
			this.LegalActIssuingBodyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LegalActIssuingBodyDropEdit, "SpecialCaseTaxes.LegalActIssuingBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).LegalActIssuingBody)));
			this.LegalActIssuingBodyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 123, true);
			this.LegalActIssuingBodyDropEdit.Name = "LegalActIssuingBodyDropEdit";
			this.LegalActIssuingBodyDropEdit.ShowDescriptionBox = false;
			this.LegalActIssuingBodyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.LegalActIssuingBodyDropEdit.TabIndex = 7;
			// 
			// LegalActTypeDropEdit
			// 
			this.LegalActTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LegalActTypeDropEdit, "SpecialCaseTaxes.LegalActType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).LegalActType)));
			this.LegalActTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 97, true);
			this.LegalActTypeDropEdit.Name = "LegalActTypeDropEdit";
			this.LegalActTypeDropEdit.ShowDescriptionBox = false;
			this.LegalActTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.LegalActTypeDropEdit.TabIndex = 6;
			// 
			// QuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "SpecialCaseTaxes.Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).Quantity)));
			this.QuantityCalcEdit.DecimalPlaces = 0;
			this.QuantityCalcEdit.Decimals = 0;
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(715, 71, true);
			this.QuantityCalcEdit.MaxValue = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
			this.QuantityCalcEdit.Name = "QuantityCalcEdit";
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.QuantityCalcEdit.TabIndex = 5;
			this.QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfMeasureDropEdit
			// 
			this.UnitOfMeasureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitOfMeasureDropEdit, "SpecialCaseTaxes.UnitOfMeasure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).UnitOfMeasure)));
			this.UnitOfMeasureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 71, true);
			this.UnitOfMeasureDropEdit.Name = "UnitOfMeasureDropEdit";
			this.UnitOfMeasureDropEdit.PreBoundMaxLength = 3;
			this.UnitOfMeasureDropEdit.ShowDescriptionBox = false;
			this.UnitOfMeasureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 38, true);
			this.UnitOfMeasureDropEdit.TabIndex = 4;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "SpecialCaseTaxes.CurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).CurrencyCode)));
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 71, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.CurrencyCodeFindBox.ParentType = null;
			this.CurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.CurrencyCodeFindBox.ShowDescriptionBox = false;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 3;
			// 
			// RateOrUnitValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RateOrUnitValueCalcEdit, "SpecialCaseTaxes.RateOrUnitValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).RateOrUnitValue)));
			this.RateOrUnitValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 71, true);
			this.RateOrUnitValueCalcEdit.Name = "RateOrUnitValueCalcEdit";
			this.RateOrUnitValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.RateOrUnitValueCalcEdit.TabIndex = 2;
			this.RateOrUnitValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RateOrUnitValueCalcEdit.TrackDisposedAccess = true;
			// 
			// TaxTypeDropEdit
			// 
			this.TaxTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxTypeDropEdit, "SpecialCaseTaxes.TaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).TaxType)));
			this.TaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 45, true);
			this.TaxTypeDropEdit.Name = "TaxTypeDropEdit";
			this.TaxTypeDropEdit.ShowDescriptionBox = false;
			this.TaxTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TaxTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TaxTypeDropEdit.TabIndex = 1;
			// 
			// TaxGroupDropEdit
			// 
			this.TaxGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxGroupDropEdit, "SpecialCaseTaxes.TaxGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.SpecialCaseTax)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).SpecialCaseTaxes)).SyncRoot)).TaxGroup)));
			this.TaxGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 19, true);
			this.TaxGroupDropEdit.Name = "TaxGroupDropEdit";
			this.TaxGroupDropEdit.ShowDescriptionBox = false;
			this.TaxGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TaxGroupDropEdit.TabIndex = 0;
			// 
			// SpecialCasesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SpecialCasesGrid);
			this.Controls.Add(this.SpecialCasesGroupBox);
			this.Name = "SpecialCasesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 393, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCasesGrid)).EndInit();
			this.SpecialCasesGrid.ResumeLayout(false);
			this.SpecialCasesGrid.PerformLayout();
			this.SpecialCasesGroupBox.ResumeLayout(false);
			this.SpecialCasesGroupBox.PerformLayout();
			this.LegalActIssuingBodyDropEdit.ResumeLayout(true);
			this.LegalActIssuingBodyDropEdit.PerformLayout();
			this.LegalActTypeDropEdit.ResumeLayout(true);
			this.LegalActTypeDropEdit.PerformLayout();
			this.UnitOfMeasureDropEdit.ResumeLayout(true);
			this.UnitOfMeasureDropEdit.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.TaxTypeDropEdit.ResumeLayout(true);
			this.TaxTypeDropEdit.PerformLayout();
			this.TaxGroupDropEdit.ResumeLayout(true);
			this.TaxGroupDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox SpecialCasesGroupBox;
		internal ZArchitecture.ZGrid SpecialCasesGrid;
		internal ZArchitecture.GUI.ZDropEdit TaxGroupDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TaxTypeDropEdit;
		internal ZArchitecture.ZCalcEdit RateOrUnitValueCalcEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CurrencyCodeFindBox;
		internal ZArchitecture.ZCalcEdit QuantityCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit UnitOfMeasureDropEdit;
		internal ZArchitecture.GUI.ZDropEdit LegalActTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit LegalActIssuingBodyDropEdit;
		internal ZArchitecture.ZTextBox LegalActNumberTextBox;
		internal ZArchitecture.ZTextBox LegalActYearTextBox;
	}
}
