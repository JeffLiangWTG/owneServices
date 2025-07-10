using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DutyReductionDetailsUserControl : ZUserControl
	{
		public DutyReductionDetailsUserControl()
		{
			InitializeComponent();

			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForDeclaration);
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.JobDeclarationMiscMessageSendingObjectParent);
			Controls.ChangeBindingPaths(BindingSource, BindingPathForMessageSending);

			GroupNumberDropEdit.ReadOnly = true;
			SeqNumberTextBox.ReadOnly = true;
			ItemNumberTextBox.ReadOnly = true;
		}

		const string BindingPathForMessageSending = "SendingObjectsCollection.MessageSendingEntryLines.InvoiceLine.";
		const string BindingPathForDeclaration = "FilteredInvoiceLines.";
	}
}
