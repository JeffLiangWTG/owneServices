using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportAmendmentEntryDetailsLayoutItemsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ImportAmendmentEntryDetailsLayoutItemsUserControl();
		[ThreadStatic]
		static ImportAmendmentEntryDetailsLayoutItemsControlBag instance;

		public static ImportAmendmentEntryDetailsLayoutItemsControlBag Instance => instance ?? (instance = new ImportAmendmentEntryDetailsLayoutItemsControlBag());

		public ImportAmendmentEntryDetailsLayoutItemsControlBag()
		{
			VersionNoCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.VersionNoCalcEdit));
			AmendmentTypeTextBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.AmendmentTypeTextBox));
			AmendmentTypeDescriptionTextBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.AmendmentTypeDescriptionTextBox));
			ReasonCodeDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.ReasonCodeDropEdit));
			AmendmentReasonTextBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.AmendmentReasonTextBox));
			FaultPartyDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.FaultPartyDropEdit));
			FaultReasonTextBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.FaultReasonTextBox));
			PenaltyPaymentReasonCodeFindBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyPaymentReasonCodeFindBox));
			TotalAmendedCountItemCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.TotalAmendedCountItemCalcEdit));
			TotalAmendedCountDutyTaxCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.TotalAmendedCountDutyTaxCalcEdit));
			BeforeTotalDutyTaxCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.BeforeTotalDutyTaxCalcEdit));
			AfterTotalDutyTaxCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.AfterTotalDutyTaxCalcEdit));
			DutyTaxDifferenceCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.DutyTaxDifferenceCalcEdit));
			BeforeCustomsValueCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.BeforeCustomsValueCalcEdit));
			AfterCustomsValueCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.AfterCustomsValueCalcEdit));
			CustomsValueDifferenceCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.CustomsValueDifferenceCalcEdit));

			DTYPenaltyTypeDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.DTYPenaltyTypeDropEdit));
			DTYPenaltyReducedYNDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.DTYPenaltyReducedYNDropEdit));
			DomesticTaxPenaltyTypeDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.DomesticTaxPenaltyTypeDropEdit));
			PenaltyExemptReqDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyExemptReqDropEdit));
			PenaltyExemptReasonCodeDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyExemptReasonCodeDropEdit));
			PenaltyExemptReasonMultiLineTextBox = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyExemptReasonMultiLineTextBox));
			PenaltyExemptSequenceCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyExemptSequenceCalcEdit));
			PenaltyExemptAmountCalcEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.PenaltyExemptAmountCalcEdit));
			RefundRequestYNDropEdit = RegisterControl(nameof(ImportAmendmentEntryDetailsLayoutItemsUserControl.RefundRequestYNDropEdit));
		}

		public ControlReference VersionNoCalcEdit { get; }
		public ControlReference AmendmentTypeTextBox { get; }
		public ControlReference AmendmentTypeDescriptionTextBox { get; }
		public ControlReference ReasonCodeDropEdit { get; }
		public ControlReference AmendmentReasonTextBox { get; }
		public ControlReference FaultPartyDropEdit { get; }
		public ControlReference FaultReasonTextBox { get; }
		public ControlReference PenaltyPaymentReasonCodeFindBox { get; }
		public ControlReference TotalAmendedCountItemCalcEdit { get; }
		public ControlReference TotalAmendedCountDutyTaxCalcEdit { get; }
		public ControlReference BeforeTotalDutyTaxCalcEdit { get; }
		public ControlReference AfterTotalDutyTaxCalcEdit { get; }
		public ControlReference DutyTaxDifferenceCalcEdit { get; }
		public ControlReference BeforeCustomsValueCalcEdit { get; }
		public ControlReference AfterCustomsValueCalcEdit { get; }
		public ControlReference CustomsValueDifferenceCalcEdit { get; }

		public ControlReference DTYPenaltyTypeDropEdit { get; }
		public ControlReference DTYPenaltyReducedYNDropEdit { get; }
		public ControlReference DomesticTaxPenaltyTypeDropEdit { get; }
		public ControlReference PenaltyExemptReqDropEdit { get; }
		public ControlReference PenaltyExemptReasonCodeDropEdit { get; }
		public ControlReference PenaltyExemptReasonMultiLineTextBox { get; }
		public ControlReference PenaltyExemptSequenceCalcEdit { get; }
		public ControlReference PenaltyExemptAmountCalcEdit { get; }
		public ControlReference RefundRequestYNDropEdit { get; }
	}
}
