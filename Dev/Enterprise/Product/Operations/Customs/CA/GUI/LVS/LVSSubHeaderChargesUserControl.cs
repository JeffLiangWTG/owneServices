using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSSubHeaderChargesUserControl : ZUserControl
	{
		public LVSSubHeaderChargesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			var invoice = (JobComInvoiceHeader)CurrentDataItem;
			if (invoice != null)
			{
				this.ChargesSplitContainer.Panel2Collapsed = invoice.IsAttachedToPersistentLVXDeclaration;
			}
		}
	}
}
