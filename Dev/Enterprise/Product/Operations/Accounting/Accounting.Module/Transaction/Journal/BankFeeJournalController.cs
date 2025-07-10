using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public abstract class BankFeeJournalController : MiscellaneousTransactionController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new BankFeeJournalForm((Journal)businessEntity);
		}

		protected override bool ShouldHaveReversedBizo
		{
			get { return false; }
		}
	}
}
