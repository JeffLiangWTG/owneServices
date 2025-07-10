using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class InvoiceLineDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, InvoiceLineDetailsControlBag> where T : Business.EMCSJobComInvoiceLine
	{
		public override InvoiceLineDetailsControlBag CommonBag => InvoiceLineDetailsControlBag.Instance;

		public override bool NarrowColumnForMediumControls => true;

		protected override int MaxColumns => 3;
	}
}
