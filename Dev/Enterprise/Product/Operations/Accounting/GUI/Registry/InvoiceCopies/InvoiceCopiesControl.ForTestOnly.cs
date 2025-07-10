#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceCopiesControl
	{
		public ZArchitecture.ZGrid InvoiceCopiesGrid_ForTestOnly
		{
			get { return InvoiceCopiesGrid; }
			set { InvoiceCopiesGrid = value; }
		}
	}
}

#endif
