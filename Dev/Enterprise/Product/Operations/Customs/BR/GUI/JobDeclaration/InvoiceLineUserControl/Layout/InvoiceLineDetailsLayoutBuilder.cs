using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class InvoiceLineDetailsLayoutBuilder : CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>
	{
		public override CommonInvoiceLineDetailsControlBag CommonBag { get; } = CommonInvoiceLineDetailsControlBag.Instance;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, l => l.Declaration?.IsPersistent ?? false);
		}
	}
}
