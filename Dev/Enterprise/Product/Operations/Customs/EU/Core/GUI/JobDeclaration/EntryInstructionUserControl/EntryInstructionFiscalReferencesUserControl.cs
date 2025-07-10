using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionFiscalReferencesUserControl : ZUserControl
	{
		public EntryInstructionFiscalReferencesUserControl()
		{
			InitializeComponent();
			new ControlRebinder().Rebind(FiscalReferencesUserControl, "FilteredInvoiceLines", "CustomsEntryInstructions");
		}
	}
}
