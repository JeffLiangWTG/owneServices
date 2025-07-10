using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class ImportInvoiceLineLayoutBuilder : CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>
	{
		public CommonInvoiceLineDetailsControlBag CommonInvoiceLineDetailsControlBag => CommonInvoiceLineDetailsControlBag.Instance;

		public ImportInvoiceLineControlBag JPImportBag => ImportInvoiceLineControlBag.Instance;

		public InvoiceLineControlBag JPCommonBag => InvoiceLineControlBag.Instance;

		protected override int MaxColumns => 2;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(JPCommonBag.EntryInstructionGuidDropEdit, x => x.Declaration?.AreMultipleEntryInstructionsAllowed ?? false);
			SetVisibility(JPImportBag.StorageTypeDropEdit, x => x.JI_StorageTypeVisible);
		}
	}
}
