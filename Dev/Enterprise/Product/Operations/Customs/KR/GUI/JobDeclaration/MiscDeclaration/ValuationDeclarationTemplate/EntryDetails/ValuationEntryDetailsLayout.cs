using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ValuationEntryDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = ValuationEntryDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionWidth = (int)ColumnLayoutBuilderCaptionWidthSize.Medium + 20;

			var captionRuler = layout.CreateRuler(captionWidth);
			var mediumWidthRuler = layout.CreateRightRuler(captionWidth + 126);
			var columnWidthRuler = layout.CreateRuler(captionWidth + 134);

			layout.Include(0, captionRuler, controlBag.EntryNumberTextBox, mediumWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, controlBag.MessageStatusDropEdit, mediumWidthRuler, columnWidthRuler);
			layout.Include(0, captionRuler, controlBag.EntryStatusDropEdit, mediumWidthRuler, columnWidthRuler);
			layout.AddColumn();
			layout.Include(1, captionRuler, controlBag.EntrySubmittedDateDateEdit, columnWidthRuler);
			layout.Include(1, captionRuler, controlBag.AcceptedDateDateEdit, columnWidthRuler);
			layout.Include(1, captionRuler, controlBag.ApprovalDateDateEdit, columnWidthRuler);
			layout.AddColumn();
			layout.Include(2, captionRuler, controlBag.EffectiveToDateDateEdit, columnWidthRuler);
			layout.Include(2, captionRuler, controlBag.ApprovalNumberTextBox, mediumWidthRuler, columnWidthRuler);
			layout.Include(2, captionRuler, controlBag.ResultReasonTextBox, mediumWidthRuler, columnWidthRuler);

			return layout;
		}
	}
}
