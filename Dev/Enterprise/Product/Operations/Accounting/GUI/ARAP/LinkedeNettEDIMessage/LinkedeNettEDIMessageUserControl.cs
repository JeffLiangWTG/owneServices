using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessageUserControl : ZUserControl
	{
		ZTabControl TabControl;
		ZTabPage MessageSummaryTabPage;
		ZArchitecture.ZTextBox OrganisationNameTextBox;
		ZArchitecture.ZTextBox OrganisationCodeTextBox;
		ZArchitecture.ZTextBox CurrencyTextBox;
		ZArchitecture.ZTextBox OSInvoiceAmtInclTaxTextBox;
		ZArchitecture.ZTextBox LocalInvoiceAmtInclTaxTextBox;
		ZArchitecture.ZTextBox PostDateTextBox;
		ZArchitecture.ZTextBox InvoiceDateTextBox;
		ZArchitecture.ZTextBox ChequeOrReferenceTextBox;
		ZArchitecture.ZTextBox JobInvoiceNoTextBox;
		ZArchitecture.ZTextBox TransactionNumberTextBox;
		ZArchitecture.ZTextBox TransactionTypeTextBox;
		ZArchitecture.ZTextBox LedgerTextBox;
		ZTabPage MessageTextTabPage;
		ZArchitecture.ZTextBox MessageTextTextBox;

		public LinkedeNettEDIMessageUserControl()
		{
			InitializeComponent();
		}
	}
}

