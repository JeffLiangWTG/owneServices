using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class TermsAcknowledgementChecker
	{
		readonly BaseTermsAgreement term;
		readonly Form parentForm;
		public TermsAcknowledgementChecker(BaseTermsAgreement term, Form parentForm)
		{
			this.term = term;
			this.parentForm = parentForm;
		}

		public async Task<bool> CheckTermAcknowledged()
		{
			var shouldShowAcknowledgementForm = false;
			using (var progressForm = new TermsProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = false;
				progressForm.UpdateStatus(Res.GetString("922DA791-7EFA-4E62-8052-5EBC3FF9A5BE", "Fetching terms and conditions..."), 0);

				if (parentForm != null)
				{
					progressForm.ShowModalTo(parentForm);
				}
				else
				{
					progressForm.Show();
				}

				shouldShowAcknowledgementForm = await term.IsDisplayConditionSatisfied();

				progressForm.Hide();
				Task.WaitAll(Task.Delay(50));
			}

			if (shouldShowAcknowledgementForm)
			{
				if (term.IsCurrentUserAllowedToAcknowledgeAgreement)
				{
					var termForm = new TermsAcknowledgementForm(term);
					return ZFormModaliser.ShowDialogAndDispose(termForm, parentForm) == DialogResult.OK;
				}
				else
				{
					Globals.Message.ShowError(term.ErrorMessageForAcknowledgementNotAllowed);
				}
			}

			return term.HasBeenAcknowledged;
		}
	}
}
