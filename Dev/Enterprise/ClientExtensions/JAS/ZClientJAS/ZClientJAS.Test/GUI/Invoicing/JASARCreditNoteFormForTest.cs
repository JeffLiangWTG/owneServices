using System.Windows.Forms;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.GUI.Testing
{
	internal class JASARCreditNoteFormForTest : JASARCreditNoteForm
	{
		public JASARCreditNoteFormForTest(JASARCreditNote businessEntity) : base(businessEntity)
		{
		}

		public new MenuItem ActionsMenuItem
		{
			get
			{
				return base.ActionsMenuItem;
			}
		}
	}
}
