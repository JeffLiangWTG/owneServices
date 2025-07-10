using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;

namespace Enterprise.Customs.MX.GUI
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
