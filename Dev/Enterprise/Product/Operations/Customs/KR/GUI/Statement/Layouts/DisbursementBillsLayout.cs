using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DisbursementBillsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var krBag = StatementHeaderControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(160);

			layout.Include(0, captionRuler, krBag.FormattedNumberTextBox);
			layout.Include(0, captionRuler, krBag.FormattedEntryNumberTextBox);
			layout.Include(0, captionRuler, krBag.BillTypeDropEdit);
			layout.Include(0, captionRuler, krBag.CustomsAccountIDTextBox);
			layout.Include(0, captionRuler, krBag.ProcessPortCodeFindBox);
			layout.Include(0, captionRuler, krBag.ImporterGuidFindBox);
			layout.AddColumn();
			layout.Include(1, captionRuler, krBag.StatusDropEdit);
			layout.Include(1, captionRuler, krBag.DueDateEdit);
			layout.Include(1, captionRuler, krBag.IssueDateEdit);
			layout.Include(1, captionRuler, krBag.ProcessDateEdit);
			layout.Include(1, captionRuler, krBag.PaymentDateEdit);
			layout.Include(1, captionRuler, krBag.StatementAmountCalcEdit);
			layout.Include(1, captionRuler, krBag.TotalAmountAfterDueDateCalcEdit);
			return layout;
		}
	}
}
