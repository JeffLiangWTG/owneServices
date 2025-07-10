namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSLineChargesUserControl : InvoiceLineChargesUserControl
	{
		public LVSLineChargesUserControl()
		{
			InitializeComponent();
		}

		internal bool IsSimplifiedLVSMode
		{
			get { return SplitContainer.Panel2Collapsed; }
			set { SplitContainer.Panel2Collapsed = value; }
		}
	}
}
