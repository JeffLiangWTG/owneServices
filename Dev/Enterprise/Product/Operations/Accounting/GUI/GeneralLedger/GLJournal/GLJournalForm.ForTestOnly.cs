#if DEBUG

using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalForm
	{
		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}

		public ContinueWithDelete ShowPreDeleteDialogs_ForTestOnly()
		{
			return ShowPreDeleteDialogs();
		}

		public GLJournalUserControl GlJournalUserControl_ForTestOnly
		{
			get { return glJournalUserControl; }
			set { glJournalUserControl = value; }
		}

		public void HandleApplyPostingButtonClickUnsafe_ForTestOnly(bool closeOnSave)
		{
			HandleApplyPostingButtonClickUnsafe(closeOnSave);
		}

		public void OnPostButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnPostButtonClick(sender, e);
		}
	}
}

#endif
