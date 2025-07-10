using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class EntryDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public EntryDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var krBag = EntryDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(krBag);

			var captionWidth = (int)ColumnLayoutBuilderCaptionWidthSize.Long + 20;
			var lastCaptionWidth = (captionWidth + 110) * 2;

			var captionRuler = layout.CreateRuler(captionWidth);
			var lastColumnCaptionRuler = layout.CreateRuler(lastCaptionWidth);
			var mediumWidthRuler = layout.CreateRightRuler(captionWidth + 116);
			var mediumWidthRuler2 = layout.CreateRightRuler(lastCaptionWidth + 116);
			var longWidthRuler = layout.CreateRightRuler(lastCaptionWidth + 293);
			var groupBoxWidthRuler = layout.CreateRightRuler(850);
			var columnWidthRuler = layout.CreateRuler(captionWidth + 200 + 14);

			layout.Include(0, captionRuler, krBag.EntryNumberTextBox, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.MessageTypeDropEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.MessageStatusDropEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.EntryStatusDropEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.EntrySubmittedDateEdit, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.AcceptedDateEdit, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.ClearedDateEdit, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.IncotermTextBox, columnWidthRuler);
			layout.Include(0, captionRuler, krBag.TotalInvoiceAmountUserControl, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.TotalCustomsValueKRWCalcEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.TotalCustomsValueUSDCalcEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.FreightCalcEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, krBag.InsuranceCalcEdit, mediumWidthRuler);
			layout.AddColumn();
			layout.Include(1, captionRuler, krBag.AdditionalAmountCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.TotalAgricultureTaxCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.DeductedAmountCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.TotalVATCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalValueForVATCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.TotalPayableAmountCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalVATExemptionValueCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.PenaltyForLateDeclarationCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalDutyAmountCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.PenaltyForMissedDeclarationCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalSpecialConsumptionTaxCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.TotalGrossWeightInKGCalcEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalTransportationTaxCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.TotalPackagesDropEdit, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalLiquorTaxCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.CustomerOfficerTextBox, mediumWidthRuler2);
			layout.Include(1, captionRuler, krBag.TotalEducationTaxCalcEdit, mediumWidthRuler, lastColumnCaptionRuler, krBag.CustomsRemarkLongTextBox, longWidthRuler);
			layout.Include(1, krBag.CustomsDisbursementBillGroupBox, groupBoxWidthRuler);
			return layout;
		}
	}
}
