using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JASARCreditNoteForm : CreditNoteForm, IJXCExportForm
	{
		public JASARCreditNoteForm(JASARCreditNote businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			new JASInvoicingFormHelper(this);
		}

		#region IJXCExportForm Members

		void IJXCExportForm.ValidateAll()
		{
			ValidateAll(ValidationType.Light);
		}

		BusinessObject IJXCExportForm.BusinessEntity
		{
			get { return BusinessEntity as BusinessObject; }
		}

		#endregion
	}
}
