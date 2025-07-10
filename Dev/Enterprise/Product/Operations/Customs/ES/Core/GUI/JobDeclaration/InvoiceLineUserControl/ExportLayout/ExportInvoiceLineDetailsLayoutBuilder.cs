using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class ExportInvoiceLineDetailsLayoutBuilder : CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>
	{
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultVisibilities() { }
	}
}
