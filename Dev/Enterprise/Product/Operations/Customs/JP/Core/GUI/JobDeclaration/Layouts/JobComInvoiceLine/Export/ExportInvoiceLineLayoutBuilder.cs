using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class ExportInvoiceLineLayoutBuilder : CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>
	{
		public CommonInvoiceLineDetailsControlBag CommonInvoiceLineDetailsControlBag => CommonInvoiceLineDetailsControlBag.Instance;

		public ExportInvoiceLineControlBag JPExportBag => ExportInvoiceLineControlBag.Instance;

		public InvoiceLineControlBag JPCommonBag => InvoiceLineControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(JPCommonBag.EntryInstructionGuidDropEdit, x => x.Declaration?.AreMultipleEntryInstructionsAllowed ?? false);
		}
	}
}
