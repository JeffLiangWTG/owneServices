using System.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using JournalBase = Enterprise.Accounting.Business.ARAP.Journal.Journal;

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	public class BalancingJournalForm : JournalBaseForm, IDoDisplayModeEditOverride
	{
		public BalancingJournalForm(JournalBase journal) : base(journal)
		{
			InitializeComponent();
		}

		void IDoDisplayModeEditOverride.DoDisplayModeEdit()
		{
			if (!BusinessEntity.IsInDatabaseIncludingChildren)
			{
				fPostButton.Visible = false;
				fPostButton.Enabled = false;
				fApplyButton.Visible = false;
				fApplyButton.Enabled = false;
				fCancelButton.Text = AccountingConstants.CloseButtonText;
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
		}
	}
}
