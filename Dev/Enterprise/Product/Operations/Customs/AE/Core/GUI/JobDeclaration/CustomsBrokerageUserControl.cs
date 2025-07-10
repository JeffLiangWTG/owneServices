using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new AEJobDeclarationUserControl();
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new SupplierHeaderUserControl();
		}

		protected override BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl()
		{
			return new GroupInvoiceUserControl();
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new MiscOptionsUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new MessageUserControl();
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionUserControl();

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => new InvoiceLineUserControl();
	}
}
