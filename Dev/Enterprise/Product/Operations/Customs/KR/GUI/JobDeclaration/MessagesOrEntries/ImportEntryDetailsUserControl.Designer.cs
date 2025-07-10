using CargoWise.Windows.UI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ImportEntryDetailsUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntrySubmittedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClearedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IncotermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalCustomsValueKRWCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCustomsValueUSDCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FreightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AdditionalAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductedAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalValueForVATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalVATExemptionValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalSpecialConsumptionTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalTransportationTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLiquorTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalEducationTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAgricultureTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalVATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPayableAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForLateDeclarationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForMissedDeclarationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalGrossWeightInKGCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomerOfficerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsRemarkLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.TotalPackagesDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalInvoiceAmountUserControl = new Enterprise.Customs.KR.GUI.TotalInvoiceAmountUserControl();
			this.CustomsDisbursementBillGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsDisbursementBillGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			this.EntrySubmittedDateEdit.SuspendLayout();
			this.AcceptedDateEdit.SuspendLayout();
			this.ClearedDateEdit.SuspendLayout();
			this.CustomsRemarkLongTextBox.SuspendLayout();
			this.TotalPackagesDropEdit.SuspendLayout();
			this.TotalInvoiceAmountUserControl.SuspendLayout();
			this.CustomsDisbursementBillGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsDisbursementBillGrid)).BeginInit();
			this.CustomsDisbursementBillGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "CustomsEntryHeaders.FormattedEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedEntryNumber)));
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 3, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.EntryNumberTextBox.TabIndex = 1;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "CustomsEntryHeaders.CH_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 29, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.MessageTypeDropEdit.TabIndex = 2;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "CustomsEntryHeaders.CH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 55, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.MessageStatusDropEdit.TabIndex = 3;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 81, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.EntryStatusDropEdit.TabIndex = 4;
			// 
			// EntrySubmittedDateEdit
			// 
			this.EntrySubmittedDateEdit.AllowDrop = true;
			this.EntrySubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EntrySubmittedDateEdit, "CustomsEntryHeaders.CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			this.EntrySubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 107, true);
			this.EntrySubmittedDateEdit.Name = "EntrySubmittedDateEdit";
			this.EntrySubmittedDateEdit.TabIndex = 5;
			// 
			// AcceptedDateEdit
			// 
			this.AcceptedDateEdit.AllowDrop = true;
			this.AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDateEdit, "CustomsEntryHeaders.AcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AcceptedDate)));
			this.AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 133, true);
			this.AcceptedDateEdit.Name = "AcceptedDateEdit";
			this.AcceptedDateEdit.TabIndex = 6;
			// 
			// ClearedDateEdit
			// 
			this.ClearedDateEdit.AllowDrop = true;
			this.ClearedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ClearedDateEdit, "CustomsEntryHeaders.CH_EntryReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			this.ClearedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 159, true);
			this.ClearedDateEdit.Name = "ClearedDateEdit";
			this.ClearedDateEdit.TabIndex = 7;
			// 
			// IncotermTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncotermTextBox, "CustomsEntryHeaders.Incoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Incoterm)));
			this.IncotermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 185, true);
			this.IncotermTextBox.Name = "IncotermTextBox";
			this.IncotermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.IncotermTextBox.TabIndex = 8;
			// 
			// TotalCustomsValueKRWCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCustomsValueKRWCalcEdit, "CustomsEntryHeaders.CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsValue)));
			this.TotalCustomsValueKRWCalcEdit.DecimalPlaces = 0;
			this.TotalCustomsValueKRWCalcEdit.Decimals = 0;
			this.TotalCustomsValueKRWCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 237, true);
			this.TotalCustomsValueKRWCalcEdit.Name = "TotalCustomsValueKRWCalcEdit";
			this.TotalCustomsValueKRWCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalCustomsValueKRWCalcEdit.TabIndex = 10;
			this.TotalCustomsValueKRWCalcEdit.Text = "0";
			this.TotalCustomsValueKRWCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCustomsValueKRWCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalCustomsValueUSDCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCustomsValueUSDCalcEdit, "CustomsEntryHeaders.CustomsValueUSD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsValueUSD)));
			this.TotalCustomsValueUSDCalcEdit.DecimalPlaces = 0;
			this.TotalCustomsValueUSDCalcEdit.Decimals = 0;
			this.TotalCustomsValueUSDCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 263, true);
			this.TotalCustomsValueUSDCalcEdit.Name = "TotalCustomsValueUSDCalcEdit";
			this.TotalCustomsValueUSDCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalCustomsValueUSDCalcEdit.TabIndex = 11;
			this.TotalCustomsValueUSDCalcEdit.Text = "0";
			this.TotalCustomsValueUSDCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCustomsValueUSDCalcEdit.TrackDisposedAccess = true;
			// 
			// FreightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FreightCalcEdit, "CustomsEntryHeaders.Freight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Freight)));
			this.FreightCalcEdit.DecimalPlaces = 0;
			this.FreightCalcEdit.Decimals = 0;
			this.FreightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 289, true);
			this.FreightCalcEdit.Name = "FreightCalcEdit";
			this.FreightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.FreightCalcEdit.TabIndex = 12;
			this.FreightCalcEdit.Text = "0";
			this.FreightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FreightCalcEdit.TrackDisposedAccess = true;
			// 
			// InsuranceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InsuranceCalcEdit, "CustomsEntryHeaders.Insurance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Insurance)));
			this.InsuranceCalcEdit.DecimalPlaces = 0;
			this.InsuranceCalcEdit.Decimals = 0;
			this.InsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 315, true);
			this.InsuranceCalcEdit.Name = "InsuranceCalcEdit";
			this.InsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.InsuranceCalcEdit.TabIndex = 13;
			this.InsuranceCalcEdit.Text = "0";
			this.InsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InsuranceCalcEdit.TrackDisposedAccess = true;
			// 
			// AdditionalAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AdditionalAmountCalcEdit, "Invoices.ImportTotalAdditionalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ImportTotalAdditionalAmount)));
			this.AdditionalAmountCalcEdit.DecimalPlaces = 0;
			this.AdditionalAmountCalcEdit.Decimals = 0;
			this.AdditionalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 3, true);
			this.AdditionalAmountCalcEdit.Name = "AdditionalAmountCalcEdit";
			this.AdditionalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.AdditionalAmountCalcEdit.TabIndex = 14;
			this.AdditionalAmountCalcEdit.Text = "0";
			this.AdditionalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AdditionalAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductedAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductedAmountCalcEdit, "Invoices.ImportTotalDeductedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ImportTotalDeductedAmount)));
			this.DeductedAmountCalcEdit.DecimalPlaces = 0;
			this.DeductedAmountCalcEdit.Decimals = 0;
			this.DeductedAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 29, true);
			this.DeductedAmountCalcEdit.Name = "DeductedAmountCalcEdit";
			this.DeductedAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.DeductedAmountCalcEdit.TabIndex = 15;
			this.DeductedAmountCalcEdit.Text = "0";
			this.DeductedAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductedAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalValueForVATCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalValueForVATCalcEdit, "CustomsEntryHeaders.ValueForVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ValueForVAT)));
			this.TotalValueForVATCalcEdit.DecimalPlaces = 0;
			this.TotalValueForVATCalcEdit.Decimals = 0;
			this.TotalValueForVATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 55, true);
			this.TotalValueForVATCalcEdit.Name = "TotalValueForVATCalcEdit";
			this.TotalValueForVATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalValueForVATCalcEdit.TabIndex = 16;
			this.TotalValueForVATCalcEdit.Text = "0";
			this.TotalValueForVATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalValueForVATCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalVATExemptionValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVATExemptionValueCalcEdit, "CustomsEntryHeaders.TotalVATExemptionValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalVATExemptionValue)));
			this.TotalVATExemptionValueCalcEdit.DecimalPlaces = 0;
			this.TotalVATExemptionValueCalcEdit.Decimals = 0;
			this.TotalVATExemptionValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 81, true);
			this.TotalVATExemptionValueCalcEdit.Name = "TotalVATExemptionValueCalcEdit";
			this.TotalVATExemptionValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalVATExemptionValueCalcEdit.TabIndex = 17;
			this.TotalVATExemptionValueCalcEdit.Text = "0";
			this.TotalVATExemptionValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalVATExemptionValueCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalDutyAmountCalcEdit, "CustomsEntryHeaders.FormattedTotalDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).FormattedTotalDutyAmount)));
			this.TotalDutyAmountCalcEdit.DecimalPlaces = 0;
			this.TotalDutyAmountCalcEdit.Decimals = 0;
			this.TotalDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 107, true);
			this.TotalDutyAmountCalcEdit.Name = "TotalDutyAmountCalcEdit";
			this.TotalDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalDutyAmountCalcEdit.TabIndex = 19;
			this.TotalDutyAmountCalcEdit.Text = "0";
			this.TotalDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalDutyAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalSpecialConsumptionTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalSpecialConsumptionTaxCalcEdit, "CustomsEntryHeaders.TotalSpecialConsumptionTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalSpecialConsumptionTax)));
			this.TotalSpecialConsumptionTaxCalcEdit.DecimalPlaces = 0;
			this.TotalSpecialConsumptionTaxCalcEdit.Decimals = 0;
			this.TotalSpecialConsumptionTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 133, true);
			this.TotalSpecialConsumptionTaxCalcEdit.Name = "TotalSpecialConsumptionTaxCalcEdit";
			this.TotalSpecialConsumptionTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalSpecialConsumptionTaxCalcEdit.TabIndex = 20;
			this.TotalSpecialConsumptionTaxCalcEdit.Text = "0";
			this.TotalSpecialConsumptionTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalSpecialConsumptionTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalTransportationTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalTransportationTaxCalcEdit, "CustomsEntryHeaders.TotalTransportationTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalTransportationTax)));
			this.TotalTransportationTaxCalcEdit.DecimalPlaces = 0;
			this.TotalTransportationTaxCalcEdit.Decimals = 0;
			this.TotalTransportationTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 159, true);
			this.TotalTransportationTaxCalcEdit.Name = "TotalTransportationTaxCalcEdit";
			this.TotalTransportationTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalTransportationTaxCalcEdit.TabIndex = 21;
			this.TotalTransportationTaxCalcEdit.Text = "0";
			this.TotalTransportationTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalTransportationTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalLiquorTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalLiquorTaxCalcEdit, "CustomsEntryHeaders.TotalLiquorTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalLiquorTax)));
			this.TotalLiquorTaxCalcEdit.DecimalPlaces = 0;
			this.TotalLiquorTaxCalcEdit.Decimals = 0;
			this.TotalLiquorTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 185, true);
			this.TotalLiquorTaxCalcEdit.Name = "TotalLiquorTaxCalcEdit";
			this.TotalLiquorTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalLiquorTaxCalcEdit.TabIndex = 22;
			this.TotalLiquorTaxCalcEdit.Text = "0";
			this.TotalLiquorTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalLiquorTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalEducationTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalEducationTaxCalcEdit, "CustomsEntryHeaders.TotalEducationTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalEducationTax)));
			this.TotalEducationTaxCalcEdit.DecimalPlaces = 0;
			this.TotalEducationTaxCalcEdit.Decimals = 0;
			this.TotalEducationTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 211, true);
			this.TotalEducationTaxCalcEdit.Name = "TotalEducationTaxCalcEdit";
			this.TotalEducationTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalEducationTaxCalcEdit.TabIndex = 23;
			this.TotalEducationTaxCalcEdit.Text = "0";
			this.TotalEducationTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalEducationTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalAgricultureTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalAgricultureTaxCalcEdit, "CustomsEntryHeaders.TotalAgricultureTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalAgricultureTax)));
			this.TotalAgricultureTaxCalcEdit.DecimalPlaces = 0;
			this.TotalAgricultureTaxCalcEdit.Decimals = 0;
			this.TotalAgricultureTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(942, 3, true);
			this.TotalAgricultureTaxCalcEdit.Name = "TotalAgricultureTaxCalcEdit";
			this.TotalAgricultureTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalAgricultureTaxCalcEdit.TabIndex = 24;
			this.TotalAgricultureTaxCalcEdit.Text = "0";
			this.TotalAgricultureTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalAgricultureTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalVATCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVATCalcEdit, "CustomsEntryHeaders.TotalVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalVAT)));
			this.TotalVATCalcEdit.DecimalPlaces = 0;
			this.TotalVATCalcEdit.Decimals = 0;
			this.TotalVATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 29, true);
			this.TotalVATCalcEdit.Name = "TotalVATCalcEdit";
			this.TotalVATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalVATCalcEdit.TabIndex = 25;
			this.TotalVATCalcEdit.Text = "0";
			this.TotalVATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalVATCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalPayableAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPayableAmountCalcEdit, "CustomsEntryHeaders.TotalAmountPayable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalAmountPayable)));
			this.TotalPayableAmountCalcEdit.DecimalPlaces = 0;
			this.TotalPayableAmountCalcEdit.Decimals = 0;
			this.TotalPayableAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 55, true);
			this.TotalPayableAmountCalcEdit.Name = "TotalPayableAmountCalcEdit";
			this.TotalPayableAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalPayableAmountCalcEdit.TabIndex = 26;
			this.TotalPayableAmountCalcEdit.Text = "0";
			this.TotalPayableAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPayableAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForLateDeclarationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyForLateDeclarationCalcEdit, "CustomsEntryHeaders.PenaltyForLateDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PenaltyForLateDeclaration)));
			this.PenaltyForLateDeclarationCalcEdit.DecimalPlaces = 0;
			this.PenaltyForLateDeclarationCalcEdit.Decimals = 0;
			this.PenaltyForLateDeclarationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 81, true);
			this.PenaltyForLateDeclarationCalcEdit.Name = "PenaltyForLateDeclarationCalcEdit";
			this.PenaltyForLateDeclarationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.PenaltyForLateDeclarationCalcEdit.TabIndex = 27;
			this.PenaltyForLateDeclarationCalcEdit.Text = "0";
			this.PenaltyForLateDeclarationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyForLateDeclarationCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForMissedDeclarationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyForMissedDeclarationCalcEdit, "CustomsEntryHeaders.PenaltyForMissedDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PenaltyForMissedDeclaration)));
			this.PenaltyForMissedDeclarationCalcEdit.DecimalPlaces = 0;
			this.PenaltyForMissedDeclarationCalcEdit.Decimals = 0;
			this.PenaltyForMissedDeclarationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 107, true);
			this.PenaltyForMissedDeclarationCalcEdit.Name = "PenaltyForMissedDeclarationCalcEdit";
			this.PenaltyForMissedDeclarationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.PenaltyForMissedDeclarationCalcEdit.TabIndex = 28;
			this.PenaltyForMissedDeclarationCalcEdit.Text = "0";
			this.PenaltyForMissedDeclarationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyForMissedDeclarationCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalGrossWeightInKGCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossWeightInKGCalcEdit, "CustomsEntryHeaders.TotalGrossWeightInKG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalGrossWeightInKG)));
			this.TotalGrossWeightInKGCalcEdit.DecimalPlaces = 3;
			this.TotalGrossWeightInKGCalcEdit.Decimals = 3;
			this.TotalGrossWeightInKGCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 133, true);
			this.TotalGrossWeightInKGCalcEdit.Name = "TotalGrossWeightInKGCalcEdit";
			this.TotalGrossWeightInKGCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalGrossWeightInKGCalcEdit.TabIndex = 29;
			this.TotalGrossWeightInKGCalcEdit.Text = "0";
			this.TotalGrossWeightInKGCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalGrossWeightInKGCalcEdit.TrackDisposedAccess = true;
			// 
			// CustomerOfficerTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerOfficerTextBox, "CustomsEntryHeaders.ResponsibleCustomsOfficer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ResponsibleCustomsOfficer)));
			this.CustomerOfficerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 185, true);
			this.CustomerOfficerTextBox.Name = "CustomerOfficerTextBox";
			this.CustomerOfficerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.CustomerOfficerTextBox.TabIndex = 31;
			// 
			// CustomsRemarkLongTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsRemarkLongTextBox, "CustomsEntryHeaders.CH_CustomsMessageRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_CustomsMessageRemarks)));
			this.CustomsRemarkLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 211, true);
			this.CustomsRemarkLongTextBox.Name = "CustomsRemarkLongTextBox";
			this.CustomsRemarkLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.CustomsRemarkLongTextBox.TabIndex = 32;
			// 
			// TotalPackagesDropEdit
			// 
			this.TotalPackagesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalPackagesDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PackageQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
			this.TotalPackagesDropEdit.BindToAmount = "CustomsEntryHeaders.PackageQuantity";
			this.TotalPackagesDropEdit.BindToUnit = "JE_TotalNoOfPacksPackType";
			this.TotalPackagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 159, true);
			this.TotalPackagesDropEdit.Name = "TotalPackagesDropEdit";
			this.TotalPackagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.TotalPackagesDropEdit.TabIndex = 33;
			this.TotalPackagesDropEdit.SetReadOnly(true);
			// 
			// TotalInvoiceAmountUserControl
			// 
			this.TotalInvoiceAmountUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountUserControl, ".");
			this.TotalInvoiceAmountUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 207, true);
			this.TotalInvoiceAmountUserControl.Name = "TotalInvoiceAmountUserControl";
			this.TotalInvoiceAmountUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 24, true);
			this.TotalInvoiceAmountUserControl.TabIndex = 35;
			// 
			// CustomsDisbursementBillGroupBox
			// 
			this.CustomsDisbursementBillGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1b5449ce-f1ea-4410-bc54-80d295216190", "Customs Individual Disbursement Bills");
			this.CustomsDisbursementBillGroupBox.Controls.Add(this.CustomsDisbursementBillGrid);
			this.CustomsDisbursementBillGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 237, true);
			this.CustomsDisbursementBillGroupBox.Name = "CustomsDisbursementBillGroupBox";
			this.CustomsDisbursementBillGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 97, true);
			this.CustomsDisbursementBillGroupBox.TabIndex = 36;
			this.CustomsDisbursementBillGroupBox.TabStop = false;
			// 
			// CustomsDisbursementBillGrid
			// 
			this.CustomsDisbursementBillGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsDisbursementBillGrid, "CustomsEntryHeaders.CustomsDisbursementBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_StatementNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_ProcessDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_PrintDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_StatementAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_StatusName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusStatementHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CustomsDisbursementBills)).SyncRoot)).B2_PaymentAuthorizationDate)));
			this.CustomsDisbursementBillGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3d9c6011-40d5-4f89-ae84-62c15643ba02", "Customs Disbursement Bill #");
			zTextBoxColumnStyleInfo1.ColumnName = "B2_StatementNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a6969300-d86a-4915-b6bf-8066a289aa15", "Process Date");
			zDateEditColumnStyleInfo1.ColumnName = "B2_ProcessDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6b8d3993-aa27-4bbb-92c1-d3cb88cf67c4", "Issue Date");
			zDateEditColumnStyleInfo2.ColumnName = "B2_PrintDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo3.ColumnName = "B2_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ee234d26-e996-4017-83aa-b8de27fdb28e", "Total Due Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "B2_StatementAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("484a83ff-b184-4f72-8c1f-b81951427ff1", "Bill Type");
			zTextBoxColumnStyleInfo2.ColumnName = "B2_StatusName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("70efc335-eef8-4a9a-984c-6bbc9da923b6", "Payment Date");
			zDateEditColumnStyleInfo4.ColumnName = "B2_PaymentAuthorizationDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsDisbursementBillGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.CustomsDisbursementBillGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDisbursementBillGrid.GridId = "33139e7d-b406-4a94-8fe4-1eaff05ec702";
			this.CustomsDisbursementBillGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsDisbursementBillGrid.LayoutKey = "CustomsDisbursementBillGrid";
			this.CustomsDisbursementBillGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsDisbursementBillGrid.Name = "CustomsDisbursementBillGrid";
			this.CustomsDisbursementBillGrid.ShouldSetErrorsOnTabPage = false;
			this.CustomsDisbursementBillGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 78, true);
			this.CustomsDisbursementBillGrid.TabIndex = 0;
			// 
			// ImportEntryDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsDisbursementBillGroupBox);
			this.Controls.Add(this.TotalInvoiceAmountUserControl);
			this.Controls.Add(this.TotalPackagesDropEdit);
			this.Controls.Add(this.CustomsRemarkLongTextBox);
			this.Controls.Add(this.CustomerOfficerTextBox);
			this.Controls.Add(this.TotalGrossWeightInKGCalcEdit);
			this.Controls.Add(this.PenaltyForMissedDeclarationCalcEdit);
			this.Controls.Add(this.PenaltyForLateDeclarationCalcEdit);
			this.Controls.Add(this.TotalPayableAmountCalcEdit);
			this.Controls.Add(this.TotalVATCalcEdit);
			this.Controls.Add(this.TotalAgricultureTaxCalcEdit);
			this.Controls.Add(this.TotalEducationTaxCalcEdit);
			this.Controls.Add(this.TotalLiquorTaxCalcEdit);
			this.Controls.Add(this.TotalTransportationTaxCalcEdit);
			this.Controls.Add(this.TotalSpecialConsumptionTaxCalcEdit);
			this.Controls.Add(this.TotalDutyAmountCalcEdit);
			this.Controls.Add(this.TotalVATExemptionValueCalcEdit);
			this.Controls.Add(this.TotalValueForVATCalcEdit);
			this.Controls.Add(this.DeductedAmountCalcEdit);
			this.Controls.Add(this.AdditionalAmountCalcEdit);
			this.Controls.Add(this.InsuranceCalcEdit);
			this.Controls.Add(this.FreightCalcEdit);
			this.Controls.Add(this.TotalCustomsValueUSDCalcEdit);
			this.Controls.Add(this.TotalCustomsValueKRWCalcEdit);
			this.Controls.Add(this.IncotermTextBox);
			this.Controls.Add(this.ClearedDateEdit);
			this.Controls.Add(this.AcceptedDateEdit);
			this.Controls.Add(this.EntrySubmittedDateEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Name = "ImportEntryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1171, 367, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.EntrySubmittedDateEdit.ResumeLayout(true);
			this.EntrySubmittedDateEdit.PerformLayout();
			this.AcceptedDateEdit.ResumeLayout(true);
			this.AcceptedDateEdit.PerformLayout();
			this.ClearedDateEdit.ResumeLayout(true);
			this.ClearedDateEdit.PerformLayout();
			this.CustomsRemarkLongTextBox.ResumeLayout(true);
			this.CustomsRemarkLongTextBox.PerformLayout();
			this.TotalPackagesDropEdit.ResumeLayout(true);
			this.TotalPackagesDropEdit.PerformLayout();
			this.TotalInvoiceAmountUserControl.ResumeLayout(true);
			this.TotalInvoiceAmountUserControl.PerformLayout();
			this.CustomsDisbursementBillGroupBox.ResumeLayout(false);
			this.CustomsDisbursementBillGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsDisbursementBillGrid)).EndInit();
			this.CustomsDisbursementBillGrid.ResumeLayout(false);
			this.CustomsDisbursementBillGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox EntryNumberTextBox;
		public ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		public ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		public ZArchitecture.GUI.ZDateEdit EntrySubmittedDateEdit;
		public ZArchitecture.GUI.ZDateEdit AcceptedDateEdit;
		public ZArchitecture.GUI.ZDateEdit ClearedDateEdit;
		public ZArchitecture.ZTextBox IncotermTextBox;
		public ZArchitecture.ZCalcEdit TotalCustomsValueKRWCalcEdit;
		public ZArchitecture.ZCalcEdit TotalCustomsValueUSDCalcEdit;
		public ZArchitecture.ZCalcEdit FreightCalcEdit;
		public ZArchitecture.ZCalcEdit InsuranceCalcEdit;
		public ZArchitecture.ZCalcEdit AdditionalAmountCalcEdit;
		public ZArchitecture.ZCalcEdit DeductedAmountCalcEdit;
		public ZArchitecture.ZCalcEdit TotalValueForVATCalcEdit;
		public ZArchitecture.ZCalcEdit TotalVATExemptionValueCalcEdit;
		public ZArchitecture.ZCalcEdit TotalDutyAmountCalcEdit;
		public ZArchitecture.ZCalcEdit TotalSpecialConsumptionTaxCalcEdit;
		public ZArchitecture.ZCalcEdit TotalTransportationTaxCalcEdit;
		public ZArchitecture.ZCalcEdit TotalLiquorTaxCalcEdit;
		public ZArchitecture.ZCalcEdit TotalEducationTaxCalcEdit;
		public ZArchitecture.ZCalcEdit TotalAgricultureTaxCalcEdit;
		public ZArchitecture.ZCalcEdit TotalVATCalcEdit;
		public ZArchitecture.ZCalcEdit TotalPayableAmountCalcEdit;
		public ZArchitecture.ZCalcEdit PenaltyForLateDeclarationCalcEdit;
		public ZArchitecture.ZCalcEdit PenaltyForMissedDeclarationCalcEdit;
		public ZArchitecture.ZCalcEdit TotalGrossWeightInKGCalcEdit;
		public ZArchitecture.ZTextBox CustomerOfficerTextBox;
		public Customs.GUI.LongTextControl CustomsRemarkLongTextBox;
		public ZArchitecture.GUI.ZCalcDropEdit TotalPackagesDropEdit;
		public TotalInvoiceAmountUserControl TotalInvoiceAmountUserControl;
		public ZArchitecture.GUI.ZGroupBox CustomsDisbursementBillGroupBox;
		private ZArchitecture.ZGrid CustomsDisbursementBillGrid;
	}
}
