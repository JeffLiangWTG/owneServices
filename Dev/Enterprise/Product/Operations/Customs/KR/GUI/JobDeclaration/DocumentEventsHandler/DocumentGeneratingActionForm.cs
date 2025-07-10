using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DocumentGeneratingActionForm : ZChildForm
	{
		public DocumentGeneratingActionForm(DocumentGeneratingActionCollection actions)
			: base(actions)
		{
			this.actions = actions;
			InitializeComponent();
			RemoveColumnStyles();
		}
		readonly DocumentGeneratingActionCollection actions;

		public override string FormVerb => string.Empty;

		void RemoveColumnStyles()
		{
			if (!actions.IsStatusRelevant)
			{
				EntriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.StatusDescription)).IsUnavailable = true;
			}
			if (!actions.IsStatementRelevant)
			{
				EntriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.PaymentInvoiceNumber)).IsUnavailable = true;
				EntriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.ReceivedDate)).IsUnavailable = true;
			}
			if (!actions.IsAmendmentRelevant)
			{
				EntriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.IncludingCurrentDifference)).IsUnavailable = true;
			}
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			if (actions.CountSelected == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("98DB8E30-367B-46B3-B07F-2CE80E5A2C54", "Please select at lease one entry to be delivered."));
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
