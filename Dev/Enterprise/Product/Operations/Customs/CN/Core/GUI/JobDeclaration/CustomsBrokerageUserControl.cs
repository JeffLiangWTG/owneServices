using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new CNJobDeclarationUserControl();
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new CustomsSupplierHeaderUserControl();
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			return new InvoiceLineUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CustomsEntriesWithMessagesUserControl();
		}

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new CustomsCusContainersUserControl();
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new EntryInstructionUserControl();
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;
	}
}
