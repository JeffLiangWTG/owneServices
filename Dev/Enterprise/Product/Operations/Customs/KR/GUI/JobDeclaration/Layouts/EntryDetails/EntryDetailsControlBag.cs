using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EntryDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ImportEntryDetailsUserControl();

		[ThreadStatic]
		static EntryDetailsControlBag instance;

		public static EntryDetailsControlBag Instance => instance ?? (instance = new EntryDetailsControlBag());

		public EntryDetailsControlBag()
		{
			EntryNumberTextBox = RegisterControl(nameof(ImportEntryDetailsUserControl.EntryNumberTextBox));
			MessageTypeDropEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.MessageTypeDropEdit));
			MessageStatusDropEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.MessageStatusDropEdit));
			EntryStatusDropEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.EntryStatusDropEdit));
			EntrySubmittedDateEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.EntrySubmittedDateEdit));
			AcceptedDateEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.AcceptedDateEdit));
			ClearedDateEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.ClearedDateEdit));
			IncotermTextBox = RegisterControl(nameof(ImportEntryDetailsUserControl.IncotermTextBox));
			TotalInvoiceAmountUserControl = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalInvoiceAmountUserControl));
			TotalCustomsValueKRWCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalCustomsValueKRWCalcEdit));
			TotalCustomsValueUSDCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalCustomsValueUSDCalcEdit));
			FreightCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.FreightCalcEdit));
			InsuranceCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.InsuranceCalcEdit));
			AdditionalAmountCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.AdditionalAmountCalcEdit));
			DeductedAmountCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.DeductedAmountCalcEdit));
			TotalValueForVATCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalValueForVATCalcEdit));
			TotalVATExemptionValueCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalVATExemptionValueCalcEdit));
			CustomsDisbursementBillGroupBox = RegisterControl(nameof(ImportEntryDetailsUserControl.CustomsDisbursementBillGroupBox));
			TotalDutyAmountCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalDutyAmountCalcEdit));
			TotalSpecialConsumptionTaxCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalSpecialConsumptionTaxCalcEdit));
			TotalTransportationTaxCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalTransportationTaxCalcEdit));
			TotalLiquorTaxCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalLiquorTaxCalcEdit));
			TotalEducationTaxCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalEducationTaxCalcEdit));
			TotalAgricultureTaxCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalAgricultureTaxCalcEdit));
			TotalVATCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalVATCalcEdit));
			TotalPayableAmountCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalPayableAmountCalcEdit));
			PenaltyForLateDeclarationCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.PenaltyForLateDeclarationCalcEdit));
			PenaltyForMissedDeclarationCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.PenaltyForMissedDeclarationCalcEdit));
			TotalGrossWeightInKGCalcEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalGrossWeightInKGCalcEdit));
			TotalPackagesDropEdit = RegisterControl(nameof(ImportEntryDetailsUserControl.TotalPackagesDropEdit));
			CustomerOfficerTextBox = RegisterControl(nameof(ImportEntryDetailsUserControl.CustomerOfficerTextBox));
			CustomsRemarkLongTextBox = RegisterControl(nameof(ImportEntryDetailsUserControl.CustomsRemarkLongTextBox));
		}

		public ControlReference EntryNumberTextBox { get; }
		public ControlReference MessageTypeDropEdit { get; }
		public ControlReference MessageStatusDropEdit { get; }
		public ControlReference EntryStatusDropEdit { get; }
		public ControlReference EntrySubmittedDateEdit { get; }
		public ControlReference AcceptedDateEdit { get; }
		public ControlReference ClearedDateEdit { get; }
		public ControlReference IncotermTextBox { get; }
		public ControlReference TotalInvoiceAmountUserControl { get; }
		public ControlReference TotalCustomsValueKRWCalcEdit { get; }
		public ControlReference TotalCustomsValueUSDCalcEdit { get; }
		public ControlReference FreightCalcEdit { get; }
		public ControlReference InsuranceCalcEdit { get; }
		public ControlReference AdditionalAmountCalcEdit { get; }
		public ControlReference DeductedAmountCalcEdit { get; }
		public ControlReference TotalValueForVATCalcEdit { get; }
		public ControlReference TotalVATExemptionValueCalcEdit { get; }
		public ControlReference TotalDutyAmountCalcEdit { get; }
		public ControlReference TotalSpecialConsumptionTaxCalcEdit { get; }
		public ControlReference TotalTransportationTaxCalcEdit { get; }
		public ControlReference TotalLiquorTaxCalcEdit { get; }
		public ControlReference TotalEducationTaxCalcEdit { get; }
		public ControlReference TotalAgricultureTaxCalcEdit { get; }
		public ControlReference TotalVATCalcEdit { get; }
		public ControlReference TotalPayableAmountCalcEdit { get; }
		public ControlReference PenaltyForLateDeclarationCalcEdit { get; }
		public ControlReference PenaltyForMissedDeclarationCalcEdit { get; }
		public ControlReference TotalGrossWeightInKGCalcEdit { get; }
		public ControlReference TotalPackagesDropEdit { get; }
		public ControlReference CustomerOfficerTextBox { get; }
		public ControlReference CustomsRemarkLongTextBox { get; }
		public ControlReference CustomsDisbursementBillGroupBox { get; }
	}
}
