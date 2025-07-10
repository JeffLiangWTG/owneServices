#if DEBUG

namespace Enterprise.Accounting.GUI.Matching
{
	public partial class PayLinesForm
	{
		public ZArchitecture.ZGrid InvoiceLinesGrid_ForTestOnly
		{
			get { return InvoiceLinesGrid; }
			set { InvoiceLinesGrid = value; }
		}
	}
}

#endif
